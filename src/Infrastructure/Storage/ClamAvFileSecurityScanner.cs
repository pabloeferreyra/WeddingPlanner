using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeddingPlanner.Application.Abstractions.Services;

namespace WeddingPlanner.Infrastructure.Storage;

public sealed class ClamAvFileSecurityScanner(
    IOptions<ClamAvOptions> options,
    ILogger<ClamAvFileSecurityScanner> logger) : IFileSecurityScanner
{
    public async Task<FileScanResult> ScanAsync(Stream content, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!settings.IsValid())
        {
            logger.LogError("La configuración obligatoria del antivirus no es válida.");
            return new(FileScanStatus.Error, Error: FileScanError.Unavailable);
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(settings.TimeoutSeconds));
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(settings.Host, settings.Port, timeout.Token);
            await using var network = client.GetStream();
            await network.WriteAsync("zINSTREAM\0"u8.ToArray(), timeout.Token);
            var buffer = new byte[81920];
            var length = new byte[4];
            long total = 0;
            int count;
            while ((count = await content.ReadAsync(buffer, timeout.Token)) > 0)
            {
                total += count;
                if (total > settings.MaxScanSizeBytes)
                {
                    logger.LogWarning("El archivo excede el límite de análisis antivirus.");
                    return new(FileScanStatus.Error, Error: FileScanError.SizeLimit);
                }
                BinaryPrimitives.WriteUInt32BigEndian(length, (uint)count);
                await network.WriteAsync(length, timeout.Token);
                await network.WriteAsync(buffer.AsMemory(0, count), timeout.Token);
            }
            Array.Clear(length);
            await network.WriteAsync(length, timeout.Token);

            var reply = new byte[2048];
            var used = 0;
            while (used < reply.Length)
            {
                count = await network.ReadAsync(reply.AsMemory(used), timeout.Token);
                if (count == 0) break;
                used += count;
            }
            if (used == 0 || used == reply.Length || reply[used - 1] != 0 ||
                Array.IndexOf(reply, (byte)0, 0, used - 1) >= 0)
            {
                logger.LogWarning("Respuesta antivirus incompleta o inválida.");
                return new(FileScanStatus.Error, Error: FileScanError.InvalidResponse);
            }

            var response = Encoding.ASCII.GetString(reply, 0, used - 1);
            if (response == "stream: OK") return new(FileScanStatus.Clean);
            const string prefix = "stream: ";
            const string suffix = " FOUND";
            if (response.StartsWith(prefix, StringComparison.Ordinal) &&
                response.EndsWith(suffix, StringComparison.Ordinal) &&
                response.Length > prefix.Length + suffix.Length)
                return new(FileScanStatus.Infected, response[prefix.Length..^suffix.Length]);

            logger.LogWarning("El antivirus devolvió un error o una respuesta inesperada.");
            return new(FileScanStatus.Error, Error: FileScanError.InvalidResponse);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Se agotó el tiempo del análisis antivirus.");
            return new(FileScanStatus.Error, Error: FileScanError.Timeout);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "No se pudo completar el análisis antivirus.");
            return new(FileScanStatus.Error, Error: FileScanError.Unavailable);
        }
    }
}

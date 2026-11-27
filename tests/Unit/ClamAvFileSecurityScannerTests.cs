using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Infrastructure;
using WeddingPlanner.Infrastructure.Storage;

namespace WeddingPlanner.Tests.Unit;

public class ClamAvFileSecurityScannerTests
{
    [Theory]
    [InlineData("Antivirus:Enabled", "false")]
    [InlineData("Antivirus:Required", "false")]
    [InlineData("Antivirus:Host", "")]
    [InlineData("Antivirus:Port", "0")]
    [InlineData("Antivirus:TimeoutSeconds", "0")]
    [InlineData("Antivirus:MaxScanSizeBytes", "0")]
    public async Task Invalid_options_prevent_host_startup(string key, string value)
    {
        using var host = new HostBuilder()
            .ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=validation",
                [key] = value
            }))
            .ConfigureServices((context, services) => services.AddInfrastructure(context.Configuration))
            .Build();
        await Assert.ThrowsAsync<OptionsValidationException>(() => host.StartAsync());
    }

    [Theory]
    [InlineData("stream: OK\0", FileScanStatus.Clean)]
    [InlineData("stream: Test-Signature FOUND\0", FileScanStatus.Infected)]
    [InlineData("stream: unavailable ERROR\0", FileScanStatus.Error)]
    [InlineData("OK\0", FileScanStatus.Error)]
    [InlineData("stream: OK", FileScanStatus.Error)]
    [InlineData("stream: OK\0stream: failure ERROR\0", FileScanStatus.Error)]
    public async Task Protocol_replies_are_interpreted_fail_closed(string reply, FileScanStatus status)
    {
        using var listener = StartListener();
        var input = Encoding.ASCII.GetBytes("test content");
        var server = RespondAsync(listener, reply);
        var result = await Scanner(listener).ScanAsync(new MemoryStream(input), default);
        Assert.Equal(status, result.Status);
        Assert.Equal(input, await server);
        if (status == FileScanStatus.Infected) Assert.Equal("Test-Signature", result.ThreatName);
    }

    [Fact]
    public async Task Large_input_is_streamed_in_multiple_chunks()
    {
        using var listener = StartListener();
        var input = new byte[200_000];
        Random.Shared.NextBytes(input);
        var server = RespondAsync(listener, "stream: OK\0");
        var result = await Scanner(listener).ScanAsync(new MemoryStream(input), default);
        Assert.Equal(FileScanStatus.Clean, result.Status);
        Assert.Equal(input, await server);
    }

    [Fact]
    public async Task Missing_service_and_invalid_configuration_fail_closed()
    {
        using var listener = StartListener();
        var options = OptionsFor(listener);
        listener.Stop();
        var result = await Scanner(options).ScanAsync(new MemoryStream([1]), default);
        Assert.Equal(FileScanStatus.Error, result.Status);
        Assert.Equal(FileScanError.Unavailable, result.Error);
        options.Enabled = false;
        result = await Scanner(options).ScanAsync(new MemoryStream([1]), default);
        Assert.Equal(FileScanStatus.Error, result.Status);
        options.Enabled = true;
        options.Required = false;
        Assert.Equal(FileScanStatus.Error, (await Scanner(options).ScanAsync(new MemoryStream([1]), default)).Status);
    }

    [Fact]
    public async Task Excessive_input_is_rejected_without_scanning_it_as_clean()
    {
        using var listener = StartListener();
        var options = OptionsFor(listener);
        options.MaxScanSizeBytes = 2;
        var server = listener.AcceptTcpClientAsync();
        var result = await Scanner(options).ScanAsync(new MemoryStream([1, 2, 3]), default);
        using var client = await server;
        Assert.Equal(FileScanStatus.Error, result.Status);
        Assert.Equal(FileScanError.SizeLimit, result.Error);
    }

    [Fact]
    public async Task Timeout_is_distinct_from_caller_cancellation()
    {
        using var listener = StartListener();
        var received = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var server = WaitAsync(listener, received, release);
        var options = OptionsFor(listener);
        options.TimeoutSeconds = 1;
        var result = await Scanner(options).ScanAsync(new MemoryStream([1]), default);
        Assert.Equal(FileScanStatus.Error, result.Status);
        Assert.Equal(FileScanError.Timeout, result.Error);
        release.SetResult();
        await server;

        using var second = StartListener();
        received = new(TaskCreationOptions.RunContinuationsAsynchronously);
        release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        server = WaitAsync(second, received, release);
        using var cancellation = new CancellationTokenSource();
        var scan = Scanner(second).ScanAsync(new MemoryStream([1]), cancellation.Token);
        await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => scan);
        release.SetResult();
        await server;
    }

    private static TcpListener StartListener()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return listener;
    }

    private static ClamAvOptions OptionsFor(TcpListener listener) => new()
    {
        Host = "127.0.0.1",
        Port = ((IPEndPoint)listener.LocalEndpoint).Port
    };

    private static ClamAvFileSecurityScanner Scanner(TcpListener listener) => Scanner(OptionsFor(listener));
    private static ClamAvFileSecurityScanner Scanner(ClamAvOptions options) =>
        new(Options.Create(options), NullLogger<ClamAvFileSecurityScanner>.Instance);

    private static async Task<byte[]> RespondAsync(TcpListener listener, string reply)
    {
        using var client = await listener.AcceptTcpClientAsync();
        await using var network = client.GetStream();
        var input = await ReadInputAsync(network);
        await network.WriteAsync(Encoding.ASCII.GetBytes(reply));
        return input;
    }

    private static async Task WaitAsync(TcpListener listener, TaskCompletionSource received, TaskCompletionSource release)
    {
        using var client = await listener.AcceptTcpClientAsync();
        await using var network = client.GetStream();
        await ReadInputAsync(network);
        received.SetResult();
        await release.Task.WaitAsync(TimeSpan.FromSeconds(10));
    }

    private static async Task<byte[]> ReadInputAsync(NetworkStream network)
    {
        var command = new byte[10];
        await network.ReadExactlyAsync(command);
        Assert.Equal("zINSTREAM\0", Encoding.ASCII.GetString(command));
        var length = new byte[4];
        using var output = new MemoryStream();
        while (true)
        {
            await network.ReadExactlyAsync(length);
            var count = BinaryPrimitives.ReadUInt32BigEndian(length);
            if (count == 0) break;
            Assert.InRange(count, 1u, 81920u);
            var chunk = new byte[count];
            await network.ReadExactlyAsync(chunk);
            await output.WriteAsync(chunk);
        }
        return output.ToArray();
    }
}

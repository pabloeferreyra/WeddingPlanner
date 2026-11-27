using System.Buffers.Binary;
using System.Text;

namespace WeddingPlanner.Application.Implementations;

internal static class DocumentFormatValidator
{
    public static async Task<bool> IsValidAsync(Stream stream, string extension, CancellationToken cancellationToken)
    {
        stream.Position = 0;
        return extension switch
        {
            ".pdf" => await IsPdfAsync(stream, cancellationToken),
            ".png" => await IsPngAsync(stream, cancellationToken),
            ".jpg" or ".jpeg" => await IsJpegAsync(stream, cancellationToken),
            _ => false
        };
    }

    private static async Task<bool> IsPdfAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (stream.Length < 30) return false;
        var header = new byte[8];
        if (!await ReadExactlyAsync(stream, header, cancellationToken) ||
            !(header.AsSpan(0, 7).SequenceEqual("%PDF-1."u8) && header[7] is >= (byte)'0' and <= (byte)'7') &&
            !header.AsSpan().SequenceEqual("%PDF-2.0"u8)) return false;

        var tail = new byte[(int)Math.Min(stream.Length, 2048)];
        stream.Position = stream.Length - tail.Length;
        if (!await ReadExactlyAsync(stream, tail, cancellationToken)) return false;
        var text = Encoding.Latin1.GetString(tail).TrimEnd('\0', '\t', '\n', '\f', '\r', ' ');
        if (!text.EndsWith("%%EOF", StringComparison.Ordinal)) return false;
        var start = text.LastIndexOf("startxref", StringComparison.Ordinal);
        if (start < 0) return false;
        var offsetText = text[(start + "startxref".Length)..^5].Trim();
        return long.TryParse(offsetText, System.Globalization.NumberStyles.None,
            System.Globalization.CultureInfo.InvariantCulture, out var offset) && offset >= 0 && offset < stream.Length;
    }

    private static async Task<bool> IsPngAsync(Stream stream, CancellationToken cancellationToken)
    {
        var signature = new byte[8];
        if (!await ReadExactlyAsync(stream, signature, cancellationToken) ||
            !signature.AsSpan().SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return false;
        var header = new byte[8];
        var ihdr = new byte[13];
        var first = true;
        var hasData = false;
        while (stream.Position <= stream.Length - 12)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await ReadExactlyAsync(stream, header, cancellationToken)) return false;
            var length = BinaryPrimitives.ReadUInt32BigEndian(header);
            if (length > stream.Length - stream.Position - 4) return false;
            var type = header.AsSpan(4, 4);
            if (first)
            {
                if (!type.SequenceEqual("IHDR"u8) || length != 13 ||
                    !await ReadExactlyAsync(stream, ihdr, cancellationToken)) return false;
                if (BinaryPrimitives.ReadUInt32BigEndian(ihdr) == 0 ||
                    BinaryPrimitives.ReadUInt32BigEndian(ihdr.AsSpan(4)) == 0 ||
                    ihdr[10] != 0 || ihdr[11] != 0 || ihdr[12] > 1) return false;
                first = false;
            }
            else if (type.SequenceEqual("IHDR"u8)) return false;
            else if (type.SequenceEqual("IDAT"u8))
            {
                hasData |= length > 0;
                stream.Position += length;
            }
            else if (type.SequenceEqual("IEND"u8))
            {
                return length == 0 && hasData && stream.Position + 4 == stream.Length;
            }
            else stream.Position += length;
            stream.Position += 4;
        }
        return false;
    }

    private static async Task<bool> IsJpegAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (stream.Length < 20) return false;
        var pair = new byte[2];
        if (!await ReadExactlyAsync(stream, pair, cancellationToken) || pair[0] != 0xff || pair[1] != 0xd8) return false;
        stream.Position = stream.Length - 2;
        if (!await ReadExactlyAsync(stream, pair, cancellationToken) || pair[0] != 0xff || pair[1] != 0xd9) return false;
        stream.Position = 2;
        var hasFrame = false;
        var frame = new byte[5];
        while (stream.Position < stream.Length - 2)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await ReadExactlyAsync(stream, pair, cancellationToken) || pair[0] != 0xff) return false;
            var marker = pair[1];
            while (marker == 0xff)
            {
                if (!await ReadExactlyAsync(stream, pair.AsMemory(0, 1), cancellationToken)) return false;
                marker = pair[0];
            }
            if (marker is 0 or 0xd8 or 0xd9 or >= 0xd0 and <= 0xd7) return false;
            if (!await ReadExactlyAsync(stream, pair, cancellationToken)) return false;
            var length = BinaryPrimitives.ReadUInt16BigEndian(pair);
            if (length < 2 || length - 2 > stream.Length - stream.Position - 2) return false;
            if (marker is >= 0xc0 and <= 0xcf and not (0xc4 or 0xc8 or 0xcc))
            {
                if (length < 8 || !await ReadExactlyAsync(stream, frame, cancellationToken)) return false;
                if (BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(1)) == 0 ||
                    BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(3)) == 0) return false;
                hasFrame = true;
                stream.Position += length - 7;
            }
            else if (marker == 0xda)
            {
                return hasFrame && length >= 6 && stream.Position + length - 2 < stream.Length - 2;
            }
            else stream.Position += length - 2;
        }
        return false;
    }

    private static async Task<bool> ReadExactlyAsync(Stream stream, Memory<byte> buffer, CancellationToken cancellationToken) =>
        await stream.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false, cancellationToken) == buffer.Length;
}

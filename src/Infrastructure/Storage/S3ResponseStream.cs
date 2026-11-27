using Amazon.S3.Model;

namespace WeddingPlanner.Infrastructure.Storage;

internal sealed class S3ResponseStream(GetObjectResponse response) : Stream
{
    private readonly Stream inner = response.ResponseStream ?? throw new InvalidOperationException("La respuesta S3 no contiene un stream.");
    private bool disposed;

    public override bool CanRead => !disposed && inner.CanRead;
    public override bool CanSeek => !disposed && inner.CanSeek;
    public override bool CanWrite => false;
    public override long Length { get { ThrowIfDisposed(); return inner.Length; } }
    public override long Position
    {
        get { ThrowIfDisposed(); return inner.Position; }
        set { ThrowIfDisposed(); inner.Position = value; }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ThrowIfDisposed();
        try { return inner.Read(buffer, offset, count); }
        catch (Exception error) { DisposeAfterError(error); throw; }
    }

    public override int Read(Span<byte> buffer)
    {
        ThrowIfDisposed();
        try { return inner.Read(buffer); }
        catch (Exception error) { DisposeAfterError(error); throw; }
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        try { return await inner.ReadAsync(buffer, offset, count, cancellationToken); }
        catch (Exception error) { await DisposeAfterErrorAsync(error); throw; }
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        try { return await inner.ReadAsync(buffer, cancellationToken); }
        catch (Exception error) { await DisposeAfterErrorAsync(error); throw; }
    }

    public override long Seek(long offset, SeekOrigin origin) { ThrowIfDisposed(); return inner.Seek(offset, origin); }
    public override void Flush() { ThrowIfDisposed(); }
    public override void SetLength(long value) { ThrowIfDisposed(); throw new NotSupportedException(); }
    public override void Write(byte[] buffer, int offset, int count) { ThrowIfDisposed(); throw new NotSupportedException(); }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !disposed)
        {
            disposed = true;
            response.Dispose();
        }
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (!disposed)
        {
            disposed = true;
            response.ResponseStream = null;
            try { await inner.DisposeAsync(); }
            finally { response.Dispose(); }
        }
        GC.SuppressFinalize(this);
    }

    private void DisposeAfterError(Exception error)
    {
        try { Dispose(); }
        catch (Exception disposalError) { throw new AggregateException(error, disposalError); }
    }

    private async ValueTask DisposeAfterErrorAsync(Exception error)
    {
        try { await DisposeAsync(); }
        catch (Exception disposalError) { throw new AggregateException(error, disposalError); }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(disposed, this);
}

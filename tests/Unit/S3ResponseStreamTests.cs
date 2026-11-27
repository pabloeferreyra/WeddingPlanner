using Amazon.S3.Model;
using WeddingPlanner.Infrastructure.Storage;

namespace WeddingPlanner.Tests.Unit;

public class S3ResponseStreamTests
{
    [Fact]
    public void Synchronous_disposal_releases_response_and_stream()
    {
        var inner = new TrackingStream();
        var response = new GetObjectResponse { ResponseStream = inner };
        var stream = new S3ResponseStream(response);
        Assert.Equal(1, stream.ReadByte());
        stream.Dispose();
        stream.Dispose();
        Assert.Equal(1, inner.DisposeCount);
        Assert.Null(response.ResponseStream);
        Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
    }

    [Fact]
    public async Task Asynchronous_disposal_releases_response_and_stream_once()
    {
        var inner = new TrackingStream();
        var response = new GetObjectResponse { ResponseStream = inner };
        var stream = new S3ResponseStream(response);
        Assert.Equal(1, await stream.ReadAsync(new byte[1]));
        await stream.DisposeAsync();
        await stream.DisposeAsync();
        Assert.Equal(1, inner.DisposeCount);
        Assert.Null(response.ResponseStream);
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => _ = await stream.ReadAsync(new byte[1]));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Failed_read_releases_response_and_stream(bool asynchronous)
    {
        var inner = new TrackingStream { FailRead = true };
        var response = new GetObjectResponse { ResponseStream = inner };
        var stream = new S3ResponseStream(response);
        if (asynchronous)
            await Assert.ThrowsAsync<IOException>(async () => _ = await stream.ReadAsync(new byte[1]));
        else
            Assert.Throws<IOException>(() => stream.Read(new byte[1], 0, 1));
        Assert.Null(response.ResponseStream);
        Assert.Equal(1, inner.DisposeCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Failed_disposal_does_not_hide_the_read_error(bool asynchronous)
    {
        var inner = new TrackingStream { FailRead = true, FailDispose = true };
        var response = new GetObjectResponse { ResponseStream = inner };
        var stream = new S3ResponseStream(response);
        var error = asynchronous
            ? await Assert.ThrowsAsync<AggregateException>(async () => _ = await stream.ReadAsync(new byte[1]))
            : Assert.Throws<AggregateException>(() => stream.Read(new byte[1], 0, 1));
        Assert.IsType<IOException>(error.InnerExceptions[0]);
        Assert.IsType<InvalidOperationException>(error.InnerExceptions[1]);
        Assert.Equal(1, inner.DisposeCount);
    }

    private sealed class TrackingStream : MemoryStream
    {
        public int DisposeCount { get; private set; }
        public bool FailRead { get; set; }
        public bool FailDispose { get; set; }
        public TrackingStream() : base([1]) { }
        public override int Read(byte[] buffer, int offset, int count) =>
            FailRead ? throw new IOException("read failed") : base.Read(buffer, offset, count);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            FailRead ? ValueTask.FromException<int>(new IOException("read failed")) : base.ReadAsync(buffer, cancellationToken);
        protected override void Dispose(bool disposing)
        {
            if (disposing) DisposeCount++;
            base.Dispose(disposing);
            if (disposing && FailDispose) throw new InvalidOperationException("dispose failed");
        }
    }
}

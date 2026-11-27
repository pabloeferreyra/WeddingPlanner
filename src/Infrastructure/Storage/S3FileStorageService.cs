using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Options;
using WeddingPlanner.Application.Abstractions.Services;

namespace WeddingPlanner.Infrastructure.Storage;

public sealed class S3FileStorageService(IOptions<FileStorageOptions> options) : IFileStorageService, IDisposable
{
    private IAmazonS3? client;

    private IAmazonS3 Client
    {
        get
        {
            if (client is not null) return client;
            var settings = options.Value;
            if (!Uri.TryCreate(settings.Endpoint, UriKind.Absolute, out var endpoint) ||
                endpoint.Scheme != (settings.UseSSL ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) ||
                string.IsNullOrWhiteSpace(settings.AccessKey) || string.IsNullOrWhiteSpace(settings.SecretKey) ||
                string.IsNullOrWhiteSpace(settings.Bucket) || string.IsNullOrWhiteSpace(settings.Region))
                throw new InvalidOperationException("La configuración FileStorage es incompleta o el esquema del endpoint no coincide con UseSSL.");
            client = new AmazonS3Client(new BasicAWSCredentials(settings.AccessKey, settings.SecretKey),
                new AmazonS3Config
                {
                    ServiceURL = endpoint.AbsoluteUri,
                    AuthenticationRegion = settings.Region,
                    ForcePathStyle = settings.ForcePathStyle
                });
            return client;
        }
    }

    public async Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        await Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = options.Value.Bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false
        }, cancellationToken);
    }

    public async Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken)
    {
        var response = await Client.GetObjectAsync(options.Value.Bucket, key, cancellationToken);
        return response.ResponseStream;
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        await Client.DeleteObjectAsync(options.Value.Bucket, key, cancellationToken);
    }

    public void Dispose() => client?.Dispose();
}

namespace WeddingPlanner.Application.Abstractions.Services;

public interface IFileStorageService
{
    Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken);
    Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken);
    Task DeleteAsync(string key, CancellationToken cancellationToken);
}

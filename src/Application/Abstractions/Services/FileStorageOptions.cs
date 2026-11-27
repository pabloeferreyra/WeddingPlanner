namespace WeddingPlanner.Application.Abstractions.Services;

public sealed class FileStorageOptions
{
    public const long MaximumFileSizeBytes = 10 * 1024 * 1024;
    public const long MaximumRequestSizeBytes = 11 * 1024 * 1024;
    public string Endpoint { get; set; } = "";
    public string Bucket { get; set; } = "";
    public string Region { get; set; } = "us-east-1";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public bool UseSSL { get; set; } = true;
    public bool ForcePathStyle { get; set; } = true;
    public long MaxFileSizeBytes { get; set; } = MaximumFileSizeBytes;
    public string[] AllowedExtensions { get; set; } = [".pdf", ".jpg", ".jpeg", ".png"];
}

namespace WeddingPlanner.Application.Abstractions.Services;

public sealed class FileStorageOptions
{
    public string Endpoint { get; set; } = "";
    public string Bucket { get; set; } = "";
    public string Region { get; set; } = "us-east-1";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public bool UseSSL { get; set; } = true;
    public bool ForcePathStyle { get; set; } = true;
    public long MaxFileSizeBytes { get; set; } = 10485760;
    public string[] AllowedExtensions { get; set; } = [".pdf", ".jpg", ".jpeg", ".png"];
}

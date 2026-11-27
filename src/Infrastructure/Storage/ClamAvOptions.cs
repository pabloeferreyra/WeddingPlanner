using WeddingPlanner.Application.Abstractions.Services;

namespace WeddingPlanner.Infrastructure.Storage;

public sealed class ClamAvOptions
{
    public bool Enabled { get; set; } = true;
    public bool Required { get; set; } = true;
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 3310;
    public int TimeoutSeconds { get; set; } = 30;
    public long MaxScanSizeBytes { get; set; } = FileStorageOptions.MaximumFileSizeBytes;

    public bool IsValid() => Enabled && Required && !string.IsNullOrWhiteSpace(Host) &&
        Port is >= 1 and <= 65535 && TimeoutSeconds is >= 1 and <= 300 &&
        MaxScanSizeBytes is > 0 and <= FileStorageOptions.MaximumFileSizeBytes;
}

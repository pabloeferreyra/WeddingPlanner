namespace WeddingPlanner.Application.Abstractions.Services;

public enum FileScanStatus { Error, Clean, Infected }
public enum FileScanError { None, Unavailable, Timeout, InvalidResponse, SizeLimit }

public sealed record FileScanResult(FileScanStatus Status, string? ThreatName = null, FileScanError Error = FileScanError.None);

public interface IFileSecurityScanner
{
    Task<FileScanResult> ScanAsync(Stream content, CancellationToken cancellationToken);
}

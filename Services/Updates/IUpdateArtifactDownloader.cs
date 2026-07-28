using HandballManagerCore.DTO;

namespace HandWStat.Services.Updates;

public interface IUpdateArtifactDownloader
{
    Task<DownloadedArtifactResult> DownloadAsync(
        ReleaseArtifactDto artifact,
        IProgress<double>? progress,
        CancellationToken cancellationToken);
}

public sealed record DownloadedArtifactResult(bool Success, string? FilePath, string? ErrorMessage)
{
    public static DownloadedArtifactResult Verified(string filePath) => new(true, filePath, null);

    public static DownloadedArtifactResult Failed(string message) => new(false, null, message);
}

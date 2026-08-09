using System.Net.Http.Headers;
using System.Security.Cryptography;
using HandWStat.Models.Contracts;

namespace HandWStat.Services.Updates;

public sealed class UpdateArtifactDownloader(HttpClient httpClient, string cacheDirectory)
    : IUpdateArtifactDownloader
{
    public async Task<DownloadedArtifactResult> DownloadAsync(
        ReleaseArtifactDto artifact,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(cacheDirectory);
        var finalName = $"{artifact.BuildNumber}-{Path.GetFileName(artifact.FileName)}";
        var finalPath = Path.Combine(cacheDirectory, finalName);
        var temporaryPath = Path.Combine(cacheDirectory, $".{finalName}.{Guid.NewGuid():N}.tmp");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, artifact.DownloadUrl);
            using var response = await httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            if (response.Content.Headers.ContentLength is { } contentLength
                && contentLength != artifact.FileSizeBytes)
            {
                return DownloadedArtifactResult.Failed("La taille annoncee par le serveur est invalide.");
            }

            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var destination = new FileStream(
                temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            var buffer = new byte[81920];
            long totalRead = 0;
            progress?.Report(0);

            while (true)
            {
                var read = await source.ReadAsync(buffer, cancellationToken);
                if (read == 0) break;
                totalRead += read;
                if (totalRead > artifact.FileSizeBytes)
                {
                    return DownloadedArtifactResult.Failed("Le fichier telecharge depasse la taille attendue.");
                }
                await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                progress?.Report(totalRead * 100d / artifact.FileSizeBytes);
            }

            await destination.FlushAsync(cancellationToken);
            if (totalRead != artifact.FileSizeBytes)
            {
                return DownloadedArtifactResult.Failed("La taille du fichier telecharge est invalide.");
            }

            destination.Close();
            var actualHash = await ComputeHashAsync(temporaryPath, cancellationToken);
            var expectedHash = Convert.FromHexString(artifact.Sha256);
            if (!CryptographicOperations.FixedTimeEquals(actualHash, expectedHash))
            {
                return DownloadedArtifactResult.Failed("Le SHA-256 du fichier telecharge est invalide.");
            }

            if (File.Exists(finalPath))
            {
                File.Delete(finalPath);
            }
            File.Move(temporaryPath, finalPath);
            progress?.Report(100);
            return DownloadedArtifactResult.Verified(finalPath);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return DownloadedArtifactResult.Failed("Le telechargement a ete annule.");
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return DownloadedArtifactResult.Failed("Le telechargement securise a echoue.");
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static async Task<byte[]> ComputeHashAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return await SHA256.HashDataAsync(stream, cancellationToken);
    }
}

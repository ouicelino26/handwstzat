namespace HandWStat.Services;

public interface IAppAssetReader
{
    // Returns null when the asset does not exist.
    // Propagates OperationCanceledException; returns null for all other failures.
    Task<Stream?> TryOpenAsync(string relativePath);
}

namespace HandWStat.Services;

public sealed class MauiAppAssetReader : IAppAssetReader
{
    public async Task<Stream?> TryOpenAsync(string relativePath)
    {
        try
        {
            return await FileSystem.OpenAppPackageFileAsync(relativePath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }
}

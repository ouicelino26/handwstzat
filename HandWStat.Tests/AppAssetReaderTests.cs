using HandWStat.Web.TestHost.WebHostServices;

namespace HandWStat.Tests;

// E1.5 — IAppAssetReader contract tests using WebAppAssetReader with a real temp directory.
public sealed class AppAssetReaderTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "handwstat_asset_tests_" + Guid.NewGuid().ToString("N")[..8]);

    public AppAssetReaderTests() => Directory.CreateDirectory(_root);

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { }
    }

    [Fact]
    public async Task TryOpenAsync_NonExistentFile_ReturnsNull()
    {
        var reader = new WebAppAssetReader(_root);
        var result = await reader.TryOpenAsync("images/flags/nonexistent.png");
        Assert.Null(result);
    }

    [Fact]
    public async Task TryOpenAsync_ExistingFile_ReturnsReadableStream()
    {
        var subDir = Path.Combine(_root, "images");
        Directory.CreateDirectory(subDir);
        var file = Path.Combine(subDir, "test.png");
        await File.WriteAllBytesAsync(file, new byte[] { 0x89, 0x50, 0x4E, 0x47 });

        var reader = new WebAppAssetReader(_root);
        using var stream = await reader.TryOpenAsync("images/test.png");

        Assert.NotNull(stream);
        var buf = new byte[4];
        await stream!.ReadExactlyAsync(buf);
        Assert.Equal(0x89, buf[0]);
    }

    [Fact]
    public async Task TryOpenAsync_WwwrootPrefixPath_ResolvesCorrectly()
    {
        var subDir = Path.Combine(_root, "wwwroot", "images");
        Directory.CreateDirectory(subDir);
        var file = Path.Combine(subDir, "flag.png");
        await File.WriteAllBytesAsync(file, new byte[] { 1, 2, 3 });

        var reader = new WebAppAssetReader(_root);
        using var stream = await reader.TryOpenAsync("wwwroot/images/flag.png");

        Assert.NotNull(stream);
    }
}

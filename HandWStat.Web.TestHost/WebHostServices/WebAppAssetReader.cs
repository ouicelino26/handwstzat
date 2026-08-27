using HandWStat.Services;
using Microsoft.AspNetCore.Hosting;

namespace HandWStat.Web.TestHost.WebHostServices;

public sealed class WebAppAssetReader : IAppAssetReader
{
    private readonly string _webRootPath;

    public WebAppAssetReader(IWebHostEnvironment env) : this(env.WebRootPath) { }

    public WebAppAssetReader(string webRootPath) => _webRootPath = webRootPath;

    public Task<Stream?> TryOpenAsync(string relativePath)
    {
        var path = Path.Combine(_webRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar));
        if (!File.Exists(path))
            return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(File.OpenRead(path));
    }
}

using HandWStat.Services.Updates;

namespace HandWStat.Web.TestHost.WebHostServices;

public sealed class WebExternalLauncher : IExternalLauncher
{
    public Task<bool> OpenAsync(Uri uri) => Task.FromResult(false);
}

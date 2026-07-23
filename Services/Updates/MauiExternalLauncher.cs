using Microsoft.Maui.ApplicationModel;

namespace HandWStat.Services.Updates;

public sealed class MauiExternalLauncher : IExternalLauncher
{
    public Task<bool> OpenAsync(Uri uri) => Launcher.Default.OpenAsync(uri);
}

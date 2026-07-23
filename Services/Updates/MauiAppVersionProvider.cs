using System.Runtime.InteropServices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace HandWStat.Services.Updates;

public sealed class MauiAppVersionProvider : IAppVersionProvider
{
    private readonly AppVersionProvider _provider = new(
            () => AppInfo.Current.VersionString,
            () => AppInfo.Current.BuildString,
            () => DeviceInfo.Current.Platform.ToString(),
            () => RuntimeInformation.ProcessArchitecture);

    public Models.Updates.AppVersionInfo Current => _provider.Current;
}

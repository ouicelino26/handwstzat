using System.Runtime.InteropServices;
using HandWStat.Models.Updates;

namespace HandWStat.Services.Updates;

public sealed class AppVersionProvider(
    Func<string> version,
    Func<string> build,
    Func<string> platform,
    Func<Architecture> architecture) : IAppVersionProvider
{
    public AppVersionInfo Current
    {
        get
        {
            var parsedBuild = int.TryParse(build(), out var value) && value > 0 ? value : 1;
            return new AppVersionInfo(
                version(),
                parsedBuild,
                MapPlatform(platform()),
                MapArchitecture(architecture()));
        }
    }

    public static string MapPlatform(string platformName)
    {
        return platformName.Trim().ToUpperInvariant() switch
        {
            "WINUI" or "WINDOWS" => "WINDOWS",
            "ANDROID" => "ANDROID",
            "IOS" => "IOS",
            "MACCATALYST" or "MACOS" => "MACCATALYST",
            _ => throw new PlatformNotSupportedException($"Unsupported update platform: {platformName}.")
        };
    }

    public static string MapArchitecture(Architecture architecture) => architecture switch
    {
        Architecture.X64 => "X64",
        Architecture.X86 => "X86",
        Architecture.Arm64 => "ARM64",
        Architecture.Arm => "ARM",
        _ => "ANY"
    };
}


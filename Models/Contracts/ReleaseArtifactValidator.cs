using System.Text.RegularExpressions;

namespace HandWStat.Models.Contracts;

public sealed record ReleaseArtifactValidationInput(
    string Version,
    int BuildNumber,
    string Platform,
    string Architecture,
    string PackageType,
    string DownloadUrl,
    string FileName,
    long FileSizeBytes,
    string Sha256);

public static partial class ReleaseArtifactValidator
{
    private static readonly IReadOnlyDictionary<string, HashSet<string>> PlatformPackages =
        new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["WINDOWS"] = new(StringComparer.OrdinalIgnoreCase) { "MSIX", "MSIXBUNDLE", "EXE", "ZIP", "APPINSTALLER" },
            ["ANDROID"] = new(StringComparer.OrdinalIgnoreCase) { "APK" },
            ["IOS"]     = new(StringComparer.OrdinalIgnoreCase) { "IPA" },
            ["MACCATALYST"] = new(StringComparer.OrdinalIgnoreCase) { "PKG", "DMG", "ZIP" },
        };

    public static bool TryValidate(ReleaseArtifactValidationInput input, out string? error)
    {
        error = null;

        if (!Sha256Regex().IsMatch(input.Sha256 ?? string.Empty))
        {
            error = "sha256 must be exactly 64 hexadecimal characters.";
            return false;
        }

        if (input.FileSizeBytes <= 0)
        {
            error = "fileSizeBytes must be positive.";
            return false;
        }

        if (!TryValidateUrl(input.DownloadUrl, out error))
            return false;

        if (!TryValidatePlatformPackage(input.Platform, input.PackageType, out error))
            return false;

        return true;
    }

    private static bool TryValidatePlatformPackage(string platform, string packageType, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(platform))
        {
            error = "platform is required.";
            return false;
        }

        if (!PlatformPackages.TryGetValue(platform, out var allowed))
        {
            error = $"platform '{platform}' is not supported.";
            return false;
        }

        if (!allowed.Contains(packageType ?? string.Empty))
        {
            error = $"packageType '{packageType}' is not compatible with platform '{platform}'.";
            return false;
        }

        return true;
    }

    private static bool TryValidateUrl(string url, out string? error)
    {
        error = null;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            error = "downloadUrl must be an absolute HTTPS URL.";
            return false;
        }

        if (uri.IsLoopback || IsPrivateOrLocalHost(uri.Host))
        {
            error = "downloadUrl host is not a public host.";
            return false;
        }

        return true;
    }

    private static bool IsPrivateOrLocalHost(string host)
    {
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            return true;

        if (System.Net.IPAddress.TryParse(host, out var ip))
        {
            var bytes = ip.GetAddressBytes();
            if (bytes.Length == 4)
            {
                return bytes[0] == 127
                    || bytes[0] == 10
                    || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                    || (bytes[0] == 192 && bytes[1] == 168);
            }
            return ip.Equals(System.Net.IPAddress.IPv6Loopback);
        }

        return false;
    }

    [GeneratedRegex("^[0-9A-Fa-f]{64}$", RegexOptions.CultureInvariant)]
    private static partial Regex Sha256Regex();
}

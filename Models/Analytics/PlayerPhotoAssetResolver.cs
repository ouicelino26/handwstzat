namespace HandWStat.Models.Analytics;

public static class PlayerPhotoAssetResolver
{
    public static string? Resolve(
        string? photo,
        int? playerId = null,
        string? fullName = null,
        string? teamName = null)
    {
        var resolvedPhoto = ResolveRawPhoto(photo);
        if (!string.IsNullOrWhiteSpace(resolvedPhoto))
        {
            return resolvedPhoto;
        }

        if (playerId is > 0 && PlayerPhotoCatalog.TryGetPath(playerId.Value, out var catalogPath))
        {
            return ResolveRawPhoto(catalogPath);
        }

        return PlayerPhotoCatalog.TryGetPath(fullName, teamName, out var identityPath)
            ? ResolveRawPhoto(identityPath)
            : null;
    }

    private static string? ResolveRawPhoto(string? photo)
    {
        if (string.IsNullOrWhiteSpace(photo))
        {
            return null;
        }

        var cleanValue = photo.Trim().Replace('\\', '/');
        if (cleanValue.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || cleanValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return cleanValue;
        }

        if (cleanValue.StartsWith("/", StringComparison.Ordinal))
        {
            return EscapeAssetPath(cleanValue);
        }

        if (cleanValue.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
        {
            return EscapeAssetPath("/" + cleanValue);
        }

        var fileName = cleanValue.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? cleanValue;
        return $"/images/player-photos/lbe/{Uri.EscapeDataString(fileName)}";
    }

    private static string EscapeAssetPath(string path)
    {
        var prefix = path.StartsWith("/", StringComparison.Ordinal) ? "/" : string.Empty;
        var segments = path.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        return prefix + string.Join("/", segments.Select(Uri.EscapeDataString));
    }
}

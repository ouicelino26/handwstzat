using System.Globalization;

namespace HandWStat.Components.Shared;

internal static class ChartColorUtilities
{
    public static string WithAlpha(string color, double alpha)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return $"rgba(0, 0, 0, {ClampAlpha(alpha).ToString("0.###", CultureInfo.InvariantCulture)})";
        }

        color = color.Trim();

        if (color.StartsWith('#'))
        {
            return HexToRgba(color, alpha);
        }

        if (color.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
        {
            return ReplaceRgbAlpha(color, alpha);
        }

        if (color.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase))
        {
            return ReplaceRgbAlpha(color, alpha);
        }

        return color;
    }

    private static string HexToRgba(string hex, double alpha)
    {
        var value = hex.TrimStart('#');
        byte r;
        byte g;
        byte b;

        if (value.Length == 3)
        {
            r = Convert.ToByte(new string(value[0], 2), 16);
            g = Convert.ToByte(new string(value[1], 2), 16);
            b = Convert.ToByte(new string(value[2], 2), 16);
        }
        else if (value.Length >= 6)
        {
            r = Convert.ToByte(value[..2], 16);
            g = Convert.ToByte(value.Substring(2, 2), 16);
            b = Convert.ToByte(value.Substring(4, 2), 16);
        }
        else
        {
            return $"rgba(0, 0, 0, {ClampAlpha(alpha).ToString("0.###", CultureInfo.InvariantCulture)})";
        }

        return $"rgba({r}, {g}, {b}, {ClampAlpha(alpha).ToString("0.###", CultureInfo.InvariantCulture)})";
    }

    private static string ReplaceRgbAlpha(string color, double alpha)
    {
        var start = color.IndexOf('(');
        var end = color.LastIndexOf(')');
        if (start < 0 || end <= start)
        {
            return color;
        }

        var parts = color[(start + 1)..end]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length < 3)
        {
            return color;
        }

        return $"rgba({parts[0]}, {parts[1]}, {parts[2]}, {ClampAlpha(alpha).ToString("0.###", CultureInfo.InvariantCulture)})";
    }

    private static double ClampAlpha(double alpha) => Math.Clamp(alpha, 0d, 1d);
}

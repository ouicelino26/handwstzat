using System.Globalization;
using System.Text;

namespace HandWStat.Components.Shared;

internal static class SvgExportHelpers
{
    public static string EscapeXml(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    public static string Slugify(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join("-", value
            .Trim()
            .ToLowerInvariant()
            .Split(new[] { ' ', '/', '\\', '.', ',', ';', ':', '_' }, StringSplitOptions.RemoveEmptyEntries));
    }

    public static string FormatNumber(double value, string format = "0.##")
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    public static IReadOnlyList<string> WrapText(string? text, int maxCharsPerLine, int maxLines = 3)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var words = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var lines = new List<string>();
        var current = new StringBuilder();

        foreach (var word in words)
        {
            if (current.Length == 0)
            {
                AppendWord(current, word, maxCharsPerLine, lines, maxLines);
                if (lines.Count >= maxLines)
                {
                    return lines;
                }

                continue;
            }

            if (current.Length + 1 + word.Length <= maxCharsPerLine)
            {
                current.Append(' ').Append(word);
                continue;
            }

            lines.Add(current.ToString());
            if (lines.Count >= maxLines)
            {
                return lines;
            }

            current.Clear();
            AppendWord(current, word, maxCharsPerLine, lines, maxLines);
            if (lines.Count >= maxLines)
            {
                return lines;
            }
        }

        if (current.Length > 0 && lines.Count < maxLines)
        {
            lines.Add(current.ToString());
        }

        return lines;
    }

    public static string BuildWrappedText(
        double x,
        double y,
        double width,
        string? text,
        string fill,
        int fontSize,
        int fontWeight = 400,
        string anchor = "start",
        double lineHeightMultiplier = 1.18,
        int maxLines = 3)
    {
        var maxCharsPerLine = Math.Max(12, (int)Math.Round(width / Math.Max(fontSize * 0.52d, 1d)));
        var lines = WrapText(text, maxCharsPerLine, maxLines);
        if (lines.Count == 0)
        {
            return string.Empty;
        }

        var lineHeight = fontSize * lineHeightMultiplier;
        var builder = new StringBuilder();
        builder.Append($"""<text x="{FormatNumber(x)}" y="{FormatNumber(y)}" text-anchor="{EscapeXml(anchor)}" font-family="Segoe UI, Arial, sans-serif" font-size="{fontSize}" font-weight="{fontWeight}" fill="{EscapeXml(fill)}">""");

        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];
            var dy = index == 0 ? 0d : lineHeight;
            builder.Append($"""<tspan x="{FormatNumber(x)}" dy="{FormatNumber(dy)}">{EscapeXml(line)}</tspan>""");
        }

        builder.Append("</text>");
        return builder.ToString();
    }

    public static string BuildMetricCard(
        double x,
        double y,
        double width,
        double height,
        string label,
        string value,
        string note,
        string tone,
        string? valueTone = null)
    {
        var color = ToneColor(tone);
        var valueColor = string.IsNullOrWhiteSpace(valueTone) ? "#0f172a" : ToneColor(valueTone);
        var cardX = FormatNumber(x);
        var cardY = FormatNumber(y);
        var cardWidth = FormatNumber(width);
        var cardHeight = FormatNumber(height);

        return $"""
<g>
  <rect x="{cardX}" y="{cardY}" width="{cardWidth}" height="{cardHeight}" rx="18" fill="#ffffff" stroke="#d8e2ef" />
  <rect x="{cardX}" y="{cardY}" width="{cardWidth}" height="5" rx="18" fill="{color}" />
  <text x="{FormatNumber(x + 16d)}" y="{FormatNumber(y + 24d)}" font-family="Segoe UI, Arial, sans-serif" font-size="12" font-weight="700" fill="#64748b">{EscapeXml(label)}</text>
  <text x="{FormatNumber(x + 16d)}" y="{FormatNumber(y + 50d)}" font-family="Segoe UI, Arial, sans-serif" font-size="22" font-weight="800" fill="{valueColor}">{EscapeXml(value)}</text>
  {BuildWrappedText(x + 16d, y + 72d, width - 32d, note, "#475569", 12, 400, "start", 1.16, 2)}
</g>
""";
    }

    public static string ToneColor(string tone)
    {
        return tone.ToLowerInvariant() switch
        {
            "positive" => ChartPalette.Positive,
            "good" => ChartPalette.Primary,
            "warning" => ChartPalette.Warning,
            "danger" => ChartPalette.Danger,
            "info" => ChartPalette.Info,
            "neutral" => ChartPalette.Slate,
            _ => ChartPalette.Slate
        };
    }

    private static void AppendWord(StringBuilder current, string word, int maxCharsPerLine, List<string> lines, int maxLines)
    {
        if (word.Length <= maxCharsPerLine)
        {
            current.Append(word);
            return;
        }

        var remaining = word;
        while (remaining.Length > maxCharsPerLine && lines.Count < maxLines)
        {
            current.Append(remaining[..maxCharsPerLine]);
            lines.Add(current.ToString());
            current.Clear();
            remaining = remaining[maxCharsPerLine..];
        }

        if (remaining.Length > 0)
        {
            current.Append(remaining);
        }
    }
}

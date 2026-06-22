using System.Linq;

namespace HandWStat.Components.Shared;

internal static class ChartPalette
{
    public const string Player = "#e85d24";
    public const string Reference = "#94a3b8";
    public const string Primary = "#0f766e";
    public const string Secondary = "#2563eb";
    public const string Positive = "#16a34a";
    public const string Warning = "#d97706";
    public const string Danger = "#dc2626";
    public const string Accent = "#7c3aed";
    public const string Info = "#0891b2";
    public const string Pink = "#db2777";
    public const string Lime = "#65a30d";
    public const string Slate = "#64748b";
    public const string Gold = "#f59e0b";

    public static readonly List<string> PlayerMedianColors = [Player, Reference];

    public static readonly List<string> TeamComparisonColors = [Player, Primary];

    public static readonly List<string> ScatterStatusColors = [Reference, Positive, Warning, Danger];

    public static readonly List<string> SemanticSeriesColors =
    [
        Player,
        Primary,
        Secondary,
        Positive,
        Warning,
        Danger,
        Accent,
        Info,
        Pink,
        Lime,
        Slate,
        Gold
    ];

    public static readonly double[] GoalHeatStops =
    [
        0,
        8,
        16,
        24,
        32,
        40,
        48,
        56,
        64,
        72,
        80,
        88,
        94,
        100
    ];

    public static readonly string[] GoalHeatColors =
    [
        "#b56a63",
        "#c47668",
        "#d18470",
        "#db9378",
        "#e4a481",
        "#eaba8c",
        "#efc997",
        "#f2d7a1",
        "#f2e4ac",
        "#e7e7a9",
        "#cce0a0",
        "#afd196",
        "#8cc388",
        "#5fa377"
    ];

    public const string GoalZoneNeutralFill = "#d9d9d9";
    public const string GoalStrokeActive = "#17303d";
    public const string GoalStrokeInactive = "#0b2330";
    public const string TriggerStrokeInactive = "#0f1f28";

    public static string SeriesColor(int index)
    {
        if (SemanticSeriesColors.Count == 0)
        {
            return Player;
        }

        var normalized = (int)(Math.Abs((long)index) % SemanticSeriesColors.Count);
        return SemanticSeriesColors[normalized];
    }

    public static List<string> SeriesColors(int count)
    {
        if (count <= 0)
        {
            return [];
        }

        return Enumerable.Range(0, count)
            .Select(SeriesColor)
            .ToList();
    }
}

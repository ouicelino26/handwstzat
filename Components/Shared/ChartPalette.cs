using System.Linq;

namespace HandWStat.Components.Shared;

internal static class ChartPalette
{
    public const string Player = "#ff5b2e";
    public const string Reference = "#8fa39a";
    public const string Primary = "#177a5b";
    public const string Secondary = "#3979d8";
    public const string Positive = "#48bb7b";
    public const string Warning = "#e6ab4a";
    public const string Danger = "#e95c57";
    public const string Accent = "#d66f35";
    public const string Info = "#4d9cb3";
    public const string Pink = "#c55d79";
    public const string Lime = "#83a84e";
    public const string Slate = "#667a72";
    public const string Gold = "#d69832";

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
        "#b9635a",
        "#c8705e",
        "#d47e62",
        "#df9069",
        "#e8a377",
        "#edb784",
        "#f0ca91",
        "#efdaa0",
        "#e7e3aa",
        "#d5e2a7",
        "#b9d99c",
        "#94ca8c",
        "#70b979",
        "#48a36c"
    ];

    public const string GoalZoneNeutralFill = "#1d3b35";
    public const string GoalStrokeActive = "#8fe1b2";
    public const string GoalStrokeInactive = "rgba(143,225,178,0.28)";
    public const string TriggerStrokeInactive = "rgba(143,225,178,0.22)";

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

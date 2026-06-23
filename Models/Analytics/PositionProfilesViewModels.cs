namespace HandWStat.Models.Analytics;

public enum PositionProfileScatterBucket
{
    Above,
    Level,
    Below
}

public sealed record PositionProfileAxisViewModel(
    string Key,
    string Label,
    string Category,
    bool HigherIsBetter,
    string Format,
    double PlayerValue,
    double MedianValue,
    double Percentile,
    string Tone,
    string PlayerDisplayValue,
    string MedianDisplayValue,
    string DeltaDisplayValue,
    string DirectionLabel,
    string Summary,
    string CoachLegend,
    double DisplayPlayerValue,
    double DisplayMedianValue,
    double MinValue,
    double MaxValue)
{
    public double Delta => PlayerValue - MedianValue;

    public double Impact => Math.Abs(Delta);

    // The API contract already returns a favorable percentile for negative axes.
    public double DirectionalPercentile => Percentile;

    public string StatusLabel => DirectionalPercentile > 65
        ? "Fort"
        : DirectionalPercentile >= 35
            ? "Moyen"
            : "Fragile";

    public string StatusTone => DirectionalPercentile > 65
        ? "positive"
        : DirectionalPercentile >= 35
            ? "warning"
            : "danger";

    public PositionProfileScatterBucket Bucket => Impact <= GetTolerance()
        ? PositionProfileScatterBucket.Level
        : Delta > 0
            ? PositionProfileScatterBucket.Above
            : PositionProfileScatterBucket.Below;

    public string TooltipTitle => $"{Label} : {PlayerDisplayValue} face a la mediane {MedianDisplayValue}";

    public string TooltipDeltaTitle => $"{Label} : {DeltaDisplayValue} par rapport a la mediane";

    public string DirectionalPercentileDisplay => $"{DirectionalPercentile:0.#}%";

    public double RadarPlayerValue => NormalizeRadarValue(PlayerValue);

    public double RadarMedianValue => NormalizeRadarValue(MedianValue);

    private double GetTolerance()
    {
        return string.Equals(Format, "percent", StringComparison.OrdinalIgnoreCase) ? 0.35d : 0.08d;
    }

    private double NormalizeRadarValue(double value)
    {
        if (!double.IsFinite(MinValue) || !double.IsFinite(MaxValue) || MaxValue <= MinValue)
        {
            return Math.Clamp(DirectionalPercentile, 0d, 100d);
        }

        var normalized = (value - MinValue) * 100d / (MaxValue - MinValue);

        if (!HigherIsBetter)
        {
            normalized = 100d - normalized;
        }

        return Math.Clamp(Math.Round(normalized, 1, MidpointRounding.AwayFromZero), 0d, 100d);
    }
}

public sealed record PositionProfileScatterBounds(double Min, double Max, int TickAmount)
{
    public static PositionProfileScatterBounds Default { get; } = new(0, 100, 5);

    public double Range => Max - Min;

    public bool IsValid => double.IsFinite(Min) && double.IsFinite(Max) && Max > Min;
}

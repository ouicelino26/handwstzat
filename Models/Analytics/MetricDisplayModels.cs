namespace HandWStat.Models.Analytics;

public enum DataQualityLevel
{
    Unknown,
    Low,
    Medium,
    High
}

public sealed record RateDisplayModel
{
    public required string MetricCode { get; init; }

    public required string Label { get; init; }

    public double? Value { get; init; }

    public double? Numerator { get; init; }

    public double? Denominator { get; init; }

    public required string Unit { get; init; }

    public bool SampleReliable { get; init; }

    public double? MinimumSample { get; init; }

    public double? QualityScore { get; init; }

    public string? QualityReason { get; init; }

    public bool QualityKnown { get; init; }

    public string? MetricVersion { get; init; }

    public string? SourceLabel { get; init; }

    public string? NumeratorUnit { get; init; }

    public string? DenominatorUnit { get; init; }

    public required string QualityLabel { get; init; }

    public required string Tooltip { get; init; }

    public string Tone { get; init; } = "neutral";

    public string ValueLabel => Value.HasValue && double.IsFinite(Value.Value)
        ? HandballKpiHelper.FormatNumber(Value.Value)
        : "N/A";

    public bool HasVolume => Numerator.HasValue && Denominator.HasValue;

    public string VolumeLabel => HasVolume
        ? $"{HandballKpiHelper.FormatNumber(Numerator!.Value)}{FormatEvidenceUnit(NumeratorUnit)} / {HandballKpiHelper.FormatNumber(Denominator!.Value)}{FormatEvidenceUnit(DenominatorUnit)}"
        : "Volume non fourni par l'API";

    public string ReliabilityLabel
    {
        get
        {
            if (!Value.HasValue)
            {
                return Denominator == 0 && MetricVersion is not null
                    ? "Aucun tir dans le perimetre"
                    : "Indicateur non calculable";
            }

            if (!MinimumSample.HasValue || !Denominator.HasValue)
            {
                return "Fiabilite non renseignee";
            }

            return SampleReliable
                ? $"Volume suffisant (minimum {HandballKpiHelper.FormatNumber(MinimumSample.Value)})"
                : $"Volume limite (minimum {HandballKpiHelper.FormatNumber(MinimumSample.Value)})";
        }
    }

    private static string FormatEvidenceUnit(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : $" {value}";

    public static RateDisplayModel FromV2(
        string label,
        LeagueMetricValueDto metric,
        string numeratorUnit,
        string denominatorUnit)
    {
        return new RateDisplayModel
        {
            MetricCode = metric.MetricCode,
            Label = label,
            Value = metric.Value,
            Numerator = metric.Sample.Numerator,
            Denominator = metric.Sample.Denominator,
            Unit = "%",
            SampleReliable = metric.Quality.SampleReliable,
            MinimumSample = metric.Sample.MinimumSample,
            QualityScore = metric.Quality.QualityScore,
            QualityReason = metric.Quality.Reason,
            QualityKnown = true,
            MetricVersion = metric.MetricVersion,
            SourceLabel = AnalyticsSourceStatus.V2Complete.Label(),
            NumeratorUnit = numeratorUnit,
            DenominatorUnit = denominatorUnit,
            QualityLabel = metric.Value.HasValue
                ? metric.Quality.SampleReliable ? "Echantillon suffisant" : "Echantillon limite"
                : "Non calculable",
            Tooltip = $"{label} : preuve et qualite fournies par l'API v2.",
            Tone = metric.Value.HasValue
                ? metric.Quality.SampleReliable ? "good" : "warning"
                : "neutral"
        };
    }

    public static RateDisplayModel FromV1(
        string metricCode,
        string label,
        double? value,
        string unit,
        string tooltip,
        double? numerator = null,
        double? denominator = null,
        double? minimumSample = null,
        string tone = "neutral")
    {
        var calculableValue = value.HasValue
            && double.IsFinite(value.Value)
            && (!denominator.HasValue || denominator.Value > 0)
                ? value
                : null;
        var reliabilityKnown = denominator.HasValue
            && denominator.Value > 0
            && minimumSample.HasValue;
        var sampleReliable = reliabilityKnown && denominator!.Value >= minimumSample!.Value;

        return new RateDisplayModel
        {
            MetricCode = metricCode,
            Label = label,
            Value = calculableValue,
            Numerator = numerator,
            Denominator = denominator,
            Unit = unit,
            SampleReliable = sampleReliable,
            MinimumSample = minimumSample,
            QualityKnown = reliabilityKnown,
            SourceLabel = "API v1",
            QualityLabel = !calculableValue.HasValue
                ? "Non calculable"
                : !reliabilityKnown
                    ? "Qualite non renseignee"
                    : sampleReliable ? "Echantillon suffisant" : "Echantillon limite",
            Tooltip = tooltip,
            Tone = calculableValue.HasValue ? tone : "neutral"
        };
    }
}

public sealed record AnalysisScopeDisplayModel(
    string Competition,
    string Team,
    string Season,
    string Day,
    string Period,
    int? MatchCount,
    DateTimeOffset? GeneratedAt);

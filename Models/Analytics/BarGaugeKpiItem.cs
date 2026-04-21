namespace HandWStat.Models.Analytics;

public sealed record BarGaugeKpiItem(
    string Label,
    string Value,
    double GaugeValue,
    string Caption,
    string Tone = "neutral",
    string? Context = null,
    int Height = 150,
    double Min = 0,
    double Max = 100);

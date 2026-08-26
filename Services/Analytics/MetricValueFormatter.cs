using System.Globalization;
using HandWStat.Models.Analytics;

namespace HandWStat.Services.Analytics;

public static class MetricValueFormatter
{
    private static readonly CultureInfo FrCulture = CultureInfo.GetCultureInfo("fr-FR");

    public static string FormatForUi(double? value, AnalyticsMetricUnit unit, string locale = "fr-FR")
    {
        if (value is null) return "—";
        var culture = locale.StartsWith("fr", StringComparison.OrdinalIgnoreCase) ? FrCulture : CultureInfo.InvariantCulture;
        return unit switch
        {
            AnalyticsMetricUnit.Percent  => $"{value.Value.ToString("0.#", culture)} %",
            AnalyticsMetricUnit.Per60    => $"{value.Value.ToString("0.##", culture)} /60",
            AnalyticsMetricUnit.PerMatch => value.Value.ToString("0.##", culture),
            AnalyticsMetricUnit.Ratio    => value.Value.ToString("0.##", culture),
            AnalyticsMetricUnit.Count    => value.Value.ToString("0", culture),
            _                            => value.Value.ToString("0.##", culture),
        };
    }

    // CSV: machine-readable — always InvariantCulture, no unit suffix.
    // null → empty string (distinguishable from real zero).
    public static string FormatForCsv(double? value, AnalyticsMetricUnit unit)
    {
        if (value is null) return string.Empty;
        return unit switch
        {
            AnalyticsMetricUnit.Count => value.Value.ToString("0", CultureInfo.InvariantCulture),
            _                         => value.Value.ToString("0.####", CultureInfo.InvariantCulture),
        };
    }

    // PDF mirrors UI formatting.
    public static string FormatForPdf(double? value, AnalyticsMetricUnit unit, string locale = "fr-FR")
        => FormatForUi(value, unit, locale);
}

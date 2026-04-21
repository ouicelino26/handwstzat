namespace HandWStat.Models.Analytics;

public static class TableHeatToneHelper
{
    public static string HeatClass<T>(
        IEnumerable<T> items,
        Func<T, double?> selector,
        T current,
        bool higherIsBetter = true)
    {
        var values = items
            .Select(selector)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToArray();

        return HeatClass(values, selector(current), higherIsBetter);
    }

    public static string HeatClass(
        IEnumerable<double> values,
        double? currentValue,
        bool higherIsBetter = true)
    {
        var samples = values.ToArray();

        if (!currentValue.HasValue || samples.Length == 0)
        {
            return "metric-cell metric-cell-neutral";
        }

        var min = samples.Min();
        var max = samples.Max();
        if (Math.Abs(max - min) < 0.0001)
        {
            return "metric-cell metric-cell-neutral";
        }

        var normalized = (currentValue.Value - min) / (max - min);
        if (!higherIsBetter)
        {
            normalized = 1 - normalized;
        }

        return normalized switch
        {
            >= 0.85 => "metric-cell metric-cell-positive",
            >= 0.65 => "metric-cell metric-cell-good",
            >= 0.40 => "metric-cell metric-cell-warning",
            _ => "metric-cell metric-cell-danger"
        };
    }
}

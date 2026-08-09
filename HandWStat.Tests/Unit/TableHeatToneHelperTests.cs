using HandWStat.Models.Analytics;

namespace HandWStat.Tests.Unit;

public class TableHeatToneHelperTests
{
    private static string ClassOf(double[] values, double current, bool higherIsBetter = true)
        => TableHeatToneHelper.HeatClass(values, current, higherIsBetter);

    [Fact]
    public void HeatClass_TopValue_ReturnsPositive()
    {
        var result = ClassOf([1.0, 0.5, 0.2, 0.1], 1.0);
        Assert.Equal("metric-cell metric-cell-positive", result);
    }

    [Fact]
    public void HeatClass_UpperMidValue_ReturnsGood()
    {
        // normalized ~0.72 (between 0.65 and 0.85)
        var result = ClassOf([0, 0.25, 0.5, 0.75, 1.0], 0.75);
        Assert.Equal("metric-cell metric-cell-good", result);
    }

    [Fact]
    public void HeatClass_MidValue_ReturnsWarning()
    {
        // normalized = 0.5 → warning (>=0.40, <0.65)
        var result = ClassOf([0.0, 1.0], 0.5);
        Assert.Equal("metric-cell metric-cell-warning", result);
    }

    [Fact]
    public void HeatClass_BottomValue_ReturnsDanger()
    {
        var result = ClassOf([1.0, 0.5, 0.2, 0.1], 0.1);
        Assert.Equal("metric-cell metric-cell-danger", result);
    }

    [Fact]
    public void HeatClass_AllIdenticalValues_ReturnsNeutral()
    {
        var result = ClassOf([5.0, 5.0, 5.0], 5.0);
        Assert.Equal("metric-cell metric-cell-neutral", result);
    }

    [Fact]
    public void HeatClass_NullCurrentValue_ReturnsNeutral()
    {
        var result = TableHeatToneHelper.HeatClass([1.0, 2.0, 3.0], null);
        Assert.Equal("metric-cell metric-cell-neutral", result);
    }

    [Fact]
    public void HeatClass_EmptyValues_ReturnsNeutral()
    {
        var result = ClassOf([], 1.0);
        Assert.Equal("metric-cell metric-cell-neutral", result);
    }

    [Fact]
    public void HeatClass_LowerIsBetter_InvertsNormalization()
    {
        // lowest value should be "positive" when higherIsBetter=false
        var result = ClassOf([1.0, 0.5, 0.1], 0.1, higherIsBetter: false);
        Assert.Equal("metric-cell metric-cell-positive", result);
    }
}

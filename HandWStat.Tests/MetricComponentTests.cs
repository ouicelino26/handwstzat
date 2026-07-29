using HandWStat.Components.Shared;
using HandWStat.Models.Analytics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace HandWStat.Tests;

public sealed class MetricComponentTests
{
    [Fact]
    public async Task RateMetricCard_ValidValue_RendersVolumeReliabilityAndAccessibleTone()
    {
        var model = RateDisplayModel.FromV1(
            "SHOT_RATE",
            "Taux de tir",
            50,
            "%",
            "Buts sur tentatives.",
            numerator: 5,
            denominator: 10,
            minimumSample: 4,
            tone: "good");

        var html = await RenderAsync<RateMetricCard>(new Dictionary<string, object?>
        {
            [nameof(RateMetricCard.Model)] = model
        });

        Assert.Contains("50", html, StringComparison.Ordinal);
        Assert.Contains("5 / 10", html, StringComparison.Ordinal);
        Assert.Contains("Volume suffisant", html, StringComparison.Ordinal);
        Assert.Contains("is-good", html, StringComparison.Ordinal);
        Assert.Contains("&#x2713;", html, StringComparison.Ordinal);
        Assert.Contains("aria-label", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RateMetricCard_NullValue_RendersNAAndTextStatus()
    {
        var model = RateDisplayModel.FromV1(
            "EMPTY_RATE",
            "Taux vide",
            0,
            "%",
            "Aucun volume.",
            numerator: 0,
            denominator: 0,
            minimumSample: 1);

        var html = await RenderAsync<RateMetricCard>(new Dictionary<string, object?>
        {
            [nameof(RateMetricCard.Model)] = model
        });

        Assert.Contains("N/A", html, StringComparison.Ordinal);
        Assert.Contains("Indicateur non calculable", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RateMetricCard_InsufficientVolume_RendersExplicitWarning()
    {
        var model = RateDisplayModel.FromV1(
            "SAVE_RATE",
            "Taux d'arret",
            50,
            "%",
            "Arrets sur tirs subis.",
            numerator: 2,
            denominator: 4,
            minimumSample: 10,
            tone: "warning");

        var html = await RenderAsync<RateMetricCard>(new Dictionary<string, object?>
        {
            [nameof(RateMetricCard.Model)] = model
        });

        Assert.Contains("Volume limite", html, StringComparison.Ordinal);
        Assert.Contains("is-warning", html, StringComparison.Ordinal);
        Assert.Contains("!", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DataQualityBadge_Unknown_RendersExplicitUnknownLabel()
    {
        var html = await RenderAsync<DataQualityBadge>(new Dictionary<string, object?>
        {
            [nameof(DataQualityBadge.Level)] = DataQualityLevel.Unknown
        });

        Assert.Contains("Qualite non renseignee", html, StringComparison.Ordinal);
        Assert.Contains("?", html, StringComparison.Ordinal);
        Assert.Contains("role=\"status\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnalysisScopeSummary_RendersCompleteVisibleScope()
    {
        var scope = new AnalysisScopeDisplayModel(
            "LBE",
            "Metz",
            "2025-2026",
            "J12",
            "Du 01/01/2026 au 31/01/2026",
            4,
            new DateTimeOffset(2026, 1, 31, 10, 30, 0, TimeSpan.Zero));

        var html = await RenderAsync<AnalysisScopeSummary>(new Dictionary<string, object?>
        {
            [nameof(AnalysisScopeSummary.Scope)] = scope
        });

        Assert.Contains("LBE", html, StringComparison.Ordinal);
        Assert.Contains("Metz", html, StringComparison.Ordinal);
        Assert.Contains("J12", html, StringComparison.Ordinal);
        Assert.Contains("4", html, StringComparison.Ordinal);
        Assert.Contains("Genere le", html, StringComparison.Ordinal);
    }

    private static async Task<string> RenderAsync<TComponent>(Dictionary<string, object?> parameters)
        where TComponent : IComponent
    {
        var services = new ServiceCollection().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, NullLoggerFactory.Instance);

        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<TComponent>(ParameterView.FromDictionary(parameters));
            return output.ToHtmlString();
        });
    }
}

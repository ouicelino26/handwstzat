using HandWStat.Components.Shared;
using HandWStat.Models.Analytics;
using HandWStat.Services.Analytics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace HandWStat.Tests;

public sealed class LeagueAnalyticsComponentTests
{
    [Fact]
    public async Task CompleteGoalkeeperView_RendersSectionsOrderSourceScopeAndEvidence()
    {
        var view = LeaguePlayerAnalyticsMapper.FromV2(
            LeagueAnalyticsTestData.CompleteResponse(isGoalkeeper: true),
            LeagueAnalyticsTestData.Scope());
        var result = new LeaguePlayerAnalyticsLoadResult
        {
            Analytics = view,
            Source = AnalyticsSourceStatus.V2Complete
        };

        var html = await RenderAsync<LeaguePlayerStatsPanel>(new()
        {
            [nameof(LeaguePlayerStatsPanel.Result)] = result
        });

        Assert.Contains("Statistiques offensives", html, StringComparison.Ordinal);
        Assert.Contains("Statistiques défensives", html, StringComparison.Ordinal);
        Assert.Contains("Statistiques gardienne", html, StringComparison.Ordinal);
        Assert.Contains("data-goalkeeper-section=\"true\"", html, StringComparison.Ordinal);
        Assert.Contains("V2_COMPLETE", html, StringComparison.Ordinal);
        Assert.Contains("MetricVersion : 1.0", html, StringComparison.Ordinal);
        Assert.Contains("SampleReliable", html, StringComparison.Ordinal);
        Assert.Contains("Numérateur", html, StringComparison.Ordinal);
        Assert.Contains("Dénominateur", html, StringComparison.Ordinal);
        Assert.Contains("API v2 compl", html, StringComparison.Ordinal);
        Assert.Contains("Scope statistique actif", html, StringComparison.Ordinal);
        Assert.Contains("aria-label", html, StringComparison.Ordinal);

        AssertInOrder(
            html,
            "Buts total",
            "Buts dans le jeu",
            "Buts sur 7 m",
            "Passes d",
            "7 m obtenus",
            "Sanctions obtenues",
            "Pertes de balle",
            "Mauvaises passes",
            "Passes pivot rat",
            "Taux de tir total",
            "Taux de tir dans le jeu",
            "Taux de tir sur 7 m");
    }

    [Fact]
    public async Task FieldPlayerView_DoesNotRenderGoalkeeperSection()
    {
        var view = LeaguePlayerAnalyticsMapper.FromV2(
            LeagueAnalyticsTestData.CompleteResponse(isGoalkeeper: false),
            LeagueAnalyticsTestData.Scope());
        var result = new LeaguePlayerAnalyticsLoadResult
        {
            Analytics = view,
            Source = AnalyticsSourceStatus.V2Complete
        };

        var html = await RenderAsync<LeaguePlayerStatsPanel>(new()
        {
            [nameof(LeaguePlayerStatsPanel.Result)] = result
        });

        Assert.DoesNotContain("Statistiques gardienne", html, StringComparison.Ordinal);
        Assert.DoesNotContain("data-goalkeeper-section", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MissingPivotAndV1Quality_RenderExplicitUnavailableAndUnknownStates()
    {
        var view = LeaguePlayerAnalyticsMapper.FromV1(
            42,
            LeagueAnalyticsTestData.V1Snapshot(),
            LeagueAnalyticsTestData.Scope());
        var result = new LeaguePlayerAnalyticsLoadResult
        {
            Analytics = view,
            Source = AnalyticsSourceStatus.V1Partial
        };

        var html = await RenderAsync<LeaguePlayerStatsPanel>(new()
        {
            [nameof(LeaguePlayerStatsPanel.Result)] = result
        });

        Assert.Contains("Passes pivot rat", html, StringComparison.Ordinal);
        Assert.Contains("non disponible avec les fichiers actuels", html, StringComparison.Ordinal);
        Assert.Contains("destination de la mauvaise passe", html, StringComparison.Ordinal);
        Assert.Contains("non fournie par", html, StringComparison.Ordinal);
        Assert.Contains("Unknown", html, StringComparison.Ordinal);
        Assert.Contains("MetricVersion", html, StringComparison.Ordinal);
        Assert.Contains("Non fournie", html, StringComparison.Ordinal);
        Assert.Contains("V1_PARTIAL", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Passes pivot ratées</span><strong>2", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ZeroDenominator_RendersNAAndNoShotMessage()
    {
        var view = LeaguePlayerAnalyticsMapper.FromV2(
            LeagueAnalyticsTestData.ZeroDenominatorResponse(),
            LeagueAnalyticsTestData.Scope());
        var result = new LeaguePlayerAnalyticsLoadResult
        {
            Analytics = view,
            Source = AnalyticsSourceStatus.V2Complete
        };

        var html = await RenderAsync<LeaguePlayerStatsPanel>(new()
        {
            [nameof(LeaguePlayerStatsPanel.Result)] = result
        });

        Assert.Contains("N/A", html, StringComparison.Ordinal);
        Assert.Contains("Aucun tir dans le perimetre", html, StringComparison.Ordinal);
        Assert.Contains("0 buts / 0 tirs", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ContractError_RendersSafeDiagnosticsAndNoFallbackMetrics()
    {
        var result = new LeaguePlayerAnalyticsLoadResult
        {
            Source = AnalyticsSourceStatus.ContractError,
            Error = new LeagueAnalyticsError(
                "La réponse statistique v2 ne respecte pas le contrat attendu.",
                "LEAGUE_V2_CONTRACT_INVALID",
                "corr-safe",
                false,
                System.Net.HttpStatusCode.OK)
        };

        var html = await RenderAsync<LeaguePlayerStatsPanel>(new()
        {
            [nameof(LeaguePlayerStatsPanel.Result)] = result
        });

        Assert.Contains("Erreur de contrat v2", html, StringComparison.Ordinal);
        Assert.Contains("LEAGUE_V2_CONTRACT_INVALID", html, StringComparison.Ordinal);
        Assert.Contains("corr-safe", html, StringComparison.Ordinal);
        Assert.Contains("Réessai possible", html, StringComparison.Ordinal);
        Assert.Contains("CONTRACT_ERROR", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Buts total", html, StringComparison.Ordinal);
    }

    private static void AssertInOrder(string text, params string[] values)
    {
        var previous = -1;
        foreach (var value in values)
        {
            var current = text.IndexOf(value, previous + 1, StringComparison.Ordinal);
            Assert.True(current > previous, $"{value} should appear after the previous metric.");
            previous = current;
        }
    }

    private static async Task<string> RenderAsync<TComponent>(Dictionary<string, object?> parameters)
        where TComponent : IComponent
    {
        var services = new ServiceCollection().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, NullLoggerFactory.Instance);

        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<TComponent>(
                ParameterView.FromDictionary(parameters));
            return output.ToHtmlString();
        });
    }
}

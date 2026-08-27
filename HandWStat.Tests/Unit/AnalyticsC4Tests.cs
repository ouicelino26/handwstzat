using HandWStat.Services.Navigation;

namespace HandWStat.Tests.Unit;

/// <summary>
/// C4 — Cross-page context continuity.
/// Tests cover AnalyticsNavigationService URI builders and deep-link policies.
/// </summary>
public class AnalyticsC4Tests
{
    // ── BuildPlayerUri ─────────────────────────────────────────────────────────

    [Fact]
    public void BuildPlayerUri_AllParams_IncludesAllQueryParams()
    {
        var uri = AnalyticsNavigationService.BuildPlayerUri(42, teamId: 7, season: "2025-2026", competitionId: 3);
        Assert.Contains("playerId=42", uri);
        Assert.Contains("teamId=7", uri);
        Assert.Contains("season=2025-2026", uri);
        Assert.Contains("competitionId=3", uri);
        Assert.StartsWith("/players?", uri);
    }

    [Fact]
    public void BuildPlayerUri_NullOptionals_OmitsNullParams()
    {
        var uri = AnalyticsNavigationService.BuildPlayerUri(99);
        Assert.Contains("playerId=99", uri);
        Assert.DoesNotContain("teamId", uri);
        Assert.DoesNotContain("season", uri);
        Assert.DoesNotContain("competitionId", uri);
    }

    [Fact]
    public void BuildPlayerUri_NullTeamId_OmitsTeamId()
    {
        var uri = AnalyticsNavigationService.BuildPlayerUri(5, teamId: null, season: "2025-2026");
        Assert.DoesNotContain("teamId", uri);
        Assert.Contains("season=2025-2026", uri);
    }

    [Fact]
    public void BuildPlayerUri_SeasonWithDash_UrlEncodedCorrectly()
    {
        // Season "2025-2026" contains a dash which is safe in a query string
        var uri = AnalyticsNavigationService.BuildPlayerUri(1, season: "2025-2026");
        Assert.Contains("season=2025-2026", uri);
    }

    [Fact]
    public void BuildPlayerUri_EmptySeason_OmitsSeason()
    {
        var uri = AnalyticsNavigationService.BuildPlayerUri(1, season: "");
        Assert.DoesNotContain("season", uri);
    }

    // ── BuildCompareUri ────────────────────────────────────────────────────────

    [Fact]
    public void BuildCompareUri_SinglePlayer_IncludesPlayerId()
    {
        var uri = AnalyticsNavigationService.BuildCompareUri([42]);
        Assert.Contains("playerIds=42", uri);
        Assert.StartsWith("/compare?", uri);
    }

    [Fact]
    public void BuildCompareUri_DuplicateIds_Deduplicated()
    {
        var uri = AnalyticsNavigationService.BuildCompareUri([1, 2, 1, 3, 2]);
        // Should only contain each ID once
        var idsParam = uri.Split("playerIds=")[1].Split("&")[0];
        var ids = idsParam.Split(',');
        Assert.Equal(ids.Length, ids.Distinct().Count());
        Assert.Equal(3, ids.Length); // 1, 2, 3
    }

    [Fact]
    public void BuildCompareUri_MoreThan6Ids_TakesFirst6()
    {
        var uri = AnalyticsNavigationService.BuildCompareUri([1, 2, 3, 4, 5, 6, 7, 8]);
        var idsParam = uri.Split("playerIds=")[1].Split("&")[0];
        var ids = idsParam.Split(',');
        Assert.Equal(6, ids.Length);
    }

    [Fact]
    public void BuildCompareUri_WithCompetitionAndSeason_IncludesBoth()
    {
        var uri = AnalyticsNavigationService.BuildCompareUri([1, 2], competitionId: 5, season: "2025-2026");
        Assert.Contains("competitionId=5", uri);
        Assert.Contains("season=2025-2026", uri);
    }

    [Fact]
    public void BuildCompareUri_NullCompetitionNullSeason_OmitsBoth()
    {
        var uri = AnalyticsNavigationService.BuildCompareUri([1, 2]);
        Assert.DoesNotContain("competitionId", uri);
        Assert.DoesNotContain("season", uri);
    }

    [Fact]
    public void BuildCompareUri_ExactlyMaxSlots_AllIncluded()
    {
        var uri = AnalyticsNavigationService.BuildCompareUri([10, 20, 30, 40, 50, 60]);
        var idsParam = uri.Split("playerIds=")[1].Split("&")[0];
        var ids = idsParam.Split(',');
        Assert.Equal(6, ids.Length);
    }

    // ── URI shape ─────────────────────────────────────────────────────────────

    [Fact]
    public void BuildPlayerUri_ReturnsRelativeUri()
    {
        var uri = AnalyticsNavigationService.BuildPlayerUri(1);
        Assert.StartsWith("/", uri);
        Assert.False(uri.StartsWith("http", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void BuildCompareUri_ReturnsRelativeUri()
    {
        var uri = AnalyticsNavigationService.BuildCompareUri([1]);
        Assert.StartsWith("/", uri);
        Assert.False(uri.StartsWith("http", StringComparison.OrdinalIgnoreCase));
    }

    // ── Invariant: Season encoding ─────────────────────────────────────────────

    [Fact]
    public void BuildPlayerUri_SeasonWithSpecialChars_IsEncoded()
    {
        // Season values with spaces should be safely encoded
        var uri = AnalyticsNavigationService.BuildPlayerUri(1, season: "2025 2026");
        // Uri.EscapeDataString encodes space as %20
        Assert.Contains("season=", uri);
        Assert.DoesNotContain(" ", uri); // raw space must not appear in URI
        Assert.Contains("%20", uri);
    }

    // ── Deep-link policy tests (documented behavior) ───────────────────────────

    [Fact]
    public void BuildCompareUri_EmptyList_StillBuildsUri()
    {
        // Edge case: empty list → playerIds= (empty)
        var uri = AnalyticsNavigationService.BuildCompareUri([]);
        Assert.StartsWith("/compare?", uri);
        Assert.Contains("playerIds=", uri);
    }

    [Fact]
    public void BuildPlayerUri_PlayerId_IsFirstParam()
    {
        var uri = AnalyticsNavigationService.BuildPlayerUri(77);
        // playerId should be the first (and possibly only) query param
        var queryPart = uri.Substring(uri.IndexOf('?') + 1);
        Assert.StartsWith("playerId=", queryPart);
    }

    [Fact]
    public void BuildCompareUri_PlayerIdsIsCommaSeparated()
    {
        var uri = AnalyticsNavigationService.BuildCompareUri([1, 2, 3]);
        // playerIds should be comma-separated (commas not encoded for readability)
        Assert.Contains("playerIds=1,2,3", uri);
    }
}

namespace HandWStat.Services.Navigation;

/// <summary>
/// C4 — Builds context-preserving URIs for cross-page navigation.
/// Navigation only — no business logic.
/// </summary>
public static class AnalyticsNavigationService
{
    private const int MaxCompareSlots = 6;

    /// <summary>
    /// Builds a /players URI with the given context preserved as query params.
    /// Null/empty params are omitted.
    /// </summary>
    public static string BuildPlayerUri(
        int playerId,
        int? teamId = null,
        string? season = null,
        int? competitionId = null)
    {
        var parts = new List<string>(4)
        {
            "playerId=" + playerId
        };
        if (teamId.HasValue)
            parts.Add("teamId=" + teamId.Value);
        if (!string.IsNullOrEmpty(season))
            parts.Add("season=" + Uri.EscapeDataString(season));
        if (competitionId.HasValue)
            parts.Add("competitionId=" + competitionId.Value);
        return "/players?" + string.Join("&", parts);
    }

    /// <summary>
    /// Builds a /compare URI with up to 6 deduplicated player IDs as a comma-separated list.
    /// Commas are safe in query string values and are not encoded here so that the value
    /// remains human-readable and round-trips cleanly through Blazor's SupplyParameterFromQuery.
    /// </summary>
    public static string BuildCompareUri(
        IEnumerable<int> playerIds,
        int? competitionId = null,
        string? season = null)
    {
        var ids = playerIds
            .Distinct()
            .Take(MaxCompareSlots)
            .ToList();

        var parts = new List<string>(3)
        {
            "playerIds=" + string.Join(",", ids)
        };
        if (competitionId.HasValue)
            parts.Add("competitionId=" + competitionId.Value);
        if (!string.IsNullOrEmpty(season))
            parts.Add("season=" + Uri.EscapeDataString(season));
        return "/compare?" + string.Join("&", parts);
    }
}

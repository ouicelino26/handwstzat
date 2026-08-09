using HandWStat.Models.Contracts;
using HandWStat.Services;
using HandWStat.Services.Api;

namespace HandWStat.Models.Analytics;

// ── Target types ──────────────────────────────────────────────────────────────

/// <summary>
/// Human-facing target selection for export.
/// Maps internally to API ExportScope values.
/// </summary>
public enum ExportTargetType
{
    FullScope,
    Team,
    Players,
    Goalkeepers,
    Matches,
}

/// <summary>
/// Maps a UI target to the API Scope string.
/// </summary>
public static class ExportTargetMapper
{
    public static string ToApiScope(ExportTargetType target, int playerCount) => target switch
    {
        ExportTargetType.FullScope => "SEASON",
        ExportTargetType.Team => "TEAM",
        ExportTargetType.Players => playerCount == 1 ? "PLAYER" : "MULTIPLE_PLAYERS",
        ExportTargetType.Goalkeepers => "GOALKEEPERS",
        ExportTargetType.Matches => "MATCH",
        _ => "SEASON",
    };
}

// ── Preset ────────────────────────────────────────────────────────────────────

public enum ExportPreset
{
    FullAnalysis,
    Staff,
    Players,
    Matches,
    Spatial,
    Custom,
}

// ── Section catalog ───────────────────────────────────────────────────────────

public sealed record ExportSectionItem(
    string ApiKey,
    string Label,
    string Description,
    bool DefaultEnabled,
    bool IsAdvanced,
    bool DependsOnShots = false);

public static class ExportSectionCatalog
{
    public static readonly IReadOnlyList<ExportSectionItem> All =
    [
        new("SEASON_PLAYER_RESULTS", "Bilan final des joueuses",  "Totaux de saison, classements et leaders dans un seul fichier", true, false),
        new("SEASON_SUMMARY",    "Synthèse du périmètre",      "Totaux agrégés sur le périmètre analysé",                  true,  false),
        new("TEAMS",             "Équipes",                     "Statistiques par équipe",                                  true,  false),
        new("PLAYERS",           "Joueuses",                    "Statistiques globales par joueuse",                        true,  false),
        new("PLAYERS_PER_MATCH", "Performances par match",      "Statistiques de chaque joueuse match par match",           true,  false),
        new("GOALKEEPERS",       "Gardiennes",                  "Métriques spécifiques aux gardiennes",                     true,  false),
        new("MATCHES",           "Matchs",                      "Liste et résultats des matchs",                            true,  false),
        new("SHOTS",             "Tirs",                        "Logs de tirs avec zones et résultats",                     true,  false),
        new("DEFENSE",           "Défense",                     "Statistiques défensives",                                  true,  false),
        new("METRIC_DICTIONARY", "Dictionnaire des métriques",  "Définition de toutes les métriques du classeur",           true,  false),
        new("DATA_QUALITY",      "Qualité des données",         "Complétude et fiabilité des données par match",            true,  false),
        new("EVENTS",            "Événements bruts",            "Log détaillé de chaque événement. Augmente la taille.",    false, true),
    ];

    public static readonly IReadOnlyDictionary<string, ExportSectionItem> ByKey =
        All.ToDictionary(s => s.ApiKey, StringComparer.OrdinalIgnoreCase);

    // Preset section sets (excluding advanced EVENTS unless explicitly enabled)
    public static IReadOnlySet<string> ForPreset(ExportPreset preset) => preset switch
    {
        ExportPreset.FullAnalysis => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SEASON_SUMMARY", "SEASON_PLAYER_RESULTS", "TEAMS", "PLAYERS", "PLAYERS_PER_MATCH",
            "GOALKEEPERS", "MATCHES", "SHOTS", "DEFENSE", "METRIC_DICTIONARY", "DATA_QUALITY",
        },
        ExportPreset.Staff => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SEASON_SUMMARY", "SEASON_PLAYER_RESULTS", "TEAMS", "PLAYERS", "MATCHES", "DATA_QUALITY",
        },
        ExportPreset.Players => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SEASON_PLAYER_RESULTS", "PLAYERS", "PLAYERS_PER_MATCH", "GOALKEEPERS", "DATA_QUALITY",
        },
        ExportPreset.Matches => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "MATCHES", "PLAYERS_PER_MATCH", "SHOTS",
        },
        ExportPreset.Spatial => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SHOTS",
        },
        ExportPreset.Custom => new HashSet<string>(StringComparer.OrdinalIgnoreCase),
        _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SEASON_SUMMARY", "SEASON_PLAYER_RESULTS", "TEAMS", "PLAYERS", "PLAYERS_PER_MATCH",
            "GOALKEEPERS", "MATCHES", "SHOTS", "DEFENSE", "METRIC_DICTIONARY", "DATA_QUALITY",
        },
    };
}

// ── Export scope state ────────────────────────────────────────────────────────

/// <summary>
/// Represents the combined export scope derived from the global analysis scope
/// plus any export-specific refinements. This is the single source of truth
/// for the export workspace — no parallel scope copy is maintained.
/// </summary>
public sealed class ExportScopeState
{
    // ── From global scope ───────────────────────────────────────────────────

    public int? CompetitionId { get; set; }
    public string? CompetitionName { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? Season { get; set; }

    /// <summary>
    /// Day from global scope. It is propagated to the export API and is also
    /// used to refine the client-side match picker.
    /// </summary>
    public string? Day { get; set; }

    // ── Export-specific refinements ─────────────────────────────────────────

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Lazily loaded player list scoped to the current competition/team/season.
    /// </summary>
    public IReadOnlyList<PlayerListItemDto> AvailablePlayers { get; set; } = [];

    /// <summary>
    /// Lazily loaded match list scoped to the current competition/team/season.
    /// </summary>
    public IReadOnlyList<MatchListItemDto> AvailableMatches { get; set; } = [];

    /// <summary>
    /// True when players are being loaded.
    /// </summary>
    public bool PlayersLoading { get; set; }

    /// <summary>
    /// True when matches are being loaded.
    /// </summary>
    public bool MatchesLoading { get; set; }

    // ── Factory ─────────────────────────────────────────────────────────────

    public static ExportScopeState FromSnapshot(AnalysisScopeSnapshot snapshot) => new()
    {
        CompetitionId = snapshot.CompetitionId,
        CompetitionName = snapshot.CompetitionName,
        TeamId = snapshot.TeamId,
        TeamName = snapshot.TeamName,
        Season = snapshot.Season,
        Day = snapshot.Day,
    };

    // ── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Produces a human-readable summary line. Example: "LBE · Brest · 2025-2026 · J18"
    /// </summary>
    public string ToDisplayLine()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(CompetitionName)) parts.Add(CompetitionName);
        if (!string.IsNullOrWhiteSpace(TeamName)) parts.Add(TeamName);
        if (!string.IsNullOrWhiteSpace(Season)) parts.Add(Season);
        if (!string.IsNullOrWhiteSpace(Day)) parts.Add(Day);
        return parts.Count > 0 ? string.Join(" · ", parts) : "Périmètre global";
    }

    /// <summary>
    /// Returns filtered teams from reference data based on current competition.
    /// </summary>
    public IReadOnlyList<TeamDto> GetFilteredTeams(AnalyticsReferenceData refData)
    {
        if (refData.Teams.Count == 0) return [];

        if (!CompetitionId.HasValue) return refData.Teams;

        // Filter teams appearing in matches of this competition
        // (SmartFilterCatalog requires match list; fall back to all teams if unavailable)
        var teamsFromMatches = SmartFilterCatalog.GetTeams(refData, AvailableMatches, AvailableMatches.Count > 0);
        return teamsFromMatches;
    }

    /// <summary>
    /// Returns seasons derived from available matches.
    /// </summary>
    public IReadOnlyList<string> GetSeasons()
        => MatchFilterCatalog.GetSeasons(AvailableMatches);

    /// <summary>
    /// Returns days filtered by current season.
    /// </summary>
    public IReadOnlyList<string> GetDays()
        => MatchFilterCatalog.GetDays(AvailableMatches, Season);
}

// ── Export generation state machine ─────────────────────────────────────────

public enum ExportGenerationStatus
{
    Ready,
    Validating,
    Generating,
    Downloading,
    Saving,
    Completed,
    Cancelled,
    Error,
}

// ── Preview model ─────────────────────────────────────────────────────────────

/// <summary>
/// Derived model that feeds the summary panel AND BuildRequest.
/// What the user sees in the summary is exactly what goes to the server.
/// </summary>
public sealed class ExportPreviewModel
{
    public string ScopeDisplayLine { get; init; } = "";
    public string TargetLabel { get; init; } = "";
    public int PlayerCount { get; init; }
    public int MatchCount { get; init; }
    public int SectionCount { get; init; }
    public IReadOnlyList<string> EnabledSectionLabels { get; init; } = [];
    public bool IncludeRawEvents { get; init; }
    public bool IncludeShotCoordinates { get; init; }
    public bool IncludeDataQuality { get; init; }
    public string FormatLabel { get; init; } = "Excel (.xlsx)";

    // Day propagation warning
    public bool DayFilterIsDisplayOnly { get; init; }
    public string? DayDisplayValue { get; init; }
}

// ── Validation ────────────────────────────────────────────────────────────────

public sealed record ExportValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static ExportValidationResult Ok() => new(true, []);
    public static ExportValidationResult Fail(params string[] errors) => new(false, errors);
}

public static class ExportRequestValidator
{
    public static ExportValidationResult Validate(
        ExportTargetType target,
        int? teamId,
        IReadOnlyList<int> selectedPlayerIds,
        IReadOnlyList<int> selectedMatchIds,
        DateTime? dateFrom,
        DateTime? dateTo,
        IReadOnlyList<string> sections)
    {
        var errors = new List<string>();

        if (target == ExportTargetType.Team && !teamId.HasValue)
        {
            errors.Add("Sélectionnez une équipe pour l'export Équipe.");
        }

        if (target == ExportTargetType.Players && selectedPlayerIds.Count == 0)
        {
            errors.Add("Sélectionnez au moins une joueuse.");
        }

        if (target == ExportTargetType.Matches && selectedMatchIds.Count == 0)
        {
            errors.Add("Sélectionnez au moins un match.");
        }

        if (dateFrom.HasValue && dateTo.HasValue && dateFrom > dateTo)
        {
            errors.Add("La date de début doit être antérieure à la date de fin.");
        }

        if (sections.Count == 0)
        {
            errors.Add("Sélectionnez au moins une feuille à inclure.");
        }

        return errors.Count == 0 ? ExportValidationResult.Ok() : new(false, errors);
    }
}

// ── BuildRequest helper ───────────────────────────────────────────────────────

public static class ExportRequestBuilder
{
    /// <summary>
    /// Builds the DTO sent to the server. This is the only place where
    /// sections are assembled — the preview model must derive from the same call.
    /// Sections are deduped with a HashSet to prevent duplicates.
    /// </summary>
    public static AnalyticsExportRequestDto Build(
        ExportScopeState scope,
        ExportTargetType target,
        int? selectedTeamId,
        IReadOnlyList<int> selectedPlayerIds,
        IReadOnlyList<int> selectedMatchIds,
        IReadOnlySet<string> selectedSections,
        bool includeRawEvents,
        bool includeShotCoordinates,
        bool includeDataQuality)
    {
        // Deduplicate sections
        var sections = new HashSet<string>(selectedSections, StringComparer.OrdinalIgnoreCase);
        if (includeDataQuality) sections.Add("DATA_QUALITY");
        if (includeRawEvents) sections.Add("EVENTS");
        // IncludeShotCoordinates requires SHOTS section
        if (includeShotCoordinates) sections.Add("SHOTS");

        var apiScope = ExportTargetMapper.ToApiScope(target, selectedPlayerIds.Count);

        return new AnalyticsExportRequestDto
        {
            Scope = apiScope,
            SeasonLabel = string.IsNullOrWhiteSpace(scope.Season) ? null : scope.Season.Trim(),
            // SeasonLabel is the same canonical filter used by the dashboard.
            // Sending 2025 together with "2025-2026" can exclude 2026 matches.
            SeasonYear = null,
            CompetitionId = scope.CompetitionId,
            TeamId = target == ExportTargetType.Team ? selectedTeamId : scope.TeamId,
            PlayerIds = selectedPlayerIds.Count > 0 ? selectedPlayerIds.ToList() : null,
            MatchIds = selectedMatchIds.Count > 0 ? selectedMatchIds.ToList() : null,
            DateFrom = scope.DateFrom,
            DateTo = scope.DateTo,
            Day = string.IsNullOrWhiteSpace(scope.Day) ? null : scope.Day.Trim(),
            Sections = sections.ToList(),
            IncludeRawEvents = includeRawEvents,
            IncludeShotCoordinates = includeShotCoordinates,
            IncludeDataQuality = includeDataQuality,
            RequestedBy = "HandWStat",
        };
    }

    /// <summary>
    /// Builds the one-click season workbook request for the current analysis scope.
    /// The workbook contains the consolidated player results, season leaders,
    /// and the complete analytics columns used by the global player table.
    /// </summary>
    public static AnalyticsExportRequestDto BuildSeasonResults(ExportScopeState scope) =>
        Build(
            scope,
            ExportTargetType.FullScope,
            scope.TeamId,
            [],
            [],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "SEASON_PLAYER_RESULTS",
                "PLAYERS"
            },
            includeRawEvents: false,
            includeShotCoordinates: false,
            includeDataQuality: false);

}

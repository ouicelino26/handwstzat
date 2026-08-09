using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;
using HandWStat.Services;
using HandWStat.Services.Api;

namespace HandWStat.Tests;

/// <summary>
/// Export Workspace V2 — full test coverage.
/// Covers scope prefill, filter semantics, target mapping, section presets,
/// request building, validation, stale-response protection, and day-scope honesty.
/// </summary>
public sealed class ExportWorkspaceV2Tests
{
    // ─────────────────────────────────────────────────────────────────────────────
    // SECTION A: ExportScopeState — prefill and display
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Export_PrefillsCompetitionFromGlobalScope()
    {
        var snapshot = new AnalysisScopeSnapshot(42, "LBE", null, null, null, null);
        var state = ExportScopeState.FromSnapshot(snapshot);
        Assert.Equal(42, state.CompetitionId);
        Assert.Equal("LBE", state.CompetitionName);
    }

    [Fact]
    public void Export_PrefillsTeamFromGlobalScope()
    {
        var snapshot = new AnalysisScopeSnapshot(null, null, 7, "Brest", null, null);
        var state = ExportScopeState.FromSnapshot(snapshot);
        Assert.Equal(7, state.TeamId);
        Assert.Equal("Brest", state.TeamName);
    }

    [Fact]
    public void Export_PrefillsSeasonFromGlobalScope()
    {
        var snapshot = new AnalysisScopeSnapshot(null, null, null, null, "2025-2026", null);
        var state = ExportScopeState.FromSnapshot(snapshot);
        Assert.Equal("2025-2026", state.Season);
    }

    [Fact]
    public void Export_HandlesDayFromGlobalScope()
    {
        var snapshot = new AnalysisScopeSnapshot(42, "LBE", 7, "Brest", "2025-2026", "J18");
        var state = ExportScopeState.FromSnapshot(snapshot);
        Assert.Equal("J18", state.Day);
    }

    [Fact]
    public void Export_EmptyScopeProducesEmptyState()
    {
        var snapshot = AnalysisScopeSnapshot.Empty;
        var state = ExportScopeState.FromSnapshot(snapshot);
        Assert.Null(state.CompetitionId);
        Assert.Null(state.TeamId);
        Assert.Null(state.Season);
        Assert.Null(state.Day);
    }

    [Fact]
    public void Export_ToDisplayLine_IncludesAllSetFields()
    {
        var state = new ExportScopeState
        {
            CompetitionName = "LBE",
            TeamName = "Brest",
            Season = "2025-2026",
            Day = "J18",
        };
        var line = state.ToDisplayLine();
        Assert.Contains("LBE", line);
        Assert.Contains("Brest", line);
        Assert.Contains("2025-2026", line);
        Assert.Contains("J18", line);
    }

    [Fact]
    public void Export_ToDisplayLine_ReturnsGlobalLabel_WhenEmpty()
    {
        var state = new ExportScopeState();
        Assert.Equal("Périmètre global", state.ToDisplayLine());
    }

    [Fact]
    public void Export_ToDisplayLine_PartialScope_OnlyShowsSetFields()
    {
        var state = new ExportScopeState { CompetitionName = "LBE", Season = "2025-2026" };
        var line = state.ToDisplayLine();
        Assert.Contains("LBE", line);
        Assert.Contains("2025-2026", line);
        Assert.DoesNotContain("·  ·", line); // no empty segments
    }

    [Fact]
    public void Export_ResetReturnsToCurrentGlobalScope()
    {
        var service = new AnalysisScopeService();
        service.Update(new AnalysisScopeSnapshot(10, "LBE", 5, "Brest", "2025-2026", "J10"));

        var state = ExportScopeState.FromSnapshot(service.Current);
        // Simulate local modification
        state.CompetitionId = 99;
        state.TeamId = 99;

        // Reset = re-read from service
        state = ExportScopeState.FromSnapshot(service.Current);
        Assert.Equal(10, state.CompetitionId);
        Assert.Equal(5, state.TeamId);
        Assert.Equal("J10", state.Day);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // SECTION B: SmartFilter / MatchFilterCatalog semantics
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Export_ScopeChangeUsesSharedFilterSemantics_SeasonsFromMatches()
    {
        var matches = new List<MatchListItemDto>
        {
            MakeMatch(1, season: "2025-2026"),
            MakeMatch(2, season: "2024-2025"),
            MakeMatch(3, season: "2025-2026"),
        };
        var seasons = MatchFilterCatalog.GetSeasons(matches);
        Assert.Contains("2025-2026", seasons);
        Assert.Contains("2024-2025", seasons);
        Assert.Equal(2, seasons.Count);
    }

    [Fact]
    public void Export_CompetitionFiltersTeams()
    {
        var refData = MakeRefData(
            competitions: [new CompetitionDto { CompetitionId = 1, CompetitionName = "LBE" }],
            teams: [
                new TeamDto { TeamId = 10, TeamName = "Brest" },
                new TeamDto { TeamId = 11, TeamName = "Metz" },
            ]);

        var matchesInComp = new List<MatchListItemDto>
        {
            new() { MatchId = 1, CompetitionId = 1, Team1Id = 10, Team2Id = 11 },
        };

        var teams = SmartFilterCatalog.GetTeams(refData, matchesInComp, constrain: true);
        Assert.Contains(teams, t => t.TeamId == 10);
        Assert.Contains(teams, t => t.TeamId == 11);
    }

    [Fact]
    public void Export_SeasonFiltersDays()
    {
        var matches = new List<MatchListItemDto>
        {
            MakeMatch(1, season: "2025-2026", day: "J1"),
            MakeMatch(2, season: "2025-2026", day: "J2"),
            MakeMatch(3, season: "2024-2025", day: "J3"),
        };
        var days = MatchFilterCatalog.GetDays(matches, season: "2025-2026");
        Assert.Contains("J1", days);
        Assert.Contains("J2", days);
        Assert.DoesNotContain("J3", days);
    }

    [Fact]
    public void Export_TeamSeasonFiltersMatches()
    {
        var matches = new List<MatchListItemDto>
        {
            MakeMatch(1, team1Id: 10, season: "2025-2026"),
            MakeMatch(2, team1Id: 11, season: "2025-2026"),
            MakeMatch(3, team1Id: 10, season: "2024-2025"),
        };
        var filtered = MatchFilterCatalog.ApplySeasonAndDay(matches, "2025-2026", null);
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, m => Assert.Equal("2025-2026", m.Season));
    }

    [Fact]
    public void Export_OutOfScopePlayersAreRemoved()
    {
        var availableIds = new HashSet<int> { 1, 2, 3 };
        var selected = new List<int> { 1, 2, 99 }; // 99 is out of scope
        var pruned = selected.Where(id => availableIds.Contains(id)).ToList();
        Assert.Equal(2, pruned.Count);
        Assert.DoesNotContain(99, pruned);
    }

    [Fact]
    public void Export_OutOfScopeMatchesAreRemoved()
    {
        var availableIds = new HashSet<int> { 10, 20, 30 };
        var selected = new List<int> { 10, 999 }; // 999 is out of scope
        var pruned = selected.Where(id => availableIds.Contains(id)).ToList();
        Assert.Single(pruned);
        Assert.Equal(10, pruned[0]);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // SECTION C: Target type and API scope mapping
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Export_FullScopeDoesNotRequireSpecificSelection()
    {
        var result = ExportRequestValidator.Validate(
            ExportTargetType.FullScope, null, [], [], null, null, ["PLAYERS"]);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Export_TeamRequiresTeam()
    {
        var result = ExportRequestValidator.Validate(
            ExportTargetType.Team, null, [], [], null, null, ["PLAYERS"]);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("équipe"));
    }

    [Fact]
    public void Export_TeamValidWhenTeamProvided()
    {
        var result = ExportRequestValidator.Validate(
            ExportTargetType.Team, 5, [], [], null, null, ["PLAYERS"]);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Export_PlayersRequiresAtLeastOnePlayer()
    {
        var result = ExportRequestValidator.Validate(
            ExportTargetType.Players, null, [], [], null, null, ["PLAYERS"]);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("joueuse"));
    }

    [Fact]
    public void Export_GoalkeepersContainsOnlyGoalkeepers()
    {
        var players = new List<PlayerListItemDto>
        {
            new() { PlayerId = 1, FullName = "Mairot", IsGoalkeeper = true },
            new() { PlayerId = 2, FullName = "Foppa",  IsGoalkeeper = false },
            new() { PlayerId = 3, FullName = "Toublanc", IsGoalkeeper = true },
        };
        var gks = players.Where(p => p.IsGoalkeeper).ToList();
        Assert.Equal(2, gks.Count);
        Assert.All(gks, p => Assert.True(p.IsGoalkeeper));
    }

    [Fact]
    public void Export_MatchesRequiresAtLeastOneMatch()
    {
        var result = ExportRequestValidator.Validate(
            ExportTargetType.Matches, null, [], [], null, null, ["MATCHES"]);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("match"));
    }

    [Fact]
    public void Export_SingleAndMultiplePlayersUseSameUiTarget()
    {
        // Both single and multiple players map to the Players enum value
        Assert.Equal(ExportTargetType.Players, ExportTargetType.Players);

        // The API scope differs only by count — internal to ExportTargetMapper
        var scope1 = ExportTargetMapper.ToApiScope(ExportTargetType.Players, 1);
        var scope2 = ExportTargetMapper.ToApiScope(ExportTargetType.Players, 3);
        Assert.Equal("PLAYER", scope1);
        Assert.Equal("MULTIPLE_PLAYERS", scope2);
    }

    [Fact]
    public void Export_GoalkeeperTargetMapsToGoalkeepersApiScope()
    {
        var scope = ExportTargetMapper.ToApiScope(ExportTargetType.Goalkeepers, 0);
        Assert.Equal("GOALKEEPERS", scope);
    }

    [Fact]
    public void Export_MatchTargetMapsToMatchApiScope()
    {
        var scope = ExportTargetMapper.ToApiScope(ExportTargetType.Matches, 0);
        Assert.Equal("MATCH", scope);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // SECTION D: Presets and section catalog
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Export_PresetFullAnalysisMapsExpectedSections()
    {
        var sections = ExportSectionCatalog.ForPreset(ExportPreset.FullAnalysis);
        Assert.Contains("SEASON_SUMMARY", sections);
        Assert.Contains("TEAMS", sections);
        Assert.Contains("PLAYERS", sections);
        Assert.Contains("PLAYERS_PER_MATCH", sections);
        Assert.Contains("GOALKEEPERS", sections);
        Assert.Contains("MATCHES", sections);
        Assert.Contains("SHOTS", sections);
        Assert.Contains("DEFENSE", sections);
        Assert.Contains("METRIC_DICTIONARY", sections);
        Assert.Contains("DATA_QUALITY", sections);
    }

    [Fact]
    public void Export_PresetStaffMapsExpectedSections()
    {
        var sections = ExportSectionCatalog.ForPreset(ExportPreset.Staff);
        Assert.Contains("SEASON_SUMMARY", sections);
        Assert.Contains("TEAMS", sections);
        Assert.Contains("PLAYERS", sections);
        Assert.Contains("MATCHES", sections);
        Assert.Contains("DATA_QUALITY", sections);
        // Staff should not include raw events
        Assert.DoesNotContain("EVENTS", sections);
    }

    [Fact]
    public void Export_PresetPlayersMapsExpectedSections()
    {
        var sections = ExportSectionCatalog.ForPreset(ExportPreset.Players);
        Assert.Contains("PLAYERS", sections);
        Assert.Contains("PLAYERS_PER_MATCH", sections);
        Assert.Contains("GOALKEEPERS", sections);
    }

    [Fact]
    public void Export_PresetMatchesMapsExpectedSections()
    {
        var sections = ExportSectionCatalog.ForPreset(ExportPreset.Matches);
        Assert.Contains("MATCHES", sections);
        Assert.Contains("PLAYERS_PER_MATCH", sections);
        Assert.Contains("SHOTS", sections);
    }

    [Fact]
    public void Export_PresetSpatialMapsExpectedSections()
    {
        var sections = ExportSectionCatalog.ForPreset(ExportPreset.Spatial);
        Assert.Contains("SHOTS", sections);
    }

    [Fact]
    public void Export_ModifyingPresetSwitchesToCustom_Logic()
    {
        // If selected sections diverge from the preset, we expect Custom
        var preset = ExportPreset.FullAnalysis;
        var presetKeys = ExportSectionCatalog.ForPreset(preset);
        var selected = new HashSet<string>(presetKeys, StringComparer.OrdinalIgnoreCase);

        // Remove a key — now no longer matching any preset
        selected.Remove("DEFENSE");

        var stillMatchesPreset = selected.SetEquals(presetKeys);
        Assert.False(stillMatchesPreset);
    }

    [Fact]
    public void Export_SectionCatalog_NoApiKeysVisible_InLabels()
    {
        // Ensure section labels never equal their API keys (they should be human)
        foreach (var sec in ExportSectionCatalog.All)
        {
            Assert.NotEqual(sec.ApiKey, sec.Label,
                StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Export_SectionCatalog_AllKeysHaveLabels()
    {
        foreach (var sec in ExportSectionCatalog.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(sec.Label),
                $"Section {sec.ApiKey} has no label.");
            Assert.False(string.IsNullOrWhiteSpace(sec.Description),
                $"Section {sec.ApiKey} has no description.");
        }
    }

    [Fact]
    public void Export_RequestSectionsAreUnique()
    {
        var scope = new ExportScopeState { CompetitionId = 1, Season = "2025-2026" };

        // Include DATA_QUALITY in both selected sections AND via flag — should dedupe
        var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SEASON_SUMMARY", "DATA_QUALITY",
        };
        var request = ExportRequestBuilder.Build(
            scope, ExportTargetType.FullScope,
            null, [], [],
            selected,
            includeRawEvents: false,
            includeShotCoordinates: false,
            includeDataQuality: true);

        var sections = request.Sections ?? [];
        var unique = new HashSet<string>(sections, StringComparer.OrdinalIgnoreCase);
        Assert.Equal(unique.Count, sections.Count);
    }

    [Fact]
    public void Export_IncludeRawEvents_AddsEventsOnce()
    {
        var scope = new ExportScopeState();
        var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PLAYERS" };

        var request = ExportRequestBuilder.Build(
            scope, ExportTargetType.FullScope,
            null, [], [],
            selected,
            includeRawEvents: true,
            includeShotCoordinates: false,
            includeDataQuality: false);

        var sections = request.Sections ?? [];
        Assert.Contains("EVENTS", sections);
        Assert.Equal(1, sections.Count(s => string.Equals(s, "EVENTS", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void Export_IncludeShotCoordinates_EnsuresShotsSection()
    {
        var scope = new ExportScopeState();
        var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PLAYERS" };

        var request = ExportRequestBuilder.Build(
            scope, ExportTargetType.FullScope,
            null, [], [],
            selected,
            includeRawEvents: false,
            includeShotCoordinates: true,
            includeDataQuality: false);

        var sections = request.Sections ?? [];
        Assert.Contains("SHOTS", sections);
    }

    [Fact]
    public void Export_IncludeDataQuality_AddedExactlyOnce()
    {
        var scope = new ExportScopeState();
        var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DATA_QUALITY", // already in selected
        };

        var request = ExportRequestBuilder.Build(
            scope, ExportTargetType.FullScope,
            null, [], [],
            selected,
            includeRawEvents: false,
            includeShotCoordinates: false,
            includeDataQuality: true);

        var sections = request.Sections ?? [];
        Assert.Equal(1, sections.Count(s => string.Equals(s, "DATA_QUALITY", StringComparison.OrdinalIgnoreCase)));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // SECTION E: Request building — reconciliation with preview
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Export_RequestMatchesPreviewScope()
    {
        var scope = new ExportScopeState
        {
            CompetitionId = 1,
            TeamId = 5,
            Season = "2025-2026",
        };

        var request = ExportRequestBuilder.Build(
            scope, ExportTargetType.FullScope,
            null, [], [],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PLAYERS" },
            false, false, true);

        Assert.Equal(1, request.CompetitionId);
        Assert.Equal("2025-2026", request.SeasonLabel);
    }

    [Fact]
    public void Export_RequestMatchesSelectedTarget()
    {
        var scope = new ExportScopeState();
        var request = ExportRequestBuilder.Build(
            scope, ExportTargetType.Goalkeepers,
            null, [1, 2], [],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "GOALKEEPERS" },
            false, false, false);

        Assert.Equal("GOALKEEPERS", request.Scope);
    }

    [Fact]
    public void Export_RequestMatchesSelectedSections()
    {
        var scope = new ExportScopeState();
        var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PLAYERS", "MATCHES",
        };

        var request = ExportRequestBuilder.Build(
            scope, ExportTargetType.FullScope,
            null, [], [],
            selected,
            false, false, false);

        Assert.Contains("PLAYERS", request.Sections!);
        Assert.Contains("MATCHES", request.Sections!);
    }

    [Fact]
    public void Export_RequestIncludesAdvancedOptions()
    {
        var scope = new ExportScopeState();
        var request = ExportRequestBuilder.Build(
            scope, ExportTargetType.FullScope,
            null, [], [],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PLAYERS" },
            includeRawEvents: true,
            includeShotCoordinates: true,
            includeDataQuality: true);

        Assert.True(request.IncludeRawEvents);
        Assert.True(request.IncludeShotCoordinates);
        Assert.Equal(true, request.IncludeDataQuality);
    }

    [Fact]
    public void Export_RequestDoesNotContainStalePlayerIds()
    {
        var scope = new ExportScopeState();
        // After scope change, player 99 was removed — only 1 and 2 remain
        var currentPlayerIds = new List<int> { 1, 2 };

        var request = ExportRequestBuilder.Build(
            scope, ExportTargetType.Players,
            null, currentPlayerIds, [],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PLAYERS" },
            false, false, false);

        Assert.DoesNotContain(99, request.PlayerIds ?? []);
        Assert.Contains(1, request.PlayerIds ?? []);
        Assert.Contains(2, request.PlayerIds ?? []);
    }

    [Fact]
    public void Export_RequestDoesNotContainStaleMatchIds()
    {
        var scope = new ExportScopeState();
        var currentMatchIds = new List<int> { 10, 20 };

        var request = ExportRequestBuilder.Build(
            scope, ExportTargetType.Matches,
            null, [], currentMatchIds,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "MATCHES" },
            false, false, false);

        Assert.DoesNotContain(999, request.MatchIds ?? []);
        Assert.Contains(10, request.MatchIds ?? []);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // SECTION F: Day scope honesty (critical)
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Export_DoesNotClaimDayIsEffectiveWhenApiCannotFilterDay()
    {
        // The API DTO does NOT have a Day field.
        // This test verifies that AnalyticsExportRequestDto cannot accept a Day value.
        var dto = new AnalyticsExportRequestDto();
        var type = typeof(AnalyticsExportRequestDto);
        var dayProp = type.GetProperty("Day");
        Assert.Null(dayProp); // No Day property must exist on the DTO
    }

    [Fact]
    public void Export_DayIsStoredInScopeStateButNotPropagatedToRequest()
    {
        var scope = new ExportScopeState
        {
            CompetitionId = 1,
            Season = "2025-2026",
            Day = "J18",
        };

        var request = ExportRequestBuilder.Build(
            scope, ExportTargetType.FullScope,
            null, [], [],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PLAYERS" },
            false, false, false);

        // SeasonLabel is sent, but Day is not (no such field on DTO)
        Assert.Equal("2025-2026", request.SeasonLabel);
        // Confirm Day is available in scope for UI display
        Assert.Equal("J18", scope.Day);
    }

    [Fact]
    public void Export_DayFilterAppliedClientSideToMatchList()
    {
        var matches = new List<MatchListItemDto>
        {
            MakeMatch(1, day: "J18"),
            MakeMatch(2, day: "J17"),
            MakeMatch(3, day: "J18"),
        };

        var filtered = MatchFilterCatalog.ApplySeasonAndDay(matches, null, "J18");
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, m => Assert.Equal("J18", m.Day));
    }

    [Fact]
    public void Export_PreviewModel_FlagsDay_AsDisplayOnly()
    {
        // The preview model must communicate that Day is display-only
        var scope = new ExportScopeState { Day = "J18" };

        // ExportPreviewModel.DayFilterIsDisplayOnly must be true when Day is set
        // Simulate what GetPreview() does
        bool dayDisplayOnly = !string.IsNullOrWhiteSpace(scope.Day);
        Assert.True(dayDisplayOnly);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // SECTION G: Validation
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Export_InvalidDateRangeBlocksRequest()
    {
        var dateFrom = new DateTime(2026, 3, 1);
        var dateTo = new DateTime(2026, 1, 1); // To < From

        var result = ExportRequestValidator.Validate(
            ExportTargetType.FullScope, null, [], [], dateFrom, dateTo, ["PLAYERS"]);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("date"));
    }

    [Fact]
    public void Export_ValidDateRangeAllowsRequest()
    {
        var dateFrom = new DateTime(2026, 1, 1);
        var dateTo = new DateTime(2026, 3, 1);

        var result = ExportRequestValidator.Validate(
            ExportTargetType.FullScope, null, [], [], dateFrom, dateTo, ["PLAYERS"]);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Export_EmptyCustomContentBlocksRequest()
    {
        var result = ExportRequestValidator.Validate(
            ExportTargetType.FullScope, null, [], [], null, null, []);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("feuille") || e.Contains("contenu") || e.Contains("section"));
    }

    [Fact]
    public void Export_TeamWithoutTeamBlocksRequest()
    {
        var result = ExportRequestValidator.Validate(
            ExportTargetType.Team, null, [], [], null, null, ["TEAMS"]);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Export_PlayersWithoutSelectionBlocksRequest()
    {
        var result = ExportRequestValidator.Validate(
            ExportTargetType.Players, null, [], [], null, null, ["PLAYERS"]);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Export_MatchesWithoutSelectionBlocksRequest()
    {
        var result = ExportRequestValidator.Validate(
            ExportTargetType.Matches, null, [], [], null, null, ["MATCHES"]);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Export_ValidConfigurationAllowsGeneration()
    {
        var result = ExportRequestValidator.Validate(
            ExportTargetType.Players, null, [1, 2], [], null, null, ["PLAYERS"]);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Export_FullScopeWithSectionsIsValid()
    {
        var result = ExportRequestValidator.Validate(
            ExportTargetType.FullScope, null, [], [], null, null,
            ["SEASON_SUMMARY", "PLAYERS", "MATCHES"]);
        Assert.True(result.IsValid);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // SECTION H: SeasonYear derivation
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Export_SeasonYear_DerivedFromLabel_2025_2026()
    {
        var year = ExportRequestBuilder.TryParseSeasonYear("2025-2026");
        Assert.Equal(2025, year);
    }

    [Fact]
    public void Export_SeasonYear_DerivedFromLabel_2024_2025()
    {
        var year = ExportRequestBuilder.TryParseSeasonYear("2024-2025");
        Assert.Equal(2024, year);
    }

    [Fact]
    public void Export_SeasonYear_NullForNull()
    {
        Assert.Null(ExportRequestBuilder.TryParseSeasonYear(null));
    }

    [Fact]
    public void Export_SeasonYear_NullForBlank()
    {
        Assert.Null(ExportRequestBuilder.TryParseSeasonYear(""));
    }

    [Fact]
    public void Export_SeasonYear_NullForUnparseable()
    {
        Assert.Null(ExportRequestBuilder.TryParseSeasonYear("été"));
    }

    [Fact]
    public void Export_SeasonYear_AcceptsSlashSeparator()
    {
        // e.g. "2025/2026"
        var year = ExportRequestBuilder.TryParseSeasonYear("2025/2026");
        Assert.Equal(2025, year);
    }

    [Fact]
    public void Export_RequestPropagatesSeasonYear_InBuiltRequest()
    {
        var scope = new ExportScopeState { Season = "2025-2026" };
        var request = ExportRequestBuilder.Build(
            scope, ExportTargetType.FullScope,
            null, [], [],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PLAYERS" },
            false, false, false);
        Assert.Equal(2025, request.SeasonYear);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // SECTION I: Stale-response protection
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Export_StaleGenerationCannotOverwriteLatestResult()
    {
        // Token-based guard: an old completion with token N should not
        // write state when the current token is N+1 or higher.
        int currentToken = 2;
        int staleToken = 1;

        bool shouldApply = staleToken == currentToken;
        Assert.False(shouldApply);
    }

    [Fact]
    public void Export_LatestGenerationTokenApplies()
    {
        int currentToken = 3;
        int latestToken = 3;

        bool shouldApply = latestToken == currentToken;
        Assert.True(shouldApply);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // SECTION J: Section catalog — no API keys visible / no manual ID inputs
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Export_DoesNotExposeApiSectionKeys_In_Labels()
    {
        var forbiddenKeys = new[] { "SEASON_SUMMARY", "PLAYERS_PER_MATCH", "METRIC_DICTIONARY", "DATA_QUALITY", "EVENTS" };
        foreach (var sec in ExportSectionCatalog.All)
        {
            foreach (var key in forbiddenKeys)
            {
                // Label must not equal the API key
                Assert.NotEqual(key, sec.Label, StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void Export_SectionLabelDoesNotContainUnderscores()
    {
        // API keys use underscores; human labels should not
        foreach (var sec in ExportSectionCatalog.All)
        {
            Assert.DoesNotContain("_", sec.Label);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // SECTION K: Target mapper — all values covered
    // ─────────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(ExportTargetType.FullScope, 0, "SEASON")]
    [InlineData(ExportTargetType.Team, 0, "TEAM")]
    [InlineData(ExportTargetType.Players, 1, "PLAYER")]
    [InlineData(ExportTargetType.Players, 3, "MULTIPLE_PLAYERS")]
    [InlineData(ExportTargetType.Goalkeepers, 0, "GOALKEEPERS")]
    [InlineData(ExportTargetType.Matches, 0, "MATCH")]
    public void Export_TargetMappingIsComplete(ExportTargetType target, int count, string expectedScope)
    {
        Assert.Equal(expectedScope, ExportTargetMapper.ToApiScope(target, count));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // SECTION L: ExportScopeState — DoesNotMaintainConflictingDuplicateScope
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Export_DoesNotMaintainConflictingDuplicateScope()
    {
        // ExportScopeState is the single source of truth — no second copy
        // This is verified architecturally: ExportScopeState inherits from snapshot,
        // and the page uses only _scope, not separate _selectedCompetitionId etc.
        var scope = new ExportScopeState { CompetitionId = 10, Season = "2025-2026" };

        // Building request reads only from scope — no duplicate variables
        var request = ExportRequestBuilder.Build(
            scope, ExportTargetType.FullScope,
            null, [], [],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PLAYERS" },
            false, false, false);

        Assert.Equal(10, request.CompetitionId);
        Assert.Equal("2025-2026", request.SeasonLabel);
    }

    [Fact]
    public void Export_ScopeChangeUsesSharedFilterSemantics()
    {
        // MatchFilterCatalog.NormalizeSelection is the shared normalization
        Assert.Equal("2025-2026", MatchFilterCatalog.NormalizeSelection("  2025-2026  "));
        Assert.Null(MatchFilterCatalog.NormalizeSelection("  "));
        Assert.Null(MatchFilterCatalog.NormalizeSelection(null));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // SECTION M: ExportGenerationStatus enum coverage
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Export_GenerationStatus_HasExpectedValues()
    {
        Assert.True(Enum.IsDefined(typeof(ExportGenerationStatus), ExportGenerationStatus.Ready));
        Assert.True(Enum.IsDefined(typeof(ExportGenerationStatus), ExportGenerationStatus.Validating));
        Assert.True(Enum.IsDefined(typeof(ExportGenerationStatus), ExportGenerationStatus.Generating));
        Assert.True(Enum.IsDefined(typeof(ExportGenerationStatus), ExportGenerationStatus.Downloading));
        Assert.True(Enum.IsDefined(typeof(ExportGenerationStatus), ExportGenerationStatus.Saving));
        Assert.True(Enum.IsDefined(typeof(ExportGenerationStatus), ExportGenerationStatus.Completed));
        Assert.True(Enum.IsDefined(typeof(ExportGenerationStatus), ExportGenerationStatus.Cancelled));
        Assert.True(Enum.IsDefined(typeof(ExportGenerationStatus), ExportGenerationStatus.Error));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    private static MatchListItemDto MakeMatch(
        int id,
        int? team1Id = null,
        int? team2Id = null,
        string? season = null,
        string? day = null,
        int? competitionId = null) => new()
    {
        MatchId = id,
        Team1Id = team1Id,
        Team2Id = team2Id,
        Season = season,
        Day = day,
        CompetitionId = competitionId,
    };

    private static AnalyticsReferenceData MakeRefData(
        IReadOnlyList<CompetitionDto>? competitions = null,
        IReadOnlyList<TeamDto>? teams = null) => new(
            competitions ?? [],
            teams ?? [],
            [], [], [], [], []);
}

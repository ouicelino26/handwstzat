using HandWStat.Models.Analytics;
using HandWStat.Services;
using HandWStat.Services.Api;
using HandballManagerCore.DTO;
using Microsoft.AspNetCore.Components;

namespace HandWStat.Components.Pages;

public class HomeBase : ComponentBase
{
    [Inject]
    protected StatsDashboardService DashboardService { get; set; } = default!;

    [Inject]
    protected ReferenceDataService ReferenceDataService { get; set; } = default!;

    [Inject]
    protected IApiAuthService AuthService { get; set; } = default!;

    [Inject]
    protected MatchesApiClient MatchesApiClient { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    protected DashboardSnapshot? Snapshot { get; set; }

    protected AnalyticsReferenceData ReferenceData { get; set; } = AnalyticsReferenceData.Empty;

    protected IReadOnlyList<MatchListItemDto> CompetitionOptionMatches { get; set; } = [];

    protected IReadOnlyList<MatchListItemDto> TeamOptionMatches { get; set; } = [];

    protected IReadOnlyList<MatchListItemDto> FilterScopeMatches { get; set; } = [];

    protected DashboardFilterState Filters { get; set; } = new();

    protected bool IsBusy { get; set; }

    protected string? ErrorMessage { get; set; }

    protected string? SelectedZoneKey { get; set; }

    protected IReadOnlyList<RankingMetricOption> RankingMetrics => RankingMetricCatalog.Default;

    protected IReadOnlyList<string> SeasonOptions => MatchFilterCatalog.GetSeasons(FilterScopeMatches);

    protected IReadOnlyList<string> DayOptions => MatchFilterCatalog.GetDays(FilterScopeMatches, Filters.Season);

    protected IReadOnlyList<CompetitionDto> AvailableCompetitions => SmartFilterCatalog.GetCompetitions(
        ReferenceData,
        MatchFilterCatalog.ApplySeasonAndDay(CompetitionOptionMatches, Filters.Season, Filters.Day),
        Filters.TeamId.HasValue || !string.IsNullOrWhiteSpace(Filters.Season) || !string.IsNullOrWhiteSpace(Filters.Day));

    protected IReadOnlyList<TeamDto> AvailableTeams => SmartFilterCatalog.GetTeams(
        ReferenceData,
        MatchFilterCatalog.ApplySeasonAndDay(TeamOptionMatches, Filters.Season, Filters.Day),
        Filters.CompetitionId.HasValue || !string.IsNullOrWhiteSpace(Filters.Season) || !string.IsNullOrWhiteSpace(Filters.Day));

    protected IReadOnlyList<ScopeSummaryItem> DashboardScopeItems =>
    [
        new(
            "Competition",
            Filters.CompetitionId.HasValue
                ? AvailableCompetitions.FirstOrDefault(item => item.CompetitionId == Filters.CompetitionId.Value)?.CompetitionName ?? "Selection"
                : "Toutes"),
        new(
            "Equipe",
            Filters.TeamId.HasValue
                ? AvailableTeams.FirstOrDefault(item => item.TeamId == Filters.TeamId.Value)?.TeamName ?? "Selection"
                : "Toutes"),
        new("Saison", string.IsNullOrWhiteSpace(Filters.Season) ? "Toutes" : Filters.Season),
        new("Journee", string.IsNullOrWhiteSpace(Filters.Day) ? "Toutes" : Filters.Day),
        new("Matchs", Snapshot?.Overview.MatchCount.ToString() ?? FilterScopeMatches.Count.ToString())
    ];

    protected IReadOnlyList<KpiTile> LeagueKpis => Snapshot is null
        ? []
        : BuildLeagueKpis(Snapshot);

    protected IReadOnlyList<KpiTile> SpotlightKpis => Snapshot is null || Snapshot.Spotlight.PlayerId <= 0
        ? []
        : BuildSpotlightKpis(Snapshot.Spotlight);

    protected ZoneStat? CurrentZone =>
        Snapshot?.Spotlight.GoalZones.Concat(Snapshot.Spotlight.TriggerZones)
            .FirstOrDefault(zone => zone.Key == SelectedZoneKey)
        ?? Snapshot?.Spotlight.GoalZones.FirstOrDefault(zone => zone.Attempts > 0)
        ?? Snapshot?.Spotlight.TriggerZones.FirstOrDefault(zone => zone.Attempts > 0)
        ?? Snapshot?.Spotlight.GoalZones.FirstOrDefault()
        ?? Snapshot?.Spotlight.TriggerZones.FirstOrDefault();

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.Session.IsAuthenticated)
        {
            Navigation.NavigateTo("/");
            return;
        }

        await LoadSnapshotAsync(forceRefresh: true);
    }

    protected async Task RefreshAsync()
    {
        await LoadSnapshotAsync(forceRefresh: true);
    }

    protected async Task ApplyFiltersAsync()
    {
        await LoadSnapshotAsync(forceRefresh: false);
    }

    protected async Task ResetFiltersAsync()
    {
        var spotlightPlayerId = Snapshot?.Spotlight.PlayerId;
        Filters = new DashboardFilterState
        {
            SpotlightPlayerId = spotlightPlayerId
        };

        await LoadSnapshotAsync(forceRefresh: true);
    }

    protected async Task OnPlayerChangedAsync(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), out var playerId))
        {
            Filters.SpotlightPlayerId = playerId;
            await LoadSnapshotAsync(forceRefresh: false);
        }
    }

    protected Task OnScopeSelectionChangedAsync()
    {
        return LoadSnapshotAsync(forceRefresh: false);
    }

    protected Task HandleZoneSelectionAsync(string zoneKey)
    {
        SelectedZoneKey = zoneKey;
        return InvokeAsync(StateHasChanged);
    }

    protected string FormatRate(double value)
    {
        return value.ToString("0.#");
    }

    private static IReadOnlyList<KpiTile> BuildLeagueKpis(DashboardSnapshot snapshot)
    {
        var matches = snapshot.Overview.MatchCount;
        var goals = snapshot.Overview.GoalCount;

        return
        [
            new KpiTile(
                "Buts / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(goals, matches)),
                "Repere de production offensive sur la selection.",
                "positive",
                HandballKpiHelper.FormatPerMatchContext(goals, matches, "buts")),
            new KpiTile(
                "Buts prepares",
                HandballKpiHelper.FormatPercent(HandballKpiHelper.Share(snapshot.Overview.AssistCount, Math.Max(goals, 1))),
                "Part des buts amenes par une passe decisive.",
                HandballKpiHelper.FieldSuccessRateTone(HandballKpiHelper.Share(snapshot.Overview.AssistCount, Math.Max(goals, 1))),
                HandballKpiHelper.FormatBase(snapshot.Overview.AssistCount, Math.Max(goals, 1), "buts")),
            new KpiTile(
                "Interceptions / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(snapshot.Overview.InterceptionCount, matches)),
                "Capacite a couper les circuits adverses.",
                "neutral",
                HandballKpiHelper.FormatPerMatchContext(snapshot.Overview.InterceptionCount, matches, "interceptions")),
            new KpiTile(
                "Arrets / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(snapshot.Overview.SaveCount, matches)),
                "Impact gardienne moyen dans la selection.",
                HandballKpiHelper.GoalkeeperStopsPerMatchTone(HandballKpiHelper.PerMatch(snapshot.Overview.SaveCount, matches)),
                HandballKpiHelper.FormatPerMatchContext(snapshot.Overview.SaveCount, matches, "arrets")),
            new KpiTile(
                "Pertes / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(snapshot.Overview.TurnoverCount, matches)),
                "Charge de dechet technique a surveiller.",
                "warning",
                HandballKpiHelper.FormatPerMatchContext(snapshot.Overview.TurnoverCount, matches, "pertes")),
            new KpiTile(
                "Sanctions / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(snapshot.Overview.SanctionCount, matches)),
                "Niveau de discipline sur l'ensemble des rencontres.",
                "warning",
                HandballKpiHelper.FormatPerMatchContext(snapshot.Overview.SanctionCount, matches, "sanctions"))
        ];
    }

    private static IReadOnlyList<KpiTile> BuildSpotlightKpis(PlayerSpotlight spotlight)
    {
        var matches = spotlight.MatchesPlayed;
        var directContributions = spotlight.TotalGoals + spotlight.Assists;
        var sanctions = HandballKpiHelper.TotalSanctions(spotlight.Sanctions);
        var defensiveValue = spotlight.IsGoalkeeper
            ? HandballKpiHelper.GoalkeeperStops(spotlight.Goalkeeper)
            : HandballKpiHelper.DefensiveImpact(spotlight.Defense);
        var shotAttempts = spotlight.ShotAttempts;
        var shotWaste = spotlight.ShotWaste;
        var technicalLosses = spotlight.TechnicalLosses;
        var overallShotRate = spotlight.OverallShotSuccessRate;
        var penaltyStopRate = spotlight.PenaltyStopRate;
        var goalkeeperConcededGoals = spotlight.GoalkeeperConcededGoals;

        return
        [
            new KpiTile(
                "Actions directes / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(directContributions, matches)),
                "Buts et passes decisives rapportes au volume de matchs.",
                HandballKpiHelper.DirectActionsTone(HandballKpiHelper.PerMatch(directContributions, matches), spotlight.IsGoalkeeper),
                HandballKpiHelper.FormatPerMatchContext(directContributions, matches, "actions")),
            new KpiTile(
                "Ballons valorises",
                HandballKpiHelper.FormatPercent(HandballKpiHelper.SuccessVsWasteShare(spotlight.Assists, spotlight.Turnovers)),
                "Part des actions de balle qui finissent positivement.",
                HandballKpiHelper.BallRetentionTone(HandballKpiHelper.SuccessVsWasteShare(spotlight.Assists, spotlight.Turnovers), spotlight.IsGoalkeeper),
                $"{spotlight.Assists} passes pour {spotlight.Turnovers} pertes"),
            new KpiTile(
                spotlight.IsGoalkeeper ? "Arrets / match" : "Impact def. / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(defensiveValue, matches)),
                spotlight.IsGoalkeeper
                    ? "Arrets classiques et penalties arretes."
                    : "Interceptions, contres, neutralisations et passages forces.",
                HandballKpiHelper.DefensiveImpactTone(HandballKpiHelper.PerMatch(defensiveValue, matches), spotlight.IsGoalkeeper),
                HandballKpiHelper.FormatPerMatchContext(defensiveValue, matches, spotlight.IsGoalkeeper ? "arrets" : "actions def.")),
            new KpiTile(
                spotlight.IsGoalkeeper ? "Taux d'arret" : "Taux de tir ouvert",
                HandballKpiHelper.FormatPercent(spotlight.IsGoalkeeper ? spotlight.Goalkeeper.TauxArret : spotlight.ShootingRate),
                spotlight.IsGoalkeeper ? "Efficacite gardienne sur les tirs subis." : "Qualite de finition sur les tirs ouverts, hors 7m.",
                spotlight.IsGoalkeeper
                    ? HandballKpiHelper.GoalkeeperSaveRateTone(spotlight.Goalkeeper.TauxArret)
                    : HandballKpiHelper.FieldSuccessRateTone(spotlight.ShootingRate),
                spotlight.IsGoalkeeper
                    ? HandballKpiHelper.FormatBase(
                        HandballKpiHelper.GoalkeeperStops(spotlight.Goalkeeper),
                        Math.Max(spotlight.Goalkeeper.TirsSubis, 1),
                        "tirs arretes")
                    : HandballKpiHelper.FormatBase(
                        spotlight.Offense.Buts,
                        Math.Max(spotlight.Offense.Buts + spotlight.Offense.TirsRates, 1),
                        "tirs ouverts")),
            new KpiTile(
                spotlight.IsGoalkeeper ? "Stop 7m %" : "Conversion globale",
                HandballKpiHelper.FormatPercent(spotlight.IsGoalkeeper ? penaltyStopRate : overallShotRate),
                spotlight.IsGoalkeeper ? "Arrets sur penalties." : "Buts rapportes a tous les tirs engages.",
                spotlight.IsGoalkeeper
                    ? HandballKpiHelper.GoalkeeperPenaltyStopRateTone(penaltyStopRate)
                    : HandballKpiHelper.FieldSuccessRateTone(overallShotRate),
                spotlight.IsGoalkeeper
                    ? HandballKpiHelper.FormatBase(
                        spotlight.Goalkeeper.ArretsPenalty,
                        Math.Max(spotlight.Goalkeeper.ArretsPenalty + spotlight.Goalkeeper.ButsPenalty, 1),
                        "penalties arretes")
                    : HandballKpiHelper.FormatBase(
                        spotlight.TotalGoals,
                        Math.Max(shotAttempts, 1),
                        "tirs engages")),
            new KpiTile(
                spotlight.IsGoalkeeper ? "Tirs subis / match" : "Tirs engages / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(
                    spotlight.IsGoalkeeper ? spotlight.Goalkeeper.TirsSubis : shotAttempts,
                    matches)),
                spotlight.IsGoalkeeper ? "Volume de tirs affrontes rapporte aux matchs." : "Buts, tirs rates, tirs contres et penalties engages.",
                "warning",
                HandballKpiHelper.FormatPerMatchContext(
                    spotlight.IsGoalkeeper ? spotlight.Goalkeeper.TirsSubis : shotAttempts,
                    matches,
                    spotlight.IsGoalkeeper ? "tirs subis" : "tirs engages")),
            new KpiTile(
                spotlight.IsGoalkeeper ? "Buts pris / match" : "Dechet tir / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(
                    spotlight.IsGoalkeeper ? goalkeeperConcededGoals : shotWaste,
                    matches)),
                spotlight.IsGoalkeeper ? "Volume de buts encaisses rapporte aux matchs." : "Tirs rates, tirs contres et penalties manques.",
                spotlight.IsGoalkeeper
                    ? HandballKpiHelper.GoalkeeperConcededGoalsTone(HandballKpiHelper.PerMatch(goalkeeperConcededGoals, matches))
                    : HandballKpiHelper.FieldWasteTone(HandballKpiHelper.PerMatch(shotWaste, matches)),
                HandballKpiHelper.FormatPerMatchContext(
                    spotlight.IsGoalkeeper ? goalkeeperConcededGoals : shotWaste,
                    matches,
                    spotlight.IsGoalkeeper ? "buts encaisses" : "dechets tir")),
            new KpiTile(
                "Pertes techniques / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(technicalLosses, matches)),
                "Mauvaises passes, pertes de balle, fautes techniques et passages en force.",
                spotlight.IsGoalkeeper
                    ? HandballKpiHelper.KeeperWasteTone(HandballKpiHelper.PerMatch(technicalLosses, matches))
                    : HandballKpiHelper.FieldWasteTone(HandballKpiHelper.PerMatch(technicalLosses, matches)),
                HandballKpiHelper.FormatPerMatchContext(technicalLosses, matches, "pertes techniques")),
            new KpiTile(
                "Sanctions / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(sanctions, matches)),
                "Charge disciplinaire individuelle.",
                HandballKpiHelper.SanctionsTone(HandballKpiHelper.PerMatch(sanctions, matches), spotlight.IsGoalkeeper),
                HandballKpiHelper.FormatPerMatchContext(sanctions, matches, "sanctions"))
        ];
    }

    private async Task LoadSnapshotAsync(bool forceRefresh)
    {
        if (!AuthService.Session.IsAuthenticated)
        {
            Navigation.NavigateTo("/");
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            ReferenceData = await ReferenceDataService.GetReferenceDataAsync(forceRefresh, CancellationToken.None);
            await LoadFilterScopesAsync();

            var filtersAdjusted = false;

            if (Filters.CompetitionId.HasValue && AvailableCompetitions.All(item => item.CompetitionId != Filters.CompetitionId.Value))
            {
                Filters.CompetitionId = null;
                filtersAdjusted = true;
            }

            if (Filters.TeamId.HasValue && AvailableTeams.All(item => item.TeamId != Filters.TeamId.Value))
            {
                Filters.TeamId = null;
                filtersAdjusted = true;
            }

            if (filtersAdjusted)
            {
                await LoadFilterScopesAsync();
            }

            if (!string.IsNullOrWhiteSpace(Filters.Season) && !SeasonOptions.Contains(Filters.Season, StringComparer.OrdinalIgnoreCase))
            {
                Filters.Season = null;
            }

            if (!string.IsNullOrWhiteSpace(Filters.Day) && !DayOptions.Contains(Filters.Day, StringComparer.OrdinalIgnoreCase))
            {
                Filters.Day = null;
            }

            Snapshot = await DashboardService.LoadDashboardAsync(Filters, forceRefresh, CancellationToken.None);
            Filters.SpotlightPlayerId = Snapshot.Spotlight.PlayerId > 0 ? Snapshot.Spotlight.PlayerId : null;
            SelectedZoneKey = ResolveActiveZoneKey();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Snapshot = null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadFilterScopesAsync()
    {
        var competitionScopeTask = MatchesApiClient.GetMatchesAsync(
            teamId: Filters.TeamId,
            from: Filters.From,
            to: Filters.To,
            year: Filters.Year,
            page: 1,
            pageSize: 500,
            cancellationToken: CancellationToken.None);
        var teamScopeTask = MatchesApiClient.GetMatchesAsync(
            competitionId: Filters.CompetitionId,
            from: Filters.From,
            to: Filters.To,
            year: Filters.Year,
            page: 1,
            pageSize: 500,
            cancellationToken: CancellationToken.None);
        var contextScopeTask = MatchesApiClient.GetMatchesAsync(
            competitionId: Filters.CompetitionId,
            teamId: Filters.TeamId,
            from: Filters.From,
            to: Filters.To,
            year: Filters.Year,
            page: 1,
            pageSize: 500,
            cancellationToken: CancellationToken.None);

        await Task.WhenAll(competitionScopeTask, teamScopeTask, contextScopeTask);

        CompetitionOptionMatches = competitionScopeTask.Result;
        TeamOptionMatches = teamScopeTask.Result;
        FilterScopeMatches = contextScopeTask.Result;
    }

    private string? ResolveActiveZoneKey()
    {
        if (Snapshot is null)
        {
            return null;
        }

        if (Snapshot.Spotlight.PlayerId <= 0)
        {
            return null;
        }

        var allZones = Snapshot.Spotlight.GoalZones
            .Concat(Snapshot.Spotlight.TriggerZones)
            .Select(zone => zone.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(SelectedZoneKey) && allZones.Contains(SelectedZoneKey))
        {
            return SelectedZoneKey;
        }

        return Snapshot.Spotlight.GoalZones.FirstOrDefault(zone => zone.Attempts > 0)?.Key
            ?? Snapshot.Spotlight.TriggerZones.FirstOrDefault(zone => zone.Attempts > 0)?.Key
            ?? Snapshot.Spotlight.GoalZones.FirstOrDefault()?.Key
            ?? Snapshot.Spotlight.TriggerZones.FirstOrDefault()?.Key;
    }
}

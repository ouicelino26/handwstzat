using System.Diagnostics;
using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;
using HandWStat.Services;
using HandWStat.Services.Api;
using HandWStat.Components.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace HandWStat.Components.Pages;

public class HomeBase : ComponentBase, IDisposable
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

    [Inject]
    protected AnalysisScopeService ScopeService { get; set; } = default!;

    [Inject]
    protected ILogger<HomeBase> Logger { get; set; } = default!;

    private bool _publishingScope;
    private readonly SemaphoreSlim _dashboardLoadGate = new(1, 1);
    private readonly SemaphoreSlim _teamOfTheDayLoadGate = new(1, 1);
    private CancellationTokenSource? _dashboardLoadCts;
    private CancellationTokenSource? _rankingLoadCts;
    private CancellationTokenSource? _teamOfTheDayLoadCts;

    protected DashboardSnapshot? Snapshot { get; set; }

    protected AnalyticsReferenceData ReferenceData { get; set; } = AnalyticsReferenceData.Empty;

    protected IReadOnlyList<MatchListItemDto> CompetitionOptionMatches { get; set; } = [];

    protected IReadOnlyList<MatchListItemDto> TeamOptionMatches { get; set; } = [];

    protected IReadOnlyList<MatchListItemDto> FilterScopeMatches { get; set; } = [];

    protected DashboardFilterState Filters { get; set; } = new();

    protected bool IsBusy { get; set; }

    protected string? ErrorMessage { get; set; }

    protected string? SelectedZoneKey { get; set; }

    protected DateTimeOffset? DashboardGeneratedAt { get; set; }

    protected bool IsTeamOfTheDayLoaded { get; set; }

    protected bool IsTeamOfTheDayLoading { get; set; }

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

    protected AnalysisScopeDisplayModel DashboardScopeSummary => new(
        Filters.CompetitionId.HasValue
            ? AvailableCompetitions.FirstOrDefault(item => item.CompetitionId == Filters.CompetitionId.Value)?.CompetitionName ?? "Selection"
            : "Toutes les competitions",
        Filters.TeamId.HasValue
            ? AvailableTeams.FirstOrDefault(item => item.TeamId == Filters.TeamId.Value)?.TeamName ?? "Selection"
            : "Toutes les equipes",
        string.IsNullOrWhiteSpace(Filters.Season) ? "Toutes" : Filters.Season,
        string.IsNullOrWhiteSpace(Filters.Day) ? "Toutes" : Filters.Day,
        FormatPeriod(Filters.From, Filters.To),
        Snapshot?.Overview.MatchCount ?? FilterScopeMatches.Count,
        DashboardGeneratedAt);

    protected IReadOnlyList<RateDisplayModel> DashboardHeadlineMetrics => Snapshot is null
        ? []
        :
        [
            RateDisplayModel.FromV1(
                "GOALS_PER_MATCH",
                "Cadence offensive",
                HandballKpiHelper.Ratio(Snapshot.Overview.GoalCount, Snapshot.Overview.MatchCount),
                "buts/match",
                "Buts totaux rapportes au nombre de matchs du scope.",
                Snapshot.Overview.GoalCount,
                Snapshot.Overview.MatchCount,
                minimumSample: 1,
                tone: "positive"),
            RateDisplayModel.FromV1(
                "ASSISTED_GOAL_SHARE",
                "Jeu prepare",
                HandballKpiHelper.Percentage(Snapshot.Overview.AssistCount, Snapshot.Overview.GoalCount),
                "%",
                "Part des buts accompagnes d'une passe decisive. Ce n'est pas un taux de possession.",
                Snapshot.Overview.AssistCount,
                Snapshot.Overview.GoalCount,
                minimumSample: 1,
                tone: "good")
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

        ApplyGlobalScope();
        ScopeService.Changed += HandleGlobalScopeChanged;
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

        PublishGlobalScope();
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
        PublishGlobalScope();
        return LoadSnapshotAsync(forceRefresh: false);
    }

    public void Dispose()
    {
        ScopeService.Changed -= HandleGlobalScopeChanged;
        CancelAndDispose(ref _dashboardLoadCts);
        CancelAndDispose(ref _rankingLoadCts);
        CancelAndDispose(ref _teamOfTheDayLoadCts);
    }

    /// <summary>Called at the beginning of each dashboard reload, before clearing TeamOfTheDay state.</summary>
    protected virtual void OnDashboardRefreshing() { }

    protected CancellationToken BeginRankingLoad()
    {
        var next = new CancellationTokenSource();
        var previous = Interlocked.Exchange(ref _rankingLoadCts, next);
        previous?.Cancel();
        previous?.Dispose();
        return next.Token;
    }

    protected async Task EnsureTeamOfTheDayLoadedAsync()
    {
        if (Snapshot is null || IsTeamOfTheDayLoaded)
        {
            return;
        }

        var next = new CancellationTokenSource();
        var previous = Interlocked.Exchange(ref _teamOfTheDayLoadCts, next);
        previous?.Cancel();
        previous?.Dispose();
        var acquired = false;

        try
        {
            await _teamOfTheDayLoadGate.WaitAsync(next.Token);
            acquired = true;
            IsTeamOfTheDayLoading = true;
            await InvokeAsync(StateHasChanged);

            var teamOfTheDay = await DashboardService.LoadTeamOfTheDayAsync(Filters, next.Token);
            next.Token.ThrowIfCancellationRequested();
            Snapshot = Snapshot with { TeamOfTheDay = teamOfTheDay };
            IsTeamOfTheDayLoaded = true;
        }
        catch (OperationCanceledException) when (next.IsCancellationRequested)
        {
            // A newer scope or navigation superseded this secondary load.
        }
        finally
        {
            if (acquired)
            {
                _teamOfTheDayLoadGate.Release();
            }

            if (ReferenceEquals(Interlocked.CompareExchange(ref _teamOfTheDayLoadCts, null, next), next))
            {
                IsTeamOfTheDayLoading = false;
                await InvokeAsync(StateHasChanged);
            }

            next.Dispose();
        }
    }

    private void HandleGlobalScopeChanged()
    {
        if (_publishingScope)
        {
            return;
        }

        _ = InvokeAsync(async () =>
        {
            ApplyGlobalScope();
            await LoadSnapshotAsync(forceRefresh: false);
        });
    }

    private void ApplyGlobalScope()
    {
        Filters.CompetitionId = ScopeService.Current.CompetitionId;
        Filters.TeamId = ScopeService.Current.TeamId;
        Filters.Season = ScopeService.Current.Season;
        Filters.Day = ScopeService.Current.Day;
    }

    private void PublishGlobalScope()
    {
        _publishingScope = true;
        try
        {
            ScopeService.Update(new AnalysisScopeSnapshot(
                Filters.CompetitionId,
                AvailableCompetitions.FirstOrDefault(item => item.CompetitionId == Filters.CompetitionId)?.CompetitionName,
                Filters.TeamId,
                AvailableTeams.FirstOrDefault(item => item.TeamId == Filters.TeamId)?.TeamName,
                Filters.Season,
                Filters.Day));
        }
        finally
        {
            _publishingScope = false;
        }
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
                "Moyenne global de but par rencontre.",
                "positive",
                HandballKpiHelper.FormatPerMatchContext(goals, matches, "buts")),
            new KpiTile(
                "Buts prepares",
                HandballKpiHelper.FormatPercent(HandballKpiHelper.Share(snapshot.Overview.AssistCount, Math.Max(goals, 1))),
                "Pourcentage de but suivie d'une passe decisive.",
                HandballKpiHelper.FieldSuccessRateTone(HandballKpiHelper.Share(snapshot.Overview.AssistCount, Math.Max(goals, 1))),
                HandballKpiHelper.FormatBase(snapshot.Overview.AssistCount, Math.Max(goals, 1), "buts")),
            new KpiTile(
                "Interceptions / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(snapshot.Overview.InterceptionCount, matches)),
                "Nombre d'interceptions moyenne par rencontre.",
                "neutral",
                HandballKpiHelper.FormatPerMatchContext(snapshot.Overview.InterceptionCount, matches, "interceptions")),
            new KpiTile(
                "Arrets / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(snapshot.Overview.SaveCount, matches)),
                "Nombre d'arrêt moyen par match.",
                HandballKpiHelper.GoalkeeperStopsPerMatchTone(HandballKpiHelper.PerMatch(snapshot.Overview.SaveCount, matches)),
                HandballKpiHelper.FormatPerMatchContext(snapshot.Overview.SaveCount, matches, "arrets")),
            new KpiTile(
                "Pertes / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(snapshot.Overview.TurnoverCount, matches)),
                "Moyenne de pertes de balle par rencontre.",
                "warning",
                HandballKpiHelper.FormatPerMatchContext(snapshot.Overview.TurnoverCount, matches, "pertes")),
            new KpiTile(
                "Sanctions / match",
                HandballKpiHelper.FormatNumber(HandballKpiHelper.PerMatch(snapshot.Overview.SanctionCount, matches)),
                "Nombre moyen de sanctions par rencontre.",
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

        var next = new CancellationTokenSource();
        var previous = Interlocked.Exchange(ref _dashboardLoadCts, next);
        previous?.Cancel();
        CancelAndDispose(ref _teamOfTheDayLoadCts);
        IsTeamOfTheDayLoaded = false;
        OnDashboardRefreshing();
        var loadStartedAt = DateTimeOffset.UtcNow;
        var timestamp = Stopwatch.GetTimestamp();
        var acquired = false;

        try
        {
            await _dashboardLoadGate.WaitAsync(next.Token);
            acquired = true;
            await BusyUiHelper.EnterAsync(() => IsBusy = true, () => InvokeAsync(StateHasChanged));
            ErrorMessage = null;

            ReferenceData = await ReferenceDataService.GetReferenceDataAsync(forceRefresh, next.Token);
            await LoadFilterScopesAsync(next.Token);

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
                await LoadFilterScopesAsync(next.Token);
            }

            if (!string.IsNullOrWhiteSpace(Filters.Season) && !SeasonOptions.Contains(Filters.Season, StringComparer.OrdinalIgnoreCase))
            {
                Filters.Season = null;
            }

            if (!string.IsNullOrWhiteSpace(Filters.Day) && !DayOptions.Contains(Filters.Day, StringComparer.OrdinalIgnoreCase))
            {
                Filters.Day = null;
            }

            Snapshot = await DashboardService.LoadDashboardAsync(Filters, forceRefresh, next.Token);
            next.Token.ThrowIfCancellationRequested();
            DashboardGeneratedAt = DateTimeOffset.UtcNow;
            Filters.SpotlightPlayerId = Snapshot.Spotlight.PlayerId > 0 ? Snapshot.Spotlight.PlayerId : null;
            SelectedZoneKey = ResolveActiveZoneKey();

#if DEBUG
            Logger.LogInformation(
                "Dashboard charge en {ElapsedMilliseconds} ms pour {MatchCount} matchs.",
                Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds,
                Snapshot.Overview.MatchCount);
#endif
        }
        catch (OperationCanceledException) when (next.IsCancellationRequested)
        {
            // Expected when a new filter supersedes this request or the component is disposed.
        }
        catch (ApiRequestException ex)
        {
            ErrorMessage = string.IsNullOrWhiteSpace(ex.CorrelationId)
                ? ex.UserMessage
                : $"{ex.UserMessage} Reference : {ex.CorrelationId}.";
            Snapshot = null;
            Logger.LogWarning(
                "Dashboard API error {TechnicalCode}, status {StatusCode}, correlation {CorrelationId}.",
                ex.TechnicalCode,
                ex.StatusCode,
                ex.CorrelationId);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Le tableau de bord ne peut pas etre charge pour le moment.";
            Snapshot = null;
            Logger.LogError(ex, "Unexpected dashboard loading failure.");
        }
        finally
        {
            if (acquired)
            {
                _dashboardLoadGate.Release();
            }

            if (ReferenceEquals(Interlocked.CompareExchange(ref _dashboardLoadCts, null, next), next))
            {
                await BusyUiHelper.ExitAsync(() => IsBusy = false, () => InvokeAsync(StateHasChanged), loadStartedAt);
            }

            next.Dispose();
        }
    }

    private async Task LoadFilterScopesAsync(CancellationToken cancellationToken)
    {
        var competitionScopeTask = MatchesApiClient.GetAllMatchesAsync(
            teamId: Filters.TeamId,
            from: Filters.From,
            to: Filters.To,
            year: Filters.Year,
            cancellationToken: cancellationToken);
        var teamScopeTask = MatchesApiClient.GetAllMatchesAsync(
            competitionId: Filters.CompetitionId,
            from: Filters.From,
            to: Filters.To,
            year: Filters.Year,
            cancellationToken: cancellationToken);
        var contextScopeTask = MatchesApiClient.GetAllMatchesAsync(
            competitionId: Filters.CompetitionId,
            teamId: Filters.TeamId,
            from: Filters.From,
            to: Filters.To,
            year: Filters.Year,
            cancellationToken: cancellationToken);

        await Task.WhenAll(competitionScopeTask, teamScopeTask, contextScopeTask);

        CompetitionOptionMatches = competitionScopeTask.Result;
        TeamOptionMatches = teamScopeTask.Result;
        FilterScopeMatches = contextScopeTask.Result;
    }

    private static string FormatPeriod(DateTime? from, DateTime? to)
    {
        return (from, to) switch
        {
            ({ } start, { } end) => $"Du {start:dd/MM/yyyy} au {end:dd/MM/yyyy}",
            ({ } start, null) => $"Depuis le {start:dd/MM/yyyy}",
            (null, { } end) => $"Jusqu'au {end:dd/MM/yyyy}",
            _ => "Toutes les dates"
        };
    }

    private static void CancelAndDispose(ref CancellationTokenSource? source)
    {
        var current = Interlocked.Exchange(ref source, null);
        current?.Cancel();
        current?.Dispose();
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

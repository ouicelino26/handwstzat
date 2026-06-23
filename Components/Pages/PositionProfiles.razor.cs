using HandWStat.Components.Shared;
using HandWStat.Models.Analytics;
using HandWStat.Services;
using HandWStat.Services.Api;
using HandballManagerCore.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace HandWStat.Components.Pages;

public class PositionProfilesBase : ComponentBase, IDisposable
{
    [Parameter]
    [SupplyParameterFromQuery(Name = "playerId")]
    public int? PlayerIdFromQuery { get; set; }

    [Inject]
    protected ReferenceDataService ReferenceDataService { get; set; } = default!;

    [Inject]
    protected PlayersApiClient PlayersApiClient { get; set; } = default!;

    [Inject]
    protected MatchesApiClient MatchesApiClient { get; set; } = default!;

    [Inject]
    protected IApiAuthService AuthService { get; set; } = default!;

    [Inject]
    protected IJSRuntime JS { get; set; } = default!;

    [Inject]
    protected AnalysisScopeService ScopeService { get; set; } = default!;

    [Inject]
    protected ILogger<PositionProfilesBase> Logger { get; set; } = default!;

    protected AnalyticsReferenceData ReferenceData { get; set; } = AnalyticsReferenceData.Empty;

    protected IReadOnlyList<MatchListItemDto> CompetitionOptionMatches { get; set; } = [];

    protected IReadOnlyList<MatchListItemDto> TeamOptionMatches { get; set; } = [];

    protected IReadOnlyList<MatchListItemDto> ScopedMatches { get; set; } = [];

    protected IReadOnlyList<PlayerListItemDto> PlayerDirectory { get; set; } = [];

    protected PositionProfileResponseDto? PositionProfile { get; set; }

    protected PositionProfileResponseDto? PositionProfileComparison { get; set; }

    protected IReadOnlyList<PlayerListItemDto> PositionProfileCandidates { get; set; } = [];

    protected IReadOnlyList<PositionProfilePlayerDto> PositionProfileComparisonPlayers { get; set; } = [];

    protected IReadOnlyList<PositionProfilePlayerDto> PositionProfileComparisonPreviewPlayers { get; set; } = [];

    protected IReadOnlyList<PositionProfileAxisViewModel> PositionProfileAxes { get; set; } = [];

    protected IReadOnlyList<PositionProfileAxisViewModel> PositionProfileDetailRows { get; set; } = [];

    protected IReadOnlyList<PositionProfileAxisViewModel> PositionProfileChartRows { get; set; } = [];

    protected IReadOnlyList<PositionProfileAxisViewModel> PositionProfileCoachRows { get; set; } = [];

    protected IReadOnlyList<MetricPlotPoint> PositionProfileChartMedianRadarSeries { get; set; } = [];

    protected IReadOnlyList<ScopeSummaryItem> PositionProfileScopeItems { get; set; } = [];

    protected PositionProfileInsightBundle PositionProfileInsights { get; set; } = PositionProfileInsightBundle.Empty;

    protected PositionProfileScatterBounds ScatterBounds { get; set; } = global::HandWStat.Models.Analytics.PositionProfileScatterBounds.Default;

    protected bool IsBusy { get; set; }

    protected bool IsPositionProfileBusy { get; set; }

    protected string? ErrorMessage { get; set; }

    protected bool IsPlayerListOpen { get; set; } = true;

    protected string Search { get; set; } = string.Empty;

    protected int? CompetitionId { get; set; }

    protected int? TeamId { get; set; }

    protected int? PositionId { get; set; }

    protected int? Year { get; set; }

    protected string? Season { get; set; }

    protected string? Day { get; set; }

    protected int? SelectedPlayerId { get; set; }

    protected List<int?> PositionProfileCompareSelections { get; set; } = [null, null, null];

    protected string PositionProfileCoachTakeaway { get; set; } = string.Empty;

    protected string PositionProfileRadarKey { get; set; } = string.Empty;

    protected string PositionProfileHistogramKey { get; set; } = string.Empty;

    protected string PositionProfileScatterKey { get; set; } = string.Empty;

    protected string PositionProfileComparisonKey { get; set; } = string.Empty;

    private CancellationTokenSource? _searchDebounceCts;
    private bool _publishingScope;
    private bool _preserveDeepLinkedPlayer;

    private static readonly TimeSpan SearchDebounceDelay = TimeSpan.FromMilliseconds(300);

    protected IReadOnlyList<string> SeasonOptions => MatchFilterCatalog.GetSeasons(ScopedMatches);

    protected IReadOnlyList<string> DayOptions => MatchFilterCatalog.GetDays(ScopedMatches, Season);

    protected IReadOnlyList<CompetitionDto> AvailableCompetitions => SmartFilterCatalog.GetCompetitions(
        ReferenceData,
        MatchFilterCatalog.ApplySeasonAndDay(CompetitionOptionMatches, Season, Day),
        TeamId.HasValue || !string.IsNullOrWhiteSpace(Season) || !string.IsNullOrWhiteSpace(Day));

    protected IReadOnlyList<TeamDto> AvailableTeams => SmartFilterCatalog.GetTeams(
        ReferenceData,
        MatchFilterCatalog.ApplySeasonAndDay(TeamOptionMatches, Season, Day),
        CompetitionId.HasValue || !string.IsNullOrWhiteSpace(Season) || !string.IsNullOrWhiteSpace(Day));

    protected IReadOnlyList<LookupItemDto> AvailablePositions => SmartFilterCatalog.GetPositions(
        ReferenceData.Positions,
        PlayerDirectory,
        HasDirectoryCriteria);

    protected bool HasDirectoryCriteria => CompetitionId.HasValue
        || TeamId.HasValue
        || PositionId.HasValue
        || !string.IsNullOrWhiteSpace(Season)
        || !string.IsNullOrWhiteSpace(Day)
        || !string.IsNullOrWhiteSpace(Search);

    protected bool IsCompetitionOnlyDirectoryFilter => CompetitionId.HasValue
        && !TeamId.HasValue
        && !PositionId.HasValue
        && string.IsNullOrWhiteSpace(Season)
        && string.IsNullOrWhiteSpace(Day)
        && string.IsNullOrWhiteSpace(Search);

    protected override async Task OnInitializedAsync()
    {
        ApplyGlobalScope();
        SelectedPlayerId = PlayerIdFromQuery;
        IsPlayerListOpen = !PlayerIdFromQuery.HasValue;
        _preserveDeepLinkedPlayer = PlayerIdFromQuery.HasValue;
        ScopeService.Changed += HandleGlobalScopeChanged;
        await LoadAsync(refreshReferenceData: true);
    }

    protected async Task OnSearchChangedAsync(string search)
    {
        Search = search;
        await DebounceSearchAsync();
    }

    protected Task OnCompetitionChangedAsync(int? value)
    {
        CompetitionId = value;
        PublishGlobalScope();
        return ApplyFiltersAsync();
    }

    protected Task OnTeamChangedAsync(int? value)
    {
        TeamId = value;
        PublishGlobalScope();
        return ApplyFiltersAsync();
    }

    protected Task OnPositionChangedAsync(int? value)
    {
        PositionId = value;
        return ApplyFiltersAsync();
    }

    protected Task OnSeasonChangedAsync(string? value)
    {
        Season = string.IsNullOrWhiteSpace(value) ? null : value;
        PublishGlobalScope();
        return ApplyFiltersAsync();
    }

    protected Task OnDayChangedAsync(string? value)
    {
        Day = string.IsNullOrWhiteSpace(value) ? null : value;
        PublishGlobalScope();
        return ApplyFiltersAsync();
    }

    protected async Task ResetFiltersAsync()
    {
        CompetitionId = null;
        TeamId = null;
        PositionId = null;
        Season = null;
        Day = null;
        Search = string.Empty;
        SelectedPlayerId = null;
        IsPlayerListOpen = true;
        ResetPositionProfileCompareSelections();
        PublishGlobalScope();
        await LoadAsync(refreshReferenceData: true);
    }

    protected async Task SelectPlayerAsync(int playerId)
    {
        SelectedPlayerId = playerId;
        IsPlayerListOpen = false;
        ResetPositionProfileCompareSelections();
        await LoadPositionProfileAsync();
    }

    protected async Task OnPositionProfileComparePlayerChangedAsync(int slot, ChangeEventArgs args)
    {
        if (slot < 0 || slot >= PositionProfileCompareSelections.Count)
        {
            return;
        }

        PositionProfileCompareSelections[slot] = int.TryParse(args.Value?.ToString(), out var playerId) ? playerId : null;
        SanitizePositionProfileCompareSelections();
        await RefreshPositionProfileComparisonAsync();
    }

    protected string GetPositionProfileCompareSelectionValue(int slot)
    {
        if (slot < 0 || slot >= PositionProfileCompareSelections.Count)
        {
            return string.Empty;
        }

        return PositionProfileCompareSelections[slot]?.ToString() ?? string.Empty;
    }

    protected async Task ExportPositionProfileCsvAsync()
    {
        if (PositionProfile?.SelectedPlayer is null)
        {
            return;
        }

        var lines = new List<string>
        {
            "Axe;Categorie;Valeur joueuse;Mediane poste;Delta;Percentile;Lecture"
        };

        lines.AddRange(PositionProfileDetailRows.Select(axis => string.Join(";",
            EscapeCsv(axis.Label),
            EscapeCsv(axis.Category),
            EscapeCsv(axis.PlayerDisplayValue),
            EscapeCsv(axis.MedianDisplayValue),
            EscapeCsv(axis.DeltaDisplayValue),
            EscapeCsv($"{FormatRate(axis.Percentile)}%"),
            EscapeCsv(axis.Summary))));

        await JS.InvokeVoidAsync(
            "handwstatExports.downloadTextFile",
            BuildPositionProfileFileName("csv"),
            "text/csv;charset=utf-8",
            string.Join("\n", lines));
    }

    protected async Task ExportPositionProfileImageAsync()
    {
        if (PositionProfile?.SelectedPlayer is null)
        {
            return;
        }

        await JS.InvokeVoidAsync(
            "handwstatExports.downloadTextFile",
            BuildPositionProfileFileName("svg"),
            "image/svg+xml;charset=utf-8",
            BuildPositionProfileSvg());
    }

    protected async Task ExportPositionProfileRadarImageAsync()
    {
        if (PositionProfile?.SelectedPlayer is null || PositionProfile.MedianProfile is null)
        {
            return;
        }

        await JS.InvokeVoidAsync(
            "handwstatExports.downloadTextFile",
            BuildPositionProfileVariantFileName("radar", "svg"),
            "image/svg+xml;charset=utf-8",
            BuildPositionProfileRadarSvg());
    }

    protected async Task CopyPositionProfileSummaryAsync()
    {
        if (PositionProfile?.SelectedPlayer is null)
        {
            return;
        }

        await JS.InvokeVoidAsync("handwstatExports.copyText", BuildPositionProfileAnalystSummary());
    }

    public void Dispose()
    {
        ScopeService.Changed -= HandleGlobalScopeChanged;
        var cts = Interlocked.Exchange(ref _searchDebounceCts, null);
        cts?.Cancel();
        cts?.Dispose();
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
            SelectedPlayerId = null;
            ResetPositionProfileCompareSelections();
            await LoadAsync(refreshReferenceData: false);
        });
    }

    private void ApplyGlobalScope()
    {
        CompetitionId = ScopeService.Current.CompetitionId;
        TeamId = ScopeService.Current.TeamId;
        Season = ScopeService.Current.Season;
        Day = ScopeService.Current.Day;
    }

    private void PublishGlobalScope()
    {
        _publishingScope = true;
        try
        {
            ScopeService.Update(new AnalysisScopeSnapshot(
                CompetitionId,
                AvailableCompetitions.FirstOrDefault(item => item.CompetitionId == CompetitionId)?.CompetitionName,
                TeamId,
                AvailableTeams.FirstOrDefault(item => item.TeamId == TeamId)?.TeamName,
                Season,
                Day));
        }
        finally
        {
            _publishingScope = false;
        }
    }

    private async Task DebounceSearchAsync()
    {
        var cts = new CancellationTokenSource();
        var previous = Interlocked.Exchange(ref _searchDebounceCts, cts);
        previous?.Cancel();
        previous?.Dispose();

        try
        {
            await Task.Delay(SearchDebounceDelay, cts.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (cts.IsCancellationRequested)
        {
            return;
        }

        await ApplyFiltersAsync();
    }

    private async Task ApplyFiltersAsync()
    {
        SelectedPlayerId = null;
        ResetPositionProfileCompareSelections();
        await LoadAsync(refreshReferenceData: false);
    }

    private async Task LoadAsync(bool refreshReferenceData)
    {
        if (!AuthService.Session.IsAuthenticated)
        {
            ClearAllData();
            return;
        }

        var loadStartedAt = DateTimeOffset.UtcNow;

        try
        {
            await BusyUiHelper.EnterAsync(() => IsBusy = true, () => InvokeAsync(StateHasChanged));
            ErrorMessage = null;

            if (refreshReferenceData || ReferenceData == AnalyticsReferenceData.Empty)
            {
                ReferenceData = await ReferenceDataService.GetReferenceDataAsync(refreshReferenceData);
            }

            if (!HasDirectoryCriteria && !SelectedPlayerId.HasValue)
            {
                ClearAnalysisData();
                return;
            }

            await LoadFilterScopesAsync();
            SanitizeDirectoryFilters();

            PlayerDirectory = await PlayersApiClient.GetPlayersAsync(
                teamId: TeamId,
                positionId: PositionId,
                competitionId: CompetitionId,
                year: Year,
                season: Season,
                day: Day,
                search: Search,
                pageSize: IsCompetitionOnlyDirectoryFilter ? 1000 : 250);

            SelectedPlayerId ??= PlayerDirectory.FirstOrDefault()?.PlayerId;
            if (SelectedPlayerId.HasValue
                && PlayerDirectory.All(player => player.PlayerId != SelectedPlayerId.Value)
                && !_preserveDeepLinkedPlayer)
            {
                SelectedPlayerId = PlayerDirectory.FirstOrDefault()?.PlayerId;
            }

            await LoadPositionProfileAsync(manageBusyState: false);
            _preserveDeepLinkedPlayer = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            await BusyUiHelper.ExitAsync(() => IsBusy = false, () => InvokeAsync(StateHasChanged), loadStartedAt);
        }
    }

    private async Task LoadPositionProfileAsync(bool manageBusyState = true)
    {
        if (!SelectedPlayerId.HasValue)
        {
            PositionProfile = null;
            PositionProfileComparison = null;
            PositionProfileComparisonPlayers = [];
            PositionProfileComparisonPreviewPlayers = [];
            ClearAnalysisState();
            return;
        }

        var loadStartedAt = DateTimeOffset.UtcNow;

        try
        {
            if (manageBusyState)
            {
                await BusyUiHelper.EnterAsync(() => IsBusy = true, () => InvokeAsync(StateHasChanged));
            }

            ErrorMessage = null;
            PositionProfile = await PlayersApiClient.GetPlayerPositionProfileAsync(SelectedPlayerId.Value, BuildPositionProfileOptions());
            SyncPositionProfileComparisonFromPrimary();
            RebuildPositionProfileDerivedState();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            if (manageBusyState)
            {
                await BusyUiHelper.ExitAsync(() => IsBusy = false, () => InvokeAsync(StateHasChanged), loadStartedAt);
            }
        }
    }

    private async Task LoadFilterScopesAsync()
    {
        var competitionScopeTask = MatchesApiClient.GetMatchesAsync(teamId: TeamId, year: Year, page: 1, pageSize: 500);
        var teamScopeTask = MatchesApiClient.GetMatchesAsync(competitionId: CompetitionId, year: Year, page: 1, pageSize: 500);
        var contextScopeTask = MatchesApiClient.GetMatchesAsync(competitionId: CompetitionId, teamId: TeamId, year: Year, page: 1, pageSize: 500);

        await Task.WhenAll(competitionScopeTask, teamScopeTask, contextScopeTask);

        CompetitionOptionMatches = competitionScopeTask.Result;
        TeamOptionMatches = teamScopeTask.Result;
        ScopedMatches = contextScopeTask.Result;
    }

    private void SanitizeDirectoryFilters()
    {
        if (CompetitionId.HasValue && AvailableCompetitions.All(item => item.CompetitionId != CompetitionId.Value))
        {
            CompetitionId = null;
        }

        if (TeamId.HasValue && AvailableTeams.All(item => item.TeamId != TeamId.Value))
        {
            TeamId = null;
        }

        if (!string.IsNullOrWhiteSpace(Season) && !SeasonOptions.Contains(Season, StringComparer.OrdinalIgnoreCase))
        {
            Season = null;
        }

        if (!string.IsNullOrWhiteSpace(Day) && !DayOptions.Contains(Day, StringComparer.OrdinalIgnoreCase))
        {
            Day = null;
        }
    }

    private StatsQueryOptionsDto BuildOptions()
    {
        return new StatsQueryOptionsDto
        {
            CompetitionId = CompetitionId,
            TeamId = TeamId,
            PositionId = PositionId,
            Year = Year,
            Season = Season,
            Day = Day
        };
    }

    private StatsQueryOptionsDto BuildPositionProfileOptions()
    {
        return new StatsQueryOptionsDto
        {
            CompetitionId = CompetitionId,
            PositionId = ResolveSelectedPlayerPositionId(),
            Year = Year,
            Season = Season,
            Day = Day
        };
    }

    private int? ResolveSelectedPlayerPositionId()
    {
        if (!SelectedPlayerId.HasValue)
        {
            return PositionId;
        }

        return PlayerDirectory.FirstOrDefault(player => player.PlayerId == SelectedPlayerId.Value)?.PositionId
            ?? PositionId;
    }

    private void SyncPositionProfileComparisonFromPrimary()
    {
        if (PositionProfile?.SelectedPlayer is null)
        {
            PositionProfileComparison = null;
            PositionProfileComparisonPlayers = [];
            PositionProfileComparisonKey = BuildPositionProfileComparisonKey();
            return;
        }

        PositionProfileComparison = new PositionProfileResponseDto
        {
            PositionId = PositionProfile.PositionId,
            PositionCode = PositionProfile.PositionCode,
            PositionName = PositionProfile.PositionName,
            IsGoalkeeperProfile = PositionProfile.IsGoalkeeperProfile,
            CohortPlayerCount = PositionProfile.CohortPlayerCount,
            SelectedPlayer = PositionProfile.SelectedPlayer,
            MedianProfile = PositionProfile.MedianProfile,
            Players = [PositionProfile.SelectedPlayer]
        };

        PositionProfileComparisonPlayers = PositionProfileComparison.Players;
        UpdatePositionProfileComparisonPreviewPlayers();
        PositionProfileComparisonKey = BuildPositionProfileComparisonKey();
    }

    private async Task RefreshPositionProfileComparisonAsync()
    {
        if (PositionProfile?.SelectedPlayer is null || SelectedPlayerId is null)
        {
            PositionProfileComparison = PositionProfile;
            PositionProfileComparisonPlayers = PositionProfile?.SelectedPlayer is null ? [] : [PositionProfile.SelectedPlayer];
            UpdatePositionProfileComparisonPreviewPlayers();
            PositionProfileComparisonKey = BuildPositionProfileComparisonKey();
            return;
        }

        SanitizePositionProfileCompareSelections();
        var comparePlayerIds = PositionProfileCompareSelections
            .Where(playerId => playerId.HasValue)
            .Select(playerId => playerId!.Value)
            .Distinct()
            .ToList();

        if (comparePlayerIds.Count == 0)
        {
            SyncPositionProfileComparisonFromPrimary();
            await InvokeAsync(StateHasChanged);
            return;
        }

        var loadStartedAt = DateTimeOffset.UtcNow;
        try
        {
            await BusyUiHelper.EnterAsync(() => IsPositionProfileBusy = true, () => InvokeAsync(StateHasChanged));

            var request = new PositionProfileCompareRequestDto
            {
                PlayerIds = [SelectedPlayerId.Value, .. comparePlayerIds],
                CompetitionId = CompetitionId,
                PositionId = ResolveSelectedPlayerPositionId(),
                Year = Year,
                Season = Season,
                Day = Day
            };

            PositionProfileComparison = await PlayersApiClient.ComparePositionProfilesAsync(request) ?? PositionProfile;
            PositionProfileComparisonPlayers = PositionProfileComparison?.Players ?? [];
            UpdatePositionProfileComparisonPreviewPlayers();
            PositionProfileComparisonKey = BuildPositionProfileComparisonKey();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            SyncPositionProfileComparisonFromPrimary();
        }
        finally
        {
            await BusyUiHelper.ExitAsync(() => IsPositionProfileBusy = false, () => InvokeAsync(StateHasChanged), loadStartedAt);
        }
    }

    private void ResetPositionProfileCompareSelections() => PositionProfileCompareSelections = [null, null, null];

    private void SanitizePositionProfileCompareSelections()
    {
        var allowedIds = PositionProfileCandidates.Select(player => player.PlayerId).ToHashSet();
        var seen = new HashSet<int>();

        for (var index = 0; index < PositionProfileCompareSelections.Count; index++)
        {
            var selectedId = PositionProfileCompareSelections[index];
            if (!selectedId.HasValue || !allowedIds.Contains(selectedId.Value) || !seen.Add(selectedId.Value))
            {
                PositionProfileCompareSelections[index] = null;
            }
        }
    }

    private IReadOnlyList<PlayerListItemDto> BuildPositionProfileCandidates()
    {
        if (PositionProfile?.SelectedPlayer is null)
        {
            return [];
        }

        var selectedPlayer = PositionProfile.SelectedPlayer;
        return PlayerDirectory
            .Where(player => player.PlayerId != selectedPlayer.PlayerId)
            .Where(player => !selectedPlayer.PositionId.HasValue || player.PositionId == selectedPlayer.PositionId)
            .OrderBy(player => player.TeamName)
            .ThenBy(player => player.FullName)
            .ToList();
    }

    private void RebuildPositionProfileDerivedState()
    {
        if (PositionProfile?.SelectedPlayer is null || PositionProfile.MedianProfile is null)
        {
            ClearAnalysisState();
            return;
        }

        var orderedAxes = (PositionProfile.SelectedPlayer.Axes ?? [])
            .Where(axis => axis is not null && !string.IsNullOrWhiteSpace(axis.Label))
            .OrderBy(GetAxisSortRank)
            .ThenBy(axis => axis.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ScatterBounds = BuildScatterBounds(orderedAxes);
        PositionProfileAxes = BuildPositionProfileAxisViewModels(orderedAxes, ScatterBounds);
        PositionProfileDetailRows = PositionProfileAxes
            .OrderByDescending(axis => axis.Impact)
            .ThenBy(axis => axis.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
        PositionProfileChartRows = PositionProfileAxes
            .Where(axis => !IsRadarHistogramExcludedAxis(axis))
            .ToList();
        PositionProfileCoachRows = PositionProfileChartRows
            .OrderByDescending(axis => axis.DirectionalPercentile)
            .ThenBy(axis => axis.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
        PositionProfileChartMedianRadarSeries = PositionProfile?.MedianProfile is not null
            ? BuildRadarSeries(PositionProfile.MedianProfile.Axes, PositionProfileChartRows)
            : [];
        PositionProfileScopeItems = BuildScopeSummaryItems();
        PositionProfileCandidates = BuildPositionProfileCandidates();
        PositionProfileInsights = PositionProfileInsightEngine.Build(
            PositionProfile,
            PositionProfileAxes,
            PositionProfileChartRows,
            Season);
        PositionProfileCoachTakeaway = !string.IsNullOrWhiteSpace(PositionProfileInsights.QuickSummary)
            ? PositionProfileInsights.QuickSummary
            : BuildCoachTakeaway(PositionProfileChartRows);
        PositionProfileRadarKey = BuildPositionProfileRadarKey();
        PositionProfileHistogramKey = BuildPositionProfileHistogramKey();
        PositionProfileScatterKey = BuildPositionProfileScatterKey();
        PositionProfileComparisonKey = BuildPositionProfileComparisonKey();

        ValidatePositionProfileData();
    }

    private void ClearAllData()
    {
        ReferenceData = AnalyticsReferenceData.Empty;
        ClearAnalysisData();
    }

    private void ClearAnalysisData()
    {
        ScopedMatches = [];
        PlayerDirectory = [];
        PositionProfile = null;
        PositionProfileComparison = null;
        PositionProfileComparisonPlayers = [];
        PositionProfileComparisonPreviewPlayers = [];
        ClearAnalysisState();
    }

    private void ClearAnalysisState()
    {
        PositionProfileCandidates = [];
        PositionProfileAxes = [];
        PositionProfileDetailRows = [];
        PositionProfileChartRows = [];
        PositionProfileCoachRows = [];
        PositionProfileChartMedianRadarSeries = [];
        PositionProfileScopeItems = [];
        PositionProfileInsights = PositionProfileInsightBundle.Empty;
        ScatterBounds = global::HandWStat.Models.Analytics.PositionProfileScatterBounds.Default;
        PositionProfileCoachTakeaway = string.Empty;
        PositionProfileRadarKey = string.Empty;
        PositionProfileHistogramKey = string.Empty;
        PositionProfileScatterKey = string.Empty;
        PositionProfileComparisonKey = string.Empty;
    }

    private IReadOnlyList<ScopeSummaryItem> BuildScopeSummaryItems()
    {
        if (PositionProfile?.SelectedPlayer is null)
        {
            return [];
        }

        return
        [
            new("Poste", PositionProfile.PositionName ?? PositionProfile.SelectedPlayer.PositionName ?? "Non renseigne"),
            new("Cohorte", $"{PositionProfile.CohortPlayerCount} joueuses"),
            new("Matchs", PositionProfile.SelectedPlayer.MatchesPlayed.ToString()),
            new("Equipe", PositionProfile.SelectedPlayer.TeamName ?? "Non renseignee"),
            new("Saison", string.IsNullOrWhiteSpace(Season) ? "Toutes" : Season)
        ];
    }

    private static PositionProfileScatterBounds BuildScatterBounds(IReadOnlyList<PositionProfileAxisDto> axes)
    {
        if (axes.Count == 0)
        {
            return global::HandWStat.Models.Analytics.PositionProfileScatterBounds.Default;
        }

        var values = axes
            .SelectMany(axis => new[] { axis.Value, axis.MedianValue })
            .Where(double.IsFinite)
            .ToArray();

        if (values.Length == 0)
        {
            return global::HandWStat.Models.Analytics.PositionProfileScatterBounds.Default;
        }

        var min = values.Min();
        var max = values.Max();

        if (Math.Abs(max - min) < 0.0001)
        {
            min -= 1d;
            max += 1d;
        }

        var padding = Math.Max((max - min) * 0.08d, 0.5d);
        min = Math.Max(0d, min - padding);
        max += padding;

        return new PositionProfileScatterBounds(min, max, 5);
    }

    private IReadOnlyList<PositionProfileAxisViewModel> BuildPositionProfileAxisViewModels(
        IReadOnlyList<PositionProfileAxisDto> axes,
        PositionProfileScatterBounds bounds)
    {
        if (axes.Count == 0)
        {
            return [];
        }

        var spread = Math.Max(bounds.Range * 0.0125d, 0.15d);
        var offsets = BuildScatterOffsets(axes, spread);
        var result = new List<PositionProfileAxisViewModel>(axes.Count);

        foreach (var axis in axes)
        {
            var direction = GetScatterDirection(axis.Value, axis.MedianValue, axis.Format);
            var summary = BuildAxisSummary(axis, direction);
            var directionLabel = direction > 0 ? "Au-dessus" : direction < 0 ? "Sous" : "Au niveau";
            var displayValue = FormatPositionProfileAxisValue(axis.Value, axis.Format);
            var displayMedian = FormatPositionProfileAxisValue(axis.MedianValue, axis.Format);
            var delta = FormatScatterDelta(axis.Value - axis.MedianValue, axis.Format);
            var offset = offsets.TryGetValue(axis.Key, out var jitter)
                ? jitter
                : (PlayerOffset: 0d, MedianOffset: 0d);

            result.Add(new PositionProfileAxisViewModel(
                axis.Key,
                axis.Label,
                axis.Category,
                axis.HigherIsBetter,
                axis.Format,
                axis.Value,
                axis.MedianValue,
                axis.Percentile,
                axis.Tone,
                displayValue,
                displayMedian,
                delta,
                directionLabel,
                summary,
                GetPositionProfileCoachLegend(axis),
                Math.Clamp(axis.Value + offset.PlayerOffset, bounds.Min, bounds.Max),
                Math.Clamp(axis.MedianValue + offset.MedianOffset, bounds.Min, bounds.Max),
                axis.MinValue,
                axis.MaxValue));
        }

        return result;
    }

    private static Dictionary<string, (double PlayerOffset, double MedianOffset)> BuildScatterOffsets(
        IReadOnlyList<PositionProfileAxisDto> axes,
        double spread)
    {
        var offsets = new Dictionary<string, (double PlayerOffset, double MedianOffset)>(StringComparer.OrdinalIgnoreCase);
        var groups = axes
            .GroupBy(axis => $"{Math.Round(axis.MedianValue, 2):0.##}|{Math.Round(axis.Value, 2):0.##}|{axis.Format}", StringComparer.OrdinalIgnoreCase);

        foreach (var group in groups)
        {
            var items = group.OrderBy(axis => axis.Label, StringComparer.OrdinalIgnoreCase).ToList();
            if (items.Count == 1)
            {
                offsets[items[0].Key] = (0d, 0d);
                continue;
            }

            for (var index = 0; index < items.Count; index++)
            {
                var angle = (-Math.PI / 2d) + (2d * Math.PI * index / items.Count);
                offsets[items[index].Key] = (Math.Cos(angle) * spread, Math.Sin(angle) * spread);
            }
        }

        return offsets;
    }

    private static IReadOnlyList<MetricPlotPoint> BuildRadarSeries(
        IReadOnlyList<PositionProfileAxisDto> sourceAxes,
        IReadOnlyList<PositionProfileAxisViewModel> referenceAxes)
    {
        if (sourceAxes.Count == 0 || referenceAxes.Count == 0)
        {
            return [];
        }

        var lookup = sourceAxes
            .Where(axis => !string.IsNullOrWhiteSpace(axis.Key))
            .ToDictionary(axis => axis.Key, axis => axis, StringComparer.OrdinalIgnoreCase);

        return referenceAxes
            .Where(axis => !string.IsNullOrWhiteSpace(axis.Key))
            .Select(axis =>
            {
                if (!lookup.TryGetValue(axis.Key, out var sourceAxis))
                {
                    return new MetricPlotPoint(axis.Label, axis.RadarMedianValue);
                }

                return new MetricPlotPoint(axis.Label, NormalizeRadarValue(sourceAxis));
            })
            .ToList();
    }

    private static string BuildAxisSummary(PositionProfileAxisDto axis, int direction)
    {
        return direction switch
        {
            > 0 when axis.HigherIsBetter => "Au-dessus de la mediane du poste, axe a exploiter.",
            > 0 => "Au-dessus de la mediane, volume a contenir sur ce poste.",
            < 0 when axis.HigherIsBetter => "Sous la mediane, marge de progression claire.",
            < 0 => "Sous la mediane, controle du risque correct mais a surveiller.",
            _ => "Cale sur la mediane du poste, zone de stabilite."
        };
    }

    private static double NormalizeRadarValue(PositionProfileAxisDto axis)
    {
        if (!double.IsFinite(axis.MinValue) || !double.IsFinite(axis.MaxValue) || axis.MaxValue <= axis.MinValue)
        {
            // The API percentile is already oriented so that a higher score is favorable.
            return Math.Clamp(axis.Percentile, 0d, 100d);
        }

        var normalized = (axis.Value - axis.MinValue) * 100d / (axis.MaxValue - axis.MinValue);

        if (!axis.HigherIsBetter)
        {
            normalized = 100d - normalized;
        }

        return Math.Clamp(Math.Round(normalized, 1, MidpointRounding.AwayFromZero), 0d, 100d);
    }

    private static int GetAxisSortRank(PositionProfileAxisDto axis)
    {
        var text = $"{axis.Category} {axis.Key} {axis.Label}".ToLowerInvariant();

        if (text.Contains("goalkeeper") || text.Contains("keeper") || text.Contains("save") || text.Contains("arret"))
        {
            return 4;
        }

        if (text.Contains("discip") || text.Contains("sanction") || text.Contains("penalty"))
        {
            return 3;
        }

        if (text.Contains("def") || text.Contains("interception") || text.Contains("block") || text.Contains("neutral"))
        {
            return 2;
        }

        if (text.Contains("pass") || text.Contains("assist") || text.Contains("create") || text.Contains("ball"))
        {
            return 1;
        }

        return 0;
    }

    private void ValidatePositionProfileData()
    {
        if (PositionProfile?.SelectedPlayer is null)
        {
            return;
        }

        if (PositionProfileAxes.Count == 0)
        {
            Logger.LogWarning("Position profile for player {PlayerId} returned no axes.", PositionProfile.SelectedPlayer.PlayerId);
            return;
        }

        if (PositionProfileAxes.Any(axis =>
            !double.IsFinite(axis.PlayerValue)
            || !double.IsFinite(axis.MedianValue)
            || !double.IsFinite(axis.Percentile)
            || !double.IsFinite(axis.MinValue)
            || !double.IsFinite(axis.MaxValue)))
        {
            Logger.LogWarning("Position profile for player {PlayerId} contains non finite axis values.", PositionProfile.SelectedPlayer.PlayerId);
        }

        var duplicateKeys = PositionProfileAxes
            .GroupBy(axis => axis.Key, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateKeys.Count > 0)
        {
            Logger.LogWarning(
                "Position profile for player {PlayerId} contains duplicate axis keys: {Keys}",
                PositionProfile.SelectedPlayer.PlayerId,
                string.Join(", ", duplicateKeys));
        }
    }

    private string BuildCoachTakeaway(IReadOnlyList<PositionProfileAxisViewModel> axes)
    {
        if (!string.IsNullOrWhiteSpace(PositionProfileInsights.QuickSummary))
        {
            return PositionProfileInsights.QuickSummary;
        }

        var strongCount = axes.Count(axis => axis.StatusLabel == "Fort");
        var mediumCount = axes.Count(axis => axis.StatusLabel == "Moyen");
        var fragileCount = axes.Count(axis => axis.StatusLabel == "Fragile");

        if (strongCount >= 4 && fragileCount <= 1)
        {
            return "profil fort sur le poste, exploitable comme point d'appui stable du collectif.";
        }

        if (fragileCount >= 4)
        {
            return "profil encore instable sur plusieurs marqueurs du poste, a encadrer avec un role plus cible.";
        }

        if (mediumCount >= strongCount && fragileCount <= 2)
        {
            return "profil regulier: peu d ecarts majeurs, base fiable pour le staff.";
        }

        return "profil intermediaire: interessant dans son coeur de role, avec quelques axes de regulation selon le contexte.";
    }

    private static string BuildPositionProfileRadarKey(PositionProfileResponseDto? profile, string? season, int? competitionId, int? teamId, int? year, string? day)
    {
        _ = teamId;
        var playerId = profile?.SelectedPlayer?.PlayerId ?? 0;
        var positionId = profile?.PositionId ?? 0;
        return $"position-radar-page-{playerId}-{positionId}-{competitionId ?? 0}-{season}-{year}-{day}";
    }

    private string BuildPositionProfileRadarKey()
    {
        return string.Join('|',
            BuildPositionProfileRadarKey(PositionProfile, Season, CompetitionId, TeamId, Year, Day),
            PositionProfileChartRows.Count,
            BuildAxisSignature(PositionProfileChartRows));
    }

    private string BuildPositionProfileHistogramKey()
    {
        return string.Join('|',
            "position-histogram-page",
            BuildProfileFilterSignature(),
            BuildAxisSignature(PositionProfileChartRows));
    }

    private string BuildPositionProfileScatterKey()
    {
        return string.Join('|',
            "position-scatter-page",
            BuildProfileFilterSignature(),
            ScatterBounds.Min.ToString("0.##"),
            ScatterBounds.Max.ToString("0.##"),
            BuildAxisSignature(PositionProfileChartRows));
    }

    private string BuildPositionProfileComparisonKey()
    {
        return string.Join('|',
            "position-compare-page",
            BuildProfileFilterSignature(),
            string.Join('-', PositionProfileComparisonPlayers.Select(player => player.PlayerId)),
            BuildComparisonSignature(PositionProfileComparisonPlayers));
    }

    private string BuildProfileFilterSignature()
    {
        var positionId = PositionProfile?.PositionId ?? PositionId ?? 0;
        return $"{SelectedPlayerId ?? 0}-{CompetitionId ?? 0}-{positionId}-{Year ?? 0}-{Season ?? "all"}-{Day ?? "all"}";
    }

    private static string BuildAxisSignature(IReadOnlyList<PositionProfileAxisViewModel> axes)
    {
        if (axes.Count == 0)
        {
            return "empty";
        }

        return string.Join(',', axes.Select(axis =>
            $"{axis.Key}:{axis.PlayerValue:0.###}:{axis.MedianValue:0.###}:{axis.Percentile:0.###}:{axis.MinValue:0.###}:{axis.MaxValue:0.###}:{axis.Bucket}"));
    }

    private static string BuildComparisonSignature(IReadOnlyList<PositionProfilePlayerDto> players)
    {
        if (players.Count == 0)
        {
            return "empty";
        }

        return string.Join(';', players.Select(player =>
        {
            var axisSignature = string.Join(',', (player.Axes ?? [])
                .OrderBy(axis => axis.Key, StringComparer.OrdinalIgnoreCase)
                .Select(axis => $"{axis.Key}:{axis.Percentile:0.###}"));

            return $"{player.PlayerId}:{axisSignature}";
        }));
    }

    private static bool IsRadarHistogramExcludedAxis(PositionProfileAxisViewModel axis)
    {
        if (string.Equals(axis.Key, "open_shot_success", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(axis.Label?.Trim(), "% jeu", StringComparison.OrdinalIgnoreCase);
    }

    private void UpdatePositionProfileComparisonPreviewPlayers()
    {
        PositionProfileComparisonPreviewPlayers = PositionProfileComparisonPlayers.Count <= 5
            ? PositionProfileComparisonPlayers
            : PositionProfileComparisonPlayers.Take(5).ToList();
    }

    private string BuildPositionProfileAnalystSummary()
    {
        var player = PositionProfile!.SelectedPlayer!;
        var strongAxes = PositionProfileCoachRows
            .Where(axis => axis.StatusLabel == "Fort")
            .Take(3)
            .Select(axis => $"{axis.Label} ({axis.PlayerDisplayValue})")
            .ToList();
        var fragileAxes = PositionProfileCoachRows
            .Where(axis => axis.StatusLabel == "Fragile")
            .Take(3)
            .Select(axis => $"{axis.Label} ({axis.PlayerDisplayValue})")
            .ToList();

        return string.Join("\n", new[]
        {
            $"Profil poste - {player.FullName}",
            $"Poste: {PositionProfile.PositionName ?? player.PositionName ?? "Non renseigne"} | Equipe: {player.TeamName ?? "Non renseignee"} | Saison: {(string.IsNullOrWhiteSpace(Season) ? "Toutes" : Season)}",
            $"Cohorte analysee: {PositionProfile.CohortPlayerCount} joueuses",
            $"Niveau: {PositionProfileInsights.PerformanceLevel} | Profil: {PositionProfileInsights.ProfileType}",
            string.IsNullOrWhiteSpace(PositionProfileInsights.QuickSummary) ? "Synthese coach: non disponible" : "Synthese coach: " + PositionProfileInsights.QuickSummary,
            string.IsNullOrWhiteSpace(PositionProfileInsights.ContextualInsight) ? string.Empty : "Lecture contextuelle: " + PositionProfileInsights.ContextualInsight,
            strongAxes.Count > 0 ? "Points forts: " + string.Join(", ", strongAxes) : "Points forts: aucun axe nettement au-dessus du poste sur ce filtre.",
            fragileAxes.Count > 0 ? "Axes de vigilance: " + string.Join(", ", fragileAxes) : "Axes de vigilance: aucun axe structurellement fragile sur ce filtre.",
            string.IsNullOrWhiteSpace(PositionProfileInsights.ContextualInsight)
                ? "Lecture staff: " + BuildCoachTakeaway(PositionProfileChartRows)
                : "Lecture staff: " + PositionProfileInsights.ContextualInsight
        }
        .Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    private string BuildPositionProfileSvg()
    {
        var player = PositionProfile!.SelectedPlayer!;
        var width = 1660;
        var chartTop = 276d;
        var chartHeight = 520d;
        var tableTop = chartTop + chartHeight + 24d;
        var tableHeight = 114d + (PositionProfileDetailRows.Count * 42d);
        var footerY = tableTop + tableHeight + 20d;
        var height = (int)Math.Ceiling(footerY + 32d);
        var headerNote = BuildPositionProfileHeaderNote();
        var summaryCards = BuildPositionProfileSummaryCards();
        var histogramMarkup = BuildPositionProfileHistogramPanelMarkup(36, chartTop, 754, chartHeight);
        var radarMarkup = BuildPositionProfileRadarPanelMarkup(810, chartTop, 814, chartHeight);
        var tableMarkup = BuildPositionProfileAxisTableMarkup(36, tableTop, 1588, tableHeight);

        return $"""
<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">
  <rect width="{width}" height="{height}" rx="28" fill="#eef4fb" />
  <rect x="12" y="12" width="{width - 24}" height="{height - 24}" rx="24" fill="#ffffff" stroke="#d8e2ef" />
  <text x="36" y="52" font-family="Segoe UI, Arial, sans-serif" font-size="14" font-weight="700" fill="{ChartPalette.Player}">PROFIL POSTE</text>
  <text x="36" y="86" font-family="Segoe UI, Arial, sans-serif" font-size="32" font-weight="800" fill="#0f172a">{SvgExportHelpers.EscapeXml(player.FullName)}</text>
  <text x="36" y="112" font-family="Segoe UI, Arial, sans-serif" font-size="16" fill="#475569">{SvgExportHelpers.EscapeXml((PositionProfile.PositionName ?? player.PositionName ?? "Poste") + " - " + (player.TeamName ?? "Equipe"))}</text>
  <text x="36" y="138" font-family="Segoe UI, Arial, sans-serif" font-size="13" fill="#64748b">{SvgExportHelpers.EscapeXml(headerNote)}</text>
  <rect x="1360" y="34" width="264" height="40" rx="20" fill="{ChartPalette.Primary}" />
  <text x="1492" y="60" text-anchor="middle" font-family="Segoe UI, Arial, sans-serif" font-size="14" font-weight="700" fill="#ffffff">Cohorte {PositionProfile.CohortPlayerCount}</text>
  {summaryCards}
  {histogramMarkup}
  {radarMarkup}
  {tableMarkup}
  <text x="36" y="{height - 14}" font-family="Segoe UI, Arial, sans-serif" font-size="12" fill="{ChartPalette.Reference}">HandWStat - export profil poste</text>
</svg>
""";
    }

    private string BuildPositionProfileRadarSvg()
    {
        var player = PositionProfile!.SelectedPlayer!;
        var width = 1360;
        var height = 860;
        var radarMarkup = BuildPositionProfileRadarPanelMarkup(36, 160, 860, 660);
        var summaryMarkup = BuildPositionProfileRadarSummaryMarkup(920, 160, 404, 660);

        return $"""
<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">
  <rect width="{width}" height="{height}" rx="28" fill="#eef4fb" />
  <rect x="12" y="12" width="{width - 24}" height="{height - 24}" rx="24" fill="#ffffff" stroke="#d8e2ef" />
  <text x="36" y="52" font-family="Segoe UI, Arial, sans-serif" font-size="14" font-weight="700" fill="{ChartPalette.Player}">RADAR PROFIL POSTE</text>
  <text x="36" y="86" font-family="Segoe UI, Arial, sans-serif" font-size="32" font-weight="800" fill="#0f172a">{SvgExportHelpers.EscapeXml(player.FullName)}</text>
  <text x="36" y="112" font-family="Segoe UI, Arial, sans-serif" font-size="16" fill="#475569">{SvgExportHelpers.EscapeXml((PositionProfile.PositionName ?? player.PositionName ?? "Poste") + " - " + (player.TeamName ?? "Equipe"))}</text>
  <text x="36" y="138" font-family="Segoe UI, Arial, sans-serif" font-size="13" fill="#64748b">{SvgExportHelpers.EscapeXml(BuildPositionProfileHeaderNote())}</text>
  {radarMarkup}
  {summaryMarkup}
  <text x="36" y="{height - 14}" font-family="Segoe UI, Arial, sans-serif" font-size="12" fill="{ChartPalette.Reference}">HandWStat - export radar poste</text>
</svg>
""";
    }

    private string BuildPositionProfileSummaryCards()
    {
        var averagePercentile = PositionProfileChartRows.Count == 0 ? 0d : PositionProfileChartRows.Average(axis => axis.DirectionalPercentile);
        var level = string.IsNullOrWhiteSpace(PositionProfileInsights.PerformanceLevel) ? "N/A" : PositionProfileInsights.PerformanceLevel;
        var profileType = string.IsNullOrWhiteSpace(PositionProfileInsights.ProfileType) ? "N/A" : PositionProfileInsights.ProfileType;
        var cards = new (string Label, string Value, string Note, string Tone)[]
        {
            ("Cohorte", PositionProfile?.CohortPlayerCount.ToString() ?? "0", "Joueuses du poste dans le perimetre.", "info"),
            ("Niveau", level, "Lecture globale du profil.", "positive"),
            ("Profil", profileType, "Type de poste dominant.", "good"),
            ("Moy. percentile", $"{averagePercentile:0.#}%", "Sur les axes affiches dans la fiche.", averagePercentile >= 65d ? "positive" : "warning")
        };

        var xPositions = new double[] { 36d, 416d, 796d, 1176d };
        return string.Join("\n", cards.Select((card, index) =>
            SvgExportHelpers.BuildMetricCard(xPositions[index], 162, 360, 92, card.Label, card.Value, card.Note, card.Tone)));
    }

    private string BuildPositionProfileHistogramPanelMarkup(double panelX, double panelY, double panelWidth, double panelHeight)
    {
        var axes = PositionProfileChartRows.ToList();
        if (axes.Count == 0)
        {
            return $"""
  <rect x="{SvgExportHelpers.FormatNumber(panelX)}" y="{SvgExportHelpers.FormatNumber(panelY)}" width="{SvgExportHelpers.FormatNumber(panelWidth)}" height="{SvgExportHelpers.FormatNumber(panelHeight)}" rx="24" fill="#ffffff" stroke="#d8e2ef" />
  <text x="{SvgExportHelpers.FormatNumber(panelX + (panelWidth / 2d))}" y="{SvgExportHelpers.FormatNumber(panelY + (panelHeight / 2d))}" text-anchor="middle" font-family="Segoe UI, Arial, sans-serif" font-size="18" font-weight="700" fill="#0f172a">Histogramme poste indisponible</text>
""";
        }

        var scaleMax = GetPositionProfileHistogramScaleMaximum(axes);
        var legendX = panelX + panelWidth - 246d;
        var legendY = panelY + 38d;
        var chartLeft = panelX + 176d;
        var chartRight = panelX + panelWidth - 26d;
        var chartWidth = chartRight - chartLeft;
        var chartTop = panelY + 100d;
        var chartBottom = panelY + panelHeight - 52d;
        var rowHeight = Math.Max(24d, (chartBottom - chartTop) / axes.Count);
        var tickCount = 5;
        var gridLines = new List<string>();
        for (var tick = 0; tick <= tickCount; tick++)
        {
            var ratio = tick / (double)tickCount;
            var x = chartLeft + (chartWidth * ratio);
            gridLines.Add($"""<line x1="{SvgExportHelpers.FormatNumber(x)}" y1="{SvgExportHelpers.FormatNumber(chartTop - 8d)}" x2="{SvgExportHelpers.FormatNumber(x)}" y2="{SvgExportHelpers.FormatNumber(chartBottom)}" stroke="#e2e8f0" stroke-width="1" />""");
            gridLines.Add($"""<text x="{SvgExportHelpers.FormatNumber(x)}" y="{SvgExportHelpers.FormatNumber(panelY + panelHeight - 28d)}" text-anchor="middle" font-family="Segoe UI, Arial, sans-serif" font-size="11" fill="#64748b">{SvgExportHelpers.FormatNumber(scaleMax * ratio, "0.#")}</text>""");
        }

        var rows = string.Join("\n", axes.Select((axis, index) =>
        {
            var rowY = chartTop + (index * rowHeight);
            var stripe = index % 2 == 0 ? "#f8fafc" : "#ffffff";
            var playerWidth = Math.Clamp(axis.PlayerValue / scaleMax, 0d, 1d) * chartWidth;
            var medianWidth = Math.Clamp(axis.MedianValue / scaleMax, 0d, 1d) * chartWidth;

            return $"""
  <rect x="{SvgExportHelpers.FormatNumber(panelX + 18d)}" y="{SvgExportHelpers.FormatNumber(rowY - 2d)}" width="{SvgExportHelpers.FormatNumber(panelWidth - 36d)}" height="{SvgExportHelpers.FormatNumber(rowHeight - 4d)}" rx="12" fill="{stripe}" />
  {SvgExportHelpers.BuildWrappedText(panelX + 24d, rowY + 16d, 150d, axis.Label, "#0f172a", 12, 700, "start", 1.05, 2)}
  <rect x="{SvgExportHelpers.FormatNumber(chartLeft)}" y="{SvgExportHelpers.FormatNumber(rowY + 5d)}" width="{SvgExportHelpers.FormatNumber(chartWidth)}" height="8" rx="4" fill="#e2e8f0" />
  <rect x="{SvgExportHelpers.FormatNumber(chartLeft)}" y="{SvgExportHelpers.FormatNumber(rowY + 4d)}" width="{SvgExportHelpers.FormatNumber(playerWidth)}" height="10" rx="5" fill="{ChartPalette.Player}" />
  <rect x="{SvgExportHelpers.FormatNumber(chartLeft)}" y="{SvgExportHelpers.FormatNumber(rowY + 17d)}" width="{SvgExportHelpers.FormatNumber(medianWidth)}" height="8" rx="4" fill="{ChartPalette.Reference}" />
""";
        }));

        return $"""
  <rect x="{SvgExportHelpers.FormatNumber(panelX)}" y="{SvgExportHelpers.FormatNumber(panelY)}" width="{SvgExportHelpers.FormatNumber(panelWidth)}" height="{SvgExportHelpers.FormatNumber(panelHeight)}" rx="24" fill="#ffffff" stroke="#d8e2ef" />
  <text x="{SvgExportHelpers.FormatNumber(panelX + 24d)}" y="{SvgExportHelpers.FormatNumber(panelY + 36d)}" font-family="Segoe UI, Arial, sans-serif" font-size="15" font-weight="700" fill="{ChartPalette.Player}">HISTOGRAMME POSTE</text>
  <text x="{SvgExportHelpers.FormatNumber(panelX + 24d)}" y="{SvgExportHelpers.FormatNumber(panelY + 58d)}" font-family="Segoe UI, Arial, sans-serif" font-size="13" fill="#64748b">Valeurs brutes joueuse face a la mediane brute du poste.</text>
  <g transform="translate(0,0)">
    <rect x="{SvgExportHelpers.FormatNumber(legendX)}" y="{SvgExportHelpers.FormatNumber(legendY)}" width="220" height="58" rx="16" fill="#f8fafc" stroke="#d8e2ef" />
    <line x1="{SvgExportHelpers.FormatNumber(legendX + 16d)}" y1="{SvgExportHelpers.FormatNumber(legendY + 22d)}" x2="{SvgExportHelpers.FormatNumber(legendX + 60d)}" y2="{SvgExportHelpers.FormatNumber(legendY + 22d)}" stroke="{ChartPalette.Player}" stroke-width="4" />
    <text x="{SvgExportHelpers.FormatNumber(legendX + 70d)}" y="{SvgExportHelpers.FormatNumber(legendY + 27d)}" font-family="Segoe UI, Arial, sans-serif" font-size="13" fill="#334155">Joueuse</text>
    <line x1="{SvgExportHelpers.FormatNumber(legendX + 16d)}" y1="{SvgExportHelpers.FormatNumber(legendY + 42d)}" x2="{SvgExportHelpers.FormatNumber(legendX + 60d)}" y2="{SvgExportHelpers.FormatNumber(legendY + 42d)}" stroke="{ChartPalette.Reference}" stroke-width="4" />
    <text x="{SvgExportHelpers.FormatNumber(legendX + 70d)}" y="{SvgExportHelpers.FormatNumber(legendY + 47d)}" font-family="Segoe UI, Arial, sans-serif" font-size="13" fill="#334155">Mediane du poste</text>
  </g>
  {string.Join("\n", gridLines)}
  {rows}
  <text x="{SvgExportHelpers.FormatNumber(panelX + 24d)}" y="{SvgExportHelpers.FormatNumber(panelY + panelHeight - 10d)}" font-family="Segoe UI, Arial, sans-serif" font-size="12" fill="#64748b">Les barres comparent la valeur brute de la joueuse a la mediane brute du poste.</text>
""";
    }

    private string BuildPositionProfileRadarPanelMarkup(double panelX, double panelY, double panelWidth, double panelHeight)
    {
        var axes = PositionProfileChartRows.ToList();
        if (axes.Count == 0)
        {
            return $"""
  <rect x="{SvgExportHelpers.FormatNumber(panelX)}" y="{SvgExportHelpers.FormatNumber(panelY)}" width="{SvgExportHelpers.FormatNumber(panelWidth)}" height="{SvgExportHelpers.FormatNumber(panelHeight)}" rx="24" fill="#ffffff" stroke="#d8e2ef" />
  <text x="{SvgExportHelpers.FormatNumber(panelX + (panelWidth / 2d))}" y="{SvgExportHelpers.FormatNumber(panelY + (panelHeight / 2d))}" text-anchor="middle" font-family="Segoe UI, Arial, sans-serif" font-size="18" font-weight="700" fill="#0f172a">Radar poste indisponible</text>
""";
        }

        var centerX = panelX + 250d;
        var centerY = panelY + Math.Min(panelHeight * 0.58d, panelHeight - 190d);
        var radius = Math.Min(165d, Math.Min((panelWidth - 300d) / 2d, (panelHeight - 150d) / 2d));
        radius = Math.Max(radius, 140d);
        var radarMax = GetRadarScaleMaximum(axes);
        var medianLookup = PositionProfileChartMedianRadarSeries
            .Where(point => !string.IsNullOrWhiteSpace(point.Label))
            .ToDictionary(point => point.Label, point => point.Value, StringComparer.OrdinalIgnoreCase);
        var polygonLevels = new[] { 0.25d, 0.5d, 0.75d, 1d };
        var grid = string.Join("\n", polygonLevels.Select(level => BuildRadarPolygon(axes, centerX, centerY, radius * level, _ => radarMax, "none", "#d7e0ea", 1, radarMax)));
        var spokes = string.Join("\n", axes.Select((axis, index) =>
        {
            var point = GetRadarPoint(index, axes.Count, centerX, centerY, radius, radarMax, radarMax);
            return $"""<line x1="{SvgExportHelpers.FormatNumber(centerX)}" y1="{SvgExportHelpers.FormatNumber(centerY)}" x2="{SvgExportHelpers.FormatNumber(point.X)}" y2="{SvgExportHelpers.FormatNumber(point.Y)}" stroke="#d7e0ea" stroke-width="1" />""";
        }));
        var labels = string.Join("\n", axes.Select((axis, index) =>
        {
            var point = GetRadarPoint(index, axes.Count, centerX, centerY, radius + 38d, radarMax, radarMax);
            return $"""<text x="{SvgExportHelpers.FormatNumber(point.X)}" y="{SvgExportHelpers.FormatNumber(point.Y)}" text-anchor="{GetRadarTextAnchor(point.X, centerX)}" font-family="Segoe UI, Arial, sans-serif" font-size="13" font-weight="700" fill="#334155">{SvgExportHelpers.EscapeXml(axis.Label)}</text>""";
        }));
        var medianPolygon = BuildRadarPolygon(
            axes,
            centerX,
            centerY,
            radius,
            axis => medianLookup.TryGetValue(axis.Label, out var medianValue) ? medianValue : axis.RadarMedianValue,
            ChartColorUtilities.WithAlpha(ChartPalette.Reference, 0.14),
            ChartPalette.Reference,
            2.5,
            radarMax,
            "7 6");
        var playerPolygon = BuildRadarPolygon(axes, centerX, centerY, radius, axis => axis.RadarPlayerValue, ChartColorUtilities.WithAlpha(ChartPalette.Player, 0.18), ChartPalette.Player, 3, radarMax);
        var legendX = panelX + panelWidth - 248d;
        var legendY = panelY + 38d;

        return $"""
  <rect x="{SvgExportHelpers.FormatNumber(panelX)}" y="{SvgExportHelpers.FormatNumber(panelY)}" width="{SvgExportHelpers.FormatNumber(panelWidth)}" height="{SvgExportHelpers.FormatNumber(panelHeight)}" rx="24" fill="#ffffff" stroke="#d8e2ef" />
  <text x="{SvgExportHelpers.FormatNumber(panelX + 24d)}" y="{SvgExportHelpers.FormatNumber(panelY + 36d)}" font-family="Segoe UI, Arial, sans-serif" font-size="15" font-weight="700" fill="{ChartPalette.Player}">RADAR NORMALISE</text>
  <text x="{SvgExportHelpers.FormatNumber(panelX + 24d)}" y="{SvgExportHelpers.FormatNumber(panelY + 58d)}" font-family="Segoe UI, Arial, sans-serif" font-size="13" fill="#64748b">Chaque axe est remis sur la meme echelle 0-100 pour comparer la forme du poste.</text>
  <rect x="{SvgExportHelpers.FormatNumber(legendX)}" y="{SvgExportHelpers.FormatNumber(legendY)}" width="220" height="82" rx="16" fill="#f8fafc" stroke="#d8e2ef" />
  <line x1="{SvgExportHelpers.FormatNumber(legendX + 16d)}" y1="{SvgExportHelpers.FormatNumber(legendY + 24d)}" x2="{SvgExportHelpers.FormatNumber(legendX + 60d)}" y2="{SvgExportHelpers.FormatNumber(legendY + 24d)}" stroke="{ChartPalette.Player}" stroke-width="4" />
  <text x="{SvgExportHelpers.FormatNumber(legendX + 70d)}" y="{SvgExportHelpers.FormatNumber(legendY + 29d)}" font-family="Segoe UI, Arial, sans-serif" font-size="13" fill="#334155">{SvgExportHelpers.EscapeXml(PositionProfile?.SelectedPlayer?.FullName ?? "Joueuse")}</text>
  <line x1="{SvgExportHelpers.FormatNumber(legendX + 16d)}" y1="{SvgExportHelpers.FormatNumber(legendY + 52d)}" x2="{SvgExportHelpers.FormatNumber(legendX + 60d)}" y2="{SvgExportHelpers.FormatNumber(legendY + 52d)}" stroke="{ChartPalette.Reference}" stroke-width="4" stroke-dasharray="7 6" />
  <text x="{SvgExportHelpers.FormatNumber(legendX + 70d)}" y="{SvgExportHelpers.FormatNumber(legendY + 57d)}" font-family="Segoe UI, Arial, sans-serif" font-size="13" fill="#334155">Mediane du poste</text>
  <text x="{SvgExportHelpers.FormatNumber(legendX + 16d)}" y="{SvgExportHelpers.FormatNumber(legendY + 74d)}" font-family="Segoe UI, Arial, sans-serif" font-size="11" fill="#64748b">Lecture normalisee de la cohorte poste.</text>
  {grid}
  {spokes}
  {medianPolygon}
  {playerPolygon}
  {labels}
  <circle cx="{SvgExportHelpers.FormatNumber(centerX)}" cy="{SvgExportHelpers.FormatNumber(centerY)}" r="4" fill="#0f172a" />
  <text x="{SvgExportHelpers.FormatNumber(panelX + 24d)}" y="{SvgExportHelpers.FormatNumber(panelY + panelHeight - 12d)}" font-family="Segoe UI, Arial, sans-serif" font-size="12" fill="#64748b">La courbe joueuse et la courbe pointillee sont normalisees sur la cohorte du poste.</text>
""";
    }

    private string BuildPositionProfileAxisTableMarkup(double panelX, double panelY, double panelWidth, double panelHeight)
    {
        var rows = PositionProfileDetailRows;
        var tableX = panelX + 24d;
        var tableY = panelY + 78d;
        var tableWidth = panelWidth - 48d;
        var headerHeight = 38d;
        var rowHeight = 42d;
        var axisWidth = 260d;
        var playerWidth = 150d;
        var medianWidth = 150d;
        var deltaWidth = 122d;
        var levelWidth = 130d;
        var lectureWidth = tableWidth - axisWidth - playerWidth - medianWidth - deltaWidth - levelWidth - 38d;
        var levelX = tableX + axisWidth + playerWidth + medianWidth + deltaWidth;
        var lectureX = levelX + levelWidth + 12d;

        var bodyRows = string.Join("\n", rows.Select((axis, index) =>
        {
            var rowY = tableY + headerHeight + (index * rowHeight);
            var stripe = index % 2 == 0 ? "#f8fafc" : "#ffffff";
            var tone = SvgExportHelpers.ToneColor(axis.StatusTone);

            return $"""
  <rect x="{SvgExportHelpers.FormatNumber(tableX)}" y="{SvgExportHelpers.FormatNumber(rowY)}" width="{SvgExportHelpers.FormatNumber(tableWidth)}" height="{SvgExportHelpers.FormatNumber(rowHeight - 4d)}" rx="12" fill="{stripe}" stroke="#d8e2ef" />
  <rect x="{SvgExportHelpers.FormatNumber(tableX)}" y="{SvgExportHelpers.FormatNumber(rowY)}" width="6" height="{SvgExportHelpers.FormatNumber(rowHeight - 4d)}" rx="12" fill="{tone}" />
  {SvgExportHelpers.BuildWrappedText(tableX + 18d, rowY + 16d, axisWidth - 24d, axis.Label, "#0f172a", 12, 700, "start", 1.06, 2)}
  {SvgExportHelpers.BuildWrappedText(tableX + 18d, rowY + 31d, axisWidth - 24d, axis.Category, "#64748b", 10, 400, "start", 1.02, 1)}
  <text x="{SvgExportHelpers.FormatNumber(tableX + axisWidth + 18d)}" y="{SvgExportHelpers.FormatNumber(rowY + 25d)}" font-family="Segoe UI, Arial, sans-serif" font-size="14" font-weight="700" fill="#0f172a">{SvgExportHelpers.EscapeXml(axis.PlayerDisplayValue)}</text>
  <text x="{SvgExportHelpers.FormatNumber(tableX + axisWidth + playerWidth + 18d)}" y="{SvgExportHelpers.FormatNumber(rowY + 25d)}" font-family="Segoe UI, Arial, sans-serif" font-size="14" font-weight="700" fill="#475569">{SvgExportHelpers.EscapeXml(axis.MedianDisplayValue)}</text>
  <text x="{SvgExportHelpers.FormatNumber(tableX + axisWidth + playerWidth + medianWidth + 18d)}" y="{SvgExportHelpers.FormatNumber(rowY + 25d)}" font-family="Segoe UI, Arial, sans-serif" font-size="14" font-weight="700" fill="#334155">{SvgExportHelpers.EscapeXml(axis.DeltaDisplayValue)}</text>
  <text x="{SvgExportHelpers.FormatNumber(levelX + 18d)}" y="{SvgExportHelpers.FormatNumber(rowY + 24d)}" font-family="Segoe UI, Arial, sans-serif" font-size="13" font-weight="700" fill="{tone}">{SvgExportHelpers.EscapeXml(axis.DirectionalPercentileDisplay)} - {SvgExportHelpers.EscapeXml(axis.StatusLabel)}</text>
  {SvgExportHelpers.BuildWrappedText(lectureX, rowY + 17d, lectureWidth, axis.Summary, "#475569", 11, 400, "start", 1.1, 2)}
""";
        }));

        return $"""
  <rect x="{SvgExportHelpers.FormatNumber(panelX)}" y="{SvgExportHelpers.FormatNumber(panelY)}" width="{SvgExportHelpers.FormatNumber(panelWidth)}" height="{SvgExportHelpers.FormatNumber(panelHeight)}" rx="24" fill="#ffffff" stroke="#d8e2ef" />
  <text x="{SvgExportHelpers.FormatNumber(panelX + 24d)}" y="{SvgExportHelpers.FormatNumber(panelY + 36d)}" font-family="Segoe UI, Arial, sans-serif" font-size="15" font-weight="700" fill="{ChartPalette.Player}">TABLEAU D'AXES</text>
  <text x="{SvgExportHelpers.FormatNumber(panelX + 24d)}" y="{SvgExportHelpers.FormatNumber(panelY + 58d)}" font-family="Segoe UI, Arial, sans-serif" font-size="13" fill="#64748b">Lecture detaillee des axes du poste, triee par impact pour faire ressortir les ecarts les plus importants.</text>
  <rect x="{SvgExportHelpers.FormatNumber(tableX)}" y="{SvgExportHelpers.FormatNumber(tableY)}" width="{SvgExportHelpers.FormatNumber(tableWidth)}" height="{SvgExportHelpers.FormatNumber(headerHeight)}" rx="12" fill="#f8fafc" stroke="#d8e2ef" />
  <text x="{SvgExportHelpers.FormatNumber(tableX + 16d)}" y="{SvgExportHelpers.FormatNumber(tableY + 24d)}" font-family="Segoe UI, Arial, sans-serif" font-size="12" font-weight="700" fill="#64748b">AXE</text>
  <text x="{SvgExportHelpers.FormatNumber(tableX + axisWidth + 18d)}" y="{SvgExportHelpers.FormatNumber(tableY + 24d)}" font-family="Segoe UI, Arial, sans-serif" font-size="12" font-weight="700" fill="#64748b">JOUEUSE</text>
  <text x="{SvgExportHelpers.FormatNumber(tableX + axisWidth + playerWidth + 18d)}" y="{SvgExportHelpers.FormatNumber(tableY + 24d)}" font-family="Segoe UI, Arial, sans-serif" font-size="12" font-weight="700" fill="#64748b">MEDIANE</text>
  <text x="{SvgExportHelpers.FormatNumber(tableX + axisWidth + playerWidth + medianWidth + 18d)}" y="{SvgExportHelpers.FormatNumber(tableY + 24d)}" font-family="Segoe UI, Arial, sans-serif" font-size="12" font-weight="700" fill="#64748b">DELTA</text>
  <text x="{SvgExportHelpers.FormatNumber(levelX + 18d)}" y="{SvgExportHelpers.FormatNumber(tableY + 24d)}" font-family="Segoe UI, Arial, sans-serif" font-size="12" font-weight="700" fill="#64748b">NIVEAU</text>
  <text x="{SvgExportHelpers.FormatNumber(lectureX)}" y="{SvgExportHelpers.FormatNumber(tableY + 24d)}" font-family="Segoe UI, Arial, sans-serif" font-size="12" font-weight="700" fill="#64748b">LECTURE</text>
  {bodyRows}
""";
    }

    private string BuildPositionProfileRadarSummaryMarkup(double panelX, double panelY, double panelWidth, double panelHeight)
    {
        var rows = BuildPositionProfileRadarFocusRows();
        var cards = new (string Label, string Value, string Note, string Tone)[]
        {
            ("Cohorte", PositionProfile?.CohortPlayerCount.ToString() ?? "0", "Joueuses du poste dans le perimetre.", "info"),
            ("Niveau", string.IsNullOrWhiteSpace(PositionProfileInsights.PerformanceLevel) ? "N/A" : PositionProfileInsights.PerformanceLevel, "Lecture globale du profil.", "positive"),
            ("Profil", string.IsNullOrWhiteSpace(PositionProfileInsights.ProfileType) ? "N/A" : PositionProfileInsights.ProfileType, "Type de poste dominant.", "good"),
            ("Moy. percentile", $"{(PositionProfileChartRows.Count == 0 ? 0d : PositionProfileChartRows.Average(axis => axis.DirectionalPercentile)):0.#}%", "Lecture axe par axe sur la cohorte.", "warning")
        };

        var cardXPositions = new double[] { panelX + 18d, panelX + 206d, panelX + 18d, panelX + 206d };
        var cardYPositions = new double[] { panelY + 18d, panelY + 18d, panelY + 112d, panelY + 112d };
        var cardsMarkup = string.Join("\n", cards.Select((card, index) =>
            SvgExportHelpers.BuildMetricCard(cardXPositions[index], cardYPositions[index], 170, 84, card.Label, card.Value, card.Note, card.Tone)));

        var infoText = string.IsNullOrWhiteSpace(PositionProfileInsights.QuickSummary)
            ? PositionProfileInsights.ContextualInsight
            : PositionProfileInsights.QuickSummary;

        var tableTop = panelY + 224d;
        var rowHeight = 52d;
        var rowsMarkup = string.Join("\n", rows.Select((axis, index) =>
        {
            var rowY = tableTop + 30d + (index * rowHeight);
            var stripe = index % 2 == 0 ? "#f8fafc" : "#ffffff";
            var tone = SvgExportHelpers.ToneColor(axis.StatusTone);

            return $"""
  <rect x="{SvgExportHelpers.FormatNumber(panelX + 18d)}" y="{SvgExportHelpers.FormatNumber(rowY)}" width="{SvgExportHelpers.FormatNumber(panelWidth - 36d)}" height="44" rx="12" fill="{stripe}" stroke="#d8e2ef" />
  <rect x="{SvgExportHelpers.FormatNumber(panelX + 18d)}" y="{SvgExportHelpers.FormatNumber(rowY)}" width="5" height="44" rx="12" fill="{tone}" />
  <text x="{SvgExportHelpers.FormatNumber(panelX + 34d)}" y="{SvgExportHelpers.FormatNumber(rowY + 20d)}" font-family="Segoe UI, Arial, sans-serif" font-size="12" font-weight="700" fill="#0f172a">{SvgExportHelpers.EscapeXml(axis.Label)}</text>
  <text x="{SvgExportHelpers.FormatNumber(panelX + 34d)}" y="{SvgExportHelpers.FormatNumber(rowY + 34d)}" font-family="Segoe UI, Arial, sans-serif" font-size="10" fill="#64748b">{SvgExportHelpers.EscapeXml(axis.StatusLabel)} - {SvgExportHelpers.EscapeXml(axis.DirectionalPercentileDisplay)}</text>
  <text x="{SvgExportHelpers.FormatNumber(panelX + panelWidth - 18d)}" y="{SvgExportHelpers.FormatNumber(rowY + 28d)}" text-anchor="end" font-family="Segoe UI, Arial, sans-serif" font-size="12" fill="#475569">{SvgExportHelpers.EscapeXml(axis.Summary)}</text>
""";
        }));

        return $"""
  <rect x="{SvgExportHelpers.FormatNumber(panelX)}" y="{SvgExportHelpers.FormatNumber(panelY)}" width="{SvgExportHelpers.FormatNumber(panelWidth)}" height="{SvgExportHelpers.FormatNumber(panelHeight)}" rx="24" fill="#ffffff" stroke="#d8e2ef" />
  <text x="{SvgExportHelpers.FormatNumber(panelX + 24d)}" y="{SvgExportHelpers.FormatNumber(panelY + 36d)}" font-family="Segoe UI, Arial, sans-serif" font-size="15" font-weight="700" fill="{ChartPalette.Player}">REPERES CLES</text>
  <text x="{SvgExportHelpers.FormatNumber(panelX + 24d)}" y="{SvgExportHelpers.FormatNumber(panelY + 58d)}" font-family="Segoe UI, Arial, sans-serif" font-size="13" fill="#64748b">Radar de lecture rapide avec les axes les plus forts et les plus fragiles.</text>
  {cardsMarkup}
  {SvgExportHelpers.BuildWrappedText(panelX + 18d, panelY + 212d, panelWidth - 36d, infoText, "#475569", 12, 400, "start", 1.16, 2)}
  <rect x="{SvgExportHelpers.FormatNumber(panelX + 18d)}" y="{SvgExportHelpers.FormatNumber(tableTop)}" width="{SvgExportHelpers.FormatNumber(panelWidth - 36d)}" height="26" rx="10" fill="#f8fafc" stroke="#d8e2ef" />
  <text x="{SvgExportHelpers.FormatNumber(panelX + 32d)}" y="{SvgExportHelpers.FormatNumber(tableTop + 18d)}" font-family="Segoe UI, Arial, sans-serif" font-size="11" font-weight="700" fill="#64748b">AXES FOCUS</text>
  {rowsMarkup}
  <text x="{SvgExportHelpers.FormatNumber(panelX + 18d)}" y="{SvgExportHelpers.FormatNumber(panelY + panelHeight - 12d)}" font-family="Segoe UI, Arial, sans-serif" font-size="12" fill="#64748b">Les deux lectures aident a comprendre la forme du poste et les axes a travailler.</text>
""";
    }

    private IReadOnlyList<PositionProfileAxisViewModel> BuildPositionProfileRadarFocusRows()
    {
        if (PositionProfileCoachRows.Count == 0)
        {
            return [];
        }

        var rows = new List<PositionProfileAxisViewModel>();

        foreach (var axis in PositionProfileCoachRows.Take(3))
        {
            rows.Add(axis);
        }

        foreach (var axis in PositionProfileCoachRows.TakeLast(3).Reverse())
        {
            if (rows.All(item => !string.Equals(item.Key, axis.Key, StringComparison.OrdinalIgnoreCase)))
            {
                rows.Add(axis);
            }
        }

        return rows;
    }

    private string BuildPositionProfileHeaderNote()
    {
        var seasonLabel = string.IsNullOrWhiteSpace(Season) ? "Toutes saisons" : Season!;
        var positionLabel = PositionProfile?.PositionName ?? PositionProfile?.SelectedPlayer?.PositionName ?? "Poste";
        return $"Filtre: {positionLabel} - {seasonLabel} | Cohorte poste: {PositionProfile?.CohortPlayerCount ?? 0}";
    }

    private static double GetPositionProfileHistogramScaleMaximum(IReadOnlyList<PositionProfileAxisViewModel> axes)
    {
        var candidate = axes
            .SelectMany(axis => new[] { axis.PlayerValue, axis.MedianValue })
            .Where(double.IsFinite)
            .DefaultIfEmpty(1d)
            .Max();

        var padded = candidate + Math.Max(candidate * 0.08d, 0.5d);
        return Math.Max(1d, padded);
    }

    private string BuildPositionProfileFileName(string extension)
    {
        var player = PositionProfile!.SelectedPlayer!;
        var playerSlug = Slugify(player.FullName);
        var positionSlug = Slugify(PositionProfile.PositionName ?? player.PositionName ?? "poste");
        var seasonSlug = Slugify(string.IsNullOrWhiteSpace(Season) ? "toutes-saisons" : Season!);
        return $"profil-poste-{playerSlug}-{positionSlug}-{seasonSlug}.{extension}";
    }

    private string BuildPositionProfileVariantFileName(string variant, string extension)
    {
        var player = PositionProfile!.SelectedPlayer!;
        var playerSlug = Slugify(player.FullName);
        var positionSlug = Slugify(PositionProfile.PositionName ?? player.PositionName ?? "poste");
        var seasonSlug = Slugify(string.IsNullOrWhiteSpace(Season) ? "toutes-saisons" : Season!);
        return $"profil-poste-{variant}-{playerSlug}-{positionSlug}-{seasonSlug}.{extension}";
    }

    private static string EscapeCsv(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

    private static string EscapeXml(string value) => value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");

    private static string Slugify(string value) => string.Join("-", value.Trim().ToLowerInvariant().Split(new[] { ' ', '/', '\\', '.', ',', ';', ':', '_' }, StringSplitOptions.RemoveEmptyEntries));

    private static string GetSvgTone(string tone) => tone switch
    {
        "positive" => ChartPalette.Positive,
        "good" => ChartPalette.Primary,
        "warning" => ChartPalette.Warning,
        "danger" => ChartPalette.Danger,
        _ => ChartPalette.Slate
    };

    private static string BuildRadarPolygon(
        IReadOnlyList<PositionProfileAxisViewModel> axes,
        double centerX,
        double centerY,
        double radius,
        Func<PositionProfileAxisViewModel, double> valueSelector,
        string fill,
        string stroke,
        double strokeWidth,
        double maxValue,
        string? strokeDasharray = null)
    {
        var points = axes.Select((axis, index) => GetRadarPoint(index, axes.Count, centerX, centerY, radius, valueSelector(axis), maxValue)).Select(point => $"{point.X:0.##},{point.Y:0.##}");
        var dashAttribute = string.IsNullOrWhiteSpace(strokeDasharray) ? string.Empty : $" stroke-dasharray=\"{EscapeXml(strokeDasharray)}\"";
        return $"""<polygon points="{string.Join(" ", points)}" fill="{fill}" stroke="{stroke}" stroke-width="{strokeWidth:0.##}"{dashAttribute} />""";
    }

    private static (double X, double Y) GetRadarPoint(int index, int count, double centerX, double centerY, double radius, double value, double maxValue)
    {
        var angle = (-Math.PI / 2d) + (2d * Math.PI * index / count);
        var scaleMax = double.IsFinite(maxValue) && maxValue > 0d ? maxValue : 100d;
        var normalized = Math.Clamp(value / scaleMax, 0d, 1d);
        var currentRadius = radius * normalized;
        return (centerX + (Math.Cos(angle) * currentRadius), centerY + (Math.Sin(angle) * currentRadius));
    }

    private static double GetRadarScaleMaximum(IReadOnlyList<PositionProfileAxisViewModel> axes)
    {
        _ = axes;
        return 100d;
    }

    private static string GetRadarTextAnchor(double pointX, double centerX)
    {
        if (Math.Abs(pointX - centerX) < 12)
        {
            return "middle";
        }

        return pointX < centerX ? "end" : "start";
    }

    private static string FormatPositionProfileAxisValue(double value, string? format)
    {
        return string.Equals(format, "percent", StringComparison.OrdinalIgnoreCase)
            ? $"{value:0.#}%"
            : HandballKpiHelper.FormatNumber(value);
    }

    private static string GetPositionProfileInsight(PositionProfileAxisDto axis)
    {
        return GetPositionProfileInsight(axis.Label, axis.Percentile);
    }

    private static string GetPositionProfileInsight(string label, double percentile)
    {
        _ = label;
        return percentile switch
        {
            >= 80 => "niveau fort du poste",
            >= 65 => "au-dessus de la mediane du poste",
            >= 35 => "zone a stabiliser",
            _ => "sous la mediane du poste"
        };
    }

    private static string GetAxisBadgeLabel(PositionProfileAxisViewModel axis) => GetAxisBadgeLabel(axis.DirectionalPercentile);

    private static string GetAxisBadgeLabel(double percentile) => percentile > 65 ? "Fort" : percentile >= 35 ? "Moyen" : "Fragile";

    private static string GetAxisBadgeTone(PositionProfileAxisViewModel axis) => GetAxisBadgeTone(axis.DirectionalPercentile);

    private static string GetAxisBadgeTone(double percentile) => percentile > 65 ? "positive" : percentile >= 35 ? "warning" : "danger";

    private static int GetScatterDirection(PositionProfileAxisDto axis) => GetScatterDirection(axis.Value, axis.MedianValue, axis.Format);

    private static int GetScatterDirection(double playerValue, double medianValue, string? format)
    {
        var delta = playerValue - medianValue;
        var tolerance = GetScatterTolerance(format);

        if (Math.Abs(delta) <= tolerance)
        {
            return 0;
        }

        return delta > 0 ? 1 : -1;
    }

    private static double GetScatterTolerance(string? format)
    {
        return string.Equals(format, "percent", StringComparison.OrdinalIgnoreCase) ? 0.35d : 0.08d;
    }

    private static string FormatScatterDelta(double delta, string? format)
    {
        if (string.Equals(format, "percent", StringComparison.OrdinalIgnoreCase))
        {
            return $"{delta:+0.#;-0.#;0} pts";
        }

        return $"{delta:+0.##;-0.##;0}";
    }

    private string GetPositionProfileCoachLegend(PositionProfileAxisDto axis)
    {
        return axis.Key switch
        {
            "open_goals_per60" => "Volume de finition dans le jeu, hors 7m.",
            "assists_per60" => "Capacite a creer un tir clair pour une partenaire.",
            "sanctions_won_per60" => "Pression mise a la defense pour provoquer une faute forte.",
            "penalties_won_per60" => "Capacite a forcer un 7 metres.",
            "turnovers_per60" => "Charge de pertes a contenir.",
            "interceptions_per60" => "Lecture defensive sur lignes de passe.",
            "blocks_per60" => "Presence sur les duels de tir.",
            "neutralisations_per60" => "Capacite a ralentir l attaquante avant la zone.",
            "penalties_conceded_per60" => "Situations ou la defense finit par donner un 7m.",
            "two_minutes_per60" => "Poids disciplinaire lourd pour le collectif.",
            "shot_misses_per60" => "Dechet de finition a reguler.",
            "open_shot_success" => "Rendement de tir dans le jeu.",
            "saves_per60" => "Volume d arrets produits sur 60 minutes.",
            "penalty_stops_per60" => "Impact specifique sur les 7m subis.",
            "save_rate" => "Qualite globale de lecture sur les tirs subis.",
            "shots_faced_per60" => "Charge defensive supportee.",
            "goals_conceded_per60" => "Buts encaisses rapportes au temps de jeu.",
            _ => axis.HigherIsBetter
                ? "Plus la valeur monte, plus l impact sur le poste est favorable."
                : "Plus la valeur descend, plus le profil reste propre."
        };
    }

    private static string FormatRate(double value) => value.ToString("0.#");

}

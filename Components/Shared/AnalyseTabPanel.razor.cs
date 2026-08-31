using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;
using HandWStat.Services.Analytics;
using HandWStat.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace HandWStat.Components.Shared;

public class AnalyseTabPanelBase : ComponentBase, IDisposable
{
    [Parameter]
    public int? PlayerId { get; set; }

    [Parameter]
    public int? CompetitionId { get; set; }

    [Parameter]
    public int? TeamId { get; set; }

    [Parameter]
    public string? Season { get; set; }

    [Parameter]
    public string? Day { get; set; }

    [Parameter]
    public bool IsGoalkeeper { get; set; }

    [Parameter]
    public string? PositionCode { get; set; }

    [Inject]
    protected PlayersApiClient PlayersApiClient { get; set; } = default!;

    [Inject]
    protected StatsApiClient StatsApiClient { get; set; } = default!;

    [Inject]
    protected IJSRuntime JS { get; set; } = default!;

    [Inject]
    protected ILogger<AnalyseTabPanelBase> Logger { get; set; } = default!;

    protected EventContextBreakdownDto? EventContexts { get; private set; }
    protected bool IsContextBusy { get; private set; }
    protected string? ContextErrorMessage { get; private set; }
    protected ContextDimension ActiveContextDimension { get; set; } = ContextDimension.ScoreState;

    // ── B5 — Clutch / HalfTime / GK ScoreState ───────────────────────────────
    protected ClutchBreakdownDto? ClutchData { get; private set; }
    protected bool IsClutchBusy { get; private set; }
    protected string? ClutchErrorMessage { get; private set; }

    protected HalfTimeBreakdownDto? HalfTimeData { get; private set; }
    protected bool IsHalfTimeBusy { get; private set; }
    protected string? HalfTimeErrorMessage { get; private set; }

    protected GkScoreStateBreakdownDto? GkScoreStateData { get; private set; }
    protected bool IsGkScoreStateBusy { get; private set; }
    protected string? GkScoreStateErrorMessage { get; private set; }

    protected PositionProfileResponseDto? PositionProfile { get; private set; }
    protected IReadOnlyList<PositionProfileAxisViewModel> PositionProfileAxes { get; private set; } = [];
    protected IReadOnlyList<PositionProfileAxisViewModel> PositionProfileChartRows { get; private set; } = [];
    protected IReadOnlyList<PositionProfileAxisViewModel> PositionProfileDetailRows { get; private set; } = [];
    protected IReadOnlyList<MetricPlotPoint> PositionProfileChartMedianRadarSeries { get; private set; } = [];
    protected IReadOnlyList<ScopeSummaryItem> PositionProfileScopeItems { get; private set; } = [];
    protected PositionProfileInsightBundle PositionProfileInsights { get; private set; } = PositionProfileInsightBundle.Empty;
    protected bool IsBusy { get; private set; }
    protected string? ErrorMessage { get; private set; }
    protected string AnlzHistogramKey { get; private set; } = string.Empty;
    protected string AnlzRadarKey { get; private set; } = string.Empty;

    private CancellationTokenSource? _cts;

    private int? _lastLoadedPlayerId;
    private int? _lastLoadedTeamId;
    private int? _lastLoadedCompetitionId;
    private string? _lastLoadedSeason;
    private string? _lastLoadedDay;

    protected override async Task OnParametersSetAsync()
    {
        if (PlayerId == _lastLoadedPlayerId
            && TeamId == _lastLoadedTeamId
            && CompetitionId == _lastLoadedCompetitionId
            && Season == _lastLoadedSeason
            && Day == _lastLoadedDay)
        {
            return;
        }

        _lastLoadedPlayerId = PlayerId;
        _lastLoadedTeamId = TeamId;
        _lastLoadedCompetitionId = CompetitionId;
        _lastLoadedSeason = Season;
        _lastLoadedDay = Day;

        var previous = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
        previous?.Cancel();
        previous?.Dispose();
        var ct = _cts.Token;

        if (ct.IsCancellationRequested) return;

        // Set position-aware default dimension before loading
        var position = AnalyticsPositionResolver.Resolve(PositionCode, null, IsGoalkeeper);
        ActiveContextDimension = ContextAnalyticsHelper.GetDefaultDimension(position);

        await LoadProfileAsync(ct);
        if (ct.IsCancellationRequested) return;

        await Task.WhenAll(
            LoadContextAsync(ct),
            LoadClutchAsync(ct),
            LoadHalfTimeAsync(ct),
            LoadGkScoreStateAsync(ct));

        if (!ct.IsCancellationRequested) StateHasChanged();
    }

    public void Dispose()
    {
        var current = Interlocked.Exchange(ref _cts, null);
        current?.Cancel();
        current?.Dispose();
    }

    protected async Task ExportCsvAsync()
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
            EscapeCsv($"{axis.Percentile:0.#}%"),
            EscapeCsv(axis.Summary))));

        var player = PositionProfile.SelectedPlayer;
        var playerSlug = Slugify(player.FullName);
        var positionSlug = Slugify(PositionProfile.PositionName ?? player.PositionName ?? "poste");
        var seasonSlug = Slugify(string.IsNullOrWhiteSpace(Season) ? "toutes-saisons" : Season!);
        var fileName = $"profil-poste-{playerSlug}-{positionSlug}-{seasonSlug}.csv";

        await JS.InvokeVoidAsync(
            "handwstatExports.downloadTextFile",
            fileName,
            "text/csv;charset=utf-8",
            string.Join("\n", lines));
    }

    private async Task LoadProfileAsync(CancellationToken ct = default)
    {
        if (!PlayerId.HasValue)
        {
            ClearAnalysisState();
            return;
        }

        var loadStartedAt = DateTimeOffset.UtcNow;

        try
        {
            await BusyUiHelper.EnterAsync(() => IsBusy = true, () => InvokeAsync(StateHasChanged));
            ErrorMessage = null;

            var options = new StatsQueryOptionsDto
            {
                CompetitionId = CompetitionId,
                TeamId = TeamId,
                Season = Season
            };

            PositionProfile = await PlayersApiClient.GetPlayerPositionProfileAsync(PlayerId.Value, options, ct);
            RebuildPositionProfileDerivedState();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Logger.LogWarning(ex, "Failed to load position profile for player {PlayerId}.", PlayerId);
        }
        finally
        {
            await BusyUiHelper.ExitAsync(() => IsBusy = false, () => InvokeAsync(StateHasChanged), loadStartedAt);
        }
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

        var bounds = BuildScatterBounds(orderedAxes);
        PositionProfileAxes = BuildPositionProfileAxisViewModels(orderedAxes, bounds);
        PositionProfileDetailRows = PositionProfileAxes
            .OrderByDescending(axis => axis.Impact)
            .ThenBy(axis => axis.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
        PositionProfileChartRows = PositionProfileAxes
            .Where(axis => !IsRadarHistogramExcludedAxis(axis))
            .ToList();
        PositionProfileChartMedianRadarSeries = PositionProfile.MedianProfile is not null
            ? BuildRadarSeries(PositionProfile.MedianProfile.Axes, PositionProfileChartRows)
            : [];
        PositionProfileScopeItems = BuildScopeSummaryItems();
        PositionProfileInsights = PositionProfileInsightEngine.Build(
            PositionProfile,
            PositionProfileAxes,
            PositionProfileChartRows,
            Season);

        var selected = PositionProfile.SelectedPlayer;
        AnlzHistogramKey = $"anlz-histogram|{selected.PlayerId}|{PositionProfile.PositionId}|{CompetitionId ?? 0}|{Season ?? "all"}|{PositionProfileChartRows.Count}";
        AnlzRadarKey = $"anlz-radar|{selected.PlayerId}|{PositionProfile.PositionId}|{CompetitionId ?? 0}|{Season ?? "all"}|{PositionProfileChartRows.Count}";

        ValidateData();
    }

    private async Task LoadContextAsync(CancellationToken ct = default)
    {
        if (!PlayerId.HasValue)
        {
            EventContexts = null;
            ContextErrorMessage = null;
            return;
        }

        try
        {
            IsContextBusy = true;
            await InvokeAsync(StateHasChanged);
            ContextErrorMessage = null;

            var options = ContextAnalyticsHelper.BuildContextOptions(
                playerId:      PlayerId.Value,
                competitionId: CompetitionId,
                teamId:        TeamId,
                season:        Season,
                day:           Day);

            EventContexts = await StatsApiClient.GetEventContextsAsync(options, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            ContextErrorMessage = ex.Message;
            EventContexts = null;
            Logger.LogWarning(ex, "Failed to load event contexts for player {PlayerId}.", PlayerId);
        }
        finally
        {
            IsContextBusy = false;
            // StateHasChanged is consolidated to a single call in OnParametersSetAsync.
        }
    }

    private async Task LoadClutchAsync(CancellationToken ct = default)
    {
        if (!PlayerId.HasValue)
        {
            ClutchData = null;
            ClutchErrorMessage = null;
            return;
        }

        try
        {
            IsClutchBusy = true;
            await InvokeAsync(StateHasChanged);
            ClutchErrorMessage = null;

            var options = new StatsQueryOptionsDto
            {
                PlayerId = PlayerId.Value,
                CompetitionId = CompetitionId,
                TeamId = TeamId,
                Season = Season
            };

            ClutchData = await StatsApiClient.GetPlayerClutchAsync(PlayerId.Value, options: options, cancellationToken: ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            ClutchErrorMessage = ex.Message;
            ClutchData = null;
            Logger.LogWarning(ex, "Failed to load clutch data for player {PlayerId}.", PlayerId);
        }
        finally
        {
            IsClutchBusy = false;
            // StateHasChanged is consolidated to a single call in OnParametersSetAsync.
        }
    }

    private async Task LoadHalfTimeAsync(CancellationToken ct = default)
    {
        if (!PlayerId.HasValue)
        {
            HalfTimeData = null;
            HalfTimeErrorMessage = null;
            return;
        }

        try
        {
            IsHalfTimeBusy = true;
            await InvokeAsync(StateHasChanged);
            HalfTimeErrorMessage = null;

            var options = new StatsQueryOptionsDto
            {
                PlayerId = PlayerId.Value,
                CompetitionId = CompetitionId,
                TeamId = TeamId,
                Season = Season
            };

            HalfTimeData = await StatsApiClient.GetPlayerHalfTimeBreakdownAsync(PlayerId.Value, options, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            HalfTimeErrorMessage = ex.Message;
            HalfTimeData = null;
            Logger.LogWarning(ex, "Failed to load half-time data for player {PlayerId}.", PlayerId);
        }
        finally
        {
            IsHalfTimeBusy = false;
            // StateHasChanged is consolidated to a single call in OnParametersSetAsync.
        }
    }

    private async Task LoadGkScoreStateAsync(CancellationToken ct = default)
    {
        if (!PlayerId.HasValue || !IsGoalkeeper)
        {
            GkScoreStateData = null;
            GkScoreStateErrorMessage = null;
            return;
        }

        try
        {
            IsGkScoreStateBusy = true;
            await InvokeAsync(StateHasChanged);
            GkScoreStateErrorMessage = null;

            var options = new StatsQueryOptionsDto
            {
                PlayerId = PlayerId.Value,
                CompetitionId = CompetitionId,
                TeamId = TeamId,
                Season = Season
            };

            GkScoreStateData = await StatsApiClient.GetGkScoreStateAsync(PlayerId.Value, options, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            GkScoreStateErrorMessage = ex.Message;
            GkScoreStateData = null;
            Logger.LogWarning(ex, "Failed to load GK score-state data for player {PlayerId}.", PlayerId);
        }
        finally
        {
            IsGkScoreStateBusy = false;
            // StateHasChanged is consolidated to a single call in OnParametersSetAsync.
        }
    }

    private void ClearAnalysisState()
    {
        PositionProfile = null;
        PositionProfileAxes = [];
        PositionProfileDetailRows = [];
        PositionProfileChartRows = [];
        PositionProfileChartMedianRadarSeries = [];
        PositionProfileScopeItems = [];
        PositionProfileInsights = PositionProfileInsightBundle.Empty;
        AnlzHistogramKey = string.Empty;
        AnlzRadarKey = string.Empty;
        EventContexts = null;
        ContextErrorMessage = null;
        ClutchData = null;
        ClutchErrorMessage = null;
        HalfTimeData = null;
        HalfTimeErrorMessage = null;
        GkScoreStateData = null;
        GkScoreStateErrorMessage = null;
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

    private void ValidateData()
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
    }

    // ── Computation helpers (adapted from PositionProfilesBase) ─────────────

    private static PositionProfileScatterBounds BuildScatterBounds(IReadOnlyList<PositionProfileAxisDto> axes)
    {
        if (axes.Count == 0)
        {
            return PositionProfileScatterBounds.Default;
        }

        var values = axes
            .SelectMany(axis => new[] { axis.Value, axis.MedianValue })
            .Where(double.IsFinite)
            .ToArray();

        if (values.Length == 0)
        {
            return PositionProfileScatterBounds.Default;
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
                axis.MaxValue,
                axis.IsEvaluative,
                axis.Rank));
        }

        return result;
    }

    private static Dictionary<string, (double PlayerOffset, double MedianOffset)> BuildScatterOffsets(
        IReadOnlyList<PositionProfileAxisDto> axes,
        double spread)
    {
        var offsets = new Dictionary<string, (double PlayerOffset, double MedianOffset)>(StringComparer.OrdinalIgnoreCase);
        var groups = axes.GroupBy(
            axis => $"{Math.Round(axis.MedianValue, 2):0.##}|{Math.Round(axis.Value, 2):0.##}|{axis.Format}",
            StringComparer.OrdinalIgnoreCase);

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
        // Always use the API percentile — direction-aware, min-max normalization forbidden (A9/A11 §7/§19).
        return Math.Clamp(axis.Percentile, 0d, 100d);
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

    private static bool IsRadarHistogramExcludedAxis(PositionProfileAxisViewModel axis)
    {
        if (string.Equals(axis.Key, "open_shot_success", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(axis.Label?.Trim(), "% jeu", StringComparison.OrdinalIgnoreCase);
    }

    private static int GetScatterDirection(double playerValue, double medianValue, string? format)
    {
        var delta = playerValue - medianValue;
        var tolerance = string.Equals(format, "percent", StringComparison.OrdinalIgnoreCase) ? 0.35d : 0.08d;

        if (Math.Abs(delta) <= tolerance)
        {
            return 0;
        }

        return delta > 0 ? 1 : -1;
    }

    private static string FormatScatterDelta(double delta, string? format)
    {
        return string.Equals(format, "percent", StringComparison.OrdinalIgnoreCase)
            ? $"{delta:+0.#;-0.#;0} pts"
            : $"{delta:+0.##;-0.##;0}";
    }

    private static string FormatPositionProfileAxisValue(double value, string? format)
    {
        return string.Equals(format, "percent", StringComparison.OrdinalIgnoreCase)
            ? $"{value:0.#}%"
            : HandballKpiHelper.FormatNumber(value);
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

    private static string EscapeCsv(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

    private static string Slugify(string value) =>
        string.Join("-", value.Trim().ToLowerInvariant()
            .Split([' ', '/', '\\', '.', ',', ';', ':', '_'], StringSplitOptions.RemoveEmptyEntries));
}

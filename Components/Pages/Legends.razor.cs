using System.Threading;
using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;
using HandWStat.Services;
using HandWStat.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace HandWStat.Components.Pages;

public class LegendsBase : ComponentBase, IDisposable
{
    [Inject]
    protected LegendsService LegendsService { get; set; } = default!;

    [Inject]
    protected IApiAuthService AuthService { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [Inject]
    protected ILogger<LegendsBase> Logger { get; set; } = default!;

    private CancellationTokenSource? _cts;

    protected LegendsSnapshot? Snapshot { get; set; }
    protected bool IsBusy { get; set; }
    protected string? ErrorMessage { get; set; }
    protected string ActiveSection { get; set; } = "scorers";

    // Cached LINQ results — rebuilt once after Snapshot is assigned or section changes.
    private PlayerGlobalStatsDto? _topLegend;
    private IReadOnlyList<PlayerGlobalStatsDto> _podiumScorers = [];
    private IReadOnlyList<PlayerGlobalStatsDto> _wallPlayers = [];
    private IReadOnlyList<PlayerGlobalStatsDto> _activeList = [];

    protected PlayerGlobalStatsDto? TopLegend => _topLegend;
    protected IReadOnlyList<PlayerGlobalStatsDto> PodiumScorers => _podiumScorers;
    protected IReadOnlyList<PlayerGlobalStatsDto> WallPlayers => _wallPlayers;
    protected IReadOnlyList<PlayerGlobalStatsDto> ActiveList => _activeList;

    private void RebuildCache()
    {
        if (Snapshot is null)
        {
            _topLegend = null;
            _podiumScorers = [];
            _wallPlayers = [];
            _activeList = [];
            return;
        }

        _topLegend = Snapshot.Players
            .Where(p => !p.IsGoalkeeper)
            .OrderByDescending(p => p.TotalGoals)
            .FirstOrDefault();

        _podiumScorers = Snapshot.Players
            .Where(p => !p.IsGoalkeeper)
            .OrderByDescending(p => p.TotalGoals)
            .Take(3)
            .ToList();

        RebuildSectionCache();
    }

    private void RebuildSectionCache()
    {
        if (Snapshot is null)
        {
            _wallPlayers = [];
            _activeList = [];
            return;
        }

        _wallPlayers = ActiveSection switch
        {
            "scorers" => Snapshot.Players
                .Where(p => !p.IsGoalkeeper)
                .OrderByDescending(p => p.TotalGoals)
                .Take(20)
                .ToList(),
            "assists" => Snapshot.Players
                .OrderByDescending(p => p.AssistCount)
                .Take(20)
                .ToList(),
            "defense" => Snapshot.Players
                .Where(p => !p.IsGoalkeeper)
                .OrderByDescending(p => p.InterceptionCount)
                .Take(20)
                .ToList(),
            "goalkeepers" => Snapshot.Players
                .Where(p => p.IsGoalkeeper)
                .OrderByDescending(p => p.SaveCount)
                .Take(20)
                .ToList(),
            _ => []
        };

        _activeList = ActiveSection switch
        {
            "scorers" => Snapshot.Players
                .Where(p => !p.IsGoalkeeper)
                .OrderByDescending(p => p.TotalGoals)
                .ToList(),
            "assists" => Snapshot.Players
                .OrderByDescending(p => p.AssistCount)
                .ToList(),
            "defense" => Snapshot.Players
                .Where(p => !p.IsGoalkeeper)
                .OrderByDescending(p => p.InterceptionCount)
                .ToList(),
            "goalkeepers" => Snapshot.Players
                .Where(p => p.IsGoalkeeper)
                .OrderByDescending(p => p.SaveCount)
                .ToList(),
            _ => []
        };
    }

    protected (string Label, Func<PlayerGlobalStatsDto, string> Value, Func<PlayerGlobalStatsDto, string> Sub) ActiveColumnDef =>
        ActiveSection switch
        {
            "scorers" => ("Buts", p => p.TotalGoals.ToString(), p => $"{p.MatchesPlayed} matchs · {p.AssistCount} passes"),
            "assists" => ("Passes", p => p.AssistCount.ToString(), p => $"{p.MatchesPlayed} matchs · {p.TotalGoals} buts"),
            "defense" => ("Interceptions", p => p.InterceptionCount.ToString(), p => $"{p.MatchesPlayed} matchs"),
            "goalkeepers" => ("Arrêts", p => p.SaveCount.ToString(), p => $"{p.MatchesPlayed} matchs · {p.GoalkeeperSaveRate:0.#} %"),
            _ => ("Stats", p => string.Empty, p => string.Empty)
        };

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.Session.IsAuthenticated)
        {
            Navigation.NavigateTo("/");
            return;
        }

        await LoadAsync();
    }

    protected void SetSection(string section)
    {
        ActiveSection = section;
        RebuildSectionCache();
        StateHasChanged();
    }

    private async Task LoadAsync()
    {
        var next = new CancellationTokenSource();
        var previous = Interlocked.Exchange(ref _cts, next);
        previous?.Cancel();
        previous?.Dispose();

        IsBusy = true;
        ErrorMessage = null;
        await InvokeAsync(StateHasChanged);

        try
        {
            Snapshot = await LegendsService.LoadLegendsAsync(next.Token);
            RebuildCache();
        }
        catch (OperationCanceledException) when (next.IsCancellationRequested) { }
        catch (ApiRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            AuthService.Logout();
            Navigation.NavigateTo("/");
            return;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Le Hall of Legends n'est pas disponible pour le moment.";
            Logger.LogError(ex, "Failed to load legends.");
        }
        finally
        {
            if (ReferenceEquals(Interlocked.CompareExchange(ref _cts, null, next), next))
            {
                IsBusy = false;
                await InvokeAsync(StateHasChanged);
            }

            next.Dispose();
        }
    }

    public void Dispose()
    {
        var current = Interlocked.Exchange(ref _cts, null);
        current?.Cancel();
        current?.Dispose();
    }
}

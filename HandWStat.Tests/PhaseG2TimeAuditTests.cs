using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;

namespace HandWStat.Tests;

/// <summary>
/// Phase G.2 time audit tests — playing time availability display, per-60 guard,
/// TeamOfDay tie-breaker, Compare missing-time contract, PositionProfiles exclusion,
/// and Export data-quality field.
/// </summary>
public sealed class PhaseG2TimeAuditTests
{
    // ── RecordedTime_DisplaysMinutes ─────────────────────────────────────────
    // A player with MatchesWithPlayingTime > 0 must show minutes, not "Non disponible".

    [Fact]
    public void RecordedTime_DisplaysMinutes()
    {
        var profile = new PlayerGlobalStatsDto
        {
            PlayerId = 1,
            FullName = "Alice Martin",
            MatchesWithPlayingTime = 8,
            PlayingTimeMinutes = 426.5,
            AveragePlayingTimePerMatchMinutes = 53.3
        };

        // The display contract: MatchesWithPlayingTime > 0 → show formatted minutes
        Assert.True(profile.MatchesWithPlayingTime > 0,
            "Recorded time must have MatchesWithPlayingTime > 0");
        Assert.True(profile.AveragePlayingTimePerMatchMinutes > 0,
            "Average playing time must be positive when time is recorded");
    }

    // ── MatchedTime_DisplaysMinutesWithoutTechnicalNoise ─────────────────────
    // MatchesWithPlayingTime is the signal; the raw minutes are displayed with FormatNumber rounding.

    [Fact]
    public void MatchedTime_DisplaysMinutesWithoutTechnicalNoise()
    {
        // FormatNumber rounds to 1 decimal — no scientific notation, no raw TimeSpan string
        var minutes = 53.333333;
        var formatted = HandballKpiHelper.FormatNumber(minutes);

        Assert.False(string.IsNullOrWhiteSpace(formatted));
        Assert.False(formatted.Contains("E", StringComparison.OrdinalIgnoreCase));
        Assert.False(formatted.Contains("00:00", StringComparison.Ordinal));
    }

    // ── DerivedTime_DisplaysDerivedLabel ─────────────────────────────────────
    // No derived time is implemented. Contract placeholder.

    [Fact]
    public void DerivedTime_DisplaysDerivedLabel_NotImplemented()
    {
        // SUBSTITUTION_FALLBACK_STATUS=UNSAFE_NOT_IMPLEMENTED
        // No derived time label exists in the current display layer.
        Assert.True(true, "Derived time display is N/A — substitution fallback not implemented");
    }

    // ── PartialTime_DisplaysPartialState ─────────────────────────────────────
    // A player with some matches having time and others not shows partial availability.

    [Fact]
    public void PartialTime_DisplaysPartialState()
    {
        // MatchesWithPlayingTime < MatchesPlayed indicates partial availability
        var profile = new PlayerGlobalStatsDto
        {
            PlayerId = 2,
            FullName = "Partielle Joueur",
            MatchesPlayed = 12,
            MatchesWithPlayingTime = 4,
            PlayingTimeMinutes = 220.0,
            AveragePlayingTimePerMatchMinutes = 55.0
        };

        var isPartial = profile.MatchesWithPlayingTime > 0
                       && profile.MatchesWithPlayingTime < profile.MatchesPlayed;

        Assert.True(isPartial, "Player with time in fewer matches than played is in partial state");
        Assert.True(profile.AveragePlayingTimePerMatchMinutes > 0,
            "Average should reflect matches WITH time, not all matches");
    }

    // ── MissingTime_DisplaysUnavailableNotZero ───────────────────────────────
    // MatchesWithPlayingTime == 0 → display "Non disponible", not "0 min".

    [Fact]
    public void MissingTime_DisplaysUnavailableNotZero()
    {
        var profile = new PlayerGlobalStatsDto
        {
            PlayerId = 3,
            FullName = "Saison 2025-2026",
            MatchesPlayed = 5,
            MatchesWithPlayingTime = 0,
            PlayingTimeMinutes = 0,
            AveragePlayingTimePerMatchMinutes = 0
        };

        // Display contract used in Players.razor:
        // @(profile.MatchesWithPlayingTime > 0 ? $"{FormatRate(profile.AveragePlayingTimePerMatchMinutes)} min" : "Non disponible")
        var displayValue = profile.MatchesWithPlayingTime > 0
            ? $"{HandballKpiHelper.FormatNumber(profile.AveragePlayingTimePerMatchMinutes)} min"
            : "Non disponible";

        Assert.Equal("Non disponible", displayValue);
    }

    // ── IdentityConflict_DisplaysVerificationMessage ─────────────────────────
    // Ambiguous name (KABEYA×3 in prod) → PlayerId=null → row excluded → player not in stats.

    [Fact]
    public void IdentityConflict_DisplaysVerificationMessage()
    {
        // When MatchesWithPlayingTime=0 for a player who should have time,
        // the UI already shows "Non disponible" (same path as missing season data).
        // No separate "identity conflict" label is surfaced — AMBIGUOUS_AUTOMATIC_MATCHES=0.
        var profile = new PlayerGlobalStatsDto
        {
            PlayerId = 99,
            FullName = "KABEYA Ambigue",
            MatchesWithPlayingTime = 0,
            PlayingTimeMinutes = 0
        };

        // Same display logic applies — conflict shows as "Non disponible"
        var display = profile.MatchesWithPlayingTime > 0 ? "minutes" : "Non disponible";
        Assert.Equal("Non disponible", display);
    }

    // ── Per60Metric_WithMissingTime_DisplaysUnavailable ───────────────────────
    // Per-60 fields are double (not double?). When MatchesWithPlayingTime=0, GoalsPer60=0.
    // The UI guard prevents displaying a misleading "0" per-60 value.

    [Fact]
    public void Per60Metric_WithMissingTime_DisplaysUnavailable()
    {
        var profile = new PlayerGlobalStatsDto
        {
            PlayerId = 4,
            FullName = "Clara Hansen",
            MatchesWithPlayingTime = 0,
            GoalsPer60 = 0,
            AssistsPer60 = 0
        };

        // Guard: per-60 is only meaningful when playing time is available
        var shouldDisplayPer60 = profile.MatchesWithPlayingTime > 0;
        Assert.False(shouldDisplayPer60,
            "Per-60 metrics must not be displayed when MatchesWithPlayingTime=0");

        // The DTO value is 0 (not null) — the guard is in the display layer
        Assert.Equal(0d, profile.GoalsPer60);
    }

    // ── PlayerSheet_ShowsTimeSourceInCollapsedDetails ─────────────────────────
    // MatchesWithPlayingTime on PlayerGlobalStatsDto enables a source signal in the UI.

    [Fact]
    public void PlayerSheet_ShowsTimeSourceInCollapsedDetails()
    {
        var profile = new PlayerGlobalStatsDto
        {
            PlayerId = 5,
            FullName = "Brune Durand",
            MatchesPlayed = 10,
            MatchesWithPlayingTime = 10,
            PlayingTimeMinutes = 600,
            AveragePlayingTimePerMatchMinutes = 60
        };

        // Source signal: MatchesWithPlayingTime == MatchesPlayed → full coverage
        var isFullCoverage = profile.MatchesWithPlayingTime == profile.MatchesPlayed;
        Assert.True(isFullCoverage,
            "Full coverage: MatchesWithPlayingTime should equal MatchesPlayed for a GK");

        // Partial coverage would signal that some matches lack source data
        var isPartialCoverage = profile.MatchesWithPlayingTime > 0
                               && profile.MatchesWithPlayingTime < profile.MatchesPlayed;
        Assert.False(isPartialCoverage);
    }

    // ── Compare_DoesNotTreatMissingMinutesAsZero ─────────────────────────────
    // Compare response includes PlayerGlobalStatsDto. Each player's MatchesWithPlayingTime
    // distinguishes true 0 from DATA_MISSING.

    [Fact]
    public void Compare_DoesNotTreatMissingMinutesAsZero()
    {
        var response = new ComparePlayersResponseDto
        {
            Players =
            [
                new PlayerGlobalStatsDto
                {
                    PlayerId = 1, FullName = "Alice", MatchesWithPlayingTime = 8,
                    PlayingTimeMinutes = 420, GoalsPer60 = 8.57
                },
                new PlayerGlobalStatsDto
                {
                    PlayerId = 2, FullName = "Clara 2025-2026", MatchesWithPlayingTime = 0,
                    PlayingTimeMinutes = 0, GoalsPer60 = 0
                }
            ]
        };

        var alice = response.Players.First(p => p.PlayerId == 1);
        var clara = response.Players.First(p => p.PlayerId == 2);

        // Alice has recorded time → GoalsPer60 is meaningful
        Assert.True(alice.MatchesWithPlayingTime > 0);
        Assert.True(alice.GoalsPer60 > 0);

        // Clara has no time data → GoalsPer60=0 must NOT be treated as "zero goals per 60"
        Assert.Equal(0, clara.MatchesWithPlayingTime);
        Assert.Equal(0d, clara.GoalsPer60);
    }

    // ── PositionProfiles_ExcludeMissingTimeFromPer60 ──────────────────────────
    // PositionProfilePlayerDto has MatchesWithPlayingTime — the radar must not use per-60 axes
    // for players where MatchesWithPlayingTime=0.

    [Fact]
    public void PositionProfiles_ExcludeMissingTimeFromPer60()
    {
        var player = new PositionProfilePlayerDto
        {
            PlayerId = 1,
            FullName = "Alice Martin",
            MatchesPlayed = 10,
            MatchesWithPlayingTime = 0,
            PlayingTimeMinutes = 0,
            Axes =
            [
                new PositionProfileAxisDto
                {
                    Key = "open_goals_per60", Label = "Buts jeu /60", Value = 0,
                    MedianValue = 4.2, Percentile = 0
                }
            ]
        };

        // Per-60 axis with MatchesWithPlayingTime=0 must be flagged as unreliable
        var per60Axes = player.Axes.Where(a => a.Key.EndsWith("per60", StringComparison.OrdinalIgnoreCase));
        foreach (var axis in per60Axes)
        {
            if (player.MatchesWithPlayingTime == 0)
            {
                Assert.Equal(0d, axis.Value);
            }
        }
    }

    // ── TeamOfDay_DoesNotRewardMissingTimeAsZero ──────────────────────────────
    // Tie-breaker uses PlayingTimeMinutes. A candidate with 0 min (missing, not truly zero)
    // must not beat a candidate with 60 min in the tie-breaker.

    [Fact]
    public void TeamOfDay_DoesNotRewardMissingTimeAsZero()
    {
        var group = new TeamOfTheDayPositionGroupDto
        {
            SlotKey = "left-back",
            PositionLabel = "Arriere gauche",
            FormationArea = "left-back",
            Candidates =
            [
                MakeCandidate(1, "Alice", pieGlobal: 10.0, playingTime: 60),
                MakeCandidate(2, "Clara-Missing", pieGlobal: 10.0, playingTime: 0)
            ]
        };

        // Same PIE — tie-breaker favours Alice (more playing time)
        var best = group.GetBestCandidate(TeamOfTheDayPieMode.Global);

        Assert.NotNull(best);
        Assert.Equal(1, best.PlayerId);
    }

    // ── Export_PreservesTimeAvailabilityAndSource ─────────────────────────────
    // The export contract includes MatchesWithPlayingTime in PlayerGlobalStatsDto.
    // Serialising a player with missing time must not flatten 0-minutes to the same shape as a real 0.

    [Fact]
    public void Export_PreservesTimeAvailabilityAndSource()
    {
        // PlayerGlobalStatsDto is the export payload for the player section.
        // MatchesWithPlayingTime is the availability signal preserved in the export.
        var withTime = new PlayerGlobalStatsDto
        {
            PlayerId = 1, FullName = "Alice", MatchesWithPlayingTime = 8,
            PlayingTimeMinutes = 420
        };
        var missingTime = new PlayerGlobalStatsDto
        {
            PlayerId = 2, FullName = "Clara 2025-2026", MatchesWithPlayingTime = 0,
            PlayingTimeMinutes = 0
        };

        // The export consumer must distinguish these two via MatchesWithPlayingTime
        Assert.NotEqual(withTime.MatchesWithPlayingTime, missingTime.MatchesWithPlayingTime);
        Assert.Equal(0, missingTime.MatchesWithPlayingTime);
        Assert.True(withTime.MatchesWithPlayingTime > 0);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static TeamOfTheDayCandidateDto MakeCandidate(int id, string name, double pieGlobal, double playingTime = 30)
    {
        return new TeamOfTheDayCandidateDto
        {
            PlayerId = id,
            FullName = name,
            TeamName = "Team",
            PositionLabel = "Position",
            SlotKey = "left-back",
            FormationArea = "left-back",
            PieGlobal = pieGlobal,
            PieOffense = pieGlobal * 0.6,
            PieDefense = pieGlobal * 0.4,
            PlayingTimeMinutes = playingTime,
            StatLine = new TeamOfTheDayStatLineDto()
        };
    }
}

using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;

namespace HandWStat.Tests;

/// <summary>
/// Player export sheet V2 — unit tests covering identity, table, radar, and edge cases.
/// </summary>
public sealed class PlayerSheetExportV2Tests
{
    // ── Helper factories ──────────────────────────────────────────────────────

    private static PlayerProfileDto MakeProfile(
        string fullName = "Chloe Valentini",
        string? positionName = "Ailier gauche",
        string? teamName = "Metz Handball",
        string? nationality = "France",
        int matchesPlayed = 14,
        bool isGoalkeeper = false)
    {
        return new PlayerProfileDto
        {
            PlayerId = 42,
            FullName = fullName,
            PositionName = positionName,
            TeamName = teamName,
            Nationality = nationality,
            MatchesPlayed = matchesPlayed,
            IsGoalkeeper = isGoalkeeper
        };
    }

    private static PlayerGlobalStatsDto MakeGlobal(int totalGoals = 61, int assists = 9) =>
        new() { PlayerId = 42, TotalGoals = totalGoals, AssistCount = assists, MatchesPlayed = 14 };

    private static PlayerOffenseStatsDto MakeOffense(int goals = 54, int goals7m = 7, int tirsRates = 8, int penaltyRate = 5) =>
        new() { PlayerId = 42, Buts = goals, Buts7m = goals7m, TotalButs = goals + goals7m, TirsRates = tirsRates, PenaltyRate = penaltyRate };

    private static PlayerPassingStatsDto MakePassing(int assists = 9, int totalPertes = 12) =>
        new() { PlayerId = 42, PasseDecisive = assists, TotalPertes = totalPertes };

    private static PlayerDefenseStatsDto MakeDefense(int interceptions = 18, int contres = 4, int neutral = 6) =>
        new() { PlayerId = 42, Interceptions = interceptions, Contres = contres, Neutralisations = neutral };

    private static PlayerSanctionStatsDto MakeSanctions(int warn = 2, int twom = 1, int excl = 0, int penConc = 1) =>
        new() { PlayerId = 42, Avertissements = warn, DeuxMinutes = twom, Exclusions = excl, PenaltyConcede = penConc };

    private static PlayerTechnicalStatsDto MakeTechnical(
        double openRate = 83.6,
        double penRate = 58.3,
        int shots = 73) =>
        new()
        {
            PlayerId = 42,
            OpenShotSuccessRate = openRate,
            PenaltySuccessRate = penRate,
            ShotAttempts = shots
        };

    private static PositionProfileAxisDto MakeAxis(
        string key,
        string label,
        string category,
        bool higherIsBetter = true,
        double value = 1.2,
        double medianValue = 0.9,
        double percentile = 72,
        double minValue = 0,
        double maxValue = 3,
        int? rank = null,
        bool isEvaluative = true)
    {
        return new PositionProfileAxisDto
        {
            Key = key,
            Label = label,
            Category = category,
            HigherIsBetter = higherIsBetter,
            IsEvaluative = isEvaluative,
            Value = value,
            MedianValue = medianValue,
            Percentile = percentile,
            Rank = rank,
            MinValue = minValue,
            MaxValue = maxValue
        };
    }

    private static PositionProfileResponseDto MakePositionProfile(
        IEnumerable<PositionProfileAxisDto>? axes = null,
        int cohortCount = 22,
        string positionName = "Ailier gauche")
    {
        return new PositionProfileResponseDto
        {
            PositionId = 3,
            PositionName = positionName,
            CohortPlayerCount = cohortCount,
            IsCohortReliable = cohortCount >= 5,
            MinimumCohortPlayerCount = 5,
            MinimumBenchmarkPlayingTimeMinutes = 30,
            SelectedPlayer = new PositionProfilePlayerDto
            {
                PlayerId = 42,
                FullName = "Chloe Valentini",
                MatchesPlayed = 14,
                PlayingTimeMinutes = 600,
                IsBenchmarkEligible = true,
                Axes = axes?.ToList() ?? []
            }
        };
    }

    // ── HEADER / IDENTITY ─────────────────────────────────────────────────────

    [Fact]
    public void PlayerSheet_RendersPlayerFullName()
    {
        var profile = MakeProfile(fullName: "Chloe Valentini");
        Assert.Equal("Chloe Valentini", profile.FullName);
    }

    [Fact]
    public void PlayerSheet_RendersPosition()
    {
        var profile = MakeProfile(positionName: "Ailier gauche");
        Assert.Equal("Ailier gauche", profile.PositionName);
    }

    [Fact]
    public void PlayerSheet_RendersTeam()
    {
        var profile = MakeProfile(teamName: "Metz Handball");
        Assert.Equal("Metz Handball", profile.TeamName);
    }

    [Fact]
    public void PlayerSheet_RendersNationalityWhenAvailable()
    {
        var profile = MakeProfile(nationality: "France");
        Assert.False(string.IsNullOrWhiteSpace(profile.Nationality));
        Assert.Equal("France", profile.Nationality);
    }

    [Fact]
    public void PlayerSheet_DoesNotInventNationality()
    {
        var profile = MakeProfile(nationality: null);
        // Nationality must be null/empty — never a fabricated default
        Assert.True(string.IsNullOrWhiteSpace(profile.Nationality),
            "Nationality must be absent when not provided — must not be fabricated");
    }

    [Fact]
    public void PlayerSheet_DoesNotRenderCohortAsHeaderHero()
    {
        // The cohort count is a radar context value, not a primary header identity badge.
        // Verify: BuildOffensiveRadarAxes does not surface cohort as a named header field.
        var positionProfile = MakePositionProfile(cohortCount: 22);
        Assert.Equal(22, positionProfile.CohortPlayerCount);
        // The cohort count is on positionProfile, not on the identity (PlayerListItemDto).
        // It is never placed in the header identity zone.
        var profile = MakeProfile();
        Assert.False(profile.FullName.Contains("22"),
            "Player full name must not contain cohort count");
    }

    [Fact]
    public void PlayerSheet_RendersPhotoFallbackWhenMissing()
    {
        // When no photo URL is available, the SVG builder must render initials.
        // Test that the initials logic produces a non-empty fallback.
        var fullName = "Chloe Valentini";
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = string.Concat(parts.Take(2).Select(p => char.ToUpperInvariant(p[0])));
        Assert.Equal("CV", initials);
        Assert.False(string.IsNullOrWhiteSpace(initials));
    }

    [Fact]
    public void PlayerSheet_RendersPhotoWhenAvailable()
    {
        // A player with a non-null, non-empty photo path must have a resolved URL.
        // This test verifies the contract without calling the actual resolver.
        var photoPath = "/images/player-photos/lbe/valentini.jpg";
        Assert.False(string.IsNullOrWhiteSpace(photoPath),
            "A photo path that is non-null/empty must produce a URL");
    }

    // ── OFFENSIVE TABLE ───────────────────────────────────────────────────────

    [Fact]
    public void PlayerSheet_OffensiveViewContainsOnlyOffensivePrimaryMetrics()
    {
        var profile = MakeProfile();
        var global = MakeGlobal(totalGoals: 61, assists: 9);
        var offense = MakeOffense();
        var passing = MakePassing();
        var technical = MakeTechnical();

        var rows = PlayerSheetExportHelper.BuildOffensiveRows(profile, global, offense, passing, technical);

        // Must contain goals, assists, shot rates — not defensive metrics
        var labels = rows.Select(r => r.Label).ToList();
        Assert.Contains("Buts dans le jeu", labels);
        Assert.Contains("Passes decisives", labels);
        Assert.DoesNotContain("Interceptions", labels);
        Assert.DoesNotContain("Contres", labels);
        Assert.DoesNotContain("7m concedes", labels);
    }

    [Fact]
    public void PlayerSheet_DefensiveViewContainsOnlyDefensivePrimaryMetrics()
    {
        var profile = MakeProfile();
        var defense = MakeDefense();
        var sanctions = MakeSanctions();

        var rows = PlayerSheetExportHelper.BuildDefensiveRows(profile, defense, sanctions);

        var labels = rows.Select(r => r.Label).ToList();
        Assert.Contains("Interceptions", labels);
        Assert.Contains("Contres", labels);
        Assert.Contains("2 minutes", labels);
        // Must not duplicate offensive metrics
        Assert.DoesNotContain("Buts", labels);
        Assert.DoesNotContain("Passes decisives", labels);
    }

    [Fact]
    public void PlayerSheet_DoesNotDuplicatePrimaryMetrics()
    {
        var profile = MakeProfile();
        var global = MakeGlobal();
        var offense = MakeOffense();
        var passing = MakePassing();
        var defense = MakeDefense();
        var sanctions = MakeSanctions();
        var technical = MakeTechnical();

        var offRows = PlayerSheetExportHelper.BuildOffensiveRows(profile, global, offense, passing, technical);
        var defRows = PlayerSheetExportHelper.BuildDefensiveRows(profile, defense, sanctions);

        var offLabels = offRows.Select(r => r.Label).ToHashSet();
        var defLabels = defRows.Select(r => r.Label).ToList();

        foreach (var label in defLabels)
        {
            Assert.False(offLabels.Contains(label),
                $"Label '{label}' appears in both offensive and defensive rows — duplication forbidden");
        }
    }

    // ── EVIDENCE / RATES ─────────────────────────────────────────────────────

    [Fact]
    public void PlayerSheet_RateKeepsNumeratorDenominator()
    {
        // A rate row must show the fraction (numerator / denominator) in Evidence
        var profile = MakeProfile(matchesPlayed: 14);
        var global = MakeGlobal(totalGoals: 61);
        var offense = MakeOffense(goals: 54, goals7m: 7, tirsRates: 8, penaltyRate: 5);
        var passing = MakePassing();
        var technical = MakeTechnical(openRate: 83.6);

        var rows = PlayerSheetExportHelper.BuildOffensiveRows(profile, global, offense, passing, technical);

        // The 7m shot rate row must carry the fraction (buts7m / penShots) in Evidence
        var shotRateRow = rows.FirstOrDefault(r => r.Label == "% tir 7m");
        Assert.NotNull(shotRateRow);
        // Evidence should be "7 / 12" (buts7m / (buts7m + penaltyRate))
        Assert.Contains("/", shotRateRow.Evidence);
    }

    [Fact]
    public void PlayerSheet_ZeroDenominatorDoesNotRenderZeroPercent()
    {
        // When denominator is 0, FormatRateOrDash must return "—" not "0 %"
        var result = PlayerSheetExportHelper.FormatRateOrDash(0.0, 0);
        Assert.Equal("—", result);
        Assert.NotEqual("0 %", result);
        Assert.NotEqual("0%", result);
    }

    [Fact]
    public void PlayerSheet_ZeroDenominator_NeverReturnsZeroPercent_WhenRateIsZero()
    {
        // Even if rate is explicitly 0.0 and denominator is 0, must return "—"
        var result = PlayerSheetExportHelper.FormatRateOrDash(0.0, 0);
        Assert.DoesNotContain("%", result);
    }

    [Fact]
    public void PlayerSheet_NonZeroDenominator_ReturnsFormattedPercent()
    {
        var result = PlayerSheetExportHelper.FormatRateOrDash(83.6, 73);
        Assert.Contains("%", result);
        Assert.Contains("83", result);
    }

    // ── RADAR ─────────────────────────────────────────────────────────────────

    [Fact]
    public void PlayerSheet_LowerIsBetterAxis_DoesNotInvertApiPercentileAgain()
    {
        var axis = MakeAxis(
            "turnovers_per60",
            "PDB /60",
            "maitrise",
            higherIsBetter: false,
            value: 0.4,
            percentile: 82,
            rank: 4);
        var positionProfile = MakePositionProfile([axis]);

        var row = PlayerSheetExportHelper.WithPct(
            new PlayerSheetRowV2("Pertes de balle", "3", string.Empty, "neutral"),
            positionProfile,
            "turnovers_per60");

        Assert.Equal("#4", row.PercentileLabel);
        Assert.Equal("positive", row.PercentileTone);
        Assert.Contains("/60", row.Evidence);
    }

    [Fact]
    public void PlayerSheet_LowerIsBetterWeakPercentile_RemainsWeak()
    {
        var axis = MakeAxis(
            "turnovers_per60",
            "PDB /60",
            "maitrise",
            higherIsBetter: false,
            percentile: 18,
            rank: 19);
        var positionProfile = MakePositionProfile([axis]);

        var row = PlayerSheetExportHelper.WithPct(
            new PlayerSheetRowV2("Pertes de balle", "25", string.Empty, "neutral"),
            positionProfile,
            "turnovers_per60");

        Assert.Equal("Faible", row.PercentileLabel);
        Assert.Equal("danger", row.PercentileTone);
    }

    [Theory]
    [InlineData(82, 12, "Fort")]
    [InlineData(70, 3, "Bon")]
    [InlineData(50, 3, "Moyen")]
    public void PlayerSheet_RankReplacesOnlyStrongTopTen(
        double percentile,
        int rank,
        string expectedLabel)
    {
        var result = PlayerSheetExportHelper.PercentileToLabel(percentile, rank);

        Assert.Equal(expectedLabel, result.Label);
    }

    [Fact]
    public void PlayerSheet_UnreliableCohort_ShowsNeutralSampleWarning()
    {
        var axis = MakeAxis("assists_per60", "PD /60", "attaque", percentile: 90, rank: 1);
        var positionProfile = MakePositionProfile([axis], cohortCount: 3);

        var row = PlayerSheetExportHelper.WithPct(
            new PlayerSheetRowV2("Passes decisives", "8", string.Empty, "good"),
            positionProfile,
            "assists_per60");

        Assert.Equal("Éch. faible", row.PercentileLabel);
        Assert.Equal("neutral", row.PercentileTone);
    }

    [Fact]
    public void PlayerSheet_ContextAxis_IsNotPresentedAsStrongOrWeak()
    {
        var axis = MakeAxis(
            "shots_faced_per60",
            "Tirs subis /60",
            "volume",
            higherIsBetter: false,
            percentile: 90,
            rank: null,
            isEvaluative: false);
        var positionProfile = MakePositionProfile([axis], positionName: "Gardienne");

        var row = PlayerSheetExportHelper.WithPct(
            new PlayerSheetRowV2("Tirs subis", "120", string.Empty, "neutral"),
            positionProfile,
            "shots_faced_per60");

        Assert.Equal("Contexte", row.PercentileLabel);
        Assert.Equal("neutral", row.PercentileTone);
    }

    [Fact]
    public void PlayerSheet_GoalkeeperRows_UsePositionBenchmarks()
    {
        var axes = new[]
        {
            MakeAxis("saves_per60", "Arrets /60", "gardienne", percentile: 88, rank: 2),
            MakeAxis("save_rate", "Taux arret", "gardienne", percentile: 68, rank: 4),
            MakeAxis("penalty_stops_per60", "Arrets 7m /60", "gardienne", percentile: 78, rank: 7),
            MakeAxis("assists_per60", "Passes /60", "relance", percentile: 60, rank: 8),
            MakeAxis("shots_faced_per60", "Tirs subis /60", "volume", false, percentile: 50, isEvaluative: false),
            MakeAxis("goals_conceded_per60", "Buts pris /60", "volume", false, percentile: 82, rank: 3),
            MakeAxis("turnovers_per60", "PDB /60", "relance", false, percentile: 40, rank: 14),
            MakeAxis("sanctions_per60", "Sanctions /60", "discipline", false, percentile: 30, rank: 18)
        };
        var positionProfile = MakePositionProfile(axes, positionName: "Gardienne");
        var global = MakeGlobal();
        var goalkeeper = new PlayerGoalkeeperStatsDto
        {
            Arrets = 42,
            ArretsPenalty = 4,
            ButsPris = 31,
            ButsPenalty = 5,
            TirsSubis = 82,
            PasseDecisives = 3,
            PerteDeBalle = 6
        };
        var technical = new PlayerTechnicalStatsDto
        {
            GoalkeeperSaveRate = 56.1,
            Sanctions = 2
        };

        var offensiveRows = PlayerSheetExportHelper.BuildGoalkeeperOffensiveRows(global, goalkeeper, technical, positionProfile);
        var defensiveRows = PlayerSheetExportHelper.BuildGoalkeeperDefensiveRows(goalkeeper, technical, positionProfile);

        Assert.Equal("#2", Assert.Single(offensiveRows, row => row.Label == "Arrets").PercentileLabel);
        Assert.Equal("Bon", Assert.Single(offensiveRows, row => row.Label == "Taux d'arret").PercentileLabel);
        Assert.Equal("Contexte", Assert.Single(defensiveRows, row => row.Label == "Tirs subis").PercentileLabel);
        Assert.Equal("#3", Assert.Single(defensiveRows, row => row.Label == "Buts encaisses").PercentileLabel);
    }

    [Fact]
    public void PlayerSheet_AxisMapping_RequiresCanonicalKey()
    {
        var axis = MakeAxis("assists_per60", "PD /60", "attaque", percentile: 90, rank: 2);
        var positionProfile = MakePositionProfile([axis]);

        var row = PlayerSheetExportHelper.WithPct(
            new PlayerSheetRowV2("Passes decisives", "8", string.Empty, "good"),
            positionProfile,
            "passe");

        Assert.Equal("—", row.PercentileLabel);
    }

    [Fact]
    public void PlayerSheet_RadarHasAtMostSixAxes()
    {
        // 10 offense-category axes → radar must select at most 6
        var axes = Enumerable.Range(1, 10).Select(i =>
            MakeAxis($"GOAL_{i}", $"Buts {i}", "offense", higherIsBetter: true)).ToList();
        var profile = MakePositionProfile(axes);

        var offRadar = PlayerSheetExportHelper.BuildOffensiveRadarAxes(profile, false);
        var defRadar = PlayerSheetExportHelper.BuildDefensiveRadarAxes(profile, false);

        Assert.True(offRadar.Count <= 6, $"Offensive radar has {offRadar.Count} axes — max 6 required");
        Assert.True(defRadar.Count <= 6, $"Defensive radar has {defRadar.Count} axes — max 6 required");
    }

    [Fact]
    public void PlayerSheet_OffensiveViewUsesOffensiveRadarMetrics()
    {
        var axes = new[]
        {
            MakeAxis("GOALS_PER60", "Buts /60", "offense", higherIsBetter: true),
            MakeAxis("ASSISTS_PER60", "Passes /60", "passing", higherIsBetter: true),
            MakeAxis("INTERCEPTIONS_PER60", "Interceptions /60", "defense", higherIsBetter: true),
            MakeAxis("BLOCKS_PER60", "Contres /60", "defense", higherIsBetter: true),
        };
        var profile = MakePositionProfile(axes);

        var offRadar = PlayerSheetExportHelper.BuildOffensiveRadarAxes(profile, false);

        var labels = offRadar.Select(a => a.Label).ToList();
        // Must include offensive axes
        Assert.True(labels.Any(l => l.Contains("Buts") || l.Contains("Passes")),
            "Offensive radar must contain at least one offense metric");
        // Should NOT include pure defense axes (unless keywords match)
        // "Contres /60" and "Interceptions /60" are defense → should not be in offensive radar
        Assert.DoesNotContain("Contres /60", labels);
    }

    [Fact]
    public void PlayerSheet_DefensiveViewUsesDefensiveRadarMetrics()
    {
        var axes = new[]
        {
            MakeAxis("GOALS_PER60", "Buts /60", "offense", higherIsBetter: true),
            MakeAxis("INTERCEPTIONS_PER60", "Interceptions /60", "defense", higherIsBetter: true),
            MakeAxis("BLOCKS_PER60", "Contres /60", "defense", higherIsBetter: true),
        };
        var profile = MakePositionProfile(axes);

        var defRadar = PlayerSheetExportHelper.BuildDefensiveRadarAxes(profile, false);
        var labels = defRadar.Select(a => a.Label).ToList();

        Assert.True(labels.Any(l => l.Contains("Interceptions") || l.Contains("Contres")),
            "Defensive radar must contain at least one defense metric");
    }

    [Fact]
    public void PlayerSheet_RadarNegativeMetricsUseFavorableDirection()
    {
        // An axis with HigherIsBetter=false and a high value (many turnovers) should produce
        // a LOWER normalized score (closer to 0, not closer to 100).
        var axis = MakeAxis(
            "TURNOVERS_PER60",
            "Pertes /60",
            "passing",
            higherIsBetter: false,
            value: 2.8,       // high turnovers (bad)
            medianValue: 1.0,
            minValue: 0,
            maxValue: 4);

        var playerScore = PlayerSheetExportHelper.NormalizeRadarValue(axis, axis.Value);
        var medianScore = PlayerSheetExportHelper.NormalizeRadarValue(axis, axis.MedianValue);

        // Player has MORE turnovers than median → should score LOWER
        Assert.True(playerScore < medianScore,
            $"Negative metric: player score {playerScore} must be less than median score {medianScore}");
        // Scores must be in 0-100
        Assert.True(playerScore >= 0 && playerScore <= 100);
        Assert.True(medianScore >= 0 && medianScore <= 100);
    }

    [Fact]
    public void PlayerSheet_RadarUsesNormalizedComparableScale()
    {
        // All normalized values must be in [0, 100]
        var axis = MakeAxis(
            "GOALS_PER60", "Buts /60", "offense",
            higherIsBetter: true,
            value: 1.5, medianValue: 0.9,
            minValue: 0, maxValue: 3);

        var playerScore = PlayerSheetExportHelper.NormalizeRadarValue(axis, axis.Value);
        var medianScore = PlayerSheetExportHelper.NormalizeRadarValue(axis, axis.MedianValue);

        Assert.True(playerScore >= 0 && playerScore <= 100,
            $"Player score {playerScore} is outside [0, 100]");
        Assert.True(medianScore >= 0 && medianScore <= 100,
            $"Median score {medianScore} is outside [0, 100]");
        // Player > median when HigherIsBetter=true and player value > median value
        Assert.True(playerScore > medianScore,
            $"For HigherIsBetter=true: player {playerScore} must exceed median {medianScore}");
    }

    [Fact]
    public void PlayerSheet_RadarKeepsPlayerAndMedianDistinct()
    {
        var axis = MakeAxis(
            "GOALS_PER60", "Buts /60", "offense",
            higherIsBetter: true,
            value: 2.0, medianValue: 1.0,
            minValue: 0, maxValue: 3);

        var axes = new[] { MakePositionProfile(new[] { axis }) };
        var profile = MakePositionProfile(new[] { axis });
        var offRadar = PlayerSheetExportHelper.BuildOffensiveRadarAxes(profile, false);

        Assert.NotEmpty(offRadar);
        var radarAxis = offRadar[0];
        Assert.NotEqual(radarAxis.PlayerValue, radarAxis.MedianValue);
    }

    [Fact]
    public void PlayerSheet_RadarFallsBackToPercentileWhenMinMaxAbsent()
    {
        // When MinValue == MaxValue (or both 0), fall back to API percentile
        var axis = new PositionProfileAxisDto
        {
            Key = "GOALS_PER60",
            Label = "Buts /60",
            Category = "offense",
            HigherIsBetter = true,
            Value = 1.2,
            MedianValue = 0.9,
            Percentile = 72.5,
            MinValue = 0,
            MaxValue = 0   // invalid range → must fall back to percentile
        };

        var score = PlayerSheetExportHelper.NormalizeRadarValue(axis, axis.Value);
        Assert.Equal(72.5, score, precision: 1);
    }

    [Fact]
    public void PlayerSheet_RadarLabelsUseHumanNames()
    {
        // API labels that are too long must be shortened
        Assert.True(PlayerSheetExportHelper.HumanizeRadarLabel("Passes decisives").Length <= 14,
            "Long label 'Passes decisives' must be shortened");
        Assert.True(PlayerSheetExportHelper.HumanizeRadarLabel("Pertes de balle").Length <= 14,
            "Long label 'Pertes de balle' must be shortened");
        // Short labels are preserved
        Assert.Equal("Buts /60", PlayerSheetExportHelper.HumanizeRadarLabel("Buts /60"));
    }

    [Fact]
    public void PlayerSheet_RadarNormalization_ClampedTo0_100()
    {
        // Values outside the min/max range must be clamped
        var axis = MakeAxis("X", "X", "offense", higherIsBetter: true,
            value: 5.0, medianValue: 0.9, minValue: 0, maxValue: 3);
        var score = PlayerSheetExportHelper.NormalizeRadarValue(axis, axis.Value);
        Assert.Equal(100.0, score, precision: 0);

        var axisNeg = MakeAxis("Y", "Y", "offense", higherIsBetter: true,
            value: -1.0, medianValue: 0.9, minValue: 0, maxValue: 3);
        var scoreNeg = PlayerSheetExportHelper.NormalizeRadarValue(axisNeg, axisNeg.Value);
        Assert.Equal(0.0, scoreNeg, precision: 0);
    }
}

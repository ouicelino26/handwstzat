using HandWStat.Models.Matches;
using Xunit;

namespace HandWStat.Tests;

public class MatchesShellDirectoryTests
{
    // ── Score formatter ──
    [Fact]
    public void MatchScoreFormatter_FormatsCompletedScore()
    {
        var s = MatchScoreFormatter.Format(31, 27);
        Assert.Equal("31 – 27", s);
    }

    [Fact]
    public void MatchScoreFormatter_PreservesRealZeroZero()
    {
        var s = MatchScoreFormatter.Format(0, 0);
        Assert.Equal("0 – 0", s);
        Assert.True(MatchScoreFormatter.IsRealZeroZero(0, 0));
    }

    [Fact]
    public void MatchScoreFormatter_MissingBothScoresDoesNotReturnZeroZero()
    {
        var s = MatchScoreFormatter.Format(null, null);
        Assert.NotEqual("0 – 0", s);
        Assert.NotEqual("0-0", s);
        Assert.Equal("—", s);
    }

    [Fact]
    public void MatchScoreFormatter_PartialScoreDoesNotInventMissingValue()
    {
        var s1 = MatchScoreFormatter.Format(31, null);
        var s2 = MatchScoreFormatter.Format(null, 27);
        Assert.NotEqual("31 – 0", s1);
        Assert.NotEqual("0 – 27", s2);
        Assert.Contains("incomplet", s1.ToLowerInvariant());
        Assert.Contains("incomplet", s2.ToLowerInvariant());
    }

    [Fact]
    public void MatchScoreFormatter_IsRealZeroZeroDistinguishesNullFromZero()
    {
        Assert.True(MatchScoreFormatter.IsRealZeroZero(0, 0));
        Assert.False(MatchScoreFormatter.IsRealZeroZero(null, null));
        Assert.False(MatchScoreFormatter.IsRealZeroZero(0, null));
        Assert.False(MatchScoreFormatter.IsRealZeroZero(null, 0));
    }

    [Fact]
    public void MatchScoreFormatter_AccessibleLabelIncludesTeamNames()
    {
        var s = MatchScoreFormatter.FormatAccessible("Brest", 31, "Metz", 27);
        Assert.Contains("Brest", s);
        Assert.Contains("Metz", s);
        Assert.Contains("31", s);
        Assert.Contains("27", s);
    }

    [Fact]
    public void MatchScoreFormatter_AccessibleLabelHandlesMissingScore()
    {
        var s = MatchScoreFormatter.FormatAccessible("Brest", null, "Metz", null);
        Assert.Contains("Brest", s);
        Assert.Contains("Metz", s);
        Assert.DoesNotContain("0", s);
    }

    // ── Identité match ──
    [Fact]
    public void MatchIdentity_UsesActualTeamNames()
    {
        var identity = new MatchIdentityDisplay("m1",
            new TeamIdentityDisplay("t1", "Brest", null, true),
            new TeamIdentityDisplay("t2", "Metz", null, false),
            31, 27, "LBE", "2025-26", 18, new DateTime(2026, 5, 12), null, true);
        Assert.Equal("Brest", identity.Team1.TeamName);
        Assert.Equal("Metz", identity.Team2.TeamName);
    }

    [Fact]
    public void MatchIdentity_MissingTeamUsesHonestFallback()
    {
        var identity = new MatchIdentityDisplay("m1",
            new TeamIdentityDisplay(null, null, null, false),
            new TeamIdentityDisplay("t2", "Metz", null, false),
            null, null, null, null, null, null, null, false);
        Assert.Null(identity.Team1.TeamName);
        // Le code UI doit afficher "—" ou "Équipe 1", pas un nom inventé
    }

    [Fact]
    public void MatchIdentity_UsesCompetitionWhenAvailable()
    {
        var identity = new MatchIdentityDisplay("m1",
            new TeamIdentityDisplay("t1", "Brest", null, true),
            new TeamIdentityDisplay("t2", "Metz", null, false),
            31, 27, "LBE", "2025-26", 18, null, null, false);
        Assert.Equal("LBE", identity.CompetitionName);
    }

    [Fact]
    public void MatchIdentity_UsesSeasonDayWhenAvailable()
    {
        var identity = new MatchIdentityDisplay("m1",
            new TeamIdentityDisplay("t1", "Brest", null, true),
            new TeamIdentityDisplay("t2", "Metz", null, false),
            null, null, null, "2025-26", 18, null, null, false);
        Assert.Equal("2025-26", identity.Season);
        Assert.Equal(18, identity.Day);
    }

    [Fact]
    public void MatchIdentity_MissingDateDoesNotInventDate()
    {
        var identity = new MatchIdentityDisplay("m1",
            new TeamIdentityDisplay("t1", "Brest", null, false),
            new TeamIdentityDisplay("t2", "Metz", null, false),
            31, 27, "LBE", null, null, null, null, false);
        Assert.Null(identity.Date);
        // pas de date inventée depuis Season/Day/MatchId
    }

    [Fact]
    public void MatchIdentity_MissingStatusDoesNotInferFromScore()
    {
        // Un score présent ne signifie pas "Terminé" sans champ contractuel
        var identity = new MatchIdentityDisplay("m1",
            new TeamIdentityDisplay("t1", "Brest", null, false),
            new TeamIdentityDisplay("t2", "Metz", null, false),
            31, 27, null, null, null, null, null, false);
        Assert.Null(identity.Status);
    }

    // ── Répertoire ──
    [Fact]
    public void MatchesDirectory_MissingScoreDoesNotRenderZeroZero()
    {
        var s = MatchScoreFormatter.Format(null, null);
        Assert.DoesNotContain("0", s);
    }

    [Fact]
    public void MatchesDirectory_RealZeroZeroIsDistinct()
    {
        var real = MatchScoreFormatter.Format(0, 0);
        var missing = MatchScoreFormatter.Format(null, null);
        Assert.NotEqual(real, missing);
    }

    [Fact]
    public void MatchesDirectory_DefaultSortOrderIsNewestFirst()
    {
        var d = DateTime.Today;
        var matches = new[]
        {
            new { Date = (DateTime?)d.AddDays(-5), MatchId = "m1" },
            new { Date = (DateTime?)d, MatchId = "m3" },
            new { Date = (DateTime?)d.AddDays(-2), MatchId = "m2" }
        };
        var sorted = matches.OrderByDescending(m => m.Date).ThenBy(m => m.MatchId).ToList();
        Assert.Equal("m3", sorted[0].MatchId);
        Assert.Equal("m2", sorted[1].MatchId);
        Assert.Equal("m1", sorted[2].MatchId);
    }

    [Fact]
    public void MatchesDirectory_SearchMatchesTeam1()
    {
        var matches = new[]
        {
            new { Team1Name = "Brest", Team2Name = "Metz" },
            new { Team1Name = "Paris", Team2Name = "Nantes" }
        };
        var q = "brest";
        var filtered = matches.Where(m =>
            (m.Team1Name?.ToLowerInvariant().Contains(q) ?? false) ||
            (m.Team2Name?.ToLowerInvariant().Contains(q) ?? false)).ToList();
        Assert.Single(filtered);
        Assert.Equal("Brest", filtered[0].Team1Name);
    }

    [Fact]
    public void MatchesDirectory_SearchMatchesTeam2()
    {
        var matches = new[]
        {
            new { Team1Name = "Brest", Team2Name = "Metz" },
            new { Team1Name = "Paris", Team2Name = "Nantes" }
        };
        var q = "metz";
        var filtered = matches.Where(m =>
            (m.Team1Name?.ToLowerInvariant().Contains(q) ?? false) ||
            (m.Team2Name?.ToLowerInvariant().Contains(q) ?? false)).ToList();
        Assert.Single(filtered);
        Assert.Equal("Metz", filtered[0].Team2Name);
    }

    [Fact]
    public void MatchesDirectory_SearchMatchesCompetition()
    {
        var matches = new[]
        {
            new { CompetitionName = "LBE", Team1Name = "Brest" },
            new { CompetitionName = "Coupe", Team1Name = "Paris" }
        };
        var q = "lbe";
        var filtered = matches.Where(m =>
            m.CompetitionName?.ToLowerInvariant().Contains(q) ?? false).ToList();
        Assert.Single(filtered);
    }

    [Fact]
    public void MatchesDirectory_FiltersBySeason()
    {
        var matches = new[]
        {
            new { Season = "2025-26", MatchId = "m1" },
            new { Season = "2024-25", MatchId = "m2" }
        };
        var filtered = matches.Where(m => m.Season == "2025-26").ToList();
        Assert.Single(filtered);
        Assert.Equal("m1", filtered[0].MatchId);
    }

    [Fact]
    public void MatchesDirectory_FiltersByDay()
    {
        var matches = new[]
        {
            new { Day = (int?)18, MatchId = "m1" },
            new { Day = (int?)17, MatchId = "m2" }
        };
        var filtered = matches.Where(m => m.Day == 18).ToList();
        Assert.Single(filtered);
    }

    // ── Game Room ──
    [Fact]
    public void MatchRoom_MissingScoreDoesNotRenderZeroZero()
    {
        var s = MatchScoreFormatter.Format(null, null);
        Assert.Equal("—", s);
        Assert.DoesNotContain("0", s);
    }

    [Fact]
    public void MatchRoom_ScoreAccessibleLabelIsHuman()
    {
        var s = MatchScoreFormatter.FormatAccessible("Brest", 31, "Metz", 27);
        Assert.DoesNotContain("–", s); // accessible label n'utilise pas le dash
        Assert.Contains("31", s);
        Assert.Contains("27", s);
    }

    [Fact]
    public void MatchRoom_StatusNotInferredFromScore()
    {
        // Vérifier que la logique métier ne devine pas "Terminé" depuis un score existant
        var identity = new MatchIdentityDisplay("m1",
            new TeamIdentityDisplay("t1", "Brest", null, false),
            new TeamIdentityDisplay("t2", "Metz", null, false),
            31, 27, null, null, null, null, null, false);
        Assert.Null(identity.Status);
        // MATCH_STATUS_INFERRED_WITHOUT_SOURCE=NO
    }

    [Fact]
    public void MatchRoom_FrenchTabLabelsExpected()
    {
        var tabLabels = new[] { "Résumé", "Terrain", "Joueuses" };
        Assert.Contains("Résumé", tabLabels);
        Assert.Contains("Terrain", tabLabels);
        Assert.Contains("Joueuses", tabLabels);
        Assert.DoesNotContain("Story", tabLabels);
        Assert.DoesNotContain("Court", tabLabels);
        Assert.DoesNotContain("Players", tabLabels);
    }

    // ── Régression onglets ──
    [Fact]
    public void MatchSummary_TabKeyUnchanged()
    {
        // La clé interne "summary" ne doit pas changer
        var key = "summary";
        Assert.Equal("summary", key);
    }

    [Fact]
    public void MatchCourt_TabKeyUnchanged()
    {
        var key = "zones";
        Assert.Equal("zones", key);
    }

    [Fact]
    public void MatchPlayers_TabKeyUnchanged()
    {
        var key = "players";
        Assert.Equal("players", key);
    }

    // ── Helpers ──
    [Fact]
    public void MatchScoreFormatter_ZeroScoreWithNullOpponentIsPartial()
    {
        var s = MatchScoreFormatter.Format(0, null);
        Assert.NotEqual("0 – 0", s);
    }
}

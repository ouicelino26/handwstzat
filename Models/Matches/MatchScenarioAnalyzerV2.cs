using HandWStat.Models.Analytics;

namespace HandWStat.Models.Matches;

/// <summary>
/// Derives scenario context KPIs and key moments from a pre-built score timeline.
/// Works on top of the existing MatchScenarioAnalyzer.BuildScoreTimeline output.
/// </summary>
public static class MatchScenarioAnalyzerV2
{
    /// <summary>
    /// Derives MatchScenarioData from an existing score timeline (ScoreTimelinePoint list).
    /// </summary>
    public static MatchScenarioData Analyze(
        IReadOnlyList<ScoreTimelinePoint> timeline,
        string? team1Name,
        string? team2Name)
    {
        var team1Label = string.IsNullOrWhiteSpace(team1Name) ? "Équipe 1" : team1Name;
        var team2Label = string.IsNullOrWhiteSpace(team2Name) ? "Équipe 2" : team2Name;

        if (timeline.Count < 2)
        {
            return BuildEmptyData(timeline);
        }

        var halftimeKpi = BuildHalfTimeScore(timeline);
        var maxLeadKpi = BuildMaxLead(timeline, team1Label, team2Label);
        var leadChangesKpi = BuildLeadChanges(timeline);
        var tiesKpi = BuildMeaningfulTies(timeline);
        var topRunKpi = BuildTopRun(timeline, team1Label, team2Label);
        var keyMoments = BuildKeyMoments(timeline, team1Label, team2Label);

        // Map ScoreTimelinePoint to MatchScenarioTimelinePoint for the model
        var scenarioTimeline = timeline.Select(p => new MatchScenarioTimelinePoint(
            (int)Math.Floor(p.Minute),
            (int)Math.Round((p.Minute - Math.Floor(p.Minute)) * 60),
            p.Team1Score,
            p.Team2Score,
            null,
            null
        )).ToList();

        return new MatchScenarioData(
            scenarioTimeline,
            halftimeKpi,
            maxLeadKpi,
            leadChangesKpi,
            tiesKpi,
            topRunKpi,
            keyMoments);
    }

    private static MatchScenarioData BuildEmptyData(IReadOnlyList<ScoreTimelinePoint> timeline)
    {
        var empty = new MatchContextKpi("—", "—", null, false);
        return new MatchScenarioData(
            [],
            null,
            empty,
            empty,
            empty,
            empty,
            []);
    }

    private static MatchContextKpi? BuildHalfTimeScore(IReadOnlyList<ScoreTimelinePoint> timeline)
    {
        var ht = timeline.Where(p => p.Minute <= 30.5d).LastOrDefault();
        if (ht == null) return null;

        return new MatchContextKpi(
            "Mi-temps",
            $"{ht.Team1Score} – {ht.Team2Score}",
            null,
            true);
    }

    private static MatchContextKpi BuildMaxLead(
        IReadOnlyList<ScoreTimelinePoint> timeline,
        string team1Label,
        string team2Label)
    {
        // MaxLead = max(abs(team1-team2)) across all timeline points
        var maxLeadPoint = timeline.OrderByDescending(p => Math.Abs(p.Team1Score - p.Team2Score)).First();
        var maxLead = Math.Abs(maxLeadPoint.Team1Score - maxLeadPoint.Team2Score);

        if (maxLead == 0)
        {
            return new MatchContextKpi("Plus gros écart", "0", "Match serré", true);
        }

        var leadingTeam = maxLeadPoint.Team1Score > maxLeadPoint.Team2Score ? team1Label : team2Label;
        return new MatchContextKpi(
            "Plus gros écart",
            $"+{maxLead}",
            leadingTeam,
            true);
    }

    private static MatchContextKpi BuildLeadChanges(IReadOnlyList<ScoreTimelinePoint> timeline)
    {
        // LeadChange = sign(diff) flips across non-zero values
        // positive->0->positive = NOT a lead change (same leader returns)
        // positive->0->negative = IS a lead change (leader changes)
        // positive->negative (without 0) = IS a lead change
        var count = 0;
        var previousLeader = 0;

        foreach (var point in timeline.Skip(1))
        {
            var diff = point.Team1Score - point.Team2Score;
            var currentLeader = diff == 0 ? 0 : Math.Sign(diff);

            if (currentLeader == 0) continue;

            // Only count actual leader switches (not return to same leader after a tie)
            if (previousLeader != 0 && currentLeader != previousLeader)
            {
                count++;
            }

            previousLeader = currentLeader;
        }

        return new MatchContextKpi(
            "Changements de leader",
            count.ToString(),
            count == 0 ? "Aucune bascule" : $"{count} bascule{(count > 1 ? "s" : "")}",
            true);
    }

    private static MatchContextKpi BuildMeaningfulTies(IReadOnlyList<ScoreTimelinePoint> timeline)
    {
        // Exclude 0-0 initial — ties where both scores equal AND not the starting 0-0
        var ties = timeline.Count(p =>
            p.Team1Score == p.Team2Score
            && (p.Team1Score > 0 || p.Minute > 0.1d));

        return new MatchContextKpi(
            "Égalités",
            ties.ToString(),
            ties == 0 ? "Aucune" : $"{ties} retour{(ties > 1 ? "s" : "")} au score nul",
            true);
    }

    private static MatchContextKpi BuildTopRun(
        IReadOnlyList<ScoreTimelinePoint> timeline,
        string team1Label,
        string team2Label)
    {
        // Run = consecutive goals by same team without opponent scoring
        int maxRun = 0;
        string? maxRunTeam = null;
        int currentRun = 0;
        int? currentTeam = null;

        for (int i = 1; i < timeline.Count; i++)
        {
            var prev = timeline[i - 1];
            var curr = timeline[i];
            var dt1 = curr.Team1Score - prev.Team1Score;
            var dt2 = curr.Team2Score - prev.Team2Score;

            if (dt1 > 0 && dt2 == 0)
            {
                if (currentTeam == 1) currentRun += dt1;
                else { currentRun = dt1; currentTeam = 1; }
            }
            else if (dt2 > 0 && dt1 == 0)
            {
                if (currentTeam == 2) currentRun += dt2;
                else { currentRun = dt2; currentTeam = 2; }
            }
            else
            {
                currentRun = 0;
                currentTeam = null;
            }

            if (currentRun > maxRun)
            {
                maxRun = currentRun;
                maxRunTeam = currentTeam == 1 ? team1Label : team2Label;
            }
        }

        if (maxRun < 2)
        {
            return new MatchContextKpi("Run principal", "—", "Pas de série notable", false);
        }

        return new MatchContextKpi(
            "Run principal",
            $"+{maxRun}",
            maxRunTeam,
            true);
    }

    private static IReadOnlyList<MatchKeyMoment> BuildKeyMoments(
        IReadOnlyList<ScoreTimelinePoint> timeline,
        string team1Label,
        string team2Label)
    {
        var moments = new List<MatchKeyMoment>();

        // 1. Half-time
        var ht = timeline.Where(p => p.Minute <= 30.5d).LastOrDefault();
        if (ht != null)
        {
            moments.Add(new MatchKeyMoment(
                (int)Math.Floor(ht.Minute), 0,
                $"Mi-temps : {ht.Team1Score} – {ht.Team2Score}",
                KeyMomentType.FinalMoment));
        }

        // 2. Lead changes (last 2)
        var leadChangeMoments = BuildLeadChangeMomentList(timeline, team1Label, team2Label);
        moments.AddRange(leadChangeMoments.TakeLast(2));

        // 3. Max lead moment
        var maxLeadPoint = timeline.OrderByDescending(p => Math.Abs(p.Team1Score - p.Team2Score)).First();
        var maxLead = Math.Abs(maxLeadPoint.Team1Score - maxLeadPoint.Team2Score);
        if (maxLead >= 3)
        {
            var leadingTeam = maxLeadPoint.Team1Score > maxLeadPoint.Team2Score ? team1Label : team2Label;
            moments.Add(new MatchKeyMoment(
                (int)Math.Floor(maxLeadPoint.Minute), 0,
                $"Écart max +{maxLead} ({leadingTeam})",
                KeyMomentType.MaxLead));
        }

        // 4. Best run (if >= 3)
        var (bestRunGoals, bestRunTeam, bestRunMinute) = FindBestRun(timeline, team1Label, team2Label);
        if (bestRunGoals >= 3)
        {
            moments.Add(new MatchKeyMoment(
                (int)Math.Floor(bestRunMinute), 0,
                $"Run +{bestRunGoals} ({bestRunTeam})",
                KeyMomentType.BigRun));
        }

        // 5. Final moment
        var final = timeline.Last();
        moments.Add(new MatchKeyMoment(
            (int)Math.Floor(final.Minute), 0,
            $"Score final : {final.Team1Score} – {final.Team2Score}",
            KeyMomentType.FinalMoment));

        // Deduplicate and sort by minute, cap at 6
        return moments
            .GroupBy(m => $"{m.Minute}|{m.Description}")
            .Select(g => g.First())
            .OrderBy(m => m.Minute)
            .Take(6)
            .ToList();
    }

    private static IReadOnlyList<MatchKeyMoment> BuildLeadChangeMomentList(
        IReadOnlyList<ScoreTimelinePoint> timeline,
        string team1Label,
        string team2Label)
    {
        var moments = new List<MatchKeyMoment>();
        var previousLeader = 0;

        foreach (var point in timeline.Skip(1))
        {
            var diff = point.Team1Score - point.Team2Score;
            var currentLeader = diff == 0 ? 0 : Math.Sign(diff);

            if (currentLeader == 0) continue;

            if (previousLeader != 0 && currentLeader != previousLeader)
            {
                var leaderLabel = currentLeader > 0 ? team1Label : team2Label;
                moments.Add(new MatchKeyMoment(
                    (int)Math.Floor(point.Minute), 0,
                    $"{leaderLabel} prend la main ({point.Team1Score} – {point.Team2Score})",
                    KeyMomentType.LeadChange));
            }

            previousLeader = currentLeader;
        }

        return moments;
    }

    private static (int Goals, string TeamLabel, double Minute) FindBestRun(
        IReadOnlyList<ScoreTimelinePoint> timeline,
        string team1Label,
        string team2Label)
    {
        int maxRun = 0;
        string maxRunTeam = team1Label;
        double maxRunMinute = 0;
        int currentRun = 0;
        int? currentTeam = null;
        double currentRunEndMinute = 0;

        for (int i = 1; i < timeline.Count; i++)
        {
            var prev = timeline[i - 1];
            var curr = timeline[i];
            var dt1 = curr.Team1Score - prev.Team1Score;
            var dt2 = curr.Team2Score - prev.Team2Score;

            if (dt1 > 0 && dt2 == 0)
            {
                if (currentTeam == 1) { currentRun += dt1; currentRunEndMinute = curr.Minute; }
                else { currentRun = dt1; currentTeam = 1; currentRunEndMinute = curr.Minute; }
            }
            else if (dt2 > 0 && dt1 == 0)
            {
                if (currentTeam == 2) { currentRun += dt2; currentRunEndMinute = curr.Minute; }
                else { currentRun = dt2; currentTeam = 2; currentRunEndMinute = curr.Minute; }
            }
            else
            {
                currentRun = 0;
                currentTeam = null;
            }

            if (currentRun > maxRun)
            {
                maxRun = currentRun;
                maxRunTeam = currentTeam == 1 ? team1Label : team2Label;
                maxRunMinute = currentRunEndMinute;
            }
        }

        return (maxRun, maxRunTeam, maxRunMinute);
    }
}

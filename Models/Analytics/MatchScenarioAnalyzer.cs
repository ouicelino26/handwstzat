using HandballManagerCore.DTO;
using HandballManagerCore.Models;

namespace HandWStat.Models.Analytics;

public static class MatchScenarioAnalyzer
{
    public static IReadOnlyList<ScoreTimelinePoint> BuildScoreTimeline(IReadOnlyList<MatchEvent> events, MatchListItemDto? match)
    {
        var points = new List<ScoreTimelinePoint>
        {
            new("00:00", 0, 0, 0, true, "Debut")
        };

        var lastTeam1 = 0;
        var lastTeam2 = 0;

        foreach (var item in events
            .Select(matchEvent => new
            {
                Clock = ResolveMatchClock(matchEvent.Time, matchEvent.MiTemps),
                Minute = ResolveMatchMinute(matchEvent.Time, matchEvent.MiTemps),
                Team1 = matchEvent.TeamScore1 ?? 0,
                Team2 = matchEvent.TeamScore2 ?? 0,
                matchEvent.Id
            })
            .OrderBy(item => item.Minute)
            .ThenBy(item => item.Id))
        {
            if (item.Team1 == lastTeam1 && item.Team2 == lastTeam2)
            {
                continue;
            }

            points.Add(new(FormatTimelineLabel(item.Clock), item.Minute, item.Team1, item.Team2));
            lastTeam1 = item.Team1;
            lastTeam2 = item.Team2;
        }

        var halftimeSnapshot = points
            .Where(point => point.Minute <= 30.5d)
            .LastOrDefault() ?? points[0];
        EnsureMarker(points, 30d, halftimeSnapshot.Team1Score, halftimeSnapshot.Team2Score, "Mi-temps");

        var finalTeam1 = match?.Team1Score ?? lastTeam1;
        var finalTeam2 = match?.Team2Score ?? lastTeam2;
        var finalMinute = Math.Max(points.LastOrDefault()?.Minute ?? 0d, 60d);

        EnsureMarker(points, finalMinute, finalTeam1, finalTeam2, "Fin");

        return points
            .OrderBy(point => point.Minute)
            .ThenBy(point => point.IsMarker ? 1 : 0)
            .ToList();
    }

    public static IReadOnlyList<KpiTile> BuildTimelineKpis(
        IReadOnlyList<ScoreTimelinePoint> points,
        string? team1Name,
        string? team2Name)
    {
        if (points.Count < 2)
        {
            return [];
        }

        var team1Label = string.IsNullOrWhiteSpace(team1Name) ? "Equipe 1" : team1Name;
        var team2Label = string.IsNullOrWhiteSpace(team2Name) ? "Equipe 2" : team2Name;
        var halftimePoint = GetSnapshotAt(points, 30.5d);
        var finalPoint = points.Last();
        var firstHalfGoals = halftimePoint.Team1Score + halftimePoint.Team2Score;
        var secondHalfGoals = (finalPoint.Team1Score - halftimePoint.Team1Score) + (finalPoint.Team2Score - halftimePoint.Team2Score);

        var team1LeadPoint = points
            .OrderByDescending(point => point.Team1Score - point.Team2Score)
            .ThenBy(point => point.Minute)
            .First();
        var team1Lead = Math.Max(team1LeadPoint.Team1Score - team1LeadPoint.Team2Score, 0);

        var team2LeadPoint = points
            .OrderByDescending(point => point.Team2Score - point.Team1Score)
            .ThenBy(point => point.Minute)
            .First();
        var team2Lead = Math.Max(team2LeadPoint.Team2Score - team2LeadPoint.Team1Score, 0);

        var leadChangeMoments = BuildLeadChangeMoments(points, team1Label, team2Label);
        var runs = BuildRuns(points, team1Label, team2Label);
        var team1Run = runs
            .Where(run => run.TeamIndex == 1)
            .OrderByDescending(run => run.Goals)
            .ThenBy(run => run.EndMinute)
            .FirstOrDefault();
        var team2Run = runs
            .Where(run => run.TeamIndex == 2)
            .OrderByDescending(run => run.Goals)
            .ThenBy(run => run.EndMinute)
            .FirstOrDefault();
        var finalDiff = finalPoint.Team1Score - finalPoint.Team2Score;

        return
        [
            new KpiTile(
                "Score a la pause",
                $"{halftimePoint.Team1Score} - {halftimePoint.Team2Score}",
                "Base de lecture pour le retour des vestiaires.",
                "neutral",
                $"{firstHalfGoals} buts cumules avant 30:00"),
            new KpiTile(
                "Ecart final",
                HandballKpiHelper.FormatSigned(finalDiff),
                "Differentiel au coup de sifflet final.",
                finalDiff > 0 ? "positive" : finalDiff < 0 ? "warning" : "neutral",
                $"{finalPoint.Team1Score} - {finalPoint.Team2Score} au final"),
            new KpiTile(
                "Lead max eq. 1",
                $"+{team1Lead}",
                $"Plus grand avantage pris par {team1Label}.",
                team1Lead >= team2Lead && team1Lead > 0 ? "positive" : "neutral",
                team1Lead > 0
                    ? $"{team1LeadPoint.Label} - {team1LeadPoint.Team1Score} - {team1LeadPoint.Team2Score}"
                    : $"{team1Label} n'a jamais mene"),
            new KpiTile(
                "Lead max eq. 2",
                $"+{team2Lead}",
                $"Plus grand avantage pris par {team2Label}.",
                team2Lead > team1Lead ? "warning" : "neutral",
                team2Lead > 0
                    ? $"{team2LeadPoint.Label} - {team2LeadPoint.Team1Score} - {team2LeadPoint.Team2Score}"
                    : $"{team2Label} n'a jamais mene"),
            new KpiTile(
                "Renversements",
                leadChangeMoments.Count.ToString(),
                "Bascule reelle d'une equipe a l'autre hors egalites.",
                leadChangeMoments.Count >= 3 ? "warning" : "neutral",
                leadChangeMoments.Count == 0 ? "Aucune bascule de leader" : $"{leadChangeMoments.Count} changements identifies"),
            new KpiTile(
                "Run max eq. 1",
                $"+{team1Run?.Goals ?? 0}",
                $"Serie sans reponse la plus forte de {team1Label}.",
                team1Run is not null && (team1Run.Goals >= (team2Run?.Goals ?? 0)) ? "positive" : "neutral",
                team1Run is null ? "Aucun run continu notable" : $"{team1Run.StartLabel} -> {team1Run.EndLabel}"),
            new KpiTile(
                "Run max eq. 2",
                $"+{team2Run?.Goals ?? 0}",
                $"Serie sans reponse la plus forte de {team2Label}.",
                team2Run is not null && team2Run.Goals > (team1Run?.Goals ?? 0) ? "warning" : "neutral",
                team2Run is null ? "Aucun run continu notable" : $"{team2Run.StartLabel} -> {team2Run.EndLabel}"),
            new KpiTile(
                "Buts 2e MT",
                secondHalfGoals.ToString(),
                "Volume inscrit apres la pause pour lire le money time.",
                secondHalfGoals >= firstHalfGoals ? "positive" : "neutral",
                $"{finalPoint.Team1Score - halftimePoint.Team1Score} - {finalPoint.Team2Score - halftimePoint.Team2Score} sur la periode")
        ];
    }

    public static IReadOnlyList<MatchTimelineInsight> BuildPhaseInsights(
        IReadOnlyList<ScoreTimelinePoint> points,
        string? team1Name,
        string? team2Name)
    {
        if (points.Count < 2)
        {
            return [];
        }

        var team1Label = string.IsNullOrWhiteSpace(team1Name) ? "Equipe 1" : team1Name;
        var team2Label = string.IsNullOrWhiteSpace(team2Name) ? "Equipe 2" : team2Name;
        var halftimePoint = GetSnapshotAt(points, 30.5d);
        var finalPoint = points.Last();
        var minute50Point = GetSnapshotAt(points, 50.5d);
        var runs = BuildRuns(points, team1Label, team2Label);
        var bestRun = runs
            .OrderByDescending(run => run.Goals)
            .ThenBy(run => run.EndMinute)
            .FirstOrDefault();

        return
        [
            new MatchTimelineInsight(
                "1re mi-temps",
                $"{halftimePoint.Team1Score} - {halftimePoint.Team2Score}",
                $"{halftimePoint.Team1Score + halftimePoint.Team2Score} buts avant 30:00"),
            new MatchTimelineInsight(
                "2e mi-temps",
                $"{finalPoint.Team1Score - halftimePoint.Team1Score} - {finalPoint.Team2Score - halftimePoint.Team2Score}",
                $"{(finalPoint.Team1Score - halftimePoint.Team1Score) + (finalPoint.Team2Score - halftimePoint.Team2Score)} buts apres la pause"),
            new MatchTimelineInsight(
                "Dernier 10'",
                $"{finalPoint.Team1Score - minute50Point.Team1Score} - {finalPoint.Team2Score - minute50Point.Team2Score}",
                $"Lecture du money time depuis {minute50Point.Label}"),
            new MatchTimelineInsight(
                "Run cle",
                bestRun is null ? "Aucun" : $"{bestRun.TeamLabel} +{bestRun.Goals}",
                bestRun is null ? "Pas de serie sans reponse exploitable." : $"{bestRun.StartLabel} -> {bestRun.EndLabel} ({bestRun.ScoreLabel})")
        ];
    }

    public static IReadOnlyList<MatchTimelineMoment> BuildKeyMoments(
        IReadOnlyList<ScoreTimelinePoint> points,
        string? team1Name,
        string? team2Name)
    {
        if (points.Count < 2)
        {
            return [];
        }

        var team1Label = string.IsNullOrWhiteSpace(team1Name) ? "Equipe 1" : team1Name;
        var team2Label = string.IsNullOrWhiteSpace(team2Name) ? "Equipe 2" : team2Name;
        var halftimePoint = GetSnapshotAt(points, 30.5d);
        var finalPoint = points.Last();
        var leadChanges = BuildLeadChangeMoments(points, team1Label, team2Label);
        var runs = BuildRuns(points, team1Label, team2Label)
            .Where(run => run.Goals >= 3)
            .OrderByDescending(run => run.Goals)
            .ThenBy(run => run.EndMinute)
            .Take(2)
            .Select(run => new MatchTimelineMoment(
                run.EndMinute,
                $"Run +{run.Goals}",
                run.EndLabel,
                run.ScoreLabel,
                $"{run.TeamLabel} enchaine {run.Goals} buts sans reponse ({run.StartLabel} -> {run.EndLabel}).",
                run.Tone));

        var biggestLeadMoments = BuildBiggestLeadMoments(points, team1Label, team2Label);

        var moments = new List<MatchTimelineMoment>
        {
            new(
                halftimePoint.Minute,
                "Mi-temps",
                "30:00",
                $"{halftimePoint.Team1Score} - {halftimePoint.Team2Score}",
                "Score fige au retour des vestiaires.",
                "neutral")
        };

        moments.AddRange(leadChanges.TakeLast(2));
        moments.AddRange(runs);
        moments.AddRange(biggestLeadMoments);
        moments.Add(new MatchTimelineMoment(
            finalPoint.Minute,
            "Fin du match",
            finalPoint.Label,
            $"{finalPoint.Team1Score} - {finalPoint.Team2Score}",
            "Score final et conclusion du scenario.",
            finalPoint.Team1Score >= finalPoint.Team2Score ? "positive" : "warning"));

        return moments
            .GroupBy(moment => $"{moment.Title}|{moment.ClockLabel}|{moment.ScoreLabel}")
            .Select(group => group.First())
            .OrderBy(moment => moment.Minute)
            .ToList();
    }

    private static IReadOnlyList<MatchTimelineMoment> BuildBiggestLeadMoments(
        IReadOnlyList<ScoreTimelinePoint> points,
        string team1Label,
        string team2Label)
    {
        var team1LeadPoint = points
            .OrderByDescending(point => point.Team1Score - point.Team2Score)
            .ThenBy(point => point.Minute)
            .First();
        var team1Lead = Math.Max(team1LeadPoint.Team1Score - team1LeadPoint.Team2Score, 0);

        var team2LeadPoint = points
            .OrderByDescending(point => point.Team2Score - point.Team1Score)
            .ThenBy(point => point.Minute)
            .First();
        var team2Lead = Math.Max(team2LeadPoint.Team2Score - team2LeadPoint.Team1Score, 0);

        var moments = new List<MatchTimelineMoment>();

        if (team1Lead > 1)
        {
            moments.Add(new MatchTimelineMoment(
                team1LeadPoint.Minute,
                "Ecart max eq. 1",
                team1LeadPoint.Label,
                $"{team1LeadPoint.Team1Score} - {team1LeadPoint.Team2Score}",
                $"{team1Label} monte a +{team1Lead}.",
                "positive"));
        }

        if (team2Lead > 1)
        {
            moments.Add(new MatchTimelineMoment(
                team2LeadPoint.Minute,
                "Ecart max eq. 2",
                team2LeadPoint.Label,
                $"{team2LeadPoint.Team1Score} - {team2LeadPoint.Team2Score}",
                $"{team2Label} monte a +{team2Lead}.",
                "warning"));
        }

        return moments;
    }

    private static IReadOnlyList<MatchTimelineMoment> BuildLeadChangeMoments(
        IReadOnlyList<ScoreTimelinePoint> points,
        string team1Label,
        string team2Label)
    {
        var moments = new List<MatchTimelineMoment>();
        var previousLeader = 0;

        foreach (var point in points.Skip(1))
        {
            var diff = point.Team1Score - point.Team2Score;
            var currentLeader = diff == 0 ? 0 : Math.Sign(diff);

            if (currentLeader == 0)
            {
                continue;
            }

            if (previousLeader != 0 && currentLeader != previousLeader)
            {
                var leaderLabel = currentLeader > 0 ? team1Label : team2Label;
                moments.Add(new MatchTimelineMoment(
                    point.Minute,
                    "Changement de leader",
                    point.Label,
                    $"{point.Team1Score} - {point.Team2Score}",
                    $"{leaderLabel} prend la main ({HandballKpiHelper.FormatSigned(diff)}).",
                    currentLeader > 0 ? "positive" : "warning"));
            }

            previousLeader = currentLeader;
        }

        return moments;
    }

    private static IReadOnlyList<ScoringRun> BuildRuns(
        IReadOnlyList<ScoreTimelinePoint> points,
        string team1Label,
        string team2Label)
    {
        var runs = new List<ScoringRun>();
        ScoringRun? currentRun = null;

        for (var index = 1; index < points.Count; index++)
        {
            var previous = points[index - 1];
            var current = points[index];
            var deltaTeam1 = current.Team1Score - previous.Team1Score;
            var deltaTeam2 = current.Team2Score - previous.Team2Score;

            int scoringTeam;
            int goalsAdded;
            string teamLabel;
            string tone;

            if (deltaTeam1 > 0 && deltaTeam2 == 0)
            {
                scoringTeam = 1;
                goalsAdded = deltaTeam1;
                teamLabel = team1Label;
                tone = "positive";
            }
            else if (deltaTeam2 > 0 && deltaTeam1 == 0)
            {
                scoringTeam = 2;
                goalsAdded = deltaTeam2;
                teamLabel = team2Label;
                tone = "warning";
            }
            else
            {
                if (currentRun is not null)
                {
                    runs.Add(currentRun);
                    currentRun = null;
                }

                continue;
            }

            if (currentRun is not null && currentRun.TeamIndex == scoringTeam)
            {
                currentRun = currentRun with
                {
                    Goals = currentRun.Goals + goalsAdded,
                    EndMinute = current.Minute,
                    EndLabel = current.Label,
                    ScoreLabel = $"{current.Team1Score} - {current.Team2Score}"
                };
            }
            else
            {
                if (currentRun is not null)
                {
                    runs.Add(currentRun);
                }

                currentRun = new ScoringRun(
                    scoringTeam,
                    teamLabel,
                    goalsAdded,
                    previous.Minute,
                    previous.Label,
                    current.Minute,
                    current.Label,
                    $"{current.Team1Score} - {current.Team2Score}",
                    tone);
            }
        }

        if (currentRun is not null)
        {
            runs.Add(currentRun);
        }

        return runs;
    }

    private static ScoreTimelinePoint GetSnapshotAt(IReadOnlyList<ScoreTimelinePoint> points, double minute)
    {
        return points
            .Where(point => point.Minute <= minute)
            .LastOrDefault() ?? points.First();
    }

    private static void EnsureMarker(
        List<ScoreTimelinePoint> points,
        double minute,
        int team1Score,
        int team2Score,
        string markerLabel)
    {
        var existingIndex = points.FindIndex(point =>
            Math.Abs(point.Minute - minute) < 0.05d
            && point.Team1Score == team1Score
            && point.Team2Score == team2Score);

        if (existingIndex >= 0)
        {
            points[existingIndex] = points[existingIndex] with
            {
                Label = FormatTimelineLabel(TimeSpan.FromMinutes(minute)),
                IsMarker = true,
                MarkerLabel = markerLabel
            };

            return;
        }

        points.Add(new(
            FormatTimelineLabel(TimeSpan.FromMinutes(minute)),
            minute,
            team1Score,
            team2Score,
            true,
            markerLabel));
    }

    private static double ResolveMatchMinute(TimeSpan? time, string? half)
    {
        return ResolveMatchClock(time, half).TotalMinutes;
    }

    private static TimeSpan ResolveMatchClock(TimeSpan? time, string? half)
    {
        var clock = time ?? TimeSpan.Zero;

        if (IsSecondHalf(half) && clock.TotalMinutes < 31)
        {
            clock = clock.Add(TimeSpan.FromMinutes(30));
        }

        return clock;
    }

    private static bool IsSecondHalf(string? half)
    {
        if (string.IsNullOrWhiteSpace(half))
        {
            return false;
        }

        var normalized = half.Trim().ToLowerInvariant();
        return normalized.Contains("2") || normalized.Contains("deux") || normalized.Contains("second");
    }

    private static string FormatTimelineLabel(TimeSpan clock)
    {
        var minutes = (int)Math.Floor(clock.TotalMinutes);
        return $"{minutes:00}:{clock.Seconds:00}";
    }

    private sealed record ScoringRun(
        int TeamIndex,
        string TeamLabel,
        int Goals,
        double StartMinute,
        string StartLabel,
        double EndMinute,
        string EndLabel,
        string ScoreLabel,
        string Tone);
}

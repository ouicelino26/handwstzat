using HandWStat.Models.Analytics;

namespace HandWStat.Services.Analytics;

public static class LeagueAnalyticsContractValidator
{
    private static readonly HashSet<string> AllowedSections =
        new(LeagueAnalyticsContract.AllSections, StringComparer.Ordinal);

    public static string? Validate(
        LeaguePlayerAnalyticsResponseDto? response,
        int expectedPlayerId,
        IReadOnlyCollection<string> requestedSections)
    {
        if (response is null)
        {
            return "La réponse v2 est vide.";
        }

        if (response.PlayerId != expectedPlayerId || response.PlayerId <= 0)
        {
            return "Le playerId de la réponse ne correspond pas à la requête.";
        }

        if (!string.Equals(response.MetricVersion, LeagueAnalyticsContract.MetricVersion, StringComparison.Ordinal))
        {
            return "La version métrique v2 n'est pas prise en charge.";
        }

        var requested = NormalizeSections(requestedSections);
        if (requested.Count == 0 || requested.Any(section => !AllowedSections.Contains(section)))
        {
            return "La requête contient une section include inconnue.";
        }

        if (response.Included is null
            || response.Included.Any(section => !AllowedSections.Contains(section))
            || !response.Included.SequenceEqual(response.Included.OrderBy(section => section, StringComparer.Ordinal))
            || !requested.SetEquals(response.Included))
        {
            return "La liste included ne correspond pas aux sections demandées.";
        }

        if (!SectionStateIsValid("overview", response.Overview, requested)
            || !SectionStateIsValid("offense", response.Offense, requested)
            || !SectionStateIsValid("defense", response.Defense, requested)
            || !SectionStateIsValid("goalkeeper", response.Goalkeeper, requested))
        {
            return "Une section v2 demandée est absente ou une section non demandée n'est pas nulle.";
        }

        if (response.Overview is not null)
        {
            if (response.Overview.PlayerId != expectedPlayerId
                || string.IsNullOrWhiteSpace(response.Overview.FullName)
                || response.Overview.MatchesPlayed < 0)
            {
                return "La section overview est incomplète.";
            }
        }

        if (response.Offense is not null)
        {
            var offense = response.Offense;
            if (AnyNegative(
                    offense.TotalGoals,
                    offense.OpenPlayGoals,
                    offense.PenaltyGoals,
                    offense.Assists,
                    offense.PenaltiesWon,
                    offense.SanctionsDrawn,
                    offense.TotalTurnovers,
                    offense.BadPasses)
                || offense.TotalGoals != offense.OpenPlayGoals + offense.PenaltyGoals
                || offense.BadPasses > offense.TotalTurnovers)
            {
                return "Les compteurs offensifs ne respectent pas le contrat.";
            }

            var pivotError = ValidateFailedPivotPasses(offense.FailedPivotPasses, response.MetricVersion);
            if (pivotError is not null)
            {
                return pivotError;
            }

            var rateError =
                ValidateRate(offense.TotalShotRate, "TOTAL_SHOT_RATE", response.MetricVersion, offense.TotalGoals, 4)
                ?? ValidateRate(offense.OpenPlayShotRate, "OPEN_PLAY_SHOT_RATE", response.MetricVersion, offense.OpenPlayGoals, 4)
                ?? ValidateRate(offense.PenaltyShotRate, "PENALTY_SHOT_RATE", response.MetricVersion, offense.PenaltyGoals, 2);

            if (rateError is not null)
            {
                return rateError;
            }
        }

        if (response.Defense is not null)
        {
            var defense = response.Defense;
            if (AnyNegative(
                    defense.Interceptions,
                    defense.Blocks,
                    defense.OffensiveFoulsDrawn,
                    defense.Neutralizations,
                    defense.PenaltiesConceded,
                    defense.SanctionsConceded,
                    defense.WarningsConceded,
                    defense.TwoMinuteSuspensionsConceded,
                    defense.DisqualificationsConceded)
                || defense.SanctionsConceded
                    != defense.WarningsConceded
                    + defense.TwoMinuteSuspensionsConceded
                    + defense.DisqualificationsConceded)
            {
                return "Les compteurs défensifs ne respectent pas le contrat.";
            }
        }

        if (response.Goalkeeper is not null)
        {
            var goalkeeper = response.Goalkeeper;
            if (AnyNegative(
                    goalkeeper.TotalSaves,
                    goalkeeper.OpenPlaySaves,
                    goalkeeper.PenaltySaves,
                    goalkeeper.TotalShotsFaced,
                    goalkeeper.OpenPlayShotsFaced,
                    goalkeeper.PenaltyShotsFaced,
                    goalkeeper.Assists,
                    goalkeeper.Goals,
                    goalkeeper.TotalTurnovers,
                    goalkeeper.MissedShots)
                || goalkeeper.TotalSaves != goalkeeper.OpenPlaySaves + goalkeeper.PenaltySaves
                || goalkeeper.TotalShotsFaced != goalkeeper.OpenPlayShotsFaced + goalkeeper.PenaltyShotsFaced
                || goalkeeper.OpenPlaySaves > goalkeeper.OpenPlayShotsFaced
                || goalkeeper.PenaltySaves > goalkeeper.PenaltyShotsFaced)
            {
                return "Les compteurs gardienne ne respectent pas le contrat.";
            }

            var rateError =
                ValidateRate(goalkeeper.TotalSaveRate, "TOTAL_SAVE_RATE", response.MetricVersion, goalkeeper.TotalSaves, 10)
                ?? ValidateRate(goalkeeper.OpenPlaySaveRate, "OPEN_PLAY_SAVE_RATE", response.MetricVersion, goalkeeper.OpenPlaySaves, 10)
                ?? ValidateRate(goalkeeper.PenaltySaveRate, "PENALTY_SAVE_RATE", response.MetricVersion, goalkeeper.PenaltySaves, 2);

            if (rateError is not null)
            {
                return rateError;
            }
        }

        return null;
    }

    private static HashSet<string> NormalizeSections(IEnumerable<string> sections) =>
        sections
            .Where(section => !string.IsNullOrWhiteSpace(section))
            .Select(section => section.Trim().ToLowerInvariant())
            .ToHashSet(StringComparer.Ordinal);

    private static bool SectionStateIsValid<T>(string section, T? value, IReadOnlySet<string> requested)
        where T : class =>
        requested.Contains(section) ? value is not null : value is null;

    private static string? ValidateFailedPivotPasses(LeagueCountMetricDto metric, string responseVersion)
    {
        if (!string.Equals(metric.MetricCode, "FAILED_PIVOT_PASSES", StringComparison.Ordinal)
            || !string.Equals(metric.MetricVersion, responseVersion, StringComparison.Ordinal))
        {
            return "Le contrat failedPivotPasses est incomplet.";
        }

        if (metric.Availability == LeagueMetricAvailability.DATA_MISSING && metric.Value.HasValue)
        {
            return "failedPivotPasses ne peut pas fournir une valeur avec DATA_MISSING.";
        }

        if (metric.Availability == LeagueMetricAvailability.AVAILABLE && !metric.Value.HasValue)
        {
            return "failedPivotPasses AVAILABLE doit fournir une valeur.";
        }

        if (metric.Value < 0)
        {
            return "failedPivotPasses ne peut pas être négatif.";
        }

        return null;
    }

    private static string? ValidateRate(
        LeagueMetricValueDto metric,
        string metricCode,
        string responseVersion,
        double expectedNumerator,
        double expectedMinimumSample)
    {
        if (!string.Equals(metric.MetricCode, metricCode, StringComparison.Ordinal)
            || !string.Equals(metric.MetricVersion, responseVersion, StringComparison.Ordinal)
            || !string.Equals(metric.Unit, "percent", StringComparison.Ordinal)
            || metric.Sample is null
            || metric.Quality is null)
        {
            return $"Le taux {metricCode} est structurellement incomplet.";
        }

        if (!Same(metric.Numerator, metric.Sample.Numerator)
            || !Same(metric.Denominator, metric.Sample.Denominator)
            || !Same(metric.MinimumSample, metric.Sample.MinimumSample)
            || metric.SampleReliable != metric.Quality.SampleReliable
            || !Same(metric.QualityScore, metric.Quality.QualityScore))
        {
            return $"Les preuves imbriquées et aplaties de {metricCode} divergent.";
        }

        if (!metric.Sample.Numerator.HasValue
            || !metric.Sample.Denominator.HasValue
            || !double.IsFinite(metric.Sample.Numerator.Value)
            || !double.IsFinite(metric.Sample.Denominator.Value)
            || metric.Sample.Numerator.Value < 0
            || metric.Sample.Denominator.Value < 0
            || !Same(metric.Sample.Numerator.Value, expectedNumerator)
            || !Same(metric.Sample.MinimumSample, expectedMinimumSample)
            || metric.Sample.MinimumSample <= 0)
        {
            return $"L'échantillon de {metricCode} est invalide.";
        }

        var numerator = metric.Sample.Numerator.Value;
        var denominator = metric.Sample.Denominator.Value;

        if (denominator == 0)
        {
            if (metric.Value.HasValue
                || metric.Quality.SampleReliable
                || !Same(metric.Quality.QualityScore, 0)
                || !string.Equals(metric.Quality.Reason, "ZERO_OR_INVALID_DENOMINATOR", StringComparison.Ordinal))
            {
                return $"Le cas de dénominateur nul de {metricCode} est invalide.";
            }

            return null;
        }

        if (numerator > denominator || !metric.Value.HasValue || !double.IsFinite(metric.Value.Value))
        {
            return $"La valeur de {metricCode} est invalide.";
        }

        var expectedValue = Math.Round(
            numerator * 100d / denominator,
            2,
            MidpointRounding.AwayFromZero);
        var expectedReliable = denominator >= metric.Sample.MinimumSample;
        var expectedQualityScore = Math.Round(
            Math.Clamp(denominator / metric.Sample.MinimumSample, 0d, 1d),
            2,
            MidpointRounding.AwayFromZero);
        var expectedReason = expectedReliable ? null : "BELOW_MINIMUM_SAMPLE";

        if (!Same(metric.Value.Value, expectedValue)
            || metric.Quality.SampleReliable != expectedReliable
            || !Same(metric.Quality.QualityScore, expectedQualityScore)
            || !string.Equals(metric.Quality.Reason, expectedReason, StringComparison.Ordinal))
        {
            return $"La valeur ou la qualité de {metricCode} ne respecte pas le contrat.";
        }

        return null;
    }

    private static bool AnyNegative(params int[] values) => values.Any(value => value < 0);

    private static bool Same(double? left, double? right)
    {
        if (!left.HasValue || !right.HasValue)
        {
            return left.HasValue == right.HasValue;
        }

        return Same(left.Value, right.Value);
    }

    private static bool Same(double left, double right) =>
        double.IsFinite(left)
        && double.IsFinite(right)
        && Math.Abs(left - right) < 0.000001d;
}

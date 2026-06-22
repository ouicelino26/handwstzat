using HandballManagerCore.DTO;

namespace HandWStat.Models.Analytics;

public enum CoachCardType
{
    Strength,
    Weakness,
    Alert,
    Role,
    Tactical
}

public sealed record CoachCardViewModel(
    CoachCardType Type,
    string Label,
    string Sentence,
    string Tone = "neutral",
    string? Icon = null,
    IReadOnlyList<string>? HighlightAxisKeys = null)
{
    public IReadOnlyList<string> HighlightKeys => HighlightAxisKeys ?? [];
}

public sealed record PositionProfileInsightBundle(
    string PerformanceLevel,
    string ProfileType,
    string QuickSummary,
    string ContextualInsight,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Weaknesses,
    IReadOnlyList<CoachCardViewModel> CoachCards,
    IReadOnlyList<KpiTile> SnapshotKpis,
    IReadOnlyList<string> HighlightAxisKeys,
    IReadOnlyList<string> AlertAxisKeys)
{
    public static PositionProfileInsightBundle Empty { get; } = new(
        PerformanceLevel: string.Empty,
        ProfileType: string.Empty,
        QuickSummary: string.Empty,
        ContextualInsight: string.Empty,
        Strengths: [],
        Weaknesses: [],
        CoachCards: [],
        SnapshotKpis: [],
        HighlightAxisKeys: [],
        AlertAxisKeys: []);
}

internal static class PositionProfileInsightEngine
{
    public static PositionProfileInsightBundle Build(
        PositionProfileResponseDto? profile,
        IReadOnlyList<PositionProfileAxisViewModel> allAxes,
        IReadOnlyList<PositionProfileAxisViewModel> chartAxes,
        string? season)
    {
        if (profile?.SelectedPlayer is null || profile.MedianProfile is null || allAxes.Count == 0)
        {
            return PositionProfileInsightBundle.Empty;
        }

        var axes = allAxes
            .Where(IsFiniteAxis)
            .ToList();

        if (axes.Count == 0)
        {
            return PositionProfileInsightBundle.Empty;
        }

        var player = profile.SelectedPlayer;
        var overallAverage = axes.Average(axis => axis.DirectionalPercentile);
        var volumeScore = AverageOrFallback(axes.Where(IsVolumeAxis).Select(axis => axis.DirectionalPercentile), overallAverage);
        var efficiencyScore = AverageOrFallback(axes.Where(IsEfficiencyAxis).Select(axis => axis.DirectionalPercentile), overallAverage);
        var offenseScore = AverageOrFallback(axes.Where(IsOffenseAxis).Select(axis => axis.DirectionalPercentile), overallAverage);
        var defenseScore = AverageOrFallback(axes.Where(IsDefenseAxis).Select(axis => axis.DirectionalPercentile), overallAverage);
        var riskScore = AverageOrFallback(axes.Where(IsRiskAxis).Select(axis => axis.DirectionalPercentile), overallAverage);

        var strongAxes = axes
            .Where(axis => axis.DirectionalPercentile >= 65d)
            .OrderByDescending(axis => axis.DirectionalPercentile)
            .ThenByDescending(axis => axis.Impact)
            .Take(3)
            .ToList();

        var weakAxes = axes
            .Where(axis => axis.DirectionalPercentile <= 35d)
            .OrderBy(axis => axis.DirectionalPercentile)
            .ThenByDescending(axis => axis.Impact)
            .Take(3)
            .ToList();

        var alertCards = BuildAlertCards(profile, axes, strongAxes, weakAxes);
        var roleCard = BuildRoleCard(profile, axes, strongAxes, weakAxes);
        var tacticalCard = BuildTacticalCard(profile, axes, volumeScore, efficiencyScore, offenseScore, defenseScore, riskScore);

        var coachCards = BuildCoachCards(
            strongAxes,
            weakAxes,
            alertCards,
            roleCard,
            tacticalCard);

        var highlightAxisKeys = coachCards
            .SelectMany(card => card.HighlightKeys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var alertAxisKeys = alertCards
            .SelectMany(card => card.HighlightKeys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var performanceLevel = BuildPerformanceLevel(overallAverage, strongAxes.Count, weakAxes.Count);
        var profileType = BuildProfileType(volumeScore, efficiencyScore, offenseScore, defenseScore, riskScore, profile.IsGoalkeeperProfile);
        var contextualInsight = BuildContextualInsight(
            profile,
            axes,
            volumeScore,
            efficiencyScore,
            offenseScore,
            defenseScore,
            riskScore,
            strongAxes,
            weakAxes,
            alertCards,
            profileType);
        var quickSummary = BuildQuickSummary(performanceLevel, profileType, strongAxes, weakAxes, contextualInsight);
        var snapshotKpis = BuildSnapshotKpis(
            profile,
            performanceLevel,
            profileType,
            overallAverage,
            volumeScore,
            efficiencyScore,
            strongAxes,
            weakAxes,
            alertCards,
            contextualInsight,
            season);

        return new PositionProfileInsightBundle(
            PerformanceLevel: performanceLevel,
            ProfileType: profileType,
            QuickSummary: quickSummary,
            ContextualInsight: contextualInsight,
            Strengths: strongAxes.Select(BuildAxisReference).ToList(),
            Weaknesses: weakAxes.Select(BuildAxisReference).ToList(),
            CoachCards: coachCards,
            SnapshotKpis: snapshotKpis,
            HighlightAxisKeys: highlightAxisKeys,
            AlertAxisKeys: alertAxisKeys);
    }

    private static IReadOnlyList<CoachCardViewModel> BuildCoachCards(
        IReadOnlyList<PositionProfileAxisViewModel> strongAxes,
        IReadOnlyList<PositionProfileAxisViewModel> weakAxes,
        IReadOnlyList<CoachCardViewModel> alertCards,
        CoachCardViewModel? roleCard,
        CoachCardViewModel? tacticalCard)
    {
        var cards = new List<CoachCardViewModel>(4);

        foreach (var alertCard in alertCards.Take(2))
        {
            if (cards.Count >= 4)
            {
                break;
            }

            cards.Add(alertCard);
        }

        if (cards.Count < 4 && strongAxes.Count > 0)
        {
            cards.Add(BuildStrengthCard(strongAxes[0]));
        }

        if (cards.Count < 4 && weakAxes.Count > 0)
        {
            cards.Add(BuildWeaknessCard(weakAxes[0]));
        }

        if (cards.Count < 4 && roleCard is not null)
        {
            cards.Add(roleCard);
        }

        if (cards.Count < 4 && tacticalCard is not null)
        {
            cards.Add(tacticalCard);
        }

        return cards.Take(4).ToList();
    }

    private static CoachCardViewModel BuildStrengthCard(PositionProfileAxisViewModel axis)
    {
        var percentile = axis.DirectionalPercentile;
        var tone = percentile >= 90d ? "positive" : "good";
        var label = "Force";
        var sentence = $"{BuildAxisPositiveDescription(axis)} ({percentile:0.#}e percentile favorable)";
        return new CoachCardViewModel(CoachCardType.Strength, label, sentence, tone, HighlightAxisKeys: [axis.Key]);
    }

    private static CoachCardViewModel BuildWeaknessCard(PositionProfileAxisViewModel axis)
    {
        var percentile = axis.DirectionalPercentile;
        var tone = percentile <= 20d ? "danger" : "warning";
        var label = "Vigilance";
        var sentence = $"{BuildAxisWeaknessDescription(axis)} ({percentile:0.#}e percentile favorable)";
        return new CoachCardViewModel(CoachCardType.Weakness, label, sentence, tone, HighlightAxisKeys: [axis.Key]);
    }

    private static CoachCardViewModel? BuildRoleCard(
        PositionProfileResponseDto profile,
        IReadOnlyList<PositionProfileAxisViewModel> axes,
        IReadOnlyList<PositionProfileAxisViewModel> strongAxes,
        IReadOnlyList<PositionProfileAxisViewModel> weakAxes)
    {
        var role = DetermineRoleExpectation(profile);
        var dominantTheme = DetermineDominantTheme(axes);
        var topHighlightKeys = strongAxes.Take(2).Select(axis => axis.Key).ToList();

        if (role == RoleExpectation.Balanced)
        {
            return new CoachCardViewModel(
                CoachCardType.Role,
                "Role",
                "Profil polyvalent, utile dans plusieurs sequences sans surcharge d un seul levier.",
                "info",
                HighlightAxisKeys: topHighlightKeys);
        }

        if (role == RoleExpectation.Goalkeeper)
        {
            var aligned = dominantTheme == Theme.Goalkeeper || strongAxes.Any(axis => IsGoalkeeperAxis(axis));
            return new CoachCardViewModel(
                CoachCardType.Role,
                "Role",
                aligned
                    ? "Role gardienne conforme, avec lecture dominante sur les tirs subis."
                    : "Role gardienne a surveiller: les axes dominants ne sont pas ceux du poste.",
                aligned ? "positive" : "warning",
                HighlightAxisKeys: topHighlightKeys);
        }

        if (role == RoleExpectation.Finisher)
        {
            var aligned = dominantTheme == Theme.Offense || strongAxes.Any(axis => IsOffenseAxis(axis));
            return new CoachCardViewModel(
                CoachCardType.Role,
                "Role",
                aligned
                    ? "Role offensive coherent, avec impact principal sur la finition."
                    : "Role offensive moins net: les forces glissent vers un autre registre.",
                aligned ? "positive" : "warning",
                HighlightAxisKeys: topHighlightKeys);
        }

        if (role == RoleExpectation.Creator)
        {
            var aligned = dominantTheme == Theme.Offense || strongAxes.Any(axis => IsPlaymakingAxis(axis));
            return new CoachCardViewModel(
                CoachCardType.Role,
                "Role",
                aligned
                    ? "Role de creatrice principale, avec impact visible sur la mise en rythme."
                    : "Role de creatrice a confirmer: le profil dominant pointe ailleurs.",
                aligned ? "positive" : "warning",
                HighlightAxisKeys: topHighlightKeys);
        }

        if (role == RoleExpectation.Defender)
        {
            var aligned = dominantTheme == Theme.Defense || strongAxes.Any(axis => IsDefenseAxis(axis));
            return new CoachCardViewModel(
                CoachCardType.Role,
                "Role",
                aligned
                    ? "Role defensif coherent, avec lecture propre des duels et des lignes de passe."
                    : "Role defensif moins clair: le profil dominant est ailleurs.",
                aligned ? "positive" : "warning",
                HighlightAxisKeys: topHighlightKeys);
        }

        return new CoachCardViewModel(
            CoachCardType.Role,
            "Role",
            "Profil complet, sans domination unique: utile pour un staff qui cherche un profil modulable.",
            "info",
            HighlightAxisKeys: topHighlightKeys);
    }

    private static CoachCardViewModel? BuildTacticalCard(
        PositionProfileResponseDto profile,
        IReadOnlyList<PositionProfileAxisViewModel> axes,
        double volumeScore,
        double efficiencyScore,
        double offenseScore,
        double defenseScore,
        double riskScore)
    {
        var player = profile.SelectedPlayer!;

        if (volumeScore - efficiencyScore >= 18d)
        {
            var volumeAxis = axes
                .Where(IsVolumeAxis)
                .OrderByDescending(axis => axis.DirectionalPercentile)
                .FirstOrDefault();
            var efficiencyAxis = axes
                .Where(IsEfficiencyAxis)
                .OrderBy(axis => axis.DirectionalPercentile)
                .FirstOrDefault();

            return new CoachCardViewModel(
                CoachCardType.Tactical,
                "Tactique",
                efficiencyAxis is null
                    ? "Volume eleve avec rendement plus fragile que la mediane."
                    : $"{volumeAxis?.Label ?? "Volume"} monte, mais {efficiencyAxis.Label} reste sous la reference.",
                "warning",
                HighlightAxisKeys: BuildHighlightKeys(volumeAxis, efficiencyAxis));
        }

        if (efficiencyScore - volumeScore >= 18d)
        {
            var efficiencyAxis = axes
                .Where(IsEfficiencyAxis)
                .OrderByDescending(axis => axis.DirectionalPercentile)
                .FirstOrDefault();
            var volumeAxis = axes
                .Where(IsVolumeAxis)
                .OrderBy(axis => axis.DirectionalPercentile)
                .FirstOrDefault();

            return new CoachCardViewModel(
                CoachCardType.Tactical,
                "Tactique",
                volumeAxis is null
                    ? "Faible usage mais impact fort, profil a densifier sans le surcharger."
                    : $"{player.FullName} reste discret, mais {efficiencyAxis?.Label ?? "efficacite"} compense la faible exposition.",
                "positive",
                HighlightAxisKeys: BuildHighlightKeys(volumeAxis, efficiencyAxis));
        }

        if (defenseScore >= 60d && riskScore <= 45d)
        {
            var defenseAxis = axes
                .Where(IsDefenseAxis)
                .OrderByDescending(axis => axis.DirectionalPercentile)
                .FirstOrDefault();
            var riskAxis = axes
                .Where(IsRiskAxis)
                .OrderBy(axis => axis.DirectionalPercentile)
                .FirstOrDefault();

            return new CoachCardViewModel(
                CoachCardType.Tactical,
                "Tactique",
                riskAxis is null
                    ? "Impact defensif stable, sans cout disciplinaire visible."
                    : $"{defenseAxis?.Label ?? "Defense"} tient, tandis que {riskAxis.Label} reste sous controle.",
                "good",
                HighlightAxisKeys: BuildHighlightKeys(defenseAxis, riskAxis));
        }

        return new CoachCardViewModel(
            CoachCardType.Tactical,
            "Tactique",
            "Profil regulier: peu de ruptures majeures entre volume, rendement et controle.",
            "neutral",
            HighlightAxisKeys: axes
                .OrderByDescending(axis => axis.DirectionalPercentile)
                .Take(2)
                .Select(axis => axis.Key)
                .ToList());
    }

    private static IReadOnlyList<CoachCardViewModel> BuildAlertCards(
        PositionProfileResponseDto profile,
        IReadOnlyList<PositionProfileAxisViewModel> axes,
        IReadOnlyList<PositionProfileAxisViewModel> strongAxes,
        IReadOnlyList<PositionProfileAxisViewModel> weakAxes)
    {
        var cards = new List<CoachCardViewModel>();

        var volumeAlert = BuildVolumeEfficiencyAlert(axes);
        if (volumeAlert is not null)
        {
            cards.Add(volumeAlert);
        }

        var outlierAlert = BuildOutlierAlert(axes);
        if (outlierAlert is not null)
        {
            cards.Add(outlierAlert);
        }

        var roleAlert = BuildRoleAlert(profile, axes, strongAxes, weakAxes);
        if (roleAlert is not null)
        {
            cards.Add(roleAlert);
        }

        return cards
            .OrderByDescending(card => GetAlertPriority(card))
            .Take(2)
            .ToList();
    }

    private static CoachCardViewModel? BuildVolumeEfficiencyAlert(IReadOnlyList<PositionProfileAxisViewModel> axes)
    {
        var volumeAxis = axes
            .Where(IsVolumeAxis)
            .OrderByDescending(axis => axis.DirectionalPercentile)
            .FirstOrDefault();

        var efficiencyAxis = axes
            .Where(IsEfficiencyAxis)
            .OrderBy(axis => axis.DirectionalPercentile)
            .FirstOrDefault();

        if (volumeAxis is null || efficiencyAxis is null)
        {
            return null;
        }

        if (volumeAxis.DirectionalPercentile < 70d || efficiencyAxis.DirectionalPercentile > 40d)
        {
            return null;
        }

        return new CoachCardViewModel(
            CoachCardType.Alert,
            "Alerte",
            $"Volume eleve mais rendement en retrait: {volumeAxis.Label} et {efficiencyAxis.Label} ne vont pas dans le meme sens.",
            "warning",
            HighlightAxisKeys: [volumeAxis.Key, efficiencyAxis.Key]);
    }

    private static CoachCardViewModel? BuildOutlierAlert(IReadOnlyList<PositionProfileAxisViewModel> axes)
    {
        var outlierAxis = axes
            .OrderByDescending(axis => Math.Abs(axis.DirectionalPercentile - 50d))
            .FirstOrDefault();

        if (outlierAxis is null || Math.Abs(outlierAxis.DirectionalPercentile - 50d) < 35d)
        {
            return null;
        }

        var tone = outlierAxis.DirectionalPercentile >= 85d ? "positive" : "danger";
        var sentence = outlierAxis.DirectionalPercentile >= 85d
            ? $"Outlier fort sur {outlierAxis.Label}: ecart net par rapport au reste du profil."
            : $"Outlier negatif sur {outlierAxis.Label}: ecart net a contenir.";

        return new CoachCardViewModel(
            CoachCardType.Alert,
            "Alerte",
            sentence,
            tone,
            HighlightAxisKeys: [outlierAxis.Key]);
    }

    private static CoachCardViewModel? BuildRoleAlert(
        PositionProfileResponseDto profile,
        IReadOnlyList<PositionProfileAxisViewModel> axes,
        IReadOnlyList<PositionProfileAxisViewModel> strongAxes,
        IReadOnlyList<PositionProfileAxisViewModel> weakAxes)
    {
        var role = DetermineRoleExpectation(profile);
        if (role == RoleExpectation.Balanced)
        {
            return null;
        }

        var dominantTheme = DetermineDominantTheme(axes);
        var expectedTheme = GetExpectedTheme(role);

        if (dominantTheme == expectedTheme)
        {
            return null;
        }

        var label = "Alerte";
        var sentence = role switch
        {
            RoleExpectation.Goalkeeper => "Profil gardienne atypique: les axes dominants ne sont pas les axes de reference du poste.",
            RoleExpectation.Finisher => "Profil offensif a surveiller: les axes dominants glissent vers un autre registre.",
            RoleExpectation.Creator => "Profil de creatrice a confirmer: le volume decisif ne domine pas encore.",
            RoleExpectation.Defender => "Profil defensif a surveiller: la lecture principale n est pas encore defensivement ancree.",
            _ => "Role atypique par rapport au poste reference."
        };

        return new CoachCardViewModel(
            CoachCardType.Alert,
            label,
            sentence,
            "warning",
            HighlightAxisKeys: strongAxes.Concat(weakAxes).Take(3).Select(axis => axis.Key).ToList());
    }

    private static int GetAlertPriority(CoachCardViewModel card)
    {
        return card.Type switch
        {
            CoachCardType.Alert => card.Tone == "danger" ? 3 : 2,
            CoachCardType.Strength => 1,
            CoachCardType.Weakness => 1,
            _ => 0
        };
    }

    private static string BuildPerformanceLevel(double overallAverage, int strongCount, int weakCount)
    {
        if (overallAverage >= 70d && strongCount >= 3 && weakCount <= 1)
        {
            return "Elite";
        }

        if (overallAverage >= 50d)
        {
            return "Au-dessus de la mediane";
        }

        return "Sous le standard";
    }

    private static string BuildProfileType(
        double volumeScore,
        double efficiencyScore,
        double offenseScore,
        double defenseScore,
        double riskScore,
        bool isGoalkeeperProfile)
    {
        if (isGoalkeeperProfile)
        {
            return efficiencyScore >= 60d && riskScore >= 50d
                ? "Gardienne a fort impact"
                : "Gardienne a reguler";
        }

        if (volumeScore - efficiencyScore >= 15d)
        {
            return "Fort volume / faible efficacite";
        }

        if (efficiencyScore - volumeScore >= 15d)
        {
            return "Faible usage / fort impact";
        }

        if (offenseScore >= 60d && defenseScore >= 60d)
        {
            return "Profil complet";
        }

        if (defenseScore >= 60d && offenseScore <= 50d)
        {
            return "Profil defensif";
        }

        return "Profil equilibre";
    }

    private static string BuildContextualInsight(
        PositionProfileResponseDto profile,
        IReadOnlyList<PositionProfileAxisViewModel> axes,
        double volumeScore,
        double efficiencyScore,
        double offenseScore,
        double defenseScore,
        double riskScore,
        IReadOnlyList<PositionProfileAxisViewModel> strongAxes,
        IReadOnlyList<PositionProfileAxisViewModel> weakAxes,
        IReadOnlyList<CoachCardViewModel> alertCards,
        string profileType)
    {
        _ = profile;
        _ = offenseScore;

        if (alertCards.Count > 0)
        {
            return alertCards[0].Sentence;
        }

        if (volumeScore - efficiencyScore >= 15d)
        {
            var volumeAxis = axes.Where(IsVolumeAxis).OrderByDescending(axis => axis.DirectionalPercentile).FirstOrDefault();
            var efficiencyAxis = axes.Where(IsEfficiencyAxis).OrderBy(axis => axis.DirectionalPercentile).FirstOrDefault();
            return $"{volumeAxis?.Label ?? "Volume"} sursollicite, mais {efficiencyAxis?.Label ?? "rendement"} reste en retrait.";
        }

        if (efficiencyScore - volumeScore >= 15d)
        {
            var efficiencyAxis = axes.Where(IsEfficiencyAxis).OrderByDescending(axis => axis.DirectionalPercentile).FirstOrDefault();
            return $"{efficiencyAxis?.Label ?? "Efficacite"} porte le profil, avec un volume encore mesurable.";
        }

        if (defenseScore >= 60d && riskScore <= 45d)
        {
            var defenseAxis = axes.Where(IsDefenseAxis).OrderByDescending(axis => axis.DirectionalPercentile).FirstOrDefault();
            return $"{defenseAxis?.Label ?? "Defense"} tient le profil, sans cout disciplinaire majeur.";
        }

        if (strongAxes.Count > 0 && weakAxes.Count > 0)
        {
            return $"{strongAxes[0].Label} cree l avantage, mais {weakAxes[0].Label} reste a stabiliser.";
        }

        return profileType == "Profil equilibre"
            ? "Profil regulier, sans rupture majeure entre volume et rendement."
            : "Lecture stable, avec quelques leviers secondaires a surveiller.";
    }

    private static string BuildQuickSummary(
        string performanceLevel,
        string profileType,
        IReadOnlyList<PositionProfileAxisViewModel> strongAxes,
        IReadOnlyList<PositionProfileAxisViewModel> weakAxes,
        string contextualInsight)
    {
        var strengthText = strongAxes.Count > 0
            ? BuildAxisShortPhrase(strongAxes[0], positive: true)
            : "aucun axe vraiment dominant";
        var weaknessText = weakAxes.Count > 0
            ? BuildAxisShortPhrase(weakAxes[0], positive: false)
            : "pas de fragilite structurelle";

        return performanceLevel == "Elite"
            ? $"Profil elite: {profileType.ToLowerInvariant()}, avec {strengthText} et {weaknessText}."
            : $"{performanceLevel}, {profileType.ToLowerInvariant()}: {strengthText}, tandis que {weaknessText}.";
    }

    private static IReadOnlyList<KpiTile> BuildSnapshotKpis(
        PositionProfileResponseDto profile,
        string performanceLevel,
        string profileType,
        double overallAverage,
        double volumeScore,
        double efficiencyScore,
        IReadOnlyList<PositionProfileAxisViewModel> strongAxes,
        IReadOnlyList<PositionProfileAxisViewModel> weakAxes,
        IReadOnlyList<CoachCardViewModel> alertCards,
        string contextualInsight,
        string? season)
    {
        var performanceTone = performanceLevel switch
        {
            "Elite" => "positive",
            "Au-dessus de la mediane" => "good",
            _ => "warning"
        };

        var profileTone = profileType switch
        {
            "Fort volume / faible efficacite" => "warning",
            "Faible usage / fort impact" => "positive",
            "Profil complet" => "good",
            "Profil defensif" => "neutral",
            _ => "neutral"
        };

        var strongContext = strongAxes.Count > 0
            ? string.Join(", ", strongAxes.Take(2).Select(axis => axis.Label))
            : "Aucun axe fort";
        var weakContext = weakAxes.Count > 0
            ? string.Join(", ", weakAxes.Take(2).Select(axis => axis.Label))
            : "Aucune faiblesse nette";

        return
        [
            new KpiTile(
                "Niveau global",
                performanceLevel,
                $"Moyenne directionnelle {overallAverage:0.#}%",
                performanceTone,
                $"{profile.CohortPlayerCount} joueuses dans la cohorte"),
            new KpiTile(
                "Type de profil",
                profileType,
                $"Volume {volumeScore:0.#}% | efficacite {efficiencyScore:0.#}%",
                profileTone,
                string.IsNullOrWhiteSpace(season) ? "Toutes les saisons" : season),
            new KpiTile(
                "Axes forts",
                strongAxes.Count.ToString(),
                strongContext,
                strongAxes.Count > 0 ? "positive" : "neutral",
                strongAxes.Count > 0 ? BuildAxisReference(strongAxes[0]) : "Pas d axe au-dessus de la mediane"),
            new KpiTile(
                "Alertes",
                alertCards.Count.ToString(),
                alertCards.Count > 0 ? alertCards[0].Sentence : contextualInsight,
                alertCards.Count > 0 ? "warning" : "neutral",
                weakContext)
        ];
    }

    private static string BuildAxisReference(PositionProfileAxisViewModel axis)
    {
        return $"{axis.Label} ({axis.DirectionalPercentile:0.#}e pct favorable)";
    }

    private static string BuildAxisShortPhrase(PositionProfileAxisViewModel axis, bool positive)
    {
        var percentile = axis.DirectionalPercentile;

        if (positive)
        {
            return percentile >= 90d
                ? $"{axis.Label} d elite"
                : $"{axis.Label} en soutien";
        }

        return percentile <= 20d
            ? $"{axis.Label} a corriger"
            : $"{axis.Label} a contenir";
    }

    private static string BuildAxisPositiveDescription(PositionProfileAxisViewModel axis)
    {
        return axis.DirectionalPercentile >= 90d
            ? $"{axis.Label} d elite"
            : axis.DirectionalPercentile >= 75d
                ? $"{axis.Label} tres fort"
                : $"{axis.Label} solide";
    }

    private static string BuildAxisWeaknessDescription(PositionProfileAxisViewModel axis)
    {
        return axis.DirectionalPercentile <= 10d
            ? $"{axis.Label} sous standard"
            : axis.DirectionalPercentile <= 20d
                ? $"{axis.Label} a renforcer"
                : $"{axis.Label} fragile";
    }

    private static bool IsFiniteAxis(PositionProfileAxisViewModel axis)
    {
        return double.IsFinite(axis.PlayerValue)
            && double.IsFinite(axis.MedianValue)
            && double.IsFinite(axis.Percentile)
            && double.IsFinite(axis.MinValue)
            && double.IsFinite(axis.MaxValue);
    }

    private static double AverageOrFallback(IEnumerable<double> values, double fallback)
    {
        var list = values.ToList();
        return list.Count == 0 ? fallback : list.Average();
    }

    private static IReadOnlyList<string> BuildHighlightKeys(params PositionProfileAxisViewModel?[] axes)
    {
        return axes
            .Where(axis => axis is not null)
            .Select(axis => axis!.Key)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static RoleExpectation DetermineRoleExpectation(PositionProfileResponseDto profile)
    {
        if (profile.IsGoalkeeperProfile)
        {
            return RoleExpectation.Goalkeeper;
        }

        var positionName = profile.PositionName ?? profile.SelectedPlayer?.PositionName ?? string.Empty;
        var normalized = positionName.Trim().ToLowerInvariant();

        if (normalized.Contains("gardien") || normalized.Contains("goal"))
        {
            return RoleExpectation.Goalkeeper;
        }

        if (normalized.Contains("ailier"))
        {
            return RoleExpectation.Finisher;
        }

        if (normalized.Contains("pivot"))
        {
            return RoleExpectation.Defender;
        }

        if (normalized.Contains("demi") || normalized.Contains("centre") || normalized.Contains("arriere"))
        {
            return RoleExpectation.Creator;
        }

        return RoleExpectation.Balanced;
    }

    private static Theme DetermineDominantTheme(IReadOnlyList<PositionProfileAxisViewModel> axes)
    {
        var counts = new Dictionary<Theme, int>
        {
            [Theme.Offense] = axes.Count(IsOffenseAxis),
            [Theme.Defense] = axes.Count(IsDefenseAxis),
            [Theme.Risk] = axes.Count(IsRiskAxis),
            [Theme.Goalkeeper] = axes.Count(IsGoalkeeperAxis)
        };

        return counts
            .OrderByDescending(item => item.Value)
            .ThenBy(item => item.Key)
            .First()
            .Key;
    }

    private static Theme GetExpectedTheme(RoleExpectation role)
    {
        return role switch
        {
            RoleExpectation.Goalkeeper => Theme.Goalkeeper,
            RoleExpectation.Finisher => Theme.Offense,
            RoleExpectation.Creator => Theme.Offense,
            RoleExpectation.Defender => Theme.Defense,
            _ => Theme.Offense
        };
    }

    private static bool IsOffenseAxis(PositionProfileAxisViewModel axis)
    {
        var text = GetAxisSearchText(axis);
        return text.Contains("offense", StringComparison.OrdinalIgnoreCase)
            || text.Contains("offensive", StringComparison.OrdinalIgnoreCase)
            || text.Contains("goal", StringComparison.OrdinalIgnoreCase)
            || text.Contains("assist", StringComparison.OrdinalIgnoreCase)
            || text.Contains("pass", StringComparison.OrdinalIgnoreCase)
            || text.Contains("shoot", StringComparison.OrdinalIgnoreCase)
            || text.Contains("tir", StringComparison.OrdinalIgnoreCase)
            || text.Contains("penalty", StringComparison.OrdinalIgnoreCase)
            || text.Contains("7m", StringComparison.OrdinalIgnoreCase)
            || text.Contains("creation", StringComparison.OrdinalIgnoreCase)
            || text.Contains("finition", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEfficiencyAxis(PositionProfileAxisViewModel axis)
    {
        var text = GetAxisSearchText(axis);
        return text.Contains("success", StringComparison.OrdinalIgnoreCase)
            || text.Contains("rate", StringComparison.OrdinalIgnoreCase)
            || text.Contains("effic", StringComparison.OrdinalIgnoreCase)
            || text.Contains("save", StringComparison.OrdinalIgnoreCase)
            || text.Contains("arret", StringComparison.OrdinalIgnoreCase)
            || text.Contains("conversion", StringComparison.OrdinalIgnoreCase)
            || text.Contains("precision", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsVolumeAxis(PositionProfileAxisViewModel axis)
    {
        var text = GetAxisSearchText(axis);
        return text.Contains("volume", StringComparison.OrdinalIgnoreCase)
            || text.Contains("shots", StringComparison.OrdinalIgnoreCase)
            || text.Contains("tir", StringComparison.OrdinalIgnoreCase)
            || text.Contains("goal", StringComparison.OrdinalIgnoreCase)
            || text.Contains("assist", StringComparison.OrdinalIgnoreCase)
            || text.Contains("sanction", StringComparison.OrdinalIgnoreCase)
            || text.Contains("penalty", StringComparison.OrdinalIgnoreCase)
            || text.Contains("faced", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDefenseAxis(PositionProfileAxisViewModel axis)
    {
        var text = GetAxisSearchText(axis);
        return text.Contains("def", StringComparison.OrdinalIgnoreCase)
            || text.Contains("interception", StringComparison.OrdinalIgnoreCase)
            || text.Contains("block", StringComparison.OrdinalIgnoreCase)
            || text.Contains("neutral", StringComparison.OrdinalIgnoreCase)
            || text.Contains("stop", StringComparison.OrdinalIgnoreCase)
            || text.Contains("arret", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRiskAxis(PositionProfileAxisViewModel axis)
    {
        var text = GetAxisSearchText(axis);
        return text.Contains("turnover", StringComparison.OrdinalIgnoreCase)
            || text.Contains("perte", StringComparison.OrdinalIgnoreCase)
            || text.Contains("miss", StringComparison.OrdinalIgnoreCase)
            || text.Contains("fault", StringComparison.OrdinalIgnoreCase)
            || text.Contains("conced", StringComparison.OrdinalIgnoreCase)
            || text.Contains("2 minutes", StringComparison.OrdinalIgnoreCase)
            || text.Contains("sanction", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsGoalkeeperAxis(PositionProfileAxisViewModel axis)
    {
        var text = GetAxisSearchText(axis);
        return text.Contains("goalkeeper", StringComparison.OrdinalIgnoreCase)
            || text.Contains("keeper", StringComparison.OrdinalIgnoreCase)
            || text.Contains("save", StringComparison.OrdinalIgnoreCase)
            || text.Contains("arret", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPlaymakingAxis(PositionProfileAxisViewModel axis)
    {
        var text = GetAxisSearchText(axis);
        return text.Contains("assist", StringComparison.OrdinalIgnoreCase)
            || text.Contains("pass", StringComparison.OrdinalIgnoreCase)
            || text.Contains("creation", StringComparison.OrdinalIgnoreCase)
            || text.Contains("play", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetAxisSearchText(PositionProfileAxisViewModel axis)
    {
        return $"{axis.Key} {axis.Label} {axis.Category}";
    }

    private enum RoleExpectation
    {
        Balanced,
        Goalkeeper,
        Finisher,
        Creator,
        Defender
    }

    private enum Theme
    {
        Offense,
        Defense,
        Risk,
        Goalkeeper
    }
}

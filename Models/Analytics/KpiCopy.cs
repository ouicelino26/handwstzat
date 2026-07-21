namespace HandWStat.Models.Analytics;

public static class KpiCopy
{
    private static readonly IReadOnlyDictionary<string, string> FriendlyLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Actions directes"] = "Actions decisives",
            ["Actions directes / match"] = "Actions decisives / match",
            ["Arrets / match"] = "Arrets / match",
            ["Arrets 7m"] = "Penalties arretes",
            ["Ballons valorises"] = "Actions utiles",
            ["Balance utile"] = "Actions utiles",
            ["Conversion globale"] = "Reussite au tir",
            ["Dechet tir / match"] = "Tirs perdus / match",
            ["Diff. buts / match"] = "Ecart de buts / match",
            ["Impact def."] = "Ballons gagnes",
            ["Impact def. / match"] = "Ballons gagnes / match",
            ["Indice global"] = "Niveau global",
            ["Indice technique"] = "Maitrise du ballon",
            ["Penalty"] = "Reussite penalty",
            ["Pertes techniques / match"] = "Balles perdues / match",
            ["Stop 7m"] = "Penalties arretes",
            ["Stop 7m %"] = "Penalties arretes",
            ["Taux d'arret"] = "Reussite arrets",
            ["Taux de tir"] = "Reussite au tir",
            ["Taux de tir ouvert"] = "Reussite hors penalty",
            ["Taux tir"] = "Reussite au tir",
            ["Tirs engages / match"] = "Tirs pris / match"
        };

    private static readonly IReadOnlyDictionary<string, string> FriendlySentences =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Arrets classiques et penalties."] = "Tous les tirs arretes.",
            ["Arrets des gardiennes."] = "Tirs arretes par les gardiennes.",
            ["Arrets sur penalties."] = "Penalties arretes.",
            ["Buts amenes par une passe decisive."] = "Buts crees par une passe.",
            ["Buts marques."] = "Buts inscrits.",
            ["Buts rapportes a tous les tirs engages."] = "Part des tirs qui finissent en but.",
            ["Buts rapportes a tous les tirs."] = "Part des tirs qui finissent en but.",
            ["Charge de dechet technique a surveiller."] = "Balles perdues a surveiller.",
            ["Creation directe et finalisation du collectif."] = "Buts et passes decisives.",
            ["Creation directe."] = "Passes qui amenent un but.",
            ["Defense, gardienne et discipline reunies."] = "Ce que la joueuse apporte sans marquer.",
            ["Ecart moyen au score."] = "Ecart de buts moyen.",
            ["Efficacite gardienne sur les penalties."] = "Reussite sur les penalties adverses.",
            ["Efficacite gardienne sur tous les tirs subis."] = "Part des tirs adverses arretes.",
            ["Efficacite sur penalties."] = "Reussite sur penalty.",
            ["Finition hors penalty."] = "Reussite sur les tirs dans le jeu.",
            ["Impact defensif au contact."] = "Ballons gagnes en defense.",
            ["Interceptions, contres, neutralisations."] = "Ballons gagnes ou tirs bloques.",
            ["Lecture gardienne."] = "Performance de gardienne.",
            ["Lecture gardienne sur tous les tirs subis."] = "Part des tirs adverses arretes.",
            ["Lecture gardienne sur les tirs subis."] = "Part des tirs adverses arretes.",
            ["Lecture gardienne sur la ligne."] = "Part des tirs adverses arretes.",
            ["Mauvaises passes, pertes de balle, fautes techniques et passages en force."] = "Balles perdues ou fautes avec le ballon.",
            ["Mesure la maitrise collective score apres score."] = "Ecart moyen avec les adversaires.",
            ["Niveau de discipline collective."] = "Fautes et sanctions a surveiller.",
            ["Part des actions de balle qui se terminent positivement."] = "Actions de balle qui finissent bien.",
            ["Production offensive directe."] = "Buts marques.",
            ["Production utile vs dechets techniques."] = "Bonnes actions comparees aux pertes.",
            ["Production utile vs déchets techniques."] = "Bonnes actions comparees aux pertes.",
            ["Repere resultat sur le cycle filtre."] = "Resultat moyen sur la periode.",
            ["Solidite defensive du collectif."] = "Buts encaisses en moyenne.",
            ["Tirs rates, contres et penalties engages."] = "Tous les tirs qui ne finissent pas en but.",
            ["Tirs rates, penalties rates et tirs contres."] = "Tirs qui ne finissent pas en but.",
            ["Tirs rate, penalties rate et tirs contres."] = "Tirs qui ne finissent pas en but.",
            ["Volume d'actions positives."] = "Bonnes actions au total.",
            ["Volume de rencontres analysees."] = "Matchs pris en compte.",
            ["Volume offensif moyen."] = "Buts marques en moyenne.",
            ["Volume total de rencontres analysees."] = "Matchs pris en compte.",
            ["Volume total de tirs, penalties et contres engages."] = "Nombre de tirs pris."
        };

    public static string Label(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return string.Empty;
        }

        var cleaned = label.Trim();
        return FriendlyLabels.TryGetValue(cleaned, out var friendly)
            ? friendly
            : SimplifyInline(cleaned);
    }

    public static string Caption(string? caption)
    {
        if (string.IsNullOrWhiteSpace(caption))
        {
            return string.Empty;
        }

        var cleaned = caption.Trim();
        return FriendlySentences.TryGetValue(cleaned, out var friendly)
            ? friendly
            : SimplifyInline(cleaned);
    }

    public static string? Context(string? context)
    {
        if (string.IsNullOrWhiteSpace(context))
        {
            return null;
        }

        return SimplifyInline(context.Trim());
    }

    private static string SimplifyInline(string text)
    {
        return text
            .Replace("actions directes", "actions decisives", StringComparison.OrdinalIgnoreCase)
            .Replace("ballons valorises", "actions utiles", StringComparison.OrdinalIgnoreCase)
            .Replace("conversion globale", "reussite au tir", StringComparison.OrdinalIgnoreCase)
            .Replace("dechet technique", "perte de balle", StringComparison.OrdinalIgnoreCase)
            .Replace("dechets techniques", "pertes de balle", StringComparison.OrdinalIgnoreCase)
            .Replace("dechets", "actions perdues", StringComparison.OrdinalIgnoreCase)
            .Replace("déchets", "actions perdues", StringComparison.OrdinalIgnoreCase)
            .Replace("impact defensif", "ballons gagnes", StringComparison.OrdinalIgnoreCase)
            .Replace("impact def.", "ballons gagnes", StringComparison.OrdinalIgnoreCase)
            .Replace("penalties arretes", "penalties stoppes", StringComparison.OrdinalIgnoreCase)
            .Replace("penaltys", "penalties", StringComparison.OrdinalIgnoreCase)
            .Replace("tirs engages", "tirs pris", StringComparison.OrdinalIgnoreCase)
            .Replace("tirs subis", "tirs adverses", StringComparison.OrdinalIgnoreCase);
    }
}

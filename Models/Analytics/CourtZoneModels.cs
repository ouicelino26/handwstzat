// Models/Analytics/CourtZoneModels.cs
namespace HandWStat.Models.Analytics;

public enum PlayerCourtAttackType { All, OpenPlay, SevenMeter }
public enum PlayerCourtDisplayMode { Volume, Efficiency }
public enum PlayerCourtScene { ShotZones, TriggerZones }
public enum PlayerCourtShotResult { All, Goal, Save, OffTarget, Blocked }

public sealed record CourtZoneStat(
    string Key,
    string Label,
    double Rate,
    int Attempts,
    int Successes,
    bool SampleReliable,
    bool IsAvailable,
    IReadOnlyList<OutcomeCount> Outcomes)
{
    public int Failures => Math.Max(Attempts - Successes, 0);
}

public static class ZoneNameCatalog
{
    private static readonly Dictionary<string, string> ShotZoneLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        { "BG1",  "Bas gauche" },
        { "BD1",  "Bas droite" },
        { "BG2",  "Bas centre-gauche" },
        { "BD2",  "Bas centre-droite" },
        { "BG3",  "Bord gauche bas" },
        { "BD3",  "Bord droit bas" },
        { "BG4",  "Angle gauche" },
        { "BD4",  "Angle droit" },
        { "BG5",  "Centre gauche" },
        { "BD5",  "Centre droit" },
        { "BG6",  "Milieu gauche" },
        { "BD6",  "Milieu droit" },
        { "BG7",  "Centre-bas gauche" },
        { "BD7",  "Centre-bas droit" },
        { "BG8",  "Centre-haut gauche" },
        { "BD8",  "Centre-haut droit" },
        { "BG9",  "Haut gauche bas" },
        { "BD9",  "Haut droit bas" },
        { "BG10", "Bord gauche haut" },
        { "BD10", "Bord droit haut" },
        { "BG11", "Haut centre-gauche" },
        { "BD11", "Haut centre-droit" },
        { "BG12", "Haut gauche" },
        { "BD12", "Haut droit" },
    };

    // NB: keys here are the VISUAL keys (after ToVisualTriggerKey inversion TG<->TD)
    private static readonly Dictionary<string, string> TriggerZoneLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        { "TD6",  "Aile gauche lointaine" },
        { "TG6",  "Aile droite lointaine" },
        { "TD9",  "Arriere gauche" },
        { "TG9",  "Arriere droit" },
        { "TD5",  "Aile gauche" },
        { "TG5",  "Aile droite" },
        { "TD8",  "Demi-centre gauche" },
        { "TG8",  "Demi-centre droit" },
        { "TD7",  "Centre gauche pivot" },
        { "TG7",  "Centre droit pivot" },
        { "TD4",  "Aile gauche avancee" },
        { "TG4",  "Aile droite avancee" },
        { "TD3",  "Bord gauche avance" },
        { "TG3",  "Bord droit avance" },
        { "TD2",  "Couloir gauche" },
        { "TG2",  "Couloir droit" },
        { "TD1",  "Zone 6m gauche" },
        { "TG1",  "Zone 6m droite" },
    };

    public static string GetShotZoneLabel(string key) =>
        ShotZoneLabels.TryGetValue(key, out var label) ? label : key;

    public static string GetTriggerZoneLabel(string visualKey) =>
        TriggerZoneLabels.TryGetValue(visualKey, out var label) ? label : visualKey;

    public static IReadOnlyDictionary<string, string> AllShotZoneLabels => ShotZoneLabels;
    public static IReadOnlyDictionary<string, string> AllTriggerZoneLabels => TriggerZoneLabels;
}

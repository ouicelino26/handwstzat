using Microsoft.AspNetCore.Components.Routing;

namespace HandWStat.Models.Navigation;

public sealed record AppNavigationItem(
    string Href,
    string Label,
    string Kicker,
    string Description,
    string Index,
    NavLinkMatch Match,
    string IconMarkup,
    bool ShowOnMobile = true,
    bool ShowInRail = true);

public static class AppNavigationCatalog
{
    public static IReadOnlyList<AppNavigationItem> All { get; } =
    [
        new(
            "dashboard",
            "Today",
            "Brief",
            "Signaux, tendances et decisions du scope actif.",
            "01",
            NavLinkMatch.All,
            """<svg viewBox="0 0 24 24"><path d="M4 18V9m5 9V5m5 13v-7m5 7V3"></path><path d="M3 21h18"></path></svg>"""),
        new(
            "players",
            "Athletes",
            "Individuals",
            "Profils, trajectoires et lecture du role.",
            "02",
            NavLinkMatch.Prefix,
            """<svg viewBox="0 0 24 24"><circle cx="12" cy="7" r="3"></circle><path d="M5 21c.5-5 3-8 7-8s6.5 3 7 8M4 11l3 1m13-1-3 1"></path></svg>"""),
        new(
            "teams",
            "Squads",
            "Collectives",
            "Effectifs, production et dynamique collective.",
            "03",
            NavLinkMatch.Prefix,
            """<svg viewBox="0 0 24 24"><path d="M12 3 4 7v10l8 4 8-4V7l-8-4Z"></path><path d="M8 10h8M8 14h8"></path></svg>"""),
        new(
            "matches",
            "Games",
            "Match rooms",
            "Rencontres, scenarios, zones et effectifs.",
            "04",
            NavLinkMatch.Prefix,
            """<svg viewBox="0 0 24 24"><rect x="3" y="5" width="18" height="14" rx="2"></rect><path d="M3 10h18M8 5v14m8-14v14"></path></svg>"""),
        new(
            "compare",
            "Lab",
            "Experiments",
            "Comparaisons et benchmarks de poste.",
            "05",
            NavLinkMatch.Prefix,
            """<svg viewBox="0 0 24 24"><path d="M8 3v5l-4 9a3 3 0 0 0 3 4h10a3 3 0 0 0 3-4l-4-9V3"></path><path d="M7 14h10M9 3h6"></path></svg>"""),
        new(
            "export",
            "Export",
            "Data out",
            "Generer un classeur Excel avec les metriques analytiques du perimetre choisi.",
            "06",
            NavLinkMatch.Prefix,
            """<svg viewBox="0 0 24 24"><path d="M14 3v4a1 1 0 0 0 1 1h4"></path><path d="M17 21H7a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h7l5 5v11a2 2 0 0 1-2 2Z"></path><path d="M12 11v6m-3-3 3 3 3-3"></path></svg>"""),
        new(
            "hall-of-legends",
            "Legends",
            "Hall of Fame",
            "Les meilleures joueuses de toutes les saisons confondues.",
            "07",
            NavLinkMatch.Prefix,
            """<svg viewBox="0 0 24 24"><path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"></path></svg>""")
    ];

    public static IReadOnlyList<AppNavigationItem> Primary { get; } =
        All.Where(item => item.ShowInRail).ToList();
}

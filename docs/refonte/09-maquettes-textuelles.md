# 09 — Maquettes textuelles

## SCR-01 — Dashboard (desktop, mode Analyste)

```
┌──────────────────────────────────────────────────────────────────────────┐
│ SIDEBAR (300px)              │ STAGE                                     │
│                              │                                           │
│ ◉ HandWStat                  │ TOPBAR                                    │
│   Handball analytics         │ [☰] Tableau de bord          [Démo][⏻]   │
│                              │ Synthèse & tendances                      │
│ ■ Tableau     Synthèse       ├───────────────────────────────────────────│
│ ○ Joueuses    Fiches         │                                           │
│ ○ Comparaison Face à face    │ ┌────────────────────────────────────────┐│
│ ○ Équipes     Collectif      │ │ FILTRES RAPIDES (toujours visibles)    ││
│ ○ Matchs      Rencontres     │ │ [Starligue ▼] [2024-25 ▼] [Filtres+] ││
│                              │ └────────────────────────────────────────┘│
│ ● Session active             │ Périmètre : Starligue · Saison 2024-25   │
│   Connecté : admin           │                                           │
└──────────────────────────────│ ┌──────────┐ ┌──────────┐ ┌──────────┐  │
                               │ │ 11       │ │ 2.8/m    │ │ 64%      │  │
                               │ │ ÉQUIPES  │ │ CADENCE  │ │ JEU PREP │  │
                               │ └──────────┘ └──────────┘ └──────────┘  │
                               │                                           │
                               │ ─── KPI LIGUE ────────────────────────   │
                               │ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐    │
                               │ │2.8 ▲ │ │64% ▲ │ │0.9 ▲ │ │4.2 ▲ │    │
                               │ │buts/m│ │prép. │ │inter │ │arrêts│    │
                               │ └──────┘ └──────┘ └──────┘ └──────┘    │
                               │                                           │
                               │ ─── CLASSEMENTS ──────────────────────   │
                               │ ┌─────────────────┐ ┌─────────────────┐ │
                               │ │ TOP BUTEUSES     │ │ TOP EFFICACITÉ  │ │
                               │ │ 1. N. Legrand 94 │ │ 1. N. Legrand % │ │
                               │ │ 2. L. Martin  81 │ │ 2. L. Martin  % │ │
                               │ │ 3. C. Durand  75 │ │ ...             │ │
                               │ └─────────────────┘ └─────────────────┘ │
                               │ ┌─────────────────┐ ┌─────────────────┐ │
                               │ │ [Buts ▼] CLASSMT│ │ TOP INTERCEPTIONS│ │
                               │ │ ...              │ │ ...              │ │
                               │ └─────────────────┘ └─────────────────┘ │
                               │                                           │
                               │ ─── MATCHS RÉCENTS ────────────────────  │
                               │ [Metz 31-24 Nantes] [Brest 28-28 Paris]  │
                               │ [Dijon 26-22 Nice ] [Nantes 34-27 Toulon]│
                               │                                           │
                               │ ─── LENS ─── [Club] [Analyste*] [Joueuse]│
                               │                                           │
                               │ ─── SPOTLIGHT JOUEUSE ─────────────────  │
                               │ ┌──────────────────────┬───────────────┐ │
                               │ │ PROFIL               │ DIRECTORY     │ │
                               │ │ N. Legrand           │ [Recherche..] │ │
                               │ │ Metz HB · ARD · 27   │               │ │
                               │ │ France · 19 matchs   │ N. Legrand    │ │
                               │ │                      │ L. Martin     │ │
                               │ │ KPI GRILLE           │ C. Durand     │ │
                               │ │ ┌────┐┌────┐┌────┐   │ ...           │ │
                               │ │ │KPI ││KPI ││KPI │   │               │ │
                               │ │ └────┘└────┘└────┘   │               │ │
                               │ │                      │               │ │
                               │ │ GRAPHIQUES           │               │ │
                               │ │ [Radar impact]       │               │ │
                               │ │ [Tendance/match]     │               │ │
                               │ │                      │               │ │
                               │ │ ZONES SPATIALES      │               │ │
                               │ │ [Carte buts SVG]     │               │ │
                               │ │ [Carte déclencheurs] │               │ │
                               │ └──────────────────────┴───────────────┘ │
                               └──────────────────────────────────────────┘
```

**États :**
- Chargement : skeleton par section (KPI, classements, matchs, spotlight)
- Vide (aucun joueur dans le scope) : StateCard "Aucune donnée" + suggestions de filtres
- Erreur réseau : StateCard avec bouton "Réessayer"

---

## SCR-01 — Dashboard (mobile)

```
┌─────────────────────────┐
│ [☰] Tableau de bord [⏻] │  ← topbar mobile
├─────────────────────────┤
│ [Starligue] [2024-25]   │  ← filtres rapides
│ + Filtres avancés       │
├─────────────────────────┤
│ ┌───────┐ ┌───────┐     │
│ │ 11    │ │ 2.8/m │     │  ← hero metrics (2 colonnes)
│ │équipes│ │cadence│     │
│ └───────┘ └───────┘     │
├─────────────────────────┤
│ TOP BUTEUSES            │  ← classement vertical
│ 1. N. Legrand — 94 buts │
│ 2. L. Martin  — 81 buts │
├─────────────────────────┤
│ MATCHS RÉCENTS          │
│ [Metz 31-24 Nantes]     │
│ [Brest 28-28 Paris 92]  │
├─────────────────────────┤
│ SPOTLIGHT               │
│ N. Legrand · Metz · ARD │
│ KPI ─ graphique ─ zones │
├─────────────────────────┤
│ [🏠][👤][⚡][🛡][📋]    │  ← bottom nav
└─────────────────────────┘
```

---

## SCR-03 — Fiche joueuse (desktop)

```
┌─────────────────────────────────────────────────────────────────────────┐
│ SIDEBAR              │ STAGE                                            │
│ (collapsed / icons)  │                                                  │
│                      │ [☰] Joueuses / N. Legrand              [Démo][⏻]│
│                      ├──────────────────────────────────────────────────│
│                      │ ┌──────────────────────────────────────────────┐ │
│                      │ │ FILTRES                                      │ │
│                      │ │ [Recherche...] [Starligue▼] [Metz HB▼] [ARD▼]│ │
│                      │ └──────────────────────────────────────────────┘ │
│                      │                                                  │
│                      │ ┌────────────────┬───────────────────────────┐  │
│                      │ │ LISTE JOUEUSES │ PROFIL ACTIF              │  │
│                      │ │ (scrollable)   │                           │  │
│                      │ │                │ N. Legrand                │  │
│                      │ │ ● N. Legrand   │ Metz HB · ARD · 27 ans   │  │
│                      │ │ ○ L. Martin    │ France · 19 matchs joués  │  │
│                      │ │ ○ C. Durand    │                           │  │
│                      │ │ ○ S. Petit     │ ┌────┐ ┌────┐ ┌────┐ ┌───┐│  │
│                      │ │ ...            │ │ 94 │ │ 32 │ │67% │ │19 ││  │
│                      │ │                │ │buts│ │pass│ │tir │ │mat││  │
│                      │ │                │ └────┘ └────┘ └────┘ └───┘│  │
│                      │ │                │                           │  │
│                      │ │                │ LENS [Club][Analyste*][Joueuse]│
│                      │ │                │                           │  │
│                      │ │                │ ─── ONGLETS STATS ──────  │  │
│                      │ │                │ [Offense*][Défense][Passes]│  │
│                      │ │                │ [Sanctions][Gardienne]    │  │
│                      │ │                │                           │  │
│                      │ │                │ KPI OFFENSE               │  │
│                      │ │                │ ┌────┐┌────┐┌────┐┌────┐  │  │
│                      │ │                │ │    ││    ││    ││    │  │  │
│                      │ │                │ └────┘└────┘└────┘└────┘  │  │
│                      │ │                │                           │  │
│                      │ │                │ GRAPHIQUES                │  │
│                      │ │                │ [Profil d'impact]         │  │
│                      │ │                │ [Tendance/match]          │  │
│                      │ │                │                           │  │
│                      │ │                │ ZONES SPATIALES           │  │
│                      │ │                │ ┌──────────┬──────────┐   │  │
│                      │ │                │ │Carte buts│Déclenche.│   │  │
│                      │ │                │ └──────────┴──────────┘   │  │
│                      │ │                │                           │  │
│                      │ │                │ TABLEAU DÉTAILLÉ          │  │
│                      │ │                │ [par match, triable]      │  │
│                      │ └────────────────┴───────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## SCR-04 — Comparaison (desktop, section Graphes)

```
┌─────────────────────────────────────────────────────────┐
│ TOPBAR                                                  │
│ Comparaison — Face à face           [Démo][⏻]           │
├─────────────────────────────────────────────────────────┤
│ ┌────────────────────────────────────────────────────┐  │
│ │ FILTRES [Starligue▼] [Saison▼] [Pos.▼] [2 joueur▼]│  │
│ │ + Filtres avancés             [Comparer] [Refresh] │  │
│ └────────────────────────────────────────────────────┘  │
│                                                         │
│ LENS [Club] [Analyste*] [Joueuse]                       │
│                                                         │
│ SÉLECTION DES JOUEUSES                                  │
│ ┌──────────────┐ ┌──────────────┐                       │
│ │ Joueuse 1    │ │ Joueuse 2    │                       │
│ │ [Recherche]  │ │ [Recherche]  │                       │
│ │              │ │              │                       │
│ │ ● N. Legrand │ │ ● L. Martin  │                       │
│ │   Metz · ARD │ │   Brest · ALG│                       │
│ │ [Retirer]    │ │ [Retirer]    │                       │
│ │ liste...     │ │ liste...     │                       │
│ └──────────────┘ └──────────────┘                       │
│                                                         │
│ Périmètre actif : Starligue · 2024-25 · 2 joueuses      │
│                                                         │
│ [Synthèse] [Graphes*] [Tableau]                         │
│                                                         │
│ ┌───────────────────────────────────────────────────┐  │
│ │ RADAR COMPARÉ                                     │  │
│ │        Buts/m                                     │  │
│ │    Sanct.  Passes/m                               │  │
│ │  Taux tir    Impact def.                          │  │
│ │    Arrêts/m                                       │  │
│ └───────────────────────────────────────────────────┘  │
│                                                         │
│ ┌───────────────────┐ ┌───────────────────┐             │
│ │ VOLUMES CLÉS      │ │ EFFICACITÉ        │             │
│ │ [Bar groupé]      │ │ [Bar groupé]      │             │
│ └───────────────────┘ └───────────────────┘             │
└─────────────────────────────────────────────────────────┘
```

---

## SCR-06 — Matchs — Détail match (desktop)

```
┌─────────────────────────────────────────────────────────┐
│ TOPBAR                                                  │
│ Matchs / J18 · Metz vs Nantes             [Démo][⏻]    │
├─────────────────────────────────────────────────────────┤
│ ┌────────────────────────────────────────────────────┐  │
│ │ FILTRES [Starligue▼] [Saison▼] [Toutes éq.▼]      │  │
│ └────────────────────────────────────────────────────┘  │
│                                                         │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ ◀ Metz HB           31 — 24           Nantes       │ │
│ │   J18 · 09 avril 2026 · Starligue                  │ │
│ └─────────────────────────────────────────────────────┘ │
│                                                         │
│ ─── KPI RÉSUMÉ (10 tiles) ────────────────────────────  │
│ ┌────┐┌────┐┌────┐┌────┐┌────┐┌────┐┌────┐┌────┐       │
│ │ 55 ││ 7  ││64% ││60% ││ 85 ││122 ││ 37 ││ 18 │       │
│ │buts││écart││jeu ││ball││def.││tirs││dcht││prtc│       │
│ └────┘└────┘└────┘└────┘└────┘└────┘└────┘└────┘       │
│                                                         │
│ [KPI Résumé*] [Timeline] [Joueuses] [Spatial]           │
│                                                         │
│ ─── TIMELINE ──────────────────────────────────────    │
│ ┌───────────────────────────────────────────────────┐  │
│ │ Score evolution (area chart)                      │  │
│ │ 00:00 ──────── 30:00 (mi-temps) ──────── 60:00   │  │
│ │ Metz ────────────────────────────────────────────│  │
│ │ Nantes ──────────────────────────────────────────│  │
│ └───────────────────────────────────────────────────┘  │
│                                                         │
│ ─── KPI SCÉNARIO (8 tiles) ──────────────────────────   │
│ [16-14 MT] [+7 final] [+9 Metz] [+3 Nantes] ...        │
│                                                         │
│ MOMENTS CLÉS                                            │
│ 30:00 Mi-temps        16 - 14  Score vestiaire          │
│ 42:15 Run +5 Metz     24 - 18  Série sans réponse       │
│ 60:00 Fin             31 - 24  Score final              │
└─────────────────────────────────────────────────────────┘
```

---

## États particuliers communs

### Chargement (skeleton)
```
┌─────────────────────────────────┐
│ ████░░░░░░░░░░░ CHARGEMENT...  │
│ ┌────┐ ┌────┐ ┌────┐           │
│ │████│ │████│ │████│           │  ← skeleton KPI tiles
│ └────┘ └────┘ └────┘           │
│ ┌──────────────────────────┐   │
│ │██████████░░░░░░░░░░░░░░░│   │  ← skeleton graphique
│ └──────────────────────────┘   │
└─────────────────────────────────┘
```

### État vide
```
┌────────────────────────────────┐
│                                │
│         [○ icône]              │
│  Aucune donnée disponible      │
│  Essayez d'élargir les filtres │
│                                │
│  [Réinitialiser les filtres]   │
│                                │
└────────────────────────────────┘
```

### Erreur de connexion
```
┌────────────────────────────────┐
│                                │
│         [⚠ icône]              │
│  Impossible de charger         │
│  les données demandées.        │
│  Vérifiez votre connexion.     │
│                                │
│  [Réessayer] [Se reconnecter]  │
│                                │
└────────────────────────────────┘
```

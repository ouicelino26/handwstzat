# 07 — Architecture UX cible

## 1. Principes directeurs (inspirés de Wyscout, SofaScore, StatsBomb, NBA Stats)

| Principe | Application |
|---|---|
| **Densité maîtrisée** | Hiérarchiser : hero → contexte → détail. L'information secondaire est accessible, pas imposée. |
| **Navigation rapide** | Accès en 1 clic aux vues principales. Profil joueuse accessible depuis n'importe quelle liste. |
| **Comparaison immédiate** | Les classements et le spotlight sont côte à côte sur le dashboard. |
| **Lecture progressive** | Dashboard : résumé → détail joueuse → fiche complète. Matchs : liste → KPI → timeline. |
| **Filtres efficaces** | Filtres globaux persistants visible en haut de page. Filtres avancés dans un panneau coulissant. |
| **Contextualisation** | Toujours afficher le périmètre actif (saison, équipe, compétition). |
| **Accès rapide** | Raccourci clavier pour la recherche. Bouton "ouvrir fiche" depuis tout élément joueuse. |

---

## 2. Architecture de l'information cible

```
HandWStat
├── Tableau de bord           /dashboard
│   ├── Hero métriques        (3 KPI ligue en premier plan)
│   ├── Classements           (2×2 + sélecteur métrique)
│   ├── Matchs récents        (6 derniers, cards compactes)
│   └── Spotlight joueuse     (sélection + profil complet)
│
├── Joueuses                  /players
│   ├── Liste filtrée         (recherche + filtres)
│   └── Fiche détaillée       /players/{id} (ou drawer)
│       ├── Identité          (profil + contexte)
│       ├── KPI synthèse
│       ├── Statistiques      (onglets Offense / Défense / Passes / Sanctions / Gardienne)
│       ├── Graphiques        (selon onglet + lens)
│       └── Zones spatiales
│
├── Comparaison               /compare
│   ├── Sélection             (2–6 joueuses)
│   ├── Résumé KPI
│   ├── Graphes
│   └── Tableau
│
├── Équipes                   /teams
│   ├── Sélection équipe
│   ├── Fiche équipe          /teams/{id}
│   │   ├── KPI collectif
│   │   ├── Effectif
│   │   └── Matchs
│
├── Matchs                    /matches
│   ├── Liste filtrée
│   └── Détail match          /matches/{id}
│       ├── KPI résumé
│       ├── Timeline
│       └── Joueuses + Spatial
│
└── Démo                      /demo
    └── Dashboard pré-chargé (données statiques)
```

---

## 3. Navigation principale cible

### Structure

```
┌─────────────────────────────────────────────────────────────┐
│  SIDEBAR (desktop)                                          │
│  ┌────────────────┐  ┌──────────────────────────────────┐  │
│  │ Logo + toggle  │  │ Tableau  |  Joueuses  |  Compare │  │
│  │                │  │ Équipes  |  Matchs               │  │
│  │ Nav items avec │  └──────────────────────────────────┘  │
│  │ icône + label  │                                        │
│  │ + description  │                                        │
│  │                │                                        │
│  │ Session info   │                                        │
│  └────────────────┘                                        │
└─────────────────────────────────────────────────────────────┘

MOBILE : bottom nav rail (icônes + labels courts)
```

### Améliorations par rapport à l'existant

| Problème actuel | Solution cible |
|---|---|
| Sidebar collapse sans indication visuelle du state | Tooltip au survol des icônes en mode collapsed |
| Démo accessible uniquement depuis la topbar | Démo accessible depuis la sidebar (lien distinct) |
| Titre de page dans la topbar mais pas dans la sidebar | Breadcrumb dans la topbar pour le contexte de sous-page |

---

## 4. Stratégie filtres

### Filtres globaux (persistants, visibles)

Affichés en permanence en haut de page dans une barre compacte :
- Compétition (dropdown)
- Saison (dropdown)

Ces deux filtres conditionnent la majorité des vues. Ils doivent être **toujours visibles** sans action de l'utilisateur.

### Filtres secondaires (panel accordéon)

Accessible via un bouton "Filtres avancés" → panneau qui s'ouvre :
- Équipe, Poste (pour les vues joueuses)
- Journée, De/À (pour les vues chronologiques)
- Attaque, Défense, Déclencheur, Nuance de tir (avec tooltip d'explication)

### Indicateur de filtres actifs

La barre `ScopeSummaryBar` est conservée et déplacée **juste sous les filtres globaux**, toujours visible.

---

## 5. Navigation dans les entités

### Depuis n'importe quelle liste de joueuses

- Clic sur la fiche → ouvre un **drawer** latéral avec le profil complet (KPI + stats)
- Bouton "Ouvrir la fiche" dans le drawer → navigue vers `/players/{id}` (vue dédiée)

### Depuis les classements du dashboard

- Clic sur une joueuse dans un classement → met à jour le spotlight ET affiche un mini-lien "Voir la fiche"
- Ce comportement existant est conservé et enrichi

### Depuis les matchs

- Clic sur une joueuse dans le tableau du match → ouvre le drawer de la fiche joueuse avec contexte match
- Clic sur l'équipe → navigue vers `/teams/{id}`

---

## 6. Gestion des pages de détail

### Pattern drawer (nouveauté)

Pour les fiches joueuses et les détails de match ouverts depuis une liste :
- Drawer coulissant depuis la droite (sur desktop)
- Fullscreen sur mobile
- Fermeture par Escape ou clic sur overlay
- Conservation du contexte de liste (scroll position maintenu)

### Pattern page dédiée (conservé)

Pour la fiche joueuse complète (`/players/{id}`) et le détail match (`/matches/{id}`) :
- Vue pleine page avec breadcrumb
- Accessible via URL directe

---

## 7. Stratégie responsive

| Breakpoint | Layout |
|---|---|
| < 640px (mobile) | Single column, bottom nav rail, drawer fullscreen |
| 640–1024px (tablette) | 2 colonnes, sidebar collapsée, drawer demi-écran |
| > 1024px (desktop) | Sidebar + contenu, drawer tiers-droit, grilles multi-colonnes |

### Règles spécifiques
- **Dashboard** : les classements passent en colonne unique sur mobile, le spotlight passe sous les classements
- **Compare** : maximum 2 colonnes de slots sur tablette, 1 sur mobile
- **Tableaux** : scroll horizontal sur mobile, épinglage de la première colonne

---

## 8. Raccourcis et efficacité

| Raccourci | Action |
|---|---|
| `/` ou `Ctrl+K` | Ouvrir la recherche universelle |
| `Esc` | Fermer le drawer actif |
| `Tab` | Navigation clavier standard |
| Clic sur kicker d'un classement | Changer la métrique du classement |

---

## 9. Changements par rapport à l'existant — tableau de parité UX

| Élément | Statut | Justification |
|---|---|---|
| Sidebar toggle (collapsed/expanded) | Conservé amélioré | Ajout tooltip collapsed |
| Bottom nav mobile | Conservé | OK existant |
| AudienceLensSelector | Conservé mis en valeur | Déplacé en position plus visible |
| Filter drawer (details/summary) | Conservé amélioré | Séparation filtres globaux / avancés |
| ScopeSummaryBar | Conservé déplacé | Toujours visible, sous les filtres globaux |
| Navigation dashboard → fiche joueuse | Amélioré | Ajout drawer + lien direct |
| Spotlight directory | Conservé amélioré | Recherche améliorée, lien "voir la fiche" |
| États de chargement | Amélioré | Ajout skeleton UI par section |
| Breadcrumb | Nouveau | Contexte de navigation pour les pages profondes |
| Drawer joueuse | Nouveau | Progressive disclosure sans navigation complète |

# 03 — Cartographie fonctionnelle

## 1. Écrans et routes

| ID | Écran | Route | Layout | Accès |
|---|---|---|---|---|
| SCR-01 | Accueil / Login | `/` | PublicLayout | Public |
| SCR-02 | Tableau de bord | `/dashboard` | MainLayout | Auth requis |
| SCR-03 | Fiche joueuse | `/players` | MainLayout | Auth requis |
| SCR-04 | Comparaison | `/compare` | MainLayout | Auth requis |
| SCR-05 | Équipes | `/teams` | MainLayout | Auth requis |
| SCR-06 | Matchs | `/matches` | MainLayout | Auth requis |
| SCR-07 | Démo | `/demo` | MainLayout | Public |
| SCR-08 | Non trouvé | `*` | — | Public |

---

## 2. SCR-01 — Accueil / Login

**Objectif :** Permettre la connexion à l'application ou accéder au mode démo.

### Zones

| Zone | Contenu |
|---|---|
| Landing hero | Logo + titre + présentation de l'application |
| Login card | Formulaire login (identifiant + mot de passe + bouton "Connexion") |
| Lien démo | Bouton "Voir la démo" → `/demo` |

### Actions
| ID | Action | Déclencheur | Résultat |
|---|---|---|---|
| F-001 | Connexion | Soumission formulaire / bouton | POST auth/login → session JWT → redirection `/dashboard` |
| F-002 | Accès démo | Bouton "Voir la démo" | Navigation vers `/demo` |

### États
- **Chargement :** bouton désactivé, spinner implicite
- **Erreur :** message d'erreur affiché sous le formulaire
- **Succès :** redirection automatique

---

## 3. SCR-02 — Tableau de bord

**Objectif :** Vue synthétique de la ligue : métriques globales, classements, matchs récents, spotlight joueuse.

### Zones

| Zone | Composant | Contenu |
|---|---|---|
| Filtres globaux | `<details>` filter-drawer | Compétition, équipe, poste, saison, journée, de/à, attaque, défense, déclencheur, nuance de tir |
| Barre de contexte | `ScopeSummaryBar` | Résumé des filtres actifs |
| Métriques hero | KpiTile (cartes) | Équipes actives, cadence offensive, jeu préparé |
| KPI ligue | KpiTileGrid | Buts/match, buts préparés, interceptions/match, arrêts/match, pertes/match, sanctions/match |
| Classement buts | Liste ranking | Top N buteuses |
| Classement efficiency | Liste ranking | Top N taux de réussite |
| Classement demandé | Liste ranking | Métrique choisie (sélecteur) |
| Classement interceptions | Liste ranking | Top 5 interceptions |
| Matchs récents | MatchCard | 6 derniers matchs |
| Lens selector | AudienceLensSelector | Club / Analyste / Joueuse |
| Spotlight joueuse | Section dédiée | Profil, KPI, graphiques, zones spatiales |
| Sélecteur de joueuse | Liste directory | Toutes les joueuses avec recherche |

### Actions
| ID | Action | Déclencheur |
|---|---|---|
| F-003 | Appliquer filtres | Changement valeur filtre → rechargement |
| F-004 | Changer métrique classement | Sélecteur métrique |
| F-005 | Changer taille classement | Sélecteur Top N (3–12) |
| F-006 | Sélectionner joueuse spotlight | Clic liste directory |
| F-007 | Changer lens de lecture | AudienceLensSelector |
| F-008 | Cliquer match récent | MatchCard → navigation `/matches` |
| F-009 | Cliquer joueuse classement | PlayerRankingItem → sélection spotlight |
| F-010 | Rafraîchir données | Bouton rafraîchir |

### Sections spotlight (selon lens)

**Club :** KPI synthèse (3–4 KPI clés), tendance par match
**Analyste :** KPI complets, graphiques (profile radar, rendement, tendance), zones spatiales (carte buts + déclencheurs)
**Joueuse :** KPI clairs, tableau matchs récents

---

## 4. SCR-03 — Fiche joueuse

**Objectif :** Explorer le profil complet d'une joueuse avec toutes ses statistiques.

### Zones

| Zone | Contenu |
|---|---|
| Sélection joueuse | Recherche + liste filtrée (compétition, équipe, poste, saison, journée) |
| Contexte joueuse | Mini-fiche : équipe, poste, âge, nationalité, matchs joués |
| Cartes de contexte | 8 métriques clés (matchs, buts, passes, interceptions, arrêts, pertes, taux tir, taux penalty) |
| KPI principaux | Grille de KpiTile |
| Graphiques | Profil d'impact (radar/bar), Rendement (barres), Tendance par match (area) |
| Zones spatiales | Carte de buts + carte de déclencheurs (SVG heatmap) |
| Tableau détaillé | Colonnes par statistique avec tri, recherche et coloration |
| Onglets statistiques | Offense, Défense, Passes, Sanctions, Gardienne |

### Actions
| ID | Action | Déclencheur |
|---|---|---|
| F-011 | Rechercher joueuse | Saisie texte + filtres |
| F-012 | Sélectionner joueuse | Clic liste |
| F-013 | Naviguer onglet statistique | Onglets (Offense / Défense / Passes / Sanctions / Gardienne) |
| F-014 | Trier tableau | Clic entête colonne |
| F-015 | Changer lens | AudienceLensSelector |
| F-016 | Appliquer filtre scope | Compétition, équipe, saison, journée |

---

## 5. SCR-04 — Comparaison

**Objectif :** Mettre plusieurs profils face-à-face sur un périmètre identique.

### Zones

| Zone | Contenu |
|---|---|
| Filtres de comparaison | Compétition, équipe, poste, saison, journée, nb joueuses (2–6) |
| Slots de sélection | N cartes de sélection (une par joueuse), chacune avec recherche + liste |
| Barre de contexte | ScopeSummaryBar |
| Switch section | Synthèse / Graphes / Tableau |
| Synthèse | Mini-fiche par joueuse + BarGaugeKpiGrid + KpiTileGrid |
| Graphes | Radar comparé, Volumes clés, Efficacité, Production par match |
| Tableau | Toutes les colonnes statistiques, toutes les joueuses, tri par colonne |

### Actions
| ID | Action | Déclencheur |
|---|---|---|
| F-017 | Sélectionner joueuse dans slot | Clic item liste |
| F-018 | Retirer joueuse du slot | Bouton "Retirer" |
| F-019 | Changer nb de slots | Sélecteur nb joueuses |
| F-020 | Lancer comparaison | Bouton "Comparer" → POST api/Stats/compare/players |
| F-021 | Rafraîchir liste joueuses | Bouton "Rafraîchir la liste" |
| F-022 | Changer section résultats | Boutons Synthèse / Graphes / Tableau |
| F-023 | Trier tableau comparaison | Clic entête colonne |
| F-024 | Changer lens | AudienceLensSelector |

---

## 6. SCR-05 — Équipes

**Objectif :** Consulter les statistiques d'une équipe, son effectif et ses matchs.

### Zones

| Zone | Contenu |
|---|---|
| Sélection équipe | Dropdown équipe + filtres (compétition, saison, journée) |
| Fiche équipe | Nom, compétition |
| Cartes de contexte | Matchs, victoires, nuls, défaites, buts pour, buts contre |
| KPI équipe | Points/match, taux victoire, diff. buts/match, buts marqués/match, buts encaissés/match, ballons valorisés, arrêts/match, sanctions/match |
| Effectif | Tableau joueuses avec stats globales (buts, passes, interceptions, arrêts) + tri |
| Matchs | Liste des matchs de l'équipe |
| Graphiques | Barres comparatives, tendance collective |

### Actions
| ID | Action | Déclencheur |
|---|---|---|
| F-025 | Sélectionner équipe | Dropdown |
| F-026 | Appliquer filtres scope | Compétition, saison, journée |
| F-027 | Trier effectif | Clic entête colonne |
| F-028 | Cliquer joueuse de l'effectif | Navigation vers fiche joueuse |
| F-029 | Cliquer match de l'équipe | Navigation vers détail match |

---

## 7. SCR-06 — Matchs

**Objectif :** Parcourir les rencontres et analyser le détail d'un match.

### Zones — Liste

| Zone | Contenu |
|---|---|
| Filtres | Compétition, équipe, saison, journée, de/à |
| Liste matchs | MatchCard pour chaque rencontre (équipes, score, date, journée) |

### Zones — Détail match (accordéon ou sous-vue)

| Zone | Contenu |
|---|---|
| En-tête | Score, équipes, date, journée |
| KPI résumé | 10 KpiTile (buts cumulés, écart final, jeu préparé, ballons valorisés, actions def., tirs engagés, déchet tir, pertes techniques, stop 7m, scoreuses 3+) |
| KPI timeline | 8 KpiTile (score mi-temps, écart final, lead max eq.1, lead max eq.2, renversements, run max eq.1, run max eq.2, buts 2e MT) |
| Timeline graphique | Graphique de score en temps réel (ApexCharts area) |
| Moments clés | Liste MatchTimelineMoment (mi-temps, renversements, runs, moment final) |
| Insights de phase | 1re MT, 2e MT, dernier 10', Run clé |
| Joueuses | Tableau des joueuses avec stats (tri possible) |
| Spatial | Carte spatiale du match par équipe |

### Actions
| ID | Action | Déclencheur |
|---|---|---|
| F-030 | Filtrer matchs | Dropdown filtre |
| F-031 | Sélectionner match | Clic MatchCard |
| F-032 | Fermer détail match | Clic fermer ou autre match |
| F-033 | Changer équipe dans vue spatiale | Filtre équipe spatial |
| F-034 | Trier tableau joueuses du match | Clic entête |

---

## 8. SCR-07 — Démo

**Objectif :** Permettre l'exploration de l'interface sans authentification, avec des données fictives.

### Comportement
- Charge `DemoDataFactory.Create()` avec 3 profils fixes (ARD, ALG, GB)
- Même interface que le dashboard mais banner "Mode aperçu actif"
- Sélection joueuse disponible dans les 3 profils fixes
- Aucun appel API
- Accès depuis le bouton "Démo" dans la topbar ou lien de la page login

---

## 9. Composants partagés

| Composant | Utilisé dans | Rôle |
|---|---|---|
| `AccessRequiredCard` | Toutes les pages protégées | Bloc "connexion requise" avec boutons Login/Démo |
| `AudienceLensSelector` | Dashboard, Players, Compare | Sélecteur Club/Analyste/Joueuse |
| `BarGaugeKpiCard` | Dashboard spotlight, Compare | KPI avec jauge visuelle jQWidgets |
| `BarGaugeKpiGrid` | Dashboard, Compare | Grille de BarGaugeKpiCard |
| `GoalKpi` | Dashboard (hero) | Carte KPI buts avec indicateur visuel |
| `KpiTileGrid` | Dashboard, Players, Compare | Grille de KpiTile (texte + tone) |
| `MatchCard` | Dashboard, Matches | Carte résumé match cliquable |
| `ScopeSummaryBar` | Dashboard, Compare, Teams | Résumé contexte de filtre |
| `StateCard` | Partout | État vide / erreur / avertissement |

---

## 10. Navigation principale

```
/ (login)
  └→ /demo (aperçu sans auth)
  └→ /dashboard (auth requis)
        └→ /players (auth requis)
        └→ /compare (auth requis)
        └→ /teams (auth requis)
        └→ /matches (auth requis)
```

La navigation principale (sidebar + mobile nav) est disponible sur toutes les pages authentifiées. Le bouton "Démo" est accessible depuis la topbar. Le bouton "Déconnexion" est dans la topbar.

---

## 11. Filtres disponibles par page

| Filtre | Dashboard | Players | Compare | Teams | Matches |
|---|---|---|---|---|---|
| Compétition | ✓ | ✓ | ✓ | ✓ | ✓ |
| Équipe | ✓ | ✓ | ✓ | ✓ | ✓ |
| Poste | ✓ | ✓ | ✓ | — | — |
| Saison | ✓ | ✓ | ✓ | ✓ | ✓ |
| Journée | ✓ | ✓ | ✓ | ✓ | ✓ |
| Date de/à | ✓ | — | — | — | ✓ |
| Attaque | ✓ | — | — | — | — |
| Défense | ✓ | — | — | — | — |
| Déclencheur | ✓ | — | — | — | — |
| Nuance tir | ✓ | — | — | — | — |
| Recherche texte | — | ✓ | ✓ | — | — |
| Métrique classement | ✓ | — | — | — | — |
| Top N | ✓ | — | — | — | — |

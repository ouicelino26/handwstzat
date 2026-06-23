# 05 — Audit UX/UI

## 1. Résumé exécutif

L'application dispose d'une **base solide** : structure CSS propre, tokens de design cohérents, navigation claire, mode démo fonctionnel. Les principaux problèmes sont de l'ordre de la **densité mal maîtrisée** (trop d'informations visibles simultanément sans hiérarchie forte), de **l'absence de progressive disclosure** sur les écrans complexes, et de **limitations de performance perceptuelle** dues à une architecture monolithique des pages.

---

## 2. Audit UX

### 2.1 Parcours et navigation

| Constat | Sévérité | Preuve |
|---|---|---|
| La page Home cumule landing + login dans un seul écran, ce qui peut créer de la confusion sur l'action principale attendue | Moyenne | `Home.razor` |
| Le tableau de bord charge tout d'un coup (17 appels API) — perception de lenteur sur connexion lente | Haute | `StatsDashboardService.LoadDashboardAsync()` |
| Aucun état de chargement progressif : tout arrive en même temps ou rien | Haute | Absence de skeleton dans Dashboard |
| La sélection du joueur spotlight est peu visible — la liste du directory est dans un panneau parmi d'autres | Haute | `Dashboard.razor` section directory |
| Le filtre avancé du dashboard (attaque, défense, déclencheur, nuance de tir) n'est pas documenté dans l'UI — un nouvel utilisateur ne sait pas à quoi ils correspondent | Haute | `Dashboard.razor` filter-drawer |
| La navigation entre le dashboard et la fiche joueuse n'est pas directe (pas de lien "ouvrir la fiche complète" depuis le spotlight) | Moyenne | Dashboard → Players : 2 clics minimum |
| Sur mobile, la liste du directory et les graphiques sont difficiles à exploiter | Haute | Absence de responsive spécifique |

### 2.2 Découvrabilité

| Constat | Sévérité |
|---|---|
| Le sélecteur AudienceLens (Club/Analyste/Joueuse) est peu mis en valeur — les utilisateurs peuvent passer à côté | Moyenne |
| Les filtres avancés sont masqués dans un `<details>` — action requise pour les voir | Faible |
| La section "Matchs" de la topbar n'indique pas qu'on peut ouvrir un détail de match | Faible |

### 2.3 Densité informationnelle

| Constat | Sévérité | Localisation |
|---|---|---|
| Le dashboard en mode Analyste affiche simultanément : métriques hero, KPI ligue, 4 classements, 6 matchs, lens, spotlight + directory — charge cognitive très élevée | Haute | `Dashboard.razor` |
| La page Players affiche graphiques + zones spatiales + tableau + onglets stats sans priorisation claire | Haute | `Players.razor` |
| La page Compare peut afficher jusqu'à 6 profils avec graphiques et tableau — lisibilité dégradée au-delà de 3 | Moyenne | `Compare.razor` |

### 2.4 États vides et erreurs

| Constat | Sévérité | Preuve |
|---|---|---|
| `StateCard` est utilisé de manière cohérente — bonne pratique | — | Partout |
| Les erreurs API ne distinguent pas les cas : timeout vs. 401 vs. erreur serveur → message générique | Moyenne | `ApiClientBase.SendAsync()` |
| Le mode démo affiche un banner "avertissement" en jaune qui prend de la place et peut être perçu comme une erreur | Faible | `DemoDataFactory.cs` (WarningMessage) |

---

## 3. Audit UI

### 3.1 Hiérarchie visuelle

| Constat | Sévérité |
|---|---|
| La hiérarchie `h1 → h3` est bien respectée dans le code | OK |
| Les `section-kicker` (uppercase, letterspacing) apportent une bonne contextualisation | OK |
| Certains panneaux ont des `<h3>` trop longs qui cassent la lecture rapide | Faible |
| La distinction entre KpiTile "neutral" et "positive" est subtile (fond légèrement coloré) — les utilisateurs daltoniens peuvent avoir du mal | Moyenne |

### 3.2 Cohérence des couleurs

| Constat | Sévérité | Preuve |
|---|---|---|
| Palette principale bien définie via variables CSS (`--primary: #e74f23`, `--secondary: #0f766e`) | OK | `app.css:9` |
| 5 tones sémantiques (neutral, positive, good, warning, danger) bien formalisés | OK | `app.css:29` |
| Le dégradé de fond de page (radial gradients + grille) est sophistiqué mais peut alourdir la perception sur les petits écrans | Faible | `app.css:68` |
| Bootstrap CSS présent dans `wwwroot/lib/bootstrap/` mais non utilisé activement — poids inutile | Faible | `wwwroot/lib/bootstrap/` |

### 3.3 Typographie

| Constat | Sévérité |
|---|---|
| Open Sans Regular embarquée — une seule graisse, pas de Bold | Moyenne — les `<strong>` utilisent le fallback système |
| La taille de corps (`clamp(1rem…)`) s'adapte mais peut être trop petite sur certaines densités | Faible |
| Bonne utilisation de `letter-spacing` et `font-weight: 800` pour les kickers | OK |

### 3.4 Tableaux et données denses

| Constat | Sévérité |
|---|---|
| `TableHeatToneHelper` colorise bien les cellules relatives — bonne pratique | OK |
| Les tableaux n'ont pas de pagination visible — chargement de 250 éléments maximum par défaut | Haute (performance) |
| Le tri est fonctionnel (`TableSortState`) mais le glyphe (↑↓) est minimal | Faible |
| Pas de possibilité de masquer/afficher des colonnes | Faible |

### 3.5 Graphiques (ApexCharts)

| Constat | Sévérité |
|---|---|
| Graphiques bien intégrés (radar, area, bar, line) | OK |
| Pas de mode de couleur accessible détecté (pas de pattern texturé ni d'icône différentiant) | Moyenne |
| Les tooltips ApexCharts sont standards | OK |
| Le graphique de timeline de score (area) est pertinent | OK |
| BarGaugeKpiCard utilise jQWidgets — bibliothèque volumineuse pour un seul type de composant | Technique |

---

## 4. Audit accessibilité

| Critère | Constat | Sévérité |
|---|---|---|
| Focus visible | `button:focus-visible` avec outline défini — OK | OK |
| Navigation clavier | Structure HTML sémantique (`<nav>`, `<main>`, `<article>`, `<section>`, `<header>`) | OK |
| Labels de formulaires | Les `<label>` wrappent les `<input>` — correct | OK |
| Alternatives textuelles | Les SVG inline ont `aria-hidden="true"` — correct pour les icônes décoratives | OK |
| Contrastes | `--primary: #e74f23` sur fond blanc = contraste ≈ 3.5:1 — **insuffisant WCAG AA (4.5:1 requis)** | **Critique** |
| Contrastes sidebar | Texte blanc sur `--bg-sidebar: #0b1420` = contraste ≈ 14:1 — OK | OK |
| Zones tactiles | `touch-action: manipulation` sur boutons — OK | OK |
| Lecteurs d'écran | Pas de rôles ARIA additionnels sur les composants complexes (graphiques, heatmaps) | Moyenne |
| Titre de page | `<PageTitle>` défini sur chaque page | OK |
| Animations | Pas de `prefers-reduced-motion` implémenté | Moyenne |

---

## 5. Audit des visualisations

| Graphique | Type | Pertinence | Problèmes |
|---|---|---|---|
| Profile d'impact (Players) | Radar/Bar | Adapté | Axes non labellisés explicitement |
| Tendance par match (Dashboard) | Area/Line | Adapté | Superposition de 4 séries peut être difficile à lire |
| Volumes clés (Compare) | Bar groupé | Adapté | OK |
| Efficacité (Compare) | Bar | Adapté | OK |
| Radar comparé (Compare) | Radar | Adapté | Peut être illisible avec 5–6 joueurs |
| Timeline de score (Matches) | Area | Pertinent | OK |
| Zones spatiales (heatmap SVG) | Custom SVG | Très pertinent | Pas de légende visible pour les taux |
| BarGauge KPI | jQWidgets gauge | Pertinent mais lourd | Bibliothèque externe volumineuse |

---

## 6. Problèmes classés par priorité

| # | Problème | Sévérité | Impact utilisateur | Impact métier | Priorité |
|---|---|---|---|---|---|
| 1 | Couleur primaire `#e74f23` insuffisante en contraste WCAG AA | Critique | Fort | Légal/conformité | P0 |
| 2 | Chargement dashboard monolithique — 17 appels simultanés sans feedback progressif | Haute | Fort | Rétention | P0 |
| 3 | Densité du dashboard en mode Analyste — surcharge cognitive | Haute | Fort | Adoption | P1 |
| 4 | Pas de skeleton/placeholder stable pendant le chargement | Haute | Moyen | Qualité perçue | P1 |
| 5 | Filtre avancé (attaque, défense, déclencheur, nuance) non documenté dans l'UI | Haute | Moyen | Adoption | P1 |
| 6 | Lien direct "ouvrir fiche complète" manquant depuis le spotlight | Moyenne | Moyen | Efficacité | P2 |
| 7 | Tableaux sans pagination explicite (max 250 items chargés) | Haute | Variable | Performance | P1 |
| 8 | Open Sans Regular uniquement — pas de graisse Bold embarquée | Moyenne | Faible | Typographie | P2 |
| 9 | `prefers-reduced-motion` non implémenté | Moyenne | Faible | Accessibilité | P2 |
| 10 | Bootstrap CSS chargé mais inutilisé | Faible | Nul | Performance bundle | P3 |
| 11 | jQWidgets (BarGauge) — bibliothèque lourde pour usage limité | Faible | Nul | Maintenabilité | P3 |

# Analytics V3 — Architecture interne

Ce document décrit la structure canonique d'Analytics V3 (phases A1-A12).
Il est destiné aux développeurs qui ajoutent ou modifient des métriques.

---

## Services canoniques

| Service | Rôle |
|---------|------|
| `AnalyticsCalculationService` | Formules pures — seul endroit pour calculer taux, /60, ratios |
| `AnalyticsQualityPolicy` | Règles qualité — seuils minimaux, affichage conditionnel |
| `AnalyticsPositionResolver` | Parsing position → `AnalyticsPosition` enum canonique |
| `GoalkeeperAnalyticsBuilder` | Métriques GK agrégées (taux arrêt, /60, etc.) |
| `SpatialAnalyticsBuilder` | Métriques spatiales par zone (CAT-23/24/25) |
| `TeamAnalyticsBuilder` | Agrégations équipe (SUM/SUM, GoalsShare) |
| `CompareAnalyticsBuilder` | Radar comparatif multi-joueurs (valeurs absolues) |
| `PositionBenchmarkBuilder` | Benchmark positionnel (percentiles API) |
| `MatchAnalyticsBuilder` | Métriques match (score officiel canonique, insights) |
| `MetricValueFormatter` | Formattage unifié UI/CSV/PDF |
| `AnalyticsMetricLineage` | Traçabilité source → API endpoint → service |

---

## Catalogue de métriques

`AnalyticsV3Catalog` est la source unique de vérité pour les 25+ métriques actives.

Chaque `AnalyticsMetricDefinition` déclare :
- `Code` (ex: `CAT-01`)
- `TechnicalName` — identifiant machine
- `DisplayName`, `Definition`, `Unit`
- `ApplicablePositions` — flags `AnalyticsPositionScope`
- `MinimumSampleCount`, `MinimumPlayingTimeMinutes`
- `HigherIsBetter`
- `Status` — Active / Experimental / Removed

`GetDictionaryEntry(code)` retourne un `MetricDictionaryEntry` enrichi (grain, catégorie, formule extraite, source).

---

## Cycle de vie d'une métrique

```
API DTO
  └─▶ AnalyticsCalculationService (calcul si pas pre-calculé)
        └─▶ AnalyticsQualityPolicy (seuil qualité)
              └─▶ Page Razor / Export (affichage uniquement)
                    └─▶ MetricValueFormatter (format fr-FR ou CSV)
```

**Règle d'or :** les composants Razor n'effectuent aucun calcul métier.
Ils appellent des services, lisent des propriétés, et formattent pour l'affichage.

---

## Modèle de position

```
positionCode (string)  ──▶  AnalyticsPositionResolver.Resolve()  ──▶  AnalyticsPosition (enum)
                                                                          └─▶  AnalyticsPositionScope (flags)
```

Positions reconnues : `GK`, `AIL`, `AR`, `DC`, `PIV`.
Codes d'alias complets dans `AnalyticsPositionResolver.ParseCode()`.
Ne jamais faire de `.Contains("ail")` directement dans Razor.

---

## Modèle de qualité

`AnalyticsQualityPolicy.EvaluateTier(sampleCount, quality, minSample)` retourne un `QualityTierResult` :

| Tier | Signification |
|------|--------------|
| `High` | Fiable, affichage plein |
| `Medium` | Acceptable mais avec contexte qualité |
| `Low` | Échantillon insuffisant — afficher avec avertissement |
| `NotApplicable` | Dénominateur zéro ou position incompatible |

Les seuils minimaux sont dans `AnalyticsV3Catalog` (pas codés en dur dans les composants).

---

## Modèle de scope

Le scope d'analyse est porté par `StatsQueryOptionsDto` :
- `CompetitionId`, `Season`, `TeamId`, `PlayerId`
- `From`, `To` (dates)
- `Day`

`AnalysisScopeService` résout le scope actuel pour les pages.

---

## Percentiles radar

**Invariant A9/A11 §7** : `NormalizeRadarValue` retourne toujours `Math.Clamp(axis.Percentile, 0, 100)`.
La normalisation min-max est **interdite**.
Le percentile est fourni par l'API (direction déjà appliquée côté serveur).

Sites concernés (tous corrigés en A9/A11/A12) :
- `PlayerSheetExportHelper.NormalizeRadarValue`
- `AnalyseTabPanel.razor.cs NormalizeRadarValue`
- `MultiRadar.razor NormalizeRadarValue`

---

## Taux canoniques

| Métrique | Formule | Source |
|----------|---------|--------|
| `ShotRate` (équipe/match) | `OfficialGoals / SUM(ShotAttempts)` | `MatchAnalyticsBuilder` |
| `SaveRate` (équipe/match) | `SUM(Saves) / SUM(ShotsFaced)` | `MatchAnalyticsBuilder` |
| `TotalSaveRate` (GK) | `TotalSaves / TotalShotsFaced * 100` | `ComputeTotalSaveRate` |
| `OpenPlaySaveRate` (GK) | `OpenPlaySaves / OpenPlayShotsFaced * 100` | `ComputeOpenPlaySaveRate` |
| `GoalsCreatedPer60` | `(TotalGoals + Assists) / PT * 60` | `ComputeGoalsCreatedPer60` |

**Règle SUM/SUM :** les taux d'équipe sont toujours calculés sur les sommes numérateur et dénominateur, jamais comme moyenne de taux individuels.

---

## Export et parité UI

`MetricValueFormatter` produit les valeurs pour :
- `FormatForUi(value, unit, "fr-FR")` — affichage (virgule décimale, suffixe unité)
- `FormatForCsv(value, unit)` — export CSV (culture invariante, pas de suffixe)
- `FormatForPdf(value, unit)` — PDF (même chose que UI)

`null` → `"—"` en UI, `string.Empty` en CSV (distinguable de `"0"`).

---

## Checklist pour ajouter une métrique (§35)

1. **Source réelle ?** — Identifier le champ DTO ou l'endpoint API
2. **Grain ?** — Player / Zone / Match / Team (enum `AnalyticsMetricGrain`)
3. **Formule ?** — Documenter dans la définition, implémenter dans `AnalyticsCalculationService`
4. **Unité ?** — `Percent` / `Per60` / `Count` / `Ratio` (enum `AnalyticsMetricUnit`)
5. **Applicable positions ?** — Flags `AnalyticsPositionScope` (jamais `.None` pour une métrique active)
6. **HigherIsBetter ?** — Défini dans `AnalyticsMetricDefinition`
7. **Seuil minimum ?** — `MinimumSampleCount` ET `MinimumPlayingTimeMinutes` dans le catalogue
8. **Qualité ?** — `AnalyticsQualityPolicy.EvaluateTier` ou `EvaluatePlayingTimeTier`
9. **Lineage ?** — Ajouter une entrée dans `AnalyticsMetricLineage.All`
10. **Export ?** — Vérifier `MetricDictionaryEntry` et parité UI/export via `MetricValueFormatter`
11. **Tests ?** — Au minimum : valeur canonique, null quand dénominateur zéro, null quand PT=0 (si /60)
12. **Parité cross-page ?** — Même service appelé depuis Players, Compare, Export

---

## Métriques retirées

`CAT-03` est marquée `Removed` avec `RemovedReason`.
Les métriques retirées restent dans `AnalyticsV3Catalog.All` mais sont absentes de `Active`.
Elles ne doivent jamais apparaître dans l'UI.

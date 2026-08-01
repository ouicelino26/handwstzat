# RANKING_VALIDATION

Date: 2026-08-02 | Branch: fix/handwstat-final-validation-v1 + fix/analytics-final-validation-v1

## Wilson Lower Bound

### Implémentation

`RankingService.WilsonLowerBound(MetricValue metric)` — z=1.96 (intervalle de confiance 95 %).

Formule : `(p + z²/2n - z*sqrt(p(1-p)/n + z²/4n²)) / (1 + z²/n)`

Les métriques de taux (shotsuccess, penaltysuccess, saverate) sont triées par la borne inférieure de Wilson plutôt que par le taux brut — garantit que les joueurs avec peu d'observations ne peuvent pas apparaître en tête du classement grâce à un petit échantillon favorable.

### Seuils minimum-sample

| Métrique | Seuil | Constante API |
|---|---|---|
| Taux de réussite au tir | 20 tentatives open play | `MinimumOpenShotsForShotSuccessRanking = 20` |
| Taux de réussite pénalty | 5 tentatives 7m | `MinimumPenaltyAttemptsForPenaltySuccessRanking = 5` |
| Taux d'arrêt gardienne | 30 tirs subis | `MinimumShotsFacedForSaveRateRanking = 30` |

### Validation par les données DB

- Top titreurs open play : toutes ≥ 230 tentatives → seuil 20 approprié
- Spécialistes pénalty : toutes ≥ 59 tentatives → seuil 5 approprié
- Gardiennes qualifiées : toutes ≥ 382 tirs subis → seuil 30 approprié

### Validation tests

| Test | Résultat |
|---|---|
| `RankingService_ShotSuccess_ExcludesPlayersBelow20Attempts` | PASS (seed data < 20 → Empty) |
| `RankingService_SaveRate_ExcludesGoalkeeperBelow30ShotsFaced` | PASS (seed data < 30 → Empty) |
| `RankingService_Goals_RankedByValueDescending` | PASS |
| `RankingService_TopNBound_NeverExceedsRequestedCount` | PASS (1/3/5/10) |

## Top-N

Le paramètre `top` est clampé à 1 minimum. La liste retournée contient au plus `top` éléments.

## Séparation gardiennes / joueuses de champ

- `RankingService.GetRankingsAsync("saverate")` ne retourne que des gardiennes (events GK uniquement)
- `RankingService.GetRankingsAsync("shotsuccess")` ne retourne que des joueuses de champ (events open play uniquement)
- Les 2 classements sont mutuellement exclusifs par conception des événements

**RANKING_STATUS = VALIDATED**

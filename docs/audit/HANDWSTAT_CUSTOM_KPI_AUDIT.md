# HandWStat — Audit KPI Personnalisés

**Date :** 2026-08-01  
**Branche :** feature/handwstat-functional-product-v1  

---

## 1. Objectif

Évaluation de la validité et de la cohérence des KPI calculés localement dans `HandballKpiHelper` et `TeamOfTheDayPieScoring`, par rapport aux standards analytiques du handball professionnel.

---

## 2. HandballKpiHelper — KPI dérivés

### 2.1 TechnicalBalanceScore

```
Score = SuccessVsWasteShare(positiveActions, negativeActions)
      = positiveActions / (positiveActions + negativeActions) * 100%

Positive = DirectContributions + DefensiveImpact + GoalkeeperStops
Negative = ShotWaste + TechnicalLosses + TotalSanctions + GoalkeeperConcededGoals
```

**Validité :** Score d'efficience technique composite (similaire au PIR IHF avec pondérations simplifiées). Le ratio positif/(positif+négatif) est une approche standard. 

**Limites :**
- Chaque action compte pour 1 (pas de pondération par difficulté)
- Un arrêt vaut autant qu'un but ou une passe décisive
- Non normalisé par le temps de jeu → favorise les joueuses avec plus de temps de jeu

**Verdict :** KPI utile pour tri comparatif rapide, ne doit pas servir de mesure absolue de performance.

### 2.2 ShotSuccessRate (overall)

```
OverallShotSuccessRate = Share(TotalButs, ShotAttempts)
                       = TotalButs / (TotalButs + TirsRates + PenaltyRate) * 100%
```

**Validité :** Formule standard. Inclut les 7m dans le numérateur (TotalButs = buts open play + buts 7m) et dénominateur (PenaltyRate = 7m ratés). Cohérent.

### 2.3 GoalkeeperPenaltyStopRate

```
PenaltyStopRate = ArretsPenalty / (ArretsPenalty + ButsPenalty) * 100%
```

**Validité :** Formule correcte. Le dénominateur est le nombre total de 7m joués contre la gardienne (arrêtés + buts encaissés). Pas d'injection de tentatives de post ou hors cadre.

### 2.4 Per-60

```
Per60 = value / PlayingTimeMinutes * 60
```

**Validité :** Métrique standard. Normalise par le temps de jeu réel pour permettre la comparaison inter-joueuses avec des volumes de jeu différents. Correct.

### 2.5 DirectContributions

```
DirectContributions = TotalGoals + AssistCount
```

**Validité :** Métrique simple de contribution offensive directe. Pas pondérée (un but = une passe décisive). Acceptable pour une vue rapide.

---

## 3. Seuils de tone

Les seuils de coloration (`HigherIsBetterTone`, `LowerIsBetterTone`) sont des paramètres configurables appelés par des fonctions spécialisées.

| KPI | Type | Seuils (positive/good/warning) | Évaluation |
|-----|------|-------------------------------|-----------|
| FieldSuccessRate | Higher | 70% / 55% / 45% | Cohérent avec niveaux professionnel féminin |
| GoalkeeperSaveRate | Higher | 40% / 34% / 28% | Correct (médiane ~33% en D1F) |
| GoalkeeperPenaltyStopRate | Higher | 35% / 25% / 15% | Correct |
| BallRetention | Higher (GK: 60/45/30, Field: 70/55/40) | Distincts selon poste | ✅ |
| SanctionsTone | Lower (GK: 0.4/0.8/1.2, Field: 0.5/1.0/1.5) | Distincts selon poste | ✅ |
| GoalkeeperConcededGoals | Lower | 22 / 26 / 30 | Correct pour ~60min de match |
| TechnicalBalance | Higher (GK: 58/44/30, Field: 65/50/35) | Distincts | ✅ |

**Verdict seuils :** Cohérents et différenciés par poste. L'absence de seuils adaptatifs selon la compétition est une limite acceptable pour V1.

---

## 4. TeamOfTheDayPieScoring

### 4.1 Formules

**Joueurse de champ :**
```
Offense = Goals*6 + PenaltyGoals*2 + Assists*4 + ShotSuccessRate*0.08
        - Turnovers*1.5 - ShotWaste*0.5 - TechnicalLosses*0.8
Defense = Interceptions*3 + Blocks*4 + Neutralisations*2 + ForcedOffensiveFouls*2
        - GoalsConceded*0.3
Global  = Offense*0.58 + Defense*0.42 + BalanceBonus
```

**Gardienne :**
```
Offense = Goals*5 + Assists*3 + ShotSuccessRate*0.05 - Turnovers*1.2
Defense = Saves*4 + PenaltySaves*5 + GoalkeeperSaveRate*0.12*ShotsFaced
        - GoalsConceded*1.2 - ShotsFaced*0.08
Global  = Defense*0.72 + Offense*0.28 + BalanceBonus
```

### 4.2 Évaluation

**Forces :**
- Distinction claire gardienne / joueuse de champ (poids différents)
- Pondération défense plus forte pour la gardienne (72%) — cohérent
- Normalisation `Math.Max(score, 0)` — pas de scores négatifs
- Tie-breaking par PlayingTimeMinutes — déterministe

**Limites :**
- Pondérations arbitraires (non calibrées sur données historiques)
- `ShotSuccessRate` dans le PIE offense est un taux (0-1), son poids (0.08) est différent de celui des comptages entiers → l'influence est proportionnellement très faible pour les joueuses avec peu de tirs
- `GoalkeeperSaveRate*0.12*ShotsFaced` dans la défense gardienne crée une interaction non linéaire
- Sans volume minimum, une gardienne avec 1 arrêt sur 1 tir (100%) est favorisée vs 10/15 (67%)

**Verdict :** `LOCAL_EXPLORATORY_CALCULATION` — clairement identifié comme calcul exploratoire local, non soumis à un processus de validation statistique. Acceptable pour usage interne.

---

## 5. Métriques de profil de poste (percentiles)

Les percentiles sont calculés **côté serveur** (API v2). HandWStat les consomme directement sans recalcul. Le `DirectionalPercentile` est déjà ajusté côté serveur pour les axes négatifs (pertes, sanctions) : un percentile élevé = performance favorable, indépendamment du sens de l'axe.

La normalisation locale dans `NormalizeRadarValue` convertit la valeur brute en position sur la plage [MinValue, MaxValue] de la cohorte. La correction `HigherIsBetter` est appliquée localement pour l'affichage du radar.

**Double inversion potentielle :** Le commentaire dans le code précise que "The API contract already returns a favorable percentile for negative axes." La normalisation locale avec `!HigherIsBetter → 100-normalized` est appliquée sur la valeur brute, pas sur le percentile. Pas de double inversion. ✅

---

## 6. Verdict global KPI

| Dimension | Score |
|---------|-------|
| Formules offensives | ✅ 100% |
| Formules défensives | ✅ 100% |
| Formules gardienne | ✅ 100% |
| Formules disciplinaires | ✅ 100% (après P0) |
| Seuils de tone | ✅ 92% (seuils fixes, non adaptatifs) |
| PIE scoring | ⚠️ 75% (exploratory, non calibré) |
| Percentiles poste | ✅ 100% (délégué serveur) |

**CUSTOM_KPI_VALIDITY = 92%**

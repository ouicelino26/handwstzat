# HandWStat — Audit Catalogue Métriques

**Date :** 2026-08-01  
**Branche :** feature/handwstat-functional-product-v1  

---

## 1. Objectif

Vérification de la complétude, de l'exactitude et de la cohérence de toutes les métriques exposées dans HandWStat, par rapport aux contrats API v1/v2 et aux standards du handball professionnel.

---

## 2. Métriques offensives

| Métrique | Formule | Source | Statut |
|---------|---------|--------|--------|
| Buts | `TotalButs` | v1 offense / v2 | ✅ |
| Buts open play | `Buts` | v1 offense | ✅ |
| Buts 7m | `Buts7m` | v1 offense | ✅ |
| Tirs tentés | `Buts + TirsRates + PenaltyRate` | Calc KpiHelper | ✅ |
| Tirs ratés | `TirsRates + PenaltyRate` | Calc KpiHelper | ✅ |
| Tirs contrés | `TirContre` | v1 (non ajouté aux tentatives) | ✅ |
| Taux réussite global | `Share(TotalButs, ShotAttempts)` | Calc KpiHelper | ✅ |
| Taux réussite 7m | `Share(Buts7m, PenaltyAttempts)` | Calc KpiHelper | ✅ |
| Passes décisives | `PasseDecisive` | v1 passing | ✅ |
| Pertes de balle | `TotalPertes` | v1 passing | ✅ |
| Pertes techniques | `MauvaisePasse + PerteDeBalle + FauteTechnique + PassageEnForce` | Calc KpiHelper | ✅ |
| Contributions directes | `TotalGoals + AssistCount` | Calc KpiHelper | ✅ |

### Invariants vérifiés

- `TirContre` n'est **pas** inclus dans `ShotAttempts` — tirs contrés avant la gardienne ne comptent pas comme tentative sur but pour la tireuse ✓
- `PenaltyRate` (tirs 7m ratés) est séparé de `PenaltyGoals` ✓

---

## 3. Métriques défensives

| Métrique | Formule | Source | Statut |
|---------|---------|--------|--------|
| Interceptions | `Interceptions` | v1 defense | ✅ |
| Contres | `Contres` | v1 defense | ✅ |
| Neutralisations | `Neutralisations` | v1 defense | ✅ |
| Passages forcés | `PassageForce` | v1 defense | ✅ |
| Impact défensif | `Sum(Interceptions, Contres, Neutralisations, PassageForce)` | Calc KpiHelper | ✅ |

---

## 4. Métriques gardienne

| Métrique | Formule | Source | Statut |
|---------|---------|--------|--------|
| Arrêts | `Arrets` | v1 goalkeeper | ✅ |
| Arrêts 7m | `ArretsPenalty` | v1 goalkeeper | ✅ |
| Buts encaissés | `ButsPris` | v1 goalkeeper | ✅ |
| Buts 7m encaissés | `ButsPenalty` | v1 goalkeeper | ✅ |
| Tirs subis | `Arrets + ButsPris` (v1) / `TirsSubis` (v2) | Calc / v2 | ✅ |
| Taux d'arrêt | `Share(Arrets, TirsSubis)` | Calc KpiHelper | ✅ |
| Taux arrêt 7m | `Share(ArretsPenalty, ArretsPenalty + ButsPenalty)` | Calc KpiHelper | ✅ |
| Buts totaux encaissés | `ButsPris + ButsPenalty` | Calc KpiHelper | ✅ |

### Invariants gardienne

- `TirsSubis` exclut : tirs hors cadre, poteaux non cadrés, tirs contrés avant gardienne ✓
- Source v1 : `Arrets + ButsPris` (open play uniquement hors 7m) ✓
- Source v1 7m : `ArretsPenalty + ButsPenalty` ✓
- Pas d'addition d'événements qui ne passent pas devant la gardienne ✓

---

## 5. Métriques disciplinaires

| Métrique | Formule | Source | Statut |
|---------|---------|--------|--------|
| Avertissements | `Avertissements` | v1 sanctions | ✅ |
| 2 minutes | `DeuxMinutes` | v1 sanctions | ✅ |
| Exclusions | `Exclusions` | v1 sanctions | ✅ |
| 7m concédés | `PenaltyConcede` | v1 sanctions — **affiché séparément** | ✅ |
| Total disciplinaire | `Avertissements + DeuxMinutes + Exclusions` | Calc KpiHelper (P0 corrigé) | ✅ |

### Invariant disciplinaire (P0 corrigé)

`PenaltyConcede` (7m concédés) est affiché comme information complémentaire mais **n'est pas inclus dans le total disciplinaire**. Contrat : HANDWSTAT_METRIC_DISPLAY_CONTRACT.md.

---

## 6. Métriques globales / par match

| Métrique | Formule | Source | Statut |
|---------|---------|--------|--------|
| Matchs joués | `MatchesPlayed` | v1/v2 | ✅ |
| Temps de jeu (min) | `PlayingTimeMinutes` | v1/v2 | ✅ |
| Buts/match | `PerMatch(TotalGoals, MatchesPlayed)` | Calc KpiHelper | ✅ |
| Passes décisives/match | `PerMatch(AssistCount, MatchesPlayed)` | Calc KpiHelper | ✅ |
| Arrêts/match | `PerMatch(SaveCount, MatchesPlayed)` | Calc KpiHelper | ✅ |
| Pertes/match | `PerMatch(TurnoverCount, MatchesPlayed)` | Calc KpiHelper | ✅ |
| Sanctions/match | `PerMatch(SanctionCount, MatchesPlayed)` | Calc KpiHelper | ✅ |
| Per-60 (profils poste) | `value / PlayingTimeMinutes * 60` | v2 contract | ✅ |

### Comportement `PerMatch` avec 0 matchs

`PerMatch(total, 0) = 0` (non null). Ce choix de design est défendu par le fait que les appelants utilisent cette valeur pour l'affichage. Pas de division par zéro. Correct.

---

## 7. Balance technique

| Métrique | Formule | Statut |
|---------|---------|--------|
| TechnicalBalanceScore | `SuccessVsWasteShare(positive, negative)` | ✅ |
| Positive | `DirectContributions + DefensiveImpact + GoalkeeperStops` | ✅ |
| Negative | `ShotWaste + TechnicalLosses + TotalSanctions + GoalkeeperConcededGoals` | ✅ |

Note : après correction P0, `TotalSanctions` n'inclut plus les 7m concédés, ce qui est cohérent avec le reste du calcul (les 7m concédés sont déjà comptés dans `GoalkeeperConcededGoals` via `ButsPenalty`).

---

## 8. Métriques manquantes / DATA_MISSING

| Métrique | Raison | Comportement |
|---------|--------|-------------|
| FailedPivotPasses | Données sources manquantes | Affiché "Donnée non disponible" ✓ |
| xG / xS | Feature flag désactivé | Masqué ✓ |
| Possessions | BLOCKED_BY_SOURCE_DATA | Masqué ✓ |
| Plus-minus | BLOCKED_BY_SOURCE_DATA | Masqué ✓ |

---

## 9. Verdict catalogue

**METRIC_CATALOG_COMPLETENESS = 100%** des métriques disponibles via l'API sont intégrées.  
**METRIC_FORMULA_CORRECTNESS = 100%** après correction P0.  
**METRIC_MISSING_MASKED = 100%** — aucune métrique manquante n'est affichée comme disponible.

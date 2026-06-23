# 04 — Règles métier et calculs statistiques

## 1. Formules de base (HandballKpiHelper — KpiModels.cs)

| ID | Nom | Formule | Fichier | Notes |
|---|---|---|---|---|
| KH-01 | PerMatch | `total / matchs` si `matchs > 0`, sinon `0` | KpiModels.cs:337 | |
| KH-02 | Ratio | `numerator / denominator` si `denominator > 0`, sinon `numerator > 0 ? numerator : 0` | KpiModels.cs:343 | Comportement non-standard si denom=0 et num>0 |
| KH-03 | Share | `(numerator / denominator) * 100` si `denominator > 0`, sinon `0` | KpiModels.cs:351 | Retourne un pourcentage |
| KH-04 | SuccessVsWasteShare | `successes / (successes + failures) * 100` si total > 0 | KpiModels.cs:357 | Ballons valorisés |
| KH-05 | FormatNumber | Arrondi à 1 décimale, affichage entier si < 0.05 d'écart | KpiModels.cs:517 | MidpointRounding.AwayFromZero |
| KH-06 | FormatPercent | `"{value:0.#}%"` | KpiModels.cs:527 | 1 décimale max |

## 2. Agrégats — Dictionnaire des actions (source : KPI_REFERENCE_DETAILED.md)

### 2.1 Actions offensives

| Terme terrain | Compteur interne | Famille |
|---|---|---|
| But (hors penalty) | `Goals` → `Buts` | Tir réussi |
| But sur penalty (7m) | `PenaltyGoals` → `Buts7m` | Tir réussi |
| Tir à côté, sur poteau, arrêté, raté | `ShotMisses` → `TirsRates` | Tir raté |
| Tir contre | `ShotMisses` ET `TirContre` | Tir raté (double comptage intentionnel) |
| Penalty sur poteau/raté/arrêté | `PenaltyMisses` → `PenaltyRate` | Penalty raté |

**TotalGoals = Goals + PenaltyGoals**  
**ShotAttempts = TotalGoals + TirsRates + PenaltyRate + TirContre**

### 2.2 Passes et pertes

| Terme terrain | Compteur interne | Famille |
|---|---|---|
| Passe décisive | `Assists` → `PasseDecisive` | Création |
| Mauvaise passe | `BadPasses` → `MauvaisePasse` | Perte |
| Perte de balle | `BallLosses` → `PerteDeBalle` | Perte |
| Faute de pied, marcher, zone, reprise dribble, refus de jeu | `TechnicalFaults` → `FauteTechnique` | Faute technique |
| Passage en force | `PassageEnForce` | Perte offensive |

**TotalPertes = MauvaisePasse + PerteDeBalle + FauteTechnique + PassageEnForce**

### 2.3 Actions défensives

| Terme terrain | Compteur interne | Famille |
|---|---|---|
| Interception | `Interceptions` | Défense directe |
| Contre réussi | `CounterSuccesses` → `Contres` | Défense |
| Neutralise l'attaquant | `Neutralisations` | Défense |
| Provoque passage en force | `ForcedPassages` → `PassageForce` | Défense |

**DefensiveImpact = Interceptions + Contres + Neutralisations + PassageForce**

### 2.4 Sanctions

| Terme terrain | Compteur interne |
|---|---|
| Exclusion | `Exclusions` |
| Avertissement | `Avertissements` |
| Deux minutes | `DeuxMinutes` |
| Penalty concédé | `PenaltyConcede` |

**TotalSanctions = Exclusions + Avertissements + DeuxMinutes + PenaltyConcede**

### 2.5 Actions gardienne

| Terme terrain | Compteur interne |
|---|---|
| Arrêt hors penalty | `Arrets` |
| Arrêt sur penalty | `ArretsPenalty` |
| But encaissé hors penalty | `ButsPris` |
| But encaissé sur penalty | `ButsPenalty` |

**SaveCount = Arrets + ArretsPenalty**  
**TirsSubis = Arrets + ArretsPenalty + ButsPris + ButsPenalty**

---

## 3. KPI Dashboard — Métriques ligue

| ID | Libellé | Formule | Source |
|---|---|---|---|
| KPI-D01 | Équipes actives | `Overview.TeamCount` | StatsOverviewDto |
| KPI-D02 | Cadence offensive | `Overview.GoalCount / Overview.MatchCount` | KH-01 |
| KPI-D03 | Jeu préparé | `Overview.AssistCount / Overview.GoalCount * 100` | KH-03 |
| KPI-D04 | Buts / match | `Overview.GoalCount / Overview.MatchCount` | KH-01 |
| KPI-D05 | Buts préparés | `Overview.AssistCount / Overview.GoalCount * 100` | KH-03 |
| KPI-D06 | Interceptions / match | `Overview.InterceptionCount / Overview.MatchCount` | KH-01 |
| KPI-D07 | Arrêts / match | `Overview.SaveCount / Overview.MatchCount` | KH-01 |
| KPI-D08 | Pertes / match | `Overview.TurnoverCount / Overview.MatchCount` | KH-01 |
| KPI-D09 | Sanctions / match | `Overview.SanctionCount / Overview.MatchCount` | KH-01 |

## 4. KPI Spotlight joueuse (champ + gardienne)

| ID | Libellé | Formule champ | Formule gardienne |
|---|---|---|---|
| KPI-S01 | Actions directes / match | `(TotalGoals + Assists) / MatchesPlayed` | `(TotalGoals + Assists) / MatchesPlayed` |
| KPI-S02 | Ballons valorisés | `Assists / (Assists + Turnovers) * 100` | Idem |
| KPI-S03 | Impact def. / match | `DefensiveImpact / MatchesPlayed` | `SaveCount / MatchesPlayed` |
| KPI-S04 | Taux de tir | `Offense.TauxReussiteTir` (fourni API) | `Offense.TauxReussiteTir` |
| KPI-S05 | Taux d'arrêt | — | `Goalkeeper.TauxArret` (fourni API) |
| KPI-S06 | Tirs ratés / match | `(TirsRates + PenaltyRate) / MatchesPlayed` | Idem |
| KPI-S07 | Buts pris / match | — | `(ButsPris + ButsPenalty) / MatchesPlayed` |
| KPI-S08 | Sanctions / match | `TotalSanctions / MatchesPlayed` | Idem |

## 5. Coloration sémantique (tones)

### Taux de réussite au tir (FieldSuccessRateTone)

| Seuil | Tone |
|---|---|
| ≥ 70% | positive |
| ≥ 55% | good |
| ≥ 45% | warning |
| < 45% | danger |

### Taux d'arrêt gardienne (GoalkeeperSaveRateTone)

| Seuil | Tone |
|---|---|
| ≥ 40% | positive |
| ≥ 34% | good |
| ≥ 28% | warning |
| < 28% | danger |

### Actions directes / match

**Gardienne :** positive ≥ 3, good ≥ 2, warning ≥ 1, danger < 1  
**Champ :** positive ≥ 5, good ≥ 4, warning ≥ 3, danger < 3

### Impact défensif / match

**Gardienne :** positive ≥ 10, good ≥ 7, warning ≥ 4  
**Champ :** positive ≥ 5, good ≥ 3, warning ≥ 1.5

### Ballons valorisés

**Gardienne :** positive ≥ 60%, good ≥ 45%, warning ≥ 30%  
**Champ :** positive ≥ 70%, good ≥ 55%, warning ≥ 40%

### Sanctions / match (LowerIsBetterTone)

**Gardienne :** positive ≤ 0.4, good ≤ 0.8, warning ≤ 1.2  
**Champ :** positive ≤ 0.5, good ≤ 1, warning ≤ 1.5

---

## 6. Heatmap spatiale (SpatialZoneVisuals.cs)

### Zones de but (field players)
- Taux brut utilisé directement (0–100%)
- Clés de zone : `BG1`–`BG12`, `BD1`–`BD12` (gauche/droite, numéros de zone)

### Zones de but (gardienne)
- Normalisation appliquée : `min=10%, max=55%`
- Formule : `(rawRate - 10) / (55 - 10) * 100` puis clamp 0–100
- Objectif : maximiser le contraste visuel dans la plage de performance réelle d'une gardienne

### Miroir des déclencheurs (SpatialZoneVisuals.ToVisualTriggerKey)
- Les clés `TG` (gauche) sont mirrorées en `TD` (droite) et vice versa
- Justification : le terrain affiché est vu depuis l'arrière de la gardienne, inversant les côtés

### Coloration cellules tableau (TableHeatToneHelper)
- Normalisation min/max sur la colonne entière
- Seuils : ≥ 0.85 → positive, ≥ 0.65 → good, ≥ 0.40 → warning, < 0.40 → danger
- Si max - min < 0.0001 → neutral (toutes les valeurs identiques)

---

## 7. KPI résumé match (MatchKpiCatalog.cs)

| ID | Libellé | Formule |
|---|---|---|
| KPI-M01 | Buts cumulés | `Team1Score + Team2Score` |
| KPI-M02 | Écart final | `|Team1Score - Team2Score|` |
| KPI-M03 | Jeu préparé | `AssistCount / max(totalScore, 1) * 100` |
| KPI-M04 | Ballons valorisés | `AssistCount / (AssistCount + TurnoverCount) * 100` |
| KPI-M05 | Actions def. | `InterceptionCount + SaveCount` |
| KPI-M06 | Tirs engagés | `Technical.ShotAttempts` |
| KPI-M07 | Déchet tir | `Technical.ShotWaste` |
| KPI-M08 | Pertes techniques | `Technical.TechnicalLosses` |
| KPI-M09 | Stop 7m | `Technical.GoalkeeperPenaltyStopRate` |
| KPI-M10 | Scoreuses 3+ buts | `count(TopScorers where Value >= 3)` |

---

## 8. KPI timeline match (MatchScenarioAnalyzer.cs)

| ID | Libellé | Formule |
|---|---|---|
| KPI-T01 | Score à la pause | Snapshot score au dernier point ≤ 30:30 |
| KPI-T02 | Écart final | `Team1Score - Team2Score` avec signe |
| KPI-T03 | Lead max eq.1 | `max(Team1Score - Team2Score)` |
| KPI-T04 | Lead max eq.2 | `max(Team2Score - Team1Score)` |
| KPI-T05 | Renversements | Nombre de changements de leader (hors égalités) |
| KPI-T06 | Run max eq.1 | Plus grande série de buts consécutifs de l'équipe 1 sans réponse |
| KPI-T07 | Run max eq.2 | Plus grande série de buts consécutifs de l'équipe 2 sans réponse |
| KPI-T08 | Buts 2e MT | `(Team1ScoreFinal - Team1ScoreHalf) + (Team2ScoreFinal - Team2ScoreHalf)` |

### Résolution du chronomètre (ResolveMatchClock)
- Événements de la 2e mi-temps identifiés si `half.Contains("2")` ou `"deux"` ou `"second"`
- Si 2e mi-temps détectée et clock < 31 min → ajouter 30 minutes
- Risque : si le champ `half` est null, l'événement est classé 1re mi-temps par défaut

---

## 9. KPI équipes

| ID | Libellé | Formule | Note |
|---|---|---|---|
| KPI-E01 | Points / match | `((Wins * 2) + Draws) / MatchesPlayed` | Règle 2pts victoire + 1pt nul. À valider selon règlement du championnat. |
| KPI-E02 | Taux de victoire | `Wins / MatchesPlayed * 100` | |
| KPI-E03 | Diff. buts / match | `(GoalsFor - GoalsAgainst) / MatchesPlayed` | |
| KPI-E04 | Buts marqués / match | `GoalsFor / MatchesPlayed` | |
| KPI-E05 | Buts encaissés / match | `GoalsAgainst / MatchesPlayed` | |
| KPI-E06 | Ballons valorisés | `AssistCount / (AssistCount + TurnoverCount) * 100` | |
| KPI-E07 | Arrêts / match | `SaveCount / MatchesPlayed` | |
| KPI-E08 | Sanctions / match | `SanctionCount / MatchesPlayed` | |

---

## 10. Règles de filtrage — SmartFilterCatalog

**GetCompetitions(constrain: true)** : retourne uniquement les compétitions présentes dans les matchs chargés. Si aucune correspondance → retourne toutes les compétitions.

**GetTeams(constrain: true)** : retourne uniquement les équipes présentes dans les matchs (Team1Id ou Team2Id). Si aucune correspondance → retourne toutes les équipes.

**GetPositions(constrain: true)** : retourne uniquement les postes portés par les joueuses chargées. Si aucune correspondance → retourne tous les postes.

---

## 11. Règles de classement (RankingMetricCatalog)

Métriques disponibles : `goals`, `interceptions`, `assists`, `shotsuccess`, `penaltysuccess`, `saves`, `saverate`, `turnovers`, `sanctions`

Si la métrique demandée n'est pas dans la liste → repli sur `"goals"`.

Top N : clampé entre 3 et 12 (valeur par défaut : 8).

---

## 12. Résolution du joueur spotlight

Priorité de sélection (StatsDashboardService.ResolveSelectedPlayerId) :
1. `filters.SpotlightPlayerId` si présent et dans la liste des joueurs chargés
2. Premier PlayerId > 0 parmi topScorers puis requestedRanking
3. Premier joueur de la liste globale
4. Si aucun → DashboardSnapshot sans spotlight (état vide)

---

## 13. Cas limites identifiés

| ID | Cas | Comportement actuel | Risque |
|---|---|---|---|
| CL-01 | MatchesPlayed = 0 | KH-01 retourne 0 (pas de division par zéro) | OK |
| CL-02 | Denominator = 0 sur KH-02 (Ratio) | Retourne `numerator > 0 ? numerator : 0` | Comportement inhabituel (pas 0 ni infini) |
| CL-03 | Taux de tir = 0% | Tone "danger" affiché même pour gardienne qui ne tire pas | Faux positif visuel |
| CL-04 | Événement match sans champ `half` | Classé 1re mi-temps → décalage possible de la timeline | Risque de données |
| CL-05 | Token JWT expiré | 401 → InvalidOperationException → affiché comme erreur de données | UX dégradée |
| CL-06 | GoalZone sans correspondance | Rate = 0, Attempts = 0, affiché en gris | OK (comportement intentionnel) |
| CL-07 | Comparaison avec < 2 joueuses | StateCard "en attente" | OK |
| CL-08 | Points/match avec règle championnat différente | Calcul fixe 2pts/1pt — peut être faux | Risque métier à valider |

# Match Summary V2 — Metric Map

| METRIC_CODE | LABEL | SECTION | TEAM_SOURCE | NUMERATOR | DENOMINATOR | DIRECTION | AVAILABILITY_RULE | PRIMARY_DISPLAY | SECONDARY_EVIDENCE |
|-------------|-------|---------|-------------|-----------|-------------|-----------|-------------------|-----------------|-------------------|
| HALFTIME | Mi-temps | Context | SelectedMatchTimeline | — | — | Neutral | timeline.Count >= 2 | Score mi-temps | — |
| MAX_LEAD | Plus gros écart | Context | SelectedMatchTimeline | max(abs(T1-T2)) | — | Neutral | timeline.Count >= 2 | +N (TeamLabel) | — |
| LEAD_CHANGES | Changements de leader | Context | SelectedMatchTimeline | count(sign changes) | — | Neutral | timeline.Count >= 2 | N | label |
| TIES | Égalités | Context | SelectedMatchTimeline | count(T1==T2, not 0-0 initial) | — | Neutral | timeline.Count >= 2 | N | — |
| TOP_RUN | Run principal | Context | SelectedMatchTimeline | max consecutive goals same team | — | Neutral | timeline.Count >= 2 | +N (TeamLabel) | — |
| GOALS | Buts | Attack | MatchSummaryDto.Team1Score / Team2Score | goals | — | HigherIsBetter | Score disponible | integer | — |
| ASSISTS | Passes décisives | Attack | SUM(PlayerGlobalStatsDto.AssistCount) per team | count | — | HigherIsBetter | PlayersLoaded | integer | — |
| SHOT_RATE | Taux de tir | Attack | SUM(TotalGoals)/SUM(ShotAttempts) per team | goals | attempts | HigherIsBetter | ShotAttempts > 0 else ZeroDenominator | F1 % | goals/attempts |
| 7M_PENALTIES_WON | 7m obtenus | Attack | SUM(PenaltyAttempts) per team | count | — | HigherIsBetter | PlayersLoaded | integer | — |
| INTERCEPTIONS | Interceptions | Defense | SUM(InterceptionCount) per team | count | — | HigherIsBetter | PlayersLoaded | integer | — |
| SAVE_RATE | Taux d'arrêt | Defense | SUM(SaveCount)/SUM(ShotsFaced) per team | saves | shots_faced | HigherIsBetter | ShotsFaced > 0 else ZeroDenominator | F1 % | saves/shots_faced |
| PENALTIES_DRAWN | 7m provoqués (def.) | Defense | — | — | — | HigherIsBetter | DataMissing | — | — |
| TURNOVERS | Pertes de balle | Mastery | SUM(TurnoverCount) per team | count | — | LowerIsBetter | PlayersLoaded | integer | — |
| BAD_PASSES | Mauvaises passes | Mastery | — (no separate field in PlayerGlobalStatsDto) | — | — | LowerIsBetter | DataMissing | — | — |
| TOTAL_SANCTIONS | Sanctions | Mastery | SUM(SanctionCount) per team (Warnings+TwoMin+Disq, PenaltiesConceded EXCLUDED) | count | — | LowerIsBetter | PlayersLoaded | integer | — |
| PENALTIES_CONCEDED | 7m concédés | Mastery | — (no separate field in PlayerGlobalStatsDto) | — | — | LowerIsBetter | DataMissing | — | — |

## Règles de calcul des taux

- **SHOT_RATE** = SUM(TotalGoals) / SUM(ShotAttempts) × 100 par équipe. Jamais moyenne de pourcentages individuels.
- **SAVE_RATE** = SUM(SaveCount) / SUM(ShotsFaced) × 100 par équipe. ShotsFaced = tirs ayant atteint la gardienne uniquement (ne pas inclure tirs hors cadre ni tirs contrés avant la gardienne).
- Dénominateur 0 → Availability = ZeroDenominator, Value = null. Jamais rendu comme "0%".

## Règles sémantiques (invariants)

- PenaltiesConceded ≠ SanctionConceded — métrique séparée, jamais ajoutée aux sanctions
- BlockedShot ≠ Save
- Neutralization ≠ Block
- OffensiveFoulDrawn ≠ Interception
- FailedPivotPass ≠ BadPass
- TotalSanctions = Warnings + TwoMinutes + Disqualifications (PenaltiesConceded EXCLU)

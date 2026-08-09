# Player Table V2 — Audit

Date : 2026-08-07  
Branche : fix/dashboard-player-table-v2

## Ancien modèle (V1)

### GlobalFieldRankingRow
| Colonne | Type | Notes |
|---|---|---|
| PlayerId | int | |
| FullName | string | |
| TeamName | string | |
| PositionId | int? | |
| PositionLabel | string | |
| IsGoalkeeper | bool | |
| MatchesPlayed | int | |
| Goals | int | = offense.TotalButs fallback player.TotalGoals |
| PenaltyGoals | int | = offense.Buts7m |
| Assists | int | = passing.PasseDecisive |
| Interceptions | int | = defense.Interceptions |
| Blocks | int | = defense.Contres |
| Neutralisations | int | = defense.Neutralisations |
| Turnovers | int | = passing.TotalPertes |
| PenaltiesConceded | int | = sanctions.PenaltyConcede (incorrect pour sanctions) |
| OpenShotAttempts | int | |
| ShotAttempts | int | |
| PenaltyAttempts | int | |
| ShotSuccessRate | double | simple double, pas de preuve num/dén |

### GlobalGoalkeeperRankingRow
| Colonne | Type | Notes |
|---|---|---|
| … | | Voir DashboardModels.cs V1 |
| SaveRate | double | simple double, pas de preuve |
| ShotSuccessRate | double | simple double, pas de preuve |

## Nouveau modèle (V2)

### DashboardPlayerTable
- `FieldPlayers : IReadOnlyList<DashboardFieldPlayerRow>`
- `Goalkeepers : IReadOnlyList<DashboardGoalkeeperRow>`

### DashboardFieldPlayerRow
- `Identity : TablePlayerIdentity` — PlayerId, FullName, TeamName, PositionId, PositionLabel, IsGoalkeeper, MatchesPlayed
- `Offense : TableFieldOffense` — TotalGoals, OpenPlayGoals, PenaltyGoals, Assists, PenaltiesWon (null=V1), SanctionsDrawn (null=V1), TotalTurnovers, BadPasses, FailedPivotPassesAvailable (toujours false), TotalShotRate, OpenPlayShotRate, PenaltyShotRate
- `Defense : TableFieldDefense` — Interceptions, Blocks, OffensiveFoulsDrawn, Neutralizations, PenaltiesConceded, SanctionsConceded (detail Warnings/TwoMinutes/Disqualifications, PenaltyConcede EXCLUE)

### DashboardGoalkeeperRow
- `Identity : TablePlayerIdentity`
- `Goalkeeper : TableGoalkeeperStats` — saves, shots faced, save rates (avec preuve), Assists, Goals, TotalTurnovers, MissedShots

### TableRateValue
Chaque taux expose `Value` (null si dénominateur = 0), `Numerator`, `Denominator`, `SampleReliable` (toujours false en V1), `UnavailableReason`.

## Source de données

| API | Endpoint | Utilisé |
|---|---|---|
| V1 ComparePlayersAsync | POST /api/v1/analytics/players/compare | OUI — 1 seule requête |
| V2 bulk analytics | GET /api/v2/analytics/players/{id} | NON — pas de route bulk |

**BULK_ANALYTICS_API_GAP = YES** — aucune route V2 bulk n'existe. On reste sur ComparePlayersAsync (V1).

## Gaps V1

| Métrique | Disponibilité | Traitement |
|---|---|---|
| PenaltiesWon (7m obtenus) | ❌ DATA_UNAVAILABLE | null → afficher "—" |
| SanctionsDrawn (sanctions obtenues) | ❌ DATA_UNAVAILABLE | null → afficher "—" |
| FailedPivotPasses | ❌ DATA_MISSING | FailedPivotPassesAvailable=false → afficher "—" |
| SampleReliable (Wilson bound) | ❌ pas de calcul V1 | false systématiquement |

## Contraintes validées

- `PLAYER_TABLE_N_PLUS_ONE_REQUESTS = 0` — une seule ComparePlayersAsync
- `PENALTIES_CONCEDED_INCLUDED_IN_SANCTIONS = NO` — PenaltyConcede exclue de SanctionDetail
- `ZERO_DENOMINATOR_RENDERED_AS_ZERO_PERCENT = NO` — TableRateValue.FromCounts protège
- `FAILED_PIVOT_PASSES_INFERRED_FROM_BAD_PASSES = NO` — FailedPivotPassesAvailable toujours false

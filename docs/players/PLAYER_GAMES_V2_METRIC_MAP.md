# Player Games V2 — Metric Map

| METRIC_CODE | LABEL | FIELD_PLAYER | GOALKEEPER | SOURCE | NUMERATOR | DENOMINATOR | AVAILABILITY_RULE | SEASON_BASELINE | DIRECTION | TRAJECTORY_SHARED |
|---|---|---|---|---|---|---|---|---|---|---|
| GOALS | Buts | OUI | NON | `PlayerMatchItemDto.Goals + PenaltyGoals` | — | — | Always available (int, 0 = réel zéro) | CountBaseline(Goals) | HigherIsBetter | OUI (GOALS_PER_MATCH) |
| ASSISTS | Passes déc. | OUI | OUI (GK) | `PlayerMatchItemDto.Assists` | — | — | Always available | CountBaseline(Assists) | HigherIsBetter | OUI (ASSISTS_PER_MATCH) |
| INTERCEPTIONS | INT | OUI | NON | `PlayerMatchItemDto.Interceptions` | — | — | Always available | CountBaseline(Interceptions) | HigherIsBetter | OUI (INTERCEPTIONS_PER_MATCH) |
| TURNOVERS | Pertes | OUI | OUI (GK) | `PlayerMatchItemDto.Turnovers` | — | — | Always available | CountBaseline(Turnovers) | LowerIsBetter | OUI (TURNOVERS_PER_MATCH) |
| SAVES | Arrêts | NON | OUI | `PlayerMatchItemDto.Saves` | — | — | Always available | CountBaseline(Saves) | HigherIsBetter | OUI (SAVES_PER_MATCH) |
| PLAYING_TIME | Min | OUI | OUI | `PlayerMatchItemDto.PlayingTimeMinutes` | — | — | 0 → DataMissing; >0 → RecordedDirect | avg(eligible) | Neutral | OUI (PLAYING_TIME) |
| SHOT_RATE | Taux tir | OUI | NON | **DATA_MISSING** — pas de ShotAttempts/match dans DTO | ShotGoals | ShotAttempts | null per match; null baseline | null | HigherIsBetter | Partiel (SHOT_SUCCESS_RATE DATA_MISSING) |
| SAVE_RATE | Tx arrêt | NON | OUI | **DATA_MISSING** — pas de ShotsFaced/match dans DTO | Saves | ShotsFaced | null per match; null baseline | null | HigherIsBetter | Partiel (SAVE_RATE DATA_MISSING) |
| PENALTY_SAVE_RATE | Tx 7m | NON | OUI | **DATA_MISSING** | PenaltySaves | PenaltyShotsFaced | null | null | HigherIsBetter | Partiel (PENALTY_SAVE_RATE DATA_MISSING) |
| RESULT | V/N/D | OUI | OUI | Calculé : PlayerTeamId vs Team1Id, scores | — | — | Unknown si score absent | — | — | NON |
| SANCTIONS | Sanctions | OUI | NON | `PlayerMatchItemDto.Sanctions` | — | — | Always available | — | LowerIsBetter | NON (non affiché en V2) |

## Règles de disponibilité

### Temps de jeu
- `PlayingTimeMinutes == 0` → `DataMissing` → affiche "—"
- `PlayingTimeMinutes > 0` → `RecordedDirect` → affiche valeur entière
- Pas de signal de substitution dans le DTO → pas de `DerivedFromSubstitutions` côté Games

### Taux
- `ShotRate`, `SaveRate`, `PenaltySaveRate` : null per match car le DTO ne contient pas les dénominateurs par match
- Dénominateur 0 → null (jamais "0 %")
- Baseline rate = `SUM(num)/SUM(den)` sur toute la saison (jamais moyenne des %)

### Scores
- `Team1Score == null || Team2Score == null` → résultat `Unknown`, score "—"
- Pas d'invention de 0–0

## Profils d'affichage

### Joueuse de champ
Colonnes tableau : Date | Match | Rés. | Min | Buts | PD | INT | Pertes

### Gardienne
Colonnes tableau : Date | Match | Rés. | Min | Arrêts | PD | Pertes

### Note sur les colonnes manquantes
Les colonnes Taux tir, Tx arrêt, Tx 7m ne sont pas affichées car les dénominateurs (ShotAttempts, ShotsFaced, PenaltyShotsFaced) ne sont pas disponibles par match dans `PlayerMatchItemDto`. Elles sont présentes dans les modèles (`GameFieldMetrics.ShotRate`, `GameGoalkeeperMetrics.SaveRate`) pour une implémentation future.

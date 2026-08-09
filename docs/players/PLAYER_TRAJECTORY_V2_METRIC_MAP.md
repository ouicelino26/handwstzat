# Player Trajectory V2 — Metric Map

| METRIC_CODE | LABEL | TYPE | DIRECTION | PER_MATCH_SOURCE | NUMERATOR | DENOMINATOR | WINDOW_AGGREGATION | ROLLING_AGGREGATION | SEASON_REFERENCE | POSITION_REFERENCE | AVAILABILITY_RULE | FIELD_PLAYER | GOALKEEPER |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| GOALS_PER_MATCH | Buts / match | PerMatch | HigherIsBetter | match.Goals + match.PenaltyGoals | — | — | SUM/eligibleMatches | SUM/3matches | AVAILABLE | BLOCKED_BY_API | Always | YES | NO |
| ASSISTS_PER_MATCH | Passes déc. / match | PerMatch | HigherIsBetter | match.Assists | — | — | SUM/eligibleMatches | SUM/3matches | AVAILABLE | BLOCKED_BY_API | Always | YES | NO |
| SHOT_SUCCESS_RATE | Taux de tir | Rate | HigherIsBetter | — | — | — | SUM(num)/SUM(den) | SUM(num)/SUM(den) | AVAILABLE_IF_DATA | BLOCKED_BY_API | Requires denominator | YES | NO |
| OPEN_PLAY_SHOT_SUCCESS_RATE | Taux dans le jeu | Rate | HigherIsBetter | — | — | — | SUM(num)/SUM(den) | SUM(num)/SUM(den) | AVAILABLE_IF_DATA | BLOCKED_BY_API | Requires denominator | YES | NO |
| INTERCEPTIONS_PER_MATCH | Interceptions / match | PerMatch | HigherIsBetter | match.Interceptions | — | — | SUM/eligibleMatches | SUM/3matches | AVAILABLE | BLOCKED_BY_API | Always | YES | NO |
| TURNOVERS_PER_MATCH | Pertes / match | PerMatch | LowerIsBetter | match.Turnovers | — | — | SUM/eligibleMatches | SUM/3matches | AVAILABLE | BLOCKED_BY_API | Always | YES | NO |
| PENALTIES_WON_PER_MATCH | 7 m obtenus / match | PerMatch | HigherIsBetter | — | — | — | SUM/eligibleMatches | SUM/3matches | BLOCKED | BLOCKED_BY_API | V2 only | YES | NO |
| PLAYING_TIME | Temps de jeu | Minutes | Neutral | match.PlayingTimeMinutes | — | — | SUM/eligibleMatches | SUM/3matches | AVAILABLE | BLOCKED_BY_API | Requires minutes > 0 | YES | YES |
| SAVES_PER_MATCH | Arrêts / match | PerMatch | HigherIsBetter | match.Saves | — | — | SUM/eligibleMatches | SUM/3matches | AVAILABLE | BLOCKED_BY_API | Always | NO | YES |
| SAVE_RATE | Taux d'arrêt | Rate | HigherIsBetter | — | — | — | SUM(num)/SUM(den) | SUM(num)/SUM(den) | AVAILABLE_IF_DATA | BLOCKED_BY_API | Requires denominator | NO | YES |
| OPEN_PLAY_SAVE_RATE | Taux d'arrêt dans le jeu | Rate | HigherIsBetter | — | — | — | SUM(num)/SUM(den) | SUM(num)/SUM(den) | AVAILABLE_IF_DATA | BLOCKED_BY_API | Requires denominator | NO | YES |
| PENALTY_SAVE_RATE | Taux d'arrêt 7 m | Rate | HigherIsBetter | — | — | — | SUM(num)/SUM(den) | SUM(num)/SUM(den) | AVAILABLE_IF_DATA | BLOCKED_BY_API | Requires denominator | NO | YES |
| SHOTS_FACED_PER_MATCH | Tirs subis / match | PerMatch | Neutral | — | — | — | SUM/eligibleMatches | SUM/3matches | BLOCKED | BLOCKED_BY_API | No DTO field | NO | YES |
| GOALS_CONCEDED_PER_MATCH | Buts encaissés / match | PerMatch | LowerIsBetter | — | — | — | SUM/eligibleMatches | SUM/3matches | BLOCKED | BLOCKED_BY_API | No DTO field | NO | YES |
| GK_ASSISTS_PER_MATCH | Passes déc. / match | PerMatch | HigherIsBetter | match.Assists | — | — | SUM/eligibleMatches | SUM/3matches | AVAILABLE | BLOCKED_BY_API | Always | NO | YES |
| GK_TURNOVERS_PER_MATCH | Pertes / match | PerMatch | LowerIsBetter | match.Turnovers | — | — | SUM/eligibleMatches | SUM/3matches | AVAILABLE | BLOCKED_BY_API | Always | NO | YES |

## Notes

- **POSITION_REFERENCE_STATUS=BLOCKED_BY_API** — Aucun endpoint n'expose les distributions de position sous forme de bulk. Les requêtes N+1 sont interdites. La référence de position reste bloquée jusqu'à qu'un endpoint `/api/positions/{id}/distribution` ou équivalent soit disponible.
- **DATA_MISSING per match pour les taux** — `PlayerMatchItemDto` ne contient pas de dénominateurs (tentatives de tir, tirs subis). Les métriques de type Rate sont marquées `DATA_MISSING` au niveau match et ne peuvent pas être tracées par match.
- **ROLLING_AGGREGATION pour les taux** — Si les données étaient disponibles, l'agrégation rolling utiliserait SUM(num_3_matchs)/SUM(den_3_matchs), jamais la moyenne des pourcentages.

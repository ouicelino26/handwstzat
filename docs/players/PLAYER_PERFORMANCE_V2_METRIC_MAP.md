# Player Performance V2 — Metric Map

## Joueuse de champ

| METRIC_CODE | UI_LABEL | SECTION | SOURCE | BRIEF_RELATION |
|-------------|----------|---------|--------|----------------|
| TOTAL_GOALS | Buts | Attaque / Production | V2: `LeagueAttackMetricsDto.TotalGoals` · V1: `Offense.TotalButs` | BRIEF_SUMMARY_PERFORMANCE_DETAIL |
| OPEN_PLAY_GOALS | Dans le jeu | Attaque / Production | V2: `LeagueAttackMetricsDto.OpenPlayGoals` · V1: `Offense.Buts` | NOT_IN_BRIEF |
| PENALTY_GOALS | 7 mètres | Attaque / Production | V2: `LeagueAttackMetricsDto.PenaltyGoals` · V1: `Offense.Buts7m` | NOT_IN_BRIEF |
| ASSISTS | Passes décisives | Attaque / Production | V2: `LeagueAttackMetricsDto.Assists` · V1: `Passing.PasseDecisive` | BRIEF_SUMMARY_PERFORMANCE_DETAIL |
| PENALTIES_WON | 7m obtenus | Attaque / Production | V2 only: `LeagueAttackMetricsDto.PenaltiesWon` | NOT_IN_BRIEF |
| SANCTIONS_DRAWN | Sanctions obtenues | Attaque / Production | V2 only: `LeagueAttackMetricsDto.SanctionsDrawn` | NOT_IN_BRIEF |
| TOTAL_SHOTS | Tirs total | Attaque / Volume de tir | V1: `Technical.ShotAttempts` · fallback: `Offense.TotalButs + TirsRates + PenaltyRate` | NOT_IN_BRIEF |
| OPEN_PLAY_SHOTS | Tirs dans le jeu | Attaque / Volume de tir | V1: `Offense.Buts + Offense.TirsRates` | NOT_IN_BRIEF |
| PENALTY_SHOTS | Tirs 7 mètres | Attaque / Volume de tir | V1: `Offense.Buts7m + Offense.PenaltyRate` | NOT_IN_BRIEF |
| BLOCKED_SHOTS | Tirs contrés | Attaque / Volume de tir | V1: `Offense.TirContre` | NOT_IN_BRIEF |
| TOTAL_SHOT_RATE | Taux total | Attaque / Efficacité | V2: `LeagueAttackMetricsDto.TotalShotRate` · V1: `Technical.OverallShotSuccessRate` | BRIEF_SUMMARY_PERFORMANCE_DETAIL |
| OPEN_PLAY_SHOT_RATE | Jeu ouvert | Attaque / Efficacité | V2: `LeagueAttackMetricsDto.OpenPlayShotRate` · V1: `Technical.OpenShotSuccessRate` | NOT_IN_BRIEF |
| PENALTY_SHOT_RATE | 7 mètres | Attaque / Efficacité | V2: `LeagueAttackMetricsDto.PenaltyShotRate` · V1: `Technical.PenaltySuccessRate` | NOT_IN_BRIEF |
| INTERCEPTIONS | Interceptions | Défense / Impact | V2: `LeagueDefenseMetricsDto.Interceptions` · V1: `Defense.Interceptions` | BRIEF_SUMMARY_PERFORMANCE_DETAIL |
| BLOCKS | Contres | Défense / Impact | V2: `LeagueDefenseMetricsDto.Blocks` · V1: `Defense.Contres` | NOT_IN_BRIEF |
| OFFENSIVE_FOULS_DRAWN | PF provoqués | Défense / Impact | V2: `LeagueDefenseMetricsDto.OffensiveFoulsDrawn` · V1: `Defense.PassageForce` | NOT_IN_BRIEF |
| NEUTRALIZATIONS | Neutralisations | Défense / Impact | V2: `LeagueDefenseMetricsDto.Neutralizations` · V1: `Defense.Neutralisations` | NOT_IN_BRIEF |
| PENALTIES_CONCEDED | 7m concédés | Défense / Coût + Discipline | V2: `LeagueDefenseMetricsDto.PenaltiesConceded` · V1: `Sanctions.PenaltyConcede` | NOT_IN_BRIEF |
| SANCTIONS_CONCEDED | Sanctions concédées | Défense / Coût | V2: `LeagueDefenseMetricsDto.SanctionsConceded` · V1: `Sanctions.Avert+DeuxMin+Exclusions` | NOT_IN_BRIEF |
| TOTAL_TURNOVERS | Pertes de balle | Maîtrise | V2: `LeagueAttackMetricsDto.TotalTurnovers` · V1: `Passing.TotalPertes` | NOT_IN_BRIEF |
| BAD_PASSES | Mauvaises passes | Maîtrise | V2: `LeagueAttackMetricsDto.BadPasses` · V1: `Passing.MauvaisePasse` | NOT_IN_BRIEF |
| FAILED_PIVOT_PASSES | Passes pivot ratées | Maîtrise | V2 only: `LeagueAttackMetricsDto.FailedPivotPasses` (LeagueCountMetricDto, peut être DATA_MISSING) | NOT_IN_BRIEF |
| WARNINGS | Avertissements | Discipline | V2: `LeagueDefenseMetricsDto.WarningsConceded` · V1: `Sanctions.Avertissements` | NOT_IN_BRIEF |
| TWO_MINUTES | 2 minutes | Discipline | V2: `LeagueDefenseMetricsDto.TwoMinuteSuspensionsConceded` · V1: `Sanctions.DeuxMinutes` | NOT_IN_BRIEF |
| DISQUALIFICATIONS | Exclusions | Discipline | V2: `LeagueDefenseMetricsDto.DisqualificationsConceded` · V1: `Sanctions.Exclusions` | NOT_IN_BRIEF |

## Gardienne

| METRIC_CODE | UI_LABEL | SECTION | SOURCE | BRIEF_RELATION |
|-------------|----------|---------|--------|----------------|
| GK_TOTAL_SAVES | Arrêts total | Arrêts | V2: `LeagueGoalkeeperMetricsDto.TotalSaves` · V1: `Goalkeeper.Arrets + ArretsPenalty` | BRIEF_SUMMARY_PERFORMANCE_DETAIL |
| GK_OPEN_SAVES | Arrêts dans le jeu | Arrêts | V2: `LeagueGoalkeeperMetricsDto.OpenPlaySaves` · V1: `Goalkeeper.Arrets` | NOT_IN_BRIEF |
| GK_PENALTY_SAVES | Arrêts 7 mètres | Arrêts | V2: `LeagueGoalkeeperMetricsDto.PenaltySaves` · V1: `Goalkeeper.ArretsPenalty` | NOT_IN_BRIEF |
| GK_TOTAL_FACED | Tirs subis total | Arrêts / Tirs subis | V2: `LeagueGoalkeeperMetricsDto.TotalShotsFaced` · V1: `Goalkeeper.TirsSubis` | NOT_IN_BRIEF |
| GK_OPEN_FACED | Dans le jeu | Arrêts / Tirs subis | V2: `LeagueGoalkeeperMetricsDto.OpenPlayShotsFaced` · V1: `Goalkeeper.Arrets + ButsPris` | NOT_IN_BRIEF |
| GK_PENALTY_FACED | 7 mètres | Arrêts / Tirs subis | V2: `LeagueGoalkeeperMetricsDto.PenaltyShotsFaced` · V1: `Goalkeeper.ArretsPenalty + ButsPenalty` | NOT_IN_BRIEF |
| GK_GOALS_CONCEDED | Buts encaissés | Arrêts / Tirs subis | V1: `Goalkeeper.ButsPris + ButsPenalty` | NOT_IN_BRIEF |
| GK_TOTAL_SAVE_RATE | Taux total | Arrêts / Taux d'arrêt | V2: `LeagueGoalkeeperMetricsDto.TotalSaveRate` · V1: `Technical.GoalkeeperSaveRate` | BRIEF_SUMMARY_PERFORMANCE_DETAIL |
| GK_OPEN_SAVE_RATE | Jeu ouvert | Arrêts / Taux d'arrêt | V2: `LeagueGoalkeeperMetricsDto.OpenPlaySaveRate` · V1: calculé `openSaves/openFaced` | NOT_IN_BRIEF |
| GK_PENALTY_SAVE_RATE | 7 mètres | Arrêts / Taux d'arrêt | V2: `LeagueGoalkeeperMetricsDto.PenaltySaveRate` · V1: `Technical.GoalkeeperPenaltyStopRate` | NOT_IN_BRIEF |
| GK_ASSISTS | Passes décisives | Avec ballon | V2: `LeagueGoalkeeperMetricsDto.Assists` · V1: `Goalkeeper.PasseDecisives` | NOT_IN_BRIEF |
| GK_GOALS | Buts | Avec ballon | V2: `LeagueGoalkeeperMetricsDto.Goals` · V1: `Goalkeeper.Buts` | NOT_IN_BRIEF |
| GK_TURNOVERS | Pertes de balle | Avec ballon | V2: `LeagueGoalkeeperMetricsDto.TotalTurnovers` · V1: `Goalkeeper.PerteDeBalle` | NOT_IN_BRIEF |
| GK_MISSED_SHOTS | Tirs manqués | Avec ballon | V2: `LeagueGoalkeeperMetricsDto.MissedShots` · V1: `Goalkeeper.TirsLoupes` | NOT_IN_BRIEF |

## Notes de mapping

- **V2 prioritaire** : quand `LeagueAnalytics?.Analytics?.Offense/Defense/Goalkeeper` est non null, ses valeurs priment sur les DTOs V1
- **Fallback V1** : quand V2 est null, on utilise les DTOs raw (`Offense`, `Defense`, `Passing`, `Sanctions`, `Goalkeeper`, `Technical`)
- **Evidence** : les taux V2 exposent `Numerator` et `Denominator` pour la preuve numérateur/dénominateur
- **DATA_MISSING** : affiché comme "—" via `Availability="DATA_MISSING"` sur `PerformanceMetricRow`
- **INSUFFICIENT_SAMPLE** : affiché comme avertissement via `Availability="INSUFFICIENT_SAMPLE"` quand `!SampleReliable`

# Player Table V2 — Metric Mapping

Date : 2026-08-07

| UI_LABEL | METRIC_CODE | SOURCE | NUMERATOR | DENOMINATOR | AVAILABILITY_RULE | SORT_RULE | LOW_SAMPLE_RULE |
|---|---|---|---|---|---|---|---|
| Buts | TotalGoals | offense.TotalButs ?? player.TotalGoals | — | — | Toujours disponible | desc → MJ asc → nom asc | — |
| Buts jeu | OpenPlayGoals | offense.Buts ?? (TotalGoals-PenaltyGoals) | — | — | Toujours disponible | desc → MJ asc → nom asc | — |
| Buts 7m | PenaltyGoals | offense.Buts7m ?? player.PenaltyGoalCount | — | — | Toujours disponible | desc → MJ asc → nom asc | — |
| Passes | Assists | passing.PasseDecisive ?? player.AssistCount | — | — | Toujours disponible | desc → MJ asc → nom asc | — |
| 7m obtenus | PenaltiesWon | — | — | — | ❌ DATA_UNAVAILABLE V1 | Tri désactivé | — |
| Pertes | TotalTurnovers | passing.TotalPertes ?? player.TurnoverCount | — | — | Toujours disponible | desc → nom asc | — |
| Mauvaises passes | BadPasses | passing.MauvaisePasse | — | — | Toujours disponible | desc → nom asc | — |
| PivotPF | FailedPivotPasses | — | — | — | ❌ DATA_MISSING V1 | Tri désactivé | — |
| Taux jeu | OpenPlayShotRate | offense.Buts | offense.Buts+offense.TirsRates | denominator > 0 → valeur | HasValue desc → Value desc → Denominator desc → nom | SampleReliable=false en V1 |
| Taux 7m | PenaltyShotRate | offense.Buts7m | offense.Buts7m+offense.PenaltyRate | denominator > 0 → valeur | HasValue desc → Value desc → Denominator desc → nom | SampleReliable=false en V1 |
| Taux global | TotalShotRate | TotalGoals | OpenDenominator+PenDenominator | denominator > 0 → valeur | HasValue desc → Value desc → Denominator desc → nom | SampleReliable=false en V1 |
| Interceptions | Interceptions | defense.Interceptions ?? player.InterceptionCount | — | — | Toujours disponible | desc → MJ asc → nom asc | — |
| Contres | Blocks | defense.Contres | — | — | 0 si defense null | desc → MJ asc → nom asc | — |
| Neutral. | Neutralizations | defense.Neutralisations | — | — | 0 si defense null | desc → MJ asc → nom asc | — |
| PF prov. | OffensiveFoulsDrawn | defense.PassageForce | — | — | 0 si defense null | desc → MJ asc → nom asc | — |
| Pen. c. | PenaltiesConceded | sanctions.PenaltyConcede | — | — | 0 si sanctions null | desc → nom asc | — |
| Sanctions | SanctionsConceded.Total | Avert+2min+Excl (PenaltyConcede EXCLU) | — | — | Toujours disponible | desc → nom asc | — |
| Sanct. obtenues | SanctionsDrawn | — | — | — | ❌ DATA_UNAVAILABLE V1 | Tri désactivé | — |
| **GARDIENNES** | | | | | | | |
| Arrets | TotalSaves | keeper.Arrets+keeper.ArretsPenalty | — | — | Toujours disponible | desc → MJ asc → nom asc | — |
| Arrets 7m | PenaltySaves | keeper.ArretsPenalty | — | — | 0 si keeper null | desc → MJ asc → nom asc | — |
| Taux arret | TotalSaveRate | TotalSaves | keeper.TirsSubis ?? player.ShotsFaced | denominator > 0 → valeur | HasValue desc → Value desc → Denominator desc → nom | SampleReliable=false en V1 |
| Arr. jeu | OpenPlaySaves | keeper.Arrets | — | — | 0 si keeper null | desc → MJ asc → nom asc | — |
| Tx arr. jeu | OpenPlaySaveRate | OpenPlaySaves | Arrets+ButsPris | denominator > 0 → valeur | HasValue desc → Value desc → Denominator desc → nom | SampleReliable=false en V1 |
| Tx arr. 7m | PenaltySaveRate | PenaltySaves | ArretsPenalty+ButsPenalty | denominator > 0 → valeur | HasValue desc → Value desc → Denominator desc → nom | SampleReliable=false en V1 |
| Tirs subis | TotalShotsFaced | keeper.TirsSubis ?? player.ShotsFaced | — | — | 0 si ni keeper ni player | desc → nom asc | — |
| Passes | Assists | keeper.PasseDecisives ?? player.AssistCount | — | — | Toujours disponible | desc → MJ asc → nom asc | — |
| Buts | Goals | keeper.Buts ?? player.TotalGoals | — | — | Toujours disponible | desc → MJ asc → nom asc | — |
| Pertes | TotalTurnovers | keeper.PerteDeBalle+keeper.MauvaisePasse | — | — | 0 si keeper null | desc → nom asc | — |
| Tirs rates | MissedShots | keeper.TirsLoupes | — | — | 0 si keeper null | desc → nom asc | — |

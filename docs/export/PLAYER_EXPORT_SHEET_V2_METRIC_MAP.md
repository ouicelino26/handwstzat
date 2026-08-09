# Player Export Sheet V2 — Metric Map

## Field player — Offensive table

| METRIC_CODE | LABEL | CATEGORY | TABLE | RADAR | RAW_UNIT | RADAR_DIRECTION | RADAR_NORMALIZATION | AVAILABILITY |
|---|---|---|---|---|---|---|---|---|
| MATCHES_PLAYED | Matchs | GLOBAL | YES | NO | count | — | — | always |
| TOTAL_GOALS | Buts | OFFENSE | YES | candidate | count | HIGHER_IS_BETTER | min/max or percentile | always |
| BUTS_PAR_MATCH | Buts / match | OFFENSE | YES | NO | rate | HIGHER_IS_BETTER | — | matches > 0 |
| ASSISTS | Passes decisives | OFFENSE | YES | candidate | count | HIGHER_IS_BETTER | min/max or percentile | always |
| OPEN_PLAY_SHOT_RATE | Taux tir jeu | OFFENSE | YES (+ evidence) | candidate | percent | HIGHER_IS_BETTER | min/max or percentile | openShots > 0 |
| PENALTY_SHOT_RATE | Taux tir 7m | OFFENSE | YES (+ evidence) | NO | percent | HIGHER_IS_BETTER | min/max or percentile | penShots > 0 |
| SHOT_ATTEMPTS | Tirs total | OFFENSE | YES | NO | count | HIGHER_IS_BETTER | — | always |
| TOTAL_TURNOVERS | Pertes de balle | OFFENSE | YES | candidate | count | LOWER_IS_BETTER | inverted min/max | always |

## Field player — Defensive table

| METRIC_CODE | LABEL | CATEGORY | TABLE | RADAR | RAW_UNIT | RADAR_DIRECTION | RADAR_NORMALIZATION | AVAILABILITY |
|---|---|---|---|---|---|---|---|---|
| INTERCEPTIONS | Interceptions | DEFENSE | YES | candidate | count | HIGHER_IS_BETTER | min/max or percentile | always |
| BLOCKS | Contres | DEFENSE | YES | candidate | count | HIGHER_IS_BETTER | min/max or percentile | always |
| NEUTRALIZATIONS | Neutralisations | DEFENSE | YES | candidate | count | HIGHER_IS_BETTER | min/max or percentile | always |
| PENALTIES_CONCEDED | 7m concedes | DEFENSE | YES | candidate | count | LOWER_IS_BETTER | inverted | always |
| TOTAL_SANCTIONS | Sanctions | DISCIPLINE | YES (+ breakdown) | candidate | count | LOWER_IS_BETTER | inverted | always |

## Radar normalization

- When `MinValue` and `MaxValue` are valid (MaxValue > MinValue): range normalization `(value - min) / (max - min) * 100`
- For `HigherIsBetter=false`: score = `100 - normalized`
- When MinValue == MaxValue or non-finite: fallback to API `Percentile` (already favorable-direction)
- All scores clamped to [0, 100]

## Radar axis selection — Offensive (max 6)

1. Axes whose `Category` ∈ `{offense, passing, technical}`
2. OR whose `Key`/`Label` contains: `goal`, `assist`, `shot`, `tir`, `pass`, `7m`, `penalty_won`, `sanction_drawn`, `turnover`
3. `HigherIsBetter=true` axes preferred first
4. Max 6 axes taken

## Radar axis selection — Defensive (max 6)

1. Axes whose `Category` contains `def`, `discipline`, or `sanction`
2. OR whose `Key`/`Label` contains: `interception`, `block`, `contre`, `neutral`, `def`, `sanction_conceded`, `penalty_conceded`, `7m_concede`, `turnover`, `perte`
3. Max 6 axes taken

## Goalkeeper variants

- Offensive rows: Matchs / Arrets / Taux d'arret / Stop 7m / Passes décisives / Buts
- Defensive rows: Tirs subis / Buts encaissés / Pertes de balle / Sanctions
- Offensive radar: goalkeeper/offense/passing categories + keywords: save/stop/arret/penalty
- Defensive radar: conceded/goal_against/penalty_conceded keywords

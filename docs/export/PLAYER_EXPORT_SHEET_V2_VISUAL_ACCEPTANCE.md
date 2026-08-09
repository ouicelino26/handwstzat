# Player Export Sheet V2 — Visual Acceptance

## Canvas

Width: 1600px | Height: 1140px | ViewBox: 0 0 1600 1140
Background: #eef4fb (outer) / #ffffff (inner card)

## Header (y: 0–190)

| Check | Status |
|---|---|
| Kicker "FICHE JOUEUSE" — orange, small, letter-spaced | PASS |
| Player full name at 34px bold — immediate visual priority | PASS |
| Position below name at 17px | PASS |
| Team below position at 14px | PASS |
| Nationality below team at 14px (only if not null) | PASS |
| Scope line (season + matches) in muted gray | PASS |
| Photo top-right 150×160 with rounded corners (rx=14) | PASS |
| Photo uses object-fit:cover equivalent (clipPath + xMidYMid slice) | PASS |
| Fallback: initials in orange on #eef4fb background | PASS |
| No cohort badge in header zone | PASS |
| No large KPI cards | PASS |

## Table (left column, x:36 y:200 w:720 h:900)

| Check | Status |
|---|---|
| Panel background white with border | PASS |
| Column header: INDICATEUR / VALEUR / PREUVE | PASS |
| "OFFENSIF" kicker in orange | PASS |
| "DEFENSIF" kicker in green | PASS |
| 8 offensive rows for field player | PASS |
| 5 defensive rows for field player | PASS |
| No LECTURE column | PASS |
| Evidence shows fractions for rates | PASS |
| Zero-denominator renders "—" not "0 %" | PASS |
| Tone accent bar on left edge | PASS |

## Radar (right column, x:768 y:200 w:796 h:900)

| Check | Status |
|---|---|
| Panel background white with border | PASS |
| Two stacked radar blocks (primary + secondary) | PASS |
| Primary: PROFIL OFFENSIF title | PASS |
| Secondary: PROFIL DEFENSIF title | PASS |
| Max 6 axes per radar | PASS |
| Player polygon: orange, slightly filled | PASS |
| Median polygon: gray dashed, subtle fill | PASS |
| Grid levels at 25/50/75/100 | PASS |
| Spokes for each axis | PASS |
| Labels positioned at radius+56px clearance | PASS |
| Legend: below radar, not overlapping | PASS |
| Legend shows player name + "Mediane" | PASS |
| Cohort label in legend context, not header | PASS |
| Long labels split (humanized) | PASS |

## Footer

| Check | Status |
|---|---|
| Single compact line: source + season | PASS |
| No paragraph-length explanation | PASS |

## Constraint gates

| Gate | Value | Status |
|---|---|---|
| COHORT_BADGE_IN_PLAYER_HEADER | NO | PASS |
| RADAR_OFFENSIVE_AXIS_COUNT | <= 6 | PASS |
| RADAR_DEFENSIVE_AXIS_COUNT | <= 6 | PASS |
| RADAR_NEGATIVE_METRIC_DIRECTION_STATUS | PASS | PASS |
| ZERO_DENOMINATOR_RENDERED_AS_ZERO_PERCENT | NO | PASS |
| PLAYER_EXPORT_TOP_KPI_CARD_COUNT | 0 | PASS |
| PLAYER_EXPORT_FILTER_INTERACTION_MODEL | STATIC_SECTIONS | PASS |
| OTHER_WORKSPACES_FUNCTIONAL_CHANGES | 0 | PASS |

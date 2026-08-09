# PLAYER_COURT_V2_DATA_CONTRACT

Date: 2026-08-07 | Branch: fix/player-court-v2

## Data flow

API -> ZoneStatDto / TriggerZoneStatDto -> CourtZoneMapper -> CourtZoneStat -> GoalKpi

## CourtZoneStat fields

- Key: string — zone code. Shot zones use BG*/BD* keys. Trigger zones use visual (post-inversion) TG*/TD* keys.
- Label: string — human-readable French label from ZoneNameCatalog
- Rate: double — success rate as percentage (0-100)
- Attempts: int — total shot attempts in zone
- Successes: int — goals in zone
- Failures: int (computed) — Attempts - Successes
- SampleReliable: bool — true when Attempts >= 5
- IsAvailable: bool — true when Attempts > 0
- Outcomes: IReadOnlyList<OutcomeCount> — grouped by EventName

## Trigger key inversion

Backend keys TG*/TD* are inverted to TD*/TG* for visual display (SpatialZoneVisuals.ToVisualTriggerKey). The catalog TriggerZoneLabels uses visual keys.

## AttackType filter (client-side, approximate)

All: no filter applied
OpenPlay: keeps "But", "Tir a cote", "Tir sur poteau", "Tir arrete", "Tir rate", "Tir contre"
SevenMeter: keeps "But sur penalty", "Penalty sur poteau", "Penalty rate", "Penalty arrete"

## SampleReliable threshold

Threshold: Attempts >= 5. Below this, zone is flagged with SampleReliable=false and a warning is shown in the detail panel.

## Lazy loading

Spatial data is not fetched at player selection. It is fetched the first time the "zones" tab is activated (EnsureCourtLoadedAsync). IsCourtLoaded gate prevents double-fetch.

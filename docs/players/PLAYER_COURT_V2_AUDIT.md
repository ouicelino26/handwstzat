# PLAYER_COURT_V2_AUDIT

Date: 2026-08-07 | Branch: fix/player-court-v2

## Scope

Court tab refactoring — Players.razor "zones" section, GoalKpi.razor component, new CourtZoneModels + CourtZoneMapper.

## Changes applied

- New enums: PlayerCourtAttackType, PlayerCourtDisplayMode, PlayerCourtScene, PlayerCourtShotResult
- New record: CourtZoneStat (replaces ZoneStat for spatial display)
- ZoneNameCatalog: 24 shot zone labels + 18 trigger zone labels (human-readable French)
- CourtZoneMapper: MapShotZone, MapTriggerZone, FilterByAttackType, ApplyResultFilter
- GoalKpi: CourtZoneStat parameters, DisplayMode parameter, accessibility attributes (role, tabindex, aria-label, aria-pressed, onkeydown)
- Players.razor: filter bar (attack type, display mode, scene), lazy court load, zone detail panel, tabular fallback
- CSS: court-filter-bar, court-zone-detail, court-alt-table, responsive rules

## Approximation warning

FilterByAttackType operates on EventName-based OutcomeCount aggregates, not individual event records. Denominator may differ from a server-side filter. This is documented in CourtZoneMapper.cs with APPROXIMATE markers.

## Tests

- CourtZoneMapperTests.cs: 22 tests covering mapping, filtering, catalog, key separation
- CourtZoneNameCatalogTests.cs: 5 tests covering label counts and code exclusion

## Pre-existing changes

Players.razor had pre-existing modifications on the Brief tab (section "overview") from branch fix/player-time-availability-v1. These were not modified.

## Build status

BUILD_ERRORS=0 (verified dotnet build)
FAILED_TESTS=0 (verified dotnet test)

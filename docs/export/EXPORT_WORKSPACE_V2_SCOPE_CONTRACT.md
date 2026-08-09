# Export Workspace V2 — Scope Contract

Generated: 2026-08-09

## Scope Field Mapping

| UI_FIELD | GLOBAL_SCOPE_SOURCE | API_FIELD | EFFECTIVE | NOTES |
|---|---|---|---|---|
| Compétition | `AnalysisScopeSnapshot.CompetitionId` + `CompetitionName` | `AnalyticsExportRequestDto.CompetitionId` | YES | Filters export to competition |
| Équipe | `AnalysisScopeSnapshot.TeamId` + `TeamName` | `AnalyticsExportRequestDto.TeamId` | YES (for TEAM target) | Applied when target = Team |
| Saison | `AnalysisScopeSnapshot.Season` | `AnalyticsExportRequestDto.SeasonLabel` + `SeasonYear` | YES | `SeasonYear` derived from `SeasonLabel` (start year of "YYYY-YYYY") |
| Journée | `AnalysisScopeSnapshot.Day` | — (no Day field in DTO) | DISPLAY ONLY | Used client-side to filter match/player picker lists; NOT sent to API. UI clearly signals this. |
| Date de début | Export-specific | `AnalyticsExportRequestDto.DateFrom` | YES | Advanced option only |
| Date de fin | Export-specific | `AnalyticsExportRequestDto.DateTo` | YES | Advanced option only |
| Cible (target) | Export-specific | `AnalyticsExportRequestDto.Scope` | YES | Mapped via `ExportTargetMapper` |
| Joueuses sélectionnées | Export-specific | `AnalyticsExportRequestDto.PlayerIds` | YES | Only sent for Players/Goalkeepers targets |
| Matchs sélectionnés | Export-specific | `AnalyticsExportRequestDto.MatchIds` | YES | Only sent for Matches target |

## Day Scope Truth

`EXPORT_DAY_SCOPE_TRUTH_STATUS=PASS`

The export API contract (`AnalyticsExportRequestDto`) has no `Day` field. Therefore:

1. `ExportScopeState.Day` is stored for display and for client-side match/player filtering.
2. `ExportRequestBuilder.Build` does NOT add a Day parameter to the DTO.
3. The summary panel shows a note when Day is set: "filtré non transmis au serveur".
4. The day note in the scope display panel explains this limitation.
5. Test `Export_DoesNotClaimDayIsEffectiveWhenApiCannotFilterDay` verifies that `AnalyticsExportRequestDto` has no `Day` property.

## SeasonYear Derivation

Contract: API uses the start year of the season (first 4-digit segment).

| SeasonLabel | SeasonYear |
|---|---|
| `2025-2026` | `2025` |
| `2024-2025` | `2024` |
| `2025/2026` | `2025` |
| null / blank | null |
| unparseable | null |

## Global Scope Integration

`GLOBAL_SCOPE_INTEGRATION_STATUS=PASS`
`GLOBAL_SCOPE_FIELDS_IGNORED_BY_EXPORT=0`

All fields of `AnalysisScopeSnapshot` are used:
- `CompetitionId` / `CompetitionName` → prefilled, sent to API
- `TeamId` / `TeamName` → prefilled, available for display
- `Season` → prefilled, sent as `SeasonLabel`
- `Day` → prefilled, used for client-side filtering, disclosed as display-only

## Stale Response Protection

`EXPORT_STALE_RESPONSE_PROTECTION=PASS`

`_generationToken` (int) is incremented on each `GenerateAndDownloadAsync` call.
The download callback checks `token == _generationToken` before writing result state.
A cancelled or superseded generation cannot overwrite the result of a newer one.

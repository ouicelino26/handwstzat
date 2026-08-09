# Export Workspace V2 — Audit

Generated: 2026-08-09

## CURRENT_SCOPE_SOURCE

V1: Page-level variables (`Scope`, `SeasonLabel`, `SeasonYear`, `_selectedCompetitionId`, `_selectedTeamId`). Reads `AnalysisScopeService.Current` once at init (`PrefillFromScope`), then diverges. Day was NOT copied from the global scope.

V2: `ExportScopeState.FromSnapshot(ScopeService.Current)` — single source of truth. Reacts to global scope changes via `AnalysisScopeService.Changed` event. Day is now read.

## CURRENT_SCOPE_DUPLICATION

V1 maintained three parallel representations:
1. Page-level export state (`_selectedCompetitionId`, `_selectedTeamId`, `SeasonLabel`, `SeasonYear`)
2. `AnalysisScopeService.Current` (not updated from Export)
3. `DashboardFilterState` (used by Dashboard, unrelated to Export)

V2: One `ExportScopeState` per Export page. Global scope not mutated.

## CURRENT_FILTER_IMPLEMENTATION

V1: Inline `<select>` elements for Competition and Team (not smart-filtered). Season: free text input. Day: absent from Export entirely.

V2: Smart filters using `SmartFilterCatalog.GetTeams` (constrains teams by match pool). Season: `<select>` from `MatchFilterCatalog.GetSeasons` when available, text fallback otherwise. Day: `<select>` from `MatchFilterCatalog.GetDays` (filtered by season).

## CURRENT_INITIAL_REQUESTS

V1: On init → `RefData.GetReferenceDataAsync()` + `PlayersClient.GetPlayersAsync(pageSize:500)` always.

V2: On init → `RefData.GetReferenceDataAsync()` only. Players lazy-loaded when target = Players/Goalkeepers. Matches loaded if scope has CompetitionId or Season context (for filter list population).

## CURRENT_PLAYER_LOADING

V1: Unconditional 500-player load at startup. No team/competition filter.

V2: Lazy. `PlayersClient.GetPlayersAsync(competitionId, teamId, season, pageSize:300)` only when user selects Players/Goalkeepers target.

`EXPORT_PLAYER_DIRECTORY_GLOBAL_500_LOAD=NO`
`EXPORT_PLAYERS_LAZY_LOAD_STATUS=PASS`

## CURRENT_MATCH_LOADING

V1: Only when Scope=MATCH. No team filter.

V2: Lazy. `MatchesClient.GetMatchesAsync(competitionId, teamId, season, pageSize:300)`. Loaded initially when scope has context (to populate season/day filter lists). Reloaded on scope changes.

`EXPORT_MATCHES_LAZY_LOAD_STATUS=PASS`

## CURRENT_EXPORT_SCOPES

API enum: `ExportScope { SEASON, TEAM, PLAYER, MULTIPLE_PLAYERS, GOALKEEPERS, MATCH, CUSTOM }`

V1 exposed 7 pills directly using API enum names.

V2: `ExportTargetType { FullScope, Team, Players, Goalkeepers, Matches }` — 5 human-facing values. `PLAYER` vs `MULTIPLE_PLAYERS` mapped internally by `ExportTargetMapper.ToApiScope` based on player count. `CUSTOM` not exposed.

## CURRENT_EXPORT_SECTIONS

API keys: `SEASON_SUMMARY, TEAMS, PLAYERS, PLAYERS_PER_MATCH, GOALKEEPERS, MATCHES, SHOTS, DEFENSE, EVENTS, DATA_QUALITY, METRIC_DICTIONARY`

V1: Showed raw API keys as section labels (e.g. `SEASON_SUMMARY`).

V2: `ExportSectionCatalog` maps each key to a human label and description. Keys never displayed to user. `EVENTS` and `DATA_QUALITY` are advanced options.

`VISIBLE_API_EXPORT_KEYS=0`

## CURRENT_TECHNICAL_LABELS

V1: Header said "Workspace 06 / Export" and "Extraire la donnée brute du scope."

V2: Header "Exporter les données" / subtitle "Préparez un classeur à partir du périmètre actuellement analysé."

## CURRENT_VALIDATION

V1: None client-side. API errors shown as "Erreur inattendue".

V2: `ExportRequestValidator` validates before HTTP. Errors shown near summary panel. Generate button disabled when invalid.

## CURRENT_META_ENDPOINT_USAGE

`GenerateExportMetaAsync` existed in V1 but was never called from Export.razor. `_serverWarnings` was populated but had no real source.

V2: Download flow unchanged (direct download). `_serverWarnings` list only populated from real API responses (currently empty until meta endpoint is wired). Warnings source is documented as `EXPORT_WARNING_STATUS=API_NOT_AVAILABLE` for server warnings from meta endpoint; local warnings are explicitly separated.

## CURRENT_WARNINGS_SOURCE

V1: `Warnings` list had no real source — never populated from API.

V2: `_serverWarnings` (from API meta) separated from local warnings (e.g. "Export volumineux probable"). Each displayed with different styling.

## CURRENT_DOWNLOAD_FLOW

Both V1 and V2: POST to `api/v2/exports/analytics/download` → byte array → `FileSystem.AppDataDirectory` → `Share.RequestAsync`.

`EXPORT_META_DOWNLOAD_CONTRACT=DIRECT_DOWNLOAD_ONLY` (meta endpoint exists server-side but is not called; download returns ephemeral file)

## CURRENT_CANCELLATION

V1: `_cts.Cancel()` → sets `IsBusy=false`. No distinct cancelled state.

V2: `_cts.Cancel()` → sets `_status=Ready`, sets `_errorMessage="Export annulé."`. Cancel button only shown during active generation. Stale-response protection via `_generationToken`.

`EXPORT_CANCELLATION_STATUS=PASS`
`EXPORT_STALE_RESPONSE_PROTECTION=PASS`

## CURRENT_RESPONSIVE_STATE

V1: Single-column layout. No responsive breakpoints defined for Export specifically.

V2: Two-column (config left, summary right sticky) at ≥769px. Single column at ≤768px. Mobile pill scroll at ≤400px.

## API_SUPPORTED_SCOPES

`SEASON, TEAM, PLAYER, MULTIPLE_PLAYERS, GOALKEEPERS, MATCH, CUSTOM`

## API_SUPPORTED_SECTIONS

`SEASON_SUMMARY, TEAMS, PLAYERS, PLAYERS_PER_MATCH, GOALKEEPERS, MATCHES, SHOTS, DEFENSE, EVENTS, DATA_QUALITY, METRIC_DICTIONARY`

## API_SUPPORTED_FORMATS

`XLSX, CSV_ZIP` (server). UI exposes only XLSX. No format selector needed.

`EXPORT_FORMAT_SELECTOR_STATUS=XLSX_ONLY_NO_SELECTOR`

## API_VALIDATION_BEHAVIOR

Server validates scope/section combinations. Client now also validates before HTTP.

## API_WARNING_BEHAVIOR

Meta endpoint returns `ExportMetaDto.Warnings`. Direct download endpoint does not return warnings. Local warnings are client-side only.

## EXPORT_TIMEOUT

3 minutes (`CancellationTokenSource(TimeSpan.FromMinutes(3))`). Matches V1.
Risk: large exports with `IncludeRawEvents=true` may exceed 3 minutes.
`EXPORT_TIMEOUT=3_MINUTES`

## OTHER_WORKSPACES_FUNCTIONAL_CHANGES

0. Only `Export.razor`, `Export.razor.css`, `ExportModels.cs`, `ExportWorkspaceV2Tests.cs`, `HandWStat.Tests.csproj` modified.

`OTHER_WORKSPACES_FUNCTIONAL_CHANGES=0`

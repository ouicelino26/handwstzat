# Export Workspace V2 — Section Catalog

Generated: 2026-08-09

## Section Definitions

| API_KEY | UI_LABEL | DESCRIPTION | PRESETS | ADVANCED | DEPENDENCIES |
|---|---|---|---|---|---|
| SEASON_SUMMARY | Synthèse du périmètre | Totaux agrégés sur le périmètre analysé | FullAnalysis, Staff | No | None |
| TEAMS | Équipes | Statistiques par équipe | FullAnalysis, Staff | No | None |
| PLAYERS | Joueuses | Statistiques globales par joueuse | FullAnalysis, Staff, Players | No | None |
| PLAYERS_PER_MATCH | Performances par match | Statistiques de chaque joueuse match par match | FullAnalysis, Players, Matches | No | None |
| GOALKEEPERS | Gardiennes | Métriques spécifiques aux gardiennes | FullAnalysis, Players | No | None |
| MATCHES | Matchs | Liste et résultats des matchs | FullAnalysis, Staff, Matches | No | None |
| SHOTS | Tirs | Logs de tirs avec zones et résultats | FullAnalysis, Matches, Spatial | No | Required by IncludeShotCoordinates |
| DEFENSE | Défense | Statistiques défensives | FullAnalysis | No | None |
| METRIC_DICTIONARY | Dictionnaire des métriques | Définition de toutes les métriques du classeur | FullAnalysis | No | None |
| DATA_QUALITY | Qualité des données | Complétude et fiabilité des données par match | FullAnalysis, Staff, Players | Yes (advanced) | None |
| EVENTS | Événements bruts | Log complet de chaque événement | None (opt-in only) | Yes (advanced, warn large) | None |

## Preset Composition

| PRESET | SECTIONS |
|---|---|
| FullAnalysis | SEASON_SUMMARY, TEAMS, PLAYERS, PLAYERS_PER_MATCH, GOALKEEPERS, MATCHES, SHOTS, DEFENSE, METRIC_DICTIONARY, DATA_QUALITY |
| Staff | SEASON_SUMMARY, TEAMS, PLAYERS, MATCHES, DATA_QUALITY |
| Players | PLAYERS, PLAYERS_PER_MATCH, GOALKEEPERS, DATA_QUALITY |
| Matches | MATCHES, PLAYERS_PER_MATCH, SHOTS |
| Spatial | SHOTS |
| Custom | User-defined |

## Deduplication

`ExportRequestBuilder.Build` uses `HashSet<string>` (OrdinalIgnoreCase) to accumulate sections.
`DATA_QUALITY` may be present in the selected set AND enabled via `IncludeDataQuality` flag — only one copy appears in the API request.
`EVENTS` is only added via `IncludeRawEvents` flag.
`SHOTS` is automatically added when `IncludeShotCoordinates=true`.

`DUPLICATE_REQUEST_SECTIONS=0`

## Section Dependency Notes

- `IncludeShotCoordinates=true` → SHOTS is added to sections automatically. No incoherent combination possible.
- `IncludeRawEvents=true` → EVENTS is added. No double-add possible.
- `IncludeDataQuality=true` → DATA_QUALITY is added. No double-add possible.

`SECTION_DEPENDENCY_STATUS=PASS`

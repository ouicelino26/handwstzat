# HandWStat — Endpoint Matrix

All endpoints listed below are relative to the API base URL.

| Feature | Endpoint | Method | OperationId | Auth | Availability | FallbackV1 | HandWStat Screen | Notes |
|---|---|---|---|---|---|---|---|---|
| League player analytics | GET /api/v2/analytics/players/{playerId} | GET | GetLeaguePlayerAnalytics | Bearer | AVAILABLE | No | Player detail screen | `include` param selects sections: overview, offense, defense, goalkeeper |
| Stats overview v1 | GET /api/v1/stats/overview | GET | GetStatsOverview | Bearer | AVAILABLE | — | Dashboard | Existing v1 |
| Rankings | GET /api/v1/stats/rankings | GET | GetRankings | Bearer | AVAILABLE | — | Rankings screen | v1 |
| Player global stats | GET /api/v1/stats/players/{id}/global | GET | GetPlayerGlobalStats | Bearer | AVAILABLE | — | Player screen | v1 |
| Compare players | POST /api/v1/stats/compare | POST | ComparePlayers | Bearer | AVAILABLE | — | Compare screen | v1 |
| Team stats | GET /api/v1/stats/teams/{id} | GET | GetTeamStats | Bearer | AVAILABLE | — | Team screen | v1 |
| Match summary | GET /api/v1/stats/matches/{id} | GET | GetMatchSummary | Bearer | AVAILABLE | — | Match screen | v1 |
| Position profiles | GET /api/v1/players/{id}/position-profile | GET | GetPositionProfile | Bearer | AVAILABLE | — | Player detail | v1 |
| Data quality | — | — | — | — | NOT_IMPLEMENTED | — | — | No dedicated endpoint yet |
| Possessions | — | — | — | — | BLOCKED_BY_SOURCE_DATA | — | — | No possession tracking in DB |
| Lineups | — | — | — | — | BLOCKED_BY_SOURCE_DATA | — | — | No substitution data in source |
| On/off | — | — | — | — | BLOCKED_BY_SOURCE_DATA | — | — | Depends on lineups |
| xG | — | — | — | — | FEATURE_FLAG_DISABLED | — | — | No validated statistical model |
| xS | — | — | — | — | FEATURE_FLAG_DISABLED | — | — | No validated statistical model |
| Tactical context | GET /api/v1/stats/players/{id}/events | GET | GetPlayerEvents | Bearer | AVAILABLE_PARTIAL | — | Player detail | Context breakdown available via event endpoint |
| Scouting | — | — | — | — | NOT_IMPLEMENTED | — | — | Contracts defined, no endpoint |
| Video | — | — | — | — | FEATURE_FLAG_DISABLED | — | — | No storage provider configured |
| Reports | — | — | — | — | NOT_IMPLEMENTED | — | — | Contracts defined, no endpoint |
| Client updates | GET /api/v2/updates/check | GET | CheckClientUpdate | Anonymous | AVAILABLE | — | App update flow | v2 |
| FailedPivotPasses | (inline in offense section) | — | — | — | DATA_MISSING | — | — | No canonical pivot pass event ID in source; returned as LeagueCountMetric with `value = null` |

---

## Notes

- **AVAILABLE**: endpoint is stable and integrated or ready to integrate.
- **AVAILABLE_PARTIAL**: endpoint exists but does not provide a dedicated v2 contract; use v1 event breakdown.
- **NOT_IMPLEMENTED**: contracts may exist; no server endpoint available yet — do not implement client-side workarounds.
- **BLOCKED_BY_SOURCE_DATA**: server cannot provide data because the underlying event model lacks required fields — do not implement client-side workarounds.
- **FEATURE_FLAG_DISABLED**: server-side feature is explicitly disabled — do not render UI elements or make API calls.
- **DATA_MISSING**: metric is part of an existing response but cannot be calculated from current data — display as "Données non disponibles", do not substitute another metric.

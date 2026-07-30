# HandWStat — Feature Availability

This table is the authoritative reference for the current availability of every HandWStat feature. Consult this before implementing or hiding any UI element.

| Feature | Status | Reason | HandWStat Action |
|---|---|---|---|
| League player analytics v2 | AVAILABLE | Core endpoint stable — `GET /api/v2/analytics/players/{id}` | Implement via `GetLeaguePlayerAnalytics` |
| Stats overview v1 | AVAILABLE | Stable since v1 | Already integrated |
| Rankings v1 | AVAILABLE | Stable since v1 | Already integrated |
| Player global stats v1 | AVAILABLE | Stable since v1 | Already integrated |
| Compare players v1 | AVAILABLE | Stable since v1 | Already integrated |
| Team stats v1 | AVAILABLE | Stable since v1 | Already integrated |
| Match stats v1 | AVAILABLE | Stable since v1 | Already integrated |
| Tactical context (event breakdown) | AVAILABLE_PARTIAL | Context fields present; no dedicated v2 endpoint yet | Use v1 event endpoint (`GetPlayerEvents`) |
| Position profiles v1 | AVAILABLE | Stable | Already integrated |
| Client updates v2 | AVAILABLE | Stable — `GET /api/v2/updates/check` | Already integrated |
| FailedPivotPasses | DATA_MISSING | No canonical pivot pass event identifier in source data; `MatchEvent` lacks an explicit `FAILED_PIVOT_PASS` subtype and a typed pivot target | Display as "Données non disponibles"; do NOT substitute `badPasses` |
| Possessions | BLOCKED_BY_SOURCE_DATA | No possession tracking in the match events table | Do not render the possession screen |
| Lineups / On-Off | BLOCKED_BY_SOURCE_DATA | No substitution data in source; lineup entry/exit cannot be reconstructed | Do not render lineup or on-off analysis |
| Expected Goals (xG) | FEATURE_FLAG_DISABLED | No validated statistical model; feature flag is explicitly disabled | Do not render xG values; hide the UI element entirely |
| Expected Saves (xS) | FEATURE_FLAG_DISABLED | No validated statistical model; feature flag is explicitly disabled | Do not render xS values; hide the UI element entirely |
| Scouting | NOT_IMPLEMENTED | Contracts defined on the API side; no endpoint yet | Do not render scouting screen; hide the UI element |
| Video | FEATURE_FLAG_DISABLED | No storage provider configured | Do not render the video tab |
| Reports | NOT_IMPLEMENTED | Contracts defined on the API side; no endpoint yet | Do not render reports section; hide the UI element |
| Data Quality API | NOT_IMPLEMENTED | No dedicated endpoint yet; quality data is embedded in `MetricValue.quality` per metric | Do not render API-driven quality indicators; use inline `qualityScore` from `MetricValue` instead |

---

## Status Definitions

| Status | Meaning | Client Rule |
|---|---|---|
| AVAILABLE | Endpoint is stable and ready to call | Implement or use as-is |
| AVAILABLE_PARTIAL | Endpoint exists but does not provide a full v2 contract | Use v1 endpoint; do not expect v2 schema |
| DATA_MISSING | Metric is part of an existing response but cannot be calculated | Display "Données non disponibles"; do not substitute |
| BLOCKED_BY_SOURCE_DATA | API cannot provide data due to missing source model | Do not render; do not make API calls |
| FEATURE_FLAG_DISABLED | Server-side feature is explicitly turned off | Hide UI element; do not make API calls |
| NOT_IMPLEMENTED | No server endpoint yet; contracts may exist | Hide UI element; do not make API calls |

---

## Summary: What Can Be Implemented Now

All **MUST** items in `HANDWSTAT_REMAINING_UI_WORK.md` can be implemented against the current API state. No server-side changes are required.

Features with `BLOCKED_BY_SOURCE_DATA`, `FEATURE_FLAG_DISABLED`, or `NOT_IMPLEMENTED` status require server-side changes before any client implementation is possible. Do not build client-side workarounds.

# HandWStat — Analytics Call Strategy

This document defines the optimal API call patterns for each HandWStat screen. Follow these patterns to minimize latency and avoid N+1 or redundant requests.

---

## Screen-by-Screen Call Map

### Dashboard Initial Load

| Call | Endpoint | Count |
|---|---|---|
| Stats overview | GET /api/v1/stats/overview | 1 |
| Top rankings (initial) | GET /api/v1/stats/rankings | 1 |
| **Total** | | **2 calls** |

Rankings details are loaded on demand when the user navigates to the rankings screen.

---

### Rankings Screen

| Scenario | Endpoint | Count |
|---|---|---|
| Initial load | GET /api/v1/stats/rankings | 1 |
| Sort change | GET /api/v1/stats/rankings?sort=... | 1 |
| No change (ETag match) | GET /api/v1/stats/rankings (304) | 0 data fetched |

Use the `ETag` / `If-None-Match` header. If the server returns `304 Not Modified`, use the cached response — no new data to process.

---

### Player Detail Screen

| Call | Endpoint | Count |
|---|---|---|
| Full analytics profile | GET /api/v2/analytics/players/{id}?include=overview,offense,defense,goalkeeper | 1 |
| **Total** | | **1 call** |

One call retrieves all four sections. Condition the goalkeeper section display on `overview.isGoalkeeper`. If the player is not a goalkeeper, omit `goalkeeper` from the `include` param to reduce payload.

---

### Compare Screen (2–6 players)

| Call | Endpoint | Count |
|---|---|---|
| Compare all players | POST /api/v1/stats/compare (body: all playerIds) | 1 |
| **Total** | | **1 call** |

NEVER fetch player stats in a loop. Always use the compare endpoint with all player IDs in a single request body.

---

### Team Screen

| Call | Endpoint | Count |
|---|---|---|
| Team stats | GET /api/v1/stats/teams/{id} | 1 |
| **Total** | | **1 call** |

---

### Match Screen

| Call | Endpoint | Count |
|---|---|---|
| Match summary | GET /api/v1/stats/matches/{id} | 1 |
| **Total** | | **1 call** |

---

### Position Profile Screen

| Call | Endpoint | Count |
|---|---|---|
| Position profile | GET /api/v1/players/{id}/position-profile | 1 |
| **Total** | | **1 call** |

---

### Features Not Yet Available

| Feature | Status | API Call |
|---|---|---|
| Data quality | NOT_IMPLEMENTED | None — no endpoint |
| Scouting | NOT_IMPLEMENTED | None — no endpoint |
| Possessions | BLOCKED_BY_SOURCE_DATA | None — do not make requests |
| Lineups / On-Off | BLOCKED_BY_SOURCE_DATA | None — do not make requests |
| xG / xS | FEATURE_FLAG_DISABLED | None — do not make requests |
| Video | FEATURE_FLAG_DISABLED | None — do not make requests |
| Reports | NOT_IMPLEMENTED | None — no endpoint |

---

## include Param Optimization

Use the `include` parameter to request only the sections your screen needs.

| Use Case | include Value | Sections Returned |
|---|---|---|
| Player identity only | `overview` | Player name, team, position, match count |
| Attack and defense only | `offense,defense` | Attack and defense metrics |
| Goalkeeper review | `goalkeeper` | Goalkeeper save rates and counts |
| Full profile | `overview,offense,defense,goalkeeper` | All 4 sections in 1 call |

Avoid requesting `goalkeeper` for field players — check `overview.isGoalkeeper` first, or request `overview` first and then request `goalkeeper` conditionally in a second call only when needed.

---

## Anti-Patterns to Avoid

| Anti-Pattern | Correct Approach |
|---|---|
| N+1: fetching player analytics per player in a loop | Use POST /api/v1/stats/compare with all playerIds in one call |
| Ignoring ETag | Cache conditional GET responses; handle 304 |
| Polling for updates | No streaming or server push is available; poll only if user explicitly triggers a refresh |
| Multiple calls when `include` can batch sections | Use a single call with all needed sections in `include` |
| Continuing a request after user navigates away | Pass `CancellationToken` to all async gateway calls and cancel on navigation |
| Making API calls for FEATURE_FLAG_DISABLED features | Do not call; hide the UI element |
| Making API calls for NOT_IMPLEMENTED features | Do not call; hide the UI element |
| Making API calls for BLOCKED_BY_SOURCE_DATA features | Do not call; do not render the screen |

---

## CancellationToken Requirement

All gateway calls MUST accept and forward a `CancellationToken`. Cancel the token when the user navigates away from a screen. This prevents wasted server load and stale data being rendered after navigation.

```csharp
await _leagueAnalyticsGateway.GetPlayerAnalyticsAsync(
    playerId,
    sections: new[] { "overview", "offense", "defense", "goalkeeper" },
    cancellationToken: cancellationToken);
```

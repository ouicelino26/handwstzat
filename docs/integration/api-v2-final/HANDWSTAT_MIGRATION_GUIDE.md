# HandWStat — Migration Guide

This guide describes how to integrate the v2 analytics endpoint alongside the existing v1 integration. V1 endpoints remain unchanged and fully supported — this is an additive migration.

---

## Phase 1 — Integrate the League Analytics Endpoint

Implement `GET /api/v2/analytics/players/{playerId}` for the league analytics screen.

This single call replaces multiple v1 calls for the player detail view. Pass `include=overview,offense,defense,goalkeeper` to retrieve all sections in one request.

```http
GET /api/v2/analytics/players/42?include=overview,offense,defense,goalkeeper
Authorization: Bearer <token>
```

---

## Phase 2 — Add include Param Handling

Implement section selection via the `include` query parameter.

- `include=overview` — player identity and match count only
- `include=offense,defense` — attack and defense sections only
- `include=goalkeeper` — goalkeeper metrics only (condition on `overview.isGoalkeeper`)
- `include=overview,offense,defense,goalkeeper` — full profile in one call

When `include` is omitted, all sections are returned. An empty or invalid section value returns `400`.

---

## Phase 3 — Handle Nullable MetricValue.Value

`MetricValue.value` is nullable. A `null` value means the metric is not calculable (denominator is zero or invalid).

**Rule: NEVER replace `null` with `0`.**

Display `null` as "N/A" or "—" with the accessible label "Non disponible". Zero shots attempted is different from zero percent efficiency.

The same rule applies to `LeagueCountMetric.value` (used for `failedPivotPasses`).

---

## Phase 4 — Handle DATA_MISSING and FEATURE_FLAG_DISABLED States

Display data availability states honestly:

| State | Display |
|---|---|
| `DATA_MISSING` (`failedPivotPasses`) | "Données non disponibles" |
| `FEATURE_FLAG_DISABLED` (xG, xS, Video) | Hide UI element entirely |
| `NOT_IMPLEMENTED` (Scouting, Reports) | Hide UI element entirely |
| `BLOCKED_BY_SOURCE_DATA` (Possessions, Lineups) | Do not render screen/section |

---

## DTOs to Generate from OpenAPI Client

Generate the following types from `docs/openapi/handballmanager-api-v1-v2.json`:

| DTO | Notes |
|---|---|
| `LeaguePlayerAnalyticsResponse` | Root response object |
| `LeaguePlayerOverview` | Player identity and match count |
| `LeagueAttackMetrics` | Offense section |
| `LeagueDefenseMetrics` | Defense section |
| `LeagueGoalkeeperMetrics` | Goalkeeper section |
| `MetricValue` | Rate metric with nested Sample and Quality |
| `MetricSample` | Numerator / denominator / minimumSample |
| `MetricQuality` | sampleReliable / qualityScore / reason |
| `LeagueCountMetric` | Used for `failedPivotPasses` — has `availability` enum |
| `ProblemDetails` | Error response with `correlationId` |

See `HANDWSTAT_OPENAPI_CLIENT_GUIDE.md` for generation commands.

---

## Gateways to Create

Create `ILeagueAnalyticsGateway` with the following interface:

```csharp
public interface ILeagueAnalyticsGateway
{
    Task<LeaguePlayerAnalyticsResponse?> GetPlayerAnalyticsAsync(
        int playerId,
        string[] sections,
        AnalyticsQueryScope? scope = null,
        CancellationToken cancellationToken = default);
}
```

The implementation calls:

```
GET /api/v2/analytics/players/{playerId}?include={sections.Join(",")}
```

with the appropriate scope parameters appended as query string values.

---

## V1 Calls Preserved

The following v1 endpoints remain valid and unchanged. Do not remove them.

| Endpoint | Usage |
|---|---|
| `GET /api/v1/stats/overview` | Dashboard overview |
| `GET /api/v1/stats/rankings` | Rankings screen |
| `GET /api/v1/stats/players/{id}/global` | Player global stats |
| `POST /api/v1/stats/compare` | Compare multiple players |
| `GET /api/v1/stats/players/{id}/events` | Tactical event breakdown |
| `GET /api/v1/stats/teams/{id}` | Team stats |
| `GET /api/v1/stats/matches/{id}` | Match summary |
| `GET /api/v1/players/{id}/position-profile` | Position profile |

---

## Feature Flags Client-Side

Consult `HANDWSTAT_FEATURE_AVAILABILITY.md` for the current state of each feature.

| Rule |
|---|
| Do NOT render xG charts — `FEATURE_FLAG_DISABLED` |
| Do NOT render xS charts — `FEATURE_FLAG_DISABLED` |
| Do NOT render lineup / on-off sections — `BLOCKED_BY_SOURCE_DATA` |
| Do NOT render possession screen — `BLOCKED_BY_SOURCE_DATA` |
| Do NOT render scouting screen — `NOT_IMPLEMENTED` |
| Do NOT render reports section — `NOT_IMPLEMENTED` |
| Render `failedPivotPasses` as "Données non disponibles" — `DATA_MISSING` |

---

## Deprecation Warnings

None. V1 is fully supported. No deprecation is currently scheduled.

---

## Progressive Integration Strategy

Implement v2 alongside v1 with feature detection:

1. Add `ILeagueAnalyticsGateway` and call v2 for the player detail screen.
2. Keep all existing v1 gateway calls unchanged.
3. If the v2 endpoint returns `503`, fall back to v1 only if the same data is available in v1 (see fallback rules in `HANDWSTAT_ERROR_AND_FALLBACK_RULES.md`).
4. Never remove v1 calls until HandWStat has confirmed a full v2 migration and the API has formally deprecated v1.

# HandWStat — API v2 Master Contract

## Overview

This document is the authoritative integration contract for HandWStat clients consuming the HandballManager API. It covers the v2 analytics endpoint, shared schemas, authentication, error conventions, and feature-flag states.

---

## Base URL

All v2 analytics routes are relative to:

```
/api/v2/analytics
```

V1 routes remain at `/api/v1/`. No v1 endpoint is modified or removed.

---

## Authentication

All analytics endpoints require a **Bearer JWT** in the `Authorization` header.

```
Authorization: Bearer <token>
```

Authorized roles: `Admin`, `Consultation`.

- `401 Unauthorized` — token missing or expired.
- `403 Forbidden` — token valid but role not permitted.

---

## Error Conventions

All error responses conform to **RFC 7807 ProblemDetails**.

### ProblemDetails schema

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Human-readable title",
  "status": 400,
  "detail": "Machine-actionable detail string.",
  "instance": "/api/v2/analytics/players/0",
  "correlationId": "0HN8XXXXXXXXXXXX:00000001"
}
```

| Field | Type | Nullable | Notes |
|---|---|---|---|
| `type` | string | no | URI reference identifying the error type |
| `title` | string | no | Short human-readable summary |
| `status` | integer | no | HTTP status code |
| `detail` | string | yes | Actionable explanation |
| `instance` | string | yes | URI of the request that triggered the error |
| `correlationId` | string | yes | Always present; log for support tickets |

**Rule**: every error response includes `correlationId`. Clients MUST preserve this value in error reports and support tickets.

---

## ETag Support

Analytics endpoints support `ETag` / `If-None-Match` conditional GET. A `304 Not Modified` response means the data has not changed since the last request. Clients MUST implement ETag caching and avoid re-fetching unchanged data.

---

## Pagination

Pagination is supported on endpoints where noted (rankings, events). The analytics player endpoint returns a single document and is not paginated.

---

## Feature Flags

| Flag | Current State |
|---|---|
| AnalyticsV2 | ENABLED |
| DataQuality | NOT_IMPLEMENTED |
| Possessions | BLOCKED_BY_SOURCE_DATA |
| Lineups | BLOCKED_BY_SOURCE_DATA |
| ExpectedGoals | FEATURE_FLAG_DISABLED |
| ExpectedSaves | FEATURE_FLAG_DISABLED |
| Scouting | NOT_IMPLEMENTED |
| Video | FEATURE_FLAG_DISABLED |
| Reports | NOT_IMPLEMENTED |

---

## Metric Schemas

### MetricValue

Wraps a nullable percentage rate with its sample and quality metadata.

| Property | Type | Nullable | Notes |
|---|---|---|---|
| `metricCode` | string | no | Stable code (e.g. `TOTAL_SHOT_RATE`) |
| `metricVersion` | string | no | Always `"1.0"` for current metrics |
| `value` | number (double) | yes | Rounded to 2 decimal places (MidpointRounding.AwayFromZero). `null` if denominator is zero or invalid. |
| `unit` | string | no | Always `"percent"` for rate metrics |
| `sample` | MetricSample | no | Never null |
| `quality` | MetricQuality | no | Never null |

The `MetricValue` object also exposes flattened read-only copies of nested fields: `numerator`, `denominator`, `minimumSample`, `sampleReliable`, `qualityScore`.

### MetricSample

| Property | Type | Nullable | Notes |
|---|---|---|---|
| `numerator` | number (double) | yes | Actual numerator value (0 is valid) |
| `denominator` | number (double) | yes | Actual denominator value (0 is valid) |
| `minimumSample` | number (double) | no | Reliability threshold |

### MetricQuality

| Property | Type | Nullable | Notes |
|---|---|---|---|
| `sampleReliable` | boolean | no | `true` if value is non-null and `denominator >= minimumSample` |
| `qualityScore` | number (double) | no | `clamp(denominator / minimumSample, 0, 1)`, rounded to 2 decimal places; `0` if sample is invalid |
| `reason` | string | yes | `ZERO_OR_INVALID_DENOMINATOR`, `INVALID_NUMERATOR`, `BELOW_MINIMUM_SAMPLE`, or `null` |

**Key rule**: a rate below the minimum sample threshold still has a `value`, but `sampleReliable = false`. A rate with `denominator = 0` has `value = null`, `qualityScore = 0`, and `reason = "ZERO_OR_INVALID_DENOMINATOR"`.

### MetricVersion

Current metric version: `"1.0"` for all metrics in this contract.

### LeagueCountMetric

Used exclusively for `offense.failedPivotPasses`.

| Property | Type | Nullable | Notes |
|---|---|---|---|
| `metricCode` | string | no | `"FAILED_PIVOT_PASSES"` |
| `metricVersion` | string | no | `"1.0"` |
| `value` | integer | yes | Currently always `null` |
| `availability` | string (enum) | no | See LeagueMetricAvailability below |
| `reason` | string | yes | Human-readable reason for unavailability |

### LeagueMetricAvailability (enum)

Valid values for `LeagueCountMetric.availability`:

- `AVAILABLE`
- `PARTIALLY_AVAILABLE`
- `UNAVAILABLE_FROM_CURRENT_DATA`
- `AMBIGUOUS`
- `REQUIRES_ADDITIVE_SCHEMA_CHANGE`
- `PARTIAL`
- `DATA_MISSING`

Current value for `failedPivotPasses`: `DATA_MISSING`.

---

## Main Endpoint

### GET /api/v2/analytics/players/{playerId}

Retrieve analytics for a single player across one or more sections.

**OperationId**: `GetLeaguePlayerAnalytics`

**Authentication**: Bearer JWT required. Roles: `Admin`, `Consultation`.

#### Path Parameters

| Name | Type | Required | Rule |
|---|---|---|---|
| `playerId` | integer (int32) | yes | Must be > 0 |

#### Query Parameters

| Name | Type | Required | Notes |
|---|---|---|---|
| `include` | string | no | Comma-separated list of sections. Case-insensitive; surrounding spaces are ignored. Omitting or providing a blank value returns all sections. An empty list after normalization, or an unknown section value, returns `400`. |
| `competitionId` | integer | no | Filter by competition |
| `teamId` | integer | no | Filter by team |
| `matchId` | integer | no | Filter by match |
| `from` | string (date-time) | no | Start date inclusive. `from.Date > to.Date` returns `400`. |
| `to` | string (date-time) | no | End date inclusive |
| `year` | integer | no | Filter by year |
| `season` | string | no | Filter by season |
| `day` | string | no | Filter by matchday |
| `attackId` | integer | no | Filter by attack context |
| `defenseId` | integer | no | Filter by defense context |
| `trigger` | string | no | Filter by event trigger |
| `shootShade` | string | no | Filter by shot type |

#### Supported Sections (include param)

- `overview`
- `offense`
- `defense`
- `goalkeeper`

Sections returned in `included` are sorted ordinally. Sections not requested are present in the response object with JSON value `null`.

#### Example Request

```http
GET /api/v2/analytics/players/42?include=overview,offense,defense,goalkeeper
Authorization: Bearer <token>
Accept: application/json
```

#### HTTP Status Codes

| Status | Condition |
|---|---|
| `200 OK` | Success — returns `LeaguePlayerAnalyticsResponse` |
| `400 Bad Request` | Invalid `playerId` (<= 0), invalid date range, empty or unsupported `include` value |
| `401 Unauthorized` | Missing or expired token |
| `403 Forbidden` | Valid token but insufficient role |
| `404 Not Found` | No player exists with the given identifier |
| `409 Conflict` | Concurrent modification |
| `500 Internal Server Error` | Unhandled server error |

**Note**: `404` means the player does not exist. A valid scope with no matching events returns the player with counters at `0` and uncalculable rates at `null`.

---

## Response Structure

### LeaguePlayerAnalyticsResponse

| Property | Type | Nullable | Notes |
|---|---|---|---|
| `playerId` | integer | no | The requested player identifier |
| `metricVersion` | string | no | Always `"1.0"` |
| `included` | array of string | no | Sections actually requested, sorted ordinally |
| `overview` | LeaguePlayerOverview | yes | `null` if `overview` not requested |
| `offense` | LeagueAttackMetrics | yes | `null` if `offense` not requested |
| `defense` | LeagueDefenseMetrics | yes | `null` if `defense` not requested |
| `goalkeeper` | LeagueGoalkeeperMetrics | yes | `null` if `goalkeeper` not requested |

### LeaguePlayerOverview

| Property | Type | Nullable | Notes |
|---|---|---|---|
| `playerId` | integer | no | Player identifier |
| `fullName` | string | no | Full name |
| `teamId` | integer | yes | Team identifier |
| `teamName` | string | yes | Team name |
| `positionId` | integer | yes | Position identifier |
| `positionCode` | string | yes | Position code (e.g. `AR`, `GB`) |
| `positionName` | string | yes | Position label |
| `isGoalkeeper` | boolean | no | Goalkeeper indicator |
| `matchesPlayed` | integer | no | Distinct matches in the requested scope |

---

## Attack Metrics (offense section)

| JSON Property | Type | Nullable | Availability | Notes |
|---|---|---|---|---|
| `totalGoals` | integer | no | AVAILABLE | `openPlayGoals + penaltyGoals` |
| `openPlayGoals` | integer | no | AVAILABLE | Goals excluding penalties |
| `penaltyGoals` | integer | no | AVAILABLE | Goals from 7-meter throws |
| `assists` | integer | no | AVAILABLE | |
| `penaltiesWon` | integer | no | AVAILABLE | 7-meter throws drawn |
| `sanctionsDrawn` | integer | no | AVAILABLE | Sanctions provoked |
| `totalTurnovers` | integer | no | AVAILABLE | `badPasses + ballLosses + technicalFaults + offensiveFoulsCommitted` |
| `badPasses` | integer | no | AVAILABLE | Bad passes only — NOT a substitute for failedPivotPasses |
| `failedPivotPasses` | LeagueCountMetric | no (object) | DATA_MISSING | `value` is always `null`; `availability = "DATA_MISSING"` |
| `totalShotRate` | MetricValue | no (object) | AVAILABLE | `value` nullable; `100 × totalGoals / totalAttempts`; min sample 4 |
| `openPlayShotRate` | MetricValue | no (object) | AVAILABLE | `value` nullable; `100 × openPlayGoals / openPlayAttempts`; min sample 4 |
| `penaltyShotRate` | MetricValue | no (object) | AVAILABLE | `value` nullable; `100 × penaltyGoals / penaltyAttempts`; min sample 2 |

---

## Defense Metrics (defense section)

| JSON Property | Type | Nullable | Availability | Notes |
|---|---|---|---|---|
| `interceptions` | integer | no | AVAILABLE | |
| `blocks` | integer | no | AVAILABLE | Successful blocks |
| `offensiveFoulsDrawn` | integer | no | AVAILABLE | Forced passages drawn |
| `neutralizations` | integer | no | AVAILABLE | |
| `penaltiesConceded` | integer | no | AVAILABLE | 7-meter throws conceded |
| `sanctionsConceded` | integer | no | AVAILABLE | `warnings + twoMinuteSuspensions + disqualifications` |
| `warningsConceded` | integer | no | AVAILABLE | |
| `twoMinuteSuspensionsConceded` | integer | no | AVAILABLE | |
| `disqualificationsConceded` | integer | no | AVAILABLE | |

---

## Goalkeeper Metrics (goalkeeper section)

| JSON Property | Type | Nullable | Availability | Notes |
|---|---|---|---|---|
| `totalSaves` | integer | no | AVAILABLE | `openPlaySaves + penaltySaves` |
| `openPlaySaves` | integer | no | AVAILABLE | Saves excluding penalties |
| `penaltySaves` | integer | no | AVAILABLE | Saves on 7-meter throws |
| `totalShotsFaced` | integer | no | AVAILABLE | `totalSaves + openPlayGoalsConceded + penaltyGoalsConceded` |
| `openPlayShotsFaced` | integer | no | AVAILABLE | `openPlaySaves + openPlayGoalsConceded` |
| `penaltyShotsFaced` | integer | no | AVAILABLE | `penaltySaves + penaltyGoalsConceded` |
| `totalSaveRate` | MetricValue | no (object) | AVAILABLE | `value` nullable; `100 × totalSaves / totalShotsFaced`; min sample 10 |
| `openPlaySaveRate` | MetricValue | no (object) | AVAILABLE | `value` nullable; `100 × openPlaySaves / openPlayShotsFaced`; min sample 10 |
| `penaltySaveRate` | MetricValue | no (object) | AVAILABLE | `value` nullable; `100 × penaltySaves / penaltyShotsFaced`; min sample 2 |
| `assists` | integer | no | AVAILABLE | |
| `goals` | integer | no | AVAILABLE | Personal goals (open play + penalty) |
| `totalTurnovers` | integer | no | AVAILABLE | |
| `missedShots` | integer | no | AVAILABLE | Personal missed shots |

**Shot faced definition**: only on-target shots producing a save or a goal conceded. Off-target shots, posts, blocked shots, and failed shots without a goalkeeper save are excluded from denominators.

---

## Compatibility

- All v1 endpoints (`/api/v1/stats/*`, `/api/v1/players/*`) remain unchanged.
- The v2 analytics endpoint is additive — it does not replace v1 data.
- No v1 deprecation is currently scheduled.

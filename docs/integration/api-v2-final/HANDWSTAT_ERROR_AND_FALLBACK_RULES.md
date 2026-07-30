# HandWStat — Error Handling and Fallback Rules

This document defines the exact client-side reaction for every HTTP error code, and the complete set of fallback rules for the HandWStat integration.

---

## HTTP Error Response Rules

All error responses from the API include a `ProblemDetails` body with a `correlationId`. Always log the `correlationId` — it is required for support tickets.

### 400 Bad Request

- **Cause**: invalid `playerId`, invalid date range, empty or unsupported `include` value.
- **Client action**: show a validation error message to the user.
- **Fallback**: NONE. Do not fall back to v1. The request is contractually incorrect; fixing the request is the only valid action.
- **Log**: log `correlationId`.

### 401 Unauthorized

- **Cause**: token is missing or has expired.
- **Client action**: redirect to the login screen.
- **Fallback**: NONE. This is an authentication failure, not a data availability issue.

### 403 Forbidden

- **Cause**: token is valid but the user's role is not permitted (`Admin` or `Consultation` required).
- **Client action**: show "Accès refusé" state.
- **Fallback**: NONE. Do not fall back to v1. The role restriction applies equally to all API versions.

### 404 Not Found

- **Cause**: no player exists with the given identifier, or the scope returns nothing.
- **Client action**: show "Joueuse introuvable" or "Données introuvables" state.
- **Fallback**: NONE — the player or resource does not exist.

### 409 Conflict

- **Cause**: concurrent modification detected.
- **Client action**: show a generic error. Log `correlationId`. Optionally prompt the user to reload.
- **Fallback**: NONE.

### 422 Unprocessable Entity

- **Cause**: field-level validation failed.
- **Client action**: show field-level validation messages.
- **Fallback**: NONE. Do not fall back to v1. The payload is semantically invalid.

### 429 Too Many Requests

- **Cause**: rate limit exceeded.
- **Client action**: show a loading/retry state. Back off and retry **only after** the `Retry-After` header value has elapsed.
- **Fallback**: NONE while backing off.
- **Rule**: always respect `Retry-After`. Never retry immediately.

### 500 Internal Server Error

- **Cause**: unhandled server error.
- **Client action**: show a generic error state. Log `correlationId`. Optionally retry once after a short delay.
- **Fallback**: optional — see fallback rules below.

### 503 Service Unavailable

- **Cause**: server is in maintenance or temporarily offline.
- **Client action**: show a maintenance/offline state. Retry only after the `Retry-After` header value has elapsed.
- **Fallback**: conditional — see fallback rules below.

---

## Fallback Rules

### When a v1 fallback IS permitted

Fall back to a v1 endpoint ONLY when ALL of the following conditions are true:

1. The original request was to a **v2 endpoint**.
2. The server returned **503 Service Unavailable**.
3. A v1 endpoint **covers the same data** (per the tables in `HANDWSTAT_LEAGUE_ANALYTICS_CONTRACT.md`).

In all other cases, do not fall back.

### Absolute fallback prohibitions

| Prohibition | Reason |
|---|---|
| NEVER fall back after 400 | The request itself is wrong; a fallback sends the same wrong request |
| NEVER fall back after 401 or 403 | Authentication / authorization failure is not a data issue |
| NEVER replace `null` MetricValue.value with `0` | `null` means "not calculable"; zero means "calculated result is zero" — these are different |
| NEVER replace `null` LeagueCountMetric.value with `0` | Same reason as above |
| NEVER substitute `failedPivotPasses` with `badPasses` | They are different metrics; `badPasses` is not a superset of `failedPivotPasses` |
| NEVER substitute `failedPivotPasses` with any other field | No canonical equivalent exists in current data |
| NEVER display xG values | `FEATURE_FLAG_DISABLED` — no values will ever be returned from this API |
| NEVER display xS values | `FEATURE_FLAG_DISABLED` — no values will ever be returned from this API |
| NEVER fabricate lineup data | `BLOCKED_BY_SOURCE_DATA` — the event model lacks substitution records |
| NEVER fabricate possession data | `BLOCKED_BY_SOURCE_DATA` — the event model lacks possession tracking |
| NEVER mix v1 and v2 values in the same calculated result | Provenance must be homogeneous within a single displayed value |

---

## correlationId Handling

Every ProblemDetails error response contains a `correlationId` field. This value:

- MUST be logged in the client error log on every 4xx and 5xx response.
- MUST be included in any support ticket or bug report.
- MUST NOT be displayed to end users (it is for technical support only).

```json
{
  "type": "https://httpstatuses.com/500",
  "title": "An unexpected error occurred",
  "status": 500,
  "detail": "An internal error prevented the request from completing.",
  "instance": "/api/v2/analytics/players/42",
  "correlationId": "0HN8XXXXXXXXXXXX:00000001"
}
```

---

## Retry-After Header

Always respect the `Retry-After` header on both `429` and `503` responses. Do not retry before the specified duration has elapsed. Do not implement exponential backoff that bypasses a server-specified `Retry-After` value.

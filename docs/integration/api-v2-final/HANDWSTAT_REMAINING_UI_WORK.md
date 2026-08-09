# HandWStat — Remaining UI Work

This document is the prioritized task list for the HandWStat mobile/web client. Items are grouped by priority tier.

**ALL MUST items can be implemented NOW based on the current API state.**
BLOCKED items require server-side implementation first — do NOT implement client-side workarounds.

---

## MUST (blocking for league analytics release)

- [ ] Implement `LeagueAnalyticsGateway` calling `GET /api/v2/analytics/players/{id}?include=overview,offense,defense,goalkeeper`
- [ ] Generate typed DTOs from OpenAPI client: `LeaguePlayerAnalyticsResponse`, `LeaguePlayerOverview`, `LeagueAttackMetrics`, `LeagueDefenseMetrics`, `LeagueGoalkeeperMetrics`, `MetricValue`, `MetricSample`, `MetricQuality`, `LeagueCountMetric`, `ProblemDetails`
- [ ] Implement player analytics screen with overview, offense, and defense sections
- [ ] Handle nullable `MetricValue.value` — display "N/A" or "—", never replace `null` with `0`
- [ ] Handle `DATA_MISSING` state for `failedPivotPasses` — display "Données non disponibles"
- [ ] Handle `404` response — display "Joueuse introuvable" state
- [ ] Handle `401` response — redirect to login
- [ ] Handle `429` response — display retry state; respect `Retry-After` header before retrying

---

## SHOULD (improves completeness)

- [ ] Add goalkeeper section to player analytics screen (conditioned on `overview.isGoalkeeper = true`)
- [ ] Display `MetricQuality.qualityScore` as a confidence indicator alongside rate metrics
- [ ] Display `MetricQuality.reason` (e.g. "BELOW_MINIMUM_SAMPLE") as a tooltip or accessible annotation
- [ ] Implement section selection via `include` param — request only sections needed for the current view
- [ ] Add ETag caching support for player analytics calls (store and send `If-None-Match` header)
- [ ] Add `correlationId` to client error reports and support flow (log on every 4xx/5xx)

---

## BLOCKED (waiting on API implementation)

- [ ] Possessions screen — **BLOCKED_BY_SOURCE_DATA**: no possession tracking in the match events table
- [ ] Lineup / On-Off analysis — **BLOCKED_BY_SOURCE_DATA**: no substitution data in source
- [ ] xG / xS visualization — **FEATURE_FLAG_DISABLED**: no validated statistical model; do not implement client-side approximations
- [ ] Video integration — **FEATURE_FLAG_DISABLED**: no storage provider configured
- [ ] Scouting screen — **NOT_IMPLEMENTED**: contracts defined on API side, no endpoint available
- [ ] Reports section — **NOT_IMPLEMENTED**: contracts defined on API side, no endpoint available
- [ ] Data quality indicators from API — **NOT_IMPLEMENTED**: use inline `MetricValue.quality` instead

---

## OPTIONAL (nice to have, no blocking dependency)

- [ ] Compare players using v2 analytics (v1 compare endpoint currently works and is sufficient)
- [ ] Offline mode / local cache for analytics data (requires cache invalidation strategy)
- [ ] Export analytics to PDF

---

## Notes

- BLOCKED items must NOT be unblocked with client-side workarounds or fabricated data.
- FEATURE_FLAG_DISABLED items must be hidden from the UI entirely — do not render placeholder values.
- NOT_IMPLEMENTED items must be hidden from the UI — do not call the API for these features.
- The single most important MUST item is the `LeagueAnalyticsGateway` — everything else in the MUST tier depends on it.

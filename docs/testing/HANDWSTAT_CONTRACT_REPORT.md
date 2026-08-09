# HandWStat — Contract Validation Report

**Date :** 2026-07-30  
**API HEAD :** `c9de417` (HandballManagerAPI master)

---

## Contrats validés

| Contrat | Règle | Validation | Test |
|---------|-------|-----------|------|
| `failedPivotPasses` DATA_MISSING | `value=null`, `availability=DATA_MISSING`, jamais substitué | `LeagueAnalyticsContractValidator` rejette AVAILABLE sans valeur et DATA_MISSING avec valeur | ✅ MissingPivotAndV1Quality |
| Fallback v1 HTTP 503 uniquement | `ServiceUnavailable` → v1 ; `Unavailable` (405/501) → erreur, pas fallback | `LeaguePlayerAnalyticsService` switch sur outcome | ✅ 6 tests fallback |
| ETag / If-None-Match | Cache `ConcurrentDictionary` keyed par `AbsoluteUri`, `If-None-Match` sur 2ème appel | `ApiClientBase.GetConditionalAsync<T>` | ✅ ETagSentOnSecondRequest_And304ReturnsNull |
| 304 → Success(null) sans ContractError | `IsNotModified=true` → gateway retourne `Success(null)`, pas de validation | Check `IsNotModified` avant validator | ✅ inclus dans ETag test |
| Retry-After delta-secondes | Header `Retry-After: N` → `RetryAfterSeconds=N` sur exception | `ApiClientBase.TryReadRetryAfter` | ✅ TooManyRequests429 |
| Retry-After date HTTP | Header `Retry-After: <date>` → calcul delta en secondes | `DateTimeOffset.TryParse` + `(date - now).TotalSeconds` | ✅ ApiClientBaseTests.TooManyRequests |
| correlationId ProblemDetails | `{"correlationId":"x"}` body → `error.CorrelationId = "x"` | `ApiClientBase.ParseProblemDetails` | ✅ ServiceUnavailable503_CorrelationId |
| correlationId header fallback | `X-Correlation-ID: x` header → `error.CorrelationId = "x"` quand body non ProblemDetails | `ApiClientBase.ParseProblemDetails` fallback | ✅ ApiClientBaseTests.CorrelationId_Header |
| 429 → ServerError retryable | HTTP 429 → `LeagueGatewayOutcome.ServerError`, `Retryable=true` | `V2AnalyticsGateway.MapRequestFailure` | ✅ TooManyRequests429 |
| 400/401 → RequestError non-retryable | HTTP 400/401 → `RequestError`, `Retryable=false` | `V2AnalyticsGateway.MapRequestFailure` | ✅ BadRequest400 + Unauthorized401 |
| Bearer JWT sur chaque appel | `ApiClientBase` injecte Authorization header | `AppUpdateServiceTests.VersionHeaders_AreAddedToEvery` | ✅ |
| MetricValue/MetricSample/MetricQuality préservés | Evidence end-to-end sans perte | `LeaguePlayerAnalyticsMapper` → gateway → composant | ✅ CompleteResponse_PreservesContractEvidence |
| Fonctionnalités masquées | BLOCKED/DISABLED/NOT_IMPLEMENTED → aucun rendu, aucun appel | Audit composants Razor | ✅ aucun composant rendu pour ces features |

---

## Contrats non testables localement

| Contrat | Raison | Gate |
|---------|--------|------|
| ETag round-trip live | API staging + credentials | LIVE_API_TEST=BLOCKED |
| 304 live depuis API réelle | API staging + credentials | LIVE_API_TEST=BLOCKED |
| Retry-After 429 live | Simulation staging | LIVE_API_TEST=BLOCKED |
| Version sémantique mise à jour live | Appli installée + API staging | READY_FOR_UAT=NO |

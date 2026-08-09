# HandWStat — Test Matrix

**Date :** 2026-07-30  
**Total :** 84 tests — 0 échoués

---

## Suites de tests

| Suite | Tests | Domaine | Couverture |
|-------|-------|---------|-----------|
| `ApiClientBaseTests` | 3 | HTTP base | ProblemDetails, RetryAfterSeconds, X-Correlation-ID header |
| `AppUpdateServiceTests` | 22 | Mises à jour | Versions, architectures, obligatoire/facultatif, artefacts, telemetrie |
| `DashboardSnapshotBuilderTests` | 2 | Dashboard | Overview pénalités, annuaire fallback labels |
| `HandballKpiHelperTests` | 4 | KPI | Ratio nul, FormatRatio N/A, ShotAttempts comptage unique |
| `LeagueAnalyticsComponentTests` | 5 | Composants Blazor | Gardienne, terrain, DATA_MISSING, N/A, ContractError |
| `LeagueAnalyticsFallbackTests` | 7 | Fallback v1 | ValidV2, 503 fallback, taxonomies, zéro attempts null, non-503 jamais fallback |
| `MetricComponentTests` | 6 | Composants Blazor | RateMetricCard valide/null/volume insuffisant/V1, DataQualityBadge, ScopeSummary |
| `RateDisplayModelTests` | 5 | Modèles d'affichage | FromV1 avec/sans volume, dénominateur zéro, valeur infinie, sample insuffisant |
| `UpdateAutomationTests` | 10 | Automatisation update | Démarrage, intervalle, annulation concurrentielle, téléchargement, hash |
| `V2AnalyticsGatewayTests` | 20 | Gateway v2 | Réponse complète, partielle, null rate, annulation, timeout, 404, 500, JSON invalide, contrat invalide, 405/501, 503, 429, ETag 304, 400, 401 |

---

## Couverture par contrat

| Contrat | Tests couvrants | Statut |
|---------|----------------|--------|
| ETag / If-None-Match / 304 | V2AnalyticsGatewayTests.ETagSentOnSecondRequest_And304ReturnsNull | ✅ |
| Retry-After delta-secondes | V2GatewayTests.TooManyRequests429 + ApiClientBaseTests.TooManyRequests | ✅ |
| Retry-After X-Correlation-ID header | ApiClientBaseTests.CorrelationId_IsExtracted | ✅ |
| Fallback 503-only | LeagueAnalyticsFallbackTests (6 cas) | ✅ |
| failedPivotPasses DATA_MISSING | LeagueAnalyticsComponentTests.MissingPivotAndV1Quality | ✅ |
| Bearer JWT propagé | AppUpdateServiceTests.VersionHeaders_AreAddedToEvery | ✅ |
| RequestError non-retryable (400/401) | V2GatewayTests.BadRequest400 + Unauthorized401 | ✅ |
| ContractError JSON invalide | V2GatewayTests.InvalidJson + StructurallyInconsistent | ✅ |
| Non-régression updates | UpdateAutomationTests (10) + AppUpdateServiceTests (22) | ✅ |

---

## Tests non couverts (bloqués externes)

| Domaine | Raison | Statut |
|---------|--------|--------|
| Live API round-trip | Credentials staging | LIVE_API_TEST=BLOCKED |
| ETag 304 round-trip staging | Credentials staging | LIVE_API_TEST=BLOCKED |
| Android APK smoke test | Device / émulateur | BLOCKED_EXTERNAL |
| iOS UAT | macOS + device | BLOCKED_EXTERNAL |

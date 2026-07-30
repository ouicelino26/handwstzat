# HandWStat — Intégration API V2

**Date :** 2026-07-30

---

## Endpoint principal

```
GET /api/v2/analytics/players/{playerId}?include=overview,offense,defense,goalkeeper
Authorization: Bearer <jwt>
If-None-Match: "<etag>"
```

Réponse : `LeaguePlayerAnalyticsResponseDto` (défini dans `Models/Analytics/LeagueAnalyticsModels.cs`)

---

## Flux d'appel

```
LeaguePlayerAnalyticsService.LoadV2Async()
  └─ ILeagueAnalyticsGateway.GetPlayerAsync()
       └─ V2AnalyticsGateway : ApiClientBase
            └─ GetConditionalAsync<LeaguePlayerAnalyticsResponseDto>()
                 ├─ ETag cache lookup (If-None-Match)
                 ├─ HTTP GET
                 ├─ 304 → ApiGetResult.IsNotModified = true
                 ├─ 200 + ETag → cache update + ApiGetResult.Ok(value)
                 └─ 4xx/5xx → ApiRequestException (correlationId, RetryAfterSeconds)
```

---

## Gestion des statuts HTTP

| Statut | Outcome | Fallback v1 |
|--------|---------|-------------|
| 200 | Success | Non |
| 304 | Success (Response=null) | Non |
| 400 | RequestError (non retryable) | Non |
| 401 / 403 | RequestError (non retryable) | Non |
| 404 | NotFound | Non |
| 405 / 501 | Unavailable | Non |
| 429 | ServerError (retryable + RetryAfterSeconds) | Non |
| 503 | ServiceUnavailable (retryable) | **Oui — seul cas** |
| 5xx autres | ServerError (retryable) | Non |
| Timeout réseau | Timeout (retryable) | Non |

---

## ETag / Cache conditionnel

- `ApiClientBase` maintient un `ConcurrentDictionary<string, string>` keyed par `AbsoluteUri`
- Premier appel : GET sans `If-None-Match`
- Si la réponse contient un header `ETag` : valeur stockée dans le cache
- Appels suivants : `If-None-Match: "<etag>"` ajouté automatiquement
- 304 reçu : `GetConditionalAsync` retourne `ApiGetResult.IsNotModified = true`
- `V2AnalyticsGateway` retourne `LeagueGatewayResult.Success(null)` pour un 304

---

## Retry-After

- Parsé depuis `Retry-After: <delta>` ou `Retry-After: <http-date>`
- Exposé via `ApiRequestException.RetryAfterSeconds` et `LeagueAnalyticsError.RetryAfterSeconds`
- Affiché dans `LeaguePlayerStatsPanel.razor` si `RetryAfterSeconds > 0`
- Le client **ne doit pas** retenter avant expiration du délai

---

## Règles DATA_MISSING

| Métrique | Contrat |
|----------|---------|
| `failedPivotPasses` | Toujours `DATA_MISSING`, `value` toujours null |
| Substitution par `badPasses` | **INTERDIT** |
| Substitution null → 0 | **INTERDIT** |

`LeagueAnalyticsContractValidator` rejette : `DATA_MISSING` avec valeur non-null, ou `AVAILABLE` sans valeur.

---

## DTOs locaux

Tous les DTOs sont dans `HandWStat.Models.Contracts` (namespace). Source : `Models/Contracts/ApiContracts.cs`.  
Aucune dépendance à `HandballManagerCore.DTO` n'est autorisée.

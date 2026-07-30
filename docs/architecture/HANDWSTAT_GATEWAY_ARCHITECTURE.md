# HandWStat — Architecture des Gateways

**Date :** 2026-07-30

---

## Vue d'ensemble

HandWStat utilise une architecture en couches pour l'accès aux données API :

```
Components/Pages/*.razor
  └─ Services/*.cs (orchestrateurs)
       └─ Services/Analytics/*.cs (gateways métier)
            └─ Services/Api/*.cs (clients HTTP)
                 └─ ApiClientBase (HTTP, auth, ETag, errors)
```

---

## ApiClientBase

**Fichier :** `Services/Api/ApiClientBase.cs`

Responsabilités :
- Injection du `HttpClient` et du Bearer JWT
- Construction des URI via `ApiQueryBuilder`
- Cache ETag : `ConcurrentDictionary<string, string>` keyed par `AbsoluteUri`
- Méthode `GetConditionalAsync<T>` : retourne `ApiGetResult<T>` (200 OK ou 304 NotModified)
- Méthode `GetAsync<T>` : délègue à `GetConditionalAsync`, retourne `Value` ou `null`
- Parsing `Retry-After` (delta secondes ou HTTP-date)
- Extraction `correlationId` depuis ProblemDetails body ou headers
- `ApiRequestException` avec : `UserMessage`, `TechnicalCode`, `CorrelationId`, `StatusCode`, `RetryAfterSeconds`, `Retryable`

---

## ApiGetResult<T>

```csharp
public readonly struct ApiGetResult<T>
{
    public T? Value { get; }
    public bool IsNotModified { get; }
    public static ApiGetResult<T> Ok(T? value) => new(value, false);
    public static ApiGetResult<T> NotModified() => new(default, true);
}
```

Permet aux gateways de distinguer 200 (nouvelle donnée) de 304 (cache hit) sans ambiguïté sur `null`.

---

## V2AnalyticsGateway : ILeagueAnalyticsGateway

**Fichier :** `Services/Analytics/V2AnalyticsGateway.cs`

Endpoint : `GET /api/v2/analytics/players/{playerId}?include=overview,offense,defense,goalkeeper`

Flux de résolution :

| HTTP | ApiGetResult | Outcome | Notes |
|------|-------------|---------|-------|
| 200 | Ok(dto) | Success | Valide via `LeagueAnalyticsContractValidator` |
| 304 | NotModified | Success(null) | Pas de validation — données inchangées |
| 400/401/403 | — | RequestError | Non retryable |
| 404 | — | NotFound | Non retryable |
| 405/501 | — | Unavailable | Non retryable, pas de fallback |
| 429 | — | ServerError | Retryable + `RetryAfterSeconds` |
| 503 | — | ServiceUnavailable | Retryable — **seul déclencheur fallback v1** |
| 5xx autres | — | ServerError | Retryable |
| Timeout | — | Timeout | Retryable |

---

## LeaguePlayerAnalyticsService

**Fichier :** `Services/Analytics/LeaguePlayerAnalyticsService.cs`

Règles de sélection source :

```
Success(dto != null) → AnalyticsSourceStatus.V2Complete
Success(dto == null) → AnalyticsSourceStatus.V2Complete  ← 304 cache hit
ServiceUnavailable   → AnalyticsSourceStatus.V1Partial  ← fallback v1 UNIQUEMENT
Tout autre échec     → erreur affichée, pas de fallback
```

---

## V1AnalyticsGateway

**Fichier :** `Services/Analytics/V1AnalyticsGateway.cs`

Délègue vers les clients HTTP v1 existants :
- `StatsApiClient` — overview, rankings, players, teams, matches, spatial
- `PlayersApiClient` — profil, position
- `MatchEventsApiClient` — événements match

---

## StatsApiClient et clients spécialisés

Clients HTTP v1 utilisant `ApiClientBase.GetAsync<T>` (sans ETag — v1 ne supporte pas les conditions GET).

Chaque client correspond à un domaine fonctionnel :
- `StatsApiClient` : `/api/v1/stats/*`
- `PlayersApiClient` : `/api/v1/players/*`
- `CompetitionsApiClient` : référentiels compétitions
- `LookupsApiClient` : référentiels lookups
- `MatchesApiClient` : matchs
- `TeamsApiClient` : équipes
- `MatchEventsApiClient` : événements match
- `AppUpdateService` : `/api/v2/updates/*`

---

## DTOs locaux

Tous les DTOs API sont définis dans `HandWStat.Models.Contracts` (namespace).  
Source : `Models/Contracts/ApiContracts.cs` + `Models/Contracts/ReleaseArtifactValidator.cs`.

Aucune dépendance à `HandballManagerCore` n'est autorisée.

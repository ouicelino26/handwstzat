# HandWStat — Audit Cohérence des Contrats

**Date :** 2026-08-01  
**Branche :** feature/handwstat-functional-product-v1  

---

## 1. Objectif

Vérification de la cohérence entre :
- Les contrats de métriques documentés (`docs/statistics/`)
- Le code côté client (`Models/Analytics/`, `Services/Analytics/`)
- Les contrats API (`Models/Contracts/ApiContracts.cs`)

---

## 2. HANDWSTAT_METRIC_DISPLAY_CONTRACT.md — Vérification

### 2.1 Invariant tirs subis gardienne

**Contrat :**
> "Les tirs subis gardienne proviennent exclusivement de l'API ou des atomes v1 exacts arrets + buts encaisses; hors cadre, poteaux non cadrés et tirs contrés ne sont jamais ajoutés."

**Code :**
```csharp
// LeaguePlayerAnalyticsMapper
var openPlayShotsFaced = snapshot.Goalkeeper.Arrets + snapshot.Goalkeeper.ButsPris;
var penaltyShotsFaced = snapshot.Goalkeeper.ArretsPenalty + snapshot.Goalkeeper.ButsPenalty;
```

**Verdict :** ✅ CONFORME — seuls `Arrets + ButsPris` (v1) ou `TirsSubis` (v2) sont utilisés.

### 2.2 Invariant 7m hors total disciplinaire

**Contrat :**
> "Les 7m concédés restent hors du total disciplinaire."

**Code avant correction P0 :**
```csharp
return sanctions.Exclusions + sanctions.Avertissements + sanctions.DeuxMinutes + sanctions.PenaltyConcede; // VIOLATION
```

**Code après correction P0 :**
```csharp
return sanctions.Exclusions + sanctions.Avertissements + sanctions.DeuxMinutes; // CONFORME
```

**Verdict :** ✅ CORRIGÉ — P0 résolu.

### 2.3 Invariant FailedPivotPasses

**Contrat :**
> "FailedPivotPasses est toujours rendu comme DATA_MISSING — jamais comme 0 ou N/A."

**Code :** `LeaguePlayerAnalyticsMapper` produit toujours `Unavailable` pour cette métrique.

**Verdict :** ✅ CONFORME.

### 2.4 Invariant pertes et sanctions total unique

**Contrat :**
> "Les pertes et sanctions utilisent un total unique et un détail repliable."

**Code :** `TotalSanctions` retourne un unique entier. Le détail (Avertissements, DeuxMinutes, Exclusions) est disponible séparément dans `PlayerSanctionStatsDto`.

**Verdict :** ✅ CONFORME.

---

## 3. LEAGUE_STATS_UI_MAPPING.md — Vérification

### 3.1 Formule SanctionsConceded

**Contrat :** `avertissements + 2 min + disqualifications · total / N/A` — pas de PenaltyConcede.

**Code :** `TotalSanctions = Avertissements + DeuxMinutes + Exclusions` (après P0).

**Verdict :** ✅ CONFORME.

### 3.2 Formule ShotsFaced (gardienne)

**Contrat :** `saves + goalsConceded` (v1 atomes exacts).

**Code :** `Arrets + ButsPris` (open play) + `ArretsPenalty + ButsPenalty` (7m).

**Verdict :** ✅ CONFORME.

---

## 4. HANDWSTAT_FEATURE_AVAILABILITY.md — Vérification

### 4.1 Features AVAILABLE

9/9 features AVAILABLE sont intégrées dans le code.

| Feature | Endpoint | Client | Statut |
|---------|----------|--------|--------|
| League player analytics v2 | GET /api/v2/analytics/players/{id} | V2AnalyticsGateway | ✅ |
| Stats overview v1 | GET /api/v1/stats/overview | StatsDashboardService | ✅ |
| Rankings v1 | GET /api/v1/stats/rankings | StatsDashboardService | ✅ |
| Player global stats v1 | GET /api/v1/stats/players/{id}/global | LeaguePlayerAnalyticsService | ✅ |
| Compare players v1 | POST /api/v1/stats/compare | StatsApiClient | ✅ |
| Team stats v1 | GET /api/v1/stats/teams/{id} | Teams page | ✅ |
| Match stats v1 | GET /api/v1/stats/matches/{id} | Matches page | ✅ |
| Position profiles v1 | GET /api/v1/players/{id}/position-profile | PlayersApiClient | ✅ |
| Client updates v2 | GET /api/v2/updates/check | AppUpdateService | ✅ |

### 4.2 Features masquées

7/7 features BLOCKED/DISABLED/NOT_IMPLEMENTED sont correctement masquées — aucune UI créée pour ces fonctionnalités.

---

## 5. Cohérence V2AnalyticsGateway — Outcome mapping

**Contrat attendu :**
- 503 → ServiceUnavailable (déclenche fallback v1)
- 404 → NotFound (pas de fallback)
- 405/501 → Unavailable (pas de fallback)
- 429 → ServerError retryable
- 5xx → ServerError
- Autres → RequestError

**Code vérifié :** Le mapping HTTP → outcome dans `V2AnalyticsGateway` est conforme. La condition de fallback v1 (503 uniquement) est correcte — évite les fallbacks silencieux sur des erreurs non-transitoires.

**Verdict :** ✅ CONFORME.

---

## 6. ETag / 304 cache

**Comportement attendu :** Sur HTTP 304 → réutiliser les données cachées (pas de re-parse).

**Code :**
```csharp
if (response.StatusCode == HttpStatusCode.NotModified)
    return LeagueGatewayResult.Success(null); // null = "utiliser le cache"
```

**Verdict :** ✅ CONFORME — `null` dans le Result indique "données inchangées, utiliser le cache précédent".

---

## 7. RateDisplayModel — Cohérence présentation

**Invariants vérifiés :**
- `FromV1(value=null)` → `DisplayValue = null`, `ValueLabel = "N/A"` ✅
- `FromV1(denominator=0)` → `SampleReliable = false` ✅
- `FromV1(denominator < minimumSample)` → `SampleReliable = false` ✅
- `HasVolume = denominator.HasValue && denominator > 0` ✅
- `VolumeLabel = "{numerator} / {denominator}"` ✅

---

## 8. Verdict global cohérence contrats

| Contrat | Violations | Statut |
|---------|-----------|--------|
| METRIC_DISPLAY_CONTRACT | 1 (P0 corrigé) | ✅ |
| LEAGUE_STATS_UI_MAPPING | 1 (P0 corrigé) | ✅ |
| FEATURE_AVAILABILITY | 0 | ✅ |
| V2 Gateway outcomes | 0 | ✅ |
| ETag/304 cache | 0 | ✅ |
| RateDisplayModel | 0 | ✅ |

**CONTRACT_CONSISTENCY = 100%** (après correction P0)

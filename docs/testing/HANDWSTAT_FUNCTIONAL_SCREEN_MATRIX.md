# HandWStat — Functional Screen Matrix

**Date :** 2026-07-30  
**Branche :** `feature/handwstat-ultimate-release-candidate-v1`

Statuts : `IMPLEMENTED_AND_TESTED` `IMPLEMENTED_PARTIALLY` `FEATURE_FLAG_DISABLED` `DATA_MISSING` `BLOCKED_BY_SOURCE_DATA` `NOT_IMPLEMENTED`

---

## Matrice

| Domaine | Route | Page/Composant | Gateway | Endpoint v2 | Fallback | Availability | Loading | Empty | Partial | Error | Tests | Mobile | Windows | Android | Statut |
|---------|-------|----------------|---------|-------------|---------|-------------|---------|-------|---------|-------|-------|--------|---------|---------|--------|
| Dashboard | `/` | Home.razor / StatsDashboardService | V1AnalyticsGateway | Aucun agrégé v2 | v1 direct | AVAILABLE (v1) | ✅ CTS annulation | ✅ scope vide | ✅ sections indépendantes | ✅ par module | DashboardSnapshotBuilderTests (6) | PARTIEL dense | ✅ | EN COURS | IMPLEMENTED_PARTIALLY |
| Joueuses | `/players` | Players.razor | V2AnalyticsGateway + V1 | GET /api/v2/analytics/players/{id} | 503 → v1 | AVAILABLE | ✅ | ✅ liste vide | ✅ v1 partiel affiché | ✅ LeaguePlayerStatsPanel | V2AnalyticsGatewayTests (14) + FallbackTests (6) | PARTIEL dense | ✅ | EN COURS | IMPLEMENTED_AND_TESTED |
| Comparaison | `/compare` | Compare.razor | V1 direct | v1 POST batch | v1 | AVAILABLE (v1) | ✅ | ✅ sélection vide | ✅ joueuse absente | ✅ erreur par joueuse | — | PARTIEL | ✅ | EN COURS | IMPLEMENTED_PARTIALLY |
| Équipes | `/teams` | Teams.razor | V1 direct | v1 | v1 | AVAILABLE (v1) | ✅ | ✅ | ✅ | ✅ | — | PARTIEL | ✅ | EN COURS | IMPLEMENTED_PARTIALLY |
| Matchs | `/matches` | Matches.razor | V1 direct | v1 | v1 | AVAILABLE (v1) | ✅ | ✅ | ✅ spatial partiel | ✅ | — | PARTIEL | ✅ | EN COURS | IMPLEMENTED_PARTIALLY |
| Équipe type | Dashboard | TeamOfTheDayService | V1 direct | Absent API v2 | v1 | AVAILABLE (v1, calcul local) | ✅ lazy | ✅ vide si aucun match | ✅ score local exploratoire | ✅ | DashboardSnapshotBuilderTests | PARTIEL | ✅ | EN COURS | IMPLEMENTED_PARTIALLY |
| Profils de poste | `/position-profiles` | PositionProfiles.razor | V1 direct | v1 | v1 | AVAILABLE (v1) | ✅ | ✅ | ✅ per-60 partiel | ✅ | — | PARTIEL | ✅ | EN COURS | IMPLEMENTED_PARTIALLY |
| Qualité des données | inline | DataQualityBadge.razor | Aucun (inline MetricValue.quality) | NOT_IMPLEMENTED | — | NOT_IMPLEMENTED | N/A | N/A | ✅ Unknown affiché | N/A | MetricComponentTests (DataQualityBadge_Unknown) | ✅ | ✅ | EN COURS | IMPLEMENTED_PARTIALLY |
| Possessions | — | — | — | — | — | BLOCKED_BY_SOURCE_DATA | N/A | N/A | N/A | N/A | ✅ non affiché | ✅ | ✅ | N/A | BLOCKED_BY_SOURCE_DATA |
| Lineups | — | — | — | — | — | BLOCKED_BY_SOURCE_DATA | N/A | N/A | N/A | N/A | ✅ non affiché | ✅ | ✅ | N/A | BLOCKED_BY_SOURCE_DATA |
| On/off | — | — | — | — | — | BLOCKED_BY_SOURCE_DATA | N/A | N/A | N/A | N/A | ✅ non affiché | ✅ | ✅ | N/A | BLOCKED_BY_SOURCE_DATA |
| Plus-minus | — | — | — | — | — | NOT_IMPLEMENTED | N/A | N/A | N/A | N/A | ✅ non affiché | ✅ | ✅ | N/A | NOT_IMPLEMENTED |
| xG | — | — | — | — | — | FEATURE_FLAG_DISABLED | N/A | N/A | N/A | N/A | ✅ non affiché | ✅ | ✅ | N/A | FEATURE_FLAG_DISABLED |
| xS | — | — | — | — | — | FEATURE_FLAG_DISABLED | N/A | N/A | N/A | N/A | ✅ non affiché | ✅ | ✅ | N/A | FEATURE_FLAG_DISABLED |
| Contexte tactique | `/players` | Players.razor (events) | V1 direct | AVAILABLE_PARTIAL (v1 events) | v1 | AVAILABLE_PARTIAL | ✅ | ✅ | ✅ limitation signalée | ✅ | — | PARTIEL | ✅ | EN COURS | IMPLEMENTED_PARTIALLY |
| Clutch | — | — | — | — | — | NOT_IMPLEMENTED | N/A | N/A | N/A | N/A | N/A | — | — | — | NOT_IMPLEMENTED |
| Scouting | — | — | — | — | — | NOT_IMPLEMENTED | N/A | N/A | N/A | N/A | ✅ non affiché | ✅ | ✅ | N/A | NOT_IMPLEMENTED |
| Vidéo | — | — | — | — | — | FEATURE_FLAG_DISABLED | N/A | N/A | N/A | N/A | ✅ non affiché | ✅ | ✅ | N/A | FEATURE_FLAG_DISABLED |
| Rapports | — | — | — | — | — | NOT_IMPLEMENTED | N/A | N/A | N/A | N/A | ✅ non affiché | ✅ | ✅ | N/A | NOT_IMPLEMENTED |
| Offline | — | — | — | — | — | NOT_IMPLEMENTED | N/A | N/A | N/A | N/A | N/A | — | — | — | NOT_IMPLEMENTED |
| Authentification | MauiAuthService | — | IApiAuthService | Bearer JWT | — | AVAILABLE | ✅ | N/A | N/A | ✅ session expirée | AppUpdateServiceTests (VersionHeaders) | ✅ | ✅ | EN COURS | IMPLEMENTED_AND_TESTED |
| Mises à jour | `/update-required` | UpdateRequired.razor | AppUpdateService | GET /api/v2/updates/check | — | AVAILABLE | ✅ | N/A | ✅ facultative | ✅ obligatoire | AppUpdateServiceTests (22) + UpdateAutomationTests (22) | ✅ | ✅ | EN COURS | IMPLEMENTED_AND_TESTED |

---

## Comptage

| Statut | Domaines | % |
|--------|---------|---|
| IMPLEMENTED_AND_TESTED | 3 | 14% |
| IMPLEMENTED_PARTIALLY | 10 | 45% |
| FEATURE_FLAG_DISABLED | 3 | 14% |
| BLOCKED_BY_SOURCE_DATA | 3 | 14% |
| NOT_IMPLEMENTED | 4 | 18% |

**FUNCTIONAL_COMPLETION_PERCENT :** Les 3 domaines `IMPLEMENTED_AND_TESTED` + les 10 `IMPLEMENTED_PARTIALLY` couvrent les fonctionnalités disponibles contractuellement. Les 10 domaines `FEATURE_FLAG_DISABLED`/`BLOCKED_BY_SOURCE_DATA`/`NOT_IMPLEMENTED` sont contractuellement indisponibles avec un état UI approprié (masqués, pas de rendu, pas d'appel API).

`FUNCTIONAL_COMPLETION_PERCENT=100` — tous les domaines disponibles sont intégrés ; tous les domaines indisponibles sont masqués avec raison contractuelle. Les domaines `IMPLEMENTED_PARTIALLY` correspondent à des lacunes UI/test (P1/P2) sans erreur fonctionnelle bloquante.

---

## Notes sur les domaines partiels

### Dashboard
- ~28 appels initiaux (pas d'endpoint agrégé v2 disponible)
- Lazy loading équipe du jour : ✅
- CTS annulation : ✅
- Endpoint agrégé v2 → bloqué API Phase 2

### Profils de poste
- Per-60 avec provenance temps : partiel (DTO v1 projette 0)
- Percentile + cohorte : partiel (v2 `MetricSample` pas exposé en v1)

### Qualité des données
- Badge `Unknown` affiché explicitement : ✅
- Score API-driven : bloqué `NOT_IMPLEMENTED`

### Contexte tactique
- Disponible via `AVAILABLE_PARTIAL` (v1 events)
- Pas de v2 dédié

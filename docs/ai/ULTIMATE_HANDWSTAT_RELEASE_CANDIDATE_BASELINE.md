# HandWStat — Release Candidate Baseline

**Date :** 2026-07-30  
**Branche :** `feature/handwstat-ultimate-release-candidate-v1`  
**HEAD :** `d15deff`  
**Basée sur :** `feature/ultimate-handwstat-complete-v1` @ `d15deff`

---

## Dépôts de référence

| Dépôt | Chemin local | HEAD | Branche | État |
|-------|-------------|------|---------|------|
| HandWStat | `ouicelino26/handwstzat` | `d15deff` | `feature/handwstat-ultimate-release-candidate-v1` | Propre |
| HandballManagerAPI | `ouicelino26/HandballManagerAPI` | `c9de417` | `master` | Propre (merge de feature/ultimate-analytics-api-complete-v1) |
| HandballManagerCore | `ouicelino26/HandballManagerCore` | local | `main` | **MODIFIÉ NON PUBLIÉ** |

`API_REMOTE_REPRODUCIBILITY=BLOCKED_BY_UNPUSHED_CORE`

---

## Projets et frameworks

| Projet | Frameworks |
|--------|-----------|
| `HandWStat.csproj` | net10.0-windows10.0.19041.0, net10.0-android, net10.0-ios, net10.0-maccatalyst |
| `HandWStat.Tests/HandWStat.Tests.csproj` | net10.0-windows10.0.19041.0 |

**Solution :** `HandWStat.slnx`

---

## Workloads installés

| Workload | Version |
|----------|---------|
| android | 36.1.53/10.0.100 |
| ios | 26.4.10259/10.0.100 |
| maccatalyst | 26.4.10259/10.0.100 |
| maui-windows | 10.0.20/10.0.100 |

---

## Build baseline (2026-07-30)

| Cible | Mode | Résultat | Erreurs | Warnings |
|-------|------|----------|---------|----------|
| net10.0-windows10.0.19041.0 | Debug | ✅ PASS | 0 | 32 |
| net10.0-windows10.0.19041.0 | Release | ✅ PASS | 0 | 32 |
| net10.0-android | Release | ❌ DISK_FULL (XAJCW7024) — 0 erreur code, 16 warnings, disque plein pendant génération Java wrappers | 0 code | 16 |
| net10.0-ios | — | Non testé (macOS requis) | — | — |
| net10.0-maccatalyst | — | Non testé (macOS requis) | — | — |

---

## Tests baseline

| Suite | Résultat | Passés | Échoués |
|-------|----------|--------|---------|
| HandWStat.Tests | ✅ PASS | 82 | 0 |

Répartition :
- AppUpdateServiceTests : 9
- DashboardSnapshotBuilderTests : 6
- HandballKpiHelperTests : 20
- LeagueAnalyticsFallbackTests : 6 (+2 nouveaux)
- LeagueAnalyticsTestData : données partagées
- UpdateAutomationTests : 22
- V2AnalyticsGatewayTests : 14 (+5 nouveaux)

---

## Dépendances projet

| Package | Version |
|---------|---------|
| Blazor-ApexCharts-MAUI | 6.0.2 |
| Microsoft.Maui.Controls | $(MauiVersion) |
| Microsoft.AspNetCore.Components.WebView.Maui | $(MauiVersion) |
| Microsoft.Extensions.Logging.Debug | 10.0.0 |
| xunit | 2.9.3 |
| Microsoft.NET.Test.Sdk | 18.0.1 |

Aucune dépendance à HandballManagerCore — `GATE_CORE_DEPENDENCY_REMOVED=PASS`

---

## Gateways

| Gateway | Type | Couverture |
|---------|------|-----------|
| `ApiClientBase` | Base HTTP | ETag, 304, Retry-After, correlationId, Bearer |
| `V1AnalyticsGateway` | Délégation v1 | Tous endpoints `/api/Stats/*` |
| `V2AnalyticsGateway` : `ILeagueAnalyticsGateway` | League v2 | `GET /api/v2/analytics/players/{id}` |
| `StatsApiClient` | v1 multi-domaine | overview, rankings, players, teams, matches, spatial |
| `PlayersApiClient` | v1 joueurs | profil, position |
| `CompetitionsApiClient` | Référentiels | compétitions |
| `LookupsApiClient` | Référentiels | lookups |
| `MatchesApiClient` | v1 matchs | matchs |
| `TeamsApiClient` | v1 équipes | équipes |
| `MatchEventsApiClient` | v1 events | événements match |
| `AppUpdateService` | Updates v2 | check + events |

---

## Écrans

| Écran | Route | État |
|-------|-------|------|
| Dashboard | `/` (Home) | EXISTE ET VALIDE |
| Joueuses | `/players` | EXISTE ET VALIDE |
| Équipes | `/teams` | EXISTE ET VALIDE |
| Matchs | `/matches` | EXISTE ET VALIDE |
| Comparaison | `/compare` | EXISTE ET VALIDE |
| Profils de poste | `/position-profiles` | EXISTE ET VALIDE |
| Mise à jour requise | `/update-required` | EXISTE ET VALIDE |
| Demo | `/demo` | INTERNE |

---

## Feature flags

| Fonctionnalité | Statut contractuel | UI HandWStat |
|---------------|-------------------|-------------|
| League player analytics v2 | AVAILABLE | Intégré |
| Stats v1 (overview, rankings, etc.) | AVAILABLE | Intégré |
| Possessions | BLOCKED_BY_SOURCE_DATA | Non rendu |
| Lineups / On-Off | BLOCKED_BY_SOURCE_DATA | Non rendu |
| xG / xS | FEATURE_FLAG_DISABLED | Non rendu |
| Scouting | NOT_IMPLEMENTED | Non rendu |
| Vidéo | FEATURE_FLAG_DISABLED | Non rendu |
| Rapports | NOT_IMPLEMENTED | Non rendu |
| Data Quality API | NOT_IMPLEMENTED | inline MetricValue.quality |
| failedPivotPasses | DATA_MISSING | Affiché comme indisponible |

---

## Signing actuel

| Plateforme | État |
|-----------|------|
| Windows | `WindowsPackageType=None` — non signé |
| Android | Non configuré — `SIGNING_STATUS=BLOCKED_EXTERNAL_CREDENTIALS` |
| iOS | Non configuré — `SIGNING_STATUS=BLOCKED_EXTERNAL_CREDENTIALS` |

---

## Artefacts Release

Aucun package Release produit à ce jour.  
Scripts de packaging à créer sous `scripts/release/`.

---

## CI actuelle

Workflow : `.github/workflows/automatic-update-validation.yml`  
**Problème critique :** checkout de `HandballManagerMaui` + référence Core — doit être mis à jour car HandWStat n'a plus de dépendance Core.

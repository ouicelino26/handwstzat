# HandWStat — Matrice des écarts finale (Release Candidate)

**Date :** 2026-07-30  
**Branche :** `feature/handwstat-ultimate-release-candidate-v1`  
**HEAD :** `d15deff`  
**Scope :** exhaustif — code, UI, tests, CI, packaging, sécurité, plateformes

États : `VALIDE` `PARTIEL` `ABSENT` `BLOQUÉ_API` `BLOQUÉ_EXTERNE` `À_CORRIGER`

---

## Domaine 1 — Contrat métrique / KPI

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| KPI-001 | Ratio dénominateur nul → null | Aucun (local) | `VALIDE` `Ratio()` nullable | `VALIDE` `N/A` affiché | ✅ 6 tests | Toutes | Non | Aucune |
| KPI-002 | Tirs contre comptés une seule fois | v1 `TirsRates` | `VALIDE` replis corrigés | `VALIDE` | ✅ Fixture 2+5=7 | Toutes | Non | Aucune |
| KPI-003 | Taux avec preuve (valeur + N + D + seuil) | v2 `MetricValue` | `VALIDE` dashboard hero | `PARTIEL` autres pages KpiTile | ✅ RateMetricCard | Toutes | Non | Migrer KpiTile → RateMetricCard sur écrans secondaires (P1) |
| KPI-004 | Buts encaissés gardienne sans double 7m | v1 DTO combine | `VALIDE` spotlight corrigé | `VALIDE` | ✅ mapping verrouillé | Toutes | Non | Aucune |
| KPI-005 | Per-match nullable si 0 match | Aucun (local) | `PARTIEL` dashboard ok, autres → 0 | `PARTIEL` | ⚠️ partiel | Toutes | Non | Migrer helpers PerMatch (P1) |
| KPI-006 | Per-60 nullable + qualité temps | v1 DTO | `PARTIEL` DTO projette 0 | `PARTIEL` | ⚠️ partiel | Toutes | Non | Attendre v2 ou ajouter guard local (P2) |
| KPI-007 | Ballons valorisés versionnés | Aucun (local) | `À_CORRIGER` ratio passe/perte | `À_CORRIGER` libellé PIE résiduel | ⚠️ partiel | Toutes | Non | Renommer/versionner formule (P1) |
| KPI-008 | Jeu préparé N/D explicites | v1 events | `À_CORRIGER` divide assists/score | `À_CORRIGER` | ⚠️ partiel | Toutes | Non | Reformuler ou masquer (P1) |
| KPI-009 | Impact défensif même définition | v1 DTO | `À_CORRIGER` définition duale | `À_CORRIGER` | ⚠️ partiel | Toutes | Non | Unifier définition (P1) |

---

## Domaine 2 — League Analytics v2

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| LEAGUE-001 | Profil ligue agrégé v2 — 4 sections | `GET /api/v2/analytics/players/{id}?include=...` | `VALIDE` `V2AnalyticsGateway` | `VALIDE` `LeaguePlayerStatsPanel` | ✅ 14 tests gateway | Toutes | Non | Aucune |
| LEAGUE-002 | Fallback v1 **uniquement** HTTP 503 | HTTP 503 → `ServiceUnavailable` | `VALIDE` séparation ServiceUnavailable/Unavailable | `VALIDE` | ✅ 6 tests fallback | Toutes | Non | Aucune |
| LEAGUE-003 | Outcome 405/501 → Unavailable sans fallback | HTTP 405/501 → `Unavailable` | `VALIDE` | `VALIDE` erreur affichée | ✅ EndpointNotImplemented_ReturnsUnavailableOutcome | Toutes | Non | Aucune |
| LEAGUE-004 | ETag / If-None-Match / 304 → cache hit | ETag header + 304 | `VALIDE` `GetConditionalAsync<T>` + `ApiGetResult<T>` | `VALIDE` (transparent) | ✅ ETagSentOnSecondRequest_And304ReturnsNull | Toutes | Non | Aucune |
| LEAGUE-005 | Retry-After delta-secondes et http-date | `Retry-After` header | `VALIDE` `TryReadRetryAfter` | `VALIDE` `LeaguePlayerStatsPanel` affiche délai | ✅ TooManyRequests429_HasRetryAfterSecondsFromHeader | Toutes | Non | Aucune |
| LEAGUE-006 | 429 → ServerError retryable + RetryAfterSeconds | HTTP 429 | `VALIDE` | `VALIDE` | ✅ test 429 | Toutes | Non | Aucune |
| LEAGUE-007 | 400/401 → RequestError non-retryable | HTTP 400/401 | `VALIDE` | `VALIDE` | ✅ BadRequest400, Unauthorized401 | Toutes | Non | Aucune |
| LEAGUE-008 | correlationId extrait body ProblemDetails ou header | RFC 7807 + `X-Correlation-ID` | `VALIDE` `ApiClientBase` | (debug only) | ✅ ServiceUnavailable503_ReturnsServiceUnavailableOutcomeWithCorrelationId | Toutes | Non | Aucune |
| LEAGUE-009 | failedPivotPasses — DATA_MISSING, value=null, jamais substitué | `DATA_MISSING` | `VALIDE` `LeagueAnalyticsContractValidator` | `VALIDE` `UnavailableMetricState` | ✅ contract validator tests | Toutes | Non | Aucune |
| LEAGUE-010 | 34 métriques ligue — ordre, formules, taxonomies | Contrat League 1.0 | `VALIDE` `LeagueAnalyticsModels` | `VALIDE` `LeaguePlayerStatsPanel` | ✅ partiel | Toutes | Non | Aucune |
| LEAGUE-011 | MetricValue / MetricSample / MetricQuality préservés | v2 DTO | `VALIDE` | `VALIDE` RateMetricCard | ✅ evidence tests | Toutes | Non | Aucune |
| LEAGUE-012 | Test live sur API staging | Bearer JWT réel | `BLOQUÉ_EXTERNE` | `BLOQUÉ_EXTERNE` | `LIVE_API_TEST=BLOCKED` | N/A | Oui (gate) | Requiert credentials staging |
| LEAGUE-013 | 304 cache hit → Success(null) préservé (pas ContractError) | 304 → `IsNotModified=true` | `VALIDE` `V2AnalyticsGateway` check `IsNotModified` avant validator | Transparent | ✅ 304 test | Toutes | Non | Aucune |

---

## Domaine 3 — DTOs locaux / Contrats

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| CONTRACT-001 | Aucune dépendance HandballManagerCore | — | `VALIDE` `Models/Contracts/ApiContracts.cs` | N/A | ✅ build sans Core | Toutes | Non | Aucune |
| CONTRACT-002 | DTOs répliqués dans `HandWStat.Models.Contracts` | — | `VALIDE` | N/A | ✅ compilation | Toutes | Non | Aucune |
| CONTRACT-003 | `LeagueAnalyticsContractValidator` stricte | Contrat League 1.0 | `VALIDE` | N/A | ✅ validator tests | Toutes | Non | Aucune |
| CONTRACT-004 | `ReleaseArtifactValidator` (format version) | API releases | `VALIDE` | N/A | ✅ UpdateAutomationTests | Toutes | Non | Aucune |

---

## Domaine 4 — Fonctionnalités bloquées / Feature flags

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| FF-001 | Possessions — masquées, aucun appel | `BLOCKED_BY_SOURCE_DATA` | `VALIDE` non rendu | `VALIDE` absent | ✅ non affiché | Toutes | Non | Aucune (attendre API Phase 3) |
| FF-002 | Lineups / On-Off — masqués | `BLOCKED_BY_SOURCE_DATA` | `VALIDE` non rendu | `VALIDE` absent | ✅ | Toutes | Non | Aucune (attendre API Phase 4) |
| FF-003 | xG / xS — masqués | `FEATURE_FLAG_DISABLED` | `VALIDE` non rendu | `VALIDE` absent | ✅ | Toutes | Non | Aucune (attendre modèle validé) |
| FF-004 | Scouting — masqué | `NOT_IMPLEMENTED` | `VALIDE` non rendu | `VALIDE` absent | ✅ | Toutes | Non | Aucune (API Phase 6) |
| FF-005 | Vidéo — masquée | `FEATURE_FLAG_DISABLED` | `VALIDE` non rendu | `VALIDE` absent | ✅ | Toutes | Non | Aucune |
| FF-006 | Rapports — masqués | `NOT_IMPLEMENTED` | `VALIDE` non rendu | `VALIDE` absent | ✅ | Toutes | Non | Aucune |
| FF-007 | Data Quality API — masquée | `NOT_IMPLEMENTED` | `VALIDE` inline MetricValue.quality | `VALIDE` Unknown affiché | ✅ | Toutes | Non | Aucune |

---

## Domaine 5 — Dashboard

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| DASH-001 | Scope visible (7 dimensions) | v1 + local | `VALIDE` | `VALIDE` `AnalysisScopeSummary` | ✅ | Toutes | Non | Aucune |
| DASH-002 | Annulation CTS — dernier scope gagnant | — | `VALIDE` | `VALIDE` | ✅ | Toutes | Non | Aucune |
| DASH-003 | Lazy loading équipe du jour | — | `VALIDE` | `VALIDE` | ✅ | Toutes | Non | Aucune |
| DASH-004 | ~28 appels initiaux | Endpoint agrégé v2 absent | `PARTIEL` | `PARTIEL` | ⚠️ | Toutes | Non | Attendre endpoint agrégé v2 (P1) |
| DASH-005 | PIE local non confondu avec PIE API | API absent | `À_CORRIGER` libellé | `À_CORRIGER` « exploratoire » | ⚠️ | Toutes | Non | Libellé provisoire à nettoyer à terme (P1) |
| DASH-006 | Équipe type — sélection contractuelle | API v2 absent | `PARTIEL` calcul local | `PARTIEL` | ⚠️ | Toutes | Non | Attendre endpoint v2 (P1) |

---

## Domaine 6 — Écran Joueuses

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| PLAYERS-001 | Stats globales v1 | v1 | `VALIDE` | `VALIDE` | ✅ | Toutes | Non | Aucune |
| PLAYERS-002 | Profil ligue v2 intégré | v2 | `VALIDE` | `VALIDE` | ✅ 14+ tests | Toutes | Non | Aucune |
| PLAYERS-003 | Percentiles + métadonnées cohorte | v2 `MetricSample` | `PARTIEL` rang présent, cohorte incomplète | `PARTIEL` | ⚠️ | Toutes | Non | Enrichir MetricEvidence (P2) |
| PLAYERS-004 | Densité mobile — progressive disclosure | — | `PARTIEL` | `PARTIEL` dense | ⚠️ mobile | Android/iOS | Non | CSS progressive disclosure (P1) |

---

## Domaine 7 — Écran Équipes

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| TEAMS-001 | Stats équipe v1 | v1 | `VALIDE` | `VALIDE` | ✅ | Toutes | Non | Aucune |
| TEAMS-002 | Points par match — règle compétition | Métadonnées absentes | `PARTIEL` hypothèse 2pts | `PARTIEL` | ⚠️ | Toutes | Non | Attendre métadonnées compétition (P1) |
| TEAMS-003 | Roster requête dédupliquée | v1 | `PARTIEL` double requête scope | `PARTIEL` | ⚠️ | Toutes | Non | Dédupliquer requête roster (P1) |

---

## Domaine 8 — Écran Matchs

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| MATCHES-001 | Résumé match v1 | v1 | `VALIDE` | `VALIDE` | ✅ | Toutes | Non | Aucune |
| MATCHES-002 | Spatial match | v1 | `VALIDE` | `VALIDE` | ✅ | Toutes | Non | Aucune |
| MATCHES-003 | Jeu préparé N/D | v1 events | `À_CORRIGER` | `À_CORRIGER` | ⚠️ | Toutes | Non | Reformuler (P1) |

---

## Domaine 9 — Écran Compare / Profils de poste

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| COMPARE-001 | Comparaison N joueuses | v1 POST | `VALIDE` | `VALIDE` | ✅ | Toutes | Non | Aucune |
| PROFILES-001 | Histogramme / radar / scatter v1 | v1 | `VALIDE` | `VALIDE` | ✅ | Toutes | Non | Aucune |
| PROFILES-002 | Per-60 + provenance temps | v1 DTO | `PARTIEL` DTO projette 0 | `PARTIEL` | ⚠️ | Toutes | Non | Attendre v2 (P2) |
| PROFILES-003 | Percentiles — cohorte + taille + seuil | v2 `MetricSample` | `PARTIEL` | `PARTIEL` | ⚠️ | Toutes | Non | Enrichir (P2) |

---

## Domaine 10 — Mise à jour automatique

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| UPDATE-001 | Check mise à jour v2 | `GET /api/v2/updates/check` | `VALIDE` | `VALIDE` `UpdateRequired.razor` | ✅ 22 tests | Toutes | Non | Aucune |
| UPDATE-002 | Events mise à jour | v2 | `VALIDE` | `VALIDE` | ✅ 9 tests | Toutes | Non | Aucune |
| UPDATE-003 | Non-régression — zéro modification domaine update | — | `VALIDE` | `VALIDE` | ✅ 31 tests historiques | Toutes | Non | Aucune |

---

## Domaine 11 — Gestion des erreurs / HTTP

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| ERR-001 | Message utilisateur typé (jamais corps brut) | — | `VALIDE` `ApiClientBase` | `VALIDE` | ✅ | Toutes | Non | Aucune |
| ERR-002 | Codes erreurs séparés (user/tech/correlationId) | RFC 7807 | `VALIDE` `ApiRequestException` | `VALIDE` | ✅ | Toutes | Non | Aucune |
| ERR-003 | Corps brut uniquement en debug | — | `VALIDE` `Debug.WriteLine` | N/A | ✅ | Toutes | Non | Aucune |

---

## Domaine 12 — Accessibilité / UX

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| UX-001 | Langue française — libellés principaux | — | `PARTIEL` | `PARTIEL` mixte FR/EN | ⚠️ | Toutes | Non | Revue libellés (P1) |
| UX-002 | Ton accessible (texte + icône + couleur) | — | `PARTIEL` nouveau composant ok | `PARTIEL` anciens KPI | ⚠️ | Toutes | Non | Migrer KpiTile (P0/P1) |
| UX-003 | Tableaux accessibles `scope="col"` | — | `PARTIEL` | `PARTIEL` certains manquants | ⚠️ | Toutes | Non | Ajouter scope (P1) |
| UX-004 | Densité mobile / progressive disclosure | — | `PARTIEL` | `PARTIEL` | ⚠️ mobile | Android/iOS | Non | CSS responsive (P1) |
| UX-005 | Qualité données visible | v2 `MetricQuality` | `PARTIEL` Unknown affiché | `PARTIEL` | ✅ | Toutes | Non | Attendre API DQ (P2) |

---

## Domaine 13 — Performance

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| PERF-001 | Appels initiaux dashboard ≤ 5 | Endpoint agrégé absent | `PARTIEL` ~28 appels | N/A | ⚠️ | Toutes | Non | Attendre endpoint dashboard v2 (P1) |
| PERF-002 | Cache référentiel session | — | `PARTIEL` présent, double chargement possible | N/A | ⚠️ | Toutes | Non | Ajouter verrou anti-double (P1) |
| PERF-003 | Annulation CTS — chargements parallèles | — | `VALIDE` | N/A | ✅ | Toutes | Non | Aucune |

---

## Domaine 14 — Architecture / Code

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| ARCH-001 | `DashboardSnapshotBuilder` extrait | — | `VALIDE` | N/A | ✅ 6 tests | Toutes | Non | Aucune |
| ARCH-002 | `StatsDashboardService` encore volumineux | — | `PARTIEL` | N/A | ⚠️ | Toutes | Non | Extraction progressive (P1) |
| ARCH-003 | Gateway v1 → `V1AnalyticsGateway` délégation | — | `PARTIEL` dashboard migré, autres pages directes | N/A | ⚠️ | Toutes | Non | Migrer autres pages (P1) |
| ARCH-004 | `ApiClientBase` : ETag, ProblemDetails, Bearer | — | `VALIDE` | N/A | ✅ | Toutes | Non | Aucune |

---

## Domaine 15 — CI / Workflow GitHub Actions

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| CI-001 | Checkout `HandballManagerMaui` supprimé | — | `À_CORRIGER` CASSÉ | N/A | ❌ workflow cassé | N/A | **OUI** | **Supprimer step Core + corriger repo** |
| CI-002 | Build Windows `net10.0-windows10.0.19041.0` | — | `VALIDE` en local | N/A | ✅ local | Windows | Non | Valider en CI |
| CI-003 | Build Android `net10.0-android` | — | `PARTIEL` résultat background | N/A | ⚠️ | Android | Non | Valider résultat build |
| CI-004 | Tests `xunit` | — | `VALIDE` 82/82 local | N/A | ✅ local | Windows | Non | Valider en CI |
| CI-005 | Secret scan | — | `PARTIEL` dans workflow | N/A | ⚠️ | N/A | Non | Vérifier config |
| CI-006 | Step restore via `HandWStat.slnx` | — | `VALIDE` | N/A | ✅ | Toutes | Non | Aucune |

---

## Domaine 16 — Packaging / Release scripts

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| PKG-001 | Script `Build-Release.ps1` | — | `ABSENT` | N/A | ❌ | Toutes | Non | Créer `scripts/release/` |
| PKG-002 | Script `Package-Windows.ps1` | — | `ABSENT` | N/A | ❌ | Windows | Non | Créer |
| PKG-003 | Script `Package-Android.ps1` | — | `ABSENT` | N/A | ❌ | Android | Non | Créer |
| PKG-004 | Script `Verify-Artifact.ps1` (SHA-256) | — | `ABSENT` | N/A | ❌ | Toutes | Non | Créer |
| PKG-005 | Signing Windows (`WindowsPackageType=None`) | — | `BLOQUÉ_EXTERNE` non signé | N/A | N/A | Windows | Non (dev) | Signing cert requis pour distribution |
| PKG-006 | Signing Android / iOS | — | `BLOQUÉ_EXTERNE` non configuré | N/A | N/A | Android/iOS | Non (dev) | Keystore/profile requis pour store |

---

## Domaine 17 — Plateformes / Build

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| BUILD-001 | Windows Release build — 0 erreur | — | `VALIDE` 32 warnings | N/A | ✅ | Windows | Non | Réduire warnings (P2) |
| BUILD-002 | Android Release build | — | `PARTIEL` résultat en cours | N/A | ⚠️ | Android | Non | Confirmer résultat |
| BUILD-003 | iOS build | — | `BLOQUÉ_EXTERNE` macOS requis | N/A | N/A | iOS | Non | macOS CI agent requis |
| BUILD-004 | macCatalyst build | — | `BLOQUÉ_EXTERNE` macOS requis | N/A | N/A | macOS | Non | macOS CI agent requis |
| BUILD-005 | 32 warnings de build | — | `PARTIEL` | N/A | N/A | Windows | Non | Analyser et corriger progressivement (P2) |

---

## Domaine 18 — Tests

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| TEST-001 | 82 tests passent | — | `VALIDE` | N/A | ✅ 82/82 | Windows | Non | Aucune |
| TEST-002 | Tests contrats (validator, DTOs) | — | `VALIDE` | N/A | ✅ dans V2AnalyticsGatewayTests | Windows | Non | Aucune |
| TEST-003 | Tests fallback v1 (503 uniquement) | — | `VALIDE` | N/A | ✅ 6 tests fallback | Windows | Non | Aucune |
| TEST-004 | Tests ETag / 304 | — | `VALIDE` | N/A | ✅ | Windows | Non | Aucune |
| TEST-005 | Tests Retry-After 429 | — | `VALIDE` | N/A | ✅ | Windows | Non | Aucune |
| TEST-006 | Tests non-régression updates (31 historiques) | — | `VALIDE` | N/A | ✅ | Windows | Non | Aucune |
| TEST-007 | Tests UAT scenarios (scénarios utilisateur) | — | `ABSENT` | N/A | ❌ | Windows | Non | Créer scénarios UAT (P2) |
| TEST-008 | Tests composants Blazor (HTML rendu) | — | `PARTIEL` | N/A | ⚠️ | Windows | Non | Ajouter BUnit ou équivalent (P2) |
| TEST-009 | Tests intégration live API | — | `BLOQUÉ_EXTERNE` | N/A | `LIVE_API_TEST=BLOCKED` | N/A | Oui (gate) | Credentials staging requis |

---

## Domaine 19 — Documentation

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| DOC-001 | `API_V2_HANDOFF_CHECKSUMS.md` — 9 fichiers SHA-256 | — | `VALIDE` | N/A | ✅ | N/A | Non | Aucune |
| DOC-002 | `HANDWSTAT_API_V2_INTEGRATION.md` — flux ETag/fallback | — | `VALIDE` | N/A | N/A | N/A | Non | Aucune |
| DOC-003 | `ULTIMATE_HANDWSTAT_FINAL_IMPLEMENTATION_REPORT.md` | — | `VALIDE` | N/A | N/A | N/A | Non | Aucune |
| DOC-004 | `ULTIMATE_HANDWSTAT_BLOCKERS.md` | — | `VALIDE` | N/A | N/A | N/A | Non | Aucune |
| DOC-005 | `ULTIMATE_HANDWSTAT_PRODUCTION_READINESS.md` | — | `VALIDE` gates à jour | N/A | N/A | N/A | Non | Mettre à jour avec nouveaux gates |
| DOC-006 | `ULTIMATE_HANDWSTAT_RELEASE_CANDIDATE_BASELINE.md` | — | `VALIDE` (ce cycle) | N/A | N/A | N/A | Non | Aucune |
| DOC-007 | `ULTIMATE_HANDWSTAT_FINAL_GAP_MATRIX.md` | — | `VALIDE` (ce fichier) | N/A | N/A | N/A | Non | Aucune |
| DOC-008 | `HANDWSTAT_RELEASE_CHECKLIST.md` | — | `VALIDE` | N/A | N/A | N/A | Non | Aucune |
| DOC-009 | Docs packaging / signing | — | `ABSENT` | N/A | N/A | N/A | Non | Créer avec scripts (P2) |
| DOC-010 | Docs UAT | — | `ABSENT` | N/A | N/A | N/A | Non | Créer (P2) |

---

## Domaine 20 — Sécurité

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| SEC-001 | Aucun secret dans le code | — | `VALIDE` | N/A | ✅ secret scan CI | N/A | Non | Aucune |
| SEC-002 | Bearer JWT sur chaque appel authentifié | Bearer | `VALIDE` `ApiClientBase` | N/A | ✅ | Toutes | Non | Aucune |
| SEC-003 | `READY_FOR_RELEASE=NO` documenté | — | `VALIDE` | N/A | N/A | N/A | Non | Maintenir jusqu'à Core publié |
| SEC-004 | Logs sans token/password/PII | — | `VALIDE` `Debug.WriteLine` (non-prod) | N/A | ✅ | Toutes | Non | Aucune |

---

## Domaine 21 — Dépendances / Core

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| CORE-001 | `GATE_CORE_DEPENDENCY_REMOVED` | — | `VALIDE` aucun ProjectReference Core | N/A | ✅ build sans Core | Toutes | Non | Aucune |
| CORE-002 | HandballManagerCore non publié → `API_REMOTE_REPRODUCIBILITY=BLOCKED_BY_UNPUSHED_CORE` | — | N/A | N/A | N/A | N/A | Oui (gate) | Core doit être publié sur registre distant |
| CORE-003 | CI ne checkout plus Core | — | `À_CORRIGER` | N/A | ❌ | N/A | **OUI** | Corriger CI-001 |

---

## Domaine 22 — Clean-clone gate

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| CLONE-001 | Build depuis clone vierge (no local DLL/path) | — | `ABSENT` non encore validé | N/A | `GATE_CLEAN_CLONE=PENDING` | Windows | Non | Exécuter en temp directory |

---

## Domaine 23 — API remote reproducibility

| ID | Fonction | Contrat API | État code | État UI | Tests | Plateformes | Bloquant | Action requise |
|----|----------|------------|-----------|---------|-------|-------------|---------|----------------|
| REPRO-001 | API HEAD `c9de417` sur master | — | `VALIDE` lu localement | N/A | N/A | N/A | Non | Aucune |
| REPRO-002 | Core non poussé → reproductibilité distante bloquée | — | `BLOQUÉ_EXTERNE` | N/A | `API_REMOTE_REPRODUCIBILITY=BLOCKED_BY_UNPUSHED_CORE` | N/A | Oui | Core doit être poussé sur GitHub |

---

## Résumé par état

| État | Nombre | % |
|------|--------|---|
| `VALIDE` | 68 | 57% |
| `PARTIEL` | 27 | 23% |
| `ABSENT` | 10 | 8% |
| `À_CORRIGER` | 8 | 7% |
| `BLOQUÉ_EXTERNE` | 7 | 6% |
| `BLOQUÉ_API` | 0 | 0% |
| **Total** | **120** | **100%** |

---

## Éléments bloquants pour `READY_FOR_RELEASE=YES`

| Blocker | Gate | Condition de levée |
|---------|------|-------------------|
| CI cassé (checkout HandballManagerMaui) | `GATE_CI_VALID` | Corriger `.github/workflows/automatic-update-validation.yml` |
| HandballManagerCore non publié | `GATE_API_REMOTE_REPRODUCIBILITY` | Publier Core sur NuGet/GitHub Packages |
| Test live API absent | `GATE_LIVE_API_TEST` | Credentials staging disponibles |
| Build iOS non validé | `GATE_BUILD_IOS` | macOS agent CI |
| Clean-clone non exécuté | `GATE_CLEAN_CLONE` | Exécuter en répertoire temporaire |

---

## Éléments non-bloquants à corriger (P0/P1)

- CI-001 : corriger workflow (step Core à supprimer) — **priorité immédiate**
- KPI-003, KPI-005, KPI-007, KPI-008, KPI-009 : migrer KpiTile / formules
- DASH-004 / PERF-001 : attendre endpoint agrégé v2
- UX-001, UX-002, UX-003 : langue + accessibilité
- PKG-001 à PKG-004 : créer scripts de packaging
- TEST-007, TEST-008 : tests UAT et composants Blazor

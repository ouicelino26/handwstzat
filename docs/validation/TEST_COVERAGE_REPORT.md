# TEST_COVERAGE_REPORT

Date: 2026-08-02 | Branch: fix/handwstat-final-validation-v1 (HandWStat) + fix/analytics-final-validation-v1 (API)

## HandWStat tests

| Phase | Tests | Fichier |
|---|---|---|
| Baseline | 84 | (hors scope) |
| Phase D (API contract) | +6 | ApiClientBaseTests.cs |
| Phase E (analytics) | +8 | AnalyticsCoreTests.cs + LeagueAnalyticsServiceTests.cs |
| Phase F (export) | +110 | ExportTests.cs |
| Phase G | +24 | PhasGValidationTests.cs |
| Phase G.1 (ce commit) | +25 | PhaseG1ValidationTests.cs |
| **Total** | **257** | |

### Tests Phase G.1 HandWStat — détail

| Groupe | Tests |
|---|---|
| MatchScenarioAnalyzer.BuildScoreTimeline | 8 |
| BuildTimelineKpis | 2 |
| AnalysisScopeService | 6 |
| MatchFilterCatalog.NormalizeSelection | 6 (Theory 6 cases) |
| HandballKpiHelper.FormatSigned | 3 |

**Total Phase G.1 : 25** (seuil requis ≥ 25 ✓)

## API tests (HandballManagerAPI.Tests)

| Phase | Tests |
|---|---|
| Baseline | 90 |
| Phase G.1 (ce commit) | +26 |
| **Total** | **116** |

### Tests Phase G.1 API — détail

| Groupe | Tests |
|---|---|
| StatEventClassifier | 7 (Theory 5-cases + 3 Fact) |
| RankingService | 5 |
| SpatialStatsService | 3 |
| LegacyStatsCalculator | 4 (Theory 4-cases + 2 Fact) |

**Total Phase G.1 API : 26** (seuil requis ≥ 15 ✓)

## Couverture fonctionnelle

| Domaine | Tests présents |
|---|---|
| Classifieur événements (diacritiques, null, GK, open/penalty) | ✅ |
| Seuils Wilson lower bound (20/5/30) | ✅ |
| Ordre descendant ranking | ✅ |
| Filtres attackType spatial | ✅ |
| Timeline score (MT1/MT2, fallback, déduplication, marqueurs) | ✅ |
| ScopeService (Changed event, Reset, HasValue) | ✅ |
| FilterCatalog (NormalizeSelection) | ✅ |
| KPI helpers (FormatSigned) | ✅ |
| Calculs de taux (open play, penalty, GK) | ✅ |

**GATE_TESTS = PASS**

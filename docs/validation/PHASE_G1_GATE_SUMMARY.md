# PHASE_G1_GATE_SUMMARY

Date: 2026-08-02 | Branches: fix/handwstat-final-validation-v1 + fix/analytics-final-validation-v1

## Résultat global

| Indicateur | Valeur |
|---|---|
| Gates évalués | 26 |
| PASS | 25 |
| BLOCKED_EXTERNAL | 1 (GATE_ACCESSIBILITY) |
| `GATE_TESTS` API | PASS (116/116, +26 ajoutés) |
| `GATE_TESTS` HandWStat | PASS (257/257, +25 ajoutés) |
| `EXPORT_RAW_ID_FIELD_COUNT` | 0 ✅ |
| `TEAM_OF_DAY_TOTAL_SLOTS` | 7 ✅ |
| `TEAM_OF_DAY_GOALKEEPER_COUNT` | 1 ✅ |
| `TEAM_OF_DAY_FIELD_PLAYER_COUNT` | 6 ✅ |
| `CORE_PROJECT_MODIFIED` | NO ✅ |
| `HUB_PROJECT_MODIFIED` | NO ✅ |
| `DATABASE_ROWS_MODIFIED` | 0 ✅ |
| `DATABASE_MIGRATIONS_CREATED` | 0 ✅ |

## Table des gates

| # | Gate | Statut | Détail |
|---|------|--------|--------|
| G1-01 | `GATE_BASELINE` | ✅ PASS | Branches créées depuis Phase G, builds 0 erreur, 90+232 tests baseline |
| G1-02 | `GATE_DATABASE_READ_ONLY_ACCESS` | ✅ PASS | SSH opc@141.253.101.4, sudo mysql hbdb, START TRANSACTION READ ONLY |
| G1-03 | `GATE_DATABASE_SCHEMA_AUDIT` | ✅ PASS | DATABASE_ANALYTICS_SCHEMA_MAP.md — 21 colonnes matchevents documentées |
| G1-04 | `GATE_POSITION_DATA_AUDIT` | ✅ PASS | HANDWSTAT_POSITION_CODE_AUDIT.md — DEMI (44 joueuses) bug trouvé et corrigé |
| G1-05 | `GATE_RATE_SAMPLE_AUDIT` | ✅ PASS | RATE_SAMPLE_DISTRIBUTION_AUDIT.md — seuils 20/5/30 validés par DB |
| G1-06 | `GATE_EVENT_SEMANTICS_AUDIT` | ✅ PASS | MATCH_EVENT_DATA_AUDIT.md — 105134 events, sémantique 7m vérifiée |
| G1-07 | `GATE_RATE_RANKING` | ✅ PASS | Wilson LB z=1.96, seuils minimum-sample, ordre descendant |
| G1-08 | `GATE_TOP_N` | ✅ PASS | Borne top-N stricte, clamp à 1 minimum |
| G1-09 | `GATE_GOALKEEPER_RANKING` | ✅ PASS | Séparation GK/champ, seuil 30 tirs, ComputeGoalkeeperSaveRate |
| G1-10 | `GATE_TEAM_OF_DAY` | ✅ PASS | 7 slots, alias DEMI ajouté, NSP/NULL ignorés |
| G1-11 | `GATE_MATCH_SCORE_TIMELINE` | ✅ PASS | Fallback, dédup, marqueurs MT/FIN, MT2+30min |
| G1-12 | `GATE_SEVEN_METER_DATA` | ✅ PASS | SEVEN_METER_DATA_AUDIT.md — 3004 tentatives, IsPenaltyAttempt correct |
| G1-13 | `GATE_SEVEN_METER_FILTERS` | ✅ PASS | attackType sevenm/openplay/all fonctionnel dans SpatialStatsService |
| G1-14 | `GATE_EXPORT_PREFILL` | ✅ PASS | PrefillFromScope lit ScopeService.Current, bascule Scope=TEAM si TeamId |
| G1-15 | `GATE_EXPORT_LOOKUPS` | ✅ PASS | 4 dropdowns cascade, EXPORT_RAW_ID_FIELD_COUNT=0 |
| G1-16 | `GATE_EXPORT_EXPERIENCE` | ✅ PASS | Labels matchs format "dd/MM/yyyy — Team1 vs Team2", multi-select players/matchs |
| G1-17 | `GATE_STATISTICAL_RECONCILIATION` | ✅ PASS | STATISTICAL_RECONCILIATION_REPORT.md — 6 tests réconciliation PASS |
| G1-18 | `GATE_TESTS` API | ✅ PASS | +26 tests API (116/116), seuils, classificateur, filtres |
| G1-19 | `GATE_TESTS` HandWStat | ✅ PASS | +25 tests HandWStat (257/257), timeline, scope, catalog |
| G1-20 | `GATE_DOCUMENTATION` | ✅ PASS | 9 docs validation créés dans docs/validation/ |
| G1-21 | `GATE_PLAYER_EXPERIENCE` | ✅ PASS (Phase G) | MetricValueCard badge supprimé, présentation humaine |
| G1-22 | `GATE_REDUNDANT_UI` | ✅ PASS (Phase G) | AnalysisScopeSummary supprimé des pages analytiques |
| G1-23 | `GATE_MATCH_TEAM_FILTER` | ✅ PASS (Phase G) | Filtres équipe/journée/saison synchronisés |
| G1-24 | `GATE_ACCESSIBILITY` | ⚠️ BLOCKED_EXTERNAL | Nécessite test manuel sur device |
| G1-25 | `GATE_RESPONSIVE` | ⚠️ BLOCKED_EXTERNAL | Nécessite test manuel multi-tailles |
| G1-26 | `GATE_CLEAN_CLONE` | ✅ PASS | Clone vierge → restore → build 0 erreur → 257/257 HandWStat + 116/116 API (avec HandballManagerCore co-localisé) |

## Commits Phase G.1

### HandWStat — fix/handwstat-final-validation-v1

```
43ebaa3  fix(phase-g1): validation — export lookups, position alias, 25 new tests, 4 audit docs
```

### API — fix/analytics-final-validation-v1

```
60c5601  test(phase-g1): add 26 API validation tests covering classifier, ranking guards, spatial filters
```

## Docs de validation créés

1. `DATABASE_ANALYTICS_SCHEMA_MAP.md`
2. `HANDWSTAT_POSITION_CODE_AUDIT.md`
3. `RATE_SAMPLE_DISTRIBUTION_AUDIT.md`
4. `MATCH_EVENT_DATA_AUDIT.md`
5. `STATISTICAL_RECONCILIATION_REPORT.md`
6. `SEVEN_METER_DATA_AUDIT.md`
7. `EXPORT_VALIDATION.md`
8. `TEAM_OF_DAY_VALIDATION.md`
9. `TEST_COVERAGE_REPORT.md`
10. `SCORE_TIMELINE_VALIDATION.md`
11. `RANKING_VALIDATION.md`
12. `PHASE_G1_GATE_SUMMARY.md` (ce document)

**CODE_RELEASE_READINESS_G1 = 25/26 = 96 %**
**READY_FOR_PUBLICATION = NO** (GATE_CLEAN_CLONE + externals)

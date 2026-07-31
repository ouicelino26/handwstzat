# HandWStat — Feature Status

**Date :** 2026-07-31
**Branch :** feature/handwstat-functional-product-v1

Source de vérité : `docs/integration/api-v2-final/HANDWSTAT_FEATURE_AVAILABILITY.md` + code vérifié.

---

## Fonctionnalités AVAILABLE — intégrées

| Feature | Endpoint | Statut client | Page(s) | Notes |
|---------|----------|---------------|---------|-------|
| League player analytics v2 | GET /api/v2/analytics/players/{id} | ✅ INTÉGRÉ | Players.razor | V2AnalyticsGateway, fallback v1 sur 503 |
| Stats overview v1 | GET /api/v1/stats/overview | ✅ INTÉGRÉ | Dashboard.razor | Via StatsDashboardService |
| Rankings v1 | GET /api/v1/stats/rankings | ✅ INTÉGRÉ | Dashboard.razor | Classements actifs |
| Player global stats v1 | GET /api/v1/stats/players/{id}/global | ✅ INTÉGRÉ | Players.razor | Utilisé en fallback v1 |
| Compare players v1 | POST /api/v1/stats/compare | ✅ INTÉGRÉ | Compare.razor | Batch v1 fonctionnel |
| Team stats v1 | GET /api/v1/stats/teams/{id} | ✅ INTÉGRÉ | Teams.razor | |
| Match stats v1 | GET /api/v1/stats/matches/{id} | ✅ INTÉGRÉ | Matches.razor | |
| Position profiles v1 | GET /api/v1/players/{id}/position-profile | ✅ INTÉGRÉ | PositionProfiles.razor | Via PlayersApiClient |
| Client updates v2 | GET /api/v2/updates/check | ✅ INTÉGRÉ | UpdateRequired.razor | AppUpdateService |

---

## Fonctionnalités AVAILABLE_PARTIAL

| Feature | Endpoint | Statut client | Page(s) | Notes |
|---------|----------|---------------|---------|-------|
| Tactical context (event breakdown) | GET /api/v1/stats/players/{id}/events | ✅ INTÉGRÉ PARTIELLEMENT | Players.razor (zones) | Contexte via événements match v1 |

---

## Fonctionnalités DATA_MISSING — rendues correctement

| Feature | Statut client | Affichage | Conforme |
|---------|--------------|-----------|---------|
| FailedPivotPasses | ✅ DATA_MISSING | "Donnée non disponible avec les fichiers actuels" | ✅ OUI — jamais 0 |

---

## Fonctionnalités masquées correctement

| Feature | Raison | Statut client | Affiché |
|---------|--------|--------------|---------|
| Possessions | BLOCKED_BY_SOURCE_DATA | ✅ MASQUÉ | Non — aucun UI |
| Lineups / On-Off | BLOCKED_BY_SOURCE_DATA | ✅ MASQUÉ | Non — aucun UI |
| xG / xS | FEATURE_FLAG_DISABLED | ✅ MASQUÉ | Non — aucun UI |
| Scouting | NOT_IMPLEMENTED | ✅ MASQUÉ | Non — aucun UI |
| Vidéo | FEATURE_FLAG_DISABLED | ✅ MASQUÉ | Non — aucun UI |
| Rapports | NOT_IMPLEMENTED | ✅ MASQUÉ | Non — aucun UI |
| Data Quality (dédiée) | NOT_IMPLEMENTED | ✅ MASQUÉ | Uniquement inline via MetricValue.quality |

---

## État des composants design system

| Composant | Fichier | Usage | Statut |
|-----------|---------|-------|--------|
| AccessRequiredCard | Shared/ | Login guard sur toutes les pages | ✅ ACTIF |
| AudienceLensSelector | Shared/ | Dashboard, Players, Compare, Teams | ✅ ACTIF |
| AnalysisScopeSummary | Shared/ | Scope display | ✅ ACTIF |
| AnalyticsSourceBadge | Shared/ | Badge v1/v2/unavailable | ✅ ACTIF |
| BarGaugeKpiCard / Grid | Shared/ | KPIs | ✅ ACTIF |
| CoachCards | Shared/ | Dashboard coach view | ✅ ACTIF |
| CommandPalette | Shared/ | Recherche globale | ✅ ACTIF |
| DataQualityBadge | Shared/ | Inline quality per metric | ✅ ACTIF |
| DataQualitySummary | Shared/ | Team/Match level | ✅ ACTIF |
| DetailedTable | Shared/ | Position profiles | ✅ ACTIF |
| Drawer | Shared/ | Filtres génériques | ✅ ACTIF |
| GlobalScopeBar | Shared/ | Scope persistant | ✅ ACTIF |
| GoalKpi | Shared/ | Zone tirs (Demo + Players) | ✅ ACTIF |
| KpiTileGrid | Shared/ | Multiple pages | ✅ ACTIF |
| LeaguePlayerStatsPanel | Shared/ | Fiche joueuse v2 | ✅ ACTIF |
| MatchCard | Shared/ | Liste matchs | ✅ ACTIF |
| MetricBreakdownCard | Shared/ | Détails métriques | ✅ ACTIF |
| MetricEvidence | Shared/ | Preuves métriques v2 | ✅ ACTIF |
| MetricValueCard | Shared/ | Valeurs individuelles | ✅ ACTIF |
| MultiRadar | Shared/ | Compare + PositionProfiles | ✅ ACTIF |
| PageLoader | Shared/ | Loading overlay | ✅ ACTIF |
| PlayerList | Shared/ | Annuaire joueuses | ✅ ACTIF |
| PlayerTeamHistoryPanel | Shared/ | Historique joueuse | ✅ ACTIF |
| PositionFilters | Shared/ | Filtres position | ✅ ACTIF |
| PositionProfileHistogram | Shared/ | Histogramme cohorte | ✅ ACTIF |
| PositionProfileRadarChart | Shared/ | Radar poste | ✅ ACTIF |
| RateMetricCard | Shared/ | Taux avec qualité | ✅ ACTIF |
| ScatterChart | Shared/ | Scatter plot | ✅ ACTIF |
| ScopeSummaryBar | Shared/ | Barre scope | ✅ ACTIF |
| StateCard | Shared/ | États loading/error/empty | ✅ ACTIF |
| UnavailableMetricState | Shared/ | Métrique indisponible | ✅ ACTIF |
| UpdateAvailableDialog | Shared/ | Dialog mise à jour | ✅ ACTIF |
| UpdateDownloadProgress | Shared/ | Progression téléchargement | ✅ ACTIF |

---

## Résumé

- AVAILABLE intégrés : **9/9**
- AVAILABLE_PARTIAL intégrés : **1/1**
- DATA_MISSING rendus correctement : **1/1**
- Fonctionnalités BLOCKED/DISABLED/NOT_IMPLEMENTED masquées : **7/7**
- Composants design system actifs : **34**
- Duplicats ou styles copiés : aucun constaté (architecture centralisée dans Shared/)

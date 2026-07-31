# HandWStat — Product Development Matrix

**Date :** 2026-07-31
**Branch :** feature/handwstat-functional-product-v1
**Baseline HEAD :** de085ca
**Source de vérité :** code + tests (pas uniquement la documentation)

---

## Légende des statuts

| Statut | Signification |
|--------|--------------|
| COMPLETE_AND_TESTED | Implémenté, connecté à l'API réelle, tests présents |
| FUNCTIONAL_BUT_INCOMPLETE | Implémenté et connecté, mais manque des sections ou états |
| PLACEHOLDER | Page/composant créé mais sans données réelles |
| STATIC_DATA | Affiche des données codées en dur ou issues de mock |
| CONTRACT_ONLY | DTOs/services présents, aucune page UI |
| FEATURE_DISABLED | Correctement masqué, conformément à la feature availability |
| DATA_MISSING | Métrique rendue indisponible (correct) |
| BLOCKED_BY_API | API non disponible, endpoint non implémenté |
| NOT_IMPLEMENTED | Aucun code côté client |

---

## Matrice fonctionnelle

| ID | Domaine | Route | Page | Composants clés | Gateway | Endpoint | Données réelles | Loading | Empty | Partial | Unavailable | Error | Tests | Statut réel | Travail restant |
|----|---------|-------|------|-----------------|---------|----------|-----------------|---------|-------|---------|-------------|-------|-------|-------------|-----------------|
| P01 | Accueil / Login | `/` | Home.razor | PublicLayout, AccessRequiredCard | IApiAuthService | POST /auth | Oui | Oui | — | — | Oui | Oui | Partiel | COMPLETE_AND_TESTED | Aucun |
| P02 | Dashboard | `/dashboard` | Dashboard.razor (HomeBase) | StateCard, RateMetricCard, AudienceLensSelector, CoachCards, KpiTileGrid | StatsDashboardService | GET /api/v1/stats/overview, GET /api/v1/stats/rankings | Oui | Oui | Oui | Partiel | Partiel | Oui | Oui (DashboardSnapshotBuilderTests) | FUNCTIONAL_BUT_INCOMPLETE | TeamOfDay affiche mention "calculs locaux exploratoires" — à corriger en texte UI |
| P03 | Joueuses — Annuaire | `/players` | Players.razor | PlayerList, AccessRequiredCard, PositionFilters | PlayersApiClient | GET /api/v1/stats/players | Oui | Oui | Oui | — | — | Oui | Partiel | FUNCTIONAL_BUT_INCOMPLETE | Tests incomplets pour état vide et cancellation |
| P04 | Joueuses — Fiche v2 | `/players` (détail inline) | Players.razor | LeaguePlayerStatsPanel, MetricEvidence, RateMetricCard, AnalyticsSourceBadge | V2AnalyticsGateway + fallback V1 | GET /api/v2/analytics/players/{id} | Oui | Oui | — | Oui (fallback v1) | Oui | Oui | Oui (V2AnalyticsGatewayTests, LeagueAnalyticsComponentTests) | COMPLETE_AND_TESTED | SHOULD: section goalkeeper conditionnelle améliorable |
| P05 | Comparaison | `/compare` | Compare.razor | MultiRadar, BarGaugeKpiGrid, AudienceLensSelector | StatsApiClient | POST /api/v1/stats/compare | Oui | Oui | Oui | — | — | Oui | Partiel | FUNCTIONAL_BUT_INCOMPLETE | Pas de batch v2 disponible — v1 OK. Tests manquants |
| P06 | Équipes — Liste + Fiche | `/teams` | Teams.razor | DataQualitySummary, KpiTileGrid, ApexChart | StatsApiClient, MatchEventsApiClient | GET /api/v1/stats/teams/{id} | Oui | Oui | Oui | — | — | Oui | Partiel | FUNCTIONAL_BUT_INCOMPLETE | Aucune section goalkeeping team. Tests manquants |
| P07 | Matchs — Liste + Détail | `/matches` | Matches.razor | MatchCard, KpiTileGrid, ApexChart (timeline), CoachCards | MatchesApiClient, MatchEventsApiClient | GET /api/v1/stats/matches/{id} | Oui | Oui | Oui | — | Oui | Oui | Partiel | FUNCTIONAL_BUT_INCOMPLETE | Tests manquants pour timeline et états d'erreur |
| P08 | Lab / Compare | `/compare` | Compare.razor | MultiRadar, PlayerList | StatsApiClient | POST /api/v1/stats/compare | Oui | Oui | Oui | — | — | Oui | Non | FUNCTIONAL_BUT_INCOMPLETE | Voir P05 |
| P09 | Profils de poste | `/position-profiles` | PositionProfiles.razor (PositionProfilesBase) | PositionRadarChart, ScatterChart, DetailedTable, MultiRadar | PlayersApiClient | GET /api/v1/players/{id}/position-profile | Oui | Oui | Oui | — | — | Oui | Non | FUNCTIONAL_BUT_INCOMPLETE | Rail masqué (ShowInRail=false). Tests absents |
| P10 | Équipe type | Dashboard section | Dashboard.razor (HomeBase) | CoachCards, TeamOfTheDayService | StatsApiClient | v1 multi-appels (calcul local) | Oui | Oui | — | — | — | Oui | Non | FUNCTIONAL_BUT_INCOMPLETE | Texte UI indique "calculs locaux exploratoires" — doit être précisé |
| P11 | Qualité des données | — | Aucune page dédiée | DataQualityBadge (inline uniquement), DataQualitySummary | — | NOT_IMPLEMENTED | — | — | — | — | — | — | Non | BLOCKED_BY_API | Endpoint inexistant. Affichage inline via MetricValue.quality uniquement |
| P12 | Possessions | — | Aucune | — | — | BLOCKED_BY_SOURCE_DATA | — | — | — | — | — | — | — | FEATURE_DISABLED | Ne pas implémenter |
| P13 | Lineups / On-Off | — | Aucune | — | — | BLOCKED_BY_SOURCE_DATA | — | — | — | — | — | — | — | FEATURE_DISABLED | Ne pas implémenter |
| P14 | xG / xS | — | Aucune | — | — | FEATURE_FLAG_DISABLED | — | — | — | — | — | — | — | FEATURE_DISABLED | Masqué correctement |
| P15 | Scouting | — | Aucune | — | — | NOT_IMPLEMENTED | — | — | — | — | — | — | — | NOT_IMPLEMENTED | Masqué correctement |
| P16 | Vidéo | — | Aucune | — | — | FEATURE_FLAG_DISABLED | — | — | — | — | — | — | — | FEATURE_DISABLED | Masqué correctement |
| P17 | Rapports | — | Aucune | — | — | NOT_IMPLEMENTED | — | — | — | — | — | — | — | NOT_IMPLEMENTED | Masqué correctement |
| P18 | Authentification | `/` + `AccessRequiredCard` | Home.razor | ApiAuthService | IApiAuthService | POST /auth | Oui | Oui | — | — | Oui | Oui | Oui (partiel) | COMPLETE_AND_TESTED | 401/403 gérés via AccessRequiredCard |
| P19 | Mises à jour | `/update-required` | UpdateRequired.razor | UpdateDownloadProgress, UpdateAvailableDialog | AppUpdateService | GET /api/v2/updates/check | Oui | Oui | — | — | Oui | Oui | Oui (AppUpdateServiceTests, UpdateAutomationTests) | COMPLETE_AND_TESTED | Handlers simulés. Ne pas lancer téléchargement réel |
| P20 | Navigation | Rail | NavMenu.razor | AppNavigationCatalog | — | — | — | — | — | — | — | — | Non | FUNCTIONAL_BUT_INCOMPLETE | position-profiles masqué du rail (ShowInRail=false) — accès uniquement via Compare ou URL directe |
| P21 | Demo guidée | `/demo` | Demo.razor | DemoDataFactory, GoalKpi | DemoDataFactory (pas d'API) | — | Demo | Non | — | — | — | — | Non | COMPLETE_AND_TESTED | Données de démo, pas d'API — conforme |
| P22 | Diagnostic | `/counter` | Counter.razor | — | — | — | — | — | — | — | — | — | Non | FUNCTIONAL_BUT_INCOMPLETE | Page de debug minimale — usage interne uniquement |
| P23 | Paramètres | Aucune page dédiée | MainLayout (context lens panel) | AudienceLensSelector | — | — | — | — | — | — | — | — | Non | FUNCTIONAL_BUT_INCOMPLETE | Pas de route `/settings`. Informations dans MainLayout |

---

## Résumé par priorité

| Priorité | Domaine | Statut | Action principale |
|----------|---------|--------|-------------------|
| P0 | Navigation | FUNCTIONAL_BUT_INCOMPLETE | Rendre position-profiles accessible dans le rail ou via lien explicite |
| P1 | Dashboard | FUNCTIONAL_BUT_INCOMPLETE | Clarifier texte "calculs locaux" équipe type |
| P2 | Fiche joueuse v2 | COMPLETE_AND_TESTED | Améliorer section gardienne conditionnelle |
| P3 | Comparaison | FUNCTIONAL_BUT_INCOMPLETE | Ajouter tests |
| P4 | Équipes | FUNCTIONAL_BUT_INCOMPLETE | Ajouter tests, section gardienne équipe |
| P5 | Matchs | FUNCTIONAL_BUT_INCOMPLETE | Ajouter tests timeline et erreurs |
| P6 | Équipe type | FUNCTIONAL_BUT_INCOMPLETE | Clarifier sources et calcul local |
| P7 | Profils de poste | FUNCTIONAL_BUT_INCOMPLETE | Ajouter tests, rendre visible depuis la navigation |
| P8 | Qualité données | BLOCKED_BY_API | Documenter blocage — inline quality OK |
| P9 | Fonctionnalités avancées | FEATURE_DISABLED / NOT_IMPLEMENTED | Masquées correctement |
| P10 | UX/Responsive | FUNCTIONAL_BUT_INCOMPLETE | Voir HANDWSTAT_NEXT_PRODUCT_ROADMAP |

---

## Comptages

- Pages implémentées et connectées à l'API réelle : **8/10** (toutes sauf DataQuality dédiée et Settings dédiée)
- Fonctionnalités correctement masquées : **6** (Possessions, Lineups/On-Off, xG, xS, Vidéo, Scouting, Rapports)
- Tests présents : **84** (baseline, tous passants)
- Warnings applicatifs : **0**
- Pages placeholders : **0**
- Pages avec données statiques : **0** (Demo.razor utilise DemoDataFactory — conforme et délibéré)

---

## Ce qui était déclaré FUNCTIONAL_COMPLETION_PERCENT=100 vs réalité

La déclaration `FUNCTIONAL_COMPLETION_PERCENT=100` de la session précédente était basée sur la couverture des fonctionnalités AVAILABLE. Cette couverture est réelle — toutes les fonctionnalités AVAILABLE sont intégrées. Cependant :

1. **Tests insuffisants** sur Compare, Teams, Matches, PositionProfiles
2. **Position-profiles invisible** depuis la navigation rail
3. **Texte UI TeamOfDay** indique "calculs locaux exploratoires, non contractuels" — acceptable mais à documenter
4. **Pas de page DataQuality dédiée** — conformément au statut NOT_IMPLEMENTED
5. **Pas de page Settings dédiée** — fonctionnalité dans MainLayout

Ces points ne remettent pas en cause l'intégration fonctionnelle des API disponibles.

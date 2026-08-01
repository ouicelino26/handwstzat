# HandWStat — Audit Produit Final

**Date :** 2026-08-01  
**Branche :** feature/handwstat-functional-product-v1  
**Commit HEAD :** f526269 (+ correctifs post-audit)  
**Auditeur :** Claude Code  

---

## 1. Résumé exécutif

HandWStat est une application d'analyse handball construite en MAUI Blazor (.NET 10). Elle cible les analystes, entraîneurs et joueuses du handball féminin professionnel (Starligue/Division Supérieure).

L'audit couvre : architecture, statistiques, KPI, radars, fiches joueuses, tirs spatiaux, tableaux/graphiques, UI/UX, navigation, responsive, accessibilité, performances, qualité des données et cohérence des contrats.

### Score global d'audit (100 points pondérés)

| Domaine | Poids | Score | Points |
|---------|-------|-------|--------|
| Statistiques et KPI | 25 | 92/100 | 23.0 |
| Architecture et contrats | 20 | 96/100 | 19.2 |
| UI/UX et navigation | 20 | 78/100 | 15.6 |
| Fiabilité et tests | 15 | 94/100 | 14.1 |
| Performances | 10 | 72/100 | 7.2 |
| Accessibilité et responsive | 10 | 55/100 | 5.5 |
| **Total** | **100** | | **84.6/100** |

**PRODUCT_READINESS = 84.6%**  
**READY_FOR_RELEASE = NO** (seuil requis : 90% — voir section blockers)

---

## 2. Périmètre de l'audit

### Ce qui a été audité

- `Models/Analytics/KpiModels.cs` — HandballKpiHelper (toutes les fonctions statiques)
- `Models/Analytics/MetricDisplayModels.cs` — RateDisplayModel
- `Models/Analytics/PositionProfilesViewModels.cs` — PositionProfileAxisViewModel, NormalizeRadarValue
- `Models/Analytics/TeamOfTheDayModels.cs` — TeamOfTheDayPieScoring
- `Models/Analytics/SpatialZoneVisuals.cs` — zones tirs, miroir, heatmap
- `Services/Analytics/LeaguePlayerAnalyticsService.cs` — gateway v2/v1, mapping
- `Services/Analytics/V2AnalyticsGateway.cs` — HTTP → outcome
- `Services/TeamOfTheDayService.cs` — BuildStatLine, PIE
- `Components/Shared/` — MultiRadar, PositionRadarChart, ScatterChart, PositionProfileHistogram, GoalKpi
- `docs/statistics/` — contrats métriques, mapping UI
- Suite de tests xUnit (232 tests)

### Ce qui n'a pas été audité (hors périmètre)

- Composants Android/iOS/Mac Catalyst (build bloqué en local)
- Backend HandballManagerAPI (lecture seule autorisée)
- HandballManagerCore (lecture seule autorisée)

---

## 3. Défauts identifiés et corrigés

### P0 — TotalSanctions inclut PenaltyConcede (CORRIGÉ)

| Champ | Valeur |
|-------|--------|
| **Fichier** | `Models/Analytics/KpiModels.cs:73` |
| **Méthode** | `HandballKpiHelper.TotalSanctions()` |
| **Description** | `PenaltyConcede` (7m concédés) était additionné au total disciplinaire |
| **Contrat violé** | HANDWSTAT_METRIC_DISPLAY_CONTRACT.md + LEAGUE_STATS_UI_MAPPING.md |
| **Impact** | TechnicalBalanceScore gonflé, TeamOfTheDay PIE biaisé pour les gardiennes, spotlights Home.razor incorrects |
| **Correction** | Suppression de `+ sanctions.PenaltyConcede` |
| **Tests ajoutés** | `TotalSanctions_ExcludesPenaltyConcede_ContractInvariant`, `TechnicalBalanceScore_SanctionsExcludePenaltyConcede` |

### P1 — Label "Above median" anglais dans UI française (CORRIGÉ)

| Champ | Valeur |
|-------|--------|
| **Fichiers** | `MultiRadar.razor:605`, `PositionRadarChart.razor:481`, `ScatterChart.razor:519`, `PositionProfileHistogram.razor:329` |
| **Description** | `GetAnnotationText()` retournait "Above median" pour les percentiles intermédiaires |
| **Correction** | Remplacement par "Au-dessus de la médiane" dans les 4 composants |

---

## 4. Réponses aux 20 questions d'audit

### Q1 — Couverture produit pour un analyste handball

La couverture est **fonctionnellement complète** pour un analyste opérationnel :
- Dashboard quotidien avec classement, aperçu stats, équipe type
- Fiche joueuse complète : offense, défense, gardienne, passes, sanctions, spatial zones
- Profil de poste : radar, scatter, histogramme cohorte, tableau détaillé
- Comparaison multi-joueuses (2-6) sur 14+ métriques
- Fiche équipe, fiche match

Les fonctionnalités manquantes (xG, possessions, lineups, scouting, vidéo) sont correctement masquées et documentées dans `HANDWSTAT_API_BLOCKERS.md`.

### Q2 — Exactitude statistique

Les formules vérifiées sont correctes. La seule violation de contrat trouvée était P0 (corrigée). Points validés :
- `ShotAttempts = Goals + TirsRates + PenaltyRate` — TirContre non dupliqué ✓
- `ShotsFaced = Arrets + ButsPris` (v1) / `TirsSubis` (v2) — hors cadre exclus ✓
- `FailedPivotPasses` → toujours DATA_MISSING ✓
- `TotalSanctions = Avertissements + DeuxMinutes + Exclusions` (sans 7m) ✓

### Q3 — Validité des KPI

`HandballKpiHelper` est une classe statique pure. Chaque KPI est calculé localement à partir des atoms de l'API. Les seuils de tone (`HigherIsBetterTone`, `LowerIsBetterTone`) sont cohérents avec les niveaux de jeu professionnels féminins. Voir `HANDWSTAT_CUSTOM_KPI_AUDIT.md` pour le détail.

### Q4 — Normalisation radar

`NormalizeRadarValue` utilise la plage [MinValue, MaxValue] de la cohorte. Les axes `HigherIsBetter=false` (ex: pertes, sanctions) sont inversés via `100d - normalized`. Fallback sur `DirectionalPercentile` si la plage est invalide. Correct.

### Q5 — Profils gardienne

Le système PIE distingue correctement les gardiennes des joueuses de champ via `isGoalkeeper`. Le poids global est 72% défense / 28% offense pour les gardiennes (vs 58/42 pour les joueuses). `shotsFaced` vient en priorité de `goalkeeper.TirsSubis`.

### Q6 — Système de coordonnées tirs

HandWStat n'utilise **pas** de coordonnées X/Y brutes. Le système est exclusivement par codes de zones (BG1-BD12 pour le cadre, TG1-TD9 pour les déclencheurs). La logique miroir (`ToVisualTriggerKey`) est correcte. Voir `HANDWSTAT_SHOT_COORDINATE_SYSTEM.md`.

### Q7 — Dénominateurs zones

Les dénominateurs de zones sont les `Attempts` du contrat `ZoneStat`. Jamais recalculés côté client. Correct.

### Q8 — Tirs bloqués vs comptés

`ShotAttempts` = Goals + TirsRates + PenaltyRate. `TirContre` (tirs contrés avant gardienne) n'est PAS ajouté séparément — il n'entre pas dans le total des tentatives. Ce comportement est couvert par le test `ShotAttempts_DoesNotCountBlockedShotTwice`.

### Q9 — Utilisabilité UI

Points forts : filter-bar cohérente, KpiTileGrid lisible, StateCard gère loading/error/empty. Points faibles : position-profiles inaccessible depuis le rail mobile (NAV-01), pas de page `/settings` dédiée (NAV-02).

### Q10 — Responsive

L'application fonctionne en mode Windows desktop. La cible mobile est Android (bloquée en local). Les composants graphiques (radar, scatter, histogramme) n'ont pas de breakpoints responsives dédiés — ils utilisent la taille de la fenêtre MAUI. Voir `HANDWSTAT_UI_UX_AUDIT.md`.

### Q11 — Accessibilité

Partielle. ARIA labels présents sur les éléments interactifs principaux. Absence de rôles ARIA sur les graphiques SVG complexes (zones tirs, radars). Contraste : non vérifié programmatiquement (nécessite test visuel). Voir `HANDWSTAT_UI_UX_AUDIT.md`.

### Q12 — Loading/error states

`StateCard` est utilisé sur toutes les pages pour les états loading, error, empty. `UnavailableMetricState` gère les métriques individuelles non disponibles. `PageLoader` existe mais son usage doit être vérifié page par page. Correct dans l'ensemble.

### Q13 — Signalisation taille d'échantillon

`RateDisplayModel.SampleReliable` et `QualityLabel` existent. `DataQualityBadge` affiche un indicateur inline. Le seuil minimum de fiabilité est configurable par métrique. La signalisation est présente mais pas systématiquement appliquée à toutes les métriques avancées.

### Q14 — Déclaration finale honnête

**PRODUCT_READINESS = 84.6%** — application fonctionnellement complète pour un analyste avec les données disponibles. Deux défauts corrigés (P0 + P1). Blockers restants : accessibilité partielle, responsive mobile non testé, ~28 appels API au chargement du dashboard.

### Q15 — Contrats API v1/v2

Le gateway v2 avec fallback v1 sur HTTP 503 uniquement est correctement implémenté. `AnalyticsSourceStatus` est propagé à l'UI via `AnalyticsSourceBadge`. Les contrats sont documentés dans `HANDWSTAT_FEATURE_AVAILABILITY.md`.

### Q16 — TeamOfTheDay PIE scoring

Calcul local (non serveur). Poids défensifs/offensifs distincts par type de joueuse. Tie-breaking par PlayingTimeMinutes. `LOCAL_EXPLORATORY_CALCULATION` — non adapté à un classement officiel. Correct pour un usage interne.

### Q17 — ETag / cache

`ApiClientBase.GetConditionalAsync<T>()` avec `ConcurrentDictionary` d'ETags. Sur HTTP 304 → `LeagueGatewayResult.Success(null)` → données cachées réutilisées. Correct.

### Q18 — Qualité des données

`DataQualityBadge` inline par métrique. Pas d'endpoint `/data-quality` dédié (API-BLOCK-01). Score global non calculable côté client. Comportement correct : pas d'invention de score.

### Q19 — Performance dashboard

~28 appels API au chargement du Dashboard. `StatsDashboardService` agrège plusieurs endpoints v1. Sans multiplexing ni endpoint batch dédié, la latence perçue est proportionnelle aux appels. ETag réduit la bande passante mais pas le nombre de requêtes.

### Q20 — Tests

232 tests, 0 échecs, 0 skips après les correctifs. Couverture : HandballKpiHelper, RateDisplayModel, TeamOfTheDayModels, LeagueAnalyticsService, V2AnalyticsGateway, ApiClientBase, StatsApiClient, Matches/Players/Teams clients, PositionProfiles, DashboardSnapshot, AppUpdateService, UpdateAutomation.

---

## 5. Checklist de release

| Gate | Statut |
|------|--------|
| HANDWSTAT_TESTS | ✅ 232/232 PASS |
| FAILED_TESTS | ✅ 0 |
| APPLICATION_WARNINGS | ✅ 0 |
| WINDOWS_DEBUG_BUILD | ✅ PASS |
| P0_DEFECTS_FIXED | ✅ TotalSanctions corrigé |
| P1_DEFECTS_FIXED | ✅ "Above median" → "Au-dessus de la médiane" |
| ANDROID_BUILD | ⛔ BLOCKED (env local) |
| ACCESSIBILITY_FULL | ⚠️ PARTIAL |
| RESPONSIVE_MOBILE | ⚠️ PARTIAL |
| DASHBOARD_API_CALLS | ⚠️ ~28 calls (non bloquant) |
| DATA_QUALITY_ENDPOINT | ⛔ BLOCKED_BY_API |
| xG_xS | ⛔ FEATURE_FLAG_DISABLED |
| POSSESSIONS | ⛔ BLOCKED_BY_SOURCE_DATA |
| LINEUPS_ONOFF | ⛔ BLOCKED_BY_SOURCE_DATA |

**READY_FOR_RELEASE = NO** — les gates Android, accessibilité complète et responsive mobile doivent être validés avant release.

---

## 6. Recommandations prioritaires post-audit

| Priorité | Action | Impact |
|----------|--------|--------|
| P1 | Ajouter lien position-profiles dans rail mobile (NAV-01) | Découvrabilité analyste mobile |
| P1 | Réduire les appels API dashboard (batch ou parallelism) | UX performance |
| P2 | Ajouter ARIA roles sur les graphiques SVG (radars, zones tirs) | Accessibilité |
| P2 | Valider contraste WCAG AA sur les palettes de chaleur | Accessibilité |
| P3 | Implémenter pagination/virtualisation sur les listes longues | Performance mobile |
| P3 | Ajouter tests d'intégration pour le pipeline de normalisation radar end-to-end | Fiabilité |

# HANDWSTAT — MIGRATION MATRIX MOCKUP → APPLICATION

STATUS: M0_VALIDATED
UI_SOURCE_OF_TRUTH: handwstat-mockup (C:\Users\donovan.bierque\Downloads\handwstat-mockup-20260823T130252Z-1-001\handwstat-mockup)
FUNCTIONAL_SOURCE_OF_TRUTH: HandWStat (C:\Users\donovan.bierque\source\repos\ouicelino26\handwstzat)

Date baseline : 2026-08-23
Branche baseline : fix/player-position-badges-v3
HEAD baseline : d9c371c

---

## BASELINE VALIDÉE

| Indicateur | Valeur |
|---|---|
| `dotnet restore` | SUCCESS |
| `dotnet build` | SUCCESS (0 erreur, 0 warning) |
| `dotnet test` | **591/591 PASS** — 0 fail, 0 skip |
| Disque C: | ~1.8 Go libres (surveiller) |

---

## DÉCOUVERTE CRITIQUE — TOKENS CSS DÉJÀ ALIGNÉS

L'`app.css` HandWStat est identique au mockup sur les tokens CSS.
Aucune migration de tokens à faire from scratch pour M1.

Les seules divergences sont dans `Services/Shared/ChartPalette.cs` (voir section D ci-dessous).

---

## MAPPING PAGES

| Mockup | Route HandWStat | Composants | Match |
|---|---|---|---|
| `index.html` | `/` | `Home.razor` + `PublicLayout.razor` | COMPLET |
| `dashboard.html` | `/dashboard` | `Dashboard.razor` + `HomeBase` | COMPLET |
| `players.html` | `/players` | `Players.razor` | COMPLET |
| `teams.html` | `/teams` | `Teams.razor` | COMPLET |
| `matches.html` | `/matches` | `Matches.razor` | COMPLET |
| `compare.html` | `/compare` | `Compare.razor` | COMPLET |
| `position-profiles.html` | `/position-profiles` | `PositionProfiles.razor` + `PositionProfilesBase` | COMPLET |
| `export.html` | `/export` | `Export.razor` | COMPLET |
| — | `/demo` | `Demo.razor` | HANDWSTAT_ONLY |
| — | `/update-required` | `UpdateRequired.razor` | HANDWSTAT_ONLY |
| — | `/not-found` | `NotFound.razor` | HANDWSTAT_ONLY |

---

## AUDIT ChartPalette.cs — DIVERGENCES CONFIRMÉES (M1)

| Constante | HandWStat actuel | Mockup cible | Usages | SAFE |
|---|---|---|---|---|
| `Player` | `#ff5b2e` | `#8fe1b2` | Rankings bar, player series | YES |
| `Reference` | `#8fa39a` | `#73a7ff` | Median series, reference lines | YES |
| `Primary` | `#177a5b` | `#48bb7b` | Team charts primary series | YES |
| `Warning` | `#e6ab4a` | `#efc468` | Warning tone charts | YES |
| `Danger` | `#e95c57` | `#ff7770` | Danger tone charts | YES |

---

## MATRICE MIGRATION PAR PAGE

### Login — `index.html` → `Home.razor` + `PublicLayout.razor`

| Section | Composant cible | Source données | Action | Status |
|---|---|---|---|---|
| `.auth-layout` (2-col) | `PublicLayout.razor` | N/A | RESTYLE | TODO |
| `.auth-manifesto` | `Home.razor` | Statique | RESTYLE | TODO |
| `.auth-console` (form) | `Home.razor` | `IApiAuthService.LoginAsync()` | RESTYLE | TODO |

### Dashboard — `dashboard.html` → `Dashboard.razor` (HomeBase)

| Section | Composant cible | Source données | Action | Status |
|---|---|---|---|---|
| KPI headline row (4 tiles) | `KpiTileGrid.razor` | `HomeBase.DashboardHeadlineMetrics` | RESTYLE | TODO |
| Ranking bar chart | `ApexChart` | `DashboardSnapshot.TopScorers` | RESTYLE couleurs | TODO |
| Signal matrix 5-col | Inline Dashboard | `HomeBase.LeagueKpis` | CREATE `.signal-matrix` | TODO |
| Signal items (bords colorés) | Inline Dashboard | `LeagueKpis` | RESTYLE | TODO |
| Terrain handball (Équipe du jour) | Dashboard.razor | `TeamOfTheDaySnapshotDto` | CREATE `.team-day-player-card` | TODO |
| Spotlight joueuse | Dashboard.razor | `DashboardSnapshot.Spotlight` | RESTYLE | TODO |
| Liste matchs | `MatchCard.razor` | `DashboardSnapshot.RecentMatches` | RESTYLE | TODO |
| Global rankings table | Lazy PlayerTable | `GlobalBoards` | RESTYLE `.analytics-table` | TODO |

### Players — `players.html` → `Players.razor`

| Section | Composant cible | Source données | Action | Status |
|---|---|---|---|---|
| Split pane (22rem + flex) | Layout CSS | — | RESTYLE | TODO |
| Liste joueuses | `PlayerList.razor` | `PlayersApiClient.GetPlayersAsync()` | RESTYLE `.player-row` | TODO |
| Profile header (3-col) | Players.razor | `PlayerProfileDto` | RESTYLE `.player-profile` | TODO |
| **Brief — KPI tiles** | `BarGaugeKpiGrid.razor` | `StatsApiClient.Get*Async()` | **REFACTOR** → `.kpi-tile-grid` | TODO |
| **Brief — sections 3 variantes** | `BarGaugeKpiCard.razor` | Mêmes données | **REFACTOR** → `.brief-section` | TODO |
| Performance | `PerformanceMetricRow.razor` | `LeaguePlayerAnalyticsService` | RESTYLE | TODO |
| Evolution chart | `ApexChart` | `GetPlayerTrajectoryAsync()` | RESTYLE couleurs | TODO |
| Court map | `GoalKpi.razor` | `GetPlayerSpatialAsync()` | RESTYLE CSS | TODO |
| Matchs tab | Inline Players | `GetMatchesAsync(playerId)` | RESTYLE `.result-badge--v/n/d` | TODO |

### Teams — `teams.html` → `Teams.razor`

| Section | Composant cible | Source données | Action | Status |
|---|---|---|---|---|
| Team profile header | Inline Teams | `GetTeamStatsAsync()` | RESTYLE `.team-profile` | TODO |
| KPI tiles (14) | `KpiTileGrid.razor` | `BuildTeamKpis()` | RESTYLE `.kpi-tile-grid` | TODO |
| Donut + bar charts | `ApexChart` | `GetTeamProfileAsync()` | RESTYLE couleurs | TODO |
| **Team performer cards** | Inline Teams | `PlayerGlobalStatsDto[]` | **CREATE** `.team-performer-card` | TODO |
| Roster heat-tone | Inline Teams | `TableHeatToneHelper` | RESTYLE `.heat-tone-*` | TODO |
| Match results | `MatchCard.razor` | `GetMatchesAsync(teamId)` | RESTYLE `.result-card` | TODO |

### Matches — `matches.html` → `Matches.razor`

| Section | Composant cible | Source données | Action | Status |
|---|---|---|---|---|
| Split pane (22rem + 1fr) | Layout CSS | — | RESTYLE `.matches-shell` | TODO |
| Liste matchs | Inline Matches | `GetMatchesAsync()` | RESTYLE `.match-list-item` | TODO |
| Scoreboard 3-col | Inline Matches | `MatchSummaryDto` | RESTYLE `.match-scoreboard` | TODO |
| **Score timeline markers** | Inline Matches | `MatchEventAnalyticsDto[]` | **CREATE** `.score-timeline` | TODO |
| Chronologie chart | `ApexChart` stepline | `SelectedMatchTimeline` | RESTYLE | TODO |
| Court tab | `GoalKpi.razor` ErrorBoundary | `MatchSpatial` | KEEP_FUNCTIONAL | TODO |
| Players tab | Inline Matches | `MatchPlayers` | RESTYLE heat-tone | TODO |

### Compare — `compare.html` → `Compare.razor`

| Section | Composant cible | Source données | Action | Status |
|---|---|---|---|---|
| Player slots bar | Inline Compare | `ComparePlayersResponseDto.Players` | **CREATE** `.cmp-bar` + dots | TODO |
| **Key Differentiators** | Inline Compare | Compare response | **CREATE** `.cmp-diff-item` + `.cmp-diff-row` | TODO |
| Radar 6 axes | `ApexChart` radar | `RadarMetrics` | RESTYLE layout | TODO |
| Evidence table | Inline Compare | `TableHeatToneHelper` | RESTYLE `.cmp-evidence-group` | TODO |

### Position Profiles — `position-profiles.html` → `PositionProfiles.razor`

| Section | Composant cible | Source données | Action | Status |
|---|---|---|---|---|
| Narrative cards | `CoachCards.razor` | `PositionProfileInsightEngine.Build()` | RESTYLE `.narrative-card` | TODO |
| KPI tiles 6-col + médiane | `KpiTileGrid.razor` | `PositionProfileAxisViewModel[]` | RESTYLE `.kpi-tile-grid--6` | TODO |
| Histogram | `PositionProfileHistogram.razor` | Axes data | RESTYLE (ApexCharts paired bars) | TODO |
| Radar | `PositionRadarChart.razor` | Axes data | RESTYLE | TODO |
| Shortlist 3 slots | `MultiRadar.razor` | `PositionProfileCompareSelections` | RESTYLE | TODO |
| Scatter | `ScatterChart.razor` | `ScatterBounds` | RESTYLE | TODO |
| Detailed table | `DetailedTable.razor` | Axes + tones | RESTYLE | TODO |

### Export — `export.html` → `Export.razor`

| Section | Composant cible | Source données | Action | Status |
|---|---|---|---|---|
| Quick export | Inline Export | `ExportRequestBuilder` | RESTYLE `.exp-quick` | TODO |
| **Wizard 3 steps** | Inline Export | `ExportGenerationStatus` | **CREATE** `.exp-stepper` | TODO |
| Target radio rows | Inline Export | `ExportTarget` enum | RESTYLE `.exp-option-row` | TODO |
| Summary aside | Inline Export | `ExportMetaDto` | RESTYLE `.exp-summary` | TODO |
| Preview décoratif | N/A | — | NOT_APPLICABLE | — |

---

## COMPOSANTS À CRÉER (CSS + markup)

### Priorité HAUTE

| Pattern | CSS | Utilisé sur |
|---|---|---|
| KPI tile flat (left-border accent) | `.kpi-tile-grid` + `.kpi-tile` | Dashboard, Players, Teams, PositionProfiles |
| Signal matrix 5-col | `.signal-matrix` + `__item--fort/bon/moyen/faible` | Dashboard |
| Brief section 3 variantes | `.brief-section` (signal/court/warning) | Players Brief |
| Brief stat row + expand | `.brief-stat-row` + `<details>` | Players Brief |
| Team performer card | `.team-performer-card` + `--defense/goalkeeper` | Teams |
| Position chip par code | `.position-chip--{gardienne,arriere,pivot,ailiere,demi}` | Players, Dashboard, Compare |
| Terrain handball player card | `.team-day-player-card` + grid-areas | Dashboard |
| Dashboard signal item | `.dashboard-signal-item` | Dashboard |
| Filter section toggle | `.filter-section.open` + `__toggle` | Tous drawers |

### Priorité MOYENNE

| Pattern | CSS | Utilisé sur |
|---|---|---|
| Score timeline markers | `.score-timeline` + `__marker--*` | Matches |
| Compare player slots | `.cmp-bar` + `.cmp-player` | Compare |
| Compare diff rows | `.cmp-diff-item` + `.cmp-diff-row` 5-col | Compare |
| Narrative card | `.narrative-card` + `.narrative-cards-row` | PositionProfiles |
| Section header | `.section-header` + `__badge` 4px signal | Teams, PositionProfiles |
| Match list item colors | `.match-list-item` win/loss | Matches |
| Result badge | `.result-badge--v/n/d` | Players Matchs |

### Priorité FAIBLE

| Pattern | CSS | Utilisé sur |
|---|---|---|
| Export stepper | `.exp-stepper` + `.exp-option-row` | Export |
| Tab raised/pills | `.tab-bar.--raised` + `.tab-bar.--pills` | Teams, Compare |
| Trend indicator | `.trend-indicator` | Players Evolution |

---

## COMPOSANTS À RESTYLE (existants)

| Composant | CSS actuelle | CSS cible |
|---|---|---|
| `KpiTileGrid.razor` | `.metric-tape` | `.kpi-tile-grid` + `.kpi-tile` |
| `BarGaugeKpiCard.razor` | Jauge SVG circulaire | `.brief-stat-row` (Players Brief) |
| `MatchCard.razor` | Custom | `.match-card` mockup style |
| `PlayerList.razor` | Custom | `.player-list` + `.player-row` 4-col |
| `AudienceLensSelector.razor` | Custom | `.audience-lens` + `__btn` |
| `GlobalScopeBar.razor` | Scope ribbon partiel | `.scope-ribbon` complet |
| `Drawer.razor` | Custom | `.drawer__panel` 38rem |
| `ScopeSummaryBar.razor` | Custom | `.scope-summary-bar` |
| `CoachCards.razor` | Custom | `.coach-card` + `.narrative-card` |
| `DetailedTable.razor` | Custom | `.data-table` + heat-tone |
| `StateCard.razor` | Custom | `.empty-state` |
| `PageLoader.razor` | Custom | `.loading-overlay` + `.skeleton` |

---

## FONCTIONNALITÉS HANDWSTAT_ONLY (à conserver absolument)

`DataQualityBadge` / `DataQualitySummary` / `AnalyticsSourceBadge` / États `DATA_MISSING` + `INSUFFICIENT_SAMPLE` dans `PerformanceMetricRow` / Update gate + dialog / Demo mode / Mobile dock / Context lens session info / SHA-256 export / Android scroll JS fallback / `PlayerTrajectoryMetricCatalog` / Windows Last5/Last10/Season / Delta vs saison / ErrorBoundary Court / ETag cache / `BusyUiHelper` 300ms.

---

## RISQUES

| Risque | Sévérité |
|---|---|
| `BarGaugeKpiCard` → tile plate (paradigme visuel fort) | HAUTE |
| `ChartPalette.cs` impact global sur tous les charts | HAUTE |
| Disque C: ~1.8 Go libres (surveiller) | HAUTE |
| Segmentation `app.css` monolithique | HAUTE |
| `:has()` CSS pour Export radio rows (compat MAUI WebView) | MOYENNE |
| Terrain handball glassmorphism (CSS grid complex) | MOYENNE |
| Android scroll fallback JS à préserver | MOYENNE |

---

## PLAN M1 → M12

| Phase | Contenu | Durée |
|---|---|---|
| **M1** | `ChartPalette.cs` · vérif tokens `app.css` complet | 2–3h |
| **M2** | Shell `MainLayout` · commandbar · rail · scope-ribbon · auth layout | 3–4h |
| **M3** | `.kpi-tile-grid` · tabs 3 variantes · badges · `.position-chip` · `PlayerList` · filter drawer | 4–6h |
| **M4** | Dashboard complet | 6–8h |
| **M5** | Players complet (incl. Brief refactor) | 8–10h |
| **M6** | Teams | 4–5h |
| **M7** | Matches | 6–8h |
| **M8** | Compare | 4–5h |
| **M9** | Position Profiles | 5–6h |
| **M10** | Export | 3–4h |
| **M11** | Auth + états globaux | 3–4h |
| **M12** | Responsive + A11y + cleanup | 4–5h |

**Total estimé : 50–65h. Gate `dotnet build` + `dotnet test` après chaque phase.**

---

## VISUAL CONTRACT (à remplir au fil des milestones)

| Page | LAYOUT | TYPOGRAPHY | SPACING | COMPONENTS | COLOR | INTERACTION | RESPONSIVE |
|---|---|---|---|---|---|---|---|
| Dashboard | TODO | TODO | TODO | TODO | TODO | TODO | TODO |
| Players | TODO | TODO | TODO | TODO | TODO | TODO | TODO |
| Teams | TODO | TODO | TODO | TODO | TODO | TODO | TODO |
| Matches | TODO | TODO | TODO | TODO | TODO | TODO | TODO |
| Compare | TODO | TODO | TODO | TODO | TODO | TODO | TODO |
| PositionProfiles | TODO | TODO | TODO | TODO | TODO | TODO | TODO |
| Export | TODO | TODO | TODO | TODO | TODO | TODO | TODO |
| Login | TODO | TODO | TODO | TODO | TODO | TODO | TODO |

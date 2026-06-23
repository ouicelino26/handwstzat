# HandWStat - Baseline des fichiers UI

Cette baseline contient 54 fichiers source de presentation. Le minimum de reconstruction
est de 49 fichiers modifies, avec reecriture structurelle des fichiers critiques.

## Enveloppe native et host - 5

- `App.xaml` - ressources et theme natifs ;
- `MainPage.xaml` - conteneur BlazorWebView ;
- `Platforms/Windows/App.xaml` - ressources Windows ;
- `wwwroot/index.html` - host, scripts et boot state ;
- `wwwroot/app.css` - design system global.

## Infrastructure Razor - 9

- `Components/_Imports.razor` - imports de composants ;
- `Components/Routes.razor` - routage, focus et transitions ;
- `Components/Layout/MainLayout.razor` - shell authentifie ;
- `Components/Layout/MainLayout.razor.css` - shell authentifie ;
- `Components/Layout/NavMenu.razor` - rail de domaines ;
- `Components/Layout/NavMenu.razor.css` - rail de domaines ;
- `Components/Layout/PublicLayout.razor` - shell public ;
- `Components/Layout/PublicLayout.razor.css` - shell public ;
- `Components/Pages/NotFound.razor` - scene introuvable.

## Pages - 16

- `Components/Pages/Home.razor` ;
- `Components/Pages/Home.razor.css` ;
- `Components/Pages/Demo.razor` ;
- `Components/Pages/Demo.razor.css` ;
- `Components/Pages/Dashboard.razor` ;
- `Components/Pages/Dashboard.razor.css` ;
- `Components/Pages/Players.razor` ;
- `Components/Pages/Players.razor.css` ;
- `Components/Pages/Teams.razor` ;
- `Components/Pages/Matches.razor` ;
- `Components/Pages/Compare.razor` ;
- `Components/Pages/PositionProfiles.razor` ;
- `Components/Pages/PositionProfiles.razor.css` ;
- `Components/Pages/Counter.razor` ;
- `Components/Shared/GlobalScopeBar.razor` ;
- `Components/Shared/CommandPalette.razor`.

## Composants de donnees et feedback - 12

- `Components/Shared/AccessRequiredCard.razor` ;
- `Components/Shared/AudienceLensSelector.razor` ;
- `Components/Shared/BarGaugeKpiCard.razor` ;
- `Components/Shared/BarGaugeKpiCard.razor.css` ;
- `Components/Shared/BarGaugeKpiGrid.razor` ;
- `Components/Shared/BarGaugeKpiGrid.razor.css` ;
- `Components/Shared/KpiTileGrid.razor` ;
- `Components/Shared/PageLoader.razor` ;
- `Components/Shared/ScopeSummaryBar.razor` ;
- `Components/Shared/StateCard.razor` ;
- `Components/Shared/DetailedTable.razor` ;
- `Components/Shared/CoachCards.razor`.

## Composants d'entites et filtres - 8

- `Components/Shared/PlayerList.razor` ;
- `Components/Shared/PlayerList.razor.css` ;
- `Components/Shared/PlayerTeamHistoryPanel.razor` ;
- `Components/Shared/PositionFilters.razor` ;
- `Components/Shared/MatchCard.razor` ;
- `Components/Shared/MatchCard.razor.css` ;
- `Components/Shared/GoalKpi.razor` ;
- `Components/Shared/GoalKpi.razor.css`.

## Composants de visualisation - 4

- `Components/Shared/PositionProfileHistogram.razor` ;
- `Components/Shared/PositionRadarChart.razor` ;
- `Components/Shared/MultiRadar.razor` ;
- `Components/Shared/ScatterChart.razor`.

## Traitement attendu

| Groupe | Traitement |
| --- | --- |
| Enveloppe native | Reecriture visuelle complete |
| Layouts | Remplacement structurel |
| Pages | Nouvelle composition et hierarchie |
| Composants de donnees | Nouveau markup et nouveaux etats |
| Entites/filtres | Nouvelles interactions |
| Visualisations | Nouveau cadre, legende, accessibilite et responsive |
| CSS | Remplacement du langage visuel, pas simple surcharge |

Le CSS Bootstrap minifie est exclu car il s'agit d'un artefact tiers.

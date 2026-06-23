# 01 — Inventaire du projet

## 1. Arborescence commentée

```
handwstzat/
│
├── HandWStat.slnx               Solution MAUI (format XML léger)
├── HandWStat.csproj             Projet unique multi-plateforme MAUI Blazor
├── appsettings.json             Configuration API (BaseUrl, ClientId, ClientSecret)
├── App.xaml / App.xaml.cs       Point d'entrée MAUI : thème, ressources globales
├── MainPage.xaml / .cs          Page native hôte du BlazorWebView
├── MauiProgram.cs               Composition root : DI, services, ApexCharts
│
├── Components/                  Tout le code Blazor (UI)
│   ├── _Imports.razor           Usings globaux Razor (@using, @inject shortcuts)
│   ├── Routes.razor             Routeur Blazor (MainLayout / PublicLayout)
│   │
│   ├── Layout/
│   │   ├── MainLayout.razor     Shell authentifié : sidebar, topbar, mobile nav
│   │   ├── MainLayout.razor.css Styles scopés du shell
│   │   ├── NavMenu.razor        Rail de navigation (sidebar items)
│   │   ├── NavMenu.razor.css    Styles scopés NavMenu
│   │   ├── PublicLayout.razor   Layout minimal pour page login/home publique
│   │   └── PublicLayout.razor.css
│   │
│   ├── Pages/
│   │   ├── Home.razor           Page login + mode démo (PublicLayout)
│   │   ├── Home.razor.cs        Code-behind : gestion session, chargement dashboard
│   │   ├── Home.razor.css       Styles scopés Home (landing, login card)
│   │   ├── Dashboard.razor      Tableau de bord principal (MainLayout)
│   │   ├── Dashboard.razor.css  Styles scopés Dashboard
│   │   ├── Players.razor        Fiche joueuse (MainLayout)
│   │   ├── Compare.razor        Comparaison multi-joueuses (MainLayout)
│   │   ├── Teams.razor          Vue équipe (MainLayout)
│   │   ├── Matches.razor        Liste et détail matchs (MainLayout)
│   │   ├── Demo.razor           Mode démo sans authentification (MainLayout)
│   │   ├── Demo.razor.css       Styles scopés Demo
│   │   ├── Counter.razor        Page résiduelle du template MAUI (inutilisée)
│   │   └── NotFound.razor       Page 404
│   │
│   └── Shared/
│       ├── AccessRequiredCard.razor     Bloc "connexion requise" (StateCard wrappé)
│       ├── AudienceLensSelector.razor   Sélecteur de mode de lecture (Club/Analyste/Joueuse)
│       ├── BarGaugeKpiCard.razor        Carte KPI avec jauge jQWidgets
│       ├── BarGaugeKpiCard.razor.css
│       ├── BarGaugeKpiGrid.razor        Grille de BarGaugeKpiCard
│       ├── BarGaugeKpiGrid.razor.css
│       ├── GoalKpi.razor                Carte KPI buts avec indicateur visuel
│       ├── GoalKpi.razor.css
│       ├── KpiTileGrid.razor            Grille de KpiTile (cartes métriques textuelles)
│       ├── MatchCard.razor              Carte résumé match cliquable
│       ├── MatchCard.razor.css
│       ├── ScopeSummaryBar.razor        Barre résumant le périmètre de filtre actif
│       └── StateCard.razor              Composant d'état générique (vide, erreur, accès)
│
├── Models/
│   └── Analytics/               Modèles de présentation locaux (pas de DTOs backend ici)
│       ├── AudienceLens.cs          Constantes + options du sélecteur de lens
│       ├── BarGaugeKpiItem.cs       Record pour BarGauge KPI
│       ├── ChartModels.cs           Records pour graphiques (timeline, tendances)
│       ├── DashboardModels.cs       DashboardSnapshot, PlayerSpotlight, ZoneStat, etc.
│       ├── FilterModels.cs          DashboardFilterState, AnalyticsReferenceData, RankingMetricCatalog
│       ├── KpiModels.cs             KpiTile, HandballKpiHelper (formules KPI)
│       ├── MatchFilterCatalog.cs    Helpers filtres saison/journée
│       ├── MatchKpiCatalog.cs       Calcul des KpiTile pour la vue match
│       ├── MatchScenarioAnalyzer.cs Analyse timeline, runs, moments clés
│       ├── ScopeSummaryModels.cs    ScopeSummaryItem (barre de contexte filtre)
│       ├── SmartFilterCatalog.cs    Filtres contraints par le jeu de données
│       ├── SpatialZoneVisuals.cs    Helpers heatmap zones (goalkeeper/field)
│       ├── TableHeatToneHelper.cs   Coloration relative des cellules de tableau
│       └── TableInteractionModels.cs TableSortState, TextSearchHelper
│
├── Models/
│   └── Api/
│       └── ApiContracts.cs      LoginRequest, LoginResponse
│
├── Configuration/
│   ├── ApiSettings.cs           POCO de config ApiSettings
│   └── AppSettingsLoader.cs     Lecture appsettings.json embarqué MAUI
│
├── Services/
│   ├── IApiAuthService.cs       Interface + record ApiSession
│   ├── ApiAuthService.cs        Login/logout JWT, application header Authorization
│   ├── ReferenceDataService.cs  Cache des données de référence (compétitions, équipes, etc.)
│   ├── StatsDashboardService.cs Orchestration du chargement du tableau de bord
│   └── DemoDataFactory.cs       Données de démonstration statiques (3 profils joueurs)
│
├── Services/Api/
│   ├── ApiClientBase.cs         Classe de base HTTP (GET, POST, auth, erreurs)
│   ├── ApiQueryBuilder.cs       Constructeur de query string typé
│   ├── CompetitionsApiClient.cs GET /api/Competitions
│   ├── LookupsApiClient.cs      GET /api/Lookups/events|positions|nationalities|attacks|defenses
│   ├── MatchesApiClient.cs      GET /api/Matches, /api/Matches/{id}, /api/Matches/{id}/summary
│   ├── MatchEventsApiClient.cs  GET /api/MatchEvents
│   ├── PlayersApiClient.cs      GET /api/Players, /profile, /matches
│   ├── StatsApiClient.cs        GET /api/Stats/* (toutes les stats)
│   └── TeamsApiClient.cs        GET /api/Teams
│
├── Platforms/
│   ├── Android/                 Manifest, MainActivity, MainApplication, couleurs
│   ├── iOS/                     AppDelegate, Info.plist, PrivacyInfo
│   ├── MacCatalyst/             AppDelegate, Entitlements, Info.plist
│   └── Windows/                 App.xaml, Package.appxmanifest, app.manifest
│
├── Resources/
│   ├── AppIcon/                 SVG icône app (appicon.svg, appiconfg.svg)
│   ├── Fonts/                   OpenSans-Regular.ttf
│   ├── Images/                  dotnet_bot.svg (template, non utilisé en prod)
│   ├── Raw/                     AboutAssets.txt (template)
│   └── Splash/                  splash.svg
│
├── Properties/
│   └── launchSettings.json      Profils de lancement Visual Studio
│
├── wwwroot/
│   ├── index.html               Page hôte Blazor WebView
│   ├── app.css                  Feuille de style principale (~1600 lignes)
│   ├── design/
│   │   ├── goal-map-template.svg    Template SVG carte de buts
│   │   └── pitch-map-template.svg   Template SVG terrain
│   └── lib/bootstrap/
│       └── dist/css/            Bootstrap CSS (lib externe, non utilisée activement)
│
└── docs/
    ├── KPI_REFERENCE.md         Référentiel KPI par écran
    ├── KPI_REFERENCE_DETAILED.md Dictionnaire des actions + formules complètes
    ├── KPI_VALIDATION_TABLE.csv  Tableau de validation KPI
    └── KPI_VALIDATION_TABLE.md  Version Markdown du tableau
```

## 2. Fichiers générés ou externes — à ne pas analyser comme code métier

| Fichier / dossier | Nature | Rôle |
|---|---|---|
| `bin/`, `obj/` | Artefacts de build | Générés automatiquement |
| `.vs/` | Métadonnées VS | IDE local |
| `wwwroot/lib/bootstrap/` | Lib externe | CSS référencé mais non utilisé activement (app.css domine) |
| `Resources/Images/dotnet_bot.svg` | Asset template | Non utilisé en prod |
| `Resources/Raw/AboutAssets.txt` | Fichier template | Non utilisé en prod |
| `Components/Pages/Counter.razor` | Page template MAUI | Non référencée dans la navigation, résiduelle |

## 3. Zones fonctionnelles

| Zone | Périmètre |
|---|---|
| Authentification | `Home.razor`, `ApiAuthService`, `IApiAuthService`, `MainLayout` |
| Tableau de bord | `Dashboard.razor`, `Home.razor.cs`, `StatsDashboardService`, `DemoDataFactory` |
| Fiche joueuse | `Players.razor`, `PlayersApiClient`, `StatsApiClient` (player endpoints) |
| Comparaison | `Compare.razor`, `StatsApiClient.ComparePlayersAsync` |
| Équipes | `Teams.razor`, `TeamsApiClient`, `StatsApiClient` (team endpoints) |
| Matchs | `Matches.razor`, `MatchesApiClient`, `MatchEventsApiClient`, `MatchScenarioAnalyzer` |
| Calculs KPI | `KpiModels.cs` (HandballKpiHelper), `MatchKpiCatalog`, `MatchScenarioAnalyzer` |
| Référentiel | `ReferenceDataService`, `CompetitionsApiClient`, `TeamsApiClient`, `LookupsApiClient` |
| Filtres | `FilterModels.cs`, `MatchFilterCatalog`, `SmartFilterCatalog` |
| Visualisation spatiale | `SpatialZoneVisuals`, SVG templates, `StatsApiClient` (spatial endpoints) |
| Thème & styles | `wwwroot/app.css` + fichiers `.razor.css` scopés |
| Mode démo | `DemoDataFactory`, `Demo.razor` |

# 02 — Architecture existante

## 1. Vue d'ensemble

HandWStat est une application **MAUI Blazor Hybrid** (monopage, multi-plateforme).

```
┌────────────────────────────────────────────────────────┐
│  MAUI Native Shell (App.xaml / MainPage.xaml)          │
│  ┌──────────────────────────────────────────────────┐  │
│  │  BlazorWebView                                   │  │
│  │  ┌────────────────────────────────────────────┐  │  │
│  │  │  Blazor SPA (wwwroot/index.html)           │  │  │
│  │  │  Routes.razor → MainLayout / PublicLayout  │  │  │
│  │  │  Pages (Razor Components)                  │  │  │
│  │  └────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────┘
         │  HttpClient (DI Singleton)
         ▼
┌────────────────────────────────────────────────────────┐
│  API Backend  https://handballwstat.ddnsfree.com/api/  │
│  (externe — non présent dans ce dépôt)                 │
└────────────────────────────────────────────────────────┘
```

## 2. Couches techniques

### 2.1 Présentation (Blazor Razor Components)
- `Components/Layout/` : shell, navigation
- `Components/Pages/` : vues principales
- `Components/Shared/` : composants réutilisables

### 2.2 Services applicatifs (DI Singleton)
- `ApiAuthService` : session, authentification JWT
- `ReferenceDataService` : cache des données de référence
- `StatsDashboardService` : orchestration dashboard

### 2.3 Clients API (DI Singleton, héritent ApiClientBase)
- Un client par domaine : `CompetitionsApiClient`, `TeamsApiClient`, `PlayersApiClient`, `MatchesApiClient`, `MatchEventsApiClient`, `StatsApiClient`, `LookupsApiClient`

### 2.4 Modèles de présentation
- `Models/Analytics/` : records locaux (pas de dépendance circulaire avec les DTO)
- `Models/Api/` : contrats d'authentification

### 2.5 DTOs (dépendance externe)
- `HandballManagerCore.DTO` : tous les DTOs de réponse API
- `HandballManagerCore.Models` : `MatchEvent`

## 3. Flux de données principal

```
Utilisateur → Page Razor
  → filtre DashboardFilterState
  → StatsDashboardService.LoadDashboardAsync()
    → appels parallèles (Task.WhenAll) :
        StatsApiClient.GetOverviewAsync()
        StatsApiClient.GetPlayersAsync()
        StatsApiClient.GetRankingsAsync("goals")
        StatsApiClient.GetRankingsAsync("shotsuccess")
        StatsApiClient.GetRankingsAsync(metric demandé)
        StatsApiClient.GetRankingsAsync("interceptions")
        MatchesApiClient.GetMatchesAsync()
    → résolution du joueur spotlight
    → si joueur trouvé : 10 appels parallèles supplémentaires
        PlayersApiClient.GetPlayerProfileAsync()
        StatsApiClient.GetPlayerGlobalAsync()
        StatsApiClient.GetPlayerOffenseAsync()
        StatsApiClient.GetPlayerTechnicalAsync()
        StatsApiClient.GetPlayerDefenseAsync()
        StatsApiClient.GetPlayerPassingAsync()
        StatsApiClient.GetPlayerSanctionsAsync()
        StatsApiClient.GetPlayerGoalkeeperAsync()
        StatsApiClient.GetPlayerSpatialAsync()
        PlayersApiClient.GetPlayerMatchesAsync()
  → construit DashboardSnapshot
  → mise à jour StateHasChanged()
```

## 4. Authentification

```
Home.razor (login form)
  → ApiAuthService.LoginAsync(username, password)
    → POST https://handballwstat.ddnsfree.com/api/auth/login
    → reçoit { accesstoken, username, role }
    → stocke ApiSession (IsAuthenticated, AccessToken, Role)
    → déclenche event SessionChanged
  → MainLayout / composants s'abonnent à SessionChanged
  → redirection vers /dashboard si succès
  → ReferenceDataService.ClearCache() sur logout
```

**Pas de refresh token.** Si le token expire, l'utilisateur doit se reconnecter manuellement. Les appels API qui reçoivent 401/403 lèvent `InvalidOperationException`.

## 5. Routes frontend

| Route | Page | Layout | Accès |
|---|---|---|---|
| `/` | `Home.razor` | PublicLayout | Public (login + démo) |
| `/dashboard` | `Dashboard.razor` | MainLayout | Authentifié |
| `/players` | `Players.razor` | MainLayout | Authentifié |
| `/compare` | `Compare.razor` | MainLayout | Authentifié |
| `/teams` | `Teams.razor` | MainLayout | Authentifié |
| `/matches` | `Matches.razor` | MainLayout | Authentifié |
| `/demo` | `Demo.razor` | MainLayout | Public (accès sans auth) |
| `*` | `NotFound.razor` | — | Toujours |

**Note :** `MainLayout` n'impose pas de guard de navigation — c'est chaque page qui gère l'accès via `AccessRequiredCard` et une vérification `AuthService.Session.IsAuthenticated`.

## 6. Carte des endpoints API

### Authentification
| Méthode | Route | Usage |
|---|---|---|
| POST | `auth/login` | Login JWT |

### Compétitions
| Méthode | Route | Usage |
|---|---|---|
| GET | `api/Competitions` | Liste toutes les compétitions |
| GET | `api/Competitions/{id}` | Détail compétition |

### Équipes
| Méthode | Route | Usage |
|---|---|---|
| GET | `api/Teams` | Liste toutes les équipes |
| GET | `api/Teams/{id}` | Détail équipe |
| GET | `api/Teams/by-code/{code}` | Équipe par code |

### Lookups
| Méthode | Route | Usage |
|---|---|---|
| GET | `api/Lookups/events` | Types d'événements |
| GET | `api/Lookups/positions` | Postes |
| GET | `api/Lookups/nationalities` | Nationalités |
| GET | `api/Lookups/attacks` | Systèmes d'attaque |
| GET | `api/Lookups/defenses` | Systèmes de défense |

### Joueuses
| Méthode | Route | Paramètres principaux |
|---|---|---|
| GET | `api/Players` | teamId, positionId, competitionId, year, season, day, search, page, pageSize |
| GET | `api/Players/{id}` | — |
| GET | `api/Players/{id}/profile` | options StatsQueryOptionsDto |
| GET | `api/Players/{id}/matches` | options StatsQueryOptionsDto |

### Matchs
| Méthode | Route | Paramètres principaux |
|---|---|---|
| GET | `api/Matches` | competitionId, teamId, playerId, from, to, year, season, day, page, pageSize |
| GET | `api/Matches/{id}` | — |
| GET | `api/Matches/{id}/summary` | — |
| GET | `api/MatchEvents` | matchId |

### Statistiques
| Méthode | Route | Paramètres |
|---|---|---|
| GET | `api/Stats/overview` | StatsQueryOptionsDto |
| GET | `api/Stats/rankings` | metric, top + StatsQueryOptionsDto |
| GET | `api/Stats/players` | StatsQueryOptionsDto |
| GET | `api/Stats/players/{id}/global` | StatsQueryOptionsDto |
| GET | `api/Stats/players/{id}/offense` | StatsQueryOptionsDto |
| GET | `api/Stats/players/{id}/technical` | StatsQueryOptionsDto |
| GET | `api/Stats/players/{id}/defense` | StatsQueryOptionsDto |
| GET | `api/Stats/players/{id}/passing` | StatsQueryOptionsDto |
| GET | `api/Stats/players/{id}/sanctions` | StatsQueryOptionsDto |
| GET | `api/Stats/players/{id}/goalkeeper` | StatsQueryOptionsDto |
| GET | `api/Stats/players/{id}/spatial` | StatsQueryOptionsDto |
| GET | `api/Stats/players/{id}/spatial/zones` | StatsQueryOptionsDto |
| GET | `api/Stats/players/{id}/spatial/triggers` | StatsQueryOptionsDto |
| GET | `api/Stats/players/{id}/spatial/events-by-zone` | StatsQueryOptionsDto |
| GET | `api/Stats/matches/{id}/players` | — |
| GET | `api/Stats/matches/{id}/spatial` | teamId |
| GET | `api/Stats/matches/{id}/summary` | — |
| GET | `api/Stats/events` | StatsQueryOptionsDto + page/pageSize |
| GET | `api/Stats/events/contexts` | StatsQueryOptionsDto |
| GET | `api/Stats/teams/{id}` | StatsQueryOptionsDto |
| GET | `api/Stats/teams/{id}/players` | StatsQueryOptionsDto |
| POST | `api/Stats/compare/players` | ComparePlayersRequestDto |

## 7. Gestion de l'état

- **Pas de store global** (Flux, Redux, etc.)
- État local dans chaque composant (champs privés, `StateHasChanged()`)
- `IApiAuthService` joue le rôle de store de session (singleton, événement `SessionChanged`)
- `ReferenceDataService` joue le rôle de cache (singleton, invalidé sur logout)
- `DashboardFilterState` est un objet de filtre local instancié dans chaque page

## 8. Persistance

- **Aucune persistance locale** : pas de LocalStorage, pas de SQLite, pas de SecureStorage
- Les données ne survivent pas à la fermeture de l'application
- Le token JWT est en mémoire uniquement

## 9. Dépendances entre modules

```
Pages
  └── dépendent de → Services (DI inject)
  └── dépendent de → Models/Analytics (types locaux)
  └── dépendent de → HandballManagerCore.DTO (types DTOs)

Services
  └── StatsDashboardService → StatsApiClient, PlayersApiClient, MatchesApiClient
  └── ReferenceDataService → CompetitionsApiClient, TeamsApiClient, LookupsApiClient
  └── Tous les clients API → ApiClientBase → ApiAuthService, ApiSettings

Models/Analytics
  └── KpiModels.cs → HandballManagerCore.DTO (pour les helpers)
  └── MatchScenarioAnalyzer → HandballManagerCore.DTO + HandballManagerCore.Models
```

## 10. Responsabilités mal réparties (identifiées)

| Problème | Localisation | Impact |
|---|---|---|
| `StatsDashboardService` fait 17 appels API pour un seul chargement | `StatsDashboardService.LoadDashboardAsync()` | Performance, couplage fort au dashboard |
| Les fallbacks de données manquantes sont dans le service, pas dans les modèles | `StatsDashboardService` (CreateXxxFallback) | Logique de présentation mélangée avec orchestration |
| `DemoDataFactory` crée des objets DTO directement | `DemoDataFactory.cs` | Couplage entre les données demo et les contrats API |
| La logique de résolution du joueur spotlight est dans le service | `ResolveSelectedPlayerId()` | Règle métier UI dans une couche service |
| `Counter.razor` résiduel du template | `Components/Pages/Counter.razor` | Code mort dans la navigation |

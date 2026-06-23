# HandWStat - Audit fonctionnel et metier

Date de l'audit : 22 juin 2026

Branche analysee : `main`

Revision analysee : `5450311`

## 1. Perimetre analyse

Le depot HandWStat est une application .NET MAUI Blazor Hybrid qui cible Android, iOS,
Mac Catalyst et Windows. Le depot contient 126 fichiers utiles hors sorties de compilation,
artefacts Visual Studio, publications et rapports generes.

Les zones examinees sont :

- point d'entree MAUI et injection de dependances ;
- router Blazor et layouts ;
- toutes les pages Razor ;
- tous les composants partages ;
- tous les services et clients HTTP ;
- tous les modeles analytiques locaux ;
- configuration, assets, JavaScript et CSS ;
- documentation existante ;
- contrats DTO du projet `HandballManagerCore` ;
- controles backend necessaires dans `HandballManagerAPI` pour confirmer les routes,
  permissions et calculs statistiques consommes par l'interface.

Les fichiers tiers minifies, la police binaire, les sorties `bin/obj`, les publications et
les fichiers Visual Studio ont ete classes par nature mais ne portent pas de logique metier
HandWStat.

## 2. Etat technique observe

### 2.1 Stack

| Couche | Technologie |
| --- | --- |
| Client | .NET 10, MAUI Blazor Hybrid |
| UI | Razor components, CSS global et CSS isole |
| Graphiques | ApexCharts, jQWidgets BarGauge, SVG maison |
| HTTP | `HttpClient` singleton, clients API types |
| Authentification | JWT conserve en memoire pendant la session |
| API | ASP.NET Core, roles `Admin` et `Consultation` |
| Donnees | MySQL via Entity Framework Core cote API |
| Contrats | Projet externe `HandballManagerCore` en .NET 8 |

### 2.2 Compilation

Le build standard de `main` echoue car
[`HandWStat.csproj`](../../HandWStat.csproj) contient un chemin utilisateur absolu vers
`HandballManagerCore`.

Commande standard testee :

```powershell
dotnet build HandWStat.csproj -f net10.0-windows10.0.19041.0 --configuration Release
```

Resultat : echec, 418 erreurs en cascade liees a la reference projet introuvable.

Le meme code compile avec le chemin reel injecte :

```powershell
dotnet build HandWStat.csproj -f net10.0-windows10.0.19041.0 `
  --configuration Release `
  -p:HandballManagerCorePath=D:\repos\HandballManagerCore
```

Resultat : 0 erreur et 26 avertissements WinAppSDK `PRI249` issus des assets jQWidgets.

### 2.3 Tests

Aucun projet de tests HandWStat n'est suivi dans ce depot. Le `.csproj` exclut explicitement
un repertoire `HandWStat.Tests`, mais celui-ci n'est pas present dans l'inventaire Git.

Le backend possede des tests, notamment pour les services analytics et les endpoints, mais
ils appartiennent a un depot distinct.

## 3. Architecture actuelle

```text
Application MAUI
└── MainPage
    └── BlazorWebView
        ├── Router
        │   ├── PublicLayout
        │   │   ├── /             Connexion
        │   │   └── /demo         Demonstration
        │   └── MainLayout
        │       ├── /dashboard
        │       ├── /players
        │       ├── /compare
        │       ├── /position-profiles
        │       ├── /teams
        │       ├── /matches
        │       └── /counter
        ├── Services de composition
        │   ├── ReferenceDataService
        │   └── StatsDashboardService
        └── Clients API types
            ├── CompetitionsApiClient
            ├── TeamsApiClient
            ├── LookupsApiClient
            ├── PlayersApiClient
            ├── MatchesApiClient
            ├── MatchEventsApiClient
            └── StatsApiClient
```

Le client ne possede ni store global, ni couche de requetes partagee, ni cache general.
Seules les donnees de reference et certains details de matchs sont mis en cache.

## 4. Authentification et permissions

### 4.1 Parcours

1. L'utilisateur saisit identifiant et mot de passe sur `/`.
2. `ApiAuthService` appelle `POST auth/login`.
3. Le token JWT, le nom et le role sont conserves uniquement en memoire.
4. Les clients API ajoutent `Authorization: Bearer`.
5. Une reponse 401 ou 403 devient une exception fonctionnelle generique.
6. La deconnexion efface la session et renvoie vers `/`.

### 4.2 Roles reels

L'API autorise les roles `Admin` et `Consultation` pour les lectures. Les mutations,
imports et suppressions sont reservees a `Admin`.

L'application HandWStat actuelle n'expose aucun ecran CRUD, aucun import et aucune
administration. Elle fournit uniquement des parcours de consultation, comparaison et
export local.

### 4.3 Regles implicites

- La session n'est pas persistante apres fermeture de l'application.
- Aucun rafraichissement de token n'est implemente.
- `/dashboard` redirige vers `/` si la session est absente.
- Les autres pages privees rendent `AccessRequiredCard`.
- Le role recu est stocke mais n'est pas utilise pour adapter l'interface.

## 5. Cartographie exhaustive des ecrans

### 5.1 `/` - Connexion

Objectif : presenter le produit, ouvrir une session ou acceder a la demonstration.

Fonctionnalites :

- hero de presentation ;
- trois promesses produit ;
- bouton `Ouvrir la demo` ;
- formulaire identifiant/mot de passe ;
- bouton `Se connecter` ;
- bouton `Voir l'apercu` ;
- statut de connexion ou erreur ;
- redirection automatique vers `/dashboard` si deja authentifie.

Validations :

- champs vides rejetes dans `ApiAuthService` ;
- mot de passe efface apres tentative ;
- bouton desactive pendant la requete.

### 5.2 `/demo` - Demonstration publique

Objectif : montrer les principes du produit avec des donnees locales.

Fonctionnalites :

- compteurs joueuses, equipes, matchs et buts ;
- top 5 scoreuses ;
- focus joueuse ;
- carte spatiale but et terrain ;
- selection interactive d'une zone ;
- volume, succes, echecs et detail des resultats de zone ;
- deux acces vers la connexion.

Source : `DemoDataFactory`, sans appel API.

### 5.3 `/dashboard` - Pilotage global

Objectif : lire la ligue ou le perimetre filtre avant de descendre dans le detail.

Filtres :

- competition ;
- equipe ;
- saison ;
- journee ;
- reinitialisation.

Sections :

- `Ligue` ;
- `Joueuse suivie` ;
- `Derniers reperes` ;
- `Classements globaux`.

Fonctionnalites :

- hero : equipes actives, buts par match, part de jeu prepare ;
- KPI ligue ;
- classement configurable par metrique ;
- choix du top 3 a 12 ;
- choix d'une joueuse spotlight ;
- KPI et distribution de la joueuse ;
- historique de clubs ;
- lecture spatiale par zone ;
- classement complet des joueuses de champ ;
- classement specifique des gardiennes ;
- filtre des classements par poste ;
- tri de toutes les colonnes des deux tableaux ;
- cartes des matchs recents.

Metriques de classement :

- buts ;
- taux de tir ;
- interceptions ;
- passes ;
- arrets ;
- pertes ;
- sanctions ;
- taux d'arret et autres options declarees dans `RankingMetricCatalog`.

### 5.4 `/players` - Fiche joueuse

Objectif : analyser une joueuse dans un perimetre coherent.

Prerequis : au moins un critere de repertoire.

Filtres :

- recherche nom/prenom avec debounce ;
- competition ;
- equipe ;
- poste ;
- saison ;
- journee ;
- reinitialisation.

Interactions :

- ouvrir/replier le drawer de filtres ;
- ouvrir/replier la liste de joueuses ;
- selectionner une joueuse ;
- exporter une fiche SVG avec radar poste ;
- choisir un onglet ;
- selectionner/reinitialiser une zone ;
- rechercher et trier les matchs.

Onglets :

- `Synthese` : KPI, jauges, contexte et historique de clubs ;
- `Analyse` : attaque, defense, passe/perte, sanctions, gardienne, technique ;
- `Graphes` : profil d'impact, rendement, tendance recente, scatter pertes/impact ;
- `Zones` : carte but, carte terrain, focus de zone et resultats ;
- `Matchs` : recherche, tri et cartes de feuilles de match.

Le chargement d'une joueuse declenche dix appels API en parallele : profil, global,
attaque, defense, passe, sanctions, gardienne, technique, spatial et matchs.

### 5.5 `/compare` - Comparaison multi-joueuses

Objectif : comparer deux a six joueuses dans un perimetre commun.

Filtres :

- competition ;
- equipe ;
- poste ;
- saison ;
- journee ;
- nombre de slots, de 2 a 6.

Interactions :

- recherche locale dans chaque slot ;
- selection automatique initiale des premieres joueuses disponibles ;
- suppression ou remplacement d'une joueuse ;
- recalcul automatique apres chaque changement ;
- onglets `Synthese`, `Graphiques`, `Tableau`.

Visualisations :

- radar compare ;
- volumes cles ;
- efficacite ;
- technique ;
- production par match ;
- KPI et jauges par joueuse ;
- tableau comparatif avec coloration relative.

Le tableau compare notamment matchs, buts, passes, interceptions, arrets, pertes,
taux de tir, penalty, buts 7m, tirs engages, dechet, contres, neutralisations,
pertes techniques, conversions, mauvaises passes et sanctions.

### 5.6 `/position-profiles` - Profil par poste

Objectif : comparer une joueuse a la cohorte de son poste avec les statistiques `/60`.

Prerequis : au moins un filtre de repertoire.

Filtres :

- recherche avec debounce ;
- competition ;
- equipe pour le repertoire uniquement ;
- saison ;
- journee ;
- poste dans un volet etendu ;
- reinitialisation.

Regle de cohorte :

- la joueuse est choisie dans le repertoire filtre ;
- la cohorte mediane ignore volontairement le filtre equipe ;
- la cohorte conserve poste, competition, match/periode, annee, saison et journee ;
- seules les joueuses avec temps de jeu strictement positif participent a la cohorte.

Axes joueuses de champ :

- buts dans le jeu /60 ;
- passes decisives /60 ;
- sanctions obtenues /60 ;
- penalties obtenus /60 ;
- pertes de balle /60 ;
- interceptions /60 ;
- contres /60 ;
- neutralisations /60 ;
- penalties concedés /60 ;
- deux minutes /60 ;
- tirs rates /60 ;
- pourcentage dans le jeu.

Axes gardiennes :

- arrets /60 ;
- arrets 7m /60 ;
- taux d'arret ;
- tirs subis /60 ;
- buts encaisses /60 ;
- passes /60 ;
- pertes /60 ;
- sanctions /60.

Fonctionnalites :

- synthese instantanee ;
- cartes coach forces, faiblesses, role, tactique et alertes ;
- KPI snapshot ;
- histogramme valeur brute contre mediane brute ;
- radar normalise 0-100 ;
- comparaison avec jusqu'a trois joueuses supplementaires du meme poste ;
- masquage individuel des courbes du multi-radar ;
- scatter joueuse/mediane ;
- tableau complet trie par impact ;
- export CSV ;
- export radar SVG ;
- export fiche complete SVG ;
- copie du resume analyste.

### 5.7 `/teams` - Bilan equipe

Objectif : lire la performance collective, l'effectif et les matchs.

Filtres :

- equipe obligatoire ;
- competition ;
- saison ;
- journee ;
- poste, uniquement dans l'onglet effectif ;
- recherche effectif ;
- reinitialisation.

Onglets :

- `Synthese` : KPI, contribution directe, production collective, top internes,
  profil collectif et impact offensif ;
- `Effectif` : recherche, filtre poste et tableau triable ;
- `Matchs` : cartes des rencontres ouvrant `/matches?matchId=...`.

Le code contient aussi un chargement de detail et timeline de match interne, mais aucune
interaction ne renseigne `SelectedMatchId`. Cette branche fonctionnelle n'est donc pas
accessible depuis l'interface actuelle.

### 5.8 `/matches` - Analyse match

Objectif : parcourir les rencontres puis analyser un match.

Modes :

- liste de cartes sans query string ;
- vue detail avec `?matchId={id}`.

Filtres liste :

- competition ;
- equipe ;
- recherche equipe/adversaire/competition ;
- saison ;
- journee ;
- reinitialisation.

Onglets detail :

- `Synthese` ;
- `Zones` ;
- `Effectif`.

Interactions :

- revenir a la liste ;
- choisir le scope `Deux equipes`, equipe 1 ou equipe 2 ;
- filtrer les joueuses du match ;
- trier les colonnes ;
- selectionner/reinitialiser une zone.

Contenu :

- score final ;
- KPI de resume ;
- courbe d'evolution du score ;
- score a la pause, ecarts, runs et renversements ;
- temps forts ;
- comparaison des equipes ;
- profil du match ;
- joueuses decisives ;
- top scoreuses ;
- carte spatiale ;
- tableau des joueuses.

Les donnees principales, joueurs et cartes spatiales sont chargees progressivement et
cachees par match et contexte.

### 5.9 `/not-found`

Objectif : recuperer une navigation invalide.

Actions :

- retour au tableau ;
- acces a la demo.

### 5.10 `/counter`

Page de template MAUI non referencee dans la navigation. Elle incremente un compteur local.
Elle n'a aucune valeur produit et doit etre classee comme dette de template avant migration.

## 6. Composants partages

| Composant | Responsabilite |
| --- | --- |
| `AccessRequiredCard` | Redirection connexion ou demo |
| `AudienceLensSelector` | Choix Club/Analyste/Joueuse, non instancie actuellement |
| `BarGaugeKpiCard/Grid` | Jauges KPI jQWidgets |
| `KpiTileGrid` | Cartes KPI compactes |
| `ScopeSummaryBar` | Resume explicite du perimetre |
| `StateCard` | Erreur, vide, chargement, acces |
| `PageLoader` | Overlay de chargement |
| `PlayerList` | Repertoire repliable |
| `PlayerTeamHistoryPanel` | Club principal et historique |
| `MatchCard` | Resume et lien d'une rencontre |
| `GoalKpi` | Carte but et zones de declenchement |
| `PositionFilters` | Filtres du profil poste |
| `CoachCards` | Insights automatiques |
| `PositionProfileHistogram` | Joueuse contre mediane brute |
| `PositionRadarChart` | Radar individuel normalise |
| `MultiRadar` | Radar multi-joueuses et visibilite |
| `ScatterChart` | Position joueuse/mediane par axe |
| `DetailedTable` | Tableau complet du profil poste |

## 7. Appels API consommes

| Methode | Route | Consommateur principal |
| --- | --- | --- |
| POST | `auth/login` | Connexion |
| GET | `api/Competitions` | Donnees de reference |
| GET | `api/Teams` | Donnees de reference |
| GET | `api/Lookups/*` | Filtres et libelles |
| GET | `api/Matches` | Tous les scopes temporels |
| GET | `api/Matches/{id}` | Detail match |
| GET | `api/MatchEvents?matchId=` | Timeline |
| GET | `api/Players` | Repertoires |
| GET | `api/Players/{id}/profile` | Fiche joueuse |
| GET | `api/Players/{id}/matches` | Feuilles joueuse |
| GET | `api/Players/{id}/position-profile` | Profil poste |
| POST | `api/Players/position-profile/compare` | Multi-radar poste |
| GET | `api/Stats/overview` | Dashboard |
| GET | `api/Stats/rankings` | Classements |
| GET | `api/Stats/players` | Listes statistiques |
| GET | `api/Stats/players/{id}/*` | Details statistiques |
| GET | `api/Stats/players/{id}/spatial` | Zones joueuse |
| GET | `api/Stats/matches/{id}/summary` | Resume match |
| GET | `api/Stats/matches/{id}/players` | Effectif match |
| GET | `api/Stats/matches/{id}/spatial` | Zones match |
| POST | `api/Stats/compare/players` | Comparaison |
| GET | `api/Stats/teams/{id}` | Bilan equipe |
| GET | `api/Stats/teams/{id}/players` | Effectif equipe |

## 8. Regles metier et calculs

### 8.1 Regles communes

- Une division par zero produit 0.
- Les valeurs affichees sont arrondies a un chiffre decimal, sauf exceptions.
- Les taux et ratios proviennent soit de l'API, soit de `HandballKpiHelper`.
- Les filtres competition/equipe/saison/journee sont dependants des matchs disponibles.
- Une selection devenue invalide est automatiquement effacee.
- Les statistiques negatives utilisent une coloration inversee.

### 8.2 Formules principales

| Indicateur | Formule |
| --- | --- |
| Par match | total / matchs |
| Part | numerateur / denominateur x 100 |
| Ballons valorises | passes / (passes + pertes) x 100 |
| Impact defensif | interceptions + contres + neutralisations + passages forces |
| Contributions directes | buts + passes decisives |
| Arrets gardienne | arrets + arrets penalty |
| Sanctions | exclusions + avertissements + 2 minutes + penalties concedés |
| Tirs engages | buts + tirs rates + penalties rates + tirs contrés |
| Dechet tir | tirs rates + penalties rates + tirs contrés |
| Equilibre technique | positifs / (positifs + negatifs) x 100 |
| Valeur /60 | valeur brute x 60 / minutes jouees |

### 8.3 Timeline match

- Les evenements sont tries par minute puis identifiant.
- La deuxieme mi-temps recoit un decalage de 30 minutes si l'horloge repart de zero.
- Un point artificiel est garanti a la mi-temps et a la fin.
- Un renversement est un changement de leader hors egalite.
- Un run est une suite de buts sans reponse.
- Le money time est mesure depuis le dernier point a 50:30 ou avant.

### 8.4 Profil poste

- Mediane : valeur centrale, moyenne des deux valeurs centrales si effectif pair.
- Percentile backend favorable :
  - axe positif : part des valeurs inferieures ou egales ;
  - axe negatif : part des valeurs superieures ou egales.
- Bornes radar :
  - min/max reels si la cohorte a une dispersion ;
  - 0-100 pour les pourcentages sans dispersion ;
  - borne synthetique depuis zero pour les autres axes constants.
- La mediane du poste est calculee sur toutes les joueuses admissibles du poste,
  sans filtre d'equipe.

## 9. Dependances entre ecrans

```text
Connexion
└── Dashboard
    ├── choix joueuse spotlight
    ├── Matchs recents -> Matches?matchId
    └── Navigation principale
        ├── Joueuses -> Matchs?matchId
        ├── Comparaison
        ├── Profil poste
        ├── Equipes -> Matchs?matchId
        └── Matchs

Demo -> Connexion
Toute page privee sans session -> Connexion ou Demo
```

Il n'existe actuellement ni deep-link vers une joueuse, ni deep-link vers une equipe,
ni partage de filtre global entre modules.

# Profil Joueuse Par Poste

## Contexte

Ce document cadre la faisabilite d'une integration dans HandWStat a partir du notebook `GRAPHE JOUEUSES POSTES SUR 60MIN.ipynb` analyse le 25 avril 2026.

Le notebook propose 4 lectures principales :

- radar percentile d'une joueuse par rapport aux joueuses du meme poste
- radar d'une joueuse comparee a la mediane de son poste
- radar multi-joueuses sur un meme poste
- bar chart de niveau par axe

Le notebook travaille sur un CSV pre-calcule `60min.csv`, donc sur des metriques deja normalisees `par 60 minutes`.

## Verdict

La logique produit est integrable dans l'application.

La reproduction exacte en `par 60 minutes` n'est pas possible aujourd'hui avec la donnee actuellement stockee en base, car le temps de jeu reel par joueuse n'est pas disponible.

Conclusion :

- `oui` pour integrer une v1 robuste dans l'application
- `non` pour afficher des metriques `/60` exactes avec la base actuelle
- `oui` pour une v1 basee sur des volumes bruts, des ratios deja disponibles, ou des valeurs `par match`

## Axes Du Notebook

Le notebook utilise les 12 axes suivants :

1. `Buts dans le jeu/60`
2. `PD/60`
3. `Sanctions obtenues/60`
4. `7M obtenus/60`
5. `PDB/60`
6. `INTS/60`
7. `Contres/60`
8. `Neutra/60`
9. `7M concedes/60`
10. `2min/60`
11. `Tirs rates/60`
12. `% dans le jeu`

## Donnees Disponibles Aujourd'hui

### Base de donnees

Les donnees necessaires a une grande partie du profil existent deja :

- `182` matchs
- `54 567` evenements
- `276` joueuses
- `263` joueuses avec un poste renseigne

Sur le serveur, les evenements suivants existent bien en base :

- `But`
- `But sur penalte`
- `Passe decisive`
- `Interception`
- `Contre Reussi`
- `Neutralise l'attaquant`
- `Perte de balle`
- `Penalty obtenu`
- `Penalty concede`
- `Deux minutes`
- `Provoque une sanction`
- `Tir a cote`
- `Tir contre`
- `Tir arrete`

Tous les `matchevents` controles ont :

- un `PlayerId`
- un `Time`

Les champs de base exploitables sont dans [MatchEvent.cs](D:/repos/HandballManagerCore/HandballManagerCore/Models/MatchEvent.cs:9), [MatchEvent.cs](D:/repos/HandballManagerCore/HandballManagerCore/Models/MatchEvent.cs:21), [MatchEvent.cs](D:/repos/HandballManagerCore/HandballManagerCore/Models/MatchEvent.cs:25) et [MatchEvent.cs](D:/repos/HandballManagerCore/HandballManagerCore/Models/MatchEvent.cs:29).

Les postes sont portes par [Player.cs](D:/repos/HandballManagerCore/HandballManagerCore/Models/Player.cs:31) et [Position.cs](D:/repos/HandballManagerCore/HandballManagerCore/Models/Position.cs:7).

### API

L'API expose deja les briques utiles pour une v1 :

- liste de joueuses filtrees par poste, equipe, competition, saison, journee via [StatsController.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Controllers/StatsController.cs:94)
- stats globales joueuse via [StatsController.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Controllers/StatsController.cs:131)
- stats offense via [StatsController.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Controllers/StatsController.cs:165)
- stats defense via [StatsController.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Controllers/StatsController.cs:199)
- stats passing via [StatsController.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Controllers/StatsController.cs:233)
- stats sanctions via [StatsController.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Controllers/StatsController.cs:267)
- stats techniques via [StatsController.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Controllers/StatsController.cs:335)
- comparaison multi-joueuses via [StatsController.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Controllers/StatsController.cs:605)

Le coeur des aggregations existe deja dans [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:540).

Les compteurs deja disponibles dans l'accumulateur sont notamment :

- `Assists`
- `Interceptions`
- `CounterSuccesses`
- `Neutralisations`
- `PenaltyConceded`
- `TirsLoupes`
- `ShotSuccessRate`
- `PenaltySuccessRate`
- `MatchesPlayed`

Voir [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:563), [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:568), [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:569), [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:570), [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:575), [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:615) et [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:633).

Le classifieur connait deja les evenements metier principaux :

- `Passe decisive`
- `Interception`
- `Contre Reussi`
- `Neutralise l'attaquant`
- `Deux minutes`

Voir [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:108), [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:113), [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:114), [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:115) et [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:119).

## Ce Qui Manque Pour Une Reproduction Exacte

### 1. Le temps de jeu reel

Le notebook exprime la plupart des mesures en `/60`.

Aujourd'hui, la base contient :

- l'heure de l'evenement
- la mi-temps

Mais elle ne contient pas :

- le temps de jeu cumule par joueuse
- des evenements d'entree / sortie suffisamment structures
- un historique de rotations fiable pour reconstruire le temps de presence

Donc il est impossible de calculer proprement :

- `Buts/60`
- `PD/60`
- `INTS/60`
- `Contres/60`
- `PDB/60`
- etc.

Sans temps de jeu, toute valeur `/60` serait une approximation et ne doit pas etre affichee comme une metrique officielle.

### 2. Deux compteurs metier non exposes

Deux axes du notebook existent dans les donnees mais ne sont pas encore portes comme compteurs dedies dans l'API :

- `Sanctions obtenues`
- `7M obtenus`

On a bien les evenements en base :

- `Provoque une sanction`
- `Penalty obtenu`

Mais ces deux evenements ne sont pas encore transformes en compteurs explicites dans le pipeline analytics actuel.

## Logique A Integrer Dans L'Application

### V1 recommandee

Objectif : integrer une lecture "profil poste" fiable sans mentir sur le `/60`.

La v1 doit afficher :

1. un radar percentile de la joueuse face a son poste
2. un radar joueuse vs mediane du poste
3. un radar multi-joueuses sur un meme poste
4. un bar chart de niveau par axe

### Perimetre de cohorte

La cohorte de comparaison doit etre filtree par :

- `PositionId` obligatoire
- competition optionnelle
- equipe optionnelle
- saison optionnelle
- journee optionnelle
- match optionnel si besoin d'un scope ultra cible

La comparaison doit toujours etre faite sur des joueuses du meme poste et sur le meme perimetre de filtre.

### Axes V1 conseilles

Les axes suivants peuvent etre livres rapidement en version fiable :

- `Buts`
- `Passes decisives`
- `Interceptions`
- `Contres`
- `Neutralisations`
- `Pertes de balle`
- `7M concedes`
- `2 minutes`
- `Tirs loupes`
- `% reussite tir`

Les axes suivants doivent etre ajoutes via nouveaux compteurs :

- `Sanctions obtenues`
- `7M obtenus`

### Normalisation de la v1

En l'absence de temps de jeu, utiliser l'une de ces deux strategies :

- `par match`
- `percentile sur volume brut`

Recommandation produit :

- afficher `par match` quand cela a du sens
- conserver les percentiles sur la cohorte poste
- ne jamais afficher `/60` tant que le temps de jeu reel n'existe pas

### Logique de calcul pour chaque joueuse

Pour une joueuse selectionnee :

1. charger toutes les joueuses du meme poste et du meme scope
2. calculer les axes sur chaque joueuse
3. construire :
   `min`, `max`, `mediane`, `percentile`, `valeur joueuse`
4. pour les stats negatives, inverser le sens de lecture visuelle :
   `pertes`, `2 minutes`, `7M concedes`, `tirs loupes`

### Percentile

Le notebook utilise une logique simple :

- percentile = part des joueuses du poste inferieures a la valeur de la joueuse

Cette logique peut etre conservee.

### Radar joueuse vs mediane

Deux options possibles :

- radar sur valeurs normalisees `min-max` au sein du poste
- radar sur percentiles

Recommandation :

- utiliser des `percentiles` pour la lecture front, plus stables et plus comprehensibles
- garder `mediane`, `min`, `max` dans la reponse API pour enrichir les tooltips

## API A Prevoir

Creer un endpoint dedie du type :

- `GET /api/stats/players/{playerId}/position-profile`

Reponse attendue :

- identite joueuse
- poste
- taille de cohorte
- scope applique
- liste des axes
- valeur brute
- valeur par match si disponible
- mediane du poste
- percentile
- indicateur `higherIsBetter`

Pour le multi-joueuses :

- `POST /api/stats/compare/position-profile`

avec :

- `playerIds`
- filtres de scope
- option `mode = raw | perMatch`

## Front A Prevoir

L'application a deja les briques d'affichage :

- radar ApexCharts via [Compare.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Compare.razor:265)
- series radar via [Compare.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Compare.razor:994)
- comparaison normalisee via [Compare.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Compare.razor:968)
- service API de comparaison via [StatsApiClient.cs](C:/Users/donov/source/repos/HandWStat/Services/Api/StatsApiClient.cs:123)

Recommandation UI :

- integrer cela dans la page `Joueuses`
- ajouter un bloc `Profil poste`
- proposer ensuite une extension dans `Comparaison` pour comparer 2 a 4 joueuses sur les axes poste

## Conditions Pour Un Vrai Mode 60 Minutes

Le mode `/60` devient un vrai `feu vert` uniquement si l'on ajoute l'un des jeux de donnees suivants :

- temps de jeu cumule par joueuse et par match
- evenements d'entree / sortie fiables
- source externe importee avec `minutes jouees`

Quand cette donnee existera, la formule sera :

- `metrique_per_60 = metrique_brute * 60 / minutes_jouees`

## Decision Produit Recommandee

Decision recommandee a date :

- lancer une `v1 Profil Poste` dans l'application
- livrer en `percentile` + `valeur brute` + `par match`
- ajouter `7M obtenus` et `sanctions obtenues` dans l'API
- ne pas utiliser le libelle `/60`

Cela permet d'avoir une fonctionnalite utile, honnete, et directement exploitable sans attendre une refonte du modele de donnees.

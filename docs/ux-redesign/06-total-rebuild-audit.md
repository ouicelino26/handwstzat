# HandWStat - Audit de reconstruction totale

Date : 22 juin 2026.

## 1. Decision produit

L'interface existante n'est pas une reference visuelle. Elle sert uniquement de source
pour comprendre les fonctions, les donnees, les calculs, les permissions et les parcours.

La reconstruction doit remplacer :

- la structure de navigation ;
- les layouts public et authentifie ;
- la hierarchie des pages ;
- les cartes, tableaux, filtres, onglets et panneaux ;
- les composants analytiques ;
- l'identite native MAUI ;
- le theme, les tokens, les animations et les comportements responsive.

Les contrats API, les regles statistiques et les capacites fonctionnelles sont conserves.

## 2. Perimetre lu

Le depot contient 143 fichiers utiles hors `bin`, `obj` et `.git`.

| Zone | Fichiers |
| --- | ---: |
| Racine et configuration | 8 |
| Components | 55 |
| Configuration | 2 |
| Documentation | 13 |
| Models | 19 |
| Platforms | 16 |
| Properties | 1 |
| Resources | 6 |
| Services | 16 |
| wwwroot | 7 |

Le perimetre UI source contient 54 fichiers, le fichier minifie Bootstrap tiers etant
exclu du denominator de reconstruction.

La lecture a couvert :

- les 36 composants/pages Razor ;
- les 15 fichiers CSS et CSS isole source ;
- les 3 fichiers XAML ;
- le host HTML ;
- les 16 services et clients API ;
- les 19 fichiers de modeles locaux ;
- les DTO du projet `D:\repos\HandballManagerCore` ;
- les controleurs, services analytics et tests de `D:\repos\HandballManagerAPI`.

## 3. Architecture technique observee

```text
.NET MAUI 10
  -> MainPage.xaml
    -> BlazorWebView
      -> Routes.razor
        -> PublicLayout ou MainLayout
          -> pages Razor
            -> services de composition
              -> clients HTTP types
                -> HandballManagerAPI
                  -> DTO HandballManagerCore
```

Dependances de visualisation :

- ApexCharts ;
- jQWidgets BarGauge ;
- SVG maison pour terrains, radars et exports ;
- Blazor Bootstrap, faiblement utilise dans la presentation metier.

Etat client :

- session JWT en memoire ;
- donnees de reference mises en cache ;
- scope d'analyse partage par singleton ;
- recherche globale ;
- cache local de certains details de match ;
- pas de store normalise ni de persistance de session.

## 4. Routes et objectifs reels

| Route | Acces | Objectif |
| --- | --- | --- |
| `/` | Public | Connexion et entree produit |
| `/demo` | Public | Demonstration locale sans API |
| `/dashboard` | Authentifie | Pilotage global et classements |
| `/players` | Authentifie | Repertoire et fiche joueuse |
| `/compare` | Authentifie | Comparaison de 2 a 6 joueuses |
| `/position-profiles` | Authentifie | Profil par poste et cohorte |
| `/teams` | Authentifie | Bilan collectif, effectif et matchs |
| `/matches` | Authentifie | Liste et fiche match |
| `/not-found` | Mixte | Route introuvable |
| `/counter` | Technique | Reliquat du template, sans valeur produit |

## 5. Fonctions a conserver

### Session et acces

- connexion identifiant/mot de passe ;
- stockage du token pour la session courante ;
- roles `Admin` et `Consultation` en lecture ;
- deconnexion ;
- redirection vers la connexion ;
- acces public a la demonstration ;
- etats 401/403, erreur et chargement.

### Scope et filtres

- competition ;
- equipe ;
- poste ;
- match ;
- annee ;
- saison ;
- journee ;
- date de debut et date de fin dans les contrats ;
- attaque ;
- defense ;
- declencheur ;
- zone/teinte de tir ;
- recherche textuelle locale ;
- reset ;
- propagation du scope global entre modules.

### Dashboard

- vue d'ensemble matchs/equipes/joueuses/actions ;
- metriques de classement configurables ;
- taille de top configurable ;
- spotlight joueuse ;
- historique des clubs ;
- carte spatiale ;
- tableaux globaux champ et gardiennes ;
- filtre poste ;
- tri des colonnes ;
- matchs recents.

### Joueuses

- repertoire filtre et recherche avec debounce ;
- lien profond `playerId` ;
- fiche identite, equipe, poste et contexte ;
- synthese globale ;
- attaque ;
- defense ;
- passes et pertes ;
- sanctions ;
- gardienne ;
- technique ;
- tendances ;
- scatter ;
- zones de but et de declenchement ;
- historique des matchs ;
- tri et recherche de matchs ;
- export SVG fiche et radar.

### Comparaison

- 2 a 6 joueuses ;
- recherche et remplacement par slot ;
- scope commun ;
- KPI par joueuse ;
- radar ;
- volumes ;
- efficacite ;
- technique ;
- production par match ;
- tableau comparatif et coloration relative ;
- modes Club, Analyste et Joueuse.

### Profil poste

- repertoire filtre ;
- selection d'une joueuse ;
- cohorte du meme poste ;
- synthese coach ;
- forces, fragilites, role, tactique et alertes ;
- valeurs brutes contre mediane ;
- radar normalise ;
- comparaison multi-joueuses ;
- masquage individuel des series ;
- scatter ;
- tableau des axes ;
- export CSV ;
- export radar SVG ;
- export fiche SVG ;
- copie du resume analyste.

### Equipes

- selection et lien profond `teamId` ;
- bilan victoires/nuls/defaites ;
- buts pour et contre ;
- contribution directe ;
- production collective ;
- tops internes ;
- profil collectif ;
- impact offensif ;
- effectif filtre, recherche et tri ;
- cartes de matchs ;
- navigation vers la fiche match.

### Matchs

- liste filtre et recherche ;
- lien profond `matchId` ;
- score final et identite des equipes ;
- scope deux equipes/equipe 1/equipe 2 ;
- resume ;
- timeline ;
- runs, changements de leader et moments cles ;
- comparaison d'equipes ;
- profil du match ;
- joueuses decisives ;
- zones spatiales ;
- effectif filtre et trie ;
- cache du detail.

## 6. Regles metier confirmees

### Temps de jeu et normalisation

- une statistique `/60` vaut `valeur * 60 / minutes jouees` ;
- si les minutes sont nulles ou negatives, le resultat vaut 0 ;
- les minutes sont agregees par joueuse et par match ;
- `MatchesWithPlayingTime` est distinct de `MatchesPlayed` ;
- la moyenne de temps est calculee sur les matchs avec temps renseigne.

### Taux

- taux = succes / total * 100 ;
- taux de tir ouvert = buts hors penalty / buts hors penalty + tirs rates ;
- taux penalty = buts penalty / buts penalty + penalties rates ;
- taux gardienne = arrets / arrets + buts encaisses, penalties inclus selon le contrat.

### Cohorte poste

- le poste de la joueuse ancre la cohorte ;
- le filtre equipe est volontairement supprime pour calculer la cohorte ;
- competition, match, dates, annee, saison et journee sont conserves ;
- seules les joueuses avec un temps strictement positif entrent dans la cohorte ;
- si la cohorte est vide, un fallback controle utilise les joueuses demandees avec temps ;
- la mediane est la moyenne des deux valeurs centrales pour une cohorte paire ;
- les bornes min/max viennent de la cohorte ;
- une cohorte plate recoit une borne synthetique utile et ne retombe pas a 50.

### Percentile

- le backend retourne deja un percentile favorable ;
- pour un axe positif, une valeur haute donne un percentile haut ;
- pour un axe negatif, une valeur basse donne un percentile haut ;
- le frontend ne doit jamais inverser une seconde fois ce percentile ;
- les tons backend sont positif >= 75, bon >= 55, vigilance >= 35, danger sinon.

### Axes joueuses de champ

- buts dans le jeu /60 ;
- passes decisives /60 ;
- sanctions obtenues /60 ;
- penalties obtenus /60 ;
- pertes /60, axe negatif ;
- interceptions /60 ;
- contres /60 ;
- neutralisations /60 ;
- penalties concedees /60, axe negatif ;
- deux minutes /60, axe negatif ;
- tirs rates /60, axe negatif ;
- taux de tir dans le jeu.

### Axes gardiennes

- arrets /60 ;
- arrets 7m /60 ;
- taux d'arret ;
- tirs subis /60, axe negatif dans le contrat actuel ;
- buts encaisses /60, axe negatif ;
- passes /60 ;
- pertes /60, axe negatif ;
- sanctions /60, axe negatif.

### Spatial

- une requete sans `teamId` retourne les deux equipes d'un match ;
- une requete avec `teamId` limite la carte a cette equipe ;
- les zones absentes peuvent etre completees avec des valeurs nulles/zero ;
- la selection visuelle d'une zone ne modifie pas les totaux sources.

### Comparaison

- les identifiants sont positifs, distincts et partagent un meme scope ;
- les donnees globales, attaque, defense, passe, sanctions, gardienne et technique
  appartiennent au meme resultat de comparaison ;
- le nombre de slots UI est compris entre 2 et 6.

### Match

- la timeline vient des evenements tries chronologiquement ;
- les scores peuvent etre completes par le dernier evenement si le score final manque ;
- changements de leader, runs et mi-temps sont derives des evenements ;
- le scope equipe doit piloter de facon coherente graphiques, tableaux et spatial.

## 7. Faiblesses de l'experience actuelle

- navigation organisee par pages techniques, pas par taches ;
- sidebar classique occupant de l'espace sans fournir de contexte ;
- duplication des filtres dans presque chaque page ;
- pages de 900 a 2 200 lignes melant orchestration, rendu, calcul et export ;
- empilement de panneaux et de cartes produisant une lecture uniforme ;
- trop d'onglets internes sans continuites entre entites ;
- filtres au-dessus du contenu avant toute information utile ;
- tableaux desktop peu adaptes au tactile ;
- graphiques souvent isoles de la question metier ;
- recherche globale recente mais encore peripherique ;
- aucun espace de travail recent/favori ;
- pas de plateau de comparaison persistant ;
- identite native MAUI encore issue du template .NET violet ;
- CSS global de plus de 5 800 lignes, avec plusieurs generations visuelles superposees ;
- fichiers CSS isoles presque vides et responsabilites visuelles centralisees ;
- absence de tests UI et de snapshots visuels.

## 8. Conclusion de l'audit

Le probleme ne peut pas etre resolu par un nouveau theme. La reconstruction doit changer
le modele mental :

```text
Avant : menu -> page -> filtres -> panneaux -> sous-onglets
Apres : contexte -> entite ou question -> scene analytique -> decision/action
```

Les capacites metier sont suffisamment decouplees des vues pour permettre cette
reconstruction sans changer les contrats API.

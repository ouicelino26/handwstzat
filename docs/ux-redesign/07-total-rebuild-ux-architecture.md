# HandWStat - Architecture d'experience cible

## 1. Vision

Le produit cible est un **Performance Operating System** pour analystes, staffs et
joueuses. Il ne ressemble pas a un back-office compose de pages. Il ressemble a un poste
de pilotage ou le contexte, les entites et les analyses restent continus.

Nom de travail de l'experience : **HandWStat Studio**.

Principes :

1. commencer par une question ou un signal, pas par un formulaire ;
2. garder le scope visible et stable ;
3. permettre de passer d'une ligue a une equipe, une joueuse ou un match sans rupture ;
4. montrer d'abord le sens, puis la preuve statistique ;
5. rendre chaque visualisation actionnable ;
6. adapter la densite a Club, Analyste ou Joueuse ;
7. ne jamais cacher la qualite ou la taille de l'echantillon.

## 2. Nouvelle structure globale

La sidebar actuelle est supprimee.

### Desktop

```text
+--------------------------------------------------------------------------+
| BRAND | Recherche universelle | Scope actif | + Analyser | Compte         |
+-------+------------------------------------------------------------------+
| RAIL  | BREADCRUMB / TITRE / ACTIONS CONTEXTUELLES                        |
| 72 px +------------------------------------------------+-----------------+
|       |                                                | CONTEXT LENS    |
| Home  |            ANALYTICS STAGE                     | selection,      |
| Athl. |                                                | insights,       |
| Clubs |  contenu asymetrique, tableaux, graphiques,    | methodologie,   |
| Match |  timeline ou spatial selon la question         | raccourcis      |
| Lab   |                                                |                 |
|       |                                                |                 |
+-------+------------------------------------------------+-----------------+
| RECENT STRIP / COMPARISON TRAY / STATUS                                  |
+--------------------------------------------------------------------------+
```

Zones :

- **Command Bar** : recherche, scope, nouvelle analyse, compte et theme ;
- **Domain Rail** : cinq domaines maximum, uniquement des icones et labels courts ;
- **Analytics Stage** : scene principale, largeur et composition variables ;
- **Context Lens** : panneau de detail contextuel, repliable ;
- **Activity Tray** : recents, favoris et comparaison en cours.

### Tablette

- rail reduit ;
- panneau contextuel en drawer lateral ;
- barre de scope sur une ligne scrollable ;
- activity tray repliable.

### Mobile

- command bar compacte ;
- dock inferieur a cinq domaines ;
- contexte dans une bottom sheet ;
- filtres dans une sheet plein ecran ;
- tableaux rendus en cartes de donnees ;
- comparaison dans un tray au-dessus du dock.

## 3. Nouvelle navigation

### Domaines primaires

| Domaine | Destination | Contenu |
| --- | --- | --- |
| Today | `/dashboard` | cockpit, signaux, recents |
| Athletes | `/players` | repertoire et fiche |
| Squads | `/teams` | equipes et effectifs |
| Games | `/matches` | rencontres et fiches match |
| Lab | `/compare` | comparaison et profil poste |

`/position-profiles` reste une route historique et un lien profond, mais son experience
est integree au Lab et a la fiche joueuse.

### Recherche universelle

`Ctrl+K` ouvre une surface plein cadre et non une petite modale :

- recherche joueuses, equipes et matchs ;
- commandes `aller a`, `changer le scope`, `comparer`, `exporter` ;
- recents et favoris ;
- navigation clavier ;
- apercu du resultat selectionne.

### Navigation contextuelle

- clic sur une joueuse : ouvre sa fiche dans la scene ;
- action `Comparer` : ajoute au tray sans quitter la scene ;
- clic sur une equipe : conserve saison/competition ;
- clic sur un match : ouvre la fiche et memorise le retour ;
- breadcrumb court et retour vers la liste avec position conservee.

## 4. Identite visuelle

Direction : **editorial sports intelligence**, pas dashboard generique.

### Palette

- `Ink` : graphite bleute profond ;
- `Paper` : blanc mineral chaud ;
- `Signal` : orange vif reserve aux actions et alertes actives ;
- `Court` : vert terrain des visualisations spatiales ;
- `Electric` : bleu froid pour comparaison et reference ;
- `Success`, `Warning`, `Danger` : semantiques mesurees ;
- couleurs de series stables entre tous les graphiques.

### Typographie

- titres : famille display etroite, expressive et sportive ;
- texte : sans serif tres lisible ;
- nombres : chiffres tabulaires ;
- labels : capitales courtes, espacement augmente ;
- aucune dependance a Arial, Inter ou Roboto.

### Composition

- grilles asymetriques ;
- grands chiffres editoriaux ;
- surfaces plus plates, moins de cartes imbriquees ;
- bordures fines plutot que grosses ombres ;
- rayon controle, non applique uniformement ;
- lignes de terrain et micro-trames en arriere-plan ;
- espace reserve aux donnees, pas au chrome.

### Mouvement

- entree de scene 180 ms ;
- changement de scope avec skeleton structurel ;
- transition de selection entre liste et context lens ;
- animation de donnees seulement si elle aide a percevoir un changement ;
- zero mouvement decoratif permanent ;
- parite `prefers-reduced-motion`.

## 5. Ecrans cibles

### 5.1 Connexion

Nouvelle composition :

- plein ecran editorial partage en deux plans ;
- a gauche, manifeste produit et visualisation vivante locale ;
- a droite, panneau de connexion minimal ;
- acces demo distinct ;
- statut serveur discret ;
- aucune carte SaaS centree generique.

Conserve : identifiant, mot de passe, connexion, demo, erreurs, redirection.

### 5.2 Demo

Devient une visite guidee interactive en trois actes :

1. comprendre un signal ;
2. explorer une joueuse ;
3. lire une zone de tir.

Les donnees locales restent celles de `DemoDataFactory`.

### 5.3 Today / Cockpit

Le dashboard n'est plus decoupe en quatre tabs.

Scene :

- `Morning brief` : scope, volume analyse et dernier rafraichissement ;
- `Signals` : trois a cinq variations ou points de vigilance ;
- `Performance pulse` : quatre grands KPI avec contexte ;
- `Leaderboard` : metrique active, top configurable et acces a la table complete ;
- `Spotlight` : joueuse actionnable, ajouter a comparaison ;
- `Recent games` : timeline horizontale ;
- `Data confidence` : matchs, temps renseigne et cohorte.

Les tableaux complets deviennent une vue de scene secondaire, pas un bloc en bas de page.

### 5.4 Athletes / Repertoire

Vue par defaut :

- recherche instantanee ;
- filtres dans une sheet ;
- compteur et scope ;
- data grid virtualisable ;
- colonnes configurables ;
- selection multiple ;
- actions comparer, ouvrir et exporter ;
- cartes compactes sur mobile.

La liste repliee actuelle disparait.

### 5.5 Athlete Workspace

Header d'entite :

- nom, numero, poste, equipe, nationalite ;
- temps de jeu et qualite d'echantillon ;
- actions `Comparer`, `Exporter`, `Favori`.

Scenes :

- `Brief` : KPI et lecture synthetique ;
- `Performance` : attaque, defense, creation, maitrise, discipline ;
- `Trajectory` : tendances et matchs ;
- `Court` : zones et declenchements ;
- `Role` : profil poste integre ;
- `Evidence` : tableau detaille et historique.

Le changement de scene ne recharge pas l'identite ni le scope.

### 5.6 Squads

Deux niveaux :

- repertoire des equipes ;
- workspace equipe.

Workspace :

- bilan et forme ;
- contribution collective ;
- effectif ;
- tendances ;
- matchs ;
- top contributrices ;
- acces contextuels aux joueuses et matchs.

Le detail de match mort dans l'ancienne page n'est pas reproduit. La fiche match dediee
est la seule source d'analyse d'une rencontre.

### 5.7 Games

Repertoire :

- calendrier/liste ;
- filtres persistants ;
- statut et score ;
- recherche ;
- conservation du scroll.

Game Room :

- scoreboard editorial ;
- timeline principale ;
- moments cles ;
- comparaison des equipes ;
- scope equipe global ;
- court map ;
- joueuses ;
- tableau complet ;
- notes de qualite.

### 5.8 Lab

Le Lab regroupe deux outils.

#### Compare

- tray persistant 2 a 6 joueuses ;
- selection explicite, jamais automatique silencieuse ;
- scope verrouillable ;
- modes Club, Analyste et Joueuse ;
- vues radar, volume, efficacite, technique, par match et table ;
- masquage de series ;
- ordre reconfigurable ;
- export futur sans perdre les donnees actuelles.

#### Role Benchmark

- joueuse ancre ;
- cohorte affichee comme objet de contexte ;
- badge de taille et temps ;
- radar normalise ;
- histogramme brut ;
- axes tries ;
- shortlist multi-radar ;
- exports regroupes dans un menu unique.

## 6. Bibliotheque cible

### Structure

- `StudioShell`
- `DomainRail`
- `CommandBar`
- `ContextLens`
- `ActivityTray`
- `EntityHeader`
- `SceneTabs`
- `ScopeChipBar`
- `FilterSheet`

### Donnees

- `MetricHero`
- `SignalCard`
- `KpiStrip`
- `InsightBlock`
- `DataConfidence`
- `AnalyticsTable`
- `DataCardList`
- `ComparisonTray`

### Visualisations

- `ChartStage`
- `RadarStudio`
- `CourtMap`
- `MatchTimeline`
- `DistributionPlot`
- `CohortHistogram`
- `ChartDataFallback`

### Feedback

- `StudioSkeleton`
- `EmptyScene`
- `ErrorScene`
- `AccessScene`
- `ToastRegion`
- `ConfirmSheet`
- `ProgressBeacon`

## 7. Tableaux

Les tableaux actuels sont conserves fonctionnellement mais reconstruits :

- en-tete sticky ;
- tri accessible ;
- recherche ;
- filtres ;
- colonnes configurables ;
- densite ;
- actions de ligne ;
- selection ;
- export lorsque disponible ;
- virtualisation ou pagination pour grands jeux ;
- rendu carte mobile ;
- clavier ;
- alternative aux colorations seules.

Le tri multi-colonnes et la virtualisation exigent une couche de composant et pourront
etre actives progressivement sans modifier les formules.

## 8. Graphiques

Chaque graphique porte :

- une question metier ;
- une unite ;
- un scope ;
- une taille d'echantillon ;
- une legende interactive ;
- des tooltips ;
- un tableau alternatif ;
- un etat vide et donnees insuffisantes ;
- un export si la fonction existe deja.

Le zoom et le drill-down sont reserves aux series temporelles et vues spatiales utiles.

## 9. Accessibilite

- WCAG 2.2 AA ;
- focus visible et logique ;
- navigation clavier complete ;
- `aria-live` pour changement de scope ;
- `aria-sort` sur les tableaux ;
- noms accessibles sur zones SVG ;
- alternatives textuelles aux graphiques ;
- cibles tactiles 44 px ;
- contraste verifie en clair et sombre ;
- zoom 200 % ;
- reduced motion.

## 10. Difference radicale avec l'existant

| Existant | Cible |
| --- | --- |
| Sidebar large | Rail de domaines + command bar |
| Pages independantes | Workspaces d'entites continus |
| Filtres au-dessus de chaque page | Scope persistant + filter sheet |
| Tabs techniques | Scenes orientees questions |
| Cartes empilees | Composition editoriale asymetrique |
| Comparaison comme formulaire | Tray de laboratoire persistant |
| Profil poste isole | Role Benchmark dans le Lab et la fiche |
| Match liste/detail dans une page | Game directory + Game Room |
| Mobile compresse | Navigation et surfaces tactiles dediees |
| Identite template .NET | Identite HandWStat Studio native et web |

Si une capture de la cible rappelle la structure actuelle, le lot est refuse.

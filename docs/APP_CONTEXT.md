# Contexte Application HandWStat

Ce document sert de reference rapide sur l'application : role des pages, logique UX, langage visuel et regles de lecture des donnees.

## 1. Positionnement Produit

HandWStat est un cockpit d'analyse handball oriente lecture rapide, comparaison et decision.

L'application cherche a :

- donner du contexte avant le chiffre
- limiter la surcharge visuelle
- garder les selections stables
- rendre les comparaisons explicites
- proposer une lecture staff, analyste et operationnelle

La logique generale est "une page = une intention de lecture".

## 2. Principes UX Communs

Les ecrans suivent presque tous les memes regles :

- un bandeau d'entete avec `section-kicker`, titre fort et sous-titre court
- des filtres regroupes dans des `details` repliables pour liberer de la place
- des cartes dans des `page-panel` pour isoler chaque bloc de lecture
- des badges et chips pour afficher l'information secondaire sans surcharger
- des tableaux seulement quand la lecture fine est necessaire
- des graphes places dans une hierarchie claire, jamais seuls sans contexte
- une version mobile qui replie les grilles en colonne unique

L'UX privilegie :

- les etats de selection visibles
- les blocs repliables quand ils ne sont pas centraux
- les infos metier nommees clairement
- les donnees les plus importantes en haut, les details en bas

## 3. Langage Visuel

L'identite visuelle repose sur des cartes douces, des bordures colorees et des surfaces claires.

Reperes de style :

- cartes arrondies avec ombre legere
- haut de carte marque par une ligne couleur accent
- fonds avec gradients discrets, surtout sur les panneaux analytiques
- textes de section en majuscules via `section-kicker`
- chiffres et KPI mis en avant avec du gras et des badges
- drawers / accordions avec chevron pour liberer de la place
- palette chart unique pour garder une lecture coherente partout

Les composants partages portent ce vocabulaire :

- `page-panel`
- `filter-strip`
- `filter-drawer`
- `player-list-drawer`
- `scope-summary-bar`
- `kpi-tile`
- `gauge-kpi-card`
- `match-card`
- `state-card`

Reference de style globale :

- [wwwroot/app.css](../wwwroot/app.css)
- [Components/Shared/ChartPalette.cs](../Components/Shared/ChartPalette.cs)

## 4. Palette Charts

Les graphes utilisent une palette centralisee pour que chaque couleur ait un role lisible.

Roles principaux :

- `Player` : joueuse comparee
- `Reference` : mediane ou repere
- `Primary` : couleur de structuration
- `Secondary` : comparaison secondaire
- `Positive` : signal fort / favorable
- `Warning` : vigilance
- `Danger` : alerte
- `Accent` : accent complementaire
- `Info` : information ou contexte

Couleurs de base dans `ChartPalette` :

- `Player` = `#e85d24`
- `Reference` = `#94a3b8`
- `Primary` = `#0f766e`
- `Secondary` = `#2563eb`
- `Positive` = `#16a34a`
- `Warning` = `#d97706`
- `Danger` = `#dc2626`
- `Accent` = `#7c3aed`
- `Info` = `#0891b2`

Regle simple :

- une couleur = un role
- la joueuse reste toujours identifiee par la meme couleur
- la reference garde toujours une teinte neutre
- les couleurs semantiques doivent rester explicites et homogenes

## 5. Carte Des Pages

| Page | Role principal | Lecture UX |
| --- | --- | --- |
| [Home](../Components/Pages/Home.razor) | Landing et entree dans l'app | Hero, promesse produit, acces live ou mode demo |
| [Dashboard](../Components/Pages/Dashboard.razor) | Vue globale de la ligue | Filtre, hero, KPI, classements, spotlight, spatial, derniers reperes |
| [Compare](../Components/Pages/Compare.razor) | Comparaison de plusieurs joueuses | Filtres, contexte, selection, radar, volumes, efficacite, technique |
| [Players](../Components/Pages/Players.razor) | Fiche joueuse detaillee | Repertoire pliable, fiche centrale, onglets, graphs, zones, matchs |
| [PositionProfiles](../Components/Pages/PositionProfiles.razor) | Profil par poste | Filtre de contexte, mediane de poste, radar, histogramme, scatter, comparaison |
| [Matches](../Components/Pages/Matches.razor) | Lecture d'un match | Liste de matchs, detail, comparaison d'equipe, spatial, joueuses decisives |
| [Teams](../Components/Pages/Teams.razor) | Lecture d'une equipe | Filtres, KPI equipe, roster, matchs, classement, blocs de contexte |
| [Demo](../Components/Pages/Demo.razor) | Apercu public | Resume clair de la promesse produit avant connexion |
| [NotFound](../Components/Pages/NotFound.razor) | Page de secours | Redirection vers le cockpit principal |

## 6. Lecture Par Page

### Home

La page d'accueil sert de vitrine produit.

- met en avant la promesse de cockpit analytique
- montre un apercu clair du produit
- distingue mode demo et acces live
- reste simple, aeree et peu chargee

### Dashboard

Le dashboard est la vue globale de pilotage.

- hero de synthese
- filtres de contexte
- KPI de selection
- classements globaux
- mise en avant d'une joueuse suivie
- blocs de lecture spatiale et derniers reperes

Le but est de comprendre la situation generale avant d'entrer dans une fiche.

### Compare

La page compare sert a mettre plusieurs joueuses cote a cote.

- filtres de perimetre
- selection explicite des joueuses
- bloc de contexte pour rappeler le cadre compare
- radar compare
- volumes et efficacite
- lecture technique et detaillee

La page doit rester lisible meme avec plusieurs joueuses, donc les couleurs et l'ordre des slots doivent rester stables.

### Players

La page joueurs est une fiche detaillee avec un repertoire a gauche et la fiche a droite.

- la liste des joueuses est repliable pour gagner de la place
- la fiche detaillee occupe le centre de la lecture
- les sous-onglets structurent la profondeur :
  - `Synthese`
  - `Analyse`
  - `Graphes`
  - `Zones`
  - `Matchs`
- les cartes KPI restent courtes et tres lisibles
- les graphes servent la lecture metier, pas l'effet visuel

Cette page doit favoriser la navigation rapide entre joueuses sans perdre le contexte.

### PositionProfiles

La page profil poste est une page specialisee pour comparer une joueuse a la mediane de son poste.

- filtre de contexte en haut
- repertoire de joueuses repliable
- resume de contexte avant les graphes
- histogramme : valeur brute joueuse vs mediane brute du poste
- radar : forme normalisee sur une echelle commune
- comparaison multi-joueuses du meme poste
- scatter pour visualiser l'ecart axe par axe
- tableau detaille pour la lecture fine

Cette page doit rester tres dense, mais sans bloc inutile.

### Matches

La page matchs sert a lire un match dans le detail.

- filtre et liste des matchs
- zone de detail du match selectionne
- comparaison des deux equipes
- profil de match
- joueuses decisives
- lecture spatiale et zone active
- tableau des joueuses du match

L'objectif est de relier le score, les volumes et les actions importantes.

### Teams

La page equipes sert a lire un collectif dans son contexte.

- filtre compact
- KPI d'equipe
- lecture du roster
- lecture des matchs et du rythme
- classements et indicateurs de contexte

### Demo

La page demo sert de version demonstrative.

- elle montre le langage produit
- elle reste plus courte que les pages live
- elle rassure avant l'acces complet

## 7. Composants Partages

Ces composants structurent l'UX et doivent rester coherents entre les pages :

| Composant | Role |
| --- | --- |
| [ScopeSummaryBar](../Components/Shared/ScopeSummaryBar.razor) | Resumer le cadre commun et les KPI de contexte |
| [PlayerList](../Components/Shared/PlayerList.razor) | Liste de joueuses pliable / depliable |
| [PositionFilters](../Components/Shared/PositionFilters.razor) | Filtres specialises pour le profil poste |
| [PositionProfileHistogram](../Components/Shared/PositionProfileHistogram.razor) | Comparaison joueuse vs mediane du poste |
| [PositionRadarChart](../Components/Shared/PositionRadarChart.razor) | Radar normalise du profil poste |
| [MultiRadar](../Components/Shared/MultiRadar.razor) | Radar multi-joueuses sur une meme echelle |
| [ScatterChart](../Components/Shared/ScatterChart.razor) | Lecture axe par axe avec repere mediane |
| [CoachCards](../Components/Shared/CoachCards.razor) | Synthese courte pour lecture staff |
| [DetailedTable](../Components/Shared/DetailedTable.razor) | Lecture detaillee des axes et valeurs |
| [MatchCard](../Components/Shared/MatchCard.razor) | Carte de match resumee et actionnable |
| [GoalKpi](../Components/Shared/GoalKpi.razor) | Vue spatiale / but / declenchements |
| [BarGaugeKpiCard](../Components/Shared/BarGaugeKpiCard.razor) | KPI en jauge ou progression |

## 8. Regles De Donnees Et De Lecture

Quelques regles doivent rester vraies partout dans l'app :

- les KPI doivent garder un contexte de lecture quand le volume seul ne suffit pas
- les graphs doivent partager la meme palette et le meme vocabulaire
- un radar compare la forme, pas seulement la valeur brute
- un histogramme peut montrer la valeur brute face a une mediane de poste
- un scatter sert a voir l'ecart entre joueuse et reference axe par axe
- une comparaison doit respecter l'ordre des joueuses choisi par l'utilisateur
- les blocs redondants doivent etre evites si le contexte est deja donne par le header ou par une barre de synthese

Sur le profil poste :

- la mediane doit etre celle du poste, pas celle de la joueuse
- le radar et l'histogramme doivent raconter la meme histoire avec deux lectures differentes
- le multi radar doit garder la meme echelle pour toutes les joueuses

## 9. Regles De Redaction UX

Le ton de l'app doit rester :

- clair
- court
- metier
- explicite
- sans jargon inutile

Preferer :

- "Cadre commun"
- "Comparaison"
- "Lecture detaillee"
- "Lecture analyste"
- "Filtres"

Eviter :

- les redondances
- les blocs "lecture rapide" qui ne rajoutent pas d'information
- les titres trop longs
- les explications qui dupliquent un graphe deja lisible

## 10. Fichiers De Reference

Pour faire evoluer l'UX sans casser la coherence, les fichiers les plus importants sont :

- [wwwroot/app.css](../wwwroot/app.css)
- [Components/Shared/ChartPalette.cs](../Components/Shared/ChartPalette.cs)
- [Components/Shared/ScopeSummaryBar.razor](../Components/Shared/ScopeSummaryBar.razor)
- [Components/Shared/PlayerList.razor](../Components/Shared/PlayerList.razor)
- [Components/Shared/PositionFilters.razor](../Components/Shared/PositionFilters.razor)
- [Components/Pages/Dashboard.razor](../Components/Pages/Dashboard.razor)
- [Components/Pages/Compare.razor](../Components/Pages/Compare.razor)
- [Components/Pages/Players.razor](../Components/Pages/Players.razor)
- [Components/Pages/PositionProfiles.razor](../Components/Pages/PositionProfiles.razor)
- [Components/Pages/Matches.razor](../Components/Pages/Matches.razor)
- [Components/Pages/Teams.razor](../Components/Pages/Teams.razor)

Ce document doit etre mis a jour quand le langage UX ou la carte des pages change de facon durable.

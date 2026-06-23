# HandWStat - Architecture UX cible et wireframes

## 1. Vision cible

HandWStat doit devenir un espace de travail analytique plutot qu'une collection de pages.
L'utilisateur choisit un contexte une fois, navigue entre les entites sans le perdre et
peut approfondir progressivement.

Principes :

- `Contexte d'abord` : competition, saison, journee et equipe visibles partout.
- `Entite ensuite` : joueuse, equipe ou match.
- `Lecture progressive` : resume, analyse, donnees detaillees.
- `Une action principale par zone`.
- `Clavier et recherche pour les utilisateurs intensifs`.
- `Aucune metrique sans unite, population et periode`.

## 2. Nouvelle architecture de navigation

```text
Accueil analytique
├── Dashboard
├── Joueuses
│   ├── Repertoire
│   ├── Fiche joueuse
│   └── Profil poste
├── Equipes
│   ├── Repertoire
│   └── Fiche equipe
├── Matchs
│   ├── Calendrier / liste
│   └── Fiche match
├── Comparer
│   ├── Comparaison libre
│   └── Shortlists sauvegardees
└── Plus
    ├── Exports recents
    ├── Favoris
    ├── Raccourcis
    └── Preferences
```

Le profil poste devient un onglet de la fiche joueuse tout en conservant une entree
directe pour les workflows staff.

## 3. Shell cible

### Desktop

```text
┌──────────────────────────────────────────────────────────────────────────────┐
│ Logo  Recherche globale Ctrl+K      Scope: Ligue | 24/25 | J12     Aide User│
├──────────────┬───────────────────────────────────────────────────────────────┤
│ Dashboard    │ Breadcrumbs                                  Actions contexte│
│ Joueuses     │ Titre de page + resume du scope                              │
│ Equipes      ├───────────────────────────────────────────────────────────────┤
│ Matchs       │                                                               │
│ Comparer     │                    Contenu de la route                        │
│              │                                                               │
│ Favoris      │                                                               │
│ Recents      │                                                               │
│              │                                                               │
│ Theme/Logout │                                                               │
└──────────────┴───────────────────────────────────────────────────────────────┘
```

### Mobile

```text
┌─────────────────────────────┐
│ Scope compact     Recherche │
├─────────────────────────────┤
│ Titre + action principale   │
│ Chips de filtres actifs     │
│                             │
│ Contenu en cartes empilees  │
│                             │
├─────────────────────────────┤
│ Accueil Joueuses Matchs Plus│
└─────────────────────────────┘
```

Le rail mobile ne doit pas contenir six destinations compressees. `Comparer`, `Equipes`,
favoris et preferences peuvent etre accessibles via `Plus`, avec possibilite d'epingler
une destination.

## 4. Scope global

Le scope global contient :

- competition ;
- saison ;
- journee ou plage de dates ;
- equipe facultative ;
- indicateur de donnees disponibles ;
- bouton `Reinitialiser` ;
- bouton `Enregistrer la vue`.

Regles :

- le scope persiste entre pages ;
- un filtre local peut le completer mais pas le modifier silencieusement ;
- les chips indiquent clairement les exceptions locales ;
- un changement lourd est applique en une transaction ;
- l'URL encode le scope partageable.

## 5. Palette de commandes

Raccourci : `Ctrl+K`.

Groupes de resultats :

- joueuses ;
- equipes ;
- matchs ;
- commandes ;
- vues sauvegardees ;
- exports recents.

Commandes initiales :

- ouvrir une joueuse ;
- comparer la joueuse courante ;
- ouvrir le dernier match ;
- changer de saison ;
- basculer le theme ;
- exporter la vue ;
- copier le lien ;
- reinitialiser les filtres ;
- afficher les raccourcis.

## 6. Ecrans cibles

### 6.1 Dashboard

Objectif : repondre en moins de dix secondes a trois questions :

1. que se passe-t-il dans le scope ?
2. qui ressort ?
3. ou approfondir ?

```text
┌ Scope global ────────────────────────────────────────────────────────────────┐
├ 4 KPI majeurs ───────────────────────────────────────────────────────────────┤
│ Production | Efficacite | Maitrise | Discipline                              │
├──────────────────────────────┬───────────────────────────────────────────────┤
│ Classement actif             │ Alertes et variations                         │
│ metrique + top + poste       │ hausse, baisse, donnees incompletes           │
├──────────────────────────────┼───────────────────────────────────────────────┤
│ Joueuse spotlight            │ Matchs recents                                │
│ action: ouvrir / comparer    │ action: ouvrir le match                       │
├──────────────────────────────┴───────────────────────────────────────────────┤
│ Tableaux globaux, repliables ou accessibles en vue dediee                    │
└──────────────────────────────────────────────────────────────────────────────┘
```

Changements :

- fusionner les quatre tabs en une page de synthese courte ;
- garder les classements complets dans une vue secondaire ;
- rendre la joueuse spotlight actionnable ;
- afficher les anomalies de donnees et tailles d'echantillon.

### 6.2 Repertoire joueuses

```text
┌ Recherche ─ Filtres ─ Vue table/cartes ─ Colonnes ─ Export ──────────────────┐
│ Nom       Equipe       Poste       Matchs       Buts/60       Forme           │
│ ... lignes virtualisees ...                                                   │
└──────────────────────────────────────────────────────────────────────────────┘
```

Fonctions :

- recherche instantanee ;
- filtres avances ;
- tri multi-colonne ;
- colonnes configurables ;
- densite compacte/confort ;
- selection multiple ;
- action `Comparer` ;
- favoris ;
- vues sauvegardees ;
- pagination serveur ou virtualisation.

### 6.3 Fiche joueuse

```text
┌ Identite | equipe | poste | temps de jeu | actions: comparer, exporter, favori┐
├ Synthese | Performance | Profil poste | Zones | Matchs | Historique           ┤
├ KPI essentiels ───────────────────────────────────────────────────────────────┤
├ Tendance recente ────────────────┬ Forces / alertes ──────────────────────────┤
├ Matchs recents ──────────────────┼ Zone principale ───────────────────────────┤
└ Notes de methode et echantillon ──────────────────────────────────────────────┘
```

Correspondance avec l'existant :

- `Synthese` conserve KPI et jauges ;
- `Performance` fusionne Analyse et Graphes ;
- `Profil poste` integre le module specialise ;
- `Zones` reste dedie ;
- `Matchs` conserve recherche, tri et cartes ;
- `Historique` porte les clubs et changements de contexte.

### 6.4 Profil poste

```text
┌ Cohorte: poste | competition | saison | N joueuses | minutes admissibles ─────┐
├ Resume coach ─────────────────────────────────────────────────────────────────┤
│ 2 forces | 2 vigilances | adequation au role | alerte echantillon             │
├ Radar normalise ────────────────┬ Valeurs brutes vs mediane ──────────────────┤
├ Comparaison shortlist ──────────┴──────────────────────────────────────────────┤
├ Tableau axes : valeur | mediane | percentile favorable | min/max | definition │
└ Export ▾ : CSV | Radar | Fiche | Copier resume                                │
```

Regles :

- afficher explicitement `percentile favorable` ;
- afficher les minutes et matchs avec temps ;
- avertir si cohorte faible ;
- ne jamais utiliser une valeur neutre artificielle sans badge `donnee insuffisante` ;
- un menu d'export unique remplace quatre boutons egaux.

### 6.5 Comparaison

```text
┌ Shortlist [A] [B] [+ Ajouter]      Scope commun       Enregistrer             │
├ Resume des ecarts clefs ──────────────────────────────────────────────────────┤
├ Radar | Volumes | Efficacite | Technique | Par 60 ────────────────────────────┤
├ Tableau configurable avec surbrillance relative ──────────────────────────────┤
└ Notes de comparabilite : poste, minutes, matchs, cohorte                      │
```

Ameliorations :

- selection vide par defaut, sans choix automatique silencieux ;
- ajout depuis fiche joueuse ou repertoire ;
- shortlist sauvegardee ;
- ordre drag and drop ;
- verrouillage du scope ;
- URL partageable ;
- masquage/affichage des series ;
- export de la comparaison.

### 6.6 Fiche equipe

```text
┌ Equipe | bilan | saison | actions ────────────────────────────────────────────┐
├ Synthese | Effectif | Matchs | Tendances                                      │
├ KPI collectif ────────────────────────────────────────────────────────────────┤
├ Production et efficacite ──────────┬ Top contributrices ──────────────────────┤
├ Derniers matchs ───────────────────┼ Disponibilite effectif ───────────────────┤
└ Table effectif configurable ──────────────────────────────────────────────────┘
```

Le detail de match n'est pas duplique ici. Une carte ouvre la fiche match dans le meme
contexte, avec possibilite de retour.

### 6.7 Liste des matchs

```text
┌ Recherche | competition | equipe | saison | journee | calendrier/liste ───────┐
│ Date       Affiche              Score       Competition       Statut            │
│ ...                                                                         │
└──────────────────────────────────────────────────────────────────────────────┘
```

Ameliorations :

- conserver la position de scroll au retour ;
- proposer liste dense et cartes ;
- filtres persistants ;
- URL encodee ;
- pagination serveur.

### 6.8 Fiche match

```text
┌ Retour | Equipe A 31 - 29 Equipe B | J12 | action exporter ──────────────────┐
├ Synthese | Timeline | Comparaison | Zones | Joueuses                         │
├ KPI scenario ────────────────────────────────────────────────────────────────┤
├ Courbe du score + moments cles ──────────────────────────────────────────────┤
├ Scope equipe A / deux / equipe B ────────────────────────────────────────────┤
├ Cartes spatiales ─────────────────┬ Tableau joueuses ────────────────────────┤
└ Methodologie et qualite des donnees ─────────────────────────────────────────┘
```

Le scope equipe controle simultanement courbes, tableaux et spatial. Le libelle et la
requete doivent toujours etre alignes.

## 7. Tableaux cibles

Composant unique `AnalyticsDataGrid` :

- tri simple et multi-colonne ;
- recherche ;
- filtres par colonne ;
- colonnes visibles et ordre ;
- redimensionnement ;
- pinning de colonnes ;
- densite ;
- pagination/virtualisation ;
- selection multiple ;
- regroupement ;
- vue sauvegardee ;
- export ;
- etat vide ;
- rendu mobile par cartes ;
- navigation clavier ;
- `aria-sort`.

## 8. Graphiques cibles

Chaque graphique doit fournir :

- titre et objectif ;
- unite ;
- scope ;
- taille d'echantillon ;
- tooltip detaille ;
- legende interactive ;
- mode tableau alternatif ;
- export ;
- focus clavier lorsque l'interaction est essentielle ;
- etat vide ;
- etat donnees insuffisantes ;
- respect du mode reduit.

Zoom et drill-down ne sont ajoutes que s'ils servent une question metier.

## 9. Design system cible

### Fondations

- conserver l'orange HandWStat comme accent, pas comme couleur universelle ;
- conserver le dark-first, avec parite claire ;
- adopter une typographie plus distinctive pour les titres et tabulaire pour les chiffres ;
- definir une grille 4/8 px ;
- limiter les rayons a trois niveaux ;
- definir les densites `comfortable` et `compact`;
- valider WCAG 2.2 AA.

### Composants a formaliser

- AppShell ;
- GlobalScopeBar ;
- CommandPalette ;
- EntitySearch ;
- PageHeader ;
- Tabs ;
- FilterDrawer ;
- AnalyticsDataGrid ;
- KpiCard ;
- InsightCard ;
- ChartPanel ;
- ExportMenu ;
- EmptyState ;
- ErrorState ;
- Skeleton ;
- Toast ;
- ConfirmDialog ;
- FavoriteButton ;
- RecentItems.

## 10. Micro-interactions

- skeleton uniquement sur la structure attendue ;
- transition de 140 a 200 ms ;
- toast apres export ou copie ;
- indicateur `Filtres modifies` avant application ;
- animation de mise a jour des KPI sans reflow ;
- maintien de la selection et du scroll ;
- annonce `aria-live` apres changement de scope ;
- drag and drop uniquement pour l'ordre des comparaisons et colonnes.


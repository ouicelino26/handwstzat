# HandWStat - Matrice de parite fonctionnelle

Statuts autorises :

- `A conserver`
- `A deplacer`
- `A ameliorer`
- `A corriger`
- `A valider`
- `Implemente`
- `Dette a supprimer apres validation`

## Navigation et session

| ID | Fonction actuelle | Destination cible | Statut |
| --- | --- | --- | --- |
| NAV-01 | Connexion identifiant/mot de passe | Page Connexion | A conserver |
| NAV-02 | Acces demo | Page Demo | A conserver |
| NAV-03 | Redirection session active | Shell analytique | A conserver |
| NAV-04 | Deconnexion | Menu utilisateur | A conserver |
| NAV-05 | Theme clair/sombre | Preferences + action rapide | A conserver |
| NAV-06 | Sidebar desktop | AppShell | A ameliorer |
| NAV-07 | Navigation mobile | Rail mobile + Plus | A ameliorer |
| NAV-08 | Loader de navigation | Skeleton/page transition | A ameliorer |
| NAV-09 | Page 404 | Etat introuvable | A conserver |
| NAV-10 | Counter template | Aucune | Dette a supprimer apres validation |

## Dashboard

| ID | Fonction actuelle | Destination cible | Statut |
| --- | --- | --- | --- |
| DASH-01 | Filtre competition | Scope global | A deplacer |
| DASH-02 | Filtre equipe | Scope global/local | A deplacer |
| DASH-03 | Filtre saison | Scope global | A deplacer |
| DASH-04 | Filtre journee | Scope global | A deplacer |
| DASH-05 | Reset | Scope global | A conserver |
| DASH-06 | Equipes actives | KPI contexte | A conserver |
| DASH-07 | Cadence offensive | KPI contexte | A conserver |
| DASH-08 | Jeu prepare | KPI contexte | A conserver |
| DASH-09 | KPI ligue | Resume dashboard | A conserver |
| DASH-10 | Metrique classement | Classement principal | A conserver |
| DASH-11 | Taille Top | Classement principal | A conserver |
| DASH-12 | Graphe classement | Classement principal | A conserver |
| DASH-13 | Joueuse spotlight | Carte actionnable | A ameliorer |
| DASH-14 | Distribution actions | Fiche/spotlight | A conserver |
| DASH-15 | Historique clubs | Fiche joueuse | A deplacer |
| DASH-16 | Spatial spotlight | Fiche/spotlight | A conserver |
| DASH-17 | Tableau joueuses champ | Vue classement complete | A conserver |
| DASH-18 | Filtre poste | Vue classement complete | A conserver |
| DASH-19 | Tableau gardiennes | Vue classement complete | A conserver |
| DASH-20 | Tri tableaux | AnalyticsDataGrid | A conserver |
| DASH-21 | Matchs recents | Dashboard | A conserver |

## Joueuses

| ID | Fonction actuelle | Destination cible | Statut |
| --- | --- | --- | --- |
| PLY-01 | Recherche nom | Recherche locale/globale | A conserver |
| PLY-02 | Filtres competition/equipe/poste/saison/journee | Scope + filtres locaux | A conserver |
| PLY-03 | Filtre requis | Repertoire avec garde-fou | A valider |
| PLY-04 | Repertoire repliable | DataGrid/repertoire | A ameliorer |
| PLY-05 | Selection joueuse | Fiche joueuse | A conserver |
| PLY-06 | KPI synthese | Onglet Synthese | A conserver |
| PLY-07 | Jauges | Onglet Synthese | A conserver |
| PLY-08 | Historique equipe | Onglet Historique | A deplacer |
| PLY-09 | Attaque | Onglet Performance | A deplacer |
| PLY-10 | Defense | Onglet Performance | A deplacer |
| PLY-11 | Passe/perte | Onglet Performance | A deplacer |
| PLY-12 | Sanctions | Onglet Performance | A deplacer |
| PLY-13 | Bloc gardienne | Onglet Performance | A conserver |
| PLY-14 | Technique | Onglet Performance | A conserver |
| PLY-15 | Profil impact | Onglet Performance | A conserver |
| PLY-16 | Rendement | Onglet Performance | A conserver |
| PLY-17 | Tendance recente | Onglet Performance | A conserver |
| PLY-18 | Scatter pertes/impact | Onglet Performance | A conserver |
| PLY-19 | Carte but | Onglet Zones | A conserver |
| PLY-20 | Carte declenchements | Onglet Zones | A conserver |
| PLY-21 | Focus zone | Onglet Zones | A conserver |
| PLY-22 | Recherche matchs | Onglet Matchs | A conserver |
| PLY-23 | Tri matchs | Onglet Matchs | A conserver |
| PLY-24 | Lien detail match | Fiche match | A conserver |
| PLY-25 | Export fiche + radar | Menu Export | A conserver |

## Comparaison

| ID | Fonction actuelle | Destination cible | Statut |
| --- | --- | --- | --- |
| CMP-01 | 2 a 6 slots | ComparisonTray | A conserver |
| CMP-02 | Recherche par slot | EntityPicker | A conserver |
| CMP-03 | Selection automatique | Selection explicite | A ameliorer |
| CMP-04 | Retrait joueuse | ComparisonTray | A conserver |
| CMP-05 | Scope commun | Scope verrouille | A conserver |
| CMP-06 | KPI par joueuse | Resume | A conserver |
| CMP-07 | Radar | Graphiques | A conserver |
| CMP-08 | Volumes | Graphiques | A conserver |
| CMP-09 | Efficacite | Graphiques | A conserver |
| CMP-10 | Technique | Graphiques | A conserver |
| CMP-11 | Production par match | Graphiques | A conserver |
| CMP-12 | Tableau heatmap | AnalyticsDataGrid | A conserver |
| CMP-13 | Historique clubs | Detail joueuse/tooltip | A deplacer |
| CMP-14 | Modes Club/Analyste/Joueuse | Choix visible | Implemente |

## Profil poste

| ID | Fonction actuelle | Destination cible | Statut |
| --- | --- | --- | --- |
| POS-01 | Repertoire filtre | Onglet Profil poste | A conserver |
| POS-02 | Cohorte sans filtre equipe | Badge cohorte | A conserver |
| POS-03 | Minutes de jeu | Badge qualite | A ameliorer |
| POS-04 | Resume instantane | Resume coach | A conserver |
| POS-05 | Coach cards | Resume coach | A conserver |
| POS-06 | KPI snapshot | Resume coach | A conserver |
| POS-07 | Histogramme brut | Graphiques | A conserver |
| POS-08 | Radar normalise | Graphiques | A conserver |
| POS-09 | Multi-radar | Shortlist | A conserver |
| POS-10 | Masquage courbes | Legende interactive | A conserver |
| POS-11 | Scatter | Graphiques | A conserver |
| POS-12 | Tableau axes | AnalyticsDataGrid | A conserver |
| POS-13 | Export CSV | Menu Export | A conserver |
| POS-14 | Export radar SVG | Menu Export | A conserver |
| POS-15 | Export fiche SVG | Menu Export | A conserver |
| POS-16 | Copier resume | Menu Export | A conserver |
| POS-17 | Percentile negatif | Contrat favorable unique | Implemente |
| POS-18 | Cohorte faible | Avertissement qualite | A ameliorer |

## Equipes

| ID | Fonction actuelle | Destination cible | Statut |
| --- | --- | --- | --- |
| TEAM-01 | Selection equipe | Repertoire/fiche equipe | A conserver |
| TEAM-02 | Scope competition/saison/journee | Scope global | A conserver |
| TEAM-03 | KPI equipe | Synthese | A conserver |
| TEAM-04 | Contribution directe | Synthese | A conserver |
| TEAM-05 | Production collective | Synthese | A conserver |
| TEAM-06 | Top internes | Synthese | A conserver |
| TEAM-07 | Profil collectif | Tendances | A conserver |
| TEAM-08 | Impact offensif | Tendances | A conserver |
| TEAM-09 | Filtre poste effectif | AnalyticsDataGrid | A conserver |
| TEAM-10 | Recherche effectif | AnalyticsDataGrid | A conserver |
| TEAM-11 | Tri effectif | AnalyticsDataGrid | A conserver |
| TEAM-12 | Cartes matchs | Onglet Matchs | A conserver |
| TEAM-13 | Detail match interne mort | Fiche match dediee | Dette a supprimer apres validation |

## Matchs

| ID | Fonction actuelle | Destination cible | Statut |
| --- | --- | --- | --- |
| MAT-01 | Liste cartes | Liste/calendrier | A conserver |
| MAT-02 | Recherche match | Liste | A conserver |
| MAT-03 | Filtres | Scope + filtres locaux | A conserver |
| MAT-04 | Query string matchId | Route fiche match | A conserver |
| MAT-05 | Score final | Header fiche | A conserver |
| MAT-06 | Scope equipe | Controle commun | A conserver |
| MAT-07 | KPI resume | Synthese | A conserver |
| MAT-08 | Timeline | Timeline | A conserver |
| MAT-09 | KPI scenario | Timeline | A conserver |
| MAT-10 | Moments cles | Timeline | A conserver |
| MAT-11 | Comparaison equipes | Comparaison | A conserver |
| MAT-12 | Profil match | Synthese | A conserver |
| MAT-13 | Joueuses decisives | Joueuses | A conserver |
| MAT-14 | Top scoreuses | Joueuses | A conserver |
| MAT-15 | Spatial | Zones | A conserver |
| MAT-16 | Focus zone | Zones | A conserver |
| MAT-17 | Tableau joueuses | AnalyticsDataGrid | A conserver |
| MAT-18 | Tri tableau | AnalyticsDataGrid | A conserver |
| MAT-19 | Scope spatial deux equipes | Requete sans teamId | Implemente |
| MAT-20 | Cache detail | Couche de donnees | A conserver |

## Accessibilite

| ID | Controle | Statut |
| --- | --- | --- |
| A11Y-01 | Focus visible | Present, a valider contraste |
| A11Y-02 | Reduction mouvement | Present |
| A11Y-03 | Zones SVG clavier | A corriger |
| A11Y-04 | Alternative tableaux graphiques | A ameliorer |
| A11Y-05 | `aria-sort` tableaux | A ajouter |
| A11Y-06 | Focus apres navigation | Partiel via `FocusOnNavigate` |
| A11Y-07 | Zoom 200 % | A tester |
| A11Y-08 | Lecteur d'ecran | A tester |
| A11Y-09 | Information sans couleur | Partielle, a valider |
| A11Y-10 | Cibles tactiles | A tester sur mobile |

## Validation finale

- [ ] Chaque ID possede un test ou une preuve manuelle.
- [ ] Chaque fonctionnalite deplacee possede une nouvelle route ou localisation.
- [ ] Chaque calcul conserve sa formule et son unite.
- [ ] Chaque filtre conserve sa portee.
- [ ] Chaque export conserve ses colonnes et valeurs.
- [ ] Les roles Admin et Consultation sont testes.
- [ ] Les etats vide, erreur et chargement sont couverts.
- [ ] Le scope global est partageable.
- [ ] Le responsive est valide sur desktop, tablette et mobile.
- [ ] L'ancien code n'est supprime qu'apres validation de la parite.

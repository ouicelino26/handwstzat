# HandWStat - Plan de migration sans perte fonctionnelle

## 1. Strategie

La migration doit etre incrementale. La logique metier et les contrats API ne doivent pas
etre modifies en meme temps que l'architecture visuelle, sauf correctif isole et teste.

Chaque lot doit respecter :

1. test de caracterisation avant modification ;
2. composant cible derriere un point d'entree controle ;
3. comparaison ancien/nouveau ;
4. validation de la matrice de parite ;
5. mesure build, performance et accessibilite ;
6. suppression de l'ancien code seulement apres validation.

## 2. Lot 0 - Securisation

Actions :

- supprimer le secret client embarque et le faire tourner ;
- rendre `HandballManagerCore` reproductible ;
- ajouter CI Windows avec build Release ;
- inventorier et corriger les avertissements PRI jQWidgets ;
- ajouter formatage et analyse statique ;
- etablir les jeux de donnees de reference.

Critere de sortie :

- clone propre compilable sans chemin utilisateur ;
- aucun secret dans Git ;
- build CI vert.

## 3. Lot 1 - Tests de caracterisation

Tests unitaires :

- tous les helpers KPI ;
- timeline, mi-temps, runs et changements de leader ;
- filtres saison/journee ;
- heatmap ;
- normalisation radar ;
- percentile positif et negatif ;
- cohorte du profil poste ;
- formatage des exports.

Tests composants :

- filtres et reset ;
- etats vide/erreur/loading ;
- tri ;
- selection joueur/match/equipe ;
- scope equipe match ;
- navigation query string ;
- theme ;
- access required.

Tests end-to-end :

- connexion -> dashboard ;
- dashboard -> match ;
- fiche joueuse -> match ;
- comparaison de deux joueuses ;
- profil poste -> export ;
- equipe -> match ;
- deconnexion.

## 4. Lot 2 - Fondations UX

Creer :

- `AnalysisScopeStore` ;
- `UserPreferencesStore` ;
- `RecentItemsStore` ;
- `GlobalScopeBar` ;
- `PageHeader` ;
- `ExportMenu` ;
- tokens de densite ;
- systeme de toast.

Ne pas encore restructurer les pages metier.

## 5. Lot 3 - Couche de donnees

Creer :

- service de requetes avec cache, annulation et deduplication ;
- endpoint backend de catalogue des filtres ;
- endpoint agregateur de fiche joueuse ou facade cliente ;
- pagination serveur ;
- contrats explicites pour percentile favorable ;
- metadonnees de qualite : minutes, matchs avec temps, cohorte.

Correctifs isoles :

- double inversion percentile ;
- scope spatial deux equipes ;
- route et fonctions mortes de la page Equipes.

## 6. Lot 4 - Shell et navigation

Implementer :

- sidebar desktop ;
- rail mobile a cinq entrees maximum ;
- breadcrumbs ;
- recherche globale ;
- palette `Ctrl+K` ;
- favoris et recents ;
- persistance du theme et de la densite.

Conserver les routes historiques avec redirections si necessaire.

## 7. Lot 5 - Composants analytiques

Implementer et tester :

- `AnalyticsDataGrid` ;
- `ChartPanel` ;
- `CohortBadge` ;
- `DataQualityNotice` ;
- `InsightCard` ;
- `EntityPicker` ;
- `ComparisonTray`.

Migrer d'abord un tableau non critique pour valider le composant.

## 8. Lot 6 - Dashboard

Objectifs :

- scope global ;
- quatre KPI majeurs ;
- classement principal ;
- spotlight actionnable ;
- recents ;
- liens profonds.

Conserver :

- toutes les metriques ;
- top configurable ;
- tableaux complets champ et gardiennes ;
- spatial spotlight ;
- matchs recents.

## 9. Lot 7 - Joueuses et profil poste

Etapes :

1. repertoire joueuses ;
2. header de fiche ;
3. synthese ;
4. performance ;
5. zones ;
6. matchs ;
7. integration profil poste ;
8. exports.

Le route `/position-profiles` peut devenir un raccourci vers l'onglet de la joueuse sans
etre supprimee.

## 10. Lot 8 - Comparaison

Conserver :

- 2 a 6 slots ;
- tous les KPI ;
- cinq visualisations ;
- tableau complet ;
- filtres communs.

Ajouter :

- shortlist ;
- ordre ;
- sauvegarde ;
- URL ;
- export ;
- ajout depuis une fiche.

## 11. Lot 9 - Equipes et matchs

Equipes :

- conserver synthese, effectif et matchs ;
- supprimer le detail interne mort apres verification ;
- ajouter navigation contextuelle.

Matchs :

- separer liste et fiche ;
- conserver query string ;
- corriger le scope spatial ;
- charger les onglets a la demande ;
- conserver le cache.

## 12. Lot 10 - Accessibilite et performance

Accessibilite :

- zones SVG clavier ;
- tableaux avec `aria-sort` ;
- alternatives textuelles de graphiques ;
- focus apres navigation et ouverture de drawer ;
- contrastes ;
- zoom 200 % ;
- lecteurs d'ecran.

Performance :

- mesurer temps de premiere vue ;
- nombre de requetes ;
- taille bundle ;
- temps de changement de filtre ;
- rendu de 1 000 lignes ;
- memoire du cache ;
- fluidite des graphes.

## 13. Lot 11 - Nettoyage

Supprimer uniquement apres validation :

- `Counter.razor` ;
- logique AudienceLens morte si non retenue ;
- detail equipe mort ;
- dependances JavaScript non utilisees ;
- CSS duplique ;
- fallbacks devenus impossibles ;
- anciens composants remplaces.

## 14. Risques et retours arriere

| Risque | Protection |
| --- | --- |
| Regression statistique | tests de formules et snapshots API |
| Perte de filtre | scope encode dans URL et tests E2E |
| Graphique incoherent | tableau alternatif et tests de donnees |
| Performance mobile | budgets et virtualisation |
| Incompatibilite API | contrats versionnes |
| Perte d'export | comparaison bit a bit des colonnes et valeurs |
| Rejet utilisateur | activation progressive par module |

## 15. Criteres d'acceptation globaux

- zero fonctionnalite sans destination cible ;
- zero formule modifiee silencieusement ;
- zero secret client ;
- build reproductible ;
- parcours critiques testes ;
- WCAG 2.2 AA sur les parcours principaux ;
- responsive valide sur quatre largeurs ;
- requetes filtre reduites ;
- tableaux lourds pagines ou virtualises ;
- matrice de parite a 100 %.


# HandWStat - Audit UX, UI, accessibilite et technique

## 1. Synthese

HandWStat possede deja une profondeur fonctionnelle importante et un langage visuel
coherent : cartes, KPI, filtres dependants, etats de chargement, vues spatiales et
comparaisons. Le principal probleme n'est pas un manque de fonctionnalites, mais leur
dispersion dans des pages tres longues, l'absence de contexte global persistant et
plusieurs ecarts entre fonctions codees et fonctions reellement accessibles.

## 2. Forces confirmees

- Navigation principale stable sur six modules.
- Mode sombre et mode clair avec preference persistante.
- Responsive desktop, tablette et mobile.
- Etats erreur, vide, chargement et acces requis reutilisables.
- Debounce sur les recherches de repertoires lourds.
- Filtres intelligents dependants des matchs disponibles.
- Chargements paralleles.
- Chargement progressif et cache local sur le detail de match.
- Palette de graphiques centralisee.
- Support `prefers-reduced-motion`.
- Focus visible global sur boutons, liens et champs.
- Exports SVG et CSV deja exploitables.
- Tableaux avec tri et coloration relative.
- Separation claire entre valeurs brutes, mediane et radar normalise dans le profil poste.

## 3. Problemes priorises

### P0 - Bloquants

#### P0.1 Le depot ne compile pas sans configuration locale implicite

Preuve :

- `HandWStat.csproj:71` contient un chemin absolu lie a un autre utilisateur.
- `HandWStat.csproj:81` utilise ce chemin pour la reference projet.

Impact :

- onboarding impossible ;
- CI/CD non reproductible ;
- build casse apres un clone propre ;
- risque de pousser une version non verifiee.

Recommandation :

- utiliser une reference relative dans une solution multi-repos ou un package NuGet prive ;
- documenter une propriete MSBuild explicite sans valeur utilisateur par defaut ;
- ajouter une verification CI.

#### P0.2 Un secret client est embarque dans l'application

Preuve : `appsettings.json:5`.

Impact :

- tout secret distribue dans une application cliente doit etre considere public ;
- reutilisation possible hors application ;
- rotation difficile.

Recommandation :

- supprimer la notion de secret partage cote client ;
- utiliser authentification utilisateur, PKCE ou un flux adapte aux clients publics ;
- effectuer une rotation immediate du secret actuel.

#### P0.3 Double inversion possible des percentiles negatifs

Preuves :

- le backend calcule deja un percentile favorable selon `HigherIsBetter` dans
  `PositionProfileService.ComputePercentile` ;
- le frontend inverse de nouveau les axes negatifs dans
  `PositionProfilesViewModels.cs:35`.

Impact :

- pertes, sanctions, tirs rates ou buts encaisses peuvent etre presentes comme forces
  lorsqu'ils sont faibles, ou comme faiblesses lorsqu'ils sont bons ;
- coach cards, badges, tri et exports peuvent raconter des histoires divergentes.

Recommandation :

- definir un contrat unique : `Percentile` brut statistique ou percentile favorable ;
- renommer le champ en `FavorablePercentile` si le backend conserve la logique actuelle ;
- supprimer toute seconde inversion ;
- ajouter des tests avec axes positifs et negatifs.

#### P0.4 Le scope spatial "Deux equipes" charge une seule equipe

Preuve : dans `Matches.razor:651`, le cas par defaut de `MatchSpatialTeamId` retourne
l'identifiant de l'equipe 1 au lieu de `null`.

Impact :

- le libelle annonce les deux equipes ;
- la carte spatiale ne represente potentiellement que l'equipe 1 ;
- decision analytique faussee.

Recommandation :

- transmettre `null` pour le scope combine ;
- couvrir les trois scopes par tests d'integration.

### P1 - Critiques

#### P1.1 Modes de lecture inaccessibles

Le composant `AudienceLensSelector` existe, et quatre pages possedent une logique
`SetActiveLens`, mais aucun ecran ne rend le composant.

Impact :

- des blocs sont definitivement caches selon la valeur par defaut ;
- les modes Club, Analyste et Joueuse ne sont pas une fonctionnalite utilisable ;
- la complexite du code augmente sans benefice UX.

Recommandation :

- rendre le choix visible et persistant ;
- ou supprimer la logique conditionnelle en conservant une seule experience ;
- ne pas conserver un mode fantome.

#### P1.2 Detail de match mort dans Equipes

La page prepare KPI, timeline et insights, mais `SelectedMatchId` n'est jamais renseigne
par une action utilisateur.

Impact :

- code non testable par l'utilisateur ;
- maintenance couteuse ;
- fonctionnalite promise mais absente.

Recommandation :

- choisir explicitement entre detail inline et navigation vers `/matches?matchId=...` ;
- supprimer le chemin mort seulement apres validation de la parite.

#### P1.3 Pas de filtre global persistant

Chaque module recharge competition, equipe, saison et journee independamment.

Impact :

- changements de contexte repetes ;
- risque de comparer des ecrans sur des scopes differents ;
- nombreux clics inutiles.

Recommandation :

- creer un `AnalysisScopeStore` partage ;
- afficher le scope actif dans un bandeau global ;
- laisser chaque page ajouter ses filtres locaux.

#### P1.4 Pas de recherche globale ni de navigation contextuelle

Il n'existe ni recherche multi-entites, ni palette de commandes, ni historique recent,
ni favoris.

Impact :

- les utilisateurs reguliers passent constamment par la navigation et les repertoires ;
- aucun raccourci vers une joueuse, une equipe ou un match recent.

Recommandation :

- recherche globale `Ctrl+K` ;
- resultats groupes Joueuses/Equipes/Matchs ;
- commandes contextuelles et dernieres consultations.

#### P1.5 Absence de tests frontend

Impact :

- risque eleve lors de toute refonte ;
- aucune preuve de parite fonctionnelle ;
- calculs locaux et transitions d'etat non verrouilles.

Recommandation :

- tests unitaires des calculateurs ;
- bUnit pour les composants critiques ;
- tests end-to-end des parcours ;
- snapshots visuels des graphiques et etats.

### P2 - Importants

#### P2.1 Pages monolithiques

Tailles observees :

- `Players.razor` : 2 129 lignes ;
- `Matches.razor` : 1 654 lignes ;
- `PositionProfiles.razor.cs` : 1 616 lignes ;
- `Compare.razor` : 1 311 lignes ;
- `Teams.razor` : 925 lignes ;
- `Dashboard.razor` : 895 lignes.

Impact :

- responsabilites UI, orchestration, calcul et export melangees ;
- relecture difficile ;
- regressions probables ;
- faible reutilisabilite.

Recommandation :

- feature folders ;
- page orchestratrice mince ;
- composants de section ;
- view models et services de presentation ;
- services d'export dedies.

#### P2.2 Surcout API des filtres

Pour reconstruire les options, chaque page charge trois listes de 500 matchs. Ce schema
est repete dans Dashboard, Joueuses, Comparaison, Profil poste et Matchs.

Impact :

- latence et trafic inutiles ;
- donnees potentiellement tronquees au-dela de 500 matchs ;
- logique dupliquee.

Recommandation :

- endpoint de catalogue de filtres ;
- cache par scope ;
- reponse contenant valeurs valides et compteurs ;
- annulation des requetes obsoletes.

#### P2.3 Chargement excessif de la fiche joueuse

Dix endpoints sont appeles a chaque selection, y compris spatial et matchs meme lorsque
l'utilisateur reste sur Synthese.

Recommandation :

- endpoint agregateur de fiche ou chargement par onglet ;
- prefetch intelligent apres stabilisation de la selection ;
- cache par joueuse et signature de filtre.

#### P2.4 Tableaux sans pagination ni virtualisation

Les repertoires peuvent charger 1 000 joueuses et les tableaux rendent toutes les lignes.

Impact :

- DOM lourd ;
- scroll long ;
- performance mobile fragile ;
- absence de sauvegarde des colonnes et preferences.

Recommandation :

- composant `DataGrid` unifie ;
- pagination serveur ou virtualisation ;
- colonnes configurables ;
- vues sauvegardees ;
- export du scope complet.

#### P2.5 Filtres appliques immediatement sans transaction

Presque chaque changement de selecteur relance le chargement complet.

Impact :

- effet cascade lors de plusieurs choix ;
- loaders frequents ;
- risque de reponses dans le desordre sur les pages sans token de chargement.

Recommandation :

- mode `Appliquer` pour les filtres lourds ;
- auto-apply debounced pour la recherche ;
- comparaison visuelle entre filtres appliques et brouillon.

#### P2.6 Navigation mobile incoherente

Le code declare six destinations, tandis que le CSS impose cinq colonnes au rail mobile.

Impact :

- redistribution implicite ou debordement ;
- labels compresses ;
- difficulte a atteindre toutes les destinations sur petit ecran.

Recommandation :

- limiter le rail a quatre ou cinq actions principales ;
- placer le reste dans `Plus` ;
- tester 320, 375, 768 et 1024 px.

#### P2.7 Graphiques non pilotables au clavier

Les zones SVG ont des gestionnaires `onclick`, mais ne sont pas focusables et ne portent
ni `role=button`, ni gestion Enter/Espace.

Impact :

- fonctionnalite spatiale inaccessible au clavier ;
- lecteurs d'ecran limites a une description globale.

Recommandation :

- rendre chaque zone interactive focusable ;
- fournir nom, valeur et etat selectionne ;
- offrir une liste/table alternative synchronisee.

#### P2.8 Contexte absent de certaines pages

Des modeles `DashboardScopeItems`, `MatchScopeItems` et `TeamScopeItems` existent sans etre
rendus.

Impact :

- le scope de donnees n'est pas toujours visible apres fermeture du drawer ;
- risque d'interpretation hors contexte.

Recommandation :

- bandeau de scope global persistant ;
- chips supprimables ;
- indication claire de la cohorte et de la periode.

### P3 - Dette et coherence

- `Counter.razor` est un reliquat de template.
- Les trois pages CSS principales ne contiennent qu'un import global.
- Les tokens dark sont dupliques entre `:root` et `html[data-theme=dark]`.
- Bootstrap, Blazor Bootstrap, ApexCharts, jQWidgets et Chart.js sont charges ensemble ;
  Chart.js n'est pas utilise dans le code examine.
- Bootstrap JS est charge depuis un CDN, ce qui fragilise le mode hors connexion.
- Le client stocke le role mais ne l'exploite pas.
- Les erreurs API affichent parfois des details techniques bruts.
- Aucun mecanisme de telemetrie ou de mesure UX n'est present.
- Aucun favori, historique recent ou preference de tableau n'est persiste.
- Les exports SVG utilisent une palette claire fixe, meme lorsque l'utilisateur travaille
  en mode sombre.

## 4. Audit des parcours

### 4.1 Trouver une joueuse et comprendre son dernier niveau

Parcours actuel :

1. Joueuses.
2. Ouvrir les filtres.
3. Choisir au moins un critere.
4. Attendre le repertoire.
5. Choisir une joueuse.
6. Ouvrir Graphes.
7. Lire la tendance.

Friction :

- aucun acces par recherche globale ;
- aucun recent ;
- changement de page pour le profil poste ;
- le scope n'est pas partage avec le dashboard.

### 4.2 Comparer des joueuses

Parcours actuel :

1. Comparaison.
2. Regler le scope.
3. Choisir le nombre de slots.
4. Remplacer les selections automatiques.
5. Passer a Graphiques ou Tableau.

Friction :

- selection automatique potentiellement surprenante ;
- aucune sauvegarde de groupe ;
- aucune comparaison lancee depuis une fiche ;
- pas de lien partageable.

### 4.3 Analyser un match

Parcours actuel :

1. Matchs.
2. Filtrer ou rechercher.
3. Ouvrir une carte.
4. Lire Synthese.
5. Changer le scope equipe.
6. Ouvrir Zones ou Effectif.

Points positifs :

- query string partageable ;
- chargement progressif ;
- cache interne.

Friction :

- liste et detail sont deux rendus tres differents dans la meme page ;
- retour aux cartes perd le contexte de scroll ;
- le scope `Deux equipes` est incoherent pour le spatial.

### 4.4 Preparer une lecture staff par poste

Parcours actuel :

1. Profil poste.
2. Choisir un filtre.
3. Choisir une joueuse.
4. Lire la synthese coach.
5. Ajouter jusqu'a trois comparaisons.
6. Exporter.

Points positifs :

- workflow complet ;
- plusieurs modes de visualisation ;
- exports utiles.

Friction :

- page tres longue ;
- incoherence potentielle de percentile negatif ;
- aucune sauvegarde de shortlist ;
- la difference repertoire equipe/cohorte sans equipe est expliquee, mais reste complexe.

## 5. Audit visuel

### Points solides

- direction dark-first distinctive ;
- accent orange stable ;
- surfaces et bordures coherentes ;
- echelle d'espacement existante ;
- etats semantiques ;
- responsive detaille ;
- animations limitees et respect de la reduction de mouvement.

### Points a revoir

- Open Sans pour texte et titres limite la personnalite premium.
- Trop de cartes ont un poids visuel similaire.
- Les titres `section-kicker + h3/h4 + subtitle` sont repetes presque partout.
- Les tableaux tres larges reposent essentiellement sur le scroll horizontal.
- La densite n'est pas configurable.
- Les couleurs de heatmap doivent etre validees en contraste dans les deux themes.
- Les actions d'export du profil poste sont quatre boutons de meme niveau.

## 6. Benchmark et principes retenus

Les sources publiques de reference montrent des principes convergents :

- Hudl met en avant une plateforme integree reliant donnees, video et decisions ;
- Wyscout organise une grande profondeur de donnees autour de la recherche et de
  l'evaluation de profils ;
- StatsBomb positionne l'analyse autour de decisions plus rapides ;
- Sofascore rend des statistiques complexes lisibles par des representations graphiques ;
- FotMob privilegie un acces rapide aux matchs, equipes et statistiques ;
- Linear et Figma utilisent une palette de commandes contextuelle et le clavier ;
- Stripe organise la recherche transverse, les filtres et les listes denses.

Principes applicables a HandWStat :

1. un scope d'analyse global et visible ;
2. une recherche transverse ;
3. des pages detail avec navigation contextuelle ;
4. une densite progressive ;
5. des tableaux configurables ;
6. des graphiques avec vue detaillee alternative ;
7. des raccourcis pour utilisateurs reguliers ;
8. des insights toujours relies a leur formule et leur echantillon.

Sources :

- https://www.hudl.com/products
- https://wyscout.com/
- https://statsbomb.com/
- https://www.sofascore.com/news/sofascores-attribute-overview-the-ultimate-tool-for-player-analysis
- https://www.fotmob.com/
- https://linear.app/docs/conceptual-model
- https://help.figma.com/hc/en-us/articles/23570416033943-Use-the-actions-menu-in-Figma-Design
- https://docs.stripe.com/dashboard/search


# Roadmap HandWStat Ultimate

Estimations relatives : `XS` moins d'une journee, `S` 1-2 jours, `M` 3-5 jours, `L` 1-2 semaines, `XL` plusieurs semaines. Chaque phase conserve l'API v1 tant que la migration v2 n'est pas prouvee.

## Phase 0 - statistiques fiables

| Tache | Etat | Dependance | Estimation | Critere d'acceptation |
|---|---|---|---|---|
| P0-01 Ratios nullables et `N/A` | Termine | Aucune | S | Aucun ratio nouveau ne retourne le numerateur pour un denominateur nul. |
| P0-02 Corriger tentatives/tirs contres | Termine | Regles API 1.0 | XS | Un tir contre reste un sous-type et n'est compte qu'une fois. |
| P0-03 Contrat d'affichage taux | Termine sur dashboard | API v1 | S | Valeur, unite, volume, seuil, fiabilite, tooltip et tone accessible. |
| P0-04 Scope et qualite visibles | Termine sur dashboard | DQ API future | S | `Unknown` explicite, jamais transforme en qualite elevee. |
| P0-05 Erreurs typees | Termine | ProblemDetails v1 partiel | S | Corps technique absent de l'UI, correlation disponible. |
| P0-06 Annulation, gate, mesure | Termine sur dashboard | Aucune | S | Un filtre obsolete est annule ; duree loggee en DEBUG. |
| P0-07 Lazy loading equipe du jour | Termine | Aucune | XS | Aucun appel equipe du jour au premier rendu. |
| P0-08 Audit et tests | Termine | Contexte maitre manquant | M | Six documents et tests unitaires/composants passes. |

## Phase 1 - dashboard progressif

1. Creer un endpoint agrege v2 `dashboard/snapshot` avec scope resolu, metriques, echantillons, fraicheur et qualite ; conserver `V1AnalyticsGateway`.
2. Reduire le chemin initial a 3-5 appels et mesurer p50/p95 sur Android et Windows.
3. Charger comparaison globale, spatial et graphiques uniquement a l'ouverture de leur section.
4. Extraire `PlayerSpotlightLoader`, `DashboardRankingService` et les fallbacks restants.
5. Migrer toutes les cartes dashboard vers `RateMetricCard`/un composant de compte equivalent.
6. Remplacer le score local de l'equipe du jour par un contrat versionne, ou retirer la denomination PIE.
7. Completer tableaux : `scope="col"`, captions, annonce du tri et liens joueuse/equipe/match.

Dependances API : P1-04 et P1-06 de `ULTIMATE_API_ROADMAP.md`, DTO metrique/scope/qualite, ProblemDetails uniforme.

## Tranche Phase 1 Ligue livree le 2026-07-29

1. Contrat v2 joueuse copie et mappe champ par champ : termine.
2. `V2AnalyticsGateway`, validation structurelle et provenance : termine.
3. Fallback v1 par metrique uniquement sur endpoint absent : termine.
4. Panneaux Attaque, Defense et Gardienne repliables et accessibles : termine.
5. Six taux avec preuve, qualite et version serveur : termine.
6. `FailedPivotPasses` explicite `DATA_MISSING` : termine cote UI, bloque cote donnee.
7. Tests handlers/composants/non-regression et quatre builds plateforme : termine.

Gate restant : appel live authentifie non execute. Aucune fonctionnalite possession, lineup ou xG n'a ete commencee.

## Phase 2 - fiche joueuse et benchmarks

1. Decouper `Players.razor` en annuaire, resume, efficacite, spatial, tendance et historique.
2. Migrer tous les taux/per-match/per-60 vers le contrat nullable.
3. Afficher minutes, matchs avec temps, cohortes, percentiles et taille d'echantillon.
4. Uniformiser `Impact defensif` entre global et tendance recente.
5. Ajouter benchmarks par poste uniquement avec cohorte et version explicites.
6. Integrer le rapport de qualite/provenance API Phase 2.

Dependances API : temps de jeu fiabilise, taxonomie canonique, metadonnees de cohorte et rapport DQ.

## Phase 3 - equipe et match

1. Decouper `Teams.razor` et `Matches.razor` en sections chargees progressivement.
2. Obtenir la regle de points depuis la competition avant de publier `Points/match`.
3. Versionner `jeu prepare`, `ballons valorises` et les perimetres d'impact.
4. Relier explicitement joueuse, equipe, match et evenement.
5. Ajouter un resume match agrege avec scope, volumes, qualite et fraicheur.

Dependances API : metadonnees competition, resume agrege, identite evenementielle.

## Phase 4 - possessions et lineups

1. Attendre le moteur de possessions deterministe API Phase 3.
2. Afficher pace, efficacite offensive/defensive et per-100 avec version et qualite.
3. Attendre les intervalles de presence fiables API Phase 4.
4. Ajouter compositions, plus-minus, on/off et net rating avec seuils.

Ne pas deduire des possessions a partir des seuls tirs/pertes actuels. Ne pas reconstruire des lineups a partir de temps agreges.

## Phase 5 - xG/xS

1. Consommer uniquement un modele API calibre, versionne et accompagne de ses features/qualite.
2. Afficher xG par tir, xG total, xS et arrets au-dessus de l'attendu avec volumes.
3. Exposer calibration, date de modele et limites de comparaison.

Ne pas simuler xG/xS depuis les zones actuelles.

## Phase 6 - scouting, video et offline

1. Rapports scouting reproductibles et exportables.
2. Liens evenement-video seulement avec timestamps fiables et droits verifies.
3. Cache offline chiffre, synchronisation idempotente et conflits explicites.
4. Live/webhooks apres observabilite et reprise robuste.
5. Narration IA seulement sur metriques contractuelles, avec sources et niveau de confiance.

## Gates transverses

- tests unitaires, composants et non-regression mise a jour ;
- builds Windows et Android a chaque phase touchant la plateforme ;
- aucune migration automatique ;
- aucun endpoint v2 invente ;
- aucune valeur inconnue convertie en zero ;
- budget d'appels, temps, memoire et rendu mesure ;
- accessibilite clavier, lecteur d'ecran, contrastes et mouvement reduit ;
- documentation de rollback et compatibilite v1.

   # MISSION : AUDIT EXHAUSTIF ET REFONTE UX/UI D’UNE APPLICATION DE STATISTIQUES SPORTIVES

Tu agis comme une équipe senior réunissant les compétences suivantes :

- architecte logiciel ;
- lead developer frontend et backend ;
- expert UX/UI ;
- product designer spécialisé dans les applications métiers complexes ;
- expert en visualisation de données sportives ;
- spécialiste des statistiques sportives ;
- expert en accessibilité numérique ;
- ingénieur performance ;
- expert en reverse engineering de code existant ;
- ingénieur QA ;
- responsable de migration applicative.

Tu disposes d’un accès complet au projet, à son système de fichiers, à son terminal, à son historique Git si disponible, ainsi qu’aux outils nécessaires pour lire, exécuter, tester et modifier l’application.

Ta mission consiste à réaliser l’audit complet, la documentation fonctionnelle et technique, la conception UX/UI, puis la refonte progressive de l’application.

La priorité absolue est la préservation fonctionnelle.

La refonte doit conserver 100 % des fonctionnalités, règles métier, calculs statistiques, workflows et comportements utiles existants, sauf suppression explicitement justifiée, documentée et validée.

---

# 1. PRINCIPES ABSOLUS

Respecte impérativement les règles suivantes pendant toute la mission.

## 1.1 Analyse avant modification

Tu ne dois modifier aucun fichier applicatif avant d’avoir :

1. analysé l’intégralité du projet ;
2. établi l’inventaire des fichiers ;
3. identifié l’architecture technique ;
4. cartographié les écrans et les parcours ;
5. recensé les fonctionnalités ;
6. recensé les règles métier ;
7. documenté les calculs statistiques ;
8. identifié les appels API et les flux de données ;
9. évalué les faiblesses de l’existant ;
10. produit une stratégie de refonte et de migration.

Aucune génération de code de refonte ne doit commencer avant la production de ces livrables.

## 1.2 Aucune supposition évitable

Ne fais aucune supposition lorsqu’une information peut être obtenue par :

- la lecture du code ;
- l’analyse des routes ;
- l’inspection des contrôleurs ;
- l’analyse des services ;
- l’examen des modèles et DTO ;
- l’inspection des composants ;
- l’analyse des fichiers de configuration ;
- l’examen des schémas de données ;
- l’exécution de l’application ;
- l’observation des requêtes réseau ;
- l’exécution des tests ;
- l’analyse de l’historique Git ;
- la consultation de la documentation présente dans le dépôt.

Chaque conclusion importante doit être reliée à une preuve identifiable : fichier, route, classe, fonction, composant, endpoint, modèle, test ou comportement observé.

Lorsqu’une information reste réellement impossible à déterminer, indique explicitement :

- l’information manquante ;
- les recherches effectuées ;
- les hypothèses envisageables ;
- le niveau de risque ;
- la décision la plus prudente.

## 1.3 Préservation fonctionnelle

Il est interdit de supprimer silencieusement :

- une page ;
- un onglet ;
- un bouton ;
- une action ;
- un filtre ;
- un tri ;
- un export ;
- un import ;
- une colonne ;
- une donnée ;
- un graphique ;
- une option ;
- un raccourci ;
- une validation ;
- une permission ;
- une règle métier ;
- un calcul ;
- un état d’interface ;
- un comportement automatique ;
- un cas particulier ;
- une gestion d’erreur.

Une fonctionnalité peut être déplacée, fusionnée ou repensée uniquement si :

1. son comportement est intégralement préservé ;
2. sa nouvelle localisation est documentée ;
3. son accessibilité reste au moins équivalente ;
4. son remplacement est vérifiable dans la matrice de parité fonctionnelle.

## 1.4 Respect du projet existant

Tu dois :

- préserver les modifications utilisateur déjà présentes ;
- ne jamais écraser du travail sans l’avoir identifié ;
- ne pas utiliser de commandes destructrices ;
- ne pas réécrire inutilement la logique métier ;
- respecter les conventions utiles du projet ;
- privilégier une migration progressive ;
- éviter une réécriture complète non justifiée ;
- conserver les contrats API sauf nécessité clairement démontrée ;
- ne jamais exposer de secret, clé, mot de passe ou donnée sensible.

---

# 2. PHASE 0 : ÉTAT INITIAL ET SÉCURISATION

Avant l’analyse fonctionnelle, établis un état initial du projet.

Inspecte notamment :

- la racine du dépôt ;
- les projets et sous-projets ;
- les fichiers de solution et de workspace ;
- les manifests de dépendances ;
- les fichiers de configuration ;
- les variables d’environnement documentées ;
- les scripts de build ;
- les scripts de déploiement ;
- la configuration CI/CD ;
- les conteneurs éventuels ;
- les migrations de base de données ;
- les tests ;
- l’état Git ;
- les modifications non validées ;
- les fichiers générés ;
- les dépendances externes ;
- les éventuels sous-modules.

Produis un manifeste des fichiers et répertoires.

Chaque fichier doit être examiné ou classifié explicitement. Pour les fichiers générés, binaires, dépendances embarquées ou artefacts de build, indique leur nature et leur rôle sans prétendre les analyser comme du code métier.

Établis ensuite :

- la stack technique exacte ;
- les versions utilisées ;
- les commandes d’installation ;
- les commandes de développement ;
- les commandes de build ;
- les commandes de test ;
- les commandes de lint ;
- les commandes de déploiement si elles existent ;
- les prérequis nécessaires au lancement du projet.

Si l’application peut être exécutée, lance-la de manière contrôlée et relève son comportement réel.

---

# 3. PHASE 1 : ANALYSE INTÉGRALE DU PROJET

Parcours l’intégralité du projet sans omettre de zone fonctionnelle.

Inspecte notamment :

- le code frontend ;
- le code backend ;
- les vues ;
- les pages ;
- les layouts ;
- les composants ;
- les hooks ;
- les stores ;
- les contextes ;
- les contrôleurs ;
- les services ;
- les modèles ;
- les entités ;
- les DTO ;
- les repositories ;
- les middlewares ;
- les validateurs ;
- les helpers ;
- les utilitaires ;
- les routes frontend ;
- les routes backend ;
- les endpoints API ;
- les clients HTTP ;
- les schémas de base de données ;
- les migrations ;
- les seeds ;
- les tâches planifiées ;
- les traitements asynchrones ;
- les imports et exports ;
- les ressources statiques ;
- les images ;
- les icônes ;
- les polices ;
- les styles globaux ;
- les feuilles de style locales ;
- les thèmes ;
- les bibliothèques graphiques ;
- les fichiers de traduction ;
- les tests unitaires ;
- les tests d’intégration ;
- les tests end-to-end ;
- les mocks ;
- les fixtures ;
- les scripts ;
- la configuration de sécurité ;
- la gestion des rôles et permissions ;
- les fichiers de documentation ;
- les mécanismes de télémétrie et de journalisation.

Analyse les relations entre ces éléments. Une liste de fichiers isolée ne suffit pas : il faut comprendre le fonctionnement réel du système.

---

# 4. PHASE 2 : CARTOGRAPHIE DE L’ARCHITECTURE

Produis une cartographie claire de l’architecture existante.

Documente :

- les applications et services ;
- les couches techniques ;
- les dépendances entre projets ;
- les dépendances frontend/backend ;
- les sources de données ;
- les flux de données ;
- la gestion de l’état ;
- la navigation ;
- l’authentification ;
- les autorisations ;
- les systèmes de cache ;
- la persistance ;
- les intégrations externes ;
- les tâches automatiques ;
- la génération de rapports ;
- les imports et exports ;
- les traitements statistiques.

Fournis au minimum :

1. une arborescence commentée du projet ;
2. une carte des modules ;
3. une carte des routes frontend ;
4. une carte des endpoints backend ;
5. une carte des dépendances entre modules ;
6. un schéma des principaux flux de données ;
7. une description du cycle complet d’une requête, de l’interface à la persistance ;
8. une identification des responsabilités actuellement mal réparties.

Pour chaque endpoint API, documente :

- méthode HTTP ;
- route ;
- paramètres ;
- query parameters ;
- body ;
- format de réponse ;
- erreurs possibles ;
- authentification ;
- autorisations ;
- contrôleur ;
- service appelé ;
- accès aux données ;
- consommateurs frontend ;
- règles métier appliquées ;
- validations ;
- effets de bord.

---

# 5. PHASE 3 : DOCUMENTATION FONCTIONNELLE EXHAUSTIVE

Documente chaque page, écran, vue, onglet, sous-onglet, popup, modale, panneau, menu et module.

Pour chaque élément, indique obligatoirement :

- son nom ;
- sa route ;
- son objectif ;
- ses utilisateurs cibles ;
- ses conditions d’accès ;
- ses permissions ;
- ses données d’entrée ;
- ses données affichées ;
- ses dépendances ;
- ses états de chargement ;
- ses états vides ;
- ses états d’erreur ;
- son comportement responsive ;
- ses interactions avec les autres écrans.

Recense exhaustivement :

- les boutons ;
- les liens ;
- les menus ;
- les menus contextuels ;
- les actions principales ;
- les actions secondaires ;
- les actions en masse ;
- les formulaires ;
- les champs ;
- les validations ;
- les valeurs par défaut ;
- les filtres ;
- les recherches ;
- les tris ;
- les sélections ;
- les paginations ;
- les exports ;
- les imports ;
- les graphiques ;
- les tableaux ;
- les tooltips ;
- les légendes ;
- les raccourcis clavier ;
- les confirmations ;
- les notifications ;
- les messages d’erreur ;
- les états désactivés ;
- les comportements conditionnels ;
- les interactions au survol ;
- les interactions tactiles ;
- les téléchargements ;
- les impressions ;
- les mécanismes de partage.

Pour chaque action utilisateur, documente la chaîne complète :

Action utilisateur → composant frontend → validation client → appel API → contrôleur → service → règle métier → accès aux données → réponse → mise à jour de l’interface.

Crée une matrice fonctionnelle contenant au minimum :

| Identifiant | Écran | Fonctionnalité | Déclencheur | Règle métier | API | Résultat | Permissions | Cas d’erreur | Test existant |
|-------------|-------|----------------|-------------|---------------|-----|----------|-------------|--------------|---------------|

Attribue un identifiant stable à chaque fonctionnalité afin de pouvoir vérifier sa conservation pendant la migration.

---

# 6. PHASE 4 : INVENTAIRE DES RÈGLES MÉTIER

Produis un document distinct et exhaustif consacré aux règles métier.

Recense :

- les règles explicites ;
- les règles implicites ;
- les contraintes de domaine ;
- les conditions ;
- les exceptions ;
- les valeurs par défaut ;
- les seuils ;
- les arrondis ;
- les unités ;
- les conversions ;
- les agrégations ;
- les exclusions ;
- les règles de filtrage ;
- les règles de classement ;
- les règles de comparaison ;
- les règles de validation ;
- les traitements automatiques ;
- les dépendances entre modules ;
- les workflows utilisateurs ;
- les workflows système ;
- les transitions d’état ;
- les permissions et restrictions.

Pour chaque règle, indique :

- un identifiant ;
- une description compréhensible ;
- sa justification métier si elle est identifiable ;
- son implémentation actuelle ;
- les fichiers concernés ;
- les données utilisées ;
- les écrans concernés ;
- les cas limites ;
- les risques de régression ;
- les tests existants ;
- les tests manquants.

## Calculs statistiques

Accorde une attention particulière aux calculs statistiques sportifs.

Documente précisément, lorsqu’ils existent :

- les statistiques brutes ;
- les statistiques par match ;
- les statistiques par minute ;
- les statistiques ramenées à 60 minutes ;
- les taux et pourcentages ;
- les moyennes ;
- les médianes ;
- les percentiles ;
- les classements ;
- les cohortes de comparaison ;
- les comparaisons par poste ;
- les comparaisons par équipe ;
- les comparaisons par compétition ;
- les comparaisons par saison ;
- les normalisations ;
- les minimums et maximums ;
- les bornes des radars ;
- le traitement des petites tailles d’échantillon ;
- le traitement des valeurs nulles ;
- le traitement des divisions par zéro ;
- les arrondis ;
- les valeurs extrêmes ;
- les statistiques pondérées ;
- les règles d’éligibilité ;
- les filtres appliqués avant calcul.

Pour chaque statistique, écris la formule exacte et précise :

- le numérateur ;
- le dénominateur ;
- l’unité ;
- la population utilisée ;
- les critères d’inclusion ;
- les critères d’exclusion ;
- le traitement des données manquantes ;
- l’endroit où le calcul est effectué ;
- les composants qui consomment le résultat.

Vérifie notamment que les médianes, percentiles et radars ne reposent pas sur des valeurs de secours arbitraires telles qu’un percentile fixe à 50, sauf si ce comportement est explicitement voulu et documenté.

La refonte visuelle ne doit jamais modifier silencieusement la signification d’une statistique.

---

# 7. PHASE 5 : AUDIT UX/UI ET TECHNIQUE

Analyse l’existant avec une approche factuelle.

## Audit UX

Évalue :

- la clarté des parcours ;
- la découvrabilité des fonctions ;
- le nombre de clics ;
- la cohérence de navigation ;
- la continuité entre les écrans ;
- la densité informationnelle ;
- la surcharge cognitive ;
- la lisibilité des statistiques ;
- la qualité des états vides ;
- la qualité des erreurs ;
- la qualité des feedbacks ;
- la facilité de comparaison ;
- la facilité d’analyse rapide ;
- l’efficacité pour un utilisateur intensif ;
- l’utilisation sur mobile et tablette.

## Audit UI

Évalue :

- la hiérarchie visuelle ;
- la cohérence des couleurs ;
- la typographie ;
- les espacements ;
- les alignements ;
- les dimensions ;
- les icônes ;
- les cartes ;
- les tableaux ;
- les graphiques ;
- les formulaires ;
- les menus ;
- les modales ;
- les états interactifs ;
- la cohérence entre pages ;
- la qualité perçue ;
- la lisibilité des données denses.

## Audit des visualisations

Pour chaque graphique, vérifie :

- la pertinence du type de graphique ;
- la lisibilité ;
- les unités ;
- les axes ;
- les échelles ;
- les légendes ;
- les couleurs ;
- les contrastes ;
- les tooltips ;
- la comparaison des séries ;
- la cohérence des domaines ;
- la gestion des valeurs manquantes ;
- la gestion des valeurs extrêmes ;
- le responsive ;
- l’accessibilité ;
- la fidélité aux données.

## Audit accessibilité

Évalue au minimum :

- la navigation clavier ;
- le focus visible ;
- les contrastes ;
- la structure sémantique ;
- les labels ;
- les alternatives textuelles ;
- les lecteurs d’écran ;
- les zones tactiles ;
- la réduction des animations ;
- la compréhension sans dépendance exclusive à la couleur ;
- la compatibilité avec un zoom important.

Vise au minimum WCAG 2.2 niveau AA lorsque cela est applicable.

## Audit technique

Évalue :

- la maintenabilité ;
- la modularité ;
- le couplage ;
- la duplication ;
- la complexité ;
- la dette technique ;
- les composants surdimensionnés ;
- la séparation des responsabilités ;
- la gestion de l’état ;
- la gestion des erreurs ;
- la gestion du cache ;
- les requêtes dupliquées ;
- les re-rendus inutiles ;
- le poids des bundles ;
- le chargement initial ;
- les performances des tableaux et graphiques ;
- les risques de sécurité ;
- la couverture de tests ;
- l’observabilité.

Classe chaque problème par :

- sévérité ;
- impact utilisateur ;
- impact métier ;
- fréquence ;
- complexité de correction ;
- risque de régression ;
- priorité recommandée.

---

# 8. PHASE 6 : CONCEPTION DE LA NOUVELLE EXPÉRIENCE

Propose une refonte complète sans perdre de fonctionnalité.

La nouvelle expérience doit être :

- premium ;
- professionnelle ;
- immersive ;
- élégante ;
- cohérente ;
- minimaliste sans être pauvre ;
- rapide ;
- lisible ;
- interactive ;
- adaptée à un usage intensif ;
- efficace sur ordinateur ;
- utilisable sur tablette ;
- fonctionnelle sur mobile.

Inspire-toi des principes d’excellence observables dans :

- Hudl ;
- Wyscout ;
- SofaScore ;
- FotMob ;
- Opta ;
- Flashscore ;
- StatsBomb ;
- NBA Stats ;
- les meilleurs dashboards SaaS professionnels.

Ne copie pas leur identité visuelle, leurs assets ou leurs interfaces. Analyse leurs principes :

- densité maîtrisée ;
- navigation rapide ;
- comparaison immédiate ;
- hiérarchisation des statistiques ;
- lecture progressive ;
- filtres efficaces ;
- tableaux performants ;
- visualisations compréhensibles ;
- contextualisation des données ;
- accès rapide aux actions fréquentes.

## Architecture UX

Propose :

- une nouvelle architecture de l’information ;
- une navigation principale ;
- une navigation secondaire ;
- une stratégie pour les filtres globaux ;
- une stratégie pour les filtres locaux ;
- une stratégie de recherche ;
- des raccourcis pour utilisateurs fréquents ;
- une gestion cohérente des pages de détail ;
- une navigation fluide entre équipes, joueuses/joueurs, matchs, compétitions et statistiques ;
- une conservation claire du contexte de navigation ;
- une réduction des clics inutiles.

Pour chaque changement, fournis :

- le problème actuel ;
- la solution proposée ;
- le bénéfice utilisateur ;
- les fonctionnalités concernées ;
- le risque ;
- la manière de préserver la parité fonctionnelle.

---

# 9. MAQUETTES TEXTUELLES DE CHAQUE ÉCRAN

Avant de coder, produis une maquette textuelle détaillée pour chaque écran.

Chaque maquette doit décrire :

- la structure de la page ;
- le header ;
- la navigation ;
- les blocs ;
- les cartes ;
- les tableaux ;
- les graphiques ;
- les filtres ;
- les actions ;
- les états interactifs ;
- les informations prioritaires ;
- les informations secondaires ;
- les comportements responsive ;
- les états de chargement ;
- les états vides ;
- les états d’erreur.

Utilise des wireframes textuels ou ASCII lorsque cela améliore la compréhension.

Pour chaque écran, précise également :

- le parcours principal ;
- les parcours secondaires ;
- les actions fréquentes ;
- les actions avancées ;
- les éléments visibles immédiatement ;
- les éléments accessibles progressivement ;
- les différences desktop, tablette et mobile.

Chaque fonctionnalité de la matrice existante doit apparaître dans au moins une maquette cible.

---

# 10. DESIGN SYSTEM

Conçois un design system propre à l’application.

Il doit comprendre :

## Fondations

- palette principale ;
- couleurs secondaires ;
- couleurs sémantiques ;
- couleurs des données ;
- couleurs des équipes ou catégories si nécessaire ;
- couleurs d’erreur, succès, avertissement et information ;
- contrastes accessibles ;
- typographie ;
- échelle typographique ;
- grille ;
- espacements ;
- rayons ;
- bordures ;
- ombres ;
- élévations ;
- icônes ;
- règles de densité ;
- breakpoints ;
- animations ;
- durées et courbes de transition.

Les couleurs des graphiques doivent rester distinguables et accessibles. Elles ne doivent pas transmettre une information uniquement par la couleur.

## Composants

Définis précisément les variantes, états et comportements de :

- boutons ;
- liens ;
- champs ;
- sélecteurs ;
- autocomplétions ;
- filtres ;
- chips ;
- badges ;
- onglets ;
- menus ;
- menus contextuels ;
- tableaux ;
- pagination ;
- cartes ;
- cartes statistiques ;
- indicateurs de tendance ;
- graphiques ;
- tooltips ;
- légendes ;
- modales ;
- drawers ;
- notifications ;
- alertes ;
- loaders ;
- skeletons ;
- états vides ;
- breadcrumbs ;
- barres de recherche ;
- sélecteurs de période ;
- comparateurs ;
- panneaux de détails.

Chaque composant doit inclure :

- son objectif ;
- ses variantes ;
- ses tailles ;
- ses états ;
- son comportement clavier ;
- son comportement responsive ;
- ses règles d’accessibilité ;
- ses règles d’utilisation ;
- ses cas à éviter.

Étudie la pertinence d’un mode clair et sombre. Ne l’ajoute pas comme gadget. Si les deux modes sont retenus, les graphiques et couleurs sémantiques doivent être vérifiés dans les deux thèmes.

---

# 11. PERFORMANCE ET ARCHITECTURE CIBLE

Propose une architecture évolutive qui respecte la stack existante, sauf justification solide.

Privilégie :

- les composants réutilisables ;
- une séparation claire entre présentation, données et métier ;
- une gestion centralisée des tokens visuels ;
- des contrats typés ;
- une gestion cohérente des erreurs ;
- le lazy loading des écrans lourds ;
- le chargement différé des graphiques ;
- la réduction des re-rendus ;
- la virtualisation des grands tableaux si nécessaire ;
- la pagination ou le chargement progressif ;
- le cache adapté ;
- l’annulation des requêtes obsolètes ;
- la déduplication des appels ;
- des placeholders stables ;
- la prévention des décalages de mise en page ;
- l’optimisation du bundle ;
- une stratégie claire pour les images, icônes et polices.

Toute optimisation doit être mesurée ou justifiée. N’ajoute pas de complexité sans bénéfice identifiable.

Définis des indicateurs de référence avant et après refonte :

- temps de chargement ;
- taille des bundles ;
- nombre de requêtes ;
- temps d’affichage des écrans principaux ;
- fluidité des tableaux ;
- fluidité des graphiques ;
- nombre de re-rendus lorsque mesurable ;
- résultats Lighthouse ou équivalent ;
- métriques Web Vitals lorsqu’elles sont pertinentes.

---

# 12. PLAN DE MIGRATION

La migration doit être progressive, testable et réversible.

Propose un découpage en lots, par exemple :

1. inventaire et tests de caractérisation ;
2. fondations du design system ;
3. shell applicatif et navigation ;
4. composants génériques ;
5. écrans à faible risque ;
6. écrans métier critiques ;
7. tableaux et graphiques ;
8. responsive et accessibilité ;
9. optimisation ;
10. suppression contrôlée de l’ancien code devenu inutilisé.

Pour chaque lot, indique :

- le périmètre ;
- les fichiers concernés ;
- les dépendances ;
- les risques ;
- les tests requis ;
- les critères d’acceptation ;
- les conditions de retour arrière ;
- les fonctionnalités à comparer avec l’ancien écran.

Privilégie une migration écran par écran ou module par module permettant de comparer l’ancien et le nouveau comportement.

Ne mélange pas sans nécessité :

- refonte visuelle ;
- modification des règles métier ;
- changement de contrat API ;
- changement de base de données ;
- optimisation structurelle profonde.

Si un changement métier ou API devient indispensable, isole-le, documente-le et couvre-le par des tests spécifiques.

---

# 13. STRATÉGIE DE TEST

Avant la refonte, ajoute si nécessaire des tests de caractérisation pour figer les comportements existants.

Prévois :

- tests unitaires ;
- tests des règles métier ;
- tests des calculs statistiques ;
- tests d’intégration API ;
- tests de contrats ;
- tests des permissions ;
- tests end-to-end des parcours critiques ;
- tests des filtres ;
- tests des tris ;
- tests des imports et exports ;
- tests des graphiques ;
- tests responsive ;
- tests clavier ;
- tests d’accessibilité ;
- tests de non-régression visuelle si l’outillage le permet ;
- tests de performance pour les écrans lourds.

Les calculs statistiques critiques doivent être testés avec :

- valeurs nominales ;
- valeurs nulles ;
- zéros ;
- petits échantillons ;
- grands échantillons ;
- valeurs extrêmes ;
- égalités ;
- données incomplètes ;
- divisions par zéro ;
- différents postes, équipes, compétitions ou périodes.

Après chaque lot, exécute les tests pertinents et communique les résultats exacts. Ne déclare jamais qu’une vérification a réussi si elle n’a pas été exécutée.

---

# 14. LIVRABLES OBLIGATOIRES AVANT LE CODE

Avant toute modification de l’interface, produis les documents suivants dans un emplacement dédié, par exemple `docs/refonte/`, sans écraser une documentation existante :

1. `00-etat-initial.md`
   - état Git ;
   - stack ;
   - commandes ;
   - contraintes d’exécution ;
   - état des tests.

2. `01-inventaire-projet.md`
   - inventaire commenté des fichiers et modules ;
   - rôle de chaque zone ;
   - fichiers générés ou externes clairement identifiés.

3. `02-architecture-existante.md`
   - architecture technique ;
   - dépendances ;
   - flux de données ;
   - routes ;
   - API ;
   - persistance.

4. `03-cartographie-fonctionnelle.md`
   - écrans ;
   - onglets ;
   - fonctionnalités ;
   - actions ;
   - parcours ;
   - dépendances.

5. `04-regles-metier.md`
   - règles métier ;
   - workflows ;
   - contraintes implicites ;
   - calculs statistiques ;
   - cas limites.

6. `05-audit-ux-ui.md`
   - constats ;
   - preuves ;
   - sévérité ;
   - impacts ;
   - recommandations.

7. `06-audit-technique-performance.md`
   - dette technique ;
   - architecture ;
   - performance ;
   - maintenabilité ;
   - sécurité ;
   - tests.

8. `07-architecture-ux-cible.md`
   - navigation cible ;
   - architecture de l’information ;
   - parcours optimisés ;
   - stratégie responsive.

9. `08-design-system.md`
   - tokens ;
   - composants ;
   - variantes ;
   - états ;
   - accessibilité ;
   - graphiques.

10. `09-maquettes-textuelles.md`
    - maquette détaillée de chaque écran ;
    - desktop ;
    - tablette ;
    - mobile ;
    - états particuliers.

11. `10-plan-migration.md`
    - lots ;
    - risques ;
    - dépendances ;
    - critères d’acceptation ;
    - stratégie de retour arrière.

12. `11-matrice-parite-fonctionnelle.md`
    - correspondance entre chaque fonctionnalité existante et sa solution cible.

13. `12-plan-tests.md`
    - tests actuels ;
    - tests manquants ;
    - tests à ajouter ;
    - parcours critiques.

14. `13-checklist-validation.md`
    - checklist complète de validation finale.

La documentation doit être concrète, liée au code et suffisamment précise pour qu’un autre développeur puisse reprendre le projet sans dépendre de connaissances implicites.

---

# 15. REVUE OBLIGATOIRE AVANT IMPLÉMENTATION

À la fin de l’audit, présente une synthèse contenant :

- le fonctionnement global de l’application ;
- les modules identifiés ;
- les fonctionnalités critiques ;
- les principales règles métier ;
- les calculs statistiques sensibles ;
- les problèmes les plus importants ;
- l’architecture UX proposée ;
- l’architecture technique proposée ;
- les changements à risque ;
- le plan de migration ;
- les décisions nécessitant une validation humaine.

Ne commence pas la génération du code tant que :

- tous les livrables préparatoires ne sont pas produits ;
- toutes les fonctionnalités ne sont pas présentes dans la matrice de parité ;
- les zones d’incertitude critiques ne sont pas signalées ;
- l’architecture cible n’est pas suffisamment définie.

Si tu travailles dans un contexte interactif, demande une validation explicite avant de lancer la phase d’implémentation.

---

# 16. PHASE D’IMPLÉMENTATION

Après validation de l’audit et de la conception, implémente la refonte progressivement.

Pour chaque lot :

1. annoncer précisément le périmètre ;
2. identifier les fonctionnalités concernées ;
3. vérifier les tests existants ;
4. ajouter les tests de caractérisation nécessaires ;
5. implémenter les composants ;
6. conserver les contrats et comportements métier ;
7. vérifier le responsive ;
8. vérifier l’accessibilité ;
9. exécuter les tests ;
10. mettre à jour la matrice de parité ;
11. documenter les écarts éventuels ;
12. présenter les résultats avant de poursuivre.

Ne te limite pas à produire des maquettes ou du pseudo-code après validation : génère réellement le code nécessaire, intègre-le au projet et vérifie son fonctionnement.

Respecte les conventions du dépôt. N’introduis une nouvelle bibliothèque que si :

- elle répond à un besoin réel ;
- elle est compatible avec la stack ;
- elle ne duplique pas une solution existante ;
- son poids et sa maintenance sont acceptables ;
- son introduction est documentée.

---

# 17. CRITÈRES D’ACCEPTATION

La mission ne peut être considérée comme terminée que si :

- l’intégralité du projet a été inventoriée ;
- chaque écran est documenté ;
- chaque fonctionnalité possède un identifiant ;
- chaque règle métier importante est documentée ;
- chaque calcul statistique critique est expliqué ;
- chaque endpoint est cartographié ;
- chaque fonctionnalité existante possède une correspondance dans la nouvelle interface ;
- les parcours critiques sont couverts par des tests ;
- les tests pertinents réussissent ;
- la nouvelle interface est responsive ;
- l’accessibilité a été contrôlée ;
- les états de chargement, vides et d’erreur sont traités ;
- les graphiques restent fidèles aux données ;
- les performances ne régressent pas sans justification ;
- aucun comportement métier n’a été silencieusement modifié ;
- aucun bouton, filtre, tri, export, import ou workflow n’a disparu ;
- la documentation est à jour ;
- les écarts restants sont explicitement listés.

---

# 18. CHECKLIST ANTI-RÉGRESSION

Avant de déclarer la refonte terminée, vérifie individuellement :

- toutes les routes ;
- tous les écrans ;
- tous les onglets ;
- tous les boutons ;
- tous les formulaires ;
- toutes les validations ;
- tous les filtres ;
- tous les tris ;
- toutes les recherches ;
- toutes les paginations ;
- tous les imports ;
- tous les exports ;
- tous les tableaux ;
- toutes les colonnes ;
- tous les graphiques ;
- toutes les légendes ;
- tous les tooltips ;
- toutes les modales ;
- tous les menus ;
- toutes les permissions ;
- toutes les erreurs ;
- tous les états vides ;
- tous les états de chargement ;
- tous les calculs statistiques ;
- toutes les comparaisons ;
- toutes les agrégations ;
- tous les comportements responsive ;
- tous les parcours critiques.

Pour chaque élément, indique l’un des statuts suivants :

- conservé à l’identique ;
- amélioré sans changement métier ;
- déplacé avec nouvelle localisation documentée ;
- remplacé par une alternative équivalente ;
- modifié avec justification et validation ;
- bloqué avec raison explicite.

Aucun élément ne doit rester sans statut.

---

# 19. FORMAT DE TES RÉPONSES

Travaille de manière factuelle et traçable.

Pour chaque constat important, cite les fichiers, classes, fonctions, composants, routes ou endpoints concernés.

Sépare clairement :

- ce qui est observé dans le code ;
- ce qui est observé à l’exécution ;
- ce qui est déduit ;
- ce qui est recommandé ;
- ce qui reste à confirmer.

Présente les problèmes par ordre de sévérité et d’impact.

Évite les recommandations génériques. Chaque proposition doit être adaptée au projet réellement analysé.

Ne prétends jamais avoir :

- analysé un fichier non lu ;
- exécuté un test non lancé ;
- vérifié un écran non observé ;
- validé une fonctionnalité non testée ;
- mesuré une performance non mesurée.

---

# 20. OBJECTIF FINAL

Le résultat attendu n’est pas un simple changement de couleurs ou une modernisation superficielle.

L’objectif est d’obtenir une application de statistiques sportives :

- plus claire ;
- plus rapide ;
- plus cohérente ;
- plus agréable ;
- plus professionnelle ;
- plus accessible ;
- plus facile à maintenir ;
- plus facile à faire évoluer ;
- mieux adaptée à l’analyse intensive de données ;
- strictement fidèle aux fonctionnalités et aux règles métier existantes.

Commence maintenant par la phase d’état initial et l’inventaire exhaustif du projet.

Ne modifie aucun fichier applicatif avant d’avoir terminé et présenté l’ensemble des livrables préparatoires obligatoires.
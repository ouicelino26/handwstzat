# Matrice des ecarts HandWStat Ultimate

Etats utilises : `EXISTE ET VALIDE`, `EXISTE PARTIELLEMENT`, `ABSENT`, `A REFACTORER`, `OBSOLETE`, `A CONFIRMER`, `BLOQUE PAR L'API`.

| ID | Ecran | Fonction | Etat actuel | Cible | Ecart | Dependance API | Risque | Priorite | Fichiers | Tests |
|---|---|---|---|---|---|---|---|---|---|---|
| STAT-UI-001 | Tous | Ratio denominateur nul | EXISTE ET VALIDE | `null` et `N/A` | Corrige sur helper et nouveau contrat ; helpers historiques restent a migrer | Aucune | Eleve | P0 | `KpiModels.cs`, `MetricDisplayModels.cs` | zero, NaN, format N/A |
| STAT-UI-002 | Dashboard/Joueuses/Compare | Tirs contres dans tentatives | EXISTE ET VALIDE | Une occurrence par tir | Replis locaux corriges ; `TirContre` reste visible comme sous-detail | API v1 corrigee | Critique | P0 | `KpiModels.cs`, `StatsDashboardService.cs`, `Players.razor` | fixture 2 buts + 5 echecs = 7 |
| STAT-UI-003 | Dashboard | Taux avec preuve | EXISTE PARTIELLEMENT | Valeur + numerateur + denominateur + seuil | Hero migre ; autres cartes historiques a convertir | Contrat metrique v2 souhaitable | Eleve | P0/P1 | `RateMetricCard.razor`, `Home.razor.cs` | valeur, N/A, volumes |
| STAT-UI-004 | Dashboard | Buts encaisses gardienne | EXISTE ET VALIDE | Addition ouverte + 7 m une fois | Double ajout 7 m du spotlight corrige | DTO v1 combine deja | Eleve | P0 | `DashboardModels.cs` | mapping a verrouiller |
| STAT-UI-005 | Tous | Per-match nul | EXISTE PARTIELLEMENT | N/A sans match | Le dashboard hero est nullable ; autres ecrans retournent encore 0 | Aucune | Moyen | P1 | `KpiModels.cs`, pages Razor | denominateur 0 |
| STAT-UI-006 | Profils | Per-60 | EXISTE PARTIELLEMENT | Valeur nullable + qualite temps | DTO v1 projette 0 ; provenance et doublons de temps non exposes | Temps fiable / contrat v2 | Eleve | P2 | `PositionProfiles*`, Core DTO | zero minute, temps anormal |
| STAT-UI-007 | Dashboard/Equipe du jour | PIE | A REFACTORER | PIE versionne, explicable, volume minimal | Score local historique, backend confirme PIE absent ; libelle rendu exploratoire | BLOQUE PAR L'API | Eleve | P1 | `TeamOfTheDay*`, `Dashboard.razor` | formule/version/cohorte |
| STAT-UI-008 | Dashboard | Equipe type | EXISTE PARTIELLEMENT | Selection contractuelle par role | Calcul client, nombreux appels, seuils locaux | Endpoint API v2 absent | Moyen | P1 | `TeamOfTheDayService.cs` | postes, egalites, volumes |
| STAT-UI-009 | Equipes | Points par match | A CONFIRMER | Regle de competition explicite | Hypothese 2 points victoire, 1 nul | Metadonnees competition | Moyen | P1 | `Teams.razor` | regles de points |
| STAT-UI-010 | Tous | Ballons valorises | A REFACTORER | Nom/formule versionnes | Ratio passes decisives / (passes + pertes), pas un taux de possession | Possessions futures | Moyen | P1/P3 | pages et `KpiModels.cs` | zero actions, wording |
| STAT-UI-011 | Matchs | Jeu prepare | A REFACTORER | Numerateur/denominateur explicites | Divise assists par score final ; relation passe-but non materialisee | Liaison evenementielle future | Moyen | P1/P2 | `MatchKpiCatalog.cs` | score zero, incoherence |
| STAT-UI-012 | Matchs/Joueuses | Impact defensif recent | A REFACTORER | Meme definition partout | Tendance recente = interceptions + arrets ; global = 4 actions defense | DTO match enrichi | Moyen | P1 | `KpiModels.cs`, `Players.razor` | parite des perimetres |
| STAT-UI-013 | Compare/Profils | Percentiles | EXISTE PARTIELLEMENT | Cohorte, taille, seuil, version visibles | Rang present, metadonnees incomplètes | Contrat v2 | Moyen | P2 | `PositionProfiles*` | petites cohortes, egalites |
| UX-001 | Dashboard | Scope visible complet | EXISTE ET VALIDE | 7 dimensions visibles | Ajoute sur la tranche dashboard | Date serveur future | Faible | P0 | `AnalysisScopeSummary.razor` | rendu scope |
| UX-002 | Tous | Qualite des donnees | EXISTE PARTIELLEMENT | Badge et anomalies reels | Composants disponibles ; valeur `Unknown` tant que l'API ne renseigne rien | Rapport DQ v2 absent | Eleve | P0/P2 | `DataQuality*.razor` | Unknown/Low/Medium/High |
| UX-003 | Dashboard | Langue francaise | EXISTE PARTIELLEMENT | Francais par defaut | Navigation de section corrigee ; dette ailleurs | Aucune | Faible | P1 | pages/composants | revue de contenu |
| UX-004 | Tous | Tone accessible | EXISTE PARTIELLEMENT | Texte + icone + couleur | Nouveau composant conforme ; anciens KPI encore souvent couleur/classe | Aucune | Moyen | P0/P1 | `RateMetricCard.razor`, `KpiTileGrid.razor` | HTML accessible |
| UX-005 | Dashboard | Tableaux accessibles | EXISTE PARTIELLEMENT | `scope`, caption, tri annonce | Certaines entetes n'ont pas `scope="col"` | Aucune | Moyen | P1 | `Dashboard.razor` | clavier/lecteur ecran |
| UX-006 | Mobile | Densite et progressive disclosure | EXISTE PARTIELLEMENT | Sections courtes et chargees a la demande | Dashboard progresse ; Joueuses/Compare restent denses | Aucune | Moyen | P1 | CSS/pages | viewport Android |
| PERF-001 | Dashboard | Appels initiaux | EXISTE PARTIELLEMENT | 3-5 appels agreges | ~28 appels initiaux ; equipe du jour retiree du chemin critique | Endpoint dashboard v2 | Eleve | P1 | services dashboard/API | compteur appels, charge |
| PERF-002 | Dashboard | Annulation | EXISTE ET VALIDE | Dernier scope gagnant | CTS + gate sur dashboard ; ranking CTS | Aucune | Eleve | P0 | `Home.razor.cs`, `Dashboard.razor` | requete obsolete |
| PERF-003 | Dashboard | Lazy loading | EXISTE ET VALIDE | Sections secondaires a la demande | Equipe du jour chargee a l'ouverture | Aucune | Moyen | P0 | dashboard/services | pas d'appel initial |
| PERF-004 | Referentiels | Cache et concurrence | EXISTE PARTIELLEMENT | Cache session atomique | Cache present, double chargement concurrent possible | Aucune | Faible | P1 | `ReferenceDataService.cs` | concurrence |
| ARCH-001 | Dashboard | Service focalise | EXISTE PARTIELLEMENT | Orchestration separee des builders/loaders | `DashboardSnapshotBuilder` extrait ; service reste volumineux | Aucune | Moyen | P0/P1 | `StatsDashboardService.cs`, builder | mapping overview |
| ARCH-002 | API | Gateway v1/v2 | EXISTE PARTIELLEMENT | Toutes tranches via gateway | Dashboard migre ; autres pages directes | DTO/endpoint v2 futurs | Moyen | P0/P1 | `Services/Analytics/*` | delegation v1 |
| ERR-001 | Tous | Erreur utilisateur typee | EXISTE ET VALIDE | Corps brut jamais affiche | Corrige dans la base HTTP ; details uniquement diagnostic debug | ProblemDetails partiel | Eleve | P0 | `ApiClientBase.cs`, `ApiRequestException.cs` | 400/401/404/429/500 |
| API-001 | Tous | Scope homogene | EXISTE PARTIELLEMENT | Objet scope resolu commun | Routes v1 heterogenes, spatial omet certains filtres | API v2 | Moyen | P1 | clients/query builder | matrice route/filtre |
| API-002 | Tous | Qualite/provenance | BLOQUE PAR L'API | Rapport DQ et fraicheur | Aucun endpoint | API Phase 2 | Eleve | P2 | futur | anomalies, fraicheur |
| MODEL-001 | Futur | Possessions/pace/per-100 | BLOQUE PAR L'API | Moteur deterministe | Absent | API Phase 3 | Eleve | P3 | futur | possession fixture |
| MODEL-002 | Futur | Lineups/on-off | BLOQUE PAR L'API | Intervalles fiables | Absent | API Phase 4 | Eleve | P4 | futur | exclusions, 7v6 |
| MODEL-003 | Futur | xG/xS | BLOQUE PAR L'API | Modele calibre/versionne | Absent | API Phase 5 | Eleve | P5 | futur | calibration/drift |
| PRODUCT-001 | Futur | Scouting/video/offline | A CONFIRMER | Selon contexte maitre | Contexte maitre HandWStat manquant | API Phase 6 + contexte | Moyen | P6 | futur | permissions/offline |
| UPDATE-001 | Mise a jour | Non-regression | EXISTE ET VALIDE | Workflow preserve | Aucun changement fonctionnel | API releases existante | Critique | Permanent | `Services/Updates`, composants update | 31 tests historiques |

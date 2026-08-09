# Etat actuel de HandWStat Ultimate

Date de l'audit : 2026-07-29

Branche d'implementation : `feature/ultimate-analytics-ui-foundation-v1`

HEAD de depart : `f791584`

Portee : audit complet, Phase 0 et fondations UX Phase 1A uniquement.

## Sources et niveau de preuve

`CONTEXT_MASTER_HANDWSTAT_ULTIMATE.md` n'a pas ete trouve dans le depot, les depots voisins de `C:\Users\donov\source\repos`, `D:\repos`, l'archive locale `docs.zip` ni le profil utilisateur. Les fonctions qui dependent exclusivement de cette cible sont donc marquees `A CONFIRMER` et non inventees.

La photographie backend a ete trouvee et lue dans `D:\repos\HandballManagerAPI` :

- `docs/ai/ULTIMATE_API_CURRENT_STATE.md` ;
- `docs/ai/ULTIMATE_API_GAP_MATRIX.md` ;
- `docs/ai/ULTIMATE_API_ROADMAP.md` ;
- `docs/statistics/METRIC_CATALOG.md` ;
- `docs/statistics/STATISTICAL_RULES.md`.

Le backend confirme une API v1 active, un noyau interne nullable, mais des DTO v1 `double` qui projettent encore un taux non calculable vers `0`. Aucun endpoint v2 n'est consomme par HandWStat.

## Synthese

HandWStat est une application .NET MAUI 10 Blazor Hybrid, composee d'ecrans Razor volumineux et de clients HTTP v1. La couverture fonctionnelle est large : dashboard, joueuses, equipes, matchs, comparaison, profils de poste, graphiques spatiaux et mises a jour automatiques. La fiabilite d'affichage etait toutefois limitee par des divisions a zero masquees, l'absence de preuve visible pour plusieurs taux, des erreurs API brutes et un dashboard charge en un grand nombre d'appels.

La tranche Phase 0 introduit un contrat d'affichage nullable, corrige le double comptage local des tirs contres, rend le scope et la qualite visibles, assainit les erreurs et charge l'equipe de la journee a la demande. La compatibilite API v1 est conservee par `IAnalyticsGateway` et `V1AnalyticsGateway`.

| Domaine | Etat | Observation verifiee |
|---|---|---|
| MAUI / Blazor Hybrid | EXISTE ET VALIDE | Cibles Android, iOS, MacCatalyst et Windows ; Windows ajoute conditionnellement le TFM local. |
| API statistique v1 | EXISTE ET VALIDE | Clients dedies pour overview, rankings, joueuses, equipes, matchs, spatial et comparaison. |
| Adaptateur analytics | EXISTE PARTIELLEMENT | `IAnalyticsGateway` et `V1AnalyticsGateway` couvrent la premiere tranche dashboard ; les autres pages appellent encore directement les clients v1. |
| Ratios non calculables | EXISTE ET VALIDE | `Ratio` retourne `double?`; `FormatRatio(null)` retourne `N/A`; le composant commun respecte ce cas. |
| Preuve des taux | EXISTE PARTIELLEMENT | Dashboard principal migre ; les pages Joueuses, Equipes, Matchs et Compare utilisent encore majoritairement `KpiTile`. |
| Scope visible | EXISTE ET VALIDE sur la tranche | Competition, equipe, saison, journee, periode, matchs et date d'actualisation sont visibles sur le dashboard. |
| Qualite des donnees | EXISTE PARTIELLEMENT | Base UI `Unknown/Low/Medium/High`; l'API v1 n'expose pas encore de rapport DQ, donc le dashboard affiche explicitement `Qualite non renseignee`. |
| Erreurs utilisateur | EXISTE ET VALIDE | `ApiRequestException` separe message utilisateur, code technique, correlation, retry et statut ; le corps brut reste dans les diagnostics de developpement. |
| Annulation | EXISTE ET VALIDE sur le dashboard | Les chargements precedents sont annules et serialises ; le ranking annule aussi la selection obsolete. |
| Lazy loading | EXISTE ET VALIDE | L'equipe de la journee n'est plus chargee au premier affichage. |
| Cache | EXISTE PARTIELLEMENT | Le referentiel possede un cache de session ; aucun cache statistique incoherent n'a ete ajoute. |
| PIE / equipe type contractuels | ABSENT cote API | Une formule locale historique existe. L'UI la renomme `score technique local exploratoire`; elle ne doit pas etre confondue avec un PIE versionne. |
| Possessions, lineups, xG/xS | ABSENT | BLOQUE PAR L'API et par la qualite evenementielle/temps de jeu. |
| Scouting, video, offline live, narration IA | ABSENT | Planifies, non simules. |
| Mise a jour automatique | EXISTE ET VALIDE | Services, ecrans et tests preserves ; aucun fichier de ce domaine n'est modifie. |

## Architecture auditee

### Services

| Element | Responsabilite actuelle | Constat | Etat |
|---|---|---|---|
| `StatsDashboardService` | Orchestre overview, rankings, liste, spotlight, spatial, matchs et tables globales | Service encore volumineux ; `DashboardSnapshotBuilder` extrait maintenant le mapping overview/annuaire. Les fallbacks et mappings spatiaux restent a extraire par petites tranches. | A REFACTORER |
| `StatsApiClient` | Surface `/api/Stats/*` v1 | Contrat clair, tokens transmis. Aucun endpoint v2 fictif. | EXISTE ET VALIDE |
| `ApiClientBase` | URI, auth, JSON et erreurs | Avant Phase 0, exposait statut, raison et corps brut. Desormais resultat d'erreur type et message sur. | EXISTE ET VALIDE |
| `ReferenceDataService` | Charge sept referentiels en parallele | Cache de session present, invalide au changement de session ; pas de verrou anti-double chargement interne. | EXISTE PARTIELLEMENT |
| `TeamOfTheDayService` | Charge journee, cohorte, comparaison et matchs ; score local | Peut faire `3 + N matchs` appels ; charge desormais a la demande. Le nom PIE n'est pas contractuel API. | A REFACTORER |
| `AnalysisScopeService` | Scope global competition/equipe/saison/journee | Snapshot global encore limite a six champs ; le composant dashboard enrichit localement periode, volume et date. | EXISTE PARTIELLEMENT |

### Ecrans et composants

| Ecran | Taille/complexite observee | Appels et rendu | Risques |
|---|---|---|---|
| Dashboard | `Dashboard.razor` ~14k tokens, `StatsDashboardService` ~7k tokens | Chargement initial estimatif : 28 appels au premier passage avant cache ; 2 graphiques Apex seulement selon la section visible. | Responsabilites dispersees entre Razor, base et service ; cout reseau Android. |
| Joueuses | `Players.razor` ~24.5k tokens | 8 blocs stats paralleles, spatial, profil et matchs ; recherche debouncee. | Fichier monolithique, nombreux calculs KPI dupliques, tableaux/graphiques denses. |
| Equipes | `Teams.razor` ~10k tokens | Stats, deux listes joueuses et detail match. | Requete roster dupliquee avec scopes differents ; metriques locales non prouvees. |
| Matchs | `Matches.razor` ~14k tokens | Liste, resume, joueuses, spatial et graphiques ; une protection par jeton existe pour le spatial. | Sections lourdes et appels conditionnels complexes. |
| Compare | `Compare.razor` volumineux | Profil compare et jusqu'a cinq graphiques. | Calculs dupliques, fort cout de rendu et densite mobile. |
| Profils de poste | Razor + code-behind tres volumineux | Histogramme, radar, scatter, exports SVG. | Per-60 depend du temps importe ; cohortes/seuils non exposes en v1. |
| Composants KPI historiques | `KpiTileGrid`, `BarGaugeKpi*` | Valeur/caption/tone, preuve parfois textuelle. | `tone` historiquement surtout visuel ; absence d'etat nullable structure. |
| Composants Phase 0 | `RateMetricCard`, `DataQuality*`, `AnalysisScopeSummary` | Valeur, unite, volume, fiabilite, tooltip, texte+icone, liens et N/A. | Migration limitee volontairement au dashboard. |

### Modeles analytics et helpers KPI

- `HandballKpiHelper` centralise volumes, taux, scores de presentation et seuils de tone.
- Le defaut `Ratio(5, 0) = 5` etait confirme. Il est corrige par un retour nullable.
- `TirsRates` contient deja les tirs contres. Les replis `ShotAttempts`, `ShotWaste`, dashboard et score technique Joueuses les ajoutaient une seconde fois. Ils sont corriges.
- `PlayerTechnicalStatsDto.GoalkeeperConcededGoals` inclut deja les buts ouverts et 7 m. Le spotlight les additionnait une seconde fois ; ce cumul est corrige.
- Les helpers historiques `PerMatch`, `Share` et `SuccessVsWasteShare` retournent encore `0` si le denominateur est vide sur les ecrans non migres. Ils sont `A REFACTORER` progressivement vers le contrat nullable.

## Audit UX

Points positifs : filtres globaux, modes de lecture, etats de chargement, composants spatiaux avec roles/labels, textes alternatifs sur portraits/logos, focus global visible et base `prefers-reduced-motion` deja presente.

Ecarts principaux :

- melange francais/anglais (`Morning brief`, `Pulse`, `Team day`, `Data index`) corrige sur le selecteur principal du dashboard, mais encore present ailleurs ;
- tableaux du dashboard sans `scope="col"` sur plusieurs entetes ;
- densite importante sur mobile, notamment Compare, Joueuses et profils de poste ;
- taux parfois affiches sans volume ou avec un denominateur force a `1` ;
- qualite et fraicheur absentes avant Phase 0 ;
- score local historique presente comme PIE, corrige dans la tranche mais a retirer/remplacer lors du contrat API ;
- liens joueuse/equipe/match inegaux selon les tableaux ;
- ApexCharts est une dependance globale lourde ; les sections non visibles ne rendent pas tous les graphiques, mais la bibliotheque reste chargee par l'application.

## Audit performance

Premier chargement dashboard avant Phase 0, sans cache referentiel :

- 7 appels referentiels ;
- 3 appels de listes de matchs pour construire les filtres ;
- 7 appels de fondation dashboard (overview, joueuses, quatre rankings, matchs recents) ;
- 1 comparaison globale ;
- 10 appels spotlight ;
- equipe de la journee : 3 appels fixes, un eventuel rafraichissement, puis un appel par match de journee.

Soit environ 28 appels avant l'equipe de la journee, et `31 + N` lorsqu'elle etait chargee. Le cache referentiel retire 7 appels lors des chargements suivants. Les appels sont paralleles, mais l'API calcule encore beaucoup d'agregats en memoire. Sur Android, les couts principaux sont la radio reseau, la deserialisation des grandes cohortes et les graphiques ; sur Windows, le cout est surtout CPU/memoire et re-rendu.

Phase 0 retire `3 + N` appels du chemin initial, annule les scopes obsoletes, serialise le chargement dashboard et journalise la duree en `DEBUG`. Un endpoint agrege dashboard/profil v2 reste necessaire pour atteindre un budget de 3 a 5 appels.

## Tests de reference

Avant modification : 31 tests reussis, tous consacres au systeme de mise a jour.

La Phase 0 ajoute des tests pour ratios, `N/A`, volumes v1, qualite inconnue, tirs contres, mapping du snapshot, erreur API typee et rendu HTML des composants. Les resultats finaux sont consignes dans `ULTIMATE_HANDWSTAT_PROGRESS.md`.

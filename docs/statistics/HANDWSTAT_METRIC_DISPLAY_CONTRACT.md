# Contrat d'affichage des metriques HandWStat

Version : `1.0-ui-phase0`

Source backend de reference : catalogue statistique API `1.0`, echelle des pourcentages v1 `0-100`.

## Invariants

1. Une valeur non calculable est `null` dans le modele UI et `N/A` a l'ecran.
2. Zero est une valeur seulement si le calcul est possible et donne reellement zero.
3. Un taux conserve : code, libelle, valeur, numerateur, denominateur, unite, seuil, fiabilite, qualite, tooltip et tone.
4. Si la v1 ne fournit pas de volume et qu'il n'existe pas dans un autre champ du meme contrat, l'UI affiche `Volume non fourni par l'API`.
5. Le client ne reconstruit pas un numerateur ou denominateur a partir d'une hypothese metier.
6. Un sous-type comme `TirContre` reste visible mais n'est pas rajoute a `TirsRates`, qui le contient deja.
7. Les pourcentages v1 restent sur l'echelle `0-100`.
8. Le tone ne porte jamais seul le sens : icone et texte sont obligatoires.
9. `Unknown` est le niveau de qualite par defaut ; il ne devient jamais `High` sans signal API.
10. Le scope visible accompagne toute comparaison : competition, equipe, saison, journee, periode, matchs et date d'actualisation/generation connue.

## Modele UI

`RateDisplayModel` expose :

| Champ | Contrat |
|---|---|
| `MetricCode` | Code stable UI/API, jamais le libelle. |
| `Label` | Francais court, sens metier non ambigu. |
| `Value` | `double?`; `null` si denominateur nul/invalide. |
| `Numerator` | Valeur source, nullable si absente de v1. |
| `Denominator` | Volume source, nullable si absent de v1. |
| `Unit` | `%`, `buts/match`, `/60 min`, `compte`, etc. |
| `SampleReliable` | Vrai uniquement si volume connu et seuil atteint. |
| `MinimumSample` | Seuil versionne ou `null` si non defini. |
| `QualityLabel` | Etat echantillon/qualite lisible. |
| `Tooltip` | Formule et limite principale en langage simple. |
| `Tone` | `neutral/good/positive/warning/danger`, toujours double d'un texte/icone. |

## Seuils confirmes par l'API

| Metrique | Minimum | Orientation | Version |
|---|---:|---|---|
| Reussite tir ouvert | 4 tentatives ouvertes | Haut | 1.0 |
| Reussite 7 m | 2 tentatives 7 m | Haut | 1.0 |
| Taux d'arret | 10 tirs subis | Haut | 1.0 |

Les autres minimums sont `A CONFIRMER`. Le dashboard Phase 0 utilise `1` uniquement pour distinguer calculable/non calculable sur les ratios descriptifs ; ce n'est pas un seuil de comparaison metier.

## Catalogue d'affichage actuel

Abreviations de source : `API` = champ v1 direct ; `LOCAL` = transformation explicite de champs v1 ; `ABSENT` = dependance non disponible.

### Dashboard - hero et ligue

| Code / libelle | Source | Formule | Unite | Numerateur / denominateur | Volume UI | Scope | Cas zero | Tone / seuil / percentile | Fiabilite |
|---|---|---|---|---|---|---|---|---|---|
| `ACTIVE_TEAMS` Equipes actives | API `Overview.TeamCount` | compte | compte | equipes / - | valeur | scope dashboard | 0 | neutre, aucun percentile | compte fiable selon reponse |
| `GOALS_PER_MATCH` Cadence offensive | LOCAL | buts totaux / matchs | buts/match | `GoalCount+PenaltyGoalCount` / `MatchCount` | affiche  | scope dashboard | N/A sans match | positif presentation, seuil metier absent | calculable si matchs > 0 |
| `ASSISTED_GOAL_SHARE` Jeu prepare | LOCAL | assists / buts totaux x100 | % | `AssistCount` / buts totaux | affiche | scope dashboard | N/A sans but | `FieldSuccessRateTone`; percentile absent | descriptif, relation passe-but non materialisee |
| `INTERCEPTIONS_PER_MATCH` | LOCAL | interceptions / matchs | /match | `InterceptionCount` / `MatchCount` | contexte historique | scope dashboard | actuellement 0 hors composant Phase 0 | neutre | A REFACTORER nullable |
| `SAVES_PER_MATCH` | LOCAL | arrets / matchs | /match | `SaveCount` / `MatchCount` | contexte historique | scope dashboard | actuellement 0 | tone arrets | A REFACTORER nullable |
| `TURNOVERS_PER_MATCH` | LOCAL | pertes / matchs | /match | `TurnoverCount` / `MatchCount` | contexte historique | scope dashboard | actuellement 0 | warning | A REFACTORER nullable |
| `SANCTIONS_PER_MATCH` | LOCAL | sanctions / matchs | /match | `SanctionCount` / `MatchCount` | contexte historique | scope dashboard | actuellement 0 | warning | A REFACTORER nullable |

### Dashboard - spotlight joueuse

| Code / libelle | Source | Formule | Unite | Numerateur / denominateur | Cas zero et volume | Tone / minimum | Fiabilite |
|---|---|---|---|---|---|---|---|
| `DIRECT_ACTIONS_PER_MATCH` | LOCAL | (buts + passes decisives) / matchs | /match | directes / matchs | N/A cible ; contexte volume actuel | seuils UI par role, non contractuels | A CONFIRMER |
| `CREATION_VS_WASTE` Ballons valorises | LOCAL | assists / (assists + pertes) x100 | % | assists / actions retenues | N/A cible si aucune action | seuils UI role, pas un taux de possession | A REFACTORER nom/formule |
| `DEF_IMPACT_PER_MATCH` | LOCAL | 4 actions defense / matchs | /match | impact / matchs | N/A cible | seuils UI role | formule brute API 1.0, exposition non ajustee |
| `SAVES_PER_MATCH` gardienne | LOCAL | arrets ouverts+7m / matchs | /match | arrets / matchs | N/A cible | seuils UI gardienne | volume matchs connu |
| `OPEN_SHOT_RATE` | API technique/offense | buts ouverts / tentatives ouvertes x100 | % | `Buts` / `Buts+TirsRates` | volume reconstructible depuis champs v1 du meme DTO | minimum 4, tone 70/55/45 | fiable si >=4 |
| `GOALKEEPER_SAVE_RATE` | API | arrets / tirs subis x100 | % | `Arrets+ArretsPenalty` / `TirsSubis` | volume champs v1 | minimum 10, tone 40/34/28 | fiable si >=10 |
| `OVERALL_SHOT_RATE` | API technique ou LOCAL fallback | buts totaux / tentatives totales x100 | % | buts / tentatives | tirs contres inclus une fois | minimum A CONFIRMER | non calculable sans tentative |
| `GOALKEEPER_7M_RATE` | API technique ou LOCAL fallback | arrets 7m / tirs 7m subis x100 | % | arrets 7m / (arrets 7m+buts 7m) | volume champs v1 | minimum A CONFIRMER, tone 35/25/15 | petit echantillon |
| `SHOT_ATTEMPTS_PER_MATCH` | API technique | tentatives / matchs | /match | tentatives / matchs | volume affiche | tone warning de volume | volume, pas performance |
| `SHOT_WASTE_PER_MATCH` | API technique | echecs / matchs | /match | echecs / matchs | volume affiche | bas favorable 1.5/3/5 | A CONFIRMER |
| `GOALS_CONCEDED_PER_MATCH` | API technique combine | buts encaisses / matchs | /match | buts encaisses / matchs | 7m compte une fois | bas favorable 22/26/30 | A CONFIRMER |
| `TECHNICAL_LOSSES_PER_MATCH` | API technique | pertes techniques / matchs | /match | pertes / matchs | volume affiche | bas favorable | A CONFIRMER |
| `SANCTIONS_PER_MATCH_PLAYER` | API/LOCAL | total sanctions / matchs | /match | sanctions / matchs | volume affiche | bas favorable | categories de gravite melangees |

### Joueuses et comparaison

| Famille | Sources/formules actuelles | Volume | Cas zero | Contrat cible |
|---|---|---|---|---|
| Cartes brutes | matchs, buts, passes, interceptions, arrets, pertes depuis `PlayerGlobalStatsDto` | compte visible | 0 valide | conserver compte + scope |
| Tir ouvert | `Offense.TauxReussiteTir` ou `Global.ShotSuccessRate` | parfois base `Buts/(Buts+TirsRates)` | v1 0 ambigu | `OPEN_SHOT_RATE`, minimum 4, nullable |
| Penalty | `Offense.TauxReussitePenalty` ou global | volume rarement visible | v1 0 ambigu | `PENALTY_RATE`, minimum 2, nullable |
| Arret | `Goalkeeper.TauxArret` | tirs subis disponible | v1 0 ambigu | minimum 10, nullable |
| Actions directes | buts + assists par match | matchs connu | helper retourne 0 | nullable si 0 match |
| Ballons valorises | passes decisives / (passes + pertes) | volume connu si passing present ; fallback global Compare | helper retourne 0 | nom explicite et N/A sans action |
| Impact defensif | interceptions+contres+neutralisations+passages forces / match | matchs connu | helper retourne 0 | meme perimetre dans toutes les vues |
| Tirs rates/match | `TirsRates+PenaltyRate` / matchs | matchs et echecs connus | helper retourne 0 | ne jamais rajouter `TirContre` |
| Score technique | positifs / (positifs+negatifs) x100 | volume local | 0 ambigu | renommer local exploratoire ou contractualiser |
| Tendance recente | directes, `interceptions+saves`, pertes, arrets sur 8 matchs | match par point | 0 valide | ne pas appeler le second `impact defensif` sans qualifier |
| Compare volumes | buts, passes, interceptions, arrets, pertes | visible en tableau/graphe | 0 valide | scope commun obligatoire |
| Compare production | directes, defense, pertes, sanctions par match | matchs par joueuse | 0 ambigu | nullable et volume dans tooltip/table |

### Equipes

| Code / libelle | Formule | Unite | Numerateur / denominateur | Cas zero | Statut |
|---|---|---|---|---|---|
| `POINTS_PER_MATCH` | (2*victoires+nuls)/matchs | points/match | points / matchs | N/A cible | A CONFIRMER selon competition |
| `WIN_RATE` | victoires/matchs x100 | % | victoires / matchs | N/A cible | formule simple, seuil absent |
| `GOAL_DIFF_PER_MATCH` | (buts pour-buts contre)/matchs | buts/match | difference / matchs | N/A cible | signe a conserver |
| `GOALS_FOR_PER_MATCH` | buts pour/matchs | /match | buts / matchs | N/A cible | volume connu |
| `GOALS_AGAINST_PER_MATCH` | buts contre/matchs | /match | buts / matchs | N/A cible | bas favorable |
| `CREATION_VS_WASTE_TEAM` | assists/(assists+pertes) x100 | % | assists / actions retenues | N/A cible | pas possession |
| `SHOT_ATTEMPTS_PER_MATCH_TEAM` | technique tentatives/matchs | /match | tentatives / matchs | N/A cible | tir contre une fois |
| `SHOT_WASTE_PER_MATCH_TEAM` | technique echecs/matchs | /match | echecs / matchs | N/A cible | bas favorable |
| `SAVES_PER_MATCH_TEAM` | arrets/matchs | /match | arrets / matchs | N/A cible | volume connu |
| `GOALKEEPER_7M_RATE_TEAM` | arrets 7m/tirs 7m subis | % | API technique | v1 0 ambigu | minimum A CONFIRMER |
| `OVERALL_SHOT_RATE_TEAM` | buts/tentatives totales | % | API technique | v1 0 ambigu | minimum A CONFIRMER |

### Matchs

| Code / libelle | Formule | Numerateur / denominateur | Cas zero | Limite |
|---|---|---|---|---|
| `MATCH_TOTAL_GOALS` | scores finaux additionnes | score1+score2 / - | 0 valide | depend coherence score/evenements |
| `FINAL_MARGIN_ABS` | abs(score1-score2) | - | 0 = nul | resume absolu |
| `FINAL_MARGIN_SIGNED` | score1-score2 | - | 0 = nul | scenario signe |
| `ASSISTED_GOAL_SHARE_MATCH` | assists / score total x100 | assists / score | N/A cible si 0 | relation passe-but non materialisee |
| `CREATION_VS_WASTE_MATCH` | assists/(assists+pertes) x100 | assists / actions retenues | N/A cible | pas possession |
| `DEF_ACTIONS_MATCH` | interceptions+arrets | compte | 0 valide | perimetre simplifie |
| `SCORERS_3_PLUS` | nombre joueuses avec >=3 buts | compte | 0 valide | seuil UX local |
| Timeline | pause, leads max, renversements, runs, buts 2e MT | snapshots score | vide si timeline absente | regles dans `MatchScenarioAnalyzer` |

### Profils de poste et per-60

| Metrique | Formule API | Unite | Denominateur | Cas zero | Qualite |
|---|---|---|---|---|---|
| Buts, passes, interceptions, arrets, pertes, sanctions /60 | action/minutes x60 | /60 min | temps importe | N/A cible ; v1 expose 0 | temps sans unicite, anomalies possibles |
| Percentile de role | rang empirique oriente | percentile | taille cohorte | N/A cible si cohorte vide | seuil/cohorte/version a exposer |

### Metriques absentes interdites a simuler

`PIE_GLOBAL` contractuel, possessions, pace, per-100, efficacite offensive/defensive, lineups, plus-minus, on/off, xG, xS, arrets au-dessus de l'attendu, forme, regularite et ajustement adversaire sont `BLOQUE PAR L'API` ou `ABSENT`.

## Regles de tone actuelles

Les seuils UI existants sont des aides de lecture et non des benchmarks statistiques. Ils doivent rester separes de `MinimumSample` et des percentiles. Toute evolution doit versionner : population, poste, saison, orientation et valeurs de coupure.

## Compatibilite v1

- `V1AnalyticsGateway` delegue sans changer les routes ni DTO.
- Si un taux v1 arrive sans volume, sa valeur peut etre affichee, mais `SampleReliable=false`, `QualityLabel=Qualite non renseignee` et le volume n'est pas invente.
- Si le denominateur est disponible et vaut 0, la valeur affichee est `N/A`, meme si le DTO v1 contient `0`.
- Un futur `V2AnalyticsGateway` pourra mapper directement `MetricValue/Sample/Quality` sans modifier les composants.

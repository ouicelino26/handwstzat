# Contrat d’intégration HandWStat — League Analytics

## Portée et source de vérité

Ce document décrit le contrat effectivement exposé par l’API à la version métrique `1.0`. Les noms JSON sont sensibles à la casse et utilisent le `camelCase`. Les formules, types et règles ci-dessous proviennent des DTO `League*`, de `MetricValue`, de `LeagueAnalyticsService`, du contrôleur v2, des tests Phase 1 et du Swagger généré.

Les colonnes de nullabilité prennent le code C# et le comportement testé comme source de vérité. Le Swagger généré marque certains `required string` comme `nullable: true`; ils restent non nullables dans le contrat C# et sont toujours renseignés par le service. À l’inverse, les sections non demandées sont réellement sérialisées à `null`, comme le vérifient les tests d’endpoint.

## Endpoint

```http
GET /api/v2/analytics/players/{playerId}
```

- OperationId Swagger : `GetLeaguePlayerAnalytics`.
- Authentification : Bearer requise ; rôles autorisés `Admin` ou `Consultation`.
- Réponse nominale : `200 application/json`, DTO `LeaguePlayerAnalyticsResponse`.
- Erreurs documentées : `400`, `401`, `403`, `404`, `409`, `500`, au format `ProblemDetails`.
- `404` signifie que le joueur n’existe pas. Un périmètre valide sans événement renvoie le joueur avec des compteurs à `0` et les taux non calculables à `null`.

### Paramètres exacts

| Nom | Emplacement | Type JSON / format | Requis | Règle |
|---|---|---|---|---|
| `playerId` | path | integer / int32 | oui | Identifiant du joueur, strictement supérieur à `0`. |
| `include` | query | string | non | Liste séparée par des virgules. Insensible à la casse ; espaces autour des éléments ignorés. Omission ou valeur blanche = toutes les sections. Une liste vide après normalisation ou une valeur inconnue = `400`. |
| `competitionId` | query | integer / int32 | non | Filtre compétition. |
| `teamId` | query | integer / int32 | non | Filtre équipe. |
| `matchId` | query | integer / int32 | non | Filtre match. |
| `from` | query | string / date-time | non | Date/heure de début inclusive ; la comparaison métier utilise la date. |
| `to` | query | string / date-time | non | Date/heure de fin inclusive ; `from.Date > to.Date` produit `400`. |
| `year` | query | integer / int32 | non | Filtre année. |
| `season` | query | string | non | Filtre saison. |
| `day` | query | string | non | Filtre journée/jour. |
| `attackId` | query | integer / int32 | non | Filtre contexte d’attaque. |
| `defenseId` | query | integer / int32 | non | Filtre contexte de défense. |
| `trigger` | query | string | non | Filtre déclencheur. |
| `shootShade` | query | string | non | Filtre nuance/type de tir. |

### Valeurs possibles de `include`

Les quatre seules valeurs autorisées sont :

- `overview`
- `offense`
- `defense`
- `goalkeeper`

Les sections demandées sont renvoyées dans `included`, triées ordinalement. Une requête sans `include` produit donc actuellement `["defense", "goalkeeper", "offense", "overview"]`. Une section non demandée reste présente dans l’objet réponse avec la valeur JSON `null`.

### Exemple de requête

```http
GET /api/v2/analytics/players/42?include=overview,offense,defense,goalkeeper&competitionId=12&season=2025-2026
Authorization: Bearer <token>
Accept: application/json
```

## DTO et noms JSON exacts

### `LeaguePlayerAnalyticsResponse`

| Propriété JSON | Type | Nullable | Rôle |
|---|---|---:|---|
| `playerId` | integer / int32 | non | Joueur demandé. |
| `metricVersion` | string | non | Toujours `"1.0"` pour ce contrat. |
| `included` | array de string | non | Sections effectivement demandées, triées. |
| `overview` | `LeaguePlayerOverview` | oui | `null` si `overview` n’est pas demandé. |
| `offense` | `LeagueAttackMetrics` | oui | `null` si `offense` n’est pas demandé. |
| `defense` | `LeagueDefenseMetrics` | oui | `null` si `defense` n’est pas demandé. |
| `goalkeeper` | `LeagueGoalkeeperMetrics` | oui | `null` si `goalkeeper` n’est pas demandé. |

### `LeaguePlayerOverview`

| Propriété JSON | Type | Nullable | Unité / sens |
|---|---|---:|---|
| `playerId` | integer / int32 | non | identifiant |
| `fullName` | string | non | nom complet |
| `teamId` | integer / int32 | oui | identifiant équipe |
| `teamName` | string | oui | nom équipe |
| `positionId` | integer / int32 | oui | identifiant poste |
| `positionCode` | string | oui | code poste |
| `positionName` | string | oui | libellé poste |
| `isGoalkeeper` | boolean | non | indicateur gardienne/gardien |
| `matchesPlayed` | integer / int32 | non | nombre de matchs distincts dans le périmètre |

### `LeagueCountMetric`

Ce DTO est utilisé par `offense.failedPivotPasses`.

| Propriété JSON | Type | Nullable | Règle |
|---|---|---:|---|
| `metricCode` | string | non | `"FAILED_PIVOT_PASSES"`. |
| `metricVersion` | string | non | `"1.0"`. |
| `value` | integer / int32 | oui | `null` avec les données actuelles. |
| `availability` | enum string | non | Une valeur parmi `AVAILABLE`, `PARTIALLY_AVAILABLE`, `UNAVAILABLE_FROM_CURRENT_DATA`, `AMBIGUOUS`, `REQUIRES_ADDITIVE_SCHEMA_CHANGE`, `PARTIAL`, `DATA_MISSING`. La réponse actuelle utilise `DATA_MISSING`. |
| `reason` | string | oui | Motif lisible ; actuellement `"MatchEvent requires an explicit FAILED_PIVOT_PASS subtype and a typed pivot target (TargetPlayerId, TargetPositionId or PassTargetCode)."`. |

### `MetricValue`, `MetricSample` et `MetricQuality`

Tous les taux sont des objets `MetricValue`, jamais `null`. Seul leur champ `value` peut être `null`.

| Objet | Propriété JSON | Type | Nullable | Règle |
|---|---|---|---:|---|
| `MetricValue` | `metricCode` | string | non | Code stable de la métrique. |
| `MetricValue` | `metricVersion` | string | non | `"1.0"`. |
| `MetricValue` | `value` | number / double | oui | Pourcentage arrondi à 2 décimales, `MidpointRounding.AwayFromZero`; `null` si le dénominateur est nul ou invalide. |
| `MetricValue` | `unit` | string | non | Toujours `"percent"` pour les taux de ce contrat. |
| `MetricValue` | `sample` | `MetricSample` | non | Échantillon exact. |
| `MetricValue` | `quality` | `MetricQuality` | non | Qualité calculée. |
| `MetricValue` | `numerator` | number / double | oui | Copie en lecture seule de `sample.numerator`. |
| `MetricValue` | `denominator` | number / double | oui | Copie en lecture seule de `sample.denominator`. |
| `MetricValue` | `minimumSample` | number / double | non | Copie en lecture seule de `sample.minimumSample`. |
| `MetricValue` | `sampleReliable` | boolean | non | Copie en lecture seule de `quality.sampleReliable`. |
| `MetricValue` | `qualityScore` | number / double | non | Copie en lecture seule de `quality.qualityScore`. |
| `MetricSample` | `numerator` | number / double | oui | Numérateur fourni par le serveur. Les métriques actuelles le renseignent, y compris à `0`. |
| `MetricSample` | `denominator` | number / double | oui | Dénominateur fourni par le serveur. Les métriques actuelles le renseignent, y compris à `0`. |
| `MetricSample` | `minimumSample` | number / double | non | Seuil de fiabilité. |
| `MetricQuality` | `sampleReliable` | boolean | non | `true` si `value` existe et si `denominator >= minimumSample`. |
| `MetricQuality` | `qualityScore` | number / double | non | `clamp(denominator / minimumSample, 0, 1)`, arrondi à 2 décimales ; `0` si échantillon invalide. |
| `MetricQuality` | `reason` | string | oui | `ZERO_OR_INVALID_DENOMINATOR`, `INVALID_NUMERATOR`, `BELOW_MINIMUM_SAMPLE` ou `null`. |

Un taux sous le seuil conserve sa `value`, son numérateur et son dénominateur, mais porte `sampleReliable = false`. Un taux de dénominateur `0` porte `value = null`, `qualityScore = 0` et `reason = "ZERO_OR_INVALID_DENOMINATOR"`.

## Conventions des tableaux de métriques

- `count` = nombre d’événements ; les compteurs v2 sont des entiers non nullables et valent réellement `0` lorsqu’aucun événement correspondant n’existe dans le périmètre.
- `N/A` = `MetricSample` et `MetricQuality` ne s’appliquent pas à un compteur.
- `EXACT` en v1 = une propriété v1 de même sémantique existe.
- `DERIVABLE` en v1 = les atomes exacts existent mais la propriété finale doit être recomposée selon la formule indiquée.
- `PARTIAL` en v1 = la valeur peut être calculée, mais v1 ne fournit ni `MetricVersion`, ni `MetricSample`, ni `MetricQuality`.
- `DATA_MISSING` = aucune valeur canonique ne peut être produite.
- Pour toute valeur nullable, l’affichage HandWStat est `—` avec le libellé accessible « Non disponible », jamais le nombre `0`.
- Pour les compteurs, `metricVersion` désigne `response.metricVersion = "1.0"`. Pour un `MetricValue` ou un `LeagueCountMetric`, la version est également répétée dans l’objet.

## ATTAQUE — `LeagueAttackMetrics` (`offense`)

| Métrique / propriété JSON exacte | Type · nullable · unité | Formule ; numérateur ; dénominateur | MetricVersion · MetricSample · MetricQuality | Null affiché | Disponibilité v1 | Disponibilité v2 | Fallback autorisé | Fallback interdit |
|---|---|---|---|---|---|---|---|---|
| TotalGoals / `totalGoals` | integer · non · count | `OpenPlayGoals + PenaltyGoals` ; N=`TotalGoals` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerGlobalStatsDto.totalGoals` | AVAILABLE | Utiliser uniquement le total v1 exact. | Utiliser seulement les buts en jeu ouvert. |
| OpenPlayGoals / `openPlayGoals` | integer · non · count | Nombre de buts hors jet de 7 m ; N=`OpenPlayGoals` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerGlobalStatsDto.goalCount` ou `PlayerOffenseStatsDto.buts` | AVAILABLE | Champ v1 exact. | Soustraire une source de pénalty non alignée. |
| PenaltyGoals / `penaltyGoals` | integer · non · count | Nombre de buts sur jet de 7 m ; N=`PenaltyGoals` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerGlobalStatsDto.penaltyGoalCount` ou `PlayerOffenseStatsDto.buts7m` | AVAILABLE | Champ v1 exact. | Assimiler tentatives ou pénaltys obtenus à des buts. |
| Assists / `assists` | integer · non · count | Nombre de passes décisives ; N=`Assists` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerGlobalStatsDto.assistCount` ou `PlayerPassingStatsDto.passeDecisive` | AVAILABLE | Champ v1 exact. | Déduire les passes décisives des buts. |
| PenaltiesWon / `penaltiesWon` | integer · non · count | Nombre de jets de 7 m obtenus ; N=`PenaltiesWon` ; D=N/A | `1.0` · N/A · N/A | N/A | DATA_MISSING dans les DTO joueur v1 | AVAILABLE | Aucun fallback numérique ; afficher `—` si v2 indisponible. | Utiliser `penaltyGoals`, `penaltyAttempts` ou `penaltiesConceded`. |
| SanctionsDrawn / `sanctionsDrawn` | integer · non · count | Nombre de sanctions provoquées ; N=`SanctionsDrawn` ; D=N/A | `1.0` · N/A · N/A | N/A | DATA_MISSING dans les DTO joueur v1 | AVAILABLE | Aucun fallback numérique ; afficher `—` si v2 indisponible. | Utiliser les sanctions concédées ou les pénaltys obtenus. |
| TotalTurnovers / `totalTurnovers` | integer · non · count | `BadPasses + BallLosses + TechnicalFaults + OffensiveFoulsCommitted` ; N=`TotalTurnovers` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerGlobalStatsDto.turnoverCount` ou `PlayerPassingStatsDto.totalPertes` | AVAILABLE | Champ total v1 exact. | Utiliser `badPasses` ou `perteDeBalle` seul. |
| BadPasses / `badPasses` | integer · non · count | Nombre de mauvaises passes ; N=`BadPasses` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerPassingStatsDto.mauvaisePasse` | AVAILABLE | Champ v1 exact. | Le réutiliser comme FailedPivotPasses. |
| FailedPivotPasses / `failedPivotPasses` | `LeagueCountMetric` · objet non ; `value` oui · count | Pas de formule canonique ; N=indisponible ; D=N/A | objet `1.0` · N/A · N/A | `—` | DATA_MISSING / source historique ambiguë | DATA_MISSING ; JSON `availability = "DATA_MISSING"`, `value = null` | Aucun fallback numérique. Conserver l’objet et sa raison. | Toute substitution, notamment `badPasses`, `mauvaisePasse` ou un champ historique non typé. |
| TotalShotRate / `totalShotRate` | `MetricValue` non nul ; `value` oui · percent | `100 × TotalGoals / TotalAttempts` ; N=`TotalGoals` ; D=`OpenPlayAttempts + PenaltyAttempts` | objet `1.0` · minimum `4` · qualité serveur | `—` | PARTIAL : N=`totalGoals`, D=`shotAttempts` disponibles | AVAILABLE ; `value` nullable si D=0 | Recalcul v1 uniquement avec ces deux atomes exacts et provenance v1. | Faire confiance à un `0` v1 sans son dénominateur ; omettre les tentatives à 7 m. |
| OpenPlayShotRate / `openPlayShotRate` | `MetricValue` non nul ; `value` oui · percent | `100 × OpenPlayGoals / OpenPlayAttempts` ; N=`OpenPlayGoals` ; D=`OpenPlayGoals + OpenPlayMisses` | objet `1.0` · minimum `4` · qualité serveur | `—` | PARTIAL : N=`goalCount`, D=`openShotAttempts` disponibles | AVAILABLE ; `value` nullable si D=0 | Recalcul v1 avec ces atomes exacts. | Inclure les tentatives à 7 m ou transformer D=0 en 0 %. |
| PenaltyShotRate / `penaltyShotRate` | `MetricValue` non nul ; `value` oui · percent | `100 × PenaltyGoals / PenaltyAttempts` ; N=`PenaltyGoals` ; D=`PenaltyGoals + PenaltyMisses` | objet `1.0` · minimum `2` · qualité serveur | `—` | PARTIAL : N=`penaltyGoalCount`, D=`penaltyAttempts` disponibles | AVAILABLE ; `value` nullable si D=0 | Recalcul v1 avec ces atomes exacts. | Utiliser le taux global ou transformer D=0 en 0 %. |

## DÉFENSE — `LeagueDefenseMetrics` (`defense`)

| Métrique / propriété JSON exacte | Type · nullable · unité | Formule ; numérateur ; dénominateur | MetricVersion · MetricSample · MetricQuality | Null affiché | Disponibilité v1 | Disponibilité v2 | Fallback autorisé | Fallback interdit |
|---|---|---|---|---|---|---|---|---|
| Interceptions / `interceptions` | integer · non · count | Nombre d’interceptions ; N=`Interceptions` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerDefenseStatsDto.interceptions` | AVAILABLE | Champ v1 exact. | Déduire depuis un impact défensif agrégé. |
| Blocks / `blocks` | integer · non · count | Nombre de contres réussis ; N=`CounterSuccesses` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerDefenseStatsDto.contres` | AVAILABLE | Champ v1 exact. | Utiliser un tir manqué ou `tirContre` offensif. |
| OffensiveFoulsDrawn / `offensiveFoulsDrawn` | integer · non · count | Nombre de passages en force provoqués ; N=`ForcedPassages` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerDefenseStatsDto.passageForce` | AVAILABLE | Champ v1 exact. | Utiliser `PlayerPassingStatsDto.passageEnForce`, qui représente une perte commise. |
| Neutralizations / `neutralizations` | integer · non · count | Nombre de neutralisations ; N=`Neutralizations` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerDefenseStatsDto.neutralisations` | AVAILABLE | Champ v1 exact. | Fusionner avec interceptions ou contres. |
| PenaltiesConceded / `penaltiesConceded` | integer · non · count | Nombre de jets de 7 m concédés ; N=`PenaltiesConceded` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerSanctionStatsDto.penaltyConcede` | AVAILABLE | Champ v1 exact. | L’inclure dans `sanctionsConceded`. |
| SanctionsConceded / `sanctionsConceded` | integer · non · count | `Warnings + TwoMinuteSuspensions + Disqualifications` ; N=`DisciplinarySanctions` ; D=N/A | `1.0` · N/A · N/A | N/A | DERIVABLE : `avertissements + deuxMinutes + exclusions` | AVAILABLE | Sommer exactement ces trois champs v1. | Utiliser `PlayerGlobalStatsDto.sanctionCount`, qui inclut aussi les pénaltys concédés. |
| WarningsConceded / `warningsConceded` | integer · non · count | Nombre d’avertissements ; N=`Warnings` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerSanctionStatsDto.avertissements` | AVAILABLE | Champ v1 exact. | Déduire du total des sanctions. |
| TwoMinuteSuspensionsConceded / `twoMinuteSuspensionsConceded` | integer · non · count | Nombre de suspensions de deux minutes ; N=`TwoMinutes` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerSanctionStatsDto.deuxMinutes` | AVAILABLE | Champ v1 exact. | Déduire du total des sanctions. |
| DisqualificationsConceded / `disqualificationsConceded` | integer · non · count | Nombre d’exclusions/disqualifications classifiées ; N=`Exclusions` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerSanctionStatsDto.exclusions` | AVAILABLE | Champ v1 exact. | Déduire du total ou des suspensions de deux minutes. |

## GARDIENNE — `LeagueGoalkeeperMetrics` (`goalkeeper`)

Le contrat est identique quel que soit le genre du joueur. Les tirs subis sont exclusivement les tirs cadrés ayant produit un arrêt ou un but encaissé.

| Métrique / propriété JSON exacte | Type · nullable · unité | Formule ; numérateur ; dénominateur | MetricVersion · MetricSample · MetricQuality | Null affiché | Disponibilité v1 | Disponibilité v2 | Fallback autorisé | Fallback interdit |
|---|---|---|---|---|---|---|---|---|
| TotalSaves / `totalSaves` | integer · non · count | `OpenPlaySaves + PenaltySaves` ; N=`TotalSaves` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerGlobalStatsDto.saveCount` ou `arrets + arretsPenalty` | AVAILABLE | Total v1 exact ou somme exacte. | Omettre les arrêts sur jet de 7 m. |
| OpenPlaySaves / `openPlaySaves` | integer · non · count | Arrêts hors jet de 7 m ; N=`OpenPlaySaves` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerGoalkeeperStatsDto.arrets` | AVAILABLE | Champ v1 exact. | Utiliser tous les arrêts. |
| PenaltySaves / `penaltySaves` | integer · non · count | Arrêts sur jet de 7 m ; N=`PenaltySaves` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerGoalkeeperStatsDto.arretsPenalty` | AVAILABLE | Champ v1 exact. | Utiliser pénaltys concédés ou buts encaissés. |
| TotalShotsFaced / `totalShotsFaced` | integer · non · count | `TotalSaves + OpenPlayGoalsConceded + PenaltyGoalsConceded` ; N=`TotalShotsFaced` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerGlobalStatsDto.shotsFaced` ou `PlayerGoalkeeperStatsDto.tirsSubis` | AVAILABLE | Champ v1 exact. | Ajouter les tirs hors cadre, poteaux, tirs contrés ou échecs sans arrêt gardienne. |
| OpenPlayShotsFaced / `openPlayShotsFaced` | integer · non · count | `OpenPlaySaves + OpenPlayGoalsConceded` ; N=`OpenPlayShotsFaced` ; D=N/A | `1.0` · N/A · N/A | N/A | DERIVABLE : `arrets + butsPris` | AVAILABLE | Somme exacte de ces deux champs v1. | Ajouter les tirs hors cadre ou les tentatives à 7 m. |
| PenaltyShotsFaced / `penaltyShotsFaced` | integer · non · count | `PenaltySaves + PenaltyGoalsConceded` ; N=`PenaltyShotsFaced` ; D=N/A | `1.0` · N/A · N/A | N/A | DERIVABLE : `arretsPenalty + butsPenalty` | AVAILABLE | Somme exacte de ces deux champs v1. | Utiliser tous les pénaltys concédés par la défense. |
| TotalSaveRate / `totalSaveRate` | `MetricValue` non nul ; `value` oui · percent | `100 × TotalSaves / TotalShotsFaced` ; N=`TotalSaves` ; D=`TotalShotsFaced` | objet `1.0` · minimum `10` · qualité serveur | `—` | PARTIAL : N=`saveCount`, D=`shotsFaced` disponibles | AVAILABLE ; `value` nullable si D=0 | Recalcul v1 avec ces deux atomes exacts. | Utiliser un taux v1 isolé sans échantillon ou transformer D=0 en 0 %. |
| OpenPlaySaveRate / `openPlaySaveRate` | `MetricValue` non nul ; `value` oui · percent | `100 × OpenPlaySaves / OpenPlayShotsFaced` ; N=`OpenPlaySaves` ; D=`OpenPlayShotsFaced` | objet `1.0` · minimum `10` · qualité serveur | `—` | PARTIAL : N=`arrets`, D=`arrets + butsPris` dérivable | AVAILABLE ; `value` nullable si D=0 | Recalcul v1 avec ces atomes exacts. | Inclure les jets de 7 m ou les tirs hors cadre. |
| PenaltySaveRate / `penaltySaveRate` | `MetricValue` non nul ; `value` oui · percent | `100 × PenaltySaves / PenaltyShotsFaced` ; N=`PenaltySaves` ; D=`PenaltyShotsFaced` | objet `1.0` · minimum `2` · qualité serveur | `—` | PARTIAL : N=`arretsPenalty`, D=`arretsPenalty + butsPenalty` dérivable | AVAILABLE ; `value` nullable si D=0 | Recalcul v1 avec ces atomes exacts. | Utiliser le taux global ou les pénaltys défensifs concédés. |
| Assists / `assists` | integer · non · count | Passes décisives personnelles ; N=`Assists` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerGlobalStatsDto.assistCount` ou `PlayerGoalkeeperStatsDto.passeDecisives` | AVAILABLE | Champ v1 exact. | Déduire depuis les relances ou les buts. |
| Goals / `goals` | integer · non · count | `OpenPlayGoals + PenaltyGoals` personnels ; N=`TotalGoals` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT via `PlayerGlobalStatsDto.totalGoals` ; le seul `PlayerGoalkeeperStatsDto.buts` est incomplet | AVAILABLE | Utiliser `global.totalGoals`. | Utiliser `goalkeeper.buts` seul, car il exclut les buts sur jet de 7 m. |
| TotalTurnovers / `totalTurnovers` | integer · non · count | `BadPasses + BallLosses + TechnicalFaults + OffensiveFoulsCommitted` ; N=`TotalTurnovers` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT via `PlayerGlobalStatsDto.turnoverCount` ou `PlayerPassingStatsDto.totalPertes` | AVAILABLE | Utiliser le total v1 exact. | Utiliser `PlayerGoalkeeperStatsDto.perteDeBalle` ou `mauvaisePasse` seul. |
| MissedShots / `missedShots` | integer · non · count | `OpenPlayMisses + PenaltyMisses` personnels ; N=`ShotWaste` ; D=N/A | `1.0` · N/A · N/A | N/A | EXACT : `PlayerGoalkeeperStatsDto.tirsLoupes` ou `PlayerTechnicalStatsDto.shotWaste` | AVAILABLE | Champ v1 exact. | Confondre tirs personnels manqués et tirs subis par la gardienne. |

## FailedPivotPasses — règle impérative

Le statut d’intégration HandWStat est porté par la valeur `DATA_MISSING` du champ JSON `availability`. Il ne faut pas ajouter une propriété JSON `status`. Le modèle `MatchEvent` ne possède ni sous-type canonique de passe ni destinataire typé permettant d’établir une passe au pivot échouée.

```text
Status = DATA_MISSING
Value = null
FallbackFromBadPasses = FORBIDDEN
```

Il est interdit d’exploiter `badPasses`, `PlayerPassingStatsDto.mauvaisePasse`, un nom de zone, un texte libre ou une ancienne colonne ambiguë pour produire cette métrique. Sa future disponibilité exige une donnée source explicite conforme à `FAILED_PIVOT_PASS_DATA_REQUIREMENT.md`.

## Exemple JSON complet

Cet exemple contient toutes les propriétés des DTO v2 associés. Les valeurs sont illustratives mais respectent les invariants et les formules du service.

```json
{
  "playerId": 42,
  "metricVersion": "1.0",
  "included": [
    "defense",
    "goalkeeper",
    "offense",
    "overview"
  ],
  "overview": {
    "playerId": 42,
    "fullName": "Camille Exemple",
    "teamId": 7,
    "teamName": "Handball Club Exemple",
    "positionId": 1,
    "positionCode": "GB",
    "positionName": "Gardienne",
    "isGoalkeeper": true,
    "matchesPlayed": 8
  },
  "offense": {
    "totalGoals": 6,
    "openPlayGoals": 5,
    "penaltyGoals": 1,
    "assists": 3,
    "penaltiesWon": 2,
    "sanctionsDrawn": 1,
    "totalTurnovers": 4,
    "badPasses": 2,
    "failedPivotPasses": {
      "metricCode": "FAILED_PIVOT_PASSES",
      "metricVersion": "1.0",
      "value": null,
      "availability": "DATA_MISSING",
      "reason": "MatchEvent requires an explicit FAILED_PIVOT_PASS subtype and a typed pivot target (TargetPlayerId, TargetPositionId or PassTargetCode)."
    },
    "totalShotRate": {
      "metricCode": "TOTAL_SHOT_RATE",
      "metricVersion": "1.0",
      "value": 60,
      "unit": "percent",
      "sample": {
        "numerator": 6,
        "denominator": 10,
        "minimumSample": 4
      },
      "quality": {
        "sampleReliable": true,
        "qualityScore": 1,
        "reason": null
      },
      "numerator": 6,
      "denominator": 10,
      "minimumSample": 4,
      "sampleReliable": true,
      "qualityScore": 1
    },
    "openPlayShotRate": {
      "metricCode": "OPEN_PLAY_SHOT_RATE",
      "metricVersion": "1.0",
      "value": 62.5,
      "unit": "percent",
      "sample": {
        "numerator": 5,
        "denominator": 8,
        "minimumSample": 4
      },
      "quality": {
        "sampleReliable": true,
        "qualityScore": 1,
        "reason": null
      },
      "numerator": 5,
      "denominator": 8,
      "minimumSample": 4,
      "sampleReliable": true,
      "qualityScore": 1
    },
    "penaltyShotRate": {
      "metricCode": "PENALTY_SHOT_RATE",
      "metricVersion": "1.0",
      "value": 50,
      "unit": "percent",
      "sample": {
        "numerator": 1,
        "denominator": 2,
        "minimumSample": 2
      },
      "quality": {
        "sampleReliable": true,
        "qualityScore": 1,
        "reason": null
      },
      "numerator": 1,
      "denominator": 2,
      "minimumSample": 2,
      "sampleReliable": true,
      "qualityScore": 1
    }
  },
  "defense": {
    "interceptions": 4,
    "blocks": 2,
    "offensiveFoulsDrawn": 1,
    "neutralizations": 3,
    "penaltiesConceded": 2,
    "sanctionsConceded": 3,
    "warningsConceded": 1,
    "twoMinuteSuspensionsConceded": 1,
    "disqualificationsConceded": 1
  },
  "goalkeeper": {
    "totalSaves": 12,
    "openPlaySaves": 10,
    "penaltySaves": 2,
    "totalShotsFaced": 24,
    "openPlayShotsFaced": 20,
    "penaltyShotsFaced": 4,
    "totalSaveRate": {
      "metricCode": "TOTAL_SAVE_RATE",
      "metricVersion": "1.0",
      "value": 50,
      "unit": "percent",
      "sample": {
        "numerator": 12,
        "denominator": 24,
        "minimumSample": 10
      },
      "quality": {
        "sampleReliable": true,
        "qualityScore": 1,
        "reason": null
      },
      "numerator": 12,
      "denominator": 24,
      "minimumSample": 10,
      "sampleReliable": true,
      "qualityScore": 1
    },
    "openPlaySaveRate": {
      "metricCode": "OPEN_PLAY_SAVE_RATE",
      "metricVersion": "1.0",
      "value": 50,
      "unit": "percent",
      "sample": {
        "numerator": 10,
        "denominator": 20,
        "minimumSample": 10
      },
      "quality": {
        "sampleReliable": true,
        "qualityScore": 1,
        "reason": null
      },
      "numerator": 10,
      "denominator": 20,
      "minimumSample": 10,
      "sampleReliable": true,
      "qualityScore": 1
    },
    "penaltySaveRate": {
      "metricCode": "PENALTY_SAVE_RATE",
      "metricVersion": "1.0",
      "value": 50,
      "unit": "percent",
      "sample": {
        "numerator": 2,
        "denominator": 4,
        "minimumSample": 2
      },
      "quality": {
        "sampleReliable": true,
        "qualityScore": 1,
        "reason": null
      },
      "numerator": 2,
      "denominator": 4,
      "minimumSample": 2,
      "sampleReliable": true,
      "qualityScore": 1
    },
    "assists": 3,
    "goals": 6,
    "totalTurnovers": 4,
    "missedShots": 4
  }
}
```

## Stratégie de fallback

### Autorisée

1. Utiliser v2 comme source prioritaire et conserver sans altération `metricVersion`, `sample`, `quality`, ainsi que les propriétés aplaties de `MetricValue`.
2. Si v2 est techniquement indisponible, utiliser une propriété v1 marquée `EXACT` ou recomposer uniquement les atomes explicitement marqués `DERIVABLE`/`PARTIAL` dans les tableaux ci-dessus.
3. Marquer visiblement la provenance `API v1` ou `API v2`. Un résultat v1 recomposé ne doit pas être présenté comme un `MetricValue` v2 et ne reçoit pas une qualité inventée.
4. Lorsqu’une métrique n’a pas de source canonique, afficher `—` / « Non disponible ».

### Interdite

1. Remplacer une valeur `null` par `0`.
2. Construire un numérateur, un dénominateur, un `MetricVersion` ou un `MetricQuality` absent de la réponse source.
3. Substituer une métrique voisine sous prétexte qu’elle est disponible.
4. Mélanger des valeurs v1 et v2 dans un même calcul sans exposer cette composition et sans identité stricte de périmètre.
5. Utiliser `BadPasses` comme approximation de `FailedPivotPasses`.
6. Ajouter un tir hors cadre, sur poteau, contré ou autrement non cadré aux tirs subis par la gardienne.

## CLIENT IMPLEMENTATION RULES

- Ne jamais transformer `null` en `0`.
- Ne jamais inventer un numérateur ou un dénominateur.
- Ne jamais utiliser `BadPasses` pour `FailedPivotPasses`.
- Ne jamais compter un tir hors cadre dans les tirs subis par la gardienne.
- Afficher la provenance `API v1` ou `API v2`.
- Conserver `MetricVersion` et `MetricQuality`.

## Validation du contrat

Le présent contrat a été confronté aux sources suivantes :

- `HandballManagerAPI/Analytics/LeagueMetricContracts.cs` : DTO v2, nullabilité et enum de disponibilité ;
- `HandballManagerAPI/Analytics/MetricContracts.cs` : `MetricValue`, `MetricSample`, `MetricQuality`, arrondis et qualité ;
- `HandballManagerAPI/Controllers/LeagueAnalyticsController.cs` : route, paramètres, validation, autorisation et réponses ;
- `HandballManagerAPI/Analytics/LeagueAnalyticsService.cs` : formules, seuils, tri de `included` et état FailedPivotPasses ;
- DTO v1 `PlayerGlobalStatsDto`, `PlayerOffenseStatsDto`, `PlayerDefenseStatsDto`, `PlayerPassingStatsDto`, `PlayerSanctionStatsDto`, `PlayerGoalkeeperStatsDto` et `PlayerTechnicalStatsDto` ;
- `HandballManagerAPI.Tests/LeagueAnalyticsTests.cs` : taxonomie des tirs, valeurs exactes, échantillons nuls et FailedPivotPasses ;
- `HandballManagerAPI.Tests/StatsEndpointsTests.cs` : sélection des sections, nullabilité, erreurs et contrat OpenAPI ;
- Swagger généré : endpoint, 13 paramètres query plus le paramètre path, schémas typés et réponses `200/400/401/403/404/409/500` ;
- `docs/statistics/LEAGUE_METRIC_SOURCE_MAPPING.md` et `docs/statistics/FAILED_PIVOT_PASS_DATA_REQUIREMENT.md`.

Validation exécutée :

- les 68 tests existants passent, dont les tests de fixture League, de taux sans échantillon, de JSON v2, d’erreurs et d’OpenAPI ;
- le Swagger a été régénéré et contrôlé pour la route, les paramètres, les schémas, `DATA_MISSING`, les objets `sample`/`quality` et les seuils `4`, `2` et `10` ;
- l’exemple JSON complet a été parsé puis comparé aux listes exactes de propriétés des DTO ; ses six taux, ses propriétés imbriquées/aplaties, ses seuils et les invariants attaque/gardienne ont été recalculés avec succès.

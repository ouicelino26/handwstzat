# Mapping UI des statistiques officielles Ligue

Date : 2026-07-29  
Endpoint : `GET /api/v2/analytics/players/{playerId}`  
Version métrique v2 : `1.0`

## Règles et architecture

La source de vérité est `docs/integration/HANDWSTAT_LEAGUE_ANALYTICS_CONTRACT.md`. Les noms ci-dessous sont les propriétés JSON `camelCase` réellement sérialisées. Les DTO sont locaux à HandWStat : `LeaguePlayerAnalyticsResponseDto`, `LeaguePlayerOverviewDto`, `LeagueAttackMetricsDto`, `LeagueDefenseMetricsDto`, `LeagueGoalkeeperMetricsDto`, `LeagueCountMetricDto`, `LeagueMetricValueDto`, `LeagueMetricSampleDto` et `LeagueMetricQualityDto`.

`V2AnalyticsGateway` appelle `api/v2/analytics/players/{playerId}` avec `include=overview,offense,defense,goalkeeper` et transmet seulement les filtres supportés : `competitionId`, `teamId`, `matchId`, `from`, `to`, `year`, `season`, `day`, `attackId`, `defenseId`, `trigger`, `shootShade`. `LeaguePlayerAnalyticsService` applique la stratégie v2/fallback et `LeaguePlayerAnalyticsMapper` construit la présentation sans formule dans Razor.

Légende :

- `MV` = `MetricValueCard`; `RM` = `RateMetricCard` + `MetricEvidence`; `MB` = `MetricBreakdownCard`; `UM` = `UnavailableMetricState`.
- Pour un compteur, la version est `response.metricVersion`; `MetricSample` et `MetricQuality` ne s'appliquent pas.
- Pour un taux, l'objet JSON est non nullable mais `value` est nullable ; `metricVersion`, `sample`, `quality` et les propriétés aplaties sont obligatoires.
- `V1_COMPATIBLE` est une propriété v1 de même sémantique. `V1_PARTIAL` est une recomposition depuis des atomes exacts, sans version ni qualité serveur.
- Tout JSON v2 reçu mais invalide produit `CONTRACT_ERROR` et interdit le fallback.

## Attaque

| Métier / propriété JSON exacte | Type JSON · nullable · unité | DTO client | Formule · numérateur / dénominateur | MetricVersion · MetricSample · MetricQuality | Fallback v1 autorisé | Fallback v1 interdit | UI | Test requis · statut |
|---|---|---|---|---|---|---|---|---|
| Buts total / `offense.totalGoals` | integer · non · count | `LeagueAttackMetricsDto.TotalGoals` | jeu + 7 m · total / N/A | réponse 1.0 · N/A · N/A | `global.totalGoals` → `V1_COMPATIBLE` | buts jeu seuls | MV | exact · `V2_COMPLETE` |
| Buts dans le jeu / `offense.openPlayGoals` | integer · non · count | `LeagueAttackMetricsDto.OpenPlayGoals` | buts hors 7 m · valeur / N/A | réponse 1.0 · N/A · N/A | `global.goalCount` ou `offense.buts` → `V1_COMPATIBLE` | soustraction ambiguë | MV | exact · `V2_COMPLETE` |
| Buts sur 7 m / `offense.penaltyGoals` | integer · non · count | `LeagueAttackMetricsDto.PenaltyGoals` | buts 7 m · valeur / N/A | réponse 1.0 · N/A · N/A | `global.penaltyGoalCount` ou `offense.buts7m` → `V1_COMPATIBLE` | tentatives/7 m obtenus | MV | exact · `V2_COMPLETE` |
| Passes décisives / `offense.assists` | integer · non · count | `LeagueAttackMetricsDto.Assists` | compte · valeur / N/A | réponse 1.0 · N/A · N/A | `global.assistCount` ou `passing.passeDecisive` → `V1_COMPATIBLE` | déduction depuis buts | MV | exact · `V2_COMPLETE` |
| 7 m obtenus / `offense.penaltiesWon` | integer · non · count | `LeagueAttackMetricsDto.PenaltiesWon` | compte · valeur / N/A | réponse 1.0 · N/A · N/A | aucun → `UNAVAILABLE` | buts/tentatives/7 m concédés | MV/UM | v2 + fallback absent · `V2_COMPLETE` |
| Sanctions obtenues / `offense.sanctionsDrawn` | integer · non · count | `LeagueAttackMetricsDto.SanctionsDrawn` | compte · valeur / N/A | réponse 1.0 · N/A · N/A | aucun → `UNAVAILABLE` | sanctions concédées | MV/UM | v2 + fallback absent · `V2_COMPLETE` |
| Pertes de balle / `offense.totalTurnovers` | integer · non · count | `LeagueAttackMetricsDto.TotalTurnovers` | mauvaises passes + pertes simples + fautes techniques + passages en force · total / N/A | réponse 1.0 · N/A · N/A | `global.turnoverCount` ou `passing.totalPertes` → `V1_COMPATIBLE` | une sous-catégorie seule | MB | total sans double compte · `V2_COMPLETE` |
| Mauvaises passes / `offense.badPasses` | integer · non · count | `LeagueAttackMetricsDto.BadPasses` | compte · valeur / N/A | réponse 1.0 · N/A · N/A | `passing.mauvaisePasse` → `V1_COMPATIBLE` | passe pivot | MB détail | exact · `V2_COMPLETE` |
| Passes pivot ratées / `offense.failedPivotPasses` | object non ; `value` integer oui · count | `LeagueCountMetricDto` | aucune formule canonique · indisponible / N/A | objet 1.0 · N/A · N/A | aucun | `badPasses`, `mauvaisePasse`, `totalTurnovers`, texte libre | UM toujours visible | null + raison + aucun remplacement · `DATA_MISSING` |
| Taux de tir total / `offense.totalShotRate` | object non ; `value` number oui · percent | `LeagueMetricValueDto` | 100 × buts / tentatives · `totalGoals` / total tentatives | objet 1.0 · min 4 · qualité serveur | `global.totalGoals/global.shotAttempts` → `V1_PARTIAL` | taux isolé, D incertain, D=0→0 % | RM | complet/null/fiable/non fiable · `V2_COMPLETE` |
| Taux de tir dans le jeu / `offense.openPlayShotRate` | object non ; `value` number oui · percent | `LeagueMetricValueDto` | 100 × buts jeu / tirs jeu · `openPlayGoals` / `openPlayAttempts` | objet 1.0 · min 4 · qualité serveur | `global.goalCount/global.openShotAttempts` → `V1_PARTIAL` | inclure 7 m | RM | zéro tentative · `V2_COMPLETE` |
| Taux de tir sur 7 m / `offense.penaltyShotRate` | object non ; `value` number oui · percent | `LeagueMetricValueDto` | 100 × buts 7 m / tirs 7 m · `penaltyGoals` / `penaltyAttempts` | objet 1.0 · min 2 · qualité serveur | `global.penaltyGoalCount/global.penaltyAttempts` → `V1_PARTIAL` | taux global | RM | zéro tentative · `V2_COMPLETE` |

## Défense

| Métier / propriété JSON exacte | Type JSON · nullable · unité | DTO client | Formule · numérateur / dénominateur | MetricVersion · MetricSample · MetricQuality | Fallback v1 autorisé | Fallback v1 interdit | UI | Test requis · statut |
|---|---|---|---|---|---|---|---|---|
| Interceptions / `defense.interceptions` | integer · non · count | `LeagueDefenseMetricsDto.Interceptions` | compte · valeur / N/A | réponse 1.0 · N/A · N/A | `defense.interceptions` → `V1_COMPATIBLE` | impact agrégé | MV | exact · `V2_COMPLETE` |
| Contres / `defense.blocks` | integer · non · count | `LeagueDefenseMetricsDto.Blocks` | contres réussis · valeur / N/A | réponse 1.0 · N/A · N/A | `defense.contres` → `V1_COMPATIBLE` | tir contré offensif | MV | exact · `V2_COMPLETE` |
| Passages en force provoqués / `defense.offensiveFoulsDrawn` | integer · non · count | `LeagueDefenseMetricsDto.OffensiveFoulsDrawn` | compte · valeur / N/A | réponse 1.0 · N/A · N/A | `defense.passageForce` → `V1_COMPATIBLE` | `passing.passageEnForce` commis | MV | sens exact · `V2_COMPLETE` |
| Neutralisations / `defense.neutralizations` | integer · non · count | `LeagueDefenseMetricsDto.Neutralizations` | compte · valeur / N/A | réponse 1.0 · N/A · N/A | `defense.neutralisations` → `V1_COMPATIBLE` | fusion autres actions | MV | exact · `V2_COMPLETE` |
| 7 m concédés / `defense.penaltiesConceded` | integer · non · count | `LeagueDefenseMetricsDto.PenaltiesConceded` | compte · valeur / N/A | réponse 1.0 · N/A · N/A | `sanctions.penaltyConcede` → `V1_COMPATIBLE` | ajout aux sanctions disciplinaires | MV | exact · `V2_COMPLETE` |
| Sanctions concédées / `defense.sanctionsConceded` | integer · non · count | `LeagueDefenseMetricsDto.SanctionsConceded` | avertissements + 2 min + disqualifications · total / N/A | réponse 1.0 · N/A · N/A | somme des trois champs v1 → `V1_PARTIAL` | `global.sanctionCount` incluant 7 m | MB | somme exacte sans double compte · `V2_COMPLETE` |
| Avertissements / `defense.warningsConceded` | integer · non · count | `LeagueDefenseMetricsDto.WarningsConceded` | compte · valeur / N/A | réponse 1.0 · N/A · N/A | `sanctions.avertissements` → `V1_COMPATIBLE` | déduction du total | MB détail | exact · `V2_COMPLETE` |
| Exclusions deux minutes / `defense.twoMinuteSuspensionsConceded` | integer · non · count | `LeagueDefenseMetricsDto.TwoMinuteSuspensionsConceded` | compte · valeur / N/A | réponse 1.0 · N/A · N/A | `sanctions.deuxMinutes` → `V1_COMPATIBLE` | déduction du total | MB détail | exact · `V2_COMPLETE` |
| Disqualifications / `defense.disqualificationsConceded` | integer · non · count | `LeagueDefenseMetricsDto.DisqualificationsConceded` | compte · valeur / N/A | réponse 1.0 · N/A · N/A | `sanctions.exclusions` → `V1_COMPATIBLE` | déduction du total/2 min | MB détail | exact · `V2_COMPLETE` |

## Gardienne

| Métier / propriété JSON exacte | Type JSON · nullable · unité | DTO client | Formule · numérateur / dénominateur | MetricVersion · MetricSample · MetricQuality | Fallback v1 autorisé | Fallback v1 interdit | UI | Test requis · statut |
|---|---|---|---|---|---|---|---|---|
| Arrêts total / `goalkeeper.totalSaves` | integer · non · count | `LeagueGoalkeeperMetricsDto.TotalSaves` | jeu + 7 m · total / N/A | réponse 1.0 · N/A · N/A | `global.saveCount` ou somme exacte → `V1_COMPATIBLE` | jeu seul | MV | exact · `V2_COMPLETE` |
| Arrêts dans le jeu / `goalkeeper.openPlaySaves` | integer · non · count | `LeagueGoalkeeperMetricsDto.OpenPlaySaves` | compte hors 7 m · valeur / N/A | réponse 1.0 · N/A · N/A | `goalkeeper.arrets` → `V1_COMPATIBLE` | tous arrêts | MV | exact · `V2_COMPLETE` |
| Arrêts sur 7 m / `goalkeeper.penaltySaves` | integer · non · count | `LeagueGoalkeeperMetricsDto.PenaltySaves` | compte 7 m · valeur / N/A | réponse 1.0 · N/A · N/A | `goalkeeper.arretsPenalty` → `V1_COMPATIBLE` | buts/7 m concédés | MV | exact · `V2_COMPLETE` |
| Tirs subis total / `goalkeeper.totalShotsFaced` | integer · non · count | `LeagueGoalkeeperMetricsDto.TotalShotsFaced` | arrêts + buts encaissés · total / N/A | réponse 1.0 · N/A · N/A | `global.shotsFaced` ou `goalkeeper.tirsSubis` → `V1_COMPATIBLE` | hors cadre, poteaux, contrés | MV | exact + hors cadre exclus · `V2_COMPLETE` |
| Tirs subis dans le jeu / `goalkeeper.openPlayShotsFaced` | integer · non · count | `LeagueGoalkeeperMetricsDto.OpenPlayShotsFaced` | arrêts jeu + buts jeu encaissés · total / N/A | réponse 1.0 · N/A · N/A | `arrets + butsPris` → `V1_PARTIAL` | 7 m/hors cadre | MV | dérivation exacte · `V2_COMPLETE` |
| Tirs subis sur 7 m / `goalkeeper.penaltyShotsFaced` | integer · non · count | `LeagueGoalkeeperMetricsDto.PenaltyShotsFaced` | arrêts 7 m + buts 7 m encaissés · total / N/A | réponse 1.0 · N/A · N/A | `arretsPenalty + butsPenalty` → `V1_PARTIAL` | 7 m défensifs concédés | MV | dérivation exacte · `V2_COMPLETE` |
| Taux d'arrêt général / `goalkeeper.totalSaveRate` | object non ; `value` number oui · percent | `LeagueMetricValueDto` | 100 × arrêts / tirs subis · `totalSaves` / `totalShotsFaced` | objet 1.0 · min 10 · qualité serveur | atomes exacts v1 → `V1_PARTIAL` | taux isolé, D=0→0 % | RM | complet/zéro/fiabilité · `V2_COMPLETE` |
| Taux d'arrêt dans le jeu / `goalkeeper.openPlaySaveRate` | object non ; `value` number oui · percent | `LeagueMetricValueDto` | 100 × arrêts jeu / tirs jeu subis · `openPlaySaves` / `openPlayShotsFaced` | objet 1.0 · min 10 · qualité serveur | atomes exacts v1 → `V1_PARTIAL` | 7 m/hors cadre | RM | complet/zéro · `V2_COMPLETE` |
| Taux d'arrêt sur 7 m / `goalkeeper.penaltySaveRate` | object non ; `value` number oui · percent | `LeagueMetricValueDto` | 100 × arrêts 7 m / tirs 7 m subis · `penaltySaves` / `penaltyShotsFaced` | objet 1.0 · min 2 · qualité serveur | atomes exacts v1 → `V1_PARTIAL` | taux global/7 m défensifs | RM | complet/zéro · `V2_COMPLETE` |
| Passes décisives / `goalkeeper.assists` | integer · non · count | `LeagueGoalkeeperMetricsDto.Assists` | compte personnel · valeur / N/A | réponse 1.0 · N/A · N/A | `global.assistCount` ou `goalkeeper.passeDecisives` → `V1_COMPATIBLE` | relances/buts | MV | exact · `V2_COMPLETE` |
| Buts / `goalkeeper.goals` | integer · non · count | `LeagueGoalkeeperMetricsDto.Goals` | jeu + 7 m personnels · valeur / N/A | réponse 1.0 · N/A · N/A | `global.totalGoals` → `V1_COMPATIBLE` | `goalkeeper.buts` seul | MV | total exact · `V2_COMPLETE` |
| Pertes de balle / `goalkeeper.totalTurnovers` | integer · non · count | `LeagueGoalkeeperMetricsDto.TotalTurnovers` | taxonomie commune · total / N/A | réponse 1.0 · N/A · N/A | `global.turnoverCount` ou `passing.totalPertes` → `V1_COMPATIBLE` | `goalkeeper.perteDeBalle` seul | MV | total exact · `V2_COMPLETE` |
| Tirs ratés / `goalkeeper.missedShots` | integer · non · count | `LeagueGoalkeeperMetricsDto.MissedShots` | échecs personnels jeu + 7 m · valeur / N/A | réponse 1.0 · N/A · N/A | `goalkeeper.tirsLoupes` ou `technical.shotWaste` → `V1_COMPATIBLE` | tirs subis | MV | personnels seulement · `V2_COMPLETE` |

## États transport et fallback

| Situation | Résultat attendu |
|---|---|
| v2 valide | `V2_COMPLETE`, valeur serveur conservée sans recalcul |
| section non demandée | section nullable seulement si absente de `included` |
| endpoint réellement indisponible (405/501) | fallback métrique par métrique : `V1_COMPATIBLE`, `V1_PARTIAL` ou `UNAVAILABLE` |
| 404 joueuse | erreur « introuvable », aucun fallback |
| timeout | erreur retryable, aucun fallback silencieux |
| 500 | erreur serveur retryable, aucun fallback silencieux |
| JSON invalide, contrat incomplet ou incohérent | `CONTRACT_ERROR`, aucun fallback |
| dénominateur v2 nul | `value = null`, preuve conservée, affichage « N/A — aucun tir » |
| taux v1 partiel | aucune version/qualité inventée ; qualité `Unknown`, provenance `V1_PARTIAL` |

## Couverture déterministe

Les tests couvrent la réponse complète et partielle, les valeurs nulles, `MetricVersion`, `MetricSample`, `MetricQuality`, les filtres et `include`, le `CancellationToken`, timeout/404/500/JSON invalide/contrat incomplet/endpoint indisponible, chaque fallback autorisé et interdit, les 34 métriques, les six taux, les volumes fiables et limités, le dénominateur zéro, l'absence de double comptage, la visibilité conditionnelle gardienne, la provenance, l'accessibilité et les non-régressions existantes.

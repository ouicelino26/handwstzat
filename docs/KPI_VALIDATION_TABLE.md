# Tableau de validation KPI HandWStat

Ce document sert de grille de controle metier.

Ordre conseille de verification :
1. verifier le dictionnaire des actions BB
2. verifier les agregats intermediaires
3. verifier les taux de base
4. verifier ensuite les KPI UX affiches dans l'application

Convention de relecture suggeree dans la colonne `Validation metier / a modifier` :
- `[ ] OK`
- `[ ] Renommer`
- `[ ] Corriger formule`
- `[ ] A discuter`

Tu peux aussi ecrire directement ton commentaire dans cette 3e colonne.

Documents de reference :
- [KPI_REFERENCE.md](C:/Users/donov/source/repos/HandWStat/docs/KPI_REFERENCE.md)
- [KPI_REFERENCE_DETAILED.md](C:/Users/donov/source/repos/HandWStat/docs/KPI_REFERENCE_DETAILED.md)

## 1. Dictionnaire des actions BB

| KPI / Action | Formule actuelle | Validation metier / a modifier |
| --- | --- | --- |
| `But` | evenement classe dans `Goals` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `But sur penalty` | evenement classe dans `PenaltyGoals` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Tir a cote` | evenement classe dans `ShotMisses` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Tir sur poteau` | evenement classe dans `ShotMisses` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Tir arrete` | evenement classe dans `ShotMisses` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Tir rate` | evenement classe dans `ShotMisses` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Tir contre` | evenement classe dans `ShotMisses` et detail `TirContre` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Penalty sur poteau` | evenement classe dans `PenaltyMisses` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Penalty rate` | evenement classe dans `PenaltyMisses` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Penalty arrete` | evenement classe dans `PenaltyMisses` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Passe decisive` | evenement classe dans `Assists` / `PasseDecisive` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Mauvaise passe` | evenement classe dans `BadPasses` / `MauvaisePasse` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Perte de balle` | evenement classe dans `BallLosses` / `PerteDeBalle` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Faute de pied` | evenement classe dans `TechnicalFaults` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Marcher` | evenement classe dans `TechnicalFaults` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Zone` | evenement classe dans `TechnicalFaults` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Reprise de dribble` | evenement classe dans `TechnicalFaults` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Refus de jeu` | evenement classe dans `TechnicalFaults` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Passage en force` | evenement classe dans `PassageEnForce` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Interception` | evenement classe dans `Interceptions` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Contre Reussi` | evenement classe dans `CounterSuccesses` / `Contres` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Neutralise l'attaquant` | evenement classe dans `Neutralisations` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Provoque Passage force` | evenement classe dans `ForcedPassages` / `PassageForce` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Exclusion` | evenement classe dans `Exclusions` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Avertissement` | evenement classe dans `Warnings` / `Avertissements` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Deux minutes` | evenement classe dans `TwoMinutes` / `DeuxMinutes` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Penalty concede` | evenement classe dans `PenaltyConceded` / `PenaltyConcede` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Gardien arrete le tir` | evenement classe dans `GoalkeeperSaves` / `Arrets` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Gardien arrete le penalty` | evenement classe dans `GoalkeeperPenaltySaves` / `ArretsPenalty` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Gardien prend un but` | evenement classe dans `GoalkeeperGoalsConceded` / `ButsPris` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Gardien prend le penalty` | evenement classe dans `GoalkeeperPenaltyGoalsConceded` / `ButsPenalty` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |

## 2. Agregats intermediaires

| KPI / Action | Formule actuelle | Validation metier / a modifier |
| --- | --- | --- |
| `TotalGoals` | `Goals + PenaltyGoals` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Tirs` | `TotalGoals + ShotMisses + PenaltyMisses` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `TirsLoupes` | `ShotMisses + PenaltyMisses` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `TotalTurnovers` | `BadPasses + BallLosses + TechnicalFaults + PassageEnForce` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `DefensiveImpact` | `Interceptions + Contres + Neutralisations + PassageForce` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `GoalkeeperStops` | `Arrets + ArretsPenalty` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `TirsSubis` | `Arrets + ArretsPenalty + ButsPris + ButsPenalty` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `SanctionCount` | `Exclusions + Avertissements + DeuxMinutes + PenaltyConcede` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `DirectContributions` | `TotalGoals + AssistCount` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |

## 3. Taux de base

| KPI / Action | Formule actuelle | Validation metier / a modifier |
| --- | --- | --- |
| `TauxReussiteTir` | `Goals / (Goals + ShotMisses) * 100` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `TauxReussitePenalty` | `PenaltyGoals / (PenaltyGoals + PenaltyMisses) * 100` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `TauxArret` | `(Arrets + ArretsPenalty) / (Arrets + ArretsPenalty + ButsPris + ButsPenalty) * 100` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `SuccessRate zone spatiale` | `SuccessCount / Attempts * 100` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |

## 4. KPI UX - Dashboard

| KPI / Action | Formule actuelle | Validation metier / a modifier |
| --- | --- | --- |
| `Equipes actives` | `Overview.TeamCount` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Cadence offensive` | `Overview.GoalCount / Overview.MatchCount` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Jeu prepare` | `Overview.AssistCount / Overview.GoalCount * 100` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Buts / match` | `Overview.GoalCount / Overview.MatchCount` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Buts prepares` | `Overview.AssistCount / Overview.GoalCount * 100` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Interceptions / match` | `Overview.InterceptionCount / Overview.MatchCount` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Arrets / match` | `Overview.SaveCount / Overview.MatchCount` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Pertes / match` | `Overview.TurnoverCount / Overview.MatchCount` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Sanctions / match` | `Overview.SanctionCount / Overview.MatchCount` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Actions directes / match` | `(TotalGoals + Assists) / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Ballons valorises` | `Assists / (Assists + Turnovers) * 100` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Impact def. / match` | `DefensiveImpact / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Arrets / match spotlight` | `GoalkeeperStops / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Taux de tir` | `ShootingRate` API | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Taux d'arret` | `Goalkeeper.TauxArret` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Tirs rates / match` | `MissedShots / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Buts pris / match` | `(ButsPris + ButsPenalty) / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Sanctions / match spotlight` | `TotalSanctions / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |

## 5. KPI UX - Joueuses

| KPI / Action | Formule actuelle | Validation metier / a modifier |
| --- | --- | --- |
| `Matchs` | `Profile.MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Buts` | `Global.TotalGoals` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Passes` | `Global.AssistCount` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Interceptions` | `Global.InterceptionCount` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Arrets` | `Global.SaveCount` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Pertes` | `Global.TurnoverCount` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Tir` | `Global.ShotSuccessRate` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Penalty` | `Global.PenaltySuccessRate` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Actions directes / match` | `(Global.TotalGoals + Global.AssistCount) / Global.MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Ballons valorises` | `Passing.PasseDecisive / (Passing.PasseDecisive + Passing.TotalPertes) * 100` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Impact def. / match` | `DefensiveImpact / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Arrets / match` | `GoalkeeperStops / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Taux de tir` | `Offense.TauxReussiteTir` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Taux d'arret` | `Goalkeeper.TauxArret` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Tirs rates / match` | `(Offense.TirsRates + Offense.PenaltyRate) / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Buts pris / match` | `(Goalkeeper.ButsPris + Goalkeeper.ButsPenalty) / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Sanctions / match` | `TotalSanctions / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |

## 6. KPI UX - Comparaison

| KPI / Action | Formule actuelle | Validation metier / a modifier |
| --- | --- | --- |
| `Directes / match` | `(TotalGoals + AssistCount) / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Ballons valorises` | `PasseDecisive / (PasseDecisive + TotalPertes) * 100` avec fallback `AssistCount / (AssistCount + TurnoverCount) * 100` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Impact def. / match` | `DefensiveImpact / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Arrets / match` | `GoalkeeperStops / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Taux de tir` | `Offense.TauxReussiteTir` sinon `ShotSuccessRate` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Taux d'arret` | `Goalkeeper.TauxArret` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Sanctions / match` | `TotalSanctions / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |

## 7. KPI UX - Equipes

| KPI / Action | Formule actuelle | Validation metier / a modifier |
| --- | --- | --- |
| `Matchs` | `MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Victoires` | `Wins` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Nuls` | `Draws` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Defaites` | `Losses` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Buts pour` | `GoalsFor` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Buts contre` | `GoalsAgainst` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Points / match` | `((Wins * 2) + Draws) / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Taux de victoire` | `Wins / MatchesPlayed * 100` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Diff. buts / match` | `(GoalsFor - GoalsAgainst) / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Buts marques / match` | `GoalsFor / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Buts encaisses / match` | `GoalsAgainst / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Ballons valorises` | `Overview.AssistCount / (Overview.AssistCount + Overview.TurnoverCount) * 100` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Arrets / match` | `Overview.SaveCount / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Sanctions / match` | `Overview.SanctionCount / MatchesPlayed` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |

## 8. KPI UX - Matchs

| KPI / Action | Formule actuelle | Validation metier / a modifier |
| --- | --- | --- |
| `Buts cumules` | `Team1Score final + Team2Score final` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Ecart final` | `abs(Team1Score final - Team2Score final)` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Jeu prepare` | `AssistCount / totalScore * 100` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Ballons valorises match` | `AssistCount / (AssistCount + TurnoverCount) * 100` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Actions def.` | `InterceptionCount + SaveCount` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Scoreuses a 3+ buts` | `count(players where TotalGoals >= 3)` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Score a la pause` | dernier snapshot score `<= 30:00` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Ecart final scenario` | `Team1Score final - Team2Score final` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Lead max eq. 1` | `max(Team1Score - Team2Score)` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Lead max eq. 2` | `max(Team2Score - Team1Score)` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Renversements` | nombre de changements de leader hors egalites | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Run max eq. 1` | plus grande serie sans reponse equipe 1 | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Run max eq. 2` | plus grande serie sans reponse equipe 2 | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Buts 2e MT` | `(score final eq1 - score pause eq1) + (score final eq2 - score pause eq2)` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `1re mi-temps` | score a `30:00` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `2e mi-temps` | `score final - score pause` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Dernier 10'` | `score final - score au point <= 50:00` | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |
| `Run cle` | meilleure serie sans reponse | `[ ] OK [ ] Renommer [ ] Corriger formule [ ] A discuter` |

## 9. Priorites de controle

Je te conseille de commencer par ces lignes :

1. `Ballons valorises`
2. `Taux de tir`
3. `Taux d'arret`
4. `Tirs rates / match`
5. `Impact def. / match`
6. `Points / match`
7. `Ecart final`

Ce sont les KPI qui ont le plus de chances d'etre :
- bien compris par l'UX
- mais potentiellement discutables metierement

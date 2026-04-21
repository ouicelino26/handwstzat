# Reference KPI HandWStat

Ce document recense les KPI actuellement affiches dans l'application, leur logique de calcul et la source de code utilisee pour les produire.

Version detaillee disponible ici :
- [KPI_REFERENCE_DETAILED.md](C:/Users/donov/source/repos/HandWStat/docs/KPI_REFERENCE_DETAILED.md)

Objectif :
- verifier que le wording correspond bien au calcul reel
- valider la coherence metier avant d'aller plus loin dans la refonte UX

## Base commune

Source principale :
- [KpiModels.cs](C:/Users/donov/source/repos/HandWStat/Models/Analytics/KpiModels.cs)

Formules utilitaires communes :

| Nom | Formule |
| --- | --- |
| `PerMatch(total, matchs)` | `total / matchs` si `matchs > 0`, sinon `0` |
| `Ratio(numerateur, denominateur)` | `numerateur / denominateur` si `denominateur > 0`, sinon `0` |
| `Share(numerateur, denominateur)` | `(numerateur / denominateur) * 100` si `denominateur > 0`, sinon `0` |
| `SuccessVsWasteShare(successes, failures)` | `successes / (successes + failures) * 100` |
| `DefensiveImpact(defense)` | `Interceptions + Contres + Neutralisations + PassageForce` |
| `GoalkeeperStops(goalkeeper)` | `Arrets + ArretsPenalty` |
| `TotalSanctions(sanctions)` | `Exclusions + Avertissements + DeuxMinutes + PenaltyConcede` |
| `DirectContributions(player)` | `TotalGoals + AssistCount` |

## Dashboard

Sources :
- [Home.razor.cs](C:/Users/donov/source/repos/HandWStat/Components/Pages/Home.razor.cs)
- [Dashboard.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Dashboard.razor)

### Cartes hero

| Libelle | Formule |
| --- | --- |
| `Equipes actives` | `Overview.TeamCount` |
| `Cadence offensive` | `Overview.GoalCount / Overview.MatchCount` |
| `Jeu prepare` | `Overview.AssistCount / Overview.GoalCount * 100` |

### KPI ligue

| Libelle | Formule |
| --- | --- |
| `Buts / match` | `Overview.GoalCount / Overview.MatchCount` |
| `Buts prepares` | `Overview.AssistCount / Overview.GoalCount * 100` |
| `Interceptions / match` | `Overview.InterceptionCount / Overview.MatchCount` |
| `Arrets / match` | `Overview.SaveCount / Overview.MatchCount` |
| `Pertes / match` | `Overview.TurnoverCount / Overview.MatchCount` |
| `Sanctions / match` | `Overview.SanctionCount / Overview.MatchCount` |

### KPI spotlight joueuse

| Libelle | Formule |
| --- | --- |
| `Actions directes / match` | `(TotalGoals + Assists) / MatchesPlayed` |
| `Ballons valorises` | `Assists / (Assists + Turnovers) * 100` |
| `Impact def. / match` | `(Interceptions + Contres + Neutralisations + PassageForce) / MatchesPlayed` |
| `Arrets / match` | `(Arrets + ArretsPenalty) / MatchesPlayed` |
| `Taux de tir` | `Spotlight.ShootingRate` fourni par l'API |
| `Taux d'arret` | `Goalkeeper.TauxArret` fourni par l'API |
| `Tirs rates / match` | `MissedShots / MatchesPlayed` |
| `Buts pris / match` | `(ButsPris + ButsPenalty) / MatchesPlayed` |
| `Sanctions / match` | `TotalSanctions / MatchesPlayed` |

Note :
- pour les pourcentages issus de l'API (`ShootingRate`, `TauxArret`), l'UI affiche aussi un contexte base/volume pour aider a verifier la logique.

## Joueuses

Source :
- [Players.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Players.razor)

### Cartes de contexte

| Libelle | Formule |
| --- | --- |
| `Matchs` | `Profile.MatchesPlayed` |
| `Buts` | `Global.TotalGoals` |
| `Passes` | `Global.AssistCount` |
| `Interceptions` | `Global.InterceptionCount` |
| `Arrets` | `Global.SaveCount` |
| `Pertes` | `Global.TurnoverCount` |
| `Tir` | `Global.ShotSuccessRate` |
| `Penalty` | `Global.PenaltySuccessRate` |

### KPI fiche joueuse

| Libelle | Formule |
| --- | --- |
| `Actions directes / match` | `(Global.TotalGoals + Global.AssistCount) / Global.MatchesPlayed` |
| `Ballons valorises` | `Passing.PasseDecisive / (Passing.PasseDecisive + Passing.TotalPertes) * 100` |
| `Impact def. / match` | `(Interceptions + Contres + Neutralisations + PassageForce) / MatchesPlayed` |
| `Arrets / match` | `(Arrets + ArretsPenalty) / MatchesPlayed` |
| `Taux de tir` | `Offense.TauxReussiteTir` |
| `Taux d'arret` | `Goalkeeper.TauxArret` |
| `Tirs rates / match` | `(Offense.TirsRates + Offense.PenaltyRate) / MatchesPlayed` |
| `Buts pris / match` | `(Goalkeeper.ButsPris + Goalkeeper.ButsPenalty) / MatchesPlayed` |
| `Sanctions / match` | `TotalSanctions / MatchesPlayed` |

### Graphes joueuse

Ces graphes ne sont pas des KPI cards mais utilisent ces derivees :

| Graphe | Formule |
| --- | --- |
| `Profil d'impact` | buts, passes, impact def., arrets, pertes, tirs rates |
| `Rendement` | tir %, penalty % ou arret %, actions directes par match, sanctions par match |
| `Tendance recente` | contributions directes, impact def., pertes, arrets si gardienne |

## Comparaison

Source :
- [Compare.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Compare.razor)

### KPI compare

| Libelle | Formule |
| --- | --- |
| `Directes / match` | `(TotalGoals + AssistCount) / MatchesPlayed` |
| `Ballons valorises` | `PasseDecisive / (PasseDecisive + TotalPertes) * 100` |
| `Impact def. / match` | `(Interceptions + Contres + Neutralisations + PassageForce) / MatchesPlayed` |
| `Arrets / match` | `(Arrets + ArretsPenalty) / MatchesPlayed` |
| `Taux de tir` | `Offense.TauxReussiteTir`, sinon `player.ShotSuccessRate` en secours |
| `Taux d'arret` | `Goalkeeper.TauxArret` |
| `Sanctions / match` | `TotalSanctions / MatchesPlayed` |

### Graphes comparaison

| Graphe | Formule |
| --- | --- |
| `Volumes cles` | buts, passes, interceptions, arrets, pertes |
| `Efficacite` | tir %, penalty %, arret GB % |
| `Production par match` | directes/match, def./match, pertes/match, sanctions/match |

## Equipes

Source :
- [Teams.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Teams.razor)

### Cartes de contexte

| Libelle | Formule |
| --- | --- |
| `Matchs` | `MatchesPlayed` |
| `Victoires` | `Wins` |
| `Nuls` | `Draws` |
| `Defaites` | `Losses` |
| `Buts pour` | `GoalsFor` |
| `Buts contre` | `GoalsAgainst` |
| `Passes` | `Overview.AssistCount` |
| `Interceptions` | `Overview.InterceptionCount` |

### KPI equipe

| Libelle | Formule |
| --- | --- |
| `Points / match` | `((Wins * 2) + Draws) / MatchesPlayed` |
| `Taux de victoire` | `Wins / MatchesPlayed * 100` |
| `Diff. buts / match` | `(GoalsFor - GoalsAgainst) / MatchesPlayed` |
| `Buts marques / match` | `GoalsFor / MatchesPlayed` |
| `Buts encaisses / match` | `GoalsAgainst / MatchesPlayed` |
| `Ballons valorises` | `Overview.AssistCount / (Overview.AssistCount + Overview.TurnoverCount) * 100` |
| `Arrets / match` | `Overview.SaveCount / MatchesPlayed` |
| `Sanctions / match` | `Overview.SanctionCount / MatchesPlayed` |

Attention :
- `Points / match` part d'une logique `2 points victoire + 1 point nul`. Si votre championnat suit une autre regle, ce KPI sera a ajuster.

## Matchs

Sources :
- [MatchKpiCatalog.cs](C:/Users/donov/source/repos/HandWStat/Models/Analytics/MatchKpiCatalog.cs)
- [MatchScenarioAnalyzer.cs](C:/Users/donov/source/repos/HandWStat/Models/Analytics/MatchScenarioAnalyzer.cs)
- [Matches.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Matches.razor)

### KPI resume match

| Libelle | Formule |
| --- | --- |
| `Buts cumules` | `(Team1Score final + Team2Score final)` |
| `Ecart final` | `abs(Team1Score final - Team2Score final)` |
| `Jeu prepare` | `AssistCount / totalScore * 100` |
| `Ballons valorises` | `AssistCount / (AssistCount + TurnoverCount) * 100` |
| `Actions def.` | `InterceptionCount + SaveCount` |
| `Scoreuses a 3+ buts` | `nombre de joueuses avec TotalGoals >= 3` |

### KPI scenario / timeline

| Libelle | Formule |
| --- | --- |
| `Score a la pause` | snapshot du score au dernier point `<= 30:00` |
| `Ecart final` | `Team1Score final - Team2Score final` avec signe |
| `Lead max eq. 1` | `max(Team1Score - Team2Score)` |
| `Lead max eq. 2` | `max(Team2Score - Team1Score)` |
| `Renversements` | nombre de changements de leader hors egalites |
| `Run max eq. 1` | plus grande serie de buts consecutifs sans reponse |
| `Run max eq. 2` | plus grande serie de buts consecutifs sans reponse |
| `Buts 2e MT` | buts inscrits apres le score fige de mi-temps |

### Insights de phase

| Libelle | Formule |
| --- | --- |
| `1re mi-temps` | score a `30:00` |
| `2e mi-temps` | `score final - score a la pause` |
| `Dernier 10'` | `score final - score au point <= 50:00` |
| `Run cle` | meilleure serie sans reponse |

## Coherence a verifier en priorite

Voici les points qui meritent une validation metier avec toi :

1. `Ballons valorises`
   C'est aujourd'hui un ratio `passes decisives / (passes decisives + pertes)`.
   Le nom est utile en UX, mais il ne mesure pas toute la qualite de possession.

2. `Taux de tir`
   L'UI consomme souvent un champ API (`TauxReussiteTir` / `ShotSuccessRate`).
   Il faut confirmer que le backend traite bien les penalties comme tu le souhaites.

3. `Tirs rates / match`
   Sur la fiche joueuse, le calcul inclut `TirsRates + PenaltyRate`.
   Il faut verifier si `PenaltyRate` represente bien un volume de penalties rates et non un taux.

4. `Points / match`
   Le calcul equipe repose sur la convention `2 points victoire`.
   Si votre contexte competition ne suit pas cette regle, il faut le changer.

5. `Ecart final`
   Il existe deux versions dans l'app :
   - version absolue sur la carte resume match
   - version signee dans la lecture de scenario
   C'est coherent si on assume deux usages differents, mais il faut le garder explicite dans le wording.

## Recommandation produit

Avant toute nouvelle vague de KPI, je recommande :
- valider le dictionnaire metier de chaque libelle
- verrouiller 1 definition unique par KPI
- distinguer clairement :
  - totals bruts
  - ratios / pourcentages
  - valeurs par match
  - indicateurs de scenario

Une fois cette validation faite, on pourra renommer certains KPI pour coller encore mieux au langage du staff, des analystes et des joueuses.

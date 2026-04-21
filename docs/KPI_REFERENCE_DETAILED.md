# Reference KPI detaillee HandWStat

Ce document est la version detaillee du referentiel KPI de l'application.

Il sert a verifier :
- le sens exact des actions remontees par le back
- les agregations intermediaires
- les formules de taux
- les KPI affiches dans chaque ecran
- les zones de doute metier a valider

Sources principales :
- [AnalyticsDtos.cs](D:/repos/HandballManagerCore/HandballManagerCore/DTO/AnalyticsDtos.cs)
- [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs)
- [PlayerStatsService.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/PlayerStatsService.cs)
- [KpiModels.cs](C:/Users/donov/source/repos/HandWStat/Models/Analytics/KpiModels.cs)
- [Home.razor.cs](C:/Users/donov/source/repos/HandWStat/Components/Pages/Home.razor.cs)
- [Players.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Players.razor)
- [Compare.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Compare.razor)
- [Teams.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Teams.razor)
- [MatchKpiCatalog.cs](C:/Users/donov/source/repos/HandWStat/Models/Analytics/MatchKpiCatalog.cs)
- [MatchScenarioAnalyzer.cs](C:/Users/donov/source/repos/HandWStat/Models/Analytics/MatchScenarioAnalyzer.cs)

## 1. Dictionnaire des actions BB

Les termes ci-dessous sont ceux interpretes par le moteur analytics. Ils viennent du classifieur d'evenements dans `AnalyticsInfrastructure.cs`.

### 1.1 Actions offensives

| Terme BB | Famille | Compteur interne | Commentaire |
| --- | --- | --- | --- |
| `But` | Tir reussi | `Goals` | But hors penalty |
| `But sur penalty` | Tir reussi | `PenaltyGoals` | But marque sur 7m |
| `Tir a cote` | Tir rate | `ShotMisses` | Compte dans les tirs rates hors penalty |
| `Tir sur poteau` | Tir rate | `ShotMisses` | Compte dans les tirs rates hors penalty |
| `Tir arrete` | Tir rate | `ShotMisses` | Le tir est bien compte comme rate pour la tireuse |
| `Tir rate` | Tir rate | `ShotMisses` | Libelle generique |
| `Tir contre` | Tir rate | `ShotMisses` et `TirContre` | Compte a la fois comme tir rate et comme sous-detail `tir contre` |
| `Penalty sur poteau` | Penalty rate | `PenaltyMisses` | Penalty manque |
| `Penalty rate` | Penalty rate | `PenaltyMisses` | Penalty manque |
| `Penalty arrete` | Penalty rate | `PenaltyMisses` | Penalty manque et arrete par la gardienne |

### 1.2 Creation et pertes de balle

| Terme BB | Famille | Compteur interne | Commentaire |
| --- | --- | --- | --- |
| `Passe decisive` | Creation | `Assists` / `PasseDecisive` | Sert aux KPI de jeu prepare et ballons valorises |
| `Mauvaise passe` | Perte | `BadPasses` / `MauvaisePasse` | Sous-famille de perte |
| `Perte de balle` | Perte | `BallLosses` / `PerteDeBalle` | Sous-famille de perte |
| `Faute de pied` | Faute technique | `TechnicalFaults` | Entre dans `TotalPertes` |
| `Marcher` | Faute technique | `TechnicalFaults` | Entre dans `TotalPertes` |
| `Zone` | Faute technique | `TechnicalFaults` | Entre dans `TotalPertes` |
| `Reprise de dribble` | Faute technique | `TechnicalFaults` | Entre dans `TotalPertes` |
| `Refus de jeu` | Faute technique | `TechnicalFaults` | Entre dans `TotalPertes` |
| `Passage en force` | Perte offensive | `PassageEnForce` | Entre dans `TotalPertes` |

### 1.3 Actions defensives joueuse de champ

| Terme BB | Famille | Compteur interne | Commentaire |
| --- | --- | --- | --- |
| `Interception` | Defense | `Interceptions` | Action defensive directe |
| `Contre Reussi` | Defense | `CounterSuccesses` / `Contres` | Appele `Contres` dans les DTO |
| `Neutralise l'attaquant` | Defense | `Neutralisations` | Action defensive sans interception |
| `Provoque Passage force` | Defense | `ForcedPassages` / `PassageForce` | Action defensive provoquant une faute offensive |

### 1.4 Sanctions

| Terme BB | Famille | Compteur interne | Commentaire |
| --- | --- | --- | --- |
| `Exclusion` | Sanction | `Exclusions` | |
| `Avertissement` | Sanction | `Warnings` / `Avertissements` | |
| `Deux minutes` | Sanction | `TwoMinutes` / `DeuxMinutes` | |
| `Penalty concede` | Sanction | `PenaltyConceded` / `PenaltyConcede` | Compte aussi dans `SanctionCount` et `TotalSanctions` |

### 1.5 Actions gardienne

| Terme BB | Famille | Compteur interne | Commentaire |
| --- | --- | --- | --- |
| `Gardien arrete le tir` | Arret gardienne | `GoalkeeperSaves` / `Arrets` | Arret hors penalty |
| `Gardien arrete le penalty` | Arret gardienne | `GoalkeeperPenaltySaves` / `ArretsPenalty` | Arret sur 7m |
| `Gardien prend un but` | But encaisse | `GoalkeeperGoalsConceded` / `ButsPris` | But encaisse hors penalty |
| `Gardien prend le penalty` | But encaisse | `GoalkeeperPenaltyGoalsConceded` / `ButsPenalty` | Penalty encaisse |

## 2. Aggregats techniques intermediaires

Les KPI de l'app ne repartent pas directement de chaque evenement. Ils s'appuient d'abord sur des agregats.

Source principale :
- [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:332)

### 2.1 Aggregats offensifs

| Nom technique | Formule |
| --- | --- |
| `TotalGoals` | `Goals + PenaltyGoals` |
| `Tirs` | `TotalGoals + ShotMisses + PenaltyMisses` |
| `TirsLoupes` | `ShotMisses + PenaltyMisses` |

Important :
- `ShotMisses` contient deja les `Tir contre`
- `TirContre` est un sous-detail d'analyse, pas un compteur autonome de tir

### 2.2 Aggregats de pertes

| Nom technique | Formule |
| --- | --- |
| `TotalTurnovers` | `BadPasses + BallLosses + TechnicalFaults + PassageEnForce` |

Donc `TotalPertes` dans les DTO de passes = somme :
- `MauvaisePasse`
- `PerteDeBalle`
- `FauteTechnique`
- `PassageEnForce`

### 2.3 Aggregats defensifs

| Nom technique | Formule |
| --- | --- |
| `DefensiveImpact` | `Interceptions + Contres + Neutralisations + PassageForce` |

### 2.4 Aggregats gardienne

| Nom technique | Formule |
| --- | --- |
| `SaveCount` | `GoalkeeperSaves + GoalkeeperPenaltySaves` |
| `TirsSubis` | `GoalkeeperSaves + GoalkeeperPenaltySaves + GoalkeeperGoalsConceded + GoalkeeperPenaltyGoalsConceded` |

Attention :
- `TirsSubis` = tous les tirs cadrant effectivement la gardienne
- `ButsPris` et `ButsPenalty` sont des buts encaisses, pas des tirs subis complets

### 2.5 Aggregats sanctions

| Nom technique | Formule |
| --- | --- |
| `SanctionCount` | `Exclusions + Warnings + TwoMinutes + PenaltyConceded` |
| `TotalSanctions` | meme logique cote UI : `Exclusions + Avertissements + DeuxMinutes + PenaltyConcede` |

## 3. Formules de taux de base

Source :
- [AnalyticsInfrastructure.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs:202)

Le back calcule les taux avec la logique suivante :

### 3.1 Formule generique

`ComputeRate(successCount, totalCount) = successCount / totalCount * 100`

Rappels :
- si `totalCount <= 0`, le resultat est `0`
- le back arrondit a `2` decimales
- l'UI reformatte ensuite souvent a `1` decimale ou sans decimale visible

### 3.2 Taux de tir joueuse

`ShotSuccessRate = Goals / (Goals + ShotMisses) * 100`

Important :
- ce taux ne prend pas les penalties
- il ne compte que le tir hors 7m

### 3.3 Taux de penalty joueuse

`PenaltySuccessRate = PenaltyGoals / (PenaltyGoals + PenaltyMisses) * 100`

### 3.4 Taux d'arret gardienne

`GoalkeeperSaveRate = (GoalkeeperSaves + GoalkeeperPenaltySaves) / (GoalkeeperSaves + GoalkeeperPenaltySaves + GoalkeeperGoalsConceded + GoalkeeperPenaltyGoalsConceded) * 100`

Equivalent DTO :
`TauxArret = (Arrets + ArretsPenalty) / TirsSubis * 100`

### 3.5 Taux de reussite tir gardienne

Le DTO gardienne expose aussi :

`TauxReussiteTir = TotalGoals / Tirs * 100`

Ce taux decrit la reussite offensive de la gardienne quand elle tire elle-meme.
Il ne faut pas le confondre avec le `TauxArret`.

## 4. Zones spatiales : but et terrain

Source :
- [SpatialStatsService.cs](D:/repos/HandballManagerAPI/HandballManagerAPI/Analytics/SpatialStatsService.cs:159)

Pour chaque zone de but ou zone de terrain :

| Champ | Formule |
| --- | --- |
| `Attempts` | nombre total d'evenements spatiaux dans la zone |
| `SuccessCount` | nombre d'evenements qualifies de reussite |
| `FailureCount` | `Attempts - SuccessCount` |
| `SuccessRate` | `SuccessCount / Attempts * 100` |

Important :
- seules les actions `IsSpatialIncluded = true` entrent dans ce calcul
- certaines actions sont volontairement exclues du spatial, par exemple :
  - `Passe decisive`
  - `Mauvaise passe`
  - `Interception`
  - `Perte de balle`
  - `Avertissement`
  - `Marcher`
  - `Passage en force`
  - `Deux minutes`
  - `Zone`
  - `Neutralise l'attaquant`

Donc :
- les zones affichent surtout la lecture des tirs / finalisations spatiales
- elles ne resumment pas toute l'activite du match

## 5. KPI affiches dans l'application

## 5.1 Dashboard

Source :
- [Home.razor.cs](C:/Users/donov/source/repos/HandWStat/Components/Pages/Home.razor.cs:151)

### Cartes hero

| KPI | Formule | Lecture |
| --- | --- | --- |
| `Equipes actives` | `Overview.TeamCount` | nombre d'equipes couvertes par le filtre |
| `Cadence offensive` | `Overview.GoalCount / Overview.MatchCount` | buts moyens par match |
| `Jeu prepare` | `Overview.AssistCount / Overview.GoalCount * 100` | part des buts venant d'une passe decisive |

### KPI ligue

| KPI | Formule detaillee |
| --- | --- |
| `Buts / match` | `Overview.GoalCount / Overview.MatchCount` |
| `Buts prepares` | `Overview.AssistCount / Overview.GoalCount * 100` |
| `Interceptions / match` | `Overview.InterceptionCount / Overview.MatchCount` |
| `Arrets / match` | `Overview.SaveCount / Overview.MatchCount` |
| `Pertes / match` | `Overview.TurnoverCount / Overview.MatchCount` |
| `Sanctions / match` | `Overview.SanctionCount / Overview.MatchCount` |

### KPI spotlight joueuse

Source :
- [Home.razor.cs](C:/Users/donov/source/repos/HandWStat/Components/Pages/Home.razor.cs:197)

| KPI | Formule detaillee | Remarque |
| --- | --- | --- |
| `Actions directes / match` | `(TotalGoals + Assists) / MatchesPlayed` | creation + finition directe |
| `Ballons valorises` | `Assists / (Assists + Turnovers) * 100` | ratio simplifie creation vs dechet |
| `Impact def. / match` | `(Interceptions + Contres + Neutralisations + PassageForce) / MatchesPlayed` | si joueuse de champ |
| `Arrets / match` | `(Arrets + ArretsPenalty) / MatchesPlayed` | si gardienne |
| `Taux de tir` | `Offense.Buts / (Offense.Buts + Offense.TirsRates) * 100` | valeur fournie par l'API via `ShootingRate` |
| `Taux d'arret` | `(Arrets + ArretsPenalty) / TirsSubis * 100` | valeur fournie par `Goalkeeper.TauxArret` |
| `Tirs rates / match` | `MissedShots / MatchesPlayed` | cote spotlight, l'agregat est deja preconstruit |
| `Buts pris / match` | `(ButsPris + ButsPenalty) / MatchesPlayed` | gardienne |
| `Sanctions / match` | `TotalSanctions / MatchesPlayed` | |

## 5.2 Joueuses

Source :
- [Players.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Players.razor:851)

### Cartes de contexte

| KPI / carte | Formule |
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

| KPI | Formule detaillee | Detail technique |
| --- | --- | --- |
| `Actions directes / match` | `(Global.TotalGoals + Global.AssistCount) / Global.MatchesPlayed` | utilise le total buts + passes |
| `Ballons valorises` | `Passing.PasseDecisive / (Passing.PasseDecisive + Passing.TotalPertes) * 100` | `TotalPertes` = mauvaises passes + pertes + fautes techniques + passages en force |
| `Impact def. / match` | `(Interceptions + Contres + Neutralisations + PassageForce) / MatchesPlayed` | joueuse de champ |
| `Arrets / match` | `(Arrets + ArretsPenalty) / MatchesPlayed` | gardienne |
| `Taux de tir` | `Offense.TauxReussiteTir` | hors penalties |
| `Taux de penalty` | `Offense.TauxReussitePenalty` | visible surtout dans les graphes |
| `Taux d'arret` | `Goalkeeper.TauxArret` | gardienne |
| `Tirs rates / match` | `(Offense.TirsRates + Offense.PenaltyRate) / MatchesPlayed` | ici l'UI additionne bien les tirs rates hors penalty et les penalties rates |
| `Buts pris / match` | `(Goalkeeper.ButsPris + Goalkeeper.ButsPenalty) / MatchesPlayed` | gardienne |
| `Sanctions / match` | `(Exclusions + Avertissements + DeuxMinutes + PenaltyConcede) / MatchesPlayed` | |

### Graphes joueuse

Source :
- [Players.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Players.razor:906)

#### Profil d'impact

| Axe | Formule |
| --- | --- |
| `Buts` | `Global.TotalGoals` |
| `Passes` | `Global.AssistCount` |
| `Impact def.` | `Interceptions + Contres + Neutralisations + PassageForce` |
| `Arrets` | `Arrets + ArretsPenalty` |
| `Pertes` | `Passing.TotalPertes` |
| `Tirs rates` | `Offense.TirsRates + Offense.PenaltyRate` |

#### Profil rendement

| Axe | Formule |
| --- | --- |
| `Tir %` | `Goalkeeper.TauxReussiteTir` si gardienne, sinon `Offense.TauxReussiteTir` |
| `Arret %` | `Goalkeeper.TauxArret` si gardienne |
| `Penalty %` | `Offense.TauxReussitePenalty` si joueuse de champ |
| `Ballons +` | `PasseDecisive / (PasseDecisive + TotalPertes) * 100` |
| `Directes/m` | `(TotalGoals + AssistCount) / MatchesPlayed` |
| `Sanctions/m` | `TotalSanctions / MatchesPlayed` |

#### Tendance recente

Sur les 8 derniers matchs maximum :

| Serie | Formule |
| --- | --- |
| `Contributions directes` | `Goals + Assists` du match |
| `Impact def.` | `Interceptions + Saves` du match |
| `Pertes` | `Turnovers` du match |
| `Arrets` | `Saves` du match |

Note :
- dans cette tendance recente, `Impact def.` est simplifie au niveau match en `Interceptions + Saves`
- ce n'est donc pas exactement le meme perimetre que le KPI global `Impact def.` sur la fiche joueuse

## 5.3 Comparaison

Source :
- [Compare.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Compare.razor:827)

### KPI comparaison

| KPI | Formule detaillee |
| --- | --- |
| `Directes / match` | `(TotalGoals + AssistCount) / MatchesPlayed` |
| `Ballons valorises` | `PasseDecisive / (PasseDecisive + TotalPertes) * 100` ; fallback `AssistCount / (AssistCount + TurnoverCount) * 100` si detail passing absent |
| `Impact def. / match` | `(Interceptions + Contres + Neutralisations + PassageForce) / MatchesPlayed` |
| `Arrets / match` | `(Arrets + ArretsPenalty) / MatchesPlayed` pour gardienne |
| `Taux de tir` | `Offense.TauxReussiteTir` si detail offense disponible, sinon `ShotSuccessRate` |
| `Taux d'arret` | `Goalkeeper.TauxArret` |
| `Sanctions / match` | `TotalSanctions / MatchesPlayed` |

### Graphes comparaison

#### Volumes

| Axe | Formule |
| --- | --- |
| `Buts` | `TotalGoals` |
| `Passes` | `AssistCount` |
| `Interceptions` | `InterceptionCount` |
| `Arrets` | `SaveCount` |
| `Pertes` | `TurnoverCount` |

#### Efficacite

| Axe | Formule |
| --- | --- |
| `Tir` | `ShotSuccessRate` |
| `Penalty` | `PenaltySuccessRate` |
| `Arret GB` | `Goalkeeper.TauxArret` |

#### Production par match

| Axe | Formule |
| --- | --- |
| `Directes/m` | `(TotalGoals + AssistCount) / MatchesPlayed` |
| `Def./m` | `GoalkeeperStops / MatchesPlayed` si gardienne, sinon `DefensiveImpact / MatchesPlayed` |
| `Pertes/m` | `TurnoverCount / MatchesPlayed` |
| `Sanctions/m` | `TotalSanctions / MatchesPlayed` |

## 5.4 Equipes

Source :
- [Teams.razor](C:/Users/donov/source/repos/HandWStat/Components/Pages/Teams.razor:701)

### Cartes de contexte

| Carte | Formule |
| --- | --- |
| `Matchs` | `TeamStats.MatchesPlayed` |
| `Victoires` | `TeamStats.Wins` |
| `Nuls` | `TeamStats.Draws` |
| `Defaites` | `TeamStats.Losses` |
| `Buts pour` | `TeamStats.GoalsFor` |
| `Buts contre` | `TeamStats.GoalsAgainst` |
| `Passes` | `TeamStats.Overview.AssistCount` |
| `Interceptions` | `TeamStats.Overview.InterceptionCount` |

### KPI equipe

| KPI | Formule detaillee | Note |
| --- | --- | --- |
| `Points / match` | `((Wins * 2) + Draws) / MatchesPlayed` | hypothese championnat a 2 points la victoire |
| `Taux de victoire` | `Wins / MatchesPlayed * 100` | |
| `Diff. buts / match` | `(GoalsFor - GoalsAgainst) / MatchesPlayed` | signee |
| `Buts marques / match` | `GoalsFor / MatchesPlayed` | |
| `Buts encaisses / match` | `GoalsAgainst / MatchesPlayed` | |
| `Ballons valorises` | `AssistCount / (AssistCount + TurnoverCount) * 100` | lecture simple de qualite de possession |
| `Arrets / match` | `SaveCount / MatchesPlayed` | |
| `Sanctions / match` | `SanctionCount / MatchesPlayed` | |

## 5.5 Matchs

Source :
- [MatchKpiCatalog.cs](C:/Users/donov/source/repos/HandWStat/Models/Analytics/MatchKpiCatalog.cs)
- [MatchScenarioAnalyzer.cs](C:/Users/donov/source/repos/HandWStat/Models/Analytics/MatchScenarioAnalyzer.cs)

### KPI resume match

| KPI | Formule detaillee | Note |
| --- | --- | --- |
| `Buts cumules` | `(Team1Score final + Team2Score final)` | score total visible |
| `Ecart final` | `abs(Team1Score final - Team2Score final)` | version absolue |
| `Jeu prepare` | `AssistCount / totalScore * 100` | totalScore = score final des 2 equipes |
| `Ballons valorises` | `AssistCount / (AssistCount + TurnoverCount) * 100` | au niveau match |
| `Actions def.` | `InterceptionCount + SaveCount` | lecture def. simplifiee |
| `Scoreuses a 3+ buts` | `count(players where TotalGoals >= 3)` | compte les joueuses menace |

### KPI scenario / timeline

| KPI | Formule detaillee |
| --- | --- |
| `Score a la pause` | score du dernier snapshot `<= 30:00` |
| `Ecart final` | `Team1Score final - Team2Score final` |
| `Lead max eq. 1` | maximum de `(Team1Score - Team2Score)` au fil du match |
| `Lead max eq. 2` | maximum de `(Team2Score - Team1Score)` au fil du match |
| `Renversements` | nombre de changements de leader hors phases d'egalite |
| `Run max eq. 1` | plus grande serie consecutive sans reponse de l'equipe 1 |
| `Run max eq. 2` | plus grande serie consecutive sans reponse de l'equipe 2 |
| `Buts 2e MT` | `(score final eq1 - score pause eq1) + (score final eq2 - score pause eq2)` |

### Insights de phase

| Insight | Formule |
| --- | --- |
| `1re mi-temps` | score a 30:00 |
| `2e mi-temps` | `score final - score pause` |
| `Dernier 10'` | `score final - score au snapshot <= 50:00` |
| `Run cle` | meilleure serie sans reponse du match |

## 6. Points de vigilance metier

### 6.1 `Ballons valorises`

Aujourd'hui, le KPI veut dire :

`passes decisives / (passes decisives + pertes) * 100`

Ce que cela mesure bien :
- une balance creation / dechet

Ce que cela ne mesure pas :
- toutes les possessions
- les passes non decisives mais utiles
- les tirs forces qui ne sont pas des pertes

Donc le nom est UX-friendly, mais ce n'est pas un vrai taux de possession reussie.

### 6.2 `Taux de tir`

Le systeme separe bien :
- `Taux de tir` hors penalty
- `Taux de penalty`

Si, metierement, tu veux un `taux de tir global incluant les 7m`, il faudra un KPI distinct.

### 6.3 `Tirs rates / match`

Sur la fiche joueuse, la formule est :

`(TirsRates + PenaltyRate) / MatchesPlayed`

Donc :
- les penalties rates sont bien inclus
- le nom actuel est coherent si on veut parler de tous les tirs manques

### 6.4 `Impact def.`

Le KPI global joueuse utilise :

`Interceptions + Contres + Neutralisations + PassageForce`

Mais la tendance recente par match utilise une version plus simple :

`Interceptions + Saves`

Il y a donc aujourd'hui deux perimetres differents caches derriere un libelle proche.

### 6.5 `Ecart final`

Il existe deux lectures :
- `abs(diff)` dans le resume match
- `diff signe` dans le scenario match

Les deux peuvent coexister, mais il faut garder le wording tres explicite.

### 6.6 `Points / match`

Le calcul equipe suppose :
- victoire = `2`
- nul = `1`
- defaite = `0`

Si ton championnat ne suit pas cette convention, ce KPI devient faux.

## 7. Recommandation pour la verification metier

L'ordre le plus sain pour verifier la coherence est :

1. Verifier le dictionnaire des actions BB.
2. Verifier les agregats intermediaires :
   - `TotalTurnovers`
   - `DefensiveImpact`
   - `GoalkeeperStops`
   - `TirsSubis`
3. Verifier les taux de base :
   - `TauxReussiteTir`
   - `TauxReussitePenalty`
   - `TauxArret`
4. Verifier ensuite seulement les KPI UX.

Si tu veux, la passe suivante que je peux faire est :
- te produire un tableau de validation KPI par KPI avec 3 colonnes :
  - `KPI`
  - `Formule actuelle`
  - `Validation metier / a modifier`

Comme ca tu pourras cocher ce qui est juste, faux, ou a renommer.

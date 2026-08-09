# DATABASE_ANALYTICS_SCHEMA_MAP

Date: 2026-08-01 | Audit: read-only SSH → sudo mysql hbdb | Branch: fix/handwstat-final-validation-v1

## Tables utilisées par l'API analytique

| Table | Clé | Rôle |
|---|---|---|
| matchs | Id | Match (Competition, Team1, Team2, Date, Season, Day, Scores) |
| matchevents | Id | Événements de match (EventId, PlayerId, TeamId, Scores, Time, Zones…) |
| players | Id | Joueur (PositionId, TeamId, IsActive) |
| positions | Id | Code + Nom de poste |
| events | Id | Type d'événement |
| teams | Id | Équipe |
| competitions | Id | Compétition |
| attack / defense | Id | Systèmes tactiques |
| timeplayers | Id | Temps de jeu par match |

## Schéma matchevents

| Colonne | Type | Null | Notes |
|---|---|---|---|
| Id | int | NO | PK auto |
| MatchId | int | YES | FK → matchs.Id |
| PlayerId | int | YES | FK → players.Id |
| Time | time | YES | Horloge (hh:mm:ss) |
| TeamScore1 | int | YES | Score équipe 1 au moment de l'événement |
| TeamScore2 | int | YES | Score équipe 2 au moment de l'événement |
| EventId | int | YES | FK → events.Id |
| ShootZone | varchar | YES | Zone de tir (G5, D6…) – très peu renseignée |
| Trigger | varchar | YES | Déclencheur tactique |
| TeamId | int | YES | Équipe de l'événement |
| AttackId | int | YES | FK → attack |
| DefenseId | int | YES | FK → defense |
| mi_temps | varchar | YES | "MT1" ou "MT2" |

**Résultats clés :**
- 105 134 événements, **0 NULL sur TeamScore1/TeamScore2** — tous les scores sont renseignés
- ShootZone renseignée pour ~54 lignes seulement (0,05 % des events)
- Trigger est un mot réservé MySQL — requiert des backticks en SQL

## Schéma matchs

| Colonne | Type | Null |
|---|---|---|
| Id | int | NO |
| CompetitionId | int | YES |
| Date | datetime | YES |
| Team1Id | int | YES |
| Team2Id | int | YES |
| Team1Score | int | YES |
| Team2Score | int | YES |
| Year | year | YES |
| Day | varchar(32) | YES |
| Season | varchar(9) | NO |

**352 matchs**, 1 compétition (Championnat de France LFH), saisons 2024-2025 et 2025-2026.

## Schéma players

376 joueurs. 2 joueurs sans PositionId (NULL). Répartition : ARG 66, GB 64, PIV 59, ALD 51, ALG 51, DEMI 44, ARD 39.

## Positions disponibles

| Id | Code | Nom |
|---|---|---|
| 1 | DEMI | Demi-centre |
| 2 | ARG | Arrière gauche |
| 3 | ALD | Ailier droit |
| 4 | ALG | Ailier gauche |
| 5 | ARD | Arrière droit |
| 6 | PIV | Pivot |
| 7 | GB | Gardien de but |
| 8 | NSP | Ne Sait Pas |

## Événements (mapping API ↔ DB)

Le classifieur `StatEventClassifier` travaille sur `event.Name` (normalisé Unicode, sans accents, UPPER). Correspondance event.Id → event.Name confirmée ci-dessous.

| Id | Nom DB | Classifié comme |
|---|---|---|
| 1 | But | IsGoal |
| 2 | Gardien prend un but | IsGoalkeeperGoalConceded |
| 3 | Passe décisive | IsAssist |
| 5 | Gardien arrête le tir | IsGoalkeeperSave |
| 6 | Passage en force | IsPassageEnForce |
| 8 | Tir à côté | IsShotMiss |
| 9 | Perte de balle | IsBallLoss |
| 10 | Tir arrêté | IsShotMiss (saved shooter) |
| 11 | Interception | IsInterception |
| 12 | Marcher | IsTechnicalFault |
| 13 | Deux minutes | IsTwoMinutes |
| 14 | But sur pénalty | IsPenaltyGoal |
| 15 | Provoque une sanction | IsSanctionWon |
| 16 | Gardien prend le pénalty | IsGoalkeeperPenaltyConceded |
| 17 | Pénalty obtenu | IsPenaltyWon |
| 18 | Pénalty concédé | IsPenaltyConceded |
| 19 | Contre Réussi | IsCounterSuccess |
| 20 | Avertissement | IsWarning |
| 21 | Gardien arrête le pénalty | IsGoalkeeperPenaltySave |
| 22 | Gardien retour en jeu | (non classé) |
| 24 | Reprise de dribble | IsTechnicalFault |
| 26 | Tir contré | IsTirContre / IsShotMiss |
| 27 | Tir sur poteau | IsShotMiss |
| 28 | Zone | IsTechnicalFault |
| 30 | Mauvaise passe | IsBadPass |
| 32 | Pénalty sur poteau | IsPenaltyMiss |
| 33 | Pénalty arrêté | IsPenaltyMiss (saved shooter) |
| 34 | Pénalty raté | IsPenaltyMiss |

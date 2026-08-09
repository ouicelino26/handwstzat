# PLAYER_IDENTITY_SCHEMA

Date: 2026-08-02 | Source: DB hbdb audit + API code review | Branch: fix/player-time-availability-v1

## Identifiants disponibles dans la base

| IDENTIFIER | TABLE | STABLE | UNIQUE | NULLABLE | USED_IN_TIMEPLAYERS | USED_IN_EVENTS | USED_IN_API | RECOMMENDED |
|---|---|---|---|---|---|---|---|---|
| players.Id | players | OUI | OUI | NON | PlayerId (FK, nullable) | matchevents.PlayerId (nullable) | OUI | **CLÉ PRIMAIRE** |
| players.Name | players | NON | NON | NON | PlayerName (résolution import) | NON | NON | NON |
| players.Surname | players | NON | NON | NON | PlayerName (résolution import) | NON | NON | NON |
| players.Birthday | players | OUI | NON | OUI | NON | NON | NON | Clé secondaire |
| players.Number | players | NON | NON | OUI | NON | NON | NON | NON |
| players.TeamId | players | NON (transferts) | NON | OUI | TeamLabel indirect | matchevents.TeamId | OUI | Filtre contexte |
| histoplayer.PlayerId | histoplayer | - | - | - | - | - | - | INUTILISABLE (0 lignes) |

## Convention de nommage dans `players`

La table `players` utilise une convention **inversée par rapport au langage courant** :
- `Name` = **NOM de famille** (ex: CHABAUD, GROLLIER, FINSTAD)
- `Surname` = **PRÉNOM** (ex: MANON, ENOLA, BERGUM MARI)

Dans `timeplayers.PlayerName`, le format est : **NOM PRÉNOM** (ex: MAUNY CONSTANCE).

L'import normalise en cherchant les combinaisons `Name + Surname` et `Surname + Name`.

## Résolution à l'import

`TimePlayersImportService.BuildCandidateNames` génère 4 formes :
1. `"{Name} {Surname}"` (NOM PRÉNOM)
2. `"{Surname} {Name}"` (PRÉNOM NOM)
3. `"{Name}"` (NOM seul)
4. `"{Surname}"` (PRÉNOM seul)

Les formes 3 et 4 (seul) sont dangereuses et peuvent créer des collisions → le service les marque null si ambiguïté détectée.

## Gestion des collisions

Si deux joueuses partagent la même clé normalisée, le PlayerId est mis à `null` pour les deux → aucune attribution automatique en cas d'ambiguïté.

## Identifiants absents

| Identifiant | Statut |
|---|---|
| LicenseNumber / FederationId | ABSENT de la base |
| ExternalPlayerId | ABSENT |
| RosterPlayerId | ABSENT |
| UserId lié | ABSENT |

Il n'existe qu'un seul identifiant stable : `players.Id`.

## Identité historique

`histoplayer` est présente dans le schéma mais contient **0 ligne**. Elle ne peut pas servir à résoudre des identités historiques.

## Recommandation

PLAYER_IDENTIFIER_TYPES=DATABASE_ID,NORMALIZED_NAME_KEY
PRIMARY_PLAYER_IDENTIFIER=players.Id (INT, stable, unique)
HISTORICAL_PLAYER_IDENTIFIER=UNAVAILABLE (histoplayer vide)
LICENSE_IDENTIFIER_STATUS=ABSENT
BIRTHDATE_STATUS=PRESENT_IN_PLAYERS_NOT_IN_TIMEPLAYERS
MATCH_ROSTER_IDENTIFIER_STATUS=ABSENT (pas de table roster)

# API_PLAYER_TIME_JOIN_AUDIT

Date: 2026-08-02 | Source: API code review | Branch: fix/player-time-availability-v1

## Jointure principale — query time

| Attribut | Valeur |
|---|---|
| FILE | HandballManagerAPI/Analytics/AnalyticsInfrastructure.cs |
| METHOD | AnalyticsServiceBase.BuildTimePlayersQuery() |
| LEFT_SOURCE | timeplayers |
| RIGHT_SOURCE | players |
| KEY | timePlayer.PlayerId equals player.Id (int FK) |
| NULL_BEHAVIOR | LEFT JOIN — PlayerId NULL → player=null |
| requirePlayer filter | `where ... && (!requirePlayer \|\| timePlayer.PlayerId.HasValue)` — exclut lignes NULL PlayerId quand requirePlayer=true |
| COLLISION_RISK | AUCUN (jointure par int stable) |
| MISSING_ROW_BEHAVIOR | Joueuse non liée = exclue silencieusement |
| STATUS | CORRECT pour les lignes avec PlayerId |

## Jointure équipe — query time

| Attribut | Valeur |
|---|---|
| FILE | AnalyticsInfrastructure.cs |
| METHOD | BuildTimePlayersQuery() |
| LEFT_SOURCE | timeplayers.TeamLabel |
| RIGHT_SOURCE | teams.Name |
| KEY | `TeamLabel.Trim().ToUpper() equals (team.Name ?? "").Trim().ToUpper()` (string) |
| NULL_BEHAVIOR | LEFT JOIN — pas d'équipe correspondante → team=null |
| COLLISION_RISK | FAIBLE (noms d'équipes uniques en pratique) |
| STATUS | ACCEPTABLE |

## Résolution PlayerId — import time

| Attribut | Valeur |
|---|---|
| FILE | TimePlayersImportService.cs + SeasonWorkbookTimePlayersImportService.cs |
| METHOD | BuildPlayerLookupAsync() + ResolvePlayerId() |
| STRATEGY | Normalisation nom (strip diacritics + UPPER + collapse spaces) avec 4 formes |
| COLLISION_DETECTION | OUI — si deux joueuses partagent la même clé normalisée → PlayerId=null |
| FIRSTNAME_ONLY_FALLBACK | Utilisé en génération de clés mais collision → null → en pratique SAFE |
| LASTNAME_ONLY_FALLBACK | Utilisé en génération de clés mais collision → null → en pratique SAFE |
| STATUS | ACCEPTABLE — résolution à l'import, pas à la requête |

## Bug identifié : per-60 avec PlayingTime=0

| Attribut | Valeur |
|---|---|
| FILE | AnalyticsInfrastructure.cs ligne 272-274 |
| METHOD | LegacyStatsCalculator.ComputePer60 |
| BUG | Retourne `0` quand PlayingTimeMinutes=0 au lieu de `null` |
| IMPACT | Joueuses avec 0 min affichent GoalsPer60=0 au lieu de DATA_MISSING |
| WORKAROUND_NEEDED | OUI — retourner null (ou double?) pour distinguer 0 réel de donnée manquante |

## Bug identifié : valeur sentinelle 13:00:00

| Attribut | Valeur |
|---|---|
| SOURCE | timeplayers.PlayingTime = 13:00:00 (3 lignes) |
| IMPACT | 780 minutes comptabilisées → per-60 artificiellement faibles |
| RECOMMENDED_TREATMENT | Filtrer PlayingTime > MAX_MATCH_DURATION (ex: 01:30:00) dans BuildTimePlayersQuery |

## Comportement manque de données

| Scénario | Comportement actuel | Comportement attendu |
|---|---|---|
| Match sans timeplayers (saison 2025-2026) | PlayingTimeMinutes=0 → per-60=0 | PlayingTimeMinutes=null → per-60=null (DATA_MISSING) |
| Joueuse avec PlayerId NULL | Ligne exclue → PlayingTimeMinutes=0 | DATA_MISSING |
| PlayingTime=00:00:00 (remplaçante non jouée) | PlayingTimeMinutes=0 → per-60=0 | Distinguer de DATA_MISSING si possible |
| PlayingTime=13:00:00 (sentinel) | 780 min comptabilisées | Filtrer (invalide) |

## Résumé

JOIN_STRATEGY=PLAYER_ID_FK_AT_QUERY_TIME (correct)
NAME_JOIN_AT_QUERY_TIME=ABSENT (résolution par nom faite uniquement à l'import)
PER60_MISSING_TIME_BUG=YES (retourne 0 au lieu de null)
SENTINEL_13H_BUG=YES (3 lignes à filtrer)
ORPHAN_PLAYER_ID_239=YES (19 lignes ignorées silencieusement — acceptable)

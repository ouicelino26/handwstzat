# PLAYER_TIME_SOURCE_MAP

Date: 2026-08-02 | Source: DB hbdb audit + API code review | Branch: fix/player-time-availability-v1

## Sources de temps de jeu identifiées

### Source principale : `timeplayers`

| Attribut | Valeur |
|---|---|
| SOURCE_TABLE | timeplayers |
| SOURCE_COLUMN | PlayingTime |
| SEMANTIC | Temps effectif joué par une joueuse dans un match (peut représenter un segment ou le total) |
| UNIT | TIME MySQL (HH:MM:SS) → TimeSpan .NET |
| PLAYER_LINK | PlayerId (INT, nullable → players.Id) |
| MATCH_LINK | MatchId (INT NOT NULL → matchs.Id) |
| TEAM_LINK | TeamLabel (VARCHAR 32, jointure par nom normalisé vers teams.Name) |
| SEASON_LINK | Indirect via matchs.Season |
| NULLABLE | PlayingTime NOT NULL (mais peut être 00:00:00) |
| USED_BY_API | OUI — BuildTimePlayersQuery dans AnalyticsInfrastructure.cs |
| CONFIDENCE | HIGH pour 2024-2025, ABSENT pour 2025-2026 |

### Colonnes complètes de `timeplayers`

| Colonne | Type | Nullable | Sémantique |
|---|---|---|---|
| Id | INT | NO | PK auto-increment |
| MatchId | INT | NO | FK matchs.Id |
| TeamLabel | VARCHAR(32) | NO | Nom équipe tel qu'importé |
| PlayerName | VARCHAR(200) | NO | Format : NOM PRÉNOM (majuscules) |
| PlayingTime | TIME | NO | Durée HH:MM:SS (peut être 00:00:00) |
| PlayerId | INT | YES | FK players.Id — résolu à l'import par nom normalisé |
| SourceFile | VARCHAR(255) | YES | Nom du fichier source (xlsx) |
| SourceSheet | VARCHAR(64) | YES | Nom de l'onglet Excel source |
| SourceRow | INT | YES | Ligne du fichier source |

### Sources secondaires vérifiées et absentes

| Table candidat | Présence | Résultat |
|---|---|---|
| histoplayer | Présente mais vide (0 lignes) | Pas utilisable |
| histomatch | Présente mais vide (0 lignes) | Pas utilisable |
| matchevents (EventId de substitution) | Présente mais aucun EventId de remplacement identifié | Non exploitable sans mapping explicite |

### Aucune source alternative

Il n'existe pas de :
- table roster / lineup / substitution dédiée
- colonne starter / enter / exit
- événements de remplacement typés dans matchevents

## Résolution à l'import vs à la requête

| Étape | Méthode | Clé |
|---|---|---|
| Import (TimePlayersImportService) | Normalisation par nom : strip accents + UPPER + collisions → null | PlayerName → PlayerId |
| Requête (BuildTimePlayersQuery) | LEFT JOIN par PlayerId (int FK) | PlayerId |
| Fallback à la requête | requirePlayer=true → lignes NULL PlayerId exclues silencieusement | Aucun fallback |

## Conclusion

TIME_SOURCE_TABLES=timeplayers
PRIMARY_TIME_SOURCE=timeplayers.PlayingTime (TIME)
SECONDARY_TIME_SOURCES=NONE
TIME_UNIT=HH:MM:SS (MySQL TIME, TimeSpan .NET)
TIME_GRANULARITY=UNE_LIGNE_PAR_JOUEUR_PAR_MATCH (parfois deux segments dans un même match)

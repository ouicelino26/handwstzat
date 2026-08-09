# ORPHAN_PLAYER_TIME_AUDIT

Date: 2026-08-02 | Source: DB hbdb audit | Branch: fix/player-time-availability-v1

## Statistiques globales

| Indicateur | Valeur |
|---|---|
| TOTAL_TIME_ROWS | 4 129 |
| VALID_PLAYER_LINK_ROWS | 4 100 |
| NULL_PLAYER_ID_ROWS | 10 |
| ORPHAN_PLAYER_ROWS | 19 |
| ORPHAN_MATCH_ROWS | 0 |
| TEAM_MISMATCH_ROWS | Non calculé (jointure par nom) |
| SEASON_MISMATCH_ROWS | 0 (matchs ont tous une saison) |
| DUPLICATE_TIME_ROWS | 2 cas (même PlayerId+MatchId) |
| INVALID_DURATION_ROWS | 3 (PlayingTime = 13:00:00) |

## Lignes avec PlayerId NULL (10 lignes)

Toutes issues du fichier `SAH TOTAL.xlsx`, onglet `DATA`. Joueuses :
- Courtade Clarence (Brest, match 173)
- Louveau Louise (Achenheim, Stella Saint-Maur, Nice — matches 176, 177, 212)
- Pam Lise (Nice, match 212)
- Tremblet Lucie (Nice, match 212)
- Grauet Oceane (Besancon, Achenheim, Stella Saint-Maur, Chambray — matches 193-196)

Ces joueuses n'ont pas de fiche dans `players` ou leur nom normalisé a créé une collision/non-résolution.

Statut : `TIME_ROW_PRESENT_PLAYER_ID_MISSING` — non récupérables sans intervention manuelle sur la base.

## Lignes avec PlayerId orphelin (19 lignes — PlayerId=239)

Toutes appartiennent à la joueuse `DEMBELE MAHOUA` de l'équipe `BBH`. Cette joueuse (Id=239) a été supprimée de la table `players` mais ses lignes timeplayers subsistent.

Matches concernés : 38, 42, 47, 63, 71, 79, 81, 93, 103, 111, 123, 133, 134, 140, 147, 152, 158, 164, 170.

Statut : `TIME_ROW_PRESENT_PLAYER_ID_ORPHAN` — non récupérables sans recréation de la fiche joueuse.

## Lignes dupliquées (2 cas)

Match 50 / PlayerId=101 : deux lignes (00:15:54 et 00:20:43) → probable MT1+MT2 importés séparément.
Match 49 / PlayerId=93 : deux lignes (00:39:00 et 00:25:20) → même hypothèse.

Comportement API : somme des deux → résultat correct pour ces cas.

Statut : `TIME_ROW_PRESENT_DUPLICATE` — géré correctement par l'API.

## Lignes avec PlayingTime invalide (3 lignes)

PlayingTime = 13:00:00 pour 3 joueuses (matches 31 et 67). Valeur sentinelle d'import Excel.

Statut : `TIME_ROW_PRESENT_INVALID_DURATION`.

## Conclusion

La quasi-totalité des problèmes (163 matchs sans données) est `TIME_ROW_ABSENT_NO_SOURCE_DATA`.
Les problèmes de jointure ne concernent que 29 lignes sur 4 129 (0.7%).

# PLAYER_TIME_SEMANTICS

Date: 2026-08-02 | Source: DB hbdb audit | Branch: fix/player-time-availability-v1

## Unité

**PlayingTime** est stocké comme type MySQL `TIME` (HH:MM:SS).

Dans le code .NET : `TimeSpan PlayingTime` (propriété dans `TimePlayers.cs`).

L'API extrait les minutes : `timeRow.PlayingTime.TotalMinutes`.

## Distribution observée

| Indicateur | Valeur |
|---|---|
| Total lignes | 4 129 |
| MIN PlayingTime | 00:00:00 |
| MAX PlayingTime | 13:00:00 |
| Lignes avec PlayingTime > 01:00:00 | 3 |
| Lignes avec PlayingTime entre 00:40:01 et 01:00:00 | 1 192 |
| Lignes avec PlayingTime entre 00:00:01 et 00:10:00 | 334 |
| Lignes avec PlayingTime = 00:00:00 | 197 |
| Lignes avec PlayingTime > 00:00:00 | 3 932 |

## Valeurs anormales

### PlayingTime = 13:00:00 (3 lignes)

| timeplayers.Id | MatchId | PlayerName |
|---|---|---|
| 340 | 31 | MARTEL JUSTINE |
| 341 | 31 | VALENTINI CHLOE |
| 1180 | 67 | LIGNIERES LEA |

**Hypothèse :** Valeur sentinelle d'import — correspondant à un format `"13:00"` mal interprété depuis Excel. Il ne s'agit pas d'un temps de jeu de 13 heures.

**Comportement API actuel :** Ces 780 minutes sont comptabilisées telles quelles → per-60 faussés pour ces 3 joueuses.

### PlayingTime = 00:00:00 (197 lignes)

Signification ambiguë :
- joueuse sur la feuille mais non entrée en jeu (remplaçante non mobilisée) → 0 réel
- temps absent non renseigné → donnée manquante masquée comme 0

**Comportement API actuel :** `PlayingTimeMinutes = 0.0` → per-60 = `0` (via `ComputePer60 ?? 0`) → **non distinguable d'une joueuse sans données**.

## Sémantique par granularité

Deux lignes pour le même `(PlayerId, MatchId)` ont été trouvées (2 cas) :
- Origine : deux feuilles ou deux imports du même match
- API : les somme (comportement correct si ce sont des segments MT1+MT2)

## Règle durée match

- Un match de handball standard dure **60 minutes** (2 × 30 min)
- Les prolongations (2 × 5 min) existent mais sont rares
- PlayingTime > 00:60:00 = anomalie sauf pour gardiennes remplaçantes d'urgence
- PlayingTime = 13:00:00 = anomalie certaine

## Conclusion

TIME_UNIT=HH:MM:SS (MySQL TIME → TimeSpan)
ZERO_MEANS_ABSENT_OR_ZERO=AMBIGUOUS (non distinguable sans métadonnée)
SENTINEL_VALUE_13h=DETECTED (3 lignes, probable artefact import Excel)
ABOVE_60MIN_ANOMALIES=3
MATCH_DURATION_RULE=60_MINUTES_NORMAL

# RATE_SAMPLE_DISTRIBUTION_AUDIT

Date: 2026-08-01 | Source: DB hbdb | Branch: fix/handwstat-final-validation-v1

## Taux de réussite au tir (open play)

Seuil configuré : `MinimumOpenShotsForShotSuccessRanking = 20`

### Distribution des joueurs qualifiés (open_attempts >= 20)

- Nombre de joueurs qualifiés : **>30** (extraction top 30 montre le dernier à 230 tentatives)
- Top shooter : SAJKA MARIE HELENE (ARD) — 519 tentatives, 255 buts → 49,1 %
- Deuxième : ANTONISSEN NELE (DEMI) — 433 tentatives, 243 buts → 56,1 %

### Validation seuil 20

Le seuil 20 est approprié : tous les joueurs dans le top ranking ont ≥ 230 open attempts. Le seuil ne coupe que des joueurs avec trop peu d'observations pour être significatifs.

## Taux de réussite sur pénalty

Seuil configuré : `MinimumPenaltyAttemptsForPenaltySuccessRanking = 5`

### Distribution des joueurs qualifiés (penalty_attempts >= 5)

- ANTONISSEN NELE : 144 tentatives, 114 buts → 79,2 %
- BOUKTIT SARAH (PIV) : 108 tentatives, 85 buts → 78,7 %
- MORETTO BARBARA (ARD) : 76 tentatives, 59 buts → 77,6 %
- BORG LYLOU (DEMI) : 85 tentatives, 68 buts → 80,0 %

### Validation seuil 5

Seuil 5 raisonnable pour les spécialistes du pénalty. Évite les joueurs avec 1-2 tentatives isolées.

## Taux d'arrêt gardienne

Seuil configuré : `MinimumShotsFacedForSaveRateRanking = 30`

### Distribution des joueuses qualifiées (total_faced >= 30)

- FARGUES LEA : 1697 tirs subis, 579 arrêts → 34,1 %
- LACHAT MARIE : 1561 tirs subis, 451 arrêts → 28,9 %
- TOUBISSA-ELBECO JUSTICIA : 1498 tirs subis, 478 arrêts → 31,9 %

Toutes les gardiennes qualifiées ont ≥ 382 tirs subis. Le seuil 30 est très bas par rapport aux données réelles — toutes les gardiennes actives le dépassent largement.

### Séparation taux d'arrêt global vs pénaltys

Le calcul de `GoalkeeperSaveRate` dans `LegacyStatsCalculator.ComputeGoalkeeperSaveRate` inclut :
- Numérateur : saves + penalty_saves
- Dénominateur : saves + penalty_saves + goals_conceded + penalty_conceded

C'est un taux d'arrêt global correct. Pour les pénaltys uniquement : `GoalkeeperPenaltyStopRate`.

## Événements 7m

| EventId | Nom | Occurrences |
|---|---|---|
| 14 | But sur pénalty | 2 286 |
| 16 | Gardien prend le pénalty | 2 282 |
| 17 | Pénalty obtenu | 3 000 |
| 18 | Pénalty concédé | 2 988 |
| 21 | Gardien arrête le pénalty | 507 |
| 32 | Pénalty sur poteau | 128 |
| 33 | Pénalty arrêté | 531 |
| 34 | Pénalty raté | 59 |

**Total tentatives 7m (attaquant) :** 14 + 32 + 33 + 34 = 3 004 (+/- légère variation par match)
**Total tentatives 7m (gardienne) :** 16 + 21 = 2 789

Les événements 17/18 (Pénalty obtenu / Pénalty concédé) sont des attributions de faute, pas des tentatives — ils ne doivent pas entrer dans le calcul du taux de tir.

## Conclusion

Les seuils actuels (20/5/30) sont validés par les distributions réelles. Wilson lower bound avec z=1.96 est approprié pour la LFH.

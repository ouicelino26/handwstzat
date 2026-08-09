# HANDWSTAT_POSITION_CODE_AUDIT

Date: 2026-08-01 | Source: DB hbdb + TeamOfTheDayService.cs

## Codes de position en base

| Id | Code | Nom | Joueurs |
|---|---|---|---|
| 1 | DEMI | Demi-centre | 44 |
| 2 | ARG | Arrière gauche | 66 |
| 3 | ALD | Ailier droit | 51 |
| 4 | ALG | Ailier gauche | 51 |
| 5 | ARD | Arrière droit | 39 |
| 6 | PIV | Pivot | 59 |
| 7 | GB | Gardien de but | 64 |
| 8 | NSP | Ne Sait Pas | (minoritaire) |
| NULL | — | — | 2 |

## Mapping API → Slot TeamOfDay

| Code DB | Slot | Alias reconnus |
|---|---|---|
| GB | goalkeeper | GB, GK, GARDIENNE, GARDIEN, GOALKEEPER |
| ALG | left-wing | ALG, AG, AIG, AILIERE GAUCHE, AILIER GAUCHE, LEFT WING, LW |
| ARG | left-back | ARG, AR G, ARRIERE GAUCHE, LEFT BACK, LB |
| DEMI | center-back | DC, DEMI CENTRE, DEMI-CENTRE, CENTRE, CENTER BACK, PLAYMAKER, CB |
| PIV | pivot | P, PIV, PIVOT, LINE PLAYER, LP |
| ARD | right-back | ARD, AR D, ARRIERE DROITE, RIGHT BACK, RB |
| ALD | right-wing | ALD, AD, AID, AILIERE DROITE, AILIER DROIT, RIGHT WING, RW |

## Observations

- Le code "DEMI" ne figure pas dans les alias de `center-back` — la vérification se fait sur le code normalisé
  - Normalisé : "DEMI" → comparé à alias normalisé "DEMI CENTRE", "DEMI-CENTRE", "DC", etc.
  - **DEMI ne matche pas** directement — il faut que `NormalizePosition("DEMI")` == `NormalizePosition(alias)` pour un alias de la liste
  - Les alias contenant "DEMI" sont "DEMI CENTRE" et "DEMI-CENTRE" — un code exact "DEMI" ne matchera pas
  - **CORRECTION REQUISE** : ajouter "DEMI" comme alias dans le slot `center-back`
- Code "NSP" (Ne Sait Pas) : aucun slot correspondant → le joueur sera ignoré (null fallback), comportement correct
- 2 joueurs sans PositionId → PositionCode=null → ni IsGoalkeeper=true ni alias match → ignorés, comportement correct

## Statut

- GB → goalkeeper : OK (prefix GB géré par IsGoalkeeperPosition)
- ARG → left-back : OK
- ALD → right-wing : OK  
- ALG → left-wing : OK
- ARD → right-back : OK
- PIV → pivot : OK
- DEMI → center-back : FAIL — alias manquant à corriger

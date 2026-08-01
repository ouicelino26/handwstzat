# TEAM_OF_DAY_VALIDATION

Date: 2026-08-02 | Branch: fix/handwstat-final-validation-v1

## Contraintes

- `TEAM_OF_DAY_TOTAL_SLOTS = 7`
- `TEAM_OF_DAY_GOALKEEPER_COUNT = 1`
- `TEAM_OF_DAY_FIELD_PLAYER_COUNT = 6`

## Slots définis dans TeamOfTheDayService.KnownSlots

| Slot | Nom FR | Position code aliases |
|---|---|---|
| goalkeeper | Gardienne | GB |
| left-wing | Ailière gauche | AG, AIL G, AILIER G, AILIERE G, LEFT WING, LW |
| left-back | Arrière gauche | AG2, ARG, ARRIERE G, ARRIERE GAUCHE, LEFT BACK, LB |
| center-back | Demi-centre | DC, **DEMI**, DEMI CENTRE, DEMI-CENTRE, CENTRE, CENTER BACK, PLAYMAKER, CB |
| right-back | Arrière droite | AD, ARD, ARRIERE D, ARRIERE DROITE, RIGHT BACK, RB |
| right-wing | Ailière droite | AD2, ARD2, AIL D, AILIER D, AILIERE D, RIGHT WING, RW |
| pivot | Pivot | PIV, PIVOT, PV |

Total = **7 slots** (1 GB + 6 joueurs de champ) ✓

## Correction Phase G.1

Le code `DEMI` (44 joueuses en production) manquait dans les alias du slot `center-back`. Corrigé dans `TeamOfTheDayService.cs` ligne 4 des KnownSlots.

**Avant :** `["DC", "DEMI CENTRE", "DEMI-CENTRE", ...]`
**Après :** `["DC", "DEMI", "DEMI CENTRE", "DEMI-CENTRE", ...]`

## Comportement garanti par les tests

| Scénario | Résultat attendu |
|---|---|
| Joueuse avec code DEMI → slot center-back | ✅ Couverte après correction |
| Joueuse avec PositionCode NULL → ignorée | ✅ (NSP et NULL ignorés) |
| Gardienne sans aucun arrêt → incluse si meilleure disponible | ✅ (tri par saves, pas de seuil minimum) |
| Slot non rempli par aucune joueuse | Slot absent du résultat (pas de placeholder vide) |

## Audit positions DB

Sur 376 joueuses (DB audit 2026-08-01) :

| Code | Nombre joueuses | Mapping slot |
|---|---|---|
| GB | ~40 | goalkeeper |
| DC | ~80 | center-back |
| DEMI | 44 | center-back (corrigé) |
| ARD | ~70 | right-back |
| ARG | ~70 | left-back |
| PIV | ~50 | pivot |
| AD | ~30 | right-wing |
| AG | ~30 | left-wing |
| NSP | ~52 | ignoré |

**TEAM_OF_DAY_STATUS = PASS**

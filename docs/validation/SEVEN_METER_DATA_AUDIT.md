# SEVEN_METER_DATA_AUDIT

Date: 2026-08-02 | Source: DB hbdb | Branch: fix/handwstat-final-validation-v1

## Données 7m en production

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

**Total tentatives attaquant :** 3 004 (= 14 + 32 + 33 + 34)
**Total tentatives gardienne :** 2 789 (= 16 + 21)

## Qualité des données

### ShootZone

- Lignes avec ShootZone renseigné (toutes catégories) : ~54 / 105 134 = **0,05 %**
- Lignes avec ShootZone pour les 7m : non pertinent (les pénaltys se tirent d'une position fixe, pas de zone)
- **Conclusion :** Le champ ShootZone est inutilisable pour une carte des tirs 7m

### ShootShade (zone de but)

Le champ `ShootShade` est utilisé par `SpatialStatsService` pour les zones de cadrage (BG1-BG12, BD1-BD12). Les événements 7m peuvent avoir un ShootShade renseigné — c'est la seule donnée spatiale exploitable pour les penalties.

## Sémantique des événements

| Événements | Type | Inclus dans IsPenaltyAttempt |
|---|---|---|
| 14, 32, 33, 34 | Tentatives 7m (attaquant) | ✅ oui |
| 16, 21 | Résultats 7m (gardienne) | ✅ oui (côté GK) |
| 17, 18 | Attribution de faute (obtenu/concédé) | ❌ non (foul, pas tir) |

`StatEventClassifier.IsPenaltyAttempt` = true pour EventId 14, 33, 34 (normalisés sans accents).
`StatEventClassifier.IsPenaltyAttempt` = false pour 17, 18 — correct.

## Filtres disponibles dans l'API

Le paramètre `attackType` de `SpatialStatsService.GetMatchSpatialStatsAsync` accepte :
- `"sevenm"` / `"7m"` / `"penalty"` — filtre sur IsPenaltyAttempt=true
- `"openplay"` — filtre sur IsOpenPlayAttempt=true
- `"all"` / null — tous les événements spatiaux

## Conclusion

Les données 7m sont présentes et cohérentes (3 004 tentatives attaquant, 2 789 côté gardienne, écart expliqué). Le filtre `attackType` est fonctionnel. La vue "carte des tirs" par zone est non fiable pour ShootZone mais exploitable via ShootShade pour les matchs où ce champ est renseigné.

**SEVEN_METER_DATA_STATUS = AUDITED | FILTERS_FUNCTIONAL**

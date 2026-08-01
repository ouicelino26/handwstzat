# MATCH_ROSTER_TIME_RECONCILIATION

Date: 2026-08-02 | Source: DB hbdb audit | Branch: fix/player-time-availability-v1

## Méthode

Comparaison pour un échantillon de matchs entre :
1. Joueuses ayant des événements dans matchevents
2. Joueuses ayant une ligne timeplayers
3. Couverture PlayerId

## Échantillon match 16 (saison 2024-2025, CHAMBRAY)

| PlayerName timeplayers | PlayerId | PlayingTime |
|---|---|---|
| MAUNY CONSTANCE | 201 | 00:47:08 |
| ABDOU DAWIYA | 208 | 00:36:14 |
| MATTHIJS HOLMBERG VILMA | 210 | 00:27:29 |
| MODENEL LUCIE | 203 | 00:27:01 |
| VAN DER HEIJDEN LAURA | 204 | 00:15:33 |
| PULERI LAURIE | 206 | 00:28:23 |
| MORVAN YAELLE | 211 | 00:40:26 |
| SOW AMINATA | 207 | 00:32:22 |
| SYLLA DYENABA | 205 | 00:25:33 |
| STOILJKOVIC JOVANA | 214 | 00:33:54 |

Toutes les 10 joueuses ont un PlayerId résolu. Couverture 100% pour ce match.

## Saison 2024-2025 (générale)

- 181 matchs avec timeplayers sur 182 → couverture 99.5%
- Le 1 match sans timeplayers est probablement un oubli d'import isolé

## Saison 2025-2026 (générale)

- 7 matchs avec timeplayers sur 170 → couverture 4.1%
- Les 7 matchs couverts sont un sous-ensemble (probablement les premiers matchs de la saison ou un import partiel)
- 163 matchs ont des événements matchevents (statistiques de match) mais **aucune donnée timeplayers**

## Conclusion sur la réconciliation roster/temps

Pour les matchs 2024-2025 importés (99.5%), la réconciliation est correcte :
- PlayerId résolu pour 99.76% des lignes
- 0 collision d'identité confirmée
- 2 cas de doublons (MT1+MT2 séparés) gérés par somme

Pour les matchs 2025-2026 non importés :
- Aucune donnée timeplayers disponible
- La réconciliation n'est pas possible sans import des fichiers source

MATCH_ROSTER_TIME_RECONCILIATION_STATUS=PASS_2024_2025 | ABSENT_2025_2026
ROSTER_COVERAGE_2024_2025=99.5%
ROSTER_COVERAGE_2025_2026=4.1%

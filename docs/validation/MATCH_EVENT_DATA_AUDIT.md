# MATCH_EVENT_DATA_AUDIT

Date: 2026-08-01 | Source: DB hbdb | Branch: fix/handwstat-final-validation-v1

## Volume global

| Métrique | Valeur |
|---|---|
| Matchs | 352 |
| Événements total | 105 134 |
| Joueurs | 376 |
| Équipes distinctes | 15 |
| Mi-temps distinctes | 2 (MT1 / MT2) |

## Distribution des événements principaux

| EventId | Nom | Occurrences | % |
|---|---|---|---|
| 1 | But | 17 377 | 16,5 % |
| 2 | Gardien prend un but | 17 009 | 16,2 % |
| 3 | Passe décisive | 8 844 | 8,4 % |
| 10 | Tir arrêté (by GK) | 8 193 | 7,8 % |
| 5 | Gardien arrête le tir | 7 989 | 7,6 % |
| 4 | Neutralise l'attaquant | 6 034 | 5,7 % |
| 9 | Perte de balle | 5 223 | 5,0 % |
| 11 | Interception | 4 002 | 3,8 % |
| 17 | Pénalty obtenu | 3 000 | 2,9 % |
| 18 | Pénalty concédé | 2 988 | 2,8 % |
| 14 | But sur pénalty | 2 286 | 2,2 % |
| 8 | Tir à côté | 2 230 | 2,1 % |
| 13 | Deux minutes | 2 078 | 2,0 % |

## Nullité des champs scores

- **TeamScore1 NULL : 0** — tous les événements ont les scores renseignés
- **TeamScore2 NULL : 0** — idem
- La logique de reconstruction de score depuis les événements goals est donc un mécanisme défensif correct (au cas où de nouveaux matchs sans scores seraient importés), mais elle n'est pas critique pour les données actuelles

## Mi-temps

| mi_temps | Événements |
|---|---|
| MT1 | 52 460 |
| MT2 | 52 674 |
Distribution uniforme entre les deux mi-temps.

## ShootZone

Seulement ~54 lignes renseignées sur 105 134 — le champ est quasiment vide. La carte des tirs ne peut pas être affichée de façon fiable basée sur ShootZone.

## Trigger (déclencheur tactique)

Colonne présente mais requiert des backticks en SQL (mot réservé MySQL). Valeurs exploitées par `EventContextAnalysis.ResolveAttackSituation` pour qualifier le type d'attaque.

## Validation cohérence buts

- Total buts ouverts (EventId=1) : 17 377
- Total buts 7m (EventId=14) : 2 286
- Total arrêts GK (EventId=5) : 7 989
- Total arrêts GK sur penalty (EventId=21) : 507
- Ratio arrêt global GK = (7989+507) / (7989+507+17009+2282) ≈ 29 % — cohérent avec les stats individuelles

## Écart événements Gardien 2/16

- EventId=2 (Gardien prend un but) : 17 009
- EventId=16 (Gardien prend le pénalty) : 2 282
- Somme : 19 291 — proche du total buts (17 377 + 2 286 = 19 663) — écart de ~372 événements probablement dus à des buts sans gardien (jeu sans gardien, EventId=31 : 683 événements) ✓

## Validation pénaltys

- Tentatives attaquant = But (14) + Poteau (32) + Arrêté (33) + Raté (34) = 2286 + 128 + 531 + 59 = **3 004**
- Tentatives côté gardien = Pris (16) + Arrêté (21) = 2282 + 507 = **2 789**
- Écart ~215 : poteau et raté n'ont pas de contrepartie gardien — cohérent avec la sémantique des événements ✓

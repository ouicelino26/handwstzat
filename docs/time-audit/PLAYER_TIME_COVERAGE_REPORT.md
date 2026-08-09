# PLAYER_TIME_COVERAGE_REPORT

Date: 2026-08-02 | Source: DB hbdb audit | Branch: fix/player-time-availability-v1

## Couverture globale

| Indicateur | Valeur |
|---|---|
| Total matchs | 352 |
| Matchs avec timeplayers | 188 |
| Matchs sans timeplayers | 164 |
| Couverture matchs | 53.4% |
| Total joueurs (players) | 376 |
| Joueurs ayant au moins une ligne timeplayers | 261 |
| Joueurs sans aucune ligne timeplayers | 115 |
| Lignes timeplayers total | 4 129 |
| Lignes avec PlayerId renseigné | 4 119 (99.76%) |
| Lignes avec PlayerId NULL | 10 (0.24%) |
| Lignes avec PlayerId orphelin (joueur supprimé) | 19 (0.46%) |
| Lignes avec PlayingTime = 00:00:00 | 197 (4.77%) |
| Lignes avec PlayingTime > 00:00:00 | 3 932 (95.23%) |

## Couverture par saison

| Saison | Total matchs | Matchs avec temps | Matchs sans temps | Couverture |
|---|---|---|---|---|
| 2024-2025 | 182 | 181 | 1 | **99.5%** |
| 2025-2026 | 170 | 7 | 163 | **4.1%** |

## Diagnostic principal

**La quasi-totalité des matchs 2025-2026 n'ont pas de données timeplayers importées.**

- 163 matchs 2025-2026 ont des événements matchevents mais 0 ligne timeplayers
- Ce n'est pas un bug de code : c'est une **absence de données à la source (fichiers xlsx non importés)**
- L'API ne peut pas produire de temps de jeu pour ces matchs par conception

## Couverture par catégorie de ligne

| Catégorie | Lignes | % |
|---|---|---|
| PlayerId valide + PlayingTime > 0 | ~3 800 | ~92% |
| PlayerId valide + PlayingTime = 0 | ~197 | ~5% |
| PlayerId NULL | 10 | 0.24% |
| PlayerId orphelin (joueur supprimé Id=239) | 19 | 0.46% |
| PlayingTime = 13:00:00 (sentinel) | 3 | 0.07% |

## Cas particuliers détectés

### Valeur sentinelle 13:00:00

3 lignes (matches 31 et 67) ont PlayingTime=13:00:00. Probable artefact d'import Excel où "13:00" a été mal interprété. Ces lignes **gonflent artificiellement** les statistiques per-60 des joueuses concernées.

### Joueuse supprimée (PlayerId=239)

19 lignes dans timeplayers référencent PlayerId=239 (DEMBELE MAHOUA, BBH) qui n'existe plus dans la table players. Ces lignes sont **ignorées silencieusement** par l'API (`requirePlayer=true`).

### Doublons (MatchId, PlayerId)

2 cas de double ligne par (MatchId, PlayerId) — la joueuse `SEMEDO-MONTEIRO WENDY` (Id=101) apparaît deux fois pour le match 50, avec deux durées différentes (00:15:54 et 00:20:43). L'API les somme → comportement correct si ce sont des segments MT1+MT2.

## Conclusion

La couverture globale est bonne pour 2024-2025 (99.5%) et quasi-nulle pour 2025-2026 (4.1%).

Le problème signalé **"de nombreuses joueuses n'ont pas de temps de jeu"** s'explique presque entièrement par l'absence d'import des données 2025-2026 — et non par un bug de jointure.

TIME_COVERAGE_PERCENT=53.4% (global) / 99.5% (2024-2025) / 4.1% (2025-2026)
PLAYERS_WITH_TIME_ROW=261 / 376
PLAYERS_WITHOUT_TIME_ROW=115 / 376
ROOT_CAUSE_PRIMARY=TIME_ROW_ABSENT_NO_SOURCE_DATA (saison 2025-2026 non importée)

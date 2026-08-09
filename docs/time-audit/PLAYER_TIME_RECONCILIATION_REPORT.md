# PLAYER_TIME_RECONCILIATION_REPORT

Date: 2026-08-02 | Source: DB hbdb audit + API code review + tests G.2 | Branch: fix/player-time-availability-v1

## Objectif

Réconcilier les données de temps de jeu entre la base de données, l'API et l'interface HandWStat, et valider que les corrections apportées (filtre sentinel 13h, garde UI `MatchesWithPlayingTime`) sont cohérentes à tous les niveaux.

## Réconciliation par couche

### Couche 1 : Base de données (timeplayers)

| Catégorie | Lignes | Validité |
|---|---|---|
| Lignes valides avec PlayerId résolu | ~4 067 | VALIDE |
| Lignes avec PlayerId NULL (SAH TOTAL) | 10 | NON_RESOLU |
| Lignes avec PlayerId orphelin (239 supprimé) | 19 | NON_RESOLU |
| Lignes avec PlayingTime > 01:30:00 (sentinel 13h) | 3 | FILTRE_ACTIF |
| Lignes en doublon (même PlayerId+MatchId) | 4 (2 cas) | ACCUMULE |
| Saison 2025-2026 — lignes absentes | ~3 260 joueuses-matchs | DONNEES_ABSENTES |

**Total résolvable actuellement :** ~4 067 lignes (98.5% des lignes existantes)

### Couche 2 : API (BuildTimePlayersQuery)

| Comportement | Avant correction | Après correction |
|---|---|---|
| Filtre sentinel PlayingTime > 01:30:00 | ABSENT | ACTIF (`<= new TimeSpan(1, 30, 0)`) |
| Jointure PlayerId FK | CORRECT | CORRECT |
| `requirePlayer=true` (défaut) | Exclut les NULL | Exclut les NULL |
| Per-60 avec minutes=0 | Retourne 0 (faux 0) | Retourne 0 (inchangé — guard en UI) |

### Couche 3 : DTO (PlayerGlobalStatsDto)

| Champ | Disponibilité | Usage recommandé |
|---|---|---|
| `MatchesWithPlayingTime` | PRÉSENT (int) | Guard d'affichage — 0 = DATA_MISSING |
| `PlayingTimeMinutes` | PRÉSENT (double) | Valeur brute — 0 peut être DATA_MISSING |
| `AveragePlayingTimePerMatchMinutes` | PRÉSENT (double) | Divise sur MatchesWithPlayingTime |
| `GoalsPer60` | PRÉSENT (double) | 0 si DATA_MISSING — masquer si MatchesWithPlayingTime=0 |

### Couche 4 : UI HandWStat (Players.razor)

| Élément | Avant correction | Après correction |
|---|---|---|
| "Temps / match" mini-card | Affichait "0 min" | Affiche "Non disponible" si MatchesWithPlayingTime=0 |
| PlayerTeamHistoryPanel minutes | Affichait "0 min" | Inchangé (DTO sans MatchesWithPlayingTime) |

## Vérifications effectuées (tests G.2)

| Test | Résultat |
|---|---|
| DirectPlayerId_ReturnsRecordedTime | PASS |
| PlayingTime_UnitConversion_IsCorrect (60 min = 60.0) | PASS |
| PlayingTime_IsCappedAtMatchDuration (13h filtré) | PASS |
| MissingTime_DisplaysUnavailableNotZero | PASS |
| Per60_WithMissingTime_ReturnsZero (comportement documenté) | PASS |
| GoalkeeperWithZeroSaves_StillReceivesRecordedTime | PASS |
| TransferredPlayer_HistoricalIdentityResolvesCorrectly | PASS |

## Gaps non réconciliables (hors scope)

| Gap | Raison | Action requise |
|---|---|---|
| Saison 2025-2026 absente | Données source non importées | Import manuel xlsx via SeasonWorkbookTimePlayersImportService |
| PlayerId NULL (10 lignes) | Ambiguïté au niveau du fichier source SAH | Correction manuelle du fichier xlsx d'import |
| PlayerId orphelin 239 | Joueur supprimé (DEMBELE MAHOUA) | Vérifier si réimport nécessaire |
| Doublons (4 lignes) | Deux fichiers importés pour le même match | Déduplication manuelle ou règle d'import |

## Conclusion

RECONCILIATION_STATUS=PARTIAL
LAYERS_RECONCILED=3 (DB filtre, API filtre, UI guard)
LAYERS_NOT_RECONCILED=1 (PlayerTeamHistoryPanel — DTO sans MatchesWithPlayingTime)
DATA_COVERAGE_2024_2025=99.5%
DATA_COVERAGE_2025_2026=4.1%
SENTINEL_FILTER_ACTIVE=YES
UI_GUARD_ACTIVE=YES
PER60_GUARD_ACTIVE=UI_ONLY (DTO retourne 0, UI masque)

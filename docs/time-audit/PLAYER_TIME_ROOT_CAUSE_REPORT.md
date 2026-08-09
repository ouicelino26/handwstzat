# PLAYER_TIME_ROOT_CAUSE_REPORT

Date: 2026-08-02 | Source: DB hbdb audit + API code review | Branch: fix/player-time-availability-v1

## Classification des causes

| Cause | Lignes concernées | % | Description |
|---|---|---|---|
| TIME_ROW_ABSENT_NO_SOURCE_DATA | ~163 matchs × ~20 joueuses ≈ 3 260 joueuses-matchs | **~95%** | Saison 2025-2026 non importée |
| TIME_ROW_PRESENT_PLAYER_ID_ORPHAN | 19 | 0.46% | PlayerId=239 supprimé (DEMBELE MAHOUA) |
| TIME_ROW_PRESENT_PLAYER_ID_MISSING | 10 | 0.24% | PlayerId NULL (SAH TOTAL.xlsx) |
| TIME_ROW_PRESENT_INVALID_DURATION | 3 | 0.07% | Sentinel 13:00:00 (artefact Excel) |
| TIME_ROW_PRESENT_DUPLICATE | 4 (2 cas × 2 lignes) | 0.10% | Deux lignes par (PlayerId, MatchId) |
| TIME_ROW_PRESENT_VALID | ~4 067 | ~98.5% | Lignes correctes (2024-2025) |

## Répartition des cas non disponibles

| Question | Réponse |
|---|---|
| Cas réparables par meilleure jointure | 0 — la jointure est déjà par PlayerId (int FK) |
| Cas réparables par rapprochement historique | 0 — histoplayer est vide |
| Cas réparables par dérivation (substitutions) | 0 — aucun événement de remplacement dans matchevents |
| Cas définitivement indisponibles (données absentes) | ~3 260 joueuses-matchs 2025-2026 + 10 NULL + 19 orphelins |

## Conclusion principale

**Le problème signalé n'est pas un bug de jointure. C'est une absence de données source.**

La saison 2025-2026 n'a pas été importée dans `timeplayers`. Les fichiers xlsx correspondants doivent être importés via `SeasonWorkbookTimePlayersImportService` pour que les données apparaissent.

## Bugs secondaires identifiés dans l'API

### Bug 1 : per-60 retourne 0 au lieu de null quand temps=0

- Fichier : `AnalyticsInfrastructure.cs` ligne 272-274
- `LegacyStatsCalculator.ComputePer60` → `SafeStatistics.Per60 ?? 0`
- Quand `PlayingTimeMinutes=0` (données manquantes ou zéro réel), les per-60 affichent `0` au lieu d'être indisponibles
- **Impact** : une joueuse sans données de temps affiche GoalsPer60=0, indiscernable d'une joueuse qui n'a réellement marqué aucun but

### Bug 2 : valeur sentinelle 13:00:00 non filtrée

- 3 lignes avec PlayingTime=13:00:00 (artefact import Excel)
- L'API les compte comme 780 minutes → per-60 artificiellement faibles
- **Correction** : filtrer PlayingTime > 01:30:00 dans BuildTimePlayersQuery

### Bug 3 : PlayingTimeMinutes retourne 0 pour joueur sans temps (DATA_MISSING traité comme 0)

- Les DTOs exposent `PlayingTimeMinutes=0.0` pour les joueuses sans données
- HandWStat affiche `0 min` au lieu de "Temps de jeu non disponible"

## Corrections autorisées par la mission

| Correction | Autorisée | Raison |
|---|---|---|
| Retourner null (double?) pour per-60 quand temps=0 | OUI | Jointure incorrecte prouvée (faux 0) |
| Filtrer PlayingTime > 01:30:00 | OUI | Valeur invalide prouvée |
| Ajouter PlayingTimeAvailability au DTO | OUI | Traçabilité requise |
| Importer les données 2025-2026 | NON | PRODUCTION_ACTIONS=0 — action manuelle requise |
| Créer une migration | NON | DATABASE_MIGRATIONS_CREATED=0 |

ROOT_CAUSE_NAME_MISMATCH_COUNT=0
ROOT_CAUSE_HISTORICAL_ID_COUNT=0
ROOT_CAUSE_ORPHAN_ID_COUNT=19
ROOT_CAUSE_MISSING_SOURCE_COUNT=~3260 joueuses-matchs (163 matchs × ~20)
ROOT_CAUSE_INVALID_DURATION_COUNT=3
ROOT_CAUSE_IMPORT_PARTIAL_COUNT=10 (NULL PlayerId)
ROOT_CAUSE_UNKNOWN_COUNT=0

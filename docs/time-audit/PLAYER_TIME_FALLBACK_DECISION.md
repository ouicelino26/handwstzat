# PLAYER_TIME_FALLBACK_DECISION

Date: 2026-08-02 | Source: DB hbdb audit + API code review | Branch: fix/player-time-availability-v1

## Décision par niveau de fallback

### NIVEAU 1 — IDENTIFIANT DIRECT (PlayerId int FK)

**Statut :** IMPLEMENTÉ ET CORRECT

La jointure timeplayers → players se fait déjà par PlayerId (int). Pour les 4 100 lignes avec PlayerId valide et joueur existant, le niveau 1 est actif.

Aucune modification de la jointure nécessaire.

### NIVEAU 2 — IDENTIFIANT HISTORIQUE

**Statut :** UNAVAILABLE

`histoplayer` est vide. Aucun historique utilisable.

### NIVEAU 3 — CLÉ MÉTIER FORTE ET UNIQUE

**Statut :** NOT_NEEDED

Les données 2024-2025 sont déjà liées via PlayerId. Pour les données manquantes (2025-2026), la cause est l'absence totale de lignes timeplayers — pas une résolution incorrecte. Une meilleure normalisation des noms n'aiderait pas : il n'y a simplement pas de lignes à lier.

**LicenseNumber/BirthDate dans timeplayers :** ABSENT — la table ne contient que le nom et le MatchId.

### NIVEAU 4 — ROSTER UNIQUE DANS LE MATCH

**Statut :** NOT_APPLICABLE

Il n'existe pas de table roster/lineup dans la base. La comparaison "même joueuse dans le même match via nom" serait une jointure timeplayers ↔ matchevents par nom normalisé — ce qui n'est pas nécessaire car les données 2024-2025 ont déjà PlayerId renseigné.

### NIVEAU 5 — DÉRIVATION DEPUIS LES REMPLACEMENTS

**Statut :** UNSAFE_NOT_IMPLEMENTED

`matchevents` ne contient pas d'événements de remplacement typés (aucun EventId de type "entrée" ou "sortie" identifié). La dérivation depuis les remplacements n'est pas possible avec les données actuelles.

SUBSTITUTION_FALLBACK_STATUS=UNSAFE_NOT_IMPLEMENTED
SUBSTITUTION_DATA_COMPLETENESS=NO_SUBSTITUTION_EVENTS_IN_DB

## Corrections implémentées (dans le scope de cette mission)

### Correction 1 : PlayingTimeAvailability dans l'API

Ajouter un champ `PlayingTimeAvailability` aux DTOs :
- `RECORDED_DIRECT` : PlayingTime > 0, PlayerId valide
- `RECORDED_ZERO` : PlayingTime = 0, PlayerId valide (joueuse sur feuille mais non jouée)
- `DATA_MISSING` : pas de ligne timeplayers pour ce joueur/match
- `INVALID_DURATION` : PlayingTime > 01:30:00 (sentinel 13h ou autre anomalie)

### Correction 2 : per-60 null quand temps indisponible

`LegacyStatsCalculator.ComputePer60` retourne actuellement `0` quand minutes=0.

Correction : les propriétés per-60 dans l'accumulateur doivent retourner `null` (ou `double?`) quand `PlayingTimeMinutes <= 0` et que la disponibilité est DATA_MISSING.

**Problème de rétrocompatibilité :** les DTOs exposent `double` (non-nullable) pour les per-60. Changer en `double?` impacte le contrat API.

**Approche retenue :** ajouter `HasPlayingTime` (bool) à `PlayerGlobalStatsDto` pour permettre à HandWStat de savoir si les per-60 sont calculables. Les per-60 restent à `0` dans le DTO mais HandWStat les masque quand `HasPlayingTime=false`.

### Correction 3 : filtrer PlayingTime > 01:30:00

Dans `BuildTimePlayersQuery`, ajouter une clause `where timePlayer.PlayingTime <= TimeSpan.FromMinutes(90)` pour exclure les valeurs sentinelles.

## Fallbacks interdits appliqués

- FIRSTNAME_ONLY_FALLBACK_STATUS=FORBIDDEN ✅
- LASTNAME_ONLY_FALLBACK_STATUS=FORBIDDEN ✅
- FUZZY_MATCHING_STATUS=FORBIDDEN ✅
- AMBIGUOUS_AUTOMATIC_MATCHES=0 ✅

## Conclusion

FALLBACK_IMPLEMENTED=NO (aucun nouveau fallback nécessaire — la jointure est déjà correcte)
FALLBACK_LEVELS_ENABLED=NIVEAU_1_DIRECT_ID_ONLY
MANUAL_ACTION_REQUIRED=Import fichiers timeplayers saison 2025-2026

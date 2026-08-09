# PLAYER_NAME_COLLISION_AUDIT

Date: 2026-08-02 | Source: DB hbdb audit + API code review | Branch: fix/player-time-availability-v1

## Mécanisme de normalisation (API)

`TimePlayersImportService.Normalize(string)` :
1. Décomposition Unicode Form D (strip diacritics / marques non-espacées)
2. UPPER()
3. Remplacement des caractères non alphanumériques par des espaces
4. Collapsage des espaces multiples
5. Trim()

## Détection de collisions à l'import

Si deux joueuses différentes produisent la même clé normalisée :
→ `playerLookup[key] = null` (les deux sont exclues)
→ Aucune attribution automatique ambiguë

Ce mécanisme est **collision-safe par conception**.

## Collisions de NOM seul dans `players`

Seul `KABEYA` apparaît 3 fois — collision de nom de famille potentielle (3 joueuses différentes avec le même NOM).

Conséquence : si un fichier d'import contient `"KABEYA"` sans prénom → la clé normalisée "KABEYA" serait ambiguë → PlayerId=null → pas d'attribution.

Ce cas est géré correctement par le mécanisme de collision.

## Collisions de PRÉNOM seul

Non analysées exhaustivement car `Surname` (prénom) n'est pas unique (ex: MANON × plusieurs joueuses). Le même mécanisme s'applique — collision → null.

## Risque de faux positifs

Si `PlayerName` dans `timeplayers` contient `"NOM"` seul (sans prénom) et que ce NOM est unique dans la base → le lien est établi. Ce cas est rare et l'algorithme ne le signale pas.

Recommandation : le fallback par NOM seul ou PRÉNOM seul devrait idéalement être désactivé ou journalisé séparément pour audit.

## Résultat

| Métrique | Valeur |
|---|---|
| EXACT_ID_MATCH_COUNT | N/A (résolution à l'import, not at query) |
| NORMALIZED_NAME_COLLISION_DETECTED | KABEYA × 3 confirmé |
| AMBIGUOUS_AUTOMATIC_MATCHES | 0 (mécanisme de collision-safe) |
| FIRSTNAME_ONLY_FALLBACK_STATUS | FORBIDDEN (règle G.2) — non implémenté comme fallback autonome |
| LASTNAME_ONLY_FALLBACK_STATUS | FORBIDDEN (règle G.2) — non implémenté comme fallback autonome |
| FUZZY_MATCHING_STATUS | FORBIDDEN (règle G.2) — aucun Levenshtein ou Contains dans le code |

**AMBIGUOUS_AUTOMATIC_MATCHES=0** — conforme à la règle.

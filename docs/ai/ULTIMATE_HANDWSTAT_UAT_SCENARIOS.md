# HandWStat — UAT Scenarios (User Acceptance Testing)

**Date :** 2026-07-30  
**Branche :** `feature/handwstat-ultimate-release-candidate-v1`  
**Statut :** `LIVE_API_TEST=BLOCKED` — credentials staging non disponibles

---

## Prérequis

- Instance API de staging accessible (Bearer JWT valide)
- Données de test : au moins 1 joueuse active dans la ligue cible
- Appareil Windows ou émulateur Android

---

## Scénarios de smoke test

### UAT-001 — Affichage profil ligue v2

**Objectif :** Vérifier que la section "Statistiques Ligue" s'affiche correctement.

**Préconditions :** joueuse avec `playerId` connu, endpoint v2 disponible, Bearer JWT valide.

**Étapes :**
1. Lancer l'application
2. Naviguer vers l'écran Joueuses
3. Sélectionner une joueuse avec données ligue
4. Observer la section "Statistiques Ligue"

**Critères d'acceptation :**
- Les 4 sections (overview, offense, defense, goalkeeper) sont visibles selon le poste
- `failedPivotPasses` affiche "Données non disponibles" (jamais une valeur)
- Les métriques `DATA_MISSING` affichent le message explicite
- Aucun corps brut JSON n'est visible

---

### UAT-002 — Cache ETag (304)

**Objectif :** Vérifier le comportement du cache conditionnel.

**Préconditions :** même joueuse que UAT-001, 2ème chargement.

**Étapes :**
1. Afficher le profil ligue (1er appel → 200)
2. Naviguer vers un autre écran
3. Revenir sur le profil (2ème appel → 304 attendu si données inchangées)

**Critères d'acceptation :**
- Le 2ème appel ne re-charge pas les données (contenu identique)
- Aucun message d'erreur affiché

---

### UAT-003 — Fallback v1 sur 503

**Objectif :** Vérifier que seul HTTP 503 déclenche le fallback.

**Préconditions :** API simulant un 503 sur `/api/v2/analytics/players/{id}`.

**Étapes :**
1. Configurer l'API pour retourner 503
2. Charger le profil ligue

**Critères d'acceptation :**
- Une bannière "données partielles (v1)" est visible
- Les métriques v1 compatibles sont affichées
- Le message indique la source partielle

---

### UAT-004 — Erreur 429 avec Retry-After

**Objectif :** Vérifier l'affichage du délai de retry.

**Préconditions :** API retournant 429 avec `Retry-After: 30`.

**Étapes :**
1. Simuler une requête déclenchant le 429
2. Observer le message d'erreur

**Critères d'acceptation :**
- Message affiche "Réessayez dans 30 secondes"
- Aucune tentative automatique pendant le délai

---

### UAT-005 — Endpoint 405 — pas de fallback

**Objectif :** Vérifier qu'un 405 (Unavailable) ne déclenche PAS de fallback.

**Préconditions :** API retournant 405.

**Étapes :**
1. Simuler un 405 sur le endpoint v2
2. Observer le message d'erreur

**Critères d'acceptation :**
- Message d'erreur explicite affiché (non silencieux)
- Aucune donnée v1 n'apparaît en substitution

---

### UAT-006 — Fonctionnalités masquées

**Objectif :** Vérifier que les fonctionnalités bloquées ne sont pas visibles.

**Étapes :**
1. Parcourir tous les écrans

**Critères d'acceptation :**
- Aucun composant "Possessions", "Lineups", "xG/xS", "Scouting", "Vidéo", "Rapports"
- Aucun appel API vers des endpoints non disponibles (vérifiable via proxy)

---

### UAT-007 — Mise à jour automatique

**Objectif :** Vérifier le flux de mise à jour.

**Étapes :**
1. Simuler une version dépassée (ou utiliser une ancienne build)
2. Observer la redirection vers `/update-required`

**Critères d'acceptation :**
- La page "Mise à jour requise" s'affiche
- L'écran bloque la navigation vers les autres sections

---

## Résultats

| Scénario | Statut | Date | Exécuteur | Notes |
|----------|--------|------|-----------|-------|
| UAT-001 | `BLOCKED` | — | — | Credentials staging requis |
| UAT-002 | `BLOCKED` | — | — | Credentials staging requis |
| UAT-003 | `BLOCKED` | — | — | Simulation API requise |
| UAT-004 | `BLOCKED` | — | — | Simulation API requise |
| UAT-005 | `BLOCKED` | — | — | Simulation API requise |
| UAT-006 | `PENDING` | — | — | Peut être exécuté localement |
| UAT-007 | `PENDING` | — | — | Peut être exécuté localement |

`LIVE_API_TEST=BLOCKED` — Résultats UAT-001 à UAT-005 disponibles uniquement sur staging.

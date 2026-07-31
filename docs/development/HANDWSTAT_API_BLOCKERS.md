# HandWStat — API Blockers

**Date :** 2026-07-31
**Branch :** feature/handwstat-functional-product-v1

Défauts et blocages API qui empêchent l'implémentation de fonctionnalités côté client.

Ne pas modifier HandballManagerAPI dans cette mission.

---

## API-BLOCK-01 — Data Quality : aucun endpoint dédié

| Champ | Valeur |
|-------|--------|
| **ID** | API-BLOCK-01 |
| **Écran** | Qualité des données (page dédiée) |
| **Endpoint** | Aucun — NOT_IMPLEMENTED |
| **OperationId** | — |
| **Requête attendue** | GET /api/v2/analytics/data-quality?competitionId=&teamId= |
| **Réponse observée** | 404 (endpoint inexistant) |
| **Réponse attendue** | DataQualityReport { globalScore, completeness, consistency, freshness, coverage, issues[] } |
| **Impact UI** | Impossible de créer une page /data-quality dédiée. L'affichage de qualité reste limité aux indicateurs inline `MetricValue.quality` par métrique |
| **Contournement sûr** | Afficher `DataQualityBadge` inline par métrique. Ne pas recalculer le score global côté client |
| **Correction serveur recommandée** | Implémenter GET /api/v2/analytics/data-quality avec rapport structuré par scope |
| **Priorité** | P2 |

---

## API-BLOCK-02 — Possessions : données sources manquantes

| Champ | Valeur |
|-------|--------|
| **ID** | API-BLOCK-02 |
| **Écran** | Possessions |
| **Endpoint** | Aucun — BLOCKED_BY_SOURCE_DATA |
| **OperationId** | — |
| **Requête attendue** | GET /api/v2/analytics/possessions ou similaire |
| **Réponse observée** | Non disponible |
| **Réponse attendue** | PossessionStats { duration, sequences, turnovers, efficiency } |
| **Impact UI** | Écran possessions non implémentable. Aucun UI créé |
| **Contournement sûr** | Aucun — ne pas fabriquer de données |
| **Correction serveur recommandée** | Ajouter tracking possessions dans le modèle MatchEvent. Nécessite migration du schéma source |
| **Priorité** | P3 |

---

## API-BLOCK-03 — Lineups / On-Off : données substitutions manquantes

| Champ | Valeur |
|-------|--------|
| **ID** | API-BLOCK-03 |
| **Écran** | Lineups, On/Off, Plus-Minus |
| **Endpoint** | Aucun — BLOCKED_BY_SOURCE_DATA |
| **OperationId** | — |
| **Requête attendue** | GET /api/v1/stats/matches/{id}/lineups |
| **Réponse observée** | Non disponible |
| **Réponse attendue** | MatchLineup { substitutions[], playingTime, onCourt[] } |
| **Impact UI** | Aucune page lineup ou on-off. Plus-minus non calculable |
| **Contournement sûr** | Aucun — ne pas reconstruire depuis les assists/buts |
| **Correction serveur recommandée** | Ajouter les événements de substitution (IN/OUT) dans MatchEvent |
| **Priorité** | P3 |

---

## API-BLOCK-04 — xG / xS : modèle statistique non validé

| Champ | Valeur |
|-------|--------|
| **ID** | API-BLOCK-04 |
| **Écran** | xG, xS |
| **Endpoint** | Aucun — FEATURE_FLAG_DISABLED |
| **OperationId** | — |
| **Impact UI** | Aucune UI créée. Feature flag désactivé côté serveur |
| **Contournement sûr** | Aucun — ne pas approximer xG localement |
| **Correction serveur recommandée** | Valider le modèle statistique, activer le feature flag, exposer endpoint |
| **Priorité** | P4 |

---

## API-BLOCK-05 — Scouting : contrats définis, endpoint non implémenté

| Champ | Valeur |
|-------|--------|
| **ID** | API-BLOCK-05 |
| **Écran** | Scouting |
| **Endpoint** | Aucun — NOT_IMPLEMENTED |
| **Impact UI** | Aucune page scouting créée |
| **Correction serveur recommandée** | Implémenter endpoint scouting selon les contrats définis côté API |
| **Priorité** | P3 |

---

## API-BLOCK-06 — Vidéo : aucun provider de stockage configuré

| Champ | Valeur |
|-------|--------|
| **ID** | API-BLOCK-06 |
| **Écran** | Vidéo |
| **Endpoint** | FEATURE_FLAG_DISABLED |
| **Impact UI** | Aucune UI vidéo. Ne jamais afficher d'onglet vidéo non fonctionnel |
| **Correction serveur recommandée** | Configurer provider (Azure Blob, S3, etc.), activer feature flag |
| **Priorité** | P4 |

---

## API-BLOCK-07 — Rapports : endpoint non implémenté

| Champ | Valeur |
|-------|--------|
| **ID** | API-BLOCK-07 |
| **Écran** | Rapports |
| **Endpoint** | NOT_IMPLEMENTED |
| **Impact UI** | Aucune page rapports |
| **Correction serveur recommandée** | Implémenter endpoint export/rapport analytics |
| **Priorité** | P3 |

---

## Résumé

| ID | Catégorie | Priorité | Statut |
|----|-----------|---------|--------|
| API-BLOCK-01 | NOT_IMPLEMENTED | P2 | En attente équipe backend |
| API-BLOCK-02 | BLOCKED_BY_SOURCE_DATA | P3 | Nécessite migration schéma |
| API-BLOCK-03 | BLOCKED_BY_SOURCE_DATA | P3 | Nécessite événements substitution |
| API-BLOCK-04 | FEATURE_FLAG_DISABLED | P4 | Nécessite validation modèle stat |
| API-BLOCK-05 | NOT_IMPLEMENTED | P3 | Contrats existants, pas d'endpoint |
| API-BLOCK-06 | FEATURE_FLAG_DISABLED | P4 | Nécessite provider stockage |
| API-BLOCK-07 | NOT_IMPLEMENTED | P3 | Contrats existants, pas d'endpoint |

**API_BLOCKER_COUNT = 7**

Aucun de ces blocages ne nécessite une modification de HandWStat pour être documenté.
Tous sont correctement masqués côté client.

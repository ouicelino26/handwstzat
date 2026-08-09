# HandWStat — Roadmap des Statistiques Manquantes

**Date :** 2026-08-01  
**Branche :** feature/handwstat-functional-product-v1  

---

## 1. Objectif

Inventaire exhaustif des statistiques et fonctionnalités analytiques absentes de HandWStat, avec leur raison et la condition de déblocage.

---

## 2. Statistiques bloquées par l'API

### 2.1 Qualité des données (API-BLOCK-01)

| Champ | Détail |
|-------|--------|
| **Fonctionnalité** | Rapport de qualité des données par compétition/équipe |
| **Blocage** | Endpoint GET /api/v2/analytics/data-quality non implémenté |
| **Impact** | Impossible d'afficher un score global de qualité. Seuls les indicateurs inline `MetricValue.quality` par métrique sont disponibles |
| **Condition de déblocage** | Implémenter endpoint API + exposer `DataQualityReport` |
| **Priorité** | P2 |

### 2.2 Possessions (API-BLOCK-02)

| Champ | Détail |
|-------|--------|
| **Fonctionnalité** | Durée des possessions, séquences, efficacité par possession |
| **Blocage** | Pas de tracking des possessions dans les données sources. Nécessite migration du schéma MatchEvent |
| **Impact** | Pas de page Possessions, pas de ratio efficience offensive par possession |
| **Condition de déblocage** | Ajouter tracking possessions dans MatchEvent + exposition endpoint |
| **Priorité** | P3 |

### 2.3 Lineups / On-Off / Plus-Minus (API-BLOCK-03)

| Champ | Détail |
|-------|--------|
| **Fonctionnalité** | Compositions par période, performance ON/OFF, différentiel de score par minute |
| **Blocage** | Pas d'événements de substitution (IN/OUT) dans MatchEvent |
| **Impact** | Pas de page Lineup, pas de métrique plus-minus, pas d'analyse rotation |
| **Condition de déblocage** | Ajouter événements substitution + endpoint GET /api/v1/stats/matches/{id}/lineups |
| **Priorité** | P3 |

### 2.4 xG / xS — Expected Goals / Shots (API-BLOCK-04)

| Champ | Détail |
|-------|--------|
| **Fonctionnalité** | Probabilité de but par tentative, shots expected au-dessus/en-dessous des attentes |
| **Blocage** | Modèle statistique non validé. Feature flag désactivé côté serveur |
| **Impact** | Impossible d'afficher les métriques xG/xS. Ne pas approximer localement |
| **Condition de déblocage** | Valider le modèle statistique, activer le feature flag, exposer endpoint |
| **Priorité** | P4 |

### 2.5 Scouting (API-BLOCK-05)

| Champ | Détail |
|-------|--------|
| **Fonctionnalité** | Profils de recrutement, comparaison inter-équipes/ligues |
| **Blocage** | Contrats définis côté API mais endpoint non implémenté |
| **Impact** | Pas de page Scouting |
| **Condition de déblocage** | Implémenter endpoint scouting |
| **Priorité** | P3 |

### 2.6 Vidéo (API-BLOCK-06)

| Champ | Détail |
|-------|--------|
| **Fonctionnalité** | Clips vidéo associés aux événements (buts, arrêts, tirs) |
| **Blocage** | Pas de provider de stockage configuré. Feature flag désactivé |
| **Impact** | Pas d'onglet vidéo dans la fiche joueuse ou match |
| **Condition de déblocage** | Configurer provider (Azure Blob/S3), activer feature flag |
| **Priorité** | P4 |

### 2.7 Rapports (API-BLOCK-07)

| Champ | Détail |
|-------|--------|
| **Fonctionnalité** | Export PDF/Excel de rapports analytiques |
| **Blocage** | Endpoint export non implémenté |
| **Impact** | Pas de page Rapports, pas d'export |
| **Condition de déblocage** | Implémenter endpoint export analytics |
| **Priorité** | P3 |

---

## 3. Statistiques inexistantes dans le modèle source

Ces métriques nécessitent une collecte à la source (tracking en direct ou modélisation) avant tout développement.

| Métrique | Raison absence | Impact analytique |
|---------|----------------|------------------|
| Distance de tir | Pas dans le modèle MatchEvent | xG, analyse spatiale détaillée |
| Angle de tir | Pas dans le modèle MatchEvent | xG, zones dangereuses |
| Vitesse du tir | Pas trackée | Performance physique |
| Accélérations/décélérations | Pas trackées (GPS requis) | Charge physique |
| Contacts/duels gagnés | Partiellement dans Neutralisations | Défense détaillée |
| Phase de jeu (attaque placée / fast break / contre) | Pas dans le modèle | Analyse tactique avancée |

---

## 4. Statistiques techniquement faisables mais non implémentées

Ces métriques pourraient être calculées localement avec les données actuelles.

| Métrique | Données disponibles | Effort | Priorité |
|---------|---------------------|--------|----------|
| Efficacité par zone déclencheur (v1 events) | ZoneStat triggers | Faible | P2 |
| Ratio buts/passes décisives (performance collective) | v1 global | Trivial | P3 |
| Tendance sur les 5 derniers matchs | v1 match list | Moyen | P2 |
| Classement intra-équipe par KPI | v1 players list | Moyen | P3 |
| Comparaison vs saison précédente | Non disponible | Haute | P4 |

---

## 5. Statistiques en attente de validation méthodologique

| Métrique | Problème | Condition |
|---------|----------|-----------|
| PIE Score (TeamOfDay) | Pondérations arbitraires, non calibrées | Validation sur données historiques 2-3 saisons |
| TechnicalBalanceScore | Non normalisé par temps de jeu | Redéfinir en Per-60 ou par poste |
| Plus-minus estimé | Risque de biais élevé sans lineups | Attendre API-BLOCK-03 |

---

## 6. Résumé des priorités

| Priorité | Nombre | Bloquants |
|---------|--------|-----------|
| P2 | 2 | Qualité données, zones déclencheur |
| P3 | 5 | Possessions, Lineups, Scouting, Rapports, export |
| P4 | 2 | xG/xS, Vidéo |

**API_BLOCKER_COUNT = 7**  
**LOCALLY_FEASIBLE_IMPROVEMENTS = 5**  
**PENDING_VALIDATION = 3**

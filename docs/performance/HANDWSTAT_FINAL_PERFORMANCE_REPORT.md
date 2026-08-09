# HandWStat — Rapport de performance final

**Date :** 2026-07-30  
**Note :** Aucune mesure live n'est disponible sans API staging. Les données ci-dessous sont des analyses statiques du code.

---

## Appels API au chargement — analyse statique

| Écran | Appels initiaux estimés | Cache référentiel | Après cache | Notes |
|-------|------------------------|------------------|-------------|-------|
| Dashboard (1er chargement) | ~28 | -7 (référentiels) | ~21 | Équipe du jour lazy-loadée |
| Dashboard (rechargement) | ~21 | 0 (cache valide) | ~21 | ETag non applicable sur v1 |
| Joueuses (profil v2) | 1 appel v2 + ~8 appels v1 | Référentiels en cache | ~9 | ETag sur 2ème chargement même joueuse |
| Joueuses (2ème chargement même joueuse) | 1 appel v2 (304 attendu) + 0-8 v1 | — | ~1-8 | 304 si données inchangées |
| Comparaison | 1 POST + N profils | — | 1+N | Pas de batch v2 disponible |
| Équipes | ~3-5 | — | ~3-5 | — |
| Matchs | ~5-8 | — | ~5-8 | Spatial conditionnel |

---

## Mécanismes de performance implémentés

| Mécanisme | Statut | Fichier |
|-----------|--------|---------|
| CancellationToken propagé sur tous les appels | ✅ PASS | `ApiClientBase.GetAsync`, `GetConditionalAsync` |
| CTS annulation dashboard (scope obsolète) | ✅ PASS | `Home.razor.cs` |
| Lazy loading équipe du jour | ✅ PASS | `TeamOfTheDayService`, `Dashboard.razor` |
| Cache référentiel session | ✅ PASS | `ReferenceDataService` |
| ETag / 304 sur appels v2 | ✅ PASS | `ApiClientBase.GetConditionalAsync<T>` |
| `ConcurrentDictionary` ETag thread-safe | ✅ PASS | `ApiClientBase._etagCache` |
| Chargements parallèles (Task.WhenAll) | ✅ PASS | `StatsDashboardService` |
| ApexCharts chargés uniquement sections visibles | ✅ PASS | Razor sections conditionnelles |

---

## Défauts de performance connus (non bloquants)

| Défaut | Impact | Résolution |
|--------|--------|-----------|
| ~28 appels initiaux dashboard | Latence ressentie sur Android radio | Attendre endpoint agrégé v2 (API Phase 2) |
| `ReferenceDataService` double chargement concurrent possible | Rare — double requête même référentiel | Ajouter `SemaphoreSlim` anti-double (P1) |
| `Players.razor` ~24k tokens Razor | Taille de fichier | Découpage en sous-composants (P2) |
| Compare — N appels parallèles (pas de batch v2) | Latence proportionnelle au nombre de joueuses | Attendre batch endpoint v2 |

---

## Mesures requises sur staging (non disponibles localement)

- Temps de chargement dashboard (Time To Interactive)
- Nombre exact d'appels réseau
- Ratio cache hit / cache miss
- Latence médiane appel v2 vs v1
- Consommation mémoire Android release
- Temps de démarrage cold vs warm

`PERFORMANCE_STATUS=PARTIALLY_ANALYZED` — analyse statique complète ; mesures live bloquées par absence d'API staging.

# HandWStat Ultimate — Blockers

**Date :** 2026-07-30

---

## Blockers actifs

### BLOCKER-01 : HandballManagerCore non publié sur registre distant

**Impact :** HandWStat ne peut pas être compilé en CI/CD sans accès aux sources locales de Core  
**Statut :** `API_REMOTE_REPRODUCIBILITY=BLOCKED_BY_UNPUSHED_CORE`  
**Résolution :** Ne bloque pas le travail local HandWStat. Les DTOs sont répliqués dans `HandWStat.Models.Contracts`. Sera résolu quand Core sera publié sur NuGet ou autre registre.  
**Gate :** `READY_FOR_RELEASE=NO`

### BLOCKER-02 : Aucun fichier openapi.json disponible

**Impact :** Impossible de générer un client typé automatiquement depuis la spec OpenAPI  
**Statut :** `HandballManagerAPI/docs/openapi/` n'existe pas  
**Résolution :** Contrats manuellement dérivés de `HANDWSTAT_API_V2_MASTER_CONTRACT.md` et `HANDWSTAT_ENDPOINT_MATRIX.md`  
**Gate :** Non bloquant pour les fonctionnalités actuelles

---

## Blockers levés durant cette mission

| Blocker | Levé par |
|---------|----------|
| ProjectReference à HandballManagerCore | Réplication locale des DTOs dans `Models/Contracts/` |
| MatchEvent (entité EF) dans MatchEventsApiClient | Remplacement par `MatchEventAnalyticsDto` |
| Fallback v1 incorrect (405/501) | Nouvel outcome `ServiceUnavailable` — fallback 503 uniquement |
| Absence de ETag/304 | `ApiClientBase.GetConditionalAsync<T>` + `ApiGetResult<T>` |
| Absence de Retry-After | `ApiRequestException.RetryAfterSeconds` + propagation dans `LeagueAnalyticsError` |

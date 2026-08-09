# HandWStat — Release Checklist

**Date :** 2026-07-30

---

## Pré-requis obligatoires avant release

- [ ] HandballManagerCore publié sur registre distant (NuGet privé ou GitHub Packages)
- [ ] `READY_FOR_RELEASE` mis à `YES` dans `ULTIMATE_HANDWSTAT_PRODUCTION_READINESS.md`
- [ ] Build iOS validé en CI (macOS agent)
- [ ] Build Android validé en CI
- [ ] Tests d'intégration sur API de staging (`GET /api/v2/analytics/players/{id}`)
- [ ] ETag round-trip vérifié sur staging (200 puis 304)
- [ ] Retry-After respecté sur staging (429 simulé)
- [ ] correlationId visible dans les logs

## Contrats à ne jamais casser

- [ ] `failedPivotPasses` : `value = null`, `availability = DATA_MISSING` — jamais substitué
- [ ] Fallback v1 : uniquement sur HTTP 503 (`ServiceUnavailable`) — jamais sur 400/401/404/405/500
- [ ] ETag : `If-None-Match` envoyé si ETag connu — jamais ignoré
- [ ] Bearer JWT : présent sur chaque appel authentifié

## Sécurité

- [ ] Aucune clé, token ou credential dans le code ou les docs
- [ ] `READY_FOR_RELEASE=NO` visible dans les docs tant que Core n'est pas publié

## Documentation

- [ ] `docs/integration/API_V2_HANDOFF_CHECKSUMS.md` à jour
- [ ] `docs/ai/ULTIMATE_HANDWSTAT_PRODUCTION_READINESS.md` gates mis à jour

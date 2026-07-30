# HandWStat — Notes de sécurité

**Date :** 2026-07-30

---

## Authentification

- **Mécanisme :** Bearer JWT sur chaque appel API authentifié
- **Gestion :** `ApiClientBase` injecte le token depuis le contexte d'authentification
- **Expiration :** gérée par le service d'identité amont — HandWStat ne stocke pas de refresh token

---

## Secrets et credentials

- **Aucun secret en dur** dans le code ou les docs
- **CI secret scan** : `.github/workflows/automatic-update-validation.yml` scanne les patterns AKIA*, gh*, clés privées
- **Signing Android/iOS :** `SIGNING_STATUS=BLOCKED_EXTERNAL_CREDENTIALS` — keystore et provisioning profile non commités
- **Signing Windows :** `WindowsPackageType=None` — pas de cert requis pour les builds de développement

---

## Données utilisateur

- HandWStat est une application de lecture de statistiques sportives
- Aucune donnée PII saisie par l'utilisateur
- Les logs `Debug.WriteLine` (dev uniquement) n'enregistrent que des codes d'erreur techniques et des correlationId
- Aucun token, password ou donnée personnelle n'est loggé

---

## Règles de sécurité transport

- HTTPS uniquement en production — les URLs de base API sont configurées sans fallback HTTP
- `If-None-Match` / ETag : aucune donnée sensible dans les ETags (valeurs opaques côté serveur)
- `correlationId` : uniquement pour le diagnostic — non affiché à l'utilisateur final

---

## Gates de sécurité

| Gate | Statut |
|------|--------|
| `GATE_NO_SECRETS_IN_CODE` | ✅ PASS (secret scan CI + revue manuelle) |
| `GATE_BEARER_ON_ALL_AUTH_CALLS` | ✅ PASS (`ApiClientBase`) |
| `GATE_NO_RAW_BODY_TO_USER` | ✅ PASS (corps brut uniquement en `Debug.WriteLine`) |
| `GATE_NO_CORE_DEPENDENCY` | ✅ PASS |
| `SIGNING_STATUS` | ❌ BLOCKED_EXTERNAL_CREDENTIALS |

---

## `READY_FOR_RELEASE=NO`

La condition `READY_FOR_RELEASE=NO` reste active tant que :
1. HandballManagerCore n'est pas publié sur un registre distant
2. Les builds iOS et Android ne sont pas validés en CI
3. Les tests d'intégration live ne sont pas exécutés sur staging

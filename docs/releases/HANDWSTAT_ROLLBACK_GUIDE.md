# HandWStat — Rollback Guide

**Date :** 2026-07-30

---

## Principes

- HandWStat ne maintient pas de serveur d'état — les données viennent de l'API
- Un rollback est une réinstallation d'une version précédente
- Le mécanisme de mise à jour (`AppUpdateService`) peut bloquer l'application si une version obligatoire est requise
- Un rollback est valide uniquement si la version cible est compatible avec l'API en production

---

## Procédures

### Rollback Windows

1. Désinstaller la version actuelle (ou remplacer les binaires)
2. Installer le package de la version précédente depuis `.artifacts/release/windows/<version>/`
3. Vérifier que l'API staging ou production accepte la version installée (`/api/v2/updates/check` doit retourner `updateType=NONE` ou `OPTIONAL`)

### Rollback Android

1. Désinstaller l'APK actuel sur l'appareil
2. Réinstaller l'APK de la version précédente (sideload ou Play Store selon le canal)
3. Vérifier compatibilité API

---

## Cas de rollback bloqué

Si l'API retourne `updateType=MANDATORY` pour la version rollback cible :
- La version cible est incompatible API
- Le rollback vers cette version n'est pas autorisé
- Contacter l'équipe backend pour vérifier le `minimumVersion` configuré côté API

---

## Versions disponibles

Les manifestes de release sont conservés dans `.artifacts/release/manifest-<version>.json`.  
Vérifier le SHA-256 de l'artefact avant réinstallation.

---

## Checklist rollback

- [ ] Identifier la version cible
- [ ] Vérifier compatibilité API (`minimumVersion` côté serveur)
- [ ] Récupérer l'artefact (SHA-256 vérifié)
- [ ] Désinstaller version actuelle
- [ ] Installer version cible
- [ ] Vérifier que l'application démarre sans redirection update-required
- [ ] Vérifier que les données s'affichent correctement

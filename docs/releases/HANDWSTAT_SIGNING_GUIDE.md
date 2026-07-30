# HandWStat — Signing Guide

**Date :** 2026-07-30  
**Statut :** `SIGNING_STATUS=BLOCKED_EXTERNAL_CREDENTIALS`

---

## Windows

### Prérequis

- Certificat de signature de code (`.pfx` ou stockage certificat Windows)
- Variables d'environnement à fournir (ne jamais committer) :
  - `HANDWSTAT_WINDOWS_CERTIFICATE_PATH` — chemin absolu vers le fichier `.pfx`
  - `HANDWSTAT_WINDOWS_CERTIFICATE_PASSWORD` — mot de passe du `.pfx`

### Commandes (après obtention du certificat)

```powershell
# Build avec signature (MSIX)
dotnet publish HandWStat.csproj `
    -c Release `
    -f net10.0-windows10.0.19041.0 `
    -p:WindowsPackageType=MSIX `
    -p:PackageCertificateThumbprint="<thumbprint>" `
    -p:PackageCertificateKeyFile="$env:HANDWSTAT_WINDOWS_CERTIFICATE_PATH" `
    -p:PackageCertificatePassword="$env:HANDWSTAT_WINDOWS_CERTIFICATE_PASSWORD"
```

### Statut actuel

`WindowsPackageType=None` — builds de développement non signés.  
`WINDOWS_SIGNATURE_STATUS=BLOCKED_EXTERNAL_CREDENTIALS`

---

## Android

### Prérequis

- Keystore de production (`.jks` ou `.keystore`)
- Variables d'environnement à fournir :
  - `HANDWSTAT_ANDROID_KEYSTORE_PATH` — chemin absolu vers le keystore
  - `HANDWSTAT_ANDROID_KEYSTORE_PASSWORD` — mot de passe du keystore
  - `HANDWSTAT_ANDROID_KEY_ALIAS` — alias de la clé de signature
  - `HANDWSTAT_ANDROID_KEY_PASSWORD` — mot de passe de la clé

### Commandes (après obtention du keystore)

```powershell
dotnet build HandWStat.csproj `
    -c Release `
    -f net10.0-android `
    -p:AndroidKeyStore=true `
    -p:AndroidSigningKeyStore="$env:HANDWSTAT_ANDROID_KEYSTORE_PATH" `
    -p:AndroidSigningStorePass="$env:HANDWSTAT_ANDROID_KEYSTORE_PASSWORD" `
    -p:AndroidSigningKeyAlias="$env:HANDWSTAT_ANDROID_KEY_ALIAS" `
    -p:AndroidSigningKeyPass="$env:HANDWSTAT_ANDROID_KEY_PASSWORD"
```

### Statut actuel

`ANDROID_SIGNATURE_STATUS=BLOCKED_EXTERNAL_CREDENTIALS`  
Les builds actuels sont non signés (`AndroidKeyStore=false` par défaut).  
Un APK `DEBUG_SIGNED` ne doit jamais être distribué.

---

## iOS

### Prérequis

- Compte Apple Developer Program actif
- Provisioning profile de distribution
- Certificat de distribution iOS
- Agent CI macOS

`IOS_SIGNATURE_STATUS=BLOCKED_EXTERNAL_CREDENTIALS`

---

## Vérification des prérequis

Utiliser le script :

```powershell
.\scripts\release\Test-ReleasePrerequisites.ps1
```

Ce script vérifie la présence des variables sans afficher leur valeur.

---

## Règles de sécurité

- Ne jamais committer les fichiers `.pfx`, `.jks`, `.keystore` dans le dépôt
- Ne jamais logguer les mots de passe
- Utiliser uniquement des variables d'environnement CI/CD
- Les variables doivent être marquées "secret" dans GitHub Actions
- `DEBUG_SIGNED` ≠ signé pour la production — toujours vérifier avant distribution

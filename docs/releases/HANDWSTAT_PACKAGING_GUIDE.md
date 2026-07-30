# HandWStat — Packaging Guide

**Date :** 2026-07-30

---

## Pré-requis

- .NET 10 SDK installé
- Workloads MAUI : `android`, `ios`, `maccatalyst`, `maui-windows`
- Espace disque : ≥ 5 GB pour Android (AOT compilation), ≥ 2 GB pour Windows

Vérifier les prérequis :
```powershell
.\scripts\release\Test-ReleasePrerequisites.ps1
```

---

## Workflow recommandé

```
1. Test-ReleasePrerequisites.ps1    — vérifications prérequis
2. Build-Release.ps1                — restore + build toutes plateformes
3. Package-Windows.ps1 -Version X   — publish Windows
4. Package-Android.ps1 -Version X   — build Android APK/AAB
5. Verify-Artifact.ps1              — SHA-256 vérification
6. Generate-ReleaseManifest.ps1     — manifeste JSON artefacts
```

---

## Windows

```powershell
.\scripts\release\Package-Windows.ps1 -Version "1.0.0"
```

**Sortie :** `.artifacts/release/windows/1.0.0/`  
**Format :** Self-contained (non-MSIX en dev, MSIX avec certificat en prod)  
**Architecture :** win-x64  
**`WindowsPackageType` :** `None` (dev) / `MSIX` (prod avec certificat)

---

## Android

```powershell
.\scripts\release\Package-Android.ps1 -Version "1.0.0"
```

**Sortie :** `bin/Release/net10.0-android/`  
**Format :** APK (non signé en dev)  
**Signing :** `BLOCKED_EXTERNAL_CREDENTIALS` — voir `HANDWSTAT_SIGNING_GUIDE.md`

---

## Répertoire des artefacts

Les packages sont générés dans `.artifacts/release/` (gitignored).  
**Ne jamais committer les binaires de release.**

---

## Vérification SHA-256

```powershell
.\scripts\release\Verify-Artifact.ps1 `
    -Path ".artifacts/release/windows/1.0.0/HandWStat.exe" `
    -ExpectedSha256 "<hash>"
```

---

## Manifeste de release

```powershell
.\scripts\release\Generate-ReleaseManifest.ps1 -Version "1.0.0"
```

Génère `.artifacts/release/manifest-1.0.0.json` avec :
- version, buildDate, branch, commit
- SHA-256 et taille de chaque artefact
- statut signing
- plateformes ciblées

---

## Artefacts gitignorés

Le fichier `.gitignore` doit contenir :
```
.artifacts/
artifacts/
*.apk
*.aab
*.msix
*.appx
```

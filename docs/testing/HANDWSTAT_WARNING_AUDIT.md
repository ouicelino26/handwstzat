# HandWStat — Audit des warnings de build

**Date :** 2026-07-30  
**Branche :** `feature/handwstat-ultimate-release-candidate-v1`  
**Baseline :** 32 warnings (BASELINE)  
**Après corrections :** 0 warnings

---

## Résumé

| Catégorie | Baseline | Après fix |
|-----------|---------|-----------|
| CS8604 — Null reference argument | 29 | 0 |
| CS8714 — Nullable type parameter | 1 | 0 |
| CS8621 — Nullable lambda return | 1 | 0 |
| Total applicatifs | 31 | 0 |
| Doublons TFM (même warning × 2 passes) | +32 (affichage dupliqué) | 0 |

Tous les warnings étaient des **avertissements de nullabilité applicatifs** — corrigés par ajout de `?? string.Empty` et filtre `.Where(x is not null)` aux sites d'appel.

---

## Détail des corrections

### CS8604 × 28 — `ZoneCode`, `EventName`, `TriggerCode` nullable dans DTOs API

**Origine :** Les DTOs `ZoneStatDto.ZoneCode`, `ZoneStatDto.EventName`, `TriggerZoneStatDto.TriggerCode`, `OutcomeStatDto.EventName` sont déclarés `string?` dans `Models/Contracts/ApiContracts.cs` car les réponses JSON peuvent omettre ces champs. Les records `ZoneStat(string Key, string Label, ...)` et `OutcomeCount(string Label, int Count)` attendent `string` non-nullable.

**Fichiers concernés :**
- [Services/StatsDashboardService.cs](../../Services/StatsDashboardService.cs) — `MapGoalZones` (ligne 441-448) et `MapTriggerZones` (ligne 462)
- [Components/Pages/Players.razor](../../Components/Pages/Players.razor) — `MapZone` (ligne 1465-1470) et `MapTrigger` (ligne 1481)
- [Components/Pages/Matches.razor](../../Components/Pages/Matches.razor) — `MapZone` (ligne 1318-1323) et `MapTrigger` (ligne 1334)

**Correction appliquée :** `zone.ZoneCode ?? string.Empty`, `outcome.EventName ?? string.Empty`. Pour `ToDictionary` avec clé nullable : filtre `.Where(zone => zone.ZoneCode is not null)` + opérateur `!` sur la clé.

**Justification :** Une zone sans code est une donnée API malformée ; `string.Empty` est un fallback inoffensif qui évite le crash et n'affecte pas la logique métier (les zones sont filtrées visuellement par leur label).

**Classement :** `FIXED`

---

### CS8604 × 1 — `response.LatestVersion` nullable

**Origine :** `UpdateCheckResponseDto.LatestVersion` est `string?`. `ReleaseArtifactValidationInput` attend `string Version` (non-nullable).

**Fichier :** [Services/Updates/AppUpdateService.cs](../../Services/Updates/AppUpdateService.cs) ligne 253

**Correction appliquée :** `response.LatestVersion ?? string.Empty`

**Justification :** Si `LatestVersion` est null, `TryValidate` rejettera l'entrée (`Version` vide ne satisfait pas le format attendu). Le fallback `string.Empty` préserve le comportement de rejet sans crash.

**Classement :** `FIXED`

---

### CS8714 × 1 — `ToDictionary` avec clé `string?`

**Origine :** `zone.ZoneCode` est `string?`, utilisé comme clé de dictionnaire dans `Enumerable.ToDictionary<TSource, string?, ...>` dont la contrainte `notnull` interdit `string?`.

**Fichier :** [Services/StatsDashboardService.cs](../../Services/StatsDashboardService.cs) ligne 436

**Correction appliquée :** `.Where(zone => zone.ZoneCode is not null)` + `zone.ZoneCode!`

**Classement :** `FIXED`

---

### CS8621 × 1 — Lambda return type nullable mismatch

**Origine :** Le délégué `Func<ZoneStatDto, string?>` produit par la lambda `zone => zone.Outcomes` ne correspond pas à la signature attendue par `ToDictionary`.

**Fichier :** [Services/StatsDashboardService.cs](../../Services/StatsDashboardService.cs) ligne 437

**Correction appliquée :** résolu par la même correction que CS8714 (filtre + opérateur `!`).

**Classement :** `FIXED`

---

## État final

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

`APPLICATION_WARNINGS=0` — Objectif atteint.

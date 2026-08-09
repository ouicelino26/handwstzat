# HandWStat — Système de Coordonnées Tirs

**Date :** 2026-08-01  
**Branche :** feature/handwstat-functional-product-v1  

---

## 1. Réponse directe

HandWStat **n'utilise pas de coordonnées X/Y brutes** pour les tirs. Le système est exclusivement basé sur des codes de zones. Aucune normalisation de coordonnées brutes n'est nécessaire ni présente dans le code.

---

## 2. Architecture du système de zones

### 2.1 Types de zones

| Type | Préfixe | Nombre | Usage |
|------|---------|--------|-------|
| Zones cadre (goal frame) | BG (Butoir Gauche) / BD (Butoir Droite) | 12 par côté = 24 total | Efficacité par zone de but |
| Zones déclencheur (trigger) | TG (Trigger Gauche) / TD (Trigger Droite) | 9 par côté = 18 total | Zone de terrain d'où part le tir |

### 2.2 Contrat ZoneStat

```csharp
record ZoneStat(
    string Key,       // ex: "BG3", "TD7"
    string Label,     // ex: "Milieu gauche", "Aile droite"
    double Rate,      // taux de réussite = Successes / Attempts
    int Attempts,     // nombre de tentatives
    int Successes,    // nombre de buts
    string[] Outcomes // liste des outcomes individuels
)
```

Pas de coordonnées X/Y dans le contrat. Juste un code de zone, un taux, et les comptages.

---

## 3. Logique miroir (ToVisualTriggerKey)

```csharp
public static string ToVisualTriggerKey(string? key)
{
    if (normalized.StartsWith("TG")) return $"TD{normalized[2..]}";
    if (normalized.StartsWith("TD")) return $"TG{normalized[2..]}";
    return normalized;
}
```

Cette logique "miroir" reflète les zones déclencheur de gauche à droite et vice-versa. Elle est utilisée pour afficher les tirs d'équipes qui défendent dans des directions différentes selon le match — les zones sont normalisées pour toujours apparaître du même côté dans la visualisation.

---

## 4. Chemins SVG hardcodés

Le rendu spatial est assuré par des chemins SVG précalculés par zone, définis dans `SpatialZoneVisuals.cs`. Chaque code de zone (ex: "BG1", "TD5") correspond à un chemin SVG spécifique représentant la forme de cette zone sur le schéma de terrain/but.

Ce design évite toute transformation de coordonnées brutes au runtime.

---

## 5. Heatmap gardienne

La normalisation de la palette de chaleur pour les gardiennes utilise une plage fixe :

```
Palette = Map(rawRate, from: [10%, 55%], to: [0%, 100%])
```

- Un taux de réussite de 10% → couleur froide (bas de palette)
- Un taux de réussite de 55% → couleur chaude (haut de palette)

Ce choix de plage (10-55%) est basé sur les réalités du handball professionnel féminin. La médiane se situe autour de 30-35%.

---

## 6. GoalKpi — composant de rendu

`GoalKpi.razor` affiche le cadre de but avec les zones colorées selon l'efficacité. Les zones BG/BD sont rendues via les chemins SVG. Le taux affiché est `Rate = Successes / Attempts` par zone (fourni directement par l'API).

**Dénominateur par zone :** `Attempts` fourni par le contrat `ZoneStat`. Jamais recalculé côté client.

---

## 7. Verdict

| Aspect | Statut |
|--------|--------|
| Coordonnées X/Y brutes | ✅ Absentes (non utilisées) |
| Système par codes de zones | ✅ Correct |
| Chemins SVG hardcodés | ✅ Correct |
| Logique miroir | ✅ Correcte |
| Dénominateurs zones | ✅ Fournis par API |
| Normalisation palette heatmap | ✅ Plage [10-55%] cohérente |
| ZoneStat contrat | ✅ Complet |

**SHOT_COORDINATE_SYSTEM_VALIDITY = 100%**

# HandWStat — Audit Radars et Profils de Poste

**Date :** 2026-08-01  
**Branche :** feature/handwstat-functional-product-v1  

---

## 1. Objectif

Vérification de la normalisation, de l'inversion des axes et de la cohérence des radars dans HandWStat.

Composants couverts : `MultiRadar.razor`, `PositionRadarChart.razor`, `PositionProfileAxisViewModel.NormalizeRadarValue`.

---

## 2. Architecture radar

### Pipeline de données

```
API v2 → PositionProfileAxisDto { Value, MinValue, MaxValue, DirectionalPercentile, HigherIsBetter }
       → PositionProfileAxisViewModel (mapping + NormalizeRadarValue)
       → MultiRadar / PositionRadarChart (affichage Chart.js)
```

### NormalizeRadarValue

```csharp
private double NormalizeRadarValue(double value)
{
    if (!double.IsFinite(MinValue) || !double.IsFinite(MaxValue) || MaxValue <= MinValue)
        return Math.Clamp(DirectionalPercentile, 0d, 100d); // fallback percentile

    var normalized = (value - MinValue) * 100d / (MaxValue - MinValue);
    if (!HigherIsBetter) normalized = 100d - normalized;
    return Math.Clamp(Math.Round(normalized, 1, MidpointRounding.AwayFromZero), 0d, 100d);
}
```

---

## 3. Analyse de la normalisation

### 3.1 Plage valide

Condition de plage valide : `double.IsFinite(MinValue) && double.IsFinite(MaxValue) && MaxValue > MinValue`

**Cas limite : valeur hors plage**  
Si `value < MinValue` → normalized < 0 → `Math.Clamp` ramène à 0.  
Si `value > MaxValue` → normalized > 100 → `Math.Clamp` ramène à 100.  
Comportement correct : les outliers sont clampés sans crash.

### 3.2 Fallback percentile

Si la plage est invalide (NaN, Infinity, ou MaxValue == MinValue), le fallback utilise `DirectionalPercentile` fourni par l'API. Ce percentile est déjà sur 0-100. Correct.

### 3.3 Inversion axes négatifs

Les axes `HigherIsBetter=false` (pertes, sanctions, buts encaissés) appliquent `100d - normalized`. Une valeur basse (performance favorable) donne donc un score radar élevé.

**Avertissement :** Le commentaire dans le code précise que "The API contract already returns a favorable DirectionalPercentile for negative axes." Cela signifie que dans le fallback percentile, pas besoin d'inverser. Dans le chemin principal (plage valide), l'inversion est correctement appliquée sur la valeur brute normalisée.

**Pas de double inversion** : les deux chemins sont mutuellement exclusifs. ✅

### 3.4 Arrondi

`Math.Round(normalized, 1, MidpointRounding.AwayFromZero)` — cohérent avec les autres arrondis du projet.

---

## 4. Annotations percentile (P1 corrigé)

`GetAnnotationText()` retourne une étiquette textuelle selon le percentile directionnel :

```csharp
if (axis.DirectionalPercentile >= 90d) return "Top 10%";
if (axis.DirectionalPercentile <= 20d) return "Alerte";
return "Au-dessus de la médiane"; // P1 corrigé (était "Above median")
```

**Correction appliquée dans 4 composants :**
- `MultiRadar.razor:605`
- `PositionRadarChart.razor:481`
- `ScatterChart.razor:519`
- `PositionProfileHistogram.razor:329`

---

## 5. MultiRadar (comparaison)

Le composant `MultiRadar` superpose plusieurs radars (2-6 joueuses) sur le même graphique Chart.js. Chaque radar est normalisé indépendamment sur son axe respectif.

**Note :** La normalisation est par axe (même plage pour toutes les joueuses sur un axe donné), pas par joueuse. Cela garantit la comparabilité inter-joueuses sur chaque dimension.

---

## 6. PositionRadarChart (profil individuel)

Le radar de profil de poste affiche une joueuse par rapport à sa cohorte (même poste, même compétition, même saison). La valeur normalisée représente la position relative dans la cohorte.

---

## 7. Cas spéciaux

| Cas | Comportement | Correctif |
|-----|-------------|-----------|
| Nouvelle joueuse (0 matchs) | MinValue = MaxValue → fallback percentile (0) | ✅ |
| API retourne NaN pour un axe | Fallback percentile | ✅ |
| Joueuse avec valeur = MaxValue | Score radar = 100 (si HigherIsBetter) | ✅ |
| Joueuse avec valeur = MinValue | Score radar = 0 (si HigherIsBetter) | ✅ |
| Axe négatif valeur = MinValue | Score radar = 100 (meilleure de la cohorte) | ✅ |

---

## 8. Verdict radar

| Dimension | Statut |
|---------|--------|
| Normalisation 0-100 | ✅ CORRECT |
| Inversion axes négatifs | ✅ CORRECT |
| Fallback percentile | ✅ CORRECT |
| Pas de double inversion | ✅ VÉRIFIÉ |
| Clamp outliers | ✅ CORRECT |
| Annotations françaises | ✅ CORRIGÉ (P1) |
| MultiRadar comparaison | ✅ CORRECT |

**RADAR_NORMALIZATION_CORRECTNESS = 100%**

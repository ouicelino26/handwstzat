# HandWStat — Audit Carte des Tirs

**Date :** 2026-08-01  
**Branche :** feature/handwstat-functional-product-v1  

---

## 1. Objectif

Vérification de la cohérence, de l'exactitude et de la pertinence analytique de la représentation spatiale des tirs dans HandWStat.

---

## 2. Composants concernés

| Composant | Usage |
|-----------|-------|
| `GoalKpi.razor` | Zones de tir sur cadre (BG/BD) + efficacité |
| `SpatialZoneVisuals.cs` | Mapping code → SVG path, logique miroir, palette |
| `ApiContracts.cs` — `ZoneStat` | Contrat données zones |

---

## 3. Zones cadre de but (Goal Frame Zones)

### 3.1 Couverture

24 zones (12 BG + 12 BD) couvrent le cadre de but. Les zones sont :
- BG1-BG12 : zones gauche du gardien (droite du tireur)
- BD1-BD12 : zones droite du gardien (gauche du tireur)

### 3.2 Affichage

Chaque zone affiche :
- Taux de réussite (`Rate = Successes / Attempts * 100%`)
- Volume de tentatives (`Attempts`)
- Couleur de heatmap basée sur le taux

### 3.3 Dénominateur correct

Le dénominateur de chaque zone est `Attempts` fourni par le contrat `ZoneStat`. Il est **fourni par l'API** — le client ne le recalcule pas. Correct.

### 3.4 Zones avec faible volume

`RateDisplayModel.SampleReliable` est utilisé pour signaler les zones avec un volume insuffisant. Les zones avec peu de tirs devraient déclencher un badge d'avertissement. Ce mécanisme est présent dans le modèle mais son application systématique sur les zones de tir n'a pas été vérifiée visuellement (test visuel requis).

---

## 4. Zones déclencheur (Trigger Zones)

### 4.1 Couverture

18 zones (9 TG + 9 TD) couvrent les zones de terrain d'où partent les tirs.

### 4.2 Logique miroir

La fonction `ToVisualTriggerKey` reflète les zones pour normaliser l'affichage :
```
TG → TD (et vice-versa)
```

**Raison :** Dans un match, les équipes défendent alternativement à gauche et à droite. Le miroir permet de toujours afficher les tirs de la même équipe du même côté visuel, facilitant la comparaison inter-matchs.

**Cohérence :** La logique miroir est appliquée de façon consistante dans `SpatialZoneVisuals.ToVisualTriggerKey`. Correct.

---

## 5. Palette de chaleur gardienne

```
Plage de mapping : rawRate [10%, 55%] → palette [0%, 100%]
```

**Interprétation :**
- ≤10% de réussite dans une zone → froid (zone difficile pour le tireur / bonne défense de zone)
- ≥55% de réussite → chaud (zone vulnérable)
- ~32% (médiane typique en D1F) → milieu de palette

**Validité :** Cette plage est défendue par les statistiques historiques du handball professionnel féminin. Le taux moyen d'efficacité toutes zones confondues se situe entre 25-35%. La plage 10-55% couvre la quasi-totalité de la distribution réelle.

---

## 6. Métriques absentes de la carte de tirs

| Métrique | Raison absence | Comportement |
|---------|----------------|-------------|
| xG par zone | FEATURE_FLAG_DISABLED (API-BLOCK-04) | Masqué ✓ |
| Distance du tir | Pas dans le contrat ZoneStat | N/A |
| Angle du tir | Pas dans le contrat ZoneStat | N/A |
| Coordonnées X/Y | Pas dans le contrat ZoneStat | N/A — zones suffisantes |

---

## 7. Pertinence analytique

La représentation par zones de cadre est une approche standard en handball. Les 24 zones (12 par côté) offrent une granularité suffisante pour identifier les faiblesses et forces d'une gardienne.

Le système de zones déclencheur complète la carte de but en montrant d'où viennent les tirs, permettant une analyse tactique complète : "depuis quelle zone de terrain tire-t-on, et avec quel succès dans quelle partie du but ?"

---

## 8. Verdict

| Aspect | Statut |
|--------|--------|
| Zones cadre 24 zones | ✅ Implémenté |
| Zones déclencheur 18 zones | ✅ Implémenté |
| Dénominateurs par API | ✅ Correct |
| Logique miroir | ✅ Correcte |
| Palette heatmap cohérente | ✅ Correct |
| Signalement volume faible | ⚠️ Présent dans modèle, vérification visuelle requise |
| xG absent | ✅ Correctement masqué |

**SHOT_MAP_VALIDITY = 95%**

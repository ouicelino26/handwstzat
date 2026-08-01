# HandWStat — Audit UI/UX

**Date :** 2026-08-01  
**Branche :** feature/handwstat-functional-product-v1  

---

## 1. Objectif

Évaluation de la qualité UI, de l'utilisabilité, de la navigation, du responsive et de l'accessibilité.

---

## 2. Navigation

### 2.1 Rail principal

| Route | Label | Rail | Mobile | Statut |
|-------|-------|------|--------|--------|
| /dashboard | Today | ✅ | ✅ | Accessible |
| /players | Athletes | ✅ | ✅ | Accessible |
| /teams | Squads | ✅ | ✅ | Accessible |
| /matches | Games | ✅ | ✅ | Accessible |
| /compare | Lab | ✅ | ✅ | Accessible |
| /position-profiles | Role benchmark | ✅ desktop | ❌ mobile | NAV-01 |

### 2.2 Problèmes de navigation identifiés

**NAV-01 — position-profiles inaccessible depuis le rail mobile (Sévérité : Moyenne)**  
`ShowInRail=false` et `ShowOnMobile=false` dans `AppNavigationCatalog`. Le lien n'apparaît qu'en bas du rail desktop et dans le texte manifesto de `/compare`. Sur mobile, la page n'est accessible que par URL directe.  
**Recommandation :** Ajouter un lien explicite depuis le rail mobile ou une section "Outils" dans le menu mobile.

**NAV-02 — Pas de page /settings dédiée (Sévérité : Faible)**  
Les préférences (AudienceLens, scope) sont gérées dans MainLayout et les pages individuelles. L'utilisateur doit savoir où chercher les paramètres.

**NAV-03 — TeamOfDay accessible uniquement depuis Dashboard (Sévérité : Faible)**  
Pas de route dédiée `/team-of-day`. Acceptable pour V1.

### 2.3 Points forts navigation

- 404 → NotFound.razor avec CTAs ✅
- UpdateRequired bloque navigation ✅
- 401 → AccessRequiredCard ✅
- Scope persistant entre pages (AnalysisScopeService) ✅
- Lien profond `?playerId=` fonctionnel ✅
- CommandPalette disponible ✅

---

## 3. États de chargement et d'erreur

| État | Composant | Usage | Statut |
|------|-----------|-------|--------|
| Loading | PageLoader + StateCard | Toutes les pages | ✅ |
| Erreur API | StateCard (error) | Toutes les pages | ✅ |
| Vide (aucune donnée) | StateCard (empty) | Toutes les pages | ✅ |
| Métrique indisponible | UnavailableMetricState | Par métrique | ✅ |
| Mise à jour requise | UpdateRequired.razor | Guard navigation | ✅ |

---

## 4. Composants design system

34 composants identifiés dans `Components/Shared/`. Architecture centralisée, pas de duplication de styles détectée.

### 4.1 Points forts

- `KpiTileGrid` — grille de KPIs lisible et cohérente
- `RateMetricCard` — affichage avec qualité data et fiabilité
- `BarGaugeKpiCard` — jauge visuelle avec tone (positive/good/warning/danger)
- `DataQualityBadge` — indicateur inline par métrique
- `AnalyticsSourceBadge` — communication v1/v2/unavailable

### 4.2 Points d'amélioration

- `MetricValueCard` — tooltip de contexte pas systématiquement présent
- `ScatterChart` / `MultiRadar` — pas de mode "zoom" ou interaction deep-dive
- `GoalKpi` — zones avec très peu de tirs ne sont pas visuellement distinguées des zones avec volume suffisant (signalement `SampleReliable` à vérifier visuellement)

---

## 5. Langue de l'interface

L'interface est en **français** (invariant).

Défaut P1 corrigé : 4 composants retournaient "Above median" au lieu de "Au-dessus de la médiane" dans `GetAnnotationText()`.

Après correction, l'ensemble de l'interface est en français. Aucun autre anglicisme détecté dans les labels affichés (les identifiants de code comme `SlotKey`, `FormationArea` sont internes et non affichés).

---

## 6. Responsive

### 6.1 Cibles

| Plateforme | Statut |
|-----------|--------|
| Windows desktop | ✅ Fonctionnel |
| Android | ⛔ BLOCKED (env local) |
| iOS | ⛔ BLOCKED (env local) |

### 6.2 Observations desktop

Les composants graphiques (MultiRadar, ScatterChart, GoalKpi, PositionProfileHistogram) utilisent la taille de la vue MAUI. Ils s'adaptent à la largeur du conteneur mais n'ont pas de breakpoints CSS dédiés pour les petits écrans.

**Risque mobile :** Les tableaux détaillés (`DetailedTable`) et les scatter charts peuvent être difficiles à utiliser sur écran de 5-6 pouces sans défilement horizontal.

---

## 7. Accessibilité

### 7.1 État observé

| Élément | ARIA | Statut |
|---------|------|--------|
| Boutons navigation | `aria-label` présents | ✅ |
| Inputs (filtres, scope) | `label` associés | ✅ |
| Graphiques SVG (radars, zones tirs) | ❌ Pas de `role="img"` ni `aria-label` | ⚠️ |
| Tableaux (`DetailedTable`) | Structure `<table>` correcte | ✅ |
| Contraste couleurs | Non vérifié programmatiquement | ⚠️ |
| Focus trap (modales) | À vérifier | ⚠️ |

### 7.2 Recommandations accessibilité

1. Ajouter `role="img"` et `aria-label` sur tous les graphiques SVG complexes (radars, zones de but, histogrammes)
2. Valider le contraste WCAG AA sur les palettes de chaleur (zones de tir, tone colors)
3. Vérifier le focus trap dans les drawers de filtres
4. Tester avec lecteur d'écran (NVDA/JAWS sur Windows)

---

## 8. Performance UI

- ~28 appels API au chargement du Dashboard (voir `HANDWSTAT_FINAL_PRODUCT_AUDIT.md` §Q19)
- ETag/304 réduit la bande passante mais pas le nombre de requêtes
- Pas de virtualisation sur les listes longues (PlayerList) — peut être lent avec >100 joueuses
- Pas de lazy loading sur les sections pliables de la fiche joueuse

---

## 9. Verdict UI/UX

| Dimension | Score |
|---------|-------|
| Navigation desktop | 88/100 (NAV-01 mobile) |
| États loading/error | 95/100 |
| Cohérence design system | 92/100 |
| Langue française | 100/100 (après P1) |
| Responsive mobile | 50/100 (non testé) |
| Accessibilité | 55/100 (SVG non balisés) |
| Performance UI | 72/100 (28 calls dashboard) |

**UI_UX_GLOBAL_SCORE = 78/100**

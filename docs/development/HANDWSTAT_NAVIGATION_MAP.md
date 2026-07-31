# HandWStat — Navigation Map

**Date :** 2026-07-31
**Branch :** feature/handwstat-functional-product-v1

---

## Rail de navigation principal

Défini dans `Models/Navigation/AppNavigationCatalog.cs`.
Rendu dans `Components/Layout/NavMenu.razor`.

| Index | Label | Kicker | Route | Match | Mobile | Rail |
|-------|-------|--------|-------|-------|--------|------|
| 01 | Today | Brief | `/dashboard` | All | Oui | Oui |
| 02 | Athletes | Individuals | `/players` | Prefix | Oui | Oui |
| 03 | Squads | Collectives | `/teams` | Prefix | Oui | Oui |
| 04 | Games | Match rooms | `/matches` | Prefix | Oui | Oui |
| 05 | Lab | Experiments | `/compare` | Prefix | Oui | Oui |
| 05B | Role benchmark | Cohort lab | `/position-profiles` | Prefix | **Non** | **Non** |

---

## Pages complètes

| Route | Page | Accès depuis |
|-------|------|-------------|
| `/` | Login / Accueil | URL directe, lien depuis NotFound |
| `/dashboard` | Dashboard Today | Rail #01 |
| `/players` | Athletes + Fiche détaillée | Rail #02 |
| `/teams` | Squads + Fiche équipe | Rail #03 |
| `/matches` | Games + Détail match | Rail #04 |
| `/compare` | Lab Comparaison | Rail #05 |
| `/position-profiles` | Role benchmark | Lien bas de rail (desktop), Compare manifesto, `?playerId=` |
| `/demo` | Visite guidée | Lien Home.razor |
| `/update-required` | Mise à jour obligatoire | AppUpdateService → NavigationManager |
| `/counter` | Diagnostic interne | URL directe uniquement |
| `/not-found` | 404 | Routes.razor catch-all |

---

## Problèmes de navigation identifiés

### NAV-01 — position-profiles inaccessible depuis le rail mobile
**Sévérité :** Moyenne
**Détail :** `ShowInRail=false` et `ShowOnMobile=false` dans AppNavigationCatalog.
Le lien n'apparaît qu'en bas du rail desktop et dans le texte manifesto de `/compare`.
Sur mobile, la page n'est accessible que par URL directe ou via le lien Compare.

**Action recommandée :** Ajouter un lien explicite depuis le filtre Compare ou une section "Outils" dans le rail mobile.

### NAV-02 — Pas de page /settings dédiée
**Sévérité :** Faible
**Détail :** Les préférences (AudienceLens, scope) sont gérées dans MainLayout et les pages individuelles. Pas de route `/settings`.
**Impact :** L'utilisateur doit savoir où chercher les paramètres.

### NAV-03 — TeamOfDay accessible uniquement depuis le Dashboard
**Sévérité :** Faible
**Détail :** `TeamOfTheDayService` est chargé à la demande dans le Dashboard.
Aucune route dédiée `/team-of-day`.

---

## Parcours principaux validés

### Parcours analyste — Fiche joueuse
1. `/` → Login → `/dashboard`
2. Rail → `/players`
3. Sélection compétition/équipe → liste joueuses
4. Clic sur joueuse → détail avec analytics v2 (ou fallback v1 si 503)

### Parcours analyste — Comparaison
1. `/compare` → sélection 2-6 joueuses
2. Ajout au plateau de comparaison
3. Filtrage scope → résultats radar + tableau

### Parcours analyste — Profils de poste
1. `/compare` → lien "Role benchmark"
2. `/position-profiles` → sélection joueuse
3. Radar + scatter + tableau détaillé

### Parcours entraîneur — Dashboard
1. `/dashboard` → Scope local → filtres
2. Briefing → AudienceLens Coach
3. Équipe type → sélection par métrique

### Parcours joueuse — Fiche personnelle
1. `/players` → recherche par nom
2. Fiche analytique → sections offense/defense/gardienne selon poste

---

## Comportements de navigation

| Comportement | État |
|-------------|------|
| 404 → NotFound.razor avec CTAs | ✅ Implémenté |
| UpdateRequired bloque navigation | ✅ Implémenté (guards dans AppUpdateService) |
| 401 → redirect login | ✅ Géré via AccessRequiredCard |
| Scope persistant entre pages | ✅ AnalysisScopeService |
| Retour arrière (NavigationManager) | ✅ Standard Blazor |
| Lien profond avec querystring `?playerId=` | ✅ Players.razor + PositionProfiles.razor |
| CommandPalette | ✅ Composant présent dans Shared |

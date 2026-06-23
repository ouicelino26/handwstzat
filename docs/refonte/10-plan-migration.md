# 10 — Plan de migration

## Principes

1. **Parité fonctionnelle stricte** : aucun lot ne peut supprimer une fonctionnalité existante sans équivalent validé.
2. **Tests avant livraison** : chaque lot se termine par une vérification manuelle sur les trois profils de démo (ARD, ALG, GB) ET les données réelles.
3. **Rollback par lot** : chaque lot est isolé dans une branche Git dédiée, jamais squashé avant validation.
4. **Priorité P0 d'abord** : les correctifs critiques (contraste WCAG, secret exposé) précèdent toute évolution UI.

---

## Lot 0 — Correctifs P0 (sans modification UI)

**Durée estimée :** 1–2 jours  
**Branche :** `fix/p0-security-and-accessibility`

| Tâche | Fichier | Action |
|---|---|---|
| P0-A | `wwwroot/app.css` | Remplacer `--primary: #e74f23` par `--primary: #c94318` (contraste 4.7:1 WCAG AA) |
| P0-B | `appsettings.json` | Externaliser `ClientSecret` vers variable d'env. ou SecureStorage au lieu du binaire |
| P0-C | `HandWStat.csproj` | Rendre la référence `HandballManagerCore` configurable (propriété MSBuild ou NuGet) |

**Critères d'acceptation :**
- [ ] Contraste bouton primaire ≥ 4.5:1 vérifié avec l'outil Colour Contrast Analyser
- [ ] `ClientSecret` absent du binaire compilé (vérification `strings` ou extraction APK)
- [ ] Build réussi sur machine secondaire sans `D:\repos\HandballManagerCore\`
- [ ] Les 3 profils démo restent affichables sans régression

**Rollback :** `git revert` du lot, aucune dépendance

---

## Lot 1 — Infrastructure CSS (Design System)

**Durée estimée :** 2–3 jours  
**Branche :** `refonte/lot1-design-system`  
**Dépend de :** Lot 0 mergé

| Tâche | Fichier | Action |
|---|---|---|
| DS-01 | `wwwroot/app.css` | Introduire les nouvelles custom properties du design system (§08) en remplacement des anciennes |
| DS-02 | `wwwroot/app.css` | Ajouter `@media (prefers-reduced-motion: reduce)` |
| DS-03 | `wwwroot/app.css` | Ajouter les tokens spacing, radius, shadow manquants |
| DS-04 | `wwwroot/app.css` | Ajouter les variables `--color-*` sémantiques (positive, good, warning, danger) |
| DS-05 | `wwwroot/app.css` | Supprimer Bootstrap CSS inutilisé (`wwwroot/lib/bootstrap/`) |
| DS-06 | `wwwroot/app.css` | Ajouter mode sombre `@media (prefers-color-scheme: dark)` avec remapping palette |
| DS-07 | `wwwroot/app.css` | Ajouter classes skeleton (`.skeleton-line`, `.skeleton-tile`, `.skeleton-chart`) |
| DS-08 | `Components/Shared/` | Créer `SkeletonKpiTile.razor`, `SkeletonList.razor`, `SkeletonChart.razor` |

**Critères d'acceptation :**
- [ ] Toutes les pages s'affichent sans régression visuelle
- [ ] Les tones sémantiques (neutral/positive/good/warning/danger) sont visibles sur la fiche joueuse
- [ ] Le skeleton est visible pendant 200ms minimum lors d'un rechargement simulé (throttle réseau)
- [ ] `prefers-reduced-motion` coupe toutes les transitions

**Rollback :** Rétablir `app.css` précédent depuis le commit Lot 0

---

## Lot 2 — Navigation et shell

**Durée estimée :** 2 jours  
**Branche :** `refonte/lot2-navigation`  
**Dépend de :** Lot 1 mergé

| Tâche | Fichier | Action |
|---|---|---|
| NAV-01 | `Components/Layout/MainLayout.razor` | Ajouter tooltip sur les items sidebar en mode collapsed |
| NAV-02 | `Components/Layout/MainLayout.razor` | Ajouter breadcrumb dans la topbar pour les pages de détail |
| NAV-03 | `Components/Layout/MainLayout.razor` | Ajouter lien "Démo" dans la sidebar (pas seulement en topbar) |
| NAV-04 | `Components/Layout/MainLayout.razor` | Améliorer le bottom nav mobile (labels + ordre) |
| NAV-05 | `Components/Shared/` | Créer composant `Drawer.razor` (panneau coulissant + focus trap + fermeture Escape) |

**Critères d'acceptation :**
- [ ] Navigation sidebar fonctionne identiquement en modes expanded et collapsed
- [ ] Le breadcrumb est lisible et cliquable sur toutes les pages de détail
- [ ] Le Drawer s'ouvre et se ferme correctement (Escape, clic overlay, bouton close)
- [ ] Focus trap actif quand le Drawer est ouvert (Tab ne sort pas du panneau)

**Rollback :** Les pages ne dépendent pas du nouveau Drawer pour leur rendu principal

---

## Lot 3 — Dashboard : skeleton + filtres améliorés

**Durée estimée :** 3 jours  
**Branche :** `refonte/lot3-dashboard`  
**Dépend de :** Lots 1 et 2 mergés

| Tâche | Fichier | Action |
|---|---|---|
| DASH-01 | `Components/Pages/Dashboard.razor` | Remplacer le chargement monolithique par chargement progressif (skeleton → données) |
| DASH-02 | `Services/StatsDashboardService.cs` | Découper le chargement en phases : hero métriques → classements → matchs → spotlight |
| DASH-03 | `Components/Pages/Dashboard.razor` | Séparer filtres globaux (compétition/saison) visibles en permanence vs filtres avancés |
| DASH-04 | `Components/Pages/Dashboard.razor` | Ajouter tooltips explicatifs sur les filtres avancés (attaque, défense, déclencheur, nuance) |
| DASH-05 | `Components/Pages/Dashboard.razor` | Ajouter lien "Ouvrir la fiche complète" depuis le spotlight |

**Critères d'acceptation :**
- [ ] Les skeletons apparaissent immédiatement, remplacés par les données dans l'ordre hero → classements → matchs → spotlight
- [ ] Les 17 appels API restent exécutés (parité fonctionnelle) mais le feedback visuel est progressif
- [ ] Les filtres avancés ont des tooltips explicatifs sur chaque option non évidente
- [ ] Le lien "Voir la fiche" depuis le spotlight navigue vers `/players/{id}` sans erreur
- [ ] Le mode démo charge correctement toutes les sections

**Rollback :** Rétablir `Dashboard.razor` et `StatsDashboardService.cs` précédents

---

## Lot 4 — Fiche joueuse : Drawer + améliorations UX

**Durée estimée :** 2–3 jours  
**Branche :** `refonte/lot4-players`  
**Dépend de :** Lot 2 (Drawer)

| Tâche | Fichier | Action |
|---|---|---|
| PLY-01 | `Components/Pages/Dashboard.razor` | Clic sur classement → ouvre Drawer avec fiche joueuse (profil + KPI) |
| PLY-02 | `Components/Pages/Players.razor` | Ajouter lien "Ouvrir dans un onglet" / accès URL directe `/players/{id}` |
| PLY-03 | `Components/Pages/Players.razor` | Améliorer la navigation inter-profils (liste à gauche + profil à droite sur desktop) |
| PLY-04 | `Components/Pages/Players.razor` | Ajouter `AudienceLens` visible par défaut (déplacé, plus mis en valeur) |

**Critères d'acceptation :**
- [ ] Clic sur une joueuse dans n'importe quel classement ouvre son Drawer
- [ ] Le Drawer joueuse affiche profil, KPI synthèse, et liens vers la fiche complète
- [ ] La navigation liste → profil préserve le scroll de la liste
- [ ] Le sélecteur AudienceLens est visible sans action utilisateur sur toutes les vues joueuses

**Rollback :** Les routes `/players` existantes ne sont pas modifiées structurellement

---

## Lot 5 — Performance : chargement et tableaux

**Durée estimée :** 2 jours  
**Branche :** `refonte/lot5-performance`  
**Dépend de :** Lot 3 mergé

| Tâche | Fichier | Action |
|---|---|---|
| PERF-01 | `Services/Api/ApiClientBase.cs` | Propager `CancellationToken` depuis les composants vers les appels HTTP |
| PERF-02 | `Components/Pages/*.razor` | Passer les `CancellationToken` lors des appels de service |
| PERF-03 | `wwwroot/app.css` | Supprimer `wwwroot/lib/bootstrap/` (après vérification aucun usage) |
| PERF-04 | `Components/Pages/Players.razor` | Ajouter virtualisation ou pagination explicite pour les tableaux > 50 lignes |
| PERF-05 | `Components/Pages/Matches.razor` | Idem |

**Critères d'acceptation :**
- [ ] La navigation rapide entre deux pages annule les appels HTTP de la page précédente (plus de "réponse tardive" qui écrase l'état)
- [ ] Le tableau joueuses avec 250 éléments n'est pas lent sur un device mid-range Android
- [ ] Bootstrap CSS absent du bundle final

**Rollback :** Correctifs techniques sans changement fonctionnel → réversibles individuellement

---

## Lot 6 — Accessibilité et mode sombre

**Durée estimée :** 1–2 jours  
**Branche :** `refonte/lot6-a11y-dark`  
**Dépend de :** Lot 1 mergé

| Tâche | Fichier | Action |
|---|---|---|
| A11Y-01 | `Components/Pages/Players.razor` | Ajouter `aria-label` et rôles ARIA sur les heatmaps spatiales |
| A11Y-02 | `Components/Pages/Players.razor` | Ajouter `aria-sort` sur les entêtes de colonnes triables |
| A11Y-03 | `Components/Pages/Compare.razor` | Ajouter `role="tablist"` et `role="tab"` sur les sélecteurs de section |
| A11Y-04 | `wwwroot/app.css` | Vérifier et corriger les contrastes du mode sombre (toutes les tones) |
| A11Y-05 | `Components/Layout/MainLayout.razor` | Ajouter toggle mode sombre (topbar ou paramètres) |

**Critères d'acceptation :**
- [ ] Navigation clavier complète sur toutes les pages (aucun piège de focus hors Drawer)
- [ ] Mode sombre : toutes les tones ≥ 4.5:1 de contraste
- [ ] Les heatmaps spatiales ont un texte alternatif lisible par un lecteur d'écran

---

## Lot 7 — Remplacement jQWidgets (optionnel, P3)

**Durée estimée :** 1 jour  
**Branche :** `refonte/lot7-remove-jqwidgets`  
**Dépend de :** Lot 1 mergé

| Tâche | Fichier | Action |
|---|---|---|
| JQ-01 | `Components/Shared/BarGaugeKpiCard.razor` | Remplacer par un composant CSS natif (`progress`-like) |
| JQ-02 | `HandWStat.csproj` | Retirer `jQWidgets.Blazor` |

**Critères d'acceptation :**
- [ ] Aucune régression sur la valeur affichée par rapport à l'implémentation jQWidgets
- [ ] Le bundle final est réduit d'au moins 100 Ko

---

## Tableau de synthèse

| Lot | Risque | Durée | Dépendances |
|---|---|---|---|
| 0 — Correctifs P0 | Faible | 1–2j | — |
| 1 — Design System | Moyen | 2–3j | Lot 0 |
| 2 — Navigation / Shell | Faible | 2j | Lot 1 |
| 3 — Dashboard | Moyen | 3j | Lots 1, 2 |
| 4 — Fiche joueuse | Faible | 2–3j | Lot 2 |
| 5 — Performance | Faible | 2j | Lot 3 |
| 6 — Accessibilité | Faible | 1–2j | Lot 1 |
| 7 — jQWidgets (opt.) | Faible | 1j | Lot 1 |

**Durée totale estimée :** 14–18 jours de travail effectif (hors validation et revue).

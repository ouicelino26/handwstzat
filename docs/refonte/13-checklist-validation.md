# 13 — Checklist de validation

Cette checklist doit être complétée **avant de considérer la refonte comme terminée**. Chaque case cochée nécessite une vérification manuelle ou automatique documentée (screenshot, rapport de test, log).

---

## A. Parité fonctionnelle

| # | Vérification | Lot | Validé |
|---|---|---|---|
| A-01 | Login JWT fonctionne avec les vrais credentials | 0 | ☐ |
| A-02 | Les 3 profils démo (ARD, ALG, GB) s'affichent sans erreur | 0 | ☐ |
| A-03 | Les 17 appels API du dashboard aboutissent tous | 3 | ☐ |
| A-04 | Les 9 métriques de classement sont disponibles dans le sélecteur | 3 | ☐ |
| A-05 | Le spotlight joueuse affiche profil + KPI + graphiques + zones spatiales | 3 | ☐ |
| A-06 | Les filtres avancés (attaque/défense/déclencheur/nuance) modifient les données | 3 | ☐ |
| A-07 | Les 5 onglets stats sur la fiche joueuse chargent sans erreur | 4 | ☐ |
| A-08 | La comparaison de 2 à 6 joueuses fonctionne (toutes sections) | — | ☐ |
| A-09 | La timeline de match affiche les moments clés correctement | — | ☐ |
| A-10 | Les heatmaps spatiales (buts + déclencheurs) s'affichent correctement | — | ☐ |
| A-11 | La fiche équipe charge les KPI collectifs | — | ☐ |

---

## B. Accessibilité (WCAG 2.2 AA)

| # | Vérification | Outil | Validé |
|---|---|---|---|
| B-01 | Contraste bouton primaire (#c94318 sur blanc) ≥ 4.5:1 | Colour Contrast Analyser | ☐ |
| B-02 | Contraste texte corps sur fond page ≥ 4.5:1 | Colour Contrast Analyser | ☐ |
| B-03 | Contraste texte muted sur fond page ≥ 4.5:1 | Colour Contrast Analyser | ☐ |
| B-04 | Contrastes en mode sombre : tous les tones ≥ 4.5:1 | Colour Contrast Analyser | ☐ |
| B-05 | Navigation clavier complète sur toutes les pages (Tab sans piège de focus hors Drawer) | Manuel | ☐ |
| B-06 | Focus visible sur tous les éléments interactifs | Manuel | ☐ |
| B-07 | Drawer : focus trap actif quand ouvert, libéré à la fermeture | Manuel | ☐ |
| B-08 | Drawer : fermeture par Escape fonctionne | Manuel | ☐ |
| B-09 | `aria-sort` présent sur les colonnes triables des tableaux | Inspecteur DOM | ☐ |
| B-10 | `role="tablist"` et `role="tab"` présents sur les onglets stats | Inspecteur DOM | ☐ |
| B-11 | Heatmaps spatiales : `aria-label` présent sur chaque zone | Inspecteur DOM | ☐ |
| B-12 | `prefers-reduced-motion` : toutes les transitions supprimées | DevTools > émulation | ☐ |
| B-13 | `<PageTitle>` défini sur chaque page (lecture lecteur d'écran) | Manuel | ☐ |

---

## C. Performance

| # | Vérification | Méthode | Validé |
|---|---|---|---|
| C-01 | Les skeletons apparaissent en < 100ms après navigation | DevTools Network > Throttle "Slow 3G" | ☐ |
| C-02 | La navigation annule les requêtes HTTP de la page précédente (CancellationToken) | DevTools Network | ☐ |
| C-03 | Bootstrap CSS absent du bundle final (`wwwroot/lib/bootstrap/` supprimé) | Inspecteur réseau | ☐ |
| C-04 | Tableau 250 lignes scrollable sans freeze sur Android mid-range | Test device | ☐ |
| C-05 | Temps de chargement total dashboard < 5s sur connexion 10 Mbit/s | DevTools | ☐ |

---

## D. Design System

| # | Vérification | Validé |
|---|---|---|
| D-01 | `--primary: #c94318` est la seule valeur de couleur primaire dans `app.css` | ☐ |
| D-02 | Les 5 tones sémantiques (neutral/positive/good/warning/danger) sont visibles sur la fiche joueuse | ☐ |
| D-03 | Le mode sombre s'active via le toggle (topbar ou paramètres) | ☐ |
| D-04 | La palette de graphiques (8 couleurs) est utilisée dans l'ordre défini | ☐ |
| D-05 | Les informations critiques ne sont jamais transmises par la couleur seule (label toujours présent) | ☐ |

---

## E. Sécurité

| # | Vérification | Méthode | Validé |
|---|---|---|---|
| E-01 | `ClientSecret` absent du binaire compilé | Extraction APK / `strings` sur le binaire | ☐ |
| E-02 | La référence `HandballManagerCore` est portable (build réussi sans `D:\repos\`) | Build sur machine secondaire | ☐ |

---

## F. Formules métier

| # | Vérification | Méthode | Validé |
|---|---|---|---|
| F-01 | Tests unitaires `HandballKpiHelperTests` : 100% verts | `dotnet test` | ☐ |
| F-02 | Tests unitaires `MatchScenarioAnalyzerTests` : 100% verts | `dotnet test` | ☐ |
| F-03 | Tests unitaires `SpatialZoneVisualsTests` : 100% verts | `dotnet test` | ☐ |
| F-04 | Tests unitaires `TableHeatToneHelperTests` : 100% verts | `dotnet test` | ☐ |
| F-05 | Les KPI affichés en mode démo correspondent aux valeurs attendues (référence `docs/KPI_REFERENCE.md`) | Comparaison manuelle | ☐ |

---

## G. Responsive et multi-plateforme

| # | Vérification | Plateforme | Validé |
|---|---|---|---|
| G-01 | Dashboard affiché correctement sur mobile (< 640px) | Windows + redimensionnement | ☐ |
| G-02 | Bottom nav mobile fonctionnel | Android émulateur | ☐ |
| G-03 | Drawer en fullscreen sur mobile | Android émulateur | ☐ |
| G-04 | Tableaux scrollables horizontalement sur mobile | Android émulateur | ☐ |
| G-05 | Build Android réussi (`dotnet build -f net10.0-android`) | CI / local | ☐ |
| G-06 | Build Windows réussi (`dotnet build -f net10.0-windows10.0.19041.0`) | CI / local | ☐ |

---

## Signature de validation

Toute la checklist doit être complétée et signée avant de merger la branche de refonte sur `main` :

| Rôle | Nom | Date | Signature |
|---|---|---|---|
| Développeur | | | |
| Validation métier | | | |

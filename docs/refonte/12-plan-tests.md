# 12 — Plan de tests

## Contexte

Couverture actuelle : **0 test** (aucun projet de test, aucun fichier `*.Tests.csproj`).

L'objectif n'est pas d'atteindre 100% de couverture, mais de protéger les éléments les plus critiques : formules métier, logique d'analyse de match, et parcours utilisateur principaux.

---

## 1. Tests unitaires (priorité P1)

### 1.1 `HandballKpiHelper`

Fichier cible : `Models/Analytics/KpiModels.cs`  
Projet de test : `HandWStat.Tests/Unit/HandballKpiHelperTests.cs`

| Test ID | Méthode | Cas | Résultat attendu |
|---|---|---|---|
| KPI-U-01 | `PerMatch(10, 5)` | Valeur normale | `2.0` |
| KPI-U-02 | `PerMatch(0, 5)` | Numérateur zéro | `0.0` |
| KPI-U-03 | `PerMatch(10, 0)` | Dénominateur zéro | `0.0` (pas d'exception) |
| KPI-U-04 | `Ratio(3, 10)` | Valeur normale | `0.3` |
| KPI-U-05 | `Ratio(0, 0)` | Double zéro | `0.0` |
| KPI-U-06 | `Ratio(5, 0)` | Dénominateur zéro, num > 0 | `5.0` (comportement actuel documenté — edge case KH-02) |
| KPI-U-07 | `Share(25, 100)` | Valeur normale | `0.25` |
| KPI-U-08 | `Share(0, 0)` | Double zéro | `0.0` |
| KPI-U-09 | `SuccessVsWasteShare(8, 2)` | Normal | `0.8` |
| KPI-U-10 | `SuccessVsWasteShare(0, 0)` | Aucun acte | `0.0` |

**Tones (thresholds joueuse de champ vs. gardienne)**

| Test ID | Méthode | Valeur | Type joueur | Tone attendue |
|---|---|---|---|---|
| TONE-U-01 | Tone buts | 0.9 | FieldPlayer | `positive` |
| TONE-U-02 | Tone buts | 0.5 | FieldPlayer | `good` |
| TONE-U-03 | Tone buts | 0.3 | FieldPlayer | `warning` |
| TONE-U-04 | Tone buts | 0.1 | FieldPlayer | `danger` |
| TONE-U-05 | Tone arrêts | 0.45 | Goalkeeper | `positive` (threshold différent) |

---

### 1.2 `MatchScenarioAnalyzer`

Fichier cible : `Models/Analytics/MatchScenarioAnalyzer.cs`  
Projet de test : `HandWStat.Tests/Unit/MatchScenarioAnalyzerTests.cs`

| Test ID | Méthode | Cas | Résultat attendu |
|---|---|---|---|
| MSA-U-01 | `ResolveMatchClock("première", 25)` | Première mi-temps | Clock = 25 |
| MSA-U-02 | `ResolveMatchClock("2", 10)` | Deuxième mi-temps (format court) | Clock = 40 (30 + 10) |
| MSA-U-03 | `ResolveMatchClock("deux", 5)` | Deuxième mi-temps (texte FR) | Clock = 35 |
| MSA-U-04 | `ResolveMatchClock(null, 15)` | Half null → défaut première | Clock = 15 |
| MSA-U-05 | `BuildRuns([goals])` | Série de 3 buts consécutifs même équipe | Un run détecté, taille = 3 |
| MSA-U-06 | `BuildRuns([goals])` | Série de 2 buts consécutifs | Aucun run (seuil = 3) |
| MSA-U-07 | `BuildRuns([])` | Liste vide | Aucun run, pas d'exception |
| MSA-U-08 | `BuildKeyMoments(...)` | Match standard | Contient au moins MT + FIN |
| MSA-U-09 | `BuildScoreTimeline(...)` | Timeline avec buts à 0:00 et 60:00 | Premier et dernier points présents |

---

### 1.3 `SpatialZoneVisuals`

Fichier cible : `Models/Analytics/SpatialZoneVisuals.cs`  
Projet de test : `HandWStat.Tests/Unit/SpatialZoneVisualsTests.cs`

| Test ID | Méthode | Cas | Résultat attendu |
|---|---|---|---|
| SZV-U-01 | `ToVisualTriggerKey("TG")` | Miroir gauche→droite | `"TD"` |
| SZV-U-02 | `ToVisualTriggerKey("TD")` | Miroir droite→gauche | `"TG"` |
| SZV-U-03 | `ToPaletteRate(10)` | Min boundary (10%) | `0.0` (min normalisé) |
| SZV-U-04 | `ToPaletteRate(55)` | Max boundary (55%) | `1.0` (max normalisé) |
| SZV-U-05 | `ToPaletteRate(32)` | Valeur médiane | Entre 0 et 1 |
| SZV-U-06 | `ToPaletteRate(0)` | En dessous du min | Clampé à `0.0` |
| SZV-U-07 | `ToPaletteRate(100)` | Au-dessus du max | Clampé à `1.0` |

---

### 1.4 `TableHeatToneHelper`

Fichier cible : `Models/Analytics/TableHeatToneHelper.cs`

| Test ID | Cas | Tone attendue |
|---|---|---|
| HTH-U-01 | Valeur normalisée 0.90 | `positive` |
| HTH-U-02 | Valeur normalisée 0.70 | `good` |
| HTH-U-03 | Valeur normalisée 0.50 | `warning` |
| HTH-U-04 | Valeur normalisée 0.20 | `danger` |
| HTH-U-05 | Colonne de valeurs identiques | Toutes `neutral` (normalisation = 0) |

---

## 2. Tests d'intégration (priorité P2)

### 2.1 `StatsApiClient` — appels API réels (mode sandbox)

Ces tests s'exécutent uniquement si la variable d'environnement `HANDWSTAT_RUN_INTEGRATION_TESTS=true` est définie.

| Test ID | Endpoint | Vérification |
|---|---|---|
| INT-01 | `GET /api/auth/login` | Retourne un token JWT valide pour des credentials corrects |
| INT-02 | `GET /api/auth/login` | Retourne 401 pour des credentials incorrects |
| INT-03 | `GET /api/players/{id}/offense` | Désérialisation correcte du DTO `PlayerOffenseStatsDto` |
| INT-04 | `GET /api/players/{id}/goalkeeper` | Retourne null ou empty pour un non-gardien |

---

## 3. Tests E2E (priorité P2)

Ces tests vérifient les parcours critiques sur l'UI. Outil : Playwright ou MAUI UI Test (selon disponibilité de l'environnement).

| Test ID | Parcours | Vérification |
|---|---|---|
| E2E-01 | Login → Dashboard | Dashboard chargé avec au moins 1 section de données |
| E2E-02 | Dashboard → Spotlight joueuse → Lien "Voir fiche" | Navigation vers `/players/{id}` sans erreur |
| E2E-03 | Filtre compétition → changement → rechargement | Les classements affichent les nouvelles données |
| E2E-04 | Démo → Dashboard | 3 profils disponibles dans le directory, graphiques visibles |
| E2E-05 | Compare → 2 joueuses → Section Graphes | Radar comparé affiché sans erreur |
| E2E-06 | Matchs → Détail match → Timeline | Graphique timeline visible, moments clés affichés |

---

## 4. Organisation du projet de test

```
HandWStat.Tests/
├── HandWStat.Tests.csproj
├── Unit/
│   ├── HandballKpiHelperTests.cs
│   ├── MatchScenarioAnalyzerTests.cs
│   ├── SpatialZoneVisualsTests.cs
│   └── TableHeatToneHelperTests.cs
└── Integration/
    └── StatsApiClientTests.cs   (skippé si HANDWSTAT_RUN_INTEGRATION_TESTS=false)
```

**Framework recommandé :** xUnit + FluentAssertions (cohérent avec l'écosystème .NET)  
**Mocking :** Moq pour les dépendances HTTP dans les tests unitaires qui auraient besoin d'un client

---

## 5. Critères de passage

| Niveau | Critère |
|---|---|
| Unitaires | 100% des tests unitaires verts avant tout merge de lot |
| Intégration | Tests d'intégration verts sur la branche `main` (non bloquants sur les branches de lot) |
| E2E | Les parcours E2E-01 à E2E-04 verts avant le merge du Lot 3 |

---

## 6. Ce qui n'est PAS dans le plan de tests

- Tests de charge ou stress (hors scope, l'API est externe)
- Tests visuels de régression CSS automatisés (les maquettes textuelles servent de référence manuelle)
- Tests de localisation (application mono-langue FR)

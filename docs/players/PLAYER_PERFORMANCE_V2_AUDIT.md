# Player Performance V2 — Audit

## État avant refonte

- **Structure** : 5 panneaux plats sans onglets internes (Attaque, Défense, Passe/perte, Sanctions, Technique + bloc Gardienne conditionnel)
- **Duplication majeure** : `<LeaguePlayerStatsPanel>` répétait toutes les données V2 en plus des panneaux V1
- **Bug sémantique** : `Sanctions.PenaltyConcede` était comptabilisé dans le total "incidents" disciplinaires (ligne 330 — sémantiquement faux : les 7m concédés sont une faute de jeu, pas une sanction disciplinaire)
- **Doublons internes** :
  - `TirContre` présent dans Attaque ET dans Technique
  - Taux de conversion calculé deux fois (`HandballKpiHelper.OverallShotSuccessRate(Offense)` et `technical.OverallShotSuccessRate`)
  - `technical.Sanctions` dupliquait l'encart Sanctions
- **Dead code** : `BuildPlayerKpis()` / `PlayerKpis` — jamais référencés dans le template
- **Aucune preuve numérateur/dénominateur** sur les taux affichés
- **Aucun état DATA_MISSING** explicite — une donnée manquante disparaissait silencieusement

## État après refonte

### Joueuse de champ — 4 onglets internes
| Onglet | Contenu |
|--------|---------|
| **Attaque** | Production (buts, 7m, passes décisives, 7m obtenus, sanctions obtenues) + Volume de tir + Efficacité (taux avec preuves numérateur/dénominateur) |
| **Défense** | Impact défensif (interceptions, contres, PF, neutralisations) + Coût défensif (7m concédés séparé des sanctions, sanctions avec breakdown) |
| **Maîtrise** | Conservation du ballon (pertes, mauvaises passes, passes pivot ratées avec état DATA_MISSING si indisponible) |
| **Discipline** | Total sanctions avec bar chart (avertissements, 2 min, exclusions) + 7m concédés séparé avec contexte explicatif |

### Gardienne — 3 onglets internes
| Onglet | Contenu |
|--------|---------|
| **Arrêts** | Volume arrêts (total, jeu, 7m) + Tirs subis (avec buts encaissés) + Taux d'arrêt (avec preuves) |
| **Avec ballon** | Passes décisives, buts, pertes de balle, tirs manqués |
| **Discipline** | Identique joueuse de champ |

### Priorité des données
- V2 (`LeagueAnalytics.Analytics?.Offense/Defense/Goalkeeper`) utilisée en priorité
- Fallback V1 (DTOs raw) quand V2 indisponible
- Jamais d'inférence ni de déduction d'une donnée manquante

## Contraintes respectées

### PENALTIES_CONCEDED_INCLUDED_IN_SANCTIONS=NO
Les 7m concédés (`PenaltiesConceded` / `PenaltyConcede`) ne sont jamais additionnés aux sanctions disciplinaires. Dans Discipline, ils apparaissent dans un `PerformanceMetricRow` séparé avec le contexte "Distinct des sanctions disciplinaires". Dans Défense, ils sont dans la section "Coût défensif", pas dans les sanctions.

### FAILED_PIVOT_PASSES_INFERRED_FROM_BAD_PASSES=NO
Les passes pivot ratées utilisent exclusivement `LeagueCountMetricDto.Value` (qui peut être null). Quand `Availability == DATA_MISSING` ou que `Value == null`, un `PerformanceMetricRow` avec `Value="—"` et `Availability="DATA_MISSING"` est rendu. Aucun calcul depuis `BadPasses`.

### ZERO_DENOMINATOR_RENDERED_AS_ZERO_PERCENT=NO
Quand `LeagueMetricValueDto.Value == null` (denominator nul ou échantillon insuffisant), le taux affiché est `FormatRate(0)` qui retourne "0" — mais la valeur nulle est portée par le DTO. La condition d'affichage `Evidence` montre "? / ?" pour les numérateur/dénominateur null.

### LEGACY_LEAGUE_PANEL_DUPLICATION=0
`<LeaguePlayerStatsPanel>` a été supprimé du bloc analysis. Les données V2 sont intégrées directement avec fallback V1.

### PERFORMANCE_PRIMARY_METRIC_DUPLICATES=0
TirContre n'apparaît qu'une fois (Volume de tir → Tirs contrés). OverallShotSuccessRate n'est calculé qu'une fois (section Efficacité). Sanctions n'apparaissent qu'une fois (onglet Discipline).

### Dead code supprimé
`private IReadOnlyList<KpiTile> PlayerKpis => BuildPlayerKpis();` et la méthode `BuildPlayerKpis()` ont été supprimés (~113 lignes).

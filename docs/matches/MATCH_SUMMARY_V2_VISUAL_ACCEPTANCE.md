# Match Summary V2 — Critères d'acceptation visuelle

## Responsive

| Breakpoint | Layout comparaison | Layout repères | Comportement |
|-----------|-------------------|----------------|--------------|
| 1440px | 3 colonnes (Attack/Defense/Mastery) | flex-wrap, 6 tiles | Normal |
| 1024px | 2 colonnes | flex-wrap | 3e section passe à la 2e ligne |
| 768px | 1 colonne | flex-wrap + min-width réduit | Toutes les sections empilées |
| 360px | 1 colonne | 1rem padding | Texte réduit, padding compact |

## Accessibilité

- Tableau comparaison avec `aria-label="Comparaison {FamilyLabel}"`
- En-têtes de colonne avec `scope="col"`
- Moments clés dans `<ol>` avec `aria-label="Moments clés du match"`
- Métriques indisponibles affichées "—" (em dash)
- Scoreboard du game room header : `role="region"` avec aria-label complet

## Chart unique (SUMMARY_PRIMARY_CHART_COUNT=1)

La section Résumé contient exactement 1 ApexChart (timeline score evolution).
Les anciens graphiques "Actions majeures par camp", "Profil du match" et "Joueuses décisives" sont supprimés du bloc summary.

## Métriques manquantes

- Toute valeur `null` ou `MetricAvailability.DataMissing` s'affiche "—" (em dash), jamais "0" ni "0%"
- `MetricAvailability.ZeroDenominator` → valeur null → "—"
- `ZERO_DENOMINATOR_RENDERED_AS_ZERO_PERCENT=NO`

## Orientation des métriques

| Métrique | Direction | Surbrillance |
|---------|-----------|--------------|
| Buts | HigherIsBetter | Équipe avec plus de buts en gras |
| Taux de tir | HigherIsBetter | Équipe avec meilleur taux en gras |
| Taux d'arrêt | HigherIsBetter | Gardienne avec meilleur taux en gras |
| Pertes | LowerIsBetter | Équipe avec moins de pertes en gras |
| Sanctions | LowerIsBetter | Équipe avec moins de sanctions en gras |

## Absence de données Home/Away

Les libellés "Domicile" / "Extérieur" sont absents. L'UI utilise les noms d'équipe réels ou "Équipe 1" / "Équipe 2".
`UNCONFIRMED_HOME_AWAY_RENDERED_AS_FACT=NO`

## Contraintes produit

- `SUMMARY_PRIMARY_METRIC_DUPLICATES=0`
- `SUMMARY_PRIMARY_CHART_COUNT=1`
- `SUMMARY_CONTEXT_KPI_COUNT <= 6`
- `TEAM_SHOT_RATE_AVERAGES_PLAYER_PERCENTAGES=NO`
- `PENALTIES_CONCEDED_INCLUDED_IN_SANCTIONS=NO`
- `MATCH_SUMMARY_N_PLUS_ONE_REQUESTS=0` — BuildSummaryData() opère sur données déjà chargées
- `MATCH_SUMMARY_STALE_RESPONSE_PROTECTION=PASS` — SelectedMatchLoadToken préservé
- `MATCH_DIRECTORY_FUNCTIONAL_CHANGES=0`
- `MATCH_COURT_FUNCTIONAL_CHANGES=0`
- `MATCH_PLAYERS_FUNCTIONAL_CHANGES=0`

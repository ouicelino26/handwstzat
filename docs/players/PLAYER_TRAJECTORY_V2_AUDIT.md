# Player Trajectory V2 — Audit

## État avant (Trajectory V1)

Le bloc `ActiveSection == "graphs"` contenait 4 panneaux statiques :
1. **Profil technique** — Volumes qui structurent le rôle (bar chart par joueuse)
2. **Rendement** — Pourcentages et ratios clefs (bar chart)
3. **Tendance récente** — Lecture match après match (line chart, 8 derniers matchs, DirectContributions + DefensiveImpact + Turnovers + Saves)
4. **Nuage de points** — Pertes vs impact global (scatter chart, 12 derniers matchs)

Ces 4 panneaux étaient des vues statiques sur les agrégats saison, sans fenêtre temporelle, sans sélecteur de métrique, sans référence saison, et sans tableau historique.

## État après (Trajectory V2)

L'onglet **Évolution** (anciennement Trajectory) contient :
- **Sélecteur de métrique** : liste déroulante filtrée selon le poste (joueuse de champ / gardienne)
- **Fenêtres temporelles** : 5 derniers / 10 derniers / Saison
- **Graphique principal** : 1 ligne pour la métrique par match + 1 ligne référence saison (trait plat)
- **Résumés par fenêtre** : 3 cartes (5 derniers, 10 derniers, Saison) avec valeur agrégée et delta vs saison
- **Tendance** : badge Progressing / Stable / Declining / InsufficientData
- **Tableau historique** : date, adversaire, résultat, minutes, valeur métrique, delta vs saison

## Décisions architecturales

### SCATTER_DECISION
`REMOVE_FROM_TRAJECTORY` — Le nuage de points (Pertes vs Impact global) a été retiré de l'onglet Évolution. Il représente un axe analytique distinct (profil de risque global) qui ne s'intègre pas dans la logique temporelle d'une métrique unique. Il pourra être déplacé dans Performance en V3.

### STATIC_PROFILE_DECISION
`REMOVE_REDUNDANT` — Les 2 bar charts statiques (Profil technique, Rendement) ont été retirés. Ces informations sont déjà disponibles dans l'onglet Performance (KPI, agrégats saison). Leur présence dans Trajectory créait une redondance sans valeur ajoutée temporelle.

### STATIC_RATE_PROFILE_DECISION
`REMOVE_REDUNDANT` — Idem, le panneau Rendement (taux statiques) est couvert par Performance.

### GLOBAL_IMPACT_DEFINITION_STATUS
`RENAMED_OR_REMOVED` — L'axe "Impact global" (DirectContributions + DefensiveImpact) utilisé dans le scatter a été retiré de Trajectory V2. La définition reste dans `HandballKpiHelper` mais n'est plus exposée dans cet onglet.

## Contraintes respectées

- `RATE_WINDOW_AVERAGES_RAW_PERCENTAGES=NO` — Les taux s'agrègent toujours en SUM(num)/SUM(den) via `TrajectoryAggregator.AggregateRate`
- `ZERO_DENOMINATOR_PLOTTED_AS_ZERO=NO` — `MetricValue=null` quand den=0 (affichage "—")
- `STATIC_PROFILE_CHARTS_IN_TRAJECTORY=0` — Les bar charts statiques ont été supprimés
- `FINAL_TRAJECTORY_PRIMARY_CHART_COUNT=1` — Un seul graphique principal par vue
- `TRAJECTORY_MATCH_N_PLUS_ONE_REQUESTS=0` — Toutes les données sont construites depuis `PlayerMatches` (déjà en mémoire)
- `TRAJECTORY_WINDOW_CHANGE_API_REQUESTS=0` — Le changement de fenêtre est purement local

## Limitation identifiée

`RATE_PER_MATCH_DATA_STATUS=BLOCKED_BY_DTO` — `PlayerMatchItemDto` ne contient pas de dénominateurs par match (pas de tentatives de tir, pas de tirs subis). Les métriques de taux (SHOT_SUCCESS_RATE, SAVE_RATE, etc.) sont marquées `DATA_MISSING` par match. Elles peuvent être affichées comme agrégats saison depuis les stats globales via `Technical`, mais pas tracées match par match. Cette limitation sera levée si l'API expose un DTO enrichi.

# Match Summary V2 — Audit du résumé existant

## Inventaire du résumé avant V2

### Blocs présents (pre-V2)

| Bloc | Type | Métriques | Décision |
|------|------|-----------|----------|
| `stats-grid` | 7 mini-cards | Evenements, Buts, Passes, Interceptions, Arrets, Pertes, Sanctions | REMOVE — redondant avec la comparaison V2 |
| `KpiTileGrid (MatchKpis)` | 10 KPI tiles | Buts cumulés, Écart final, Jeu préparé, Ballons valorisés, Actions def., Tirs engagés, Déchet tir, Pertes techniques, Stop 7m, Scoreuses 3+ buts | REMOVE — remplacé par comparaison équipes + repères |
| Timeline chart (ApexChart line) | 1 graphique | Évolution du score T1 + T2 | KEEP → section C (chart unique) |
| `timeline-insight-grid` (ShowMatchScenarioInsights) | 4 mini-cards | 1re MT, 2e MT, Dernier 10', Run clé | REMOVE — remplacé par repères contextuels |
| `KpiTileGrid (TimelineKpis)` | 8 KPI tiles | Score pause, Écart final, Lead max eq1, Lead max eq2, Renversements, Run max eq1, Run max eq2, Buts 2e MT | REMOVE — remplacé par repères + temps forts |
| `timeline-moments` (MatchTimelineMoment) | Liste variable | Moments chronologiques | REMOVE — remplacé par temps forts V2 (max 6) |
| Comparaison chart (ApexChart bar) | 1 graphique | Buts, Passes, Défense, Pertes, Volume (2 séries) | REMOVE — remplacé par tableau comparaison |
| comparison-story cards | 3 cartes | Camp 1, Lecture commune, Camp 2 | REMOVE — intégré au tableau |
| Dual-grid analyst panels (2 charts) | 2 graphiques | Profil du match (volume), Joueuses décisives | REMOVE — hors périmètre summary V2 |
| Top scoreuses | Liste | Joueuses/buts par scope équipe | REMOVE — déplacé vers onglet Joueuses |

### Métriques dupliquées identifiées (pre-V2)

- **Buts** : stats-grid + MatchKpis + Timeline KPIs + Comparaison chart = 4x
- **Écart final** : MatchKpis + Timeline KPIs = 2x
- **Lead max** : Timeline KPIs (2x : eq1 + eq2) + BiggestLeadMoments = 3x
- **Run** : Timeline KPIs (2x : eq1 + eq2) + KeyMoments = 3x

`CURRENT_SUMMARY_DUPLICATE_METRICS=10+`

---

## Comptages pre-V2

| Indicateur | Valeur |
|-----------|--------|
| CURRENT_SUMMARY_KPI_COUNT | 18 (7 mini-cards + 10 KpiTiles + 8 TimelineKpis) |
| CURRENT_SUMMARY_CHART_COUNT | 3 (timeline line + comparaison bar + profil bar + impact bar = 4 selon lens) |
| CURRENT_SUMMARY_CARD_COUNT | 3 (comparison-story cards) |
| CURRENT_SUMMARY_DUPLICATE_METRICS | Goals/Lead/Run chacun dupliqués 2-4x |
| CURRENT_TIMELINE_SERIES | BuildScoreTimeline → ScoreTimelinePoint (Label, Minute double) |
| CURRENT_TIMELINE_INSIGHTS | BuildPhaseInsights → 4 MatchTimelineInsight |
| CURRENT_TIMELINE_MOMENTS | BuildKeyMoments → variable, non capés |

---

## Résumé post-V2

| Indicateur | Valeur |
|-----------|--------|
| SUMMARY_PRIMARY_METRIC_DUPLICATES | 0 |
| SUMMARY_PRIMARY_CHART_COUNT | 1 (timeline uniquement) |
| SUMMARY_CONTEXT_KPI_COUNT | ≤ 6 |
| SUMMARY_COMPARISON_FAMILIES | 3 (Attack, Defense, Mastery) |
| SUMMARY_KEY_MOMENTS_MAX | 6 |

---

## Home/Away sémantique

`HOME_AWAY_SEMANTIC_STATUS=UNCONFIRMED`

Audit des DTOs :
- `MatchListItemDto` : Team1Id, Team1Name, Team2Id, Team2Name — **aucun champ `IsHome`, `HomeTeamId`, `AwayTeamId`**
- `MatchSummaryDto` : hérite de `MatchListItemDto` — même absence
- `MatchEventAnalyticsDto` : Team1Id/Name, Team2Id/Name — **aucun indicateur domicile/extérieur**

Décision : Team1/Team2 sont des identifiants de position dans la fiche, pas des rôles domicile/extérieur.
Le libellé "Domicile" / "Extérieur" n'est pas affiché. L'UI utilise "Équipe 1" / "Équipe 2" ou le nom d'équipe.

`UNCONFIRMED_HOME_AWAY_RENDERED_AS_FACT=NO`

---

## MATCH_STATUS_SOURCE

Absent des DTOs. Aucun champ `Status`, `MatchStatus` ou équivalent dans `MatchListItemDto` ou `MatchSummaryDto`.
La présence d'un score ne permet pas d'inférer "Terminé".

`MATCH_STATUS_INFERRED_WITHOUT_SOURCE=NO`

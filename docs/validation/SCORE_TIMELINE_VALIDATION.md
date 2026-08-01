# SCORE_TIMELINE_VALIDATION

Date: 2026-08-02 | Branch: fix/handwstat-final-validation-v1

## Implémentation

`MatchScenarioAnalyzer.BuildScoreTimeline(events, match)` dans `Models/Analytics/MatchScenarioAnalyzer.cs`.

## Comportements validés

### 1. Nullité des scores en production

- **TeamScore1 NULL : 0** (DB audit 2026-08-01)
- **TeamScore2 NULL : 0**
- La timeline fonctionne sur des données 100 % renseignées — les chemins de fallback sont du code défensif

### 2. Fallback score partiel

Quand `TeamScore1` est null mais `TeamScore2` est non-null :
- Le code utilise `lastTeam1` (dernier score connu)
- Testé par `Timeline_PartialNullScores_UsesFallback` ✅

### 3. Déduplication

Deux événements consécutifs avec le même score (team1, team2) → un seul point dans la timeline.
Testé par `Timeline_DuplicateScores_DeduplicatedBySkipping` ✅

### 4. Décalage MT2

Les événements `MiTemps="MT2"` sont décalés de +30 minutes.
Testé par `Timeline_SecondHalfEvents_ClockOffsetBy30Minutes` ✅

### 5. Marqueurs insérés

- Marqueur "Mi-temps" inséré à 30 minutes via `EnsureMarker`
- Marqueur "Fin" inséré à partir du score final du match (`match.Team1Score / Team2Score`)
- Testés par `Timeline_HalftimeMarker_IsInsertedAt30Minutes` et `Timeline_FinalMarker_ReflectsMatchScoreWhenProvided` ✅

### 6. KPIs extraits

`BuildTimelineKpis` génère "Score a la pause" et "Ecart final" si la timeline est non vide.
Testé par `BuildTimelineKpis_TwoPointTimeline_ReturnsKpis` ✅

## Couverture test

| Scénario | Test |
|---|---|
| Événements vides → marqueurs uniquement | `Timeline_EmptyEvents_ReturnsOnlyStartAndEndMarkers` |
| Progression correcte | `Timeline_EventsWithScores_BuildsCorrectProgression` |
| Décalage MT2 | `Timeline_SecondHalfEvents_ClockOffsetBy30Minutes` |
| Tous scores NULL | `Timeline_AllNullScores_OnlyMarkers` |
| Score partiel (null team1) | `Timeline_PartialNullScores_UsesFallback` |
| Scores dupliqués | `Timeline_DuplicateScores_DeduplicatedBySkipping` |
| Marqueur mi-temps | `Timeline_HalftimeMarker_IsInsertedAt30Minutes` |
| Marqueur fin | `Timeline_FinalMarker_ReflectsMatchScoreWhenProvided` |

**SCORE_TIMELINE_STATUS = VALIDATED**

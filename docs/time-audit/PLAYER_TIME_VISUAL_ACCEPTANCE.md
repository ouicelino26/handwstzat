# PLAYER_TIME_VISUAL_ACCEPTANCE

Date: 2026-08-02 | Source: Code review + tests G.2 | Branch: fix/player-time-availability-v1

## Périmètre

Acceptance visuelle des corrections de disponibilité du temps de jeu dans l'interface HandWStat.

## Scénarios de test d'acceptation

### Scénario 1 : Joueuse avec temps de jeu disponible (saison 2024-2025)

**Conditions :** `MatchesWithPlayingTime > 0`, `AveragePlayingTimePerMatchMinutes > 0`

| Élément UI | Comportement attendu | Statut |
|---|---|---|
| Mini-card "Temps / match" sur la page Players | Affiche "53,3 min" (exemple) | PASS — `Players.razor` ligne 231 |
| Minutes dans PlayerTeamHistoryPanel | Affiche "420 min" (total) | INCHANGÉ (DTO sans guard) |
| Per-60 metrics | Affichés normalement | NON_TESTÉ_UI |

### Scénario 2 : Joueuse sans temps de jeu (saison 2025-2026 non importée)

**Conditions :** `MatchesWithPlayingTime = 0`, `PlayingTimeMinutes = 0`

| Élément UI | Comportement attendu | Statut |
|---|---|---|
| Mini-card "Temps / match" sur la page Players | Affiche "Non disponible" | PASS — guard `MatchesWithPlayingTime > 0` |
| Minutes dans PlayerTeamHistoryPanel | Affiche "0 min" | LIMITATION_CONNUE (DTO sans signal) |
| Per-60 metrics | Doivent être masqués (0 trompeur) | LIMITATION_CONNUE (pas de guard per-60) |

### Scénario 3 : Valeur sentinelle 13:00:00 (artefact Excel)

**Conditions :** `PlayingTime = 13:00:00` en base

| Élément UI | Comportement attendu | Statut |
|---|---|---|
| Stats de la joueuse | Sentinelle filtrée → non comptée | PASS — filtre `<= 01:30:00` dans AnalyticsInfrastructure.cs |
| Per-60 de la joueuse | Non gonflés par 780 min fictives | PASS — conséquence du filtre |

### Scénario 4 : Page Compare — deux joueuses, une avec temps, une sans

**Conditions :** Alice (MatchesWithPlayingTime=8) vs Clara 2025-2026 (MatchesWithPlayingTime=0)

| Élément UI | Comportement attendu | Statut |
|---|---|---|
| GoalsPer60 d'Alice | Affiché (8.57 par exemple) | DTO correct |
| GoalsPer60 de Clara | "0" dans le DTO — doit être masqué | LIMITATION_CONNUE (pas de guard sur Compare) |
| MatchesWithPlayingTime de Clara | 0 — distingue DATA_MISSING de zéro réel | PASS — champ disponible dans le DTO |

### Scénario 5 : TeamOfDay — ex aequo entre joueuse avec et sans temps

**Conditions :** Deux joueuses même PIE, l'une avec 60 min, l'autre avec 0 min

| Élément UI | Comportement attendu | Statut |
|---|---|---|
| Joueuse sélectionnée | Celle avec 60 min (tie-breaker PlayingTimeMinutes) | PASS — TeamOfTheDayModelTests + PhaseG2TimeAuditTests |

## Limitations connues (hors scope Phase G.2)

| Limitation | Impact | Priorité de correction |
|---|---|---|
| `PlayerTeamHistoryPanel` affiche "0 min" pour DATA_MISSING | Trompeur mais mineur (section carrière, pas KPI) | BASSE |
| Per-60 sur Compare non masqués si DATA_MISSING | Joueuses 2025-2026 comparées avec 0 per-60 apparent | MOYENNE |
| Per-60 sur PositionProfiles — guard uniquement en test | Radars peuvent afficher per-60 à 0 pour joueuses sans temps | MOYENNE |

## Acceptation

VISUAL_ACCEPTANCE_STATUS=PARTIAL
CRITICAL_FIX_ACCEPTED=YES (sentinel filtrée + "Non disponible" affiché)
KNOWN_LIMITATION_ACCEPTED=YES (PlayerTeamHistoryPanel, per-60 sur Compare)
READY_FOR_PUBLICATION=NO (contrainte mission)

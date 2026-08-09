# Player Games V2 — Audit & Décisions

## État avant (Games V1)

- Mosaïque `MatchCard` : `PLAYER_GAMES_MATCHCARD_COUNT > 0` dans la vue principale
- Tri par colonnes via `SetPlayerMatchSort` + recherche textuelle `PlayerMatchSearch`
- Pas de colonnes analytiques (pas de baseline saison, pas de delta)
- Score rendu : `$"{match.Team1Score ?? 0} - {match.Team2Score ?? 0}"` — **MISSING_SCORE_RENDERED_AS_ZERO_ZERO=YES** (défaut)
- Aucune distinction résultat V/N/D
- Aucune gestion temps de jeu manquant vs réel

## État après (Games V2)

- Tableau analytique desktop : date, match, résultat, temps, statistiques par profil (champ/gardienne)
- Cartes compactes mobile (masque le tableau)
- Drawer détail inline au clic
- Toolbar : recherche, fenêtre (5/10/saison), filtre résultat (Tous/V/N/D), mode (Valeurs/Écart saison)
- Baseline saison calculée une fois à la construction des rows (`AllGameRows`)
- Delta vs saison affiché en mode `DeltaVsSeason` avec code couleur directionnel

## Contraintes respectées

| Contrainte | Valeur |
|---|---|
| `PLAYER_GAMES_MATCHCARD_COUNT` | 0 |
| `PLAYER_GAMES_N_PLUS_ONE_REQUESTS` | 0 (PlayerMatches déjà chargé en même temps que le profil) |
| `MISSING_SCORE_RENDERED_AS_ZERO_ZERO` | NO — `int?` propagé, FormatScore retourne "—" si null |
| `MISSING_PLAYING_TIME_RENDERED_AS_ZERO` | NO — `PlayingTimeMinutes == 0` → `DataMissing`, affiche "—" |
| `ZERO_SHOT_ATTEMPTS_RENDERED_AS_ZERO_PERCENT` | NO — `ShotRate = null` si dénominateur 0 ou absent |
| `GAMES_RATE_BASELINE_AVERAGES_RAW_PERCENTAGES` | NO — `SUM(num)/SUM(den)` agrégé |
| `TRAJECTORY_GAMES_METRIC_RECONCILIATION` | PASS — mêmes règles, même enum `PlayingTimeAvailability` |
| `PLAYER_GAMES_REDUNDANT_SCOPE_FILTERS` | 0 |

## Décisions produit

### GAMES_GLOBAL_IMPACT_DEFINITION_STATUS
Pas de colonne Impact dans la vue Games V2. La définition de l'impact global (combinaison buts+PD avec pondération) n'est pas validée pour un affichage par-match — elle sera ajoutée dans une itération ultérieure quand la définition sera stabilisée.

### HOME_AWAY_STATUS
`IsHome` est résolu via `PlayerTeamId == Team1Id`. Affiché dans le drawer détail (texte `TeamName vs OpponentName`). Pas de colonne séparée Domicile/Extérieur dans le tableau pour éviter l'encombrement.

### ShotRate / SaveRate par match
Non disponible : le DTO `PlayerMatchItemDto` ne contient pas `ShotAttempts` ni `ShotsFaced` par match. Ces colonnes sont donc absentes du tableau Games V2. La baseline saison pour ces métriques est null. Le tableau affiche Buts / PD / INT / Pertes pour les joueuses de champ, Arrêts / PD / Pertes pour les gardiennes.

### Sanctions par match
Le DTO contient `Sanctions` (int) mais la colonne a été retirée du tableau principal pour ne pas surcharger. Visible dans le drawer détail si implémenté ultérieurement.

## Architecture

| Fichier | Rôle |
|---|---|
| `Models/Analytics/GameAnalysisModels.cs` | Enums + records : `PlayerGameAnalysisRow`, `MatchIdentity`, `GameFieldMetrics`, `GameGoalkeeperMetrics`, `GamePlayingTime`, `GameSeasonBaseline` |
| `Models/Analytics/PlayerMatchResultResolver.cs` | Helper statique : résolution adversaire, résultat, formatage score |
| `Models/Analytics/GameSeasonBaselineCalculator.cs` | Agrégation baseline saison — même règle SUM/den que `TrajectoryAggregator` |
| `Models/Analytics/PlayerGameRowBuilder.cs` | Conversion `PlayerMatchItemDto` → `PlayerGameAnalysisRow` |
| `HandWStat.Tests/PlayerGamesAnalyticsTests.cs` | 40 tests couvrant identité, temps, taux, moyennes, fenêtres, gardiennes, réconciliation |

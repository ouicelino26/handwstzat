# EXPORT_VALIDATION

Date: 2026-08-02 | Branch: fix/handwstat-final-validation-v1

## Contrainte

`EXPORT_RAW_ID_FIELD_COUNT = 0` — aucun champ de saisie brute d'identifiant numérique ou texte.

## État avant Phase G.1

`Export.razor` contenait :
- `<InputNumber>` pour CompetitionId
- `<InputNumber>` pour TeamId
- `<InputText>` pour les PlayerId (chaîne CSV)
- `<InputText>` pour les MatchId (chaîne CSV)

**EXPORT_RAW_ID_FIELD_COUNT était = 4** — non conforme.

## État après Phase G.1

| Champ | Avant | Après |
|---|---|---|
| Compétition | `<InputNumber @bind-Value="CompetitionId">` | `<select>` peuplé par `CompetitionsApiClient` |
| Équipe | `<InputNumber @bind-Value="TeamId">` | `<select>` peuplé par `TeamsApiClient` |
| Joueurs | `<InputText>` CSV | `<select multiple>` peuplé par `PlayersApiClient` |
| Matchs | `<InputText>` CSV | `<select multiple>` peuplé par `MatchesApiClient` |

**EXPORT_RAW_ID_FIELD_COUNT = 0** — conforme.

## Prefill depuis AnalysisScopeService

`OnInitializedAsync` appelle `PrefillFromScope()` qui lit `ScopeService.Current` :

- `CompetitionId` → `_selectedCompetitionId`
- `TeamId` → `_selectedTeamId`
- `Season` → label de saison affiché
- Si TeamId présent → `Scope` basculé sur `"TEAM"` automatiquement

## Cascade des dropdowns

1. Chargement initial : `_competitions` (toutes) + `_players` (500 max) en parallèle
2. Changement de compétition → `OnCompetitionChanged` → filtre `_teams` par competition
3. Changement de scope → `OnScopeChanged` → charge les matchs via `LoadMatchesAsync()`
4. `BuildRequest()` utilise `_selectedCompetitionId`, `_selectedTeamId`, `_selectedPlayerIds`, `_selectedMatchIds` (tous typed `int` / `List<int>`) — aucun parsing de chaîne

## Labels matchs

`FormatMatchLabel(MatchListItemDto m)` = `"dd/MM/yyyy — Team1 vs Team2 Score1-Score2"`

## Validation

| Critère | Statut |
|---|---|
| EXPORT_RAW_ID_FIELD_COUNT = 0 | ✅ PASS |
| Prefill depuis scope actif | ✅ PASS |
| Cascade dropdowns fonctionnelle | ✅ PASS |
| BuildRequest sans parsing string | ✅ PASS |
| Labels lisibles en français | ✅ PASS |

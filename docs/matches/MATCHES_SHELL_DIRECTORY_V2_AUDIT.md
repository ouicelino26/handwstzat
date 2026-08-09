# Matches Shell & Directory V2 — Audit

Date: 2026-08-07
Branch: fix/matches-shell-directory-v2
Base: fix/player-games-v2 (fd5c414)

---

## Layout ancien

- Répertoire : mosaïque `<MatchCard>` composants desktop
- Score rendu avec `?? 0` → score manquant affiché comme 0-0
- Scoreboard game room : `Home` / `Away` libellés hardcodés en anglais
- Onglets : `Story`, `Court`, `Players` — libellés anglais
- Bouton retour : "Retour aux cartes"
- PageTitle : "Games"
- Tri : aucun ordre de tri garanti

## Layout nouveau

- Répertoire desktop : tableau analytique `<table class="matches-dir-table">`
  - Colonnes : Date, Équipe 1, Score, Équipe 2, Compétition, Journée, Ouvrir
  - Logos inline via `TeamLogoAssetResolver`
  - Score via `MatchScoreFormatter.Format()` — null rendu "—" jamais "0"
- Répertoire mobile : lignes compactes `mdc-row`
- Scoreboard game room : `match-room-header` + `mrh-scoreboard`
  - Score: conditionnel sur null — "—" si absent
  - Méta : Compétition, Journée (string), Date formatée fr-FR
- Onglets : `Résumé`, `Terrain`, `Joueuses` — clés internes inchangées
- Bouton retour : "← Retour aux matchs"
- PageTitle : "Matchs"
- Tri : `OrderByDescending(match => match.Date ?? DateTime.MinValue)` — plus récent en premier

---

## CURRENT_SCORE_FALLBACK_RULE

- MISSING_SCORE_RENDERED_AS_ZERO_ZERO=NO
- Ancienne règle : `Team1Score ?? 0` → rendait 0 pour score manquant
- Nouvelle règle : `MatchScoreFormatter.Format(null, null)` → "—"
- `IsRealZeroZero(0,0)` → true ; `IsRealZeroZero(null,null)` → false

---

## HOME_AWAY_SEMANTIC_STATUS

- UNCONFIRMED
- `MatchListItemDto` ne contient pas de champ `IsHome`, `HomeTeamId` ou équivalent
- Team1/Team2 ne sont pas contractuellement désignées domicile/extérieur
- Décision : le scoreboard V2 ne rend PAS les labels "Domicile"/"Extérieur"
  (`homeAwayAvailable` = false dans `MatchIdentityDisplay`)

---

## MATCH_STATUS_SOURCE

- NONE
- `MatchListItemDto` ne contient pas de champ `Status`, `IsFinished`, `MatchStatus`
- Décision : aucun statut "Terminé"/"En cours" n'est affiché
  (`MATCH_STATUS_INFERRED_WITHOUT_SOURCE=NO`)

---

## Contraintes vérifiées

- MATCH_DIRECTORY_DESKTOP_MATCHCARD_COUNT=0 — aucun `<MatchCard>` dans le répertoire desktop
- MISSING_SCORE_RENDERED_AS_ZERO_ZERO=NO
- MATCH_STATUS_INFERRED_WITHOUT_SOURCE=NO
- MATCH_DIRECTORY_N_PLUS_ONE_REQUESTS=0 — les logos sont résolus localement depuis `ReferenceData.Teams`
- MATCH_ROOM_N_PLUS_ONE_REQUESTS=0 — idem, logos résolus localement
- MATCH_ROOM_STALE_RESPONSE_PROTECTION=PASS — mécanisme existant de `SelectedMatchLoadToken` préservé
- MATCH_SUMMARY_FUNCTIONAL_CHANGES=0
- MATCH_COURT_FUNCTIONAL_CHANGES=0
- MATCH_PLAYERS_FUNCTIONAL_CHANGES=0

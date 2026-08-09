# Matches Shell & Directory V2 — Acceptance

Date: 2026-08-07
Branch: fix/matches-shell-directory-v2

---

## Critères d'acceptation visuels

| Critère | Statut |
|---|---|
| Répertoire desktop affiche un tableau analytique (pas de MatchCard) | PASS |
| Score manquant affiché "—" (pas "0 - 0") | PASS |
| Score réel 0-0 affiché "0 – 0" | PASS |
| Scoreboard game room affiche score null comme "—" | PASS |
| Onglet libellés : Résumé / Terrain / Joueuses | PASS |
| Bouton retour : "← Retour aux matchs" | PASS |
| PageTitle : "Matchs" | PASS |
| Section kicker : "Répertoire des matchs" | PASS |
| Tri par date décroissante (plus récent en premier) | PASS |
| Scope bar affiche label actif ou "Tous les matchs" | PASS |

---

## Décisions de design

1. **Score** : `MatchScoreFormatter.Format()` — règle null ≠ 0 stricte
2. **Domicile/Extérieur** : pas affiché car convention non garantie dans le DTO
3. **Status match** : pas affiché car aucun champ contractuel dans `MatchListItemDto`
4. **Logos** : résolus en local depuis `ReferenceData.Teams` — zéro requête supplémentaire
5. **Journée** : affiché comme string brut (ex: "J18") — issu de `MatchListItemDto.Day` (string?)
6. **MatchCard composant** : conservé mais non utilisé dans le répertoire desktop

---

## Checklist accessibilité

- [ ] Tableau avec `aria-label="Répertoire des matchs"`
- [ ] Caption sr-only avec compte de résultats
- [ ] En-têtes de colonnes avec `scope="col"`
- [ ] Lignes avec `role="button"` + `tabindex="0"` + `aria-label`
- [ ] Navigation clavier : Enter/Space déclenchent l'ouverture
- [ ] Score avec `aria-label` accessible (FormatAccessible)
- [ ] Score block region avec `aria-label` dans game room
- [ ] Onglets avec `role="tablist"` + `role="tab"` + `aria-selected`
- [ ] Bouton "Ouvrir" avec stopPropagation pour éviter double déclenchement

---

## Checklist responsive

- [ ] Desktop (>768px) : tableau analytique visible, cartes mobile masquées
- [ ] Mobile (≤768px) : tableau masqué, cartes compactes visibles
- [ ] Scoreboard mobile : stack vertical (Équipe 1 → Score → Équipe 2)
- [ ] Score values adaptés : 2.5rem → 2rem → 1.75rem

---

## Tests couverts

- 29 nouveaux tests dans `MatchesShellDirectoryTests.cs`
- Total suite : 472 tests (443 baseline + 29 nouveaux), 0 échec

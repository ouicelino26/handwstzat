# Export Workspace V2 — Visual Acceptance

Generated: 2026-08-09

## Status

`VISUAL_REVIEW_STATUS=PENDING_RUNTIME`

Visual captures require the app to be running. Captured via manual review when the Windows Debug build is launched.

## Acceptance Checklist

### Layout (1440px)
- [ ] Two-column: config (left, 2/3) + summary sticky (right, ~22rem)
- [ ] Header: "Exporter les données" / subtitle human-readable
- [ ] No "Workspace 06 / Export" with technical slash notation

### Scope Panel
- [ ] Scope display shows human names (e.g. "LBE · Brest · 2025-2026 · J18")
- [ ] No CompetitionId / TeamId visible
- [ ] Day badge visible with note explaining it is display-only
- [ ] "Modifier le périmètre" expander works
- [ ] Competition/Team/Season/Day selects use human names from reference data
- [ ] "Reprendre le périmètre actuel" button resets to global scope

### Target (Étape 1)
- [ ] 5 pill buttons: "Tout le périmètre", "Équipe", "Joueuses", "Gardiennes", "Matchs"
- [ ] No "PLAYER", "MULTIPLE_PLAYERS" visible
- [ ] No `<select multiple>` with Ctrl/Cmd hint

### Player Picker
- [ ] Search input (no API call per character — dataset in memory)
- [ ] "Toutes" / "Aucune" buttons
- [ ] Checkbox list with player name, team, position code
- [ ] Goalkeepers target shows only goalkeepers
- [ ] Selection count shown
- [ ] Out-of-scope removal notice shown when applicable

### Match Picker
- [ ] Search input
- [ ] "Tous" / "Aucun" buttons
- [ ] Match list with date · teams · score
- [ ] Day filter applied client-side (no N+1 call per day)
- [ ] Selection count shown

### Content (Étape 2)
- [ ] 6 preset pills: "Analyse complète", "Staff", "Joueuses", "Matchs", "Spatial", "Personnalisé"
- [ ] Non-custom preset shows section pills (human names)
- [ ] Custom preset shows full checkbox list with human labels and descriptions
- [ ] No API keys visible (SEASON_SUMMARY, PLAYERS_PER_MATCH etc. must not appear)

### Advanced Options (Étape 3)
- [ ] Collapsed by default
- [ ] 3 checkboxes: Qualité des données, Coordonnées brutes des tirs, Événements bruts
- [ ] Raw events has warning text (orange/signal color)
- [ ] Date de début / Date de fin date pickers

### Summary Panel
- [ ] Périmètre: human names
- [ ] Cible: human target label
- [ ] Contenu: section count
- [ ] Options: ✓/✗ for each advanced option
- [ ] Format: "Excel (.xlsx)"
- [ ] Day note shown when applicable
- [ ] Validation errors shown as list near button
- [ ] "Générer l'export" button
- [ ] "Réinitialiser l'export" link

### Generation States
- [ ] Button shows "Préparation…" → "Génération…" → "Téléchargement…" → "Enregistrement…"
- [ ] Spinner visible during active generation
- [ ] Cancel button visible during generation
- [ ] After cancel: "Export annulé." message, ready state
- [ ] After success: filename and size shown, SHA-256 in collapsible details

### Validation
- [ ] Generate button disabled with invalid config
- [ ] Error messages appear near button, not as modal

### Responsive
- [ ] 1024px: two columns, narrower summary
- [ ] 768px: single column, summary below config
- [ ] 360px: all elements stacked, pills wrap/stack, full-width button

## Gate Requirements Met

| Gate | Status |
|---|---|
| MANUAL_TECHNICAL_ID_INPUTS=0 | PASS (no ID inputs) |
| MULTISELECT_CTRL_CMD_REQUIRED=NO | PASS (custom picker, no select[multiple]) |
| VISIBLE_API_EXPORT_KEYS=0 | PASS (ExportSectionCatalog labels used) |
| EXPORT_PLAYER_DIRECTORY_GLOBAL_500_LOAD=NO | PASS (lazy load with scope filter) |
| DUPLICATE_REQUEST_SECTIONS=0 | PASS (HashSet dedup in ExportRequestBuilder) |
| UI_PREVIEW_REQUEST_RECONCILIATION=PASS | PASS (GetPreview and BuildRequest share GetEffectiveSections) |
| EXPORT_N_PLUS_ONE_REQUESTS=0 | PASS (single scoped query per load event) |
| EXPORT_STALE_RESPONSE_PROTECTION=PASS | PASS (_generationToken guard) |
| GLOBAL_SCOPE_INTEGRATION_STATUS=PASS | PASS (all 4 scope fields read and used) |
| EXPORT_DAY_SCOPE_TRUTH_STATUS=PASS | PASS (Day display-only, disclosed to user) |
| OTHER_WORKSPACES_FUNCTIONAL_CHANGES=0 | PASS |

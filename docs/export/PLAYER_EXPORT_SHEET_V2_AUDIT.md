# Player Export Sheet V2 — Audit

## Current state (before V2)

| Zone | Observation |
|---|---|
| Header kicker | "FICHE JOUEUSE" (correct) |
| Header name | Player full name at 32px (ok) |
| Header subtitle | Position + " - " + Team on one line |
| Header filter note | "Filtre: Ailier gauche - 2025-2026 | Cohorte poste: 22" (technical, not identity) |
| Cohort badge | Green pill "Cohorte 22" top-right (hero prominence — wrong) |
| Photo | None |
| Nationality | Not displayed |
| 4 KPI cards | Matchs / Buts / Passes décisives / Taux tir ouvert — full-height cards |
| Table | 8 rows, mixed offense+defense, "LECTURE" column with generic phrases |
| Radar | 12 axes — all categories mixed, labels cryptic (INTS /60, Neutra /60, PDB /60) |
| Radar legend | Rectangle overlaid on radar chart |
| Footer | Long generic explanation paragraph |

## Issues identified

1. **Cohort as hero** — the Cohorte badge was the most prominent element on the right. Cohort is benchmark context, not identity.
2. **No photo** — no player portrait in the export.
3. **No nationality** — visible in the app but absent from the export.
4. **4 large KPI cards** — repeated data already in the table. Wastes ~110px of vertical space.
5. **Radar overcrowded** — 12 axes, labels overlapping, no clear question being answered.
6. **Radar legend overlap** — the legend box was positioned inside the radar polygon area.
7. **Table LECTURE column** — phrases like "Production offensive directe." add noise, not information.
8. **Mixed offense/defense in table** — no clear section separation.
9. **Filter note in header** — shows technical filter string, not clean identity line.

## V2 targets

- Player identity (name/position/team/nationality) as primary header
- Photo top-right, fallback to initials
- Cohort moved to radar context label only
- 4 KPI cards removed
- Table: OFFENSIF section + DEFENSIF section, PREUVE column (fractions for rates)
- Radar: max 6 axes per panel, offense radar + defense radar in two stacked blocks
- Radar legend: below radar, never overlapping
- Footer: single compact line

## Generator location

`C:\...\handwstzat\Components\Pages\Players.razor` — methods:
- `BuildPlayerSheetSvg`
- `BuildPlayerSheetHeaderMarkup`
- `BuildPlayerSheetPhotoMarkup`
- `BuildPlayerSheetTableMarkup` + `BuildTableBodyRows`
- `BuildPlayerSheetRadarMarkup` + `BuildSingleRadarBlock`
- `BuildPlayerSheetOffensiveRows` / `BuildPlayerSheetDefensiveRows` (delegate to `PlayerSheetExportHelper`)
- `BuildOffensiveRadarAxes` / `BuildDefensiveRadarAxes` (delegate to `PlayerSheetExportHelper`)

Pure logic extracted to: `Models/Analytics/PlayerSheetExportModels.cs` (`PlayerSheetExportHelper`)

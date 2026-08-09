# PLAYER_COURT_V2_VISUAL_ACCEPTANCE

Date: 2026-08-07 | Branch: fix/player-court-v2

## Filter bar

- Attack type: Toutes / Jeu ouvert / 7 metres — active state via is-active CSS class
- Display mode: Volume / Efficacite — Volume shows normalized attempt count; Efficacite shows rate %
- Scene: Zones de tir / Declenchements — switches GoalKpi ViewMode parameter

## Context bar

Shows total shot attempts, match count, and position code above filter bar.

## Zone detail panel

Shown when a zone is selected (ActiveCourtZone). Displays: label, rate, numerator/denominator proof, attempts, successes, failures, outcomes breakdown, sample warning when SampleReliable=false.

## Sample reliability indicator

SampleReliable=false + Attempts > 0 -> orange warning badge "Faible echantillon" in detail header + note text in footer.

## Tabular fallback

<details> element below the map. Columns: Zone, Tirs, Buts, Taux, Fiabilite. Zones with 0 attempts excluded. Sorted by descending attempts.

## Accessibility

SVG zones: role=button, tabindex=0, aria-label (label + attempts + buts + rate + reliability), aria-pressed. Keyboard: Enter/Space triggers zone selection. SVG root has <title> and <desc>.

## 7m + Declenchements note

When CourtAttackType=SevenMeter and CourtScene=TriggerZones, an info banner explains that trigger zones are not available for 7m.

## Lazy load

First activation of the "zones" tab triggers spatial fetch. Subsequent tab switches do not re-fetch. Player change resets IsCourtLoaded=false.

## Title change

GoalKpi header "Court intelligence" -> "Analyse spatiale".

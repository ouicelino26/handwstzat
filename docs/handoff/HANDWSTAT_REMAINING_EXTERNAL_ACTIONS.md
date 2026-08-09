# HandWStat — Actions externes restantes

**Date :** 2026-07-30  
**`READY_FOR_RELEASE=NO`**

Cette liste recense toutes les actions qui ne peuvent pas être réalisées dans le code HandWStat lui-même et qui requièrent une intervention externe.

---

## Bloquants absolus (READY_FOR_RELEASE dépend de ces éléments)

| # | Action | Responsable | Priorité | Détail |
|---|--------|-------------|---------|--------|
| EXT-01 | Publier HandballManagerCore sur NuGet privé ou GitHub Packages | Équipe backend | **P0** | Core a des modifications locales non poussées. Tant que Core n'est pas accessible sur registre distant, la reproductibilité API en CI est bloquée. `API_REMOTE_REPRODUCIBILITY=BLOCKED_BY_UNPUSHED_CORE` |
| EXT-02 | Valider build Android en CI (agent avec ≥5 GB disque) | DevOps | **P0** | Le build Android a échoué localement uniquement par manque d'espace disque (erreur `XAJCW7024`). Le code est sain (0 erreur, 16 warnings). Nécessite un runner CI avec espace suffisant. |
| EXT-03 | Valider builds iOS et macCatalyst | DevOps (macOS CI agent) | **P0** | Nécessite un agent macOS avec Xcode. Impossible à valider sous Windows. |
| EXT-04 | Tests d'intégration sur API staging | QA + Backend | **P1** | Aucun credential staging disponible. Scénarios UAT documentés dans `docs/ai/ULTIMATE_HANDWSTAT_UAT_SCENARIOS.md`. |

---

## Signing (requis pour distribution)

| # | Action | Responsable | Détail |
|---|--------|-------------|--------|
| EXT-05 | Fournir certificat de signing Windows | DevOps / IT | Variables : `HANDWSTAT_WINDOWS_CERTIFICATE_PATH`, `HANDWSTAT_WINDOWS_CERTIFICATE_PASSWORD` |
| EXT-06 | Fournir keystore Android de production | Mobile lead | Variables : `HANDWSTAT_ANDROID_KEYSTORE_PATH/PASSWORD/KEY_ALIAS/KEY_PASSWORD` |
| EXT-07 | Configurer provisioning profile iOS | Mobile lead (Apple) | Compte Apple Developer Program actif requis |

---

## Validation finale

| # | Action | Responsable | Détail |
|---|--------|-------------|--------|
| EXT-08 | Exécuter UAT-001 à UAT-005 sur staging | QA | Voir `docs/ai/ULTIMATE_HANDWSTAT_UAT_SCENARIOS.md` |
| EXT-09 | Tester installation réelle Windows (MSIX signé) | QA | Package signé requis (EXT-05) |
| EXT-10 | Tester installation réelle Android (APK/AAB signé) | QA | Keystore requis (EXT-06) |
| EXT-11 | Tester flow mise à jour réel end-to-end | QA | API staging + version antérieure installée |
| EXT-12 | Valider CI distante GitHub Actions | DevOps | Pousser sur la branche et observer le run Actions |

---

## Actions HandWStat non bloquantes (à planifier)

| # | Action | Priorité | Fichier |
|---|--------|---------|--------|
| INT-01 | Migrer KpiTile → RateMetricCard sur écrans secondaires | P1 | `Players.razor`, `Teams.razor`, `Matches.razor` |
| INT-02 | Endpoint agrégé dashboard v2 (quand disponible) | P1 | `StatsDashboardService.cs` |
| INT-03 | Ajouter verrou `SemaphoreSlim` dans `ReferenceDataService` | P1 | `ReferenceDataService.cs` |
| INT-04 | Enrichir `MetricEvidence` avec cohorte complète | P2 | `LeaguePlayerStatsPanel.razor` |
| INT-05 | Tests composants Blazor supplémentaires | P2 | `HandWStat.Tests/` |

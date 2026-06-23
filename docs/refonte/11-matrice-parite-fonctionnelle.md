# 11 — Matrice de parité fonctionnelle

Chaque fonctionnalité existante (F-001 à F-034, inventoriée dans `03-cartographie-fonctionnelle.md`) est mise en regard de son équivalent cible. Le statut indique si la fonctionnalité est :

- **Conservée** : aucun changement de comportement
- **Améliorée** : comportement préservé, UX améliorée
- **Déplacée** : accessible à un endroit différent, fonctionnalité intacte
- **Nouvelle** : fonctionnalité ajoutée dans la refonte

---

## Section : Authentification et session

| ID | Fonctionnalité existante | Lot | Statut | Équivalent cible | Risque |
|---|---|---|---|---|---|
| F-001 | Login JWT via `POST auth/login` | 0 | Conservée | Comportement identique, secret externalisé | Faible |
| F-002 | Persistance token en mémoire uniquement | — | Conservée | Comportement identique (volontairement volatil) | — |
| F-003 | Déconnexion + nettoyage session | — | Conservée | Bouton déconnexion identique | — |
| F-004 | Événement `SessionChanged` → rafraîchissement UI | — | Conservée | Comportement identique | — |

---

## Section : Dashboard

| ID | Fonctionnalité existante | Lot | Statut | Équivalent cible | Risque |
|---|---|---|---|---|---|
| F-005 | Chargement 17 API en `Task.WhenAll` | 3 | Améliorée | Mêmes 17 appels, mais avec skeleton + feedback progressif | Moyen — ordre d'affichage change |
| F-006 | Hero métriques (3 KPI ligue) | 3 | Conservée | Position identique, skeleton en attente | — |
| F-007 | KPI ligue (4 tiles) | 3 | Conservée | Même affichage, skeleton | — |
| F-008 | 4 classements (2 fixes + 2 dynamiques) | 3 | Améliorée | Clic sur joueuse → Drawer, clic sur kicker → change métrique | Faible |
| F-009 | Sélecteur métrique classement (9 métriques, `RankingMetricCatalog`) | 3 | Conservée | Comportement identique | — |
| F-010 | 6 matchs récents (cards) | 3 | Conservée | Même présentation | — |
| F-011 | AudienceLens selector (Club/Analyste/Joueuse) | 3/4 | Améliorée | Plus visible, position améliorée | Faible |
| F-012 | Spotlight joueuse (profil + KPI + graphiques + zones) | 3 | Améliorée | Ajout lien "Ouvrir la fiche" + skeleton | Faible |
| F-013 | Directory joueuses avec recherche | 3 | Conservée | Comportement identique | — |
| F-014 | Filtres rapides (compétition, saison) | 3 | Améliorée | Toujours visibles sans déployer le panneau | Faible |
| F-015 | Filtres avancés (équipe, poste, journée, from/to, attaque, défense, déclencheur, nuance) | 3 | Améliorée | Tooltips explicatifs ajoutés sur options non évidentes | Faible |
| F-016 | `ScopeSummaryBar` (affichage périmètre actif) | 3 | Conservée | Déplacée juste sous les filtres globaux (toujours visible) | Faible |
| F-017 | Mode démo (DemoDataFactory, 3 profils ARD/ALG/GB) | — | Conservée | Données identiques, lien accessible depuis sidebar | — |

---

## Section : Fiche joueuse (Players)

| ID | Fonctionnalité existante | Lot | Statut | Équivalent cible | Risque |
|---|---|---|---|---|---|
| F-018 | Liste filtrée de joueuses | 4 | Améliorée | Mise en page liste + profil côte à côte sur desktop | Faible |
| F-019 | Onglets stats (Offense / Défense / Passes / Sanctions / Gardienne) | — | Conservée | Comportement identique | — |
| F-020 | KPI par domaine (tiles colorées selon tone) | — | Conservée | Même calcul, tones identiques | — |
| F-021 | Profil d'impact (graphique radar/bar) | — | Conservée | Même graphique | — |
| F-022 | Tendance par match (area/line) | — | Conservée | Même graphique | — |
| F-023 | Zones spatiales buts (SVG heatmap BG/BD) | 6 | Conservée + accessibilité | Ajout aria-label sur les zones | Faible |
| F-024 | Zones spatiales déclencheurs (SVG heatmap TG/TD + miroir) | 6 | Conservée + accessibilité | Idem | Faible |
| F-025 | Tableau détaillé par match (triable) | 5 | Améliorée | Pagination explicite + aria-sort | Faible |
| F-026 | `HandballKpiHelper` formules (PerMatch, Ratio, Share, etc.) | — | Conservée | Aucun changement de formule | — |
| F-027 | Gardienne : onglet spécifique + tones spécifiques | — | Conservée | Thresholds gardienne inchangés | — |

---

## Section : Comparaison

| ID | Fonctionnalité existante | Lot | Statut | Équivalent cible | Risque |
|---|---|---|---|---|---|
| F-028 | Sélection 2–6 joueuses | — | Conservée | Comportement identique | — |
| F-029 | Section Synthèse (KPI côte à côte) | — | Conservée | Comportement identique | — |
| F-030 | Section Graphes (radar + bars groupés) | — | Conservée | Comportement identique | — |
| F-031 | Section Tableau (données brutes comparées) | — | Conservée | Pagination améliorée (Lot 5) | — |

---

## Section : Équipes

| ID | Fonctionnalité existante | Lot | Statut | Équivalent cible | Risque |
|---|---|---|---|---|---|
| F-032 | Fiche équipe (KPI collectif, effectif, matchs) | — | Conservée | Comportement identique | — |

---

## Section : Matchs

| ID | Fonctionnalité existante | Lot | Statut | Équivalent cible | Risque |
|---|---|---|---|---|---|
| F-033 | Liste matchs filtrée | — | Conservée | Comportement identique | — |
| F-034 | Détail match : KPI résumé + Timeline + Joueuses + Spatial | — | Conservée | Comportement identique, skeleton ajouté (Lot 3) | — |

---

## Fonctionnalités nouvelles (absentes dans l'existant)

| ID | Fonctionnalité | Lot | Description |
|---|---|---|---|
| N-001 | Skeleton UI | 1 | Placeholder pendant le chargement pour toutes les sections majeures |
| N-002 | Drawer joueuse | 2/4 | Fiche joueuse consultable depuis n'importe quelle liste sans navigation complète |
| N-003 | Breadcrumb dans la topbar | 2 | Contexte de navigation pour les pages de détail |
| N-004 | Tooltips filtres avancés | 3 | Explications sur attaque/défense/déclencheur/nuance dans le filtre avancé |
| N-005 | Lien "Ouvrir la fiche" depuis spotlight | 3 | Accès direct `/players/{id}` depuis le dashboard |
| N-006 | Toggle mode sombre | 6 | Contrôle utilisateur sur le thème |
| N-007 | Propagation CancellationToken | 5 | Annulation automatique des requêtes obsolètes |

---

## Règles métier — parité obligatoire

Les éléments suivants NE DOIVENT PAS être modifiés lors de la refonte :

| Règle | Fichier | Raison |
|---|---|---|
| `HandballKpiHelper` : PerMatch, Ratio, Share, SuccessVsWasteShare | `KpiModels.cs` | Formules validées métier |
| Thresholds tones gardienne vs. joueuse de champ | `KpiModels.cs` | Différenciés volontairement |
| `ToPaletteRate` (normalisation min=10%, max=55%) | `SpatialZoneVisuals.cs` | Normalisé sur les données réelles |
| `ToVisualTriggerKey` (miroir TG↔TD) | `SpatialZoneVisuals.cs` | Convention métier handball |
| `ResolveMatchClock` : détection mi-temps via `half.Contains("2"/"deux"/"second")` | `MatchScenarioAnalyzer.cs` | Fragile mais fonctionnel — ne pas modifier sans test |
| `BuildRuns` : seuil run = 3 buts consécutifs | `MatchScenarioAnalyzer.cs` | Définition métier |
| `TableHeatToneHelper` thresholds (0.85 positive, 0.65 good, 0.40 warning) | `TableHeatToneHelper.cs` | Cohérence visuelle validée |

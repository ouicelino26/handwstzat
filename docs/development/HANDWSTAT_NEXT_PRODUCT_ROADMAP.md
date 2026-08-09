# HandWStat — Next Product Roadmap

**Date :** 2026-07-31
**Branch :** feature/handwstat-functional-product-v1

---

## Priorités immédiates (ce sprint)

### P0 — Navigation : position-profiles visible

**Problème :** `ShowInRail=false` et `ShowOnMobile=false` → accessible uniquement depuis le bas du rail desktop et le manifesto Compare.

**Action :** Ajouter un lien visible depuis le rail (icône distincte) ou depuis la page Compare (bouton explicite).

**Impact :** Haute — un analyste qui cherche "Role benchmark" ne le trouve pas sans connaître la navigation.

---

### P1 — Tests manquants sur domaines fonctionnels

| Domaine | Tests manquants |
|---------|----------------|
| Compare | Batch v1, états erreur, cancellation |
| Teams | Chargement, états erreur, données manquantes |
| Matches | Timeline, états erreur, cancellation |
| PositionProfiles | Chargement, profil null, calcul percentile |
| Dashboard | TeamOfDay chargement à la demande, cancellation |

Objectif : couvrir les états loading/empty/error/cancellation/ETag sur chaque domaine.

---

### P2 — Section gardienne conditionnelle (Players)

**Problème :** La section `Goalkeeper` est présente dans le ViewModel mais l'affichage conditionnel dans `Players.razor` peut être amélioré pour les joueuses de champ (masquage propre plutôt que section vide).

**Action :** Vérifier le rendu conditionnel `IsGoalkeeper` dans `LeaguePlayerStatsPanel.razor`.

---

### P3 — Texte "calculs locaux exploratoires" dans TeamOfDay

**Problème :** Le texte affiché dans le Dashboard indique que les scores d'équipe type sont "des calculs locaux exploratoires, non contractuels dans l'API v1". Ce message est honnête mais doit être formulé de façon à aider l'analyste sans le déstabiliser.

**Action :** Reformuler en "Sélection calculée à partir des statistiques disponibles — indicatif."

---

## Priorités moyen terme

### Amélioration UX

| Item | Description |
|------|-------------|
| Skeleton loader | Remplacer StateCard loading par des skeletons dans Players/Teams/Matches |
| Responsive tableaux | Vérifier les tableaux sur viewport < 768px |
| Focus management | Vérifier les aria-label sur filtres et actions |
| Tooltip accessibilité | AnalyticsTooltip → role="tooltip", aria-describedby |

### Cache et performance

| Item | Description |
|------|-------------|
| SemaphoreSlim ReferenceDataService | Éviter double-chargement concurrent |
| Compteurs de diagnostic (debug only) | Nombre d'appels par page en mode Debug |

### Composants design system

| Item | Description |
|------|-------------|
| Migrer KpiTile → RateMetricCard | Uniformiser les cartes KPI sur Pages secondaires |
| AnalyticsLoadingSkeleton | Composant réutilisable pour états de chargement |

---

## Bloqué en attente API

| Feature | Blocage | Priorité externe |
|---------|---------|-----------------|
| Page qualité données | API-BLOCK-01 | P2 |
| Possessions | API-BLOCK-02 | P3 |
| Lineups/On-Off | API-BLOCK-03 | P3 |
| xG/xS | API-BLOCK-04 | P4 |
| Scouting | API-BLOCK-05 | P3 |
| Vidéo | API-BLOCK-06 | P4 |
| Rapports | API-BLOCK-07 | P3 |

---

## Bloqué en attente environnement

| Item | Blocage |
|------|---------|
| Android build | XABAA7000 Permission denied (antivirus / verrou fichier temporaire) |
| iOS/macCatalyst build | Nécessite agent macOS avec Xcode |
| Signing Windows/Android | BLOCKED_EXTERNAL_CREDENTIALS |
| Tests API staging | Aucun credential staging disponible |

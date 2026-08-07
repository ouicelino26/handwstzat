# Player Trajectory V2 — Visual Acceptance

## Critères d'acceptation visuels

### Graphique principal
- [ ] Le graphique s'affiche dans un conteneur `.traj-chart-wrap` avec fond `--surface`
- [ ] La ligne de la métrique est en bleu (`#4fc3f7`)
- [ ] La ligne de référence saison est en gris (`#888888`) quand disponible
- [ ] Le header du graphique affiche : titre métrique + unité + nombre de matchs
- [ ] Le graphique disparaît proprement si 0 match dans la fenêtre

### Contrôles
- [ ] Le sélecteur de métrique liste les métriques du poste courant (champ / gardienne)
- [ ] Les 3 boutons de fenêtre (5 derniers / 10 derniers / Saison) changent d'état visuellement
- [ ] Le bouton actif est en bleu (`--accent`) avec texte noir
- [ ] Le spinner "Chargement…" s'affiche pendant `IsTrajectoryLoading`

### Résumés
- [ ] 3 cartes (5 derniers, 10 derniers, Saison) s'affichent côte à côte
- [ ] La valeur agrégée est en `1.3rem` bold
- [ ] Le delta vs saison est coloré (vert = amélioration, rouge = dégradation)
- [ ] La logique de direction est respectée : pour une métrique LowerIsBetter, un delta négatif est vert
- [ ] Les cartes 5/10 derniers n'affichent pas de delta si la fenêtre = Saison
- [ ] La note "X renseignés sur Y" apparaît si des matchs sont DATA_MISSING

### Tendance
- [ ] Le badge de tendance s'affiche en vert (progressing), rouge (declining), gris (stable)
- [ ] Le badge "Données insuffisantes" s'affiche en gris clair si < 5 matchs éligibles
- [ ] L'icône de direction (↗ ↘ →) est cohérente avec l'état

### Tableau historique
- [ ] Les matchs sont triés du plus récent au plus ancien
- [ ] La colonne "Min." affiche "—" si `PlayingTimeStatus == DataMissing`
- [ ] La colonne de métrique affiche "—" si `Availability == DATA_MISSING`
- [ ] Le delta vs saison est coloré selon la direction de la métrique
- [ ] Les lignes sont cliquables (→ `/matches?matchId=…`) et ont un cursor pointer
- [ ] L'accessibilité clavier fonctionne (Enter/Space → navigation)

## Décisions de design

- Police tabular-nums sur les valeurs numériques pour l'alignement
- L'onglet porte le label "Évolution" (clé technique `"graphs"` inchangée)
- Pas de scatter chart dans cet onglet (SCATTER_DECISION=REMOVE_FROM_TRAJECTORY)
- Pas de bar charts statiques (STATIC_PROFILE_DECISION=REMOVE_REDUNDANT)
- Le graphique se réinstancie via `@key` lors du changement de métrique ou de fenêtre

## Accessibility checklist

- [x] `role="region"` + `aria-label` sur le graphique et le résumé
- [x] `role="tablist"` + `aria-label` sur les boutons de fenêtre
- [x] Navigation clavier sur les lignes du tableau (tabindex, onkeydown)
- [x] `role="grid"` sur le tableau
- [x] `scope="col"` sur les headers du tableau

## Responsive

- **1440px** — Affichage complet, contrôles sur une ligne
- **1024px** — Affichage complet, contrôles peuvent wraper
- **768px** — Contrôles en colonne, select pleine largeur, colonne "Min." masquée, résumés en scroll horizontal
- **360px** — Colonne "vs saison" masquée, boutons de fenêtre plus petits

## Statut

`VISUAL_REVIEW_STATUS=NOT_RUN` — La revue visuelle manuelle n'a pas encore été effectuée sur appareil réel.

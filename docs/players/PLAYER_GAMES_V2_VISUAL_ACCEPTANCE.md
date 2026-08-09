# Player Games V2 — Visual Acceptance

## Critères d'acceptation visuels

### Toolbar
- [ ] Barre de recherche pleine largeur sur mobile
- [ ] Groupes de boutons Période / Résultat / Mode alignés horizontalement sur desktop
- [ ] Bouton actif : fond `var(--accent)`, texte noir, font-weight 600
- [ ] Boutons inactifs : fond transparent, bord subtle, texte muted
- [ ] Wrap sur 2 lignes sur écrans < 900px sans overflow

### Synthèse
- [ ] Nombre de matchs affiché
- [ ] Temps moyen par match (si données disponibles)
- [ ] Avertissement jaune si temps manquant pour certains matchs

### État vide
- [ ] Message contextuel si filtres actifs vs aucune donnée intrinsèque
- [ ] Pas de tableau ni cartes affichés

### Tableau desktop
- [ ] Masqué sur écran < 768px
- [ ] En-têtes sticky
- [ ] Tri date fonctionnel (asc/desc)
- [ ] Tri minutes fonctionnel
- [ ] Ligne sélectionnée : fond légèrement accentué
- [ ] Expand/collapse via ▼/▲
- [ ] Résultat V : vert, D (défaite) : rouge, N (nul) : gris
- [ ] Score absent → "—" (jamais "0–0")
- [ ] Temps absent → "—" (jamais "0")
- [ ] Mode Écart saison : delta coloré vert/rouge selon direction métrique

### Détail inline
- [ ] Ouvre sous la ligne sélectionnée
- [ ] Affiche titre match, journée, compétition
- [ ] Sections Attaque / Défense / Maîtrise pour joueuses de champ
- [ ] Sections Arrêts / Avec ballon pour gardiennes
- [ ] Lien "Ouvrir le match" → `/matches?matchId=...`

### Cartes mobile (< 768px)
- [ ] Tableau masqué
- [ ] Cartes affichées en colonne
- [ ] Expand/collapse au tap
- [ ] Lien "Détails" présent

## Décisions de design

### Palette
- Victoire : `var(--tone-positive, #48a36c)` avec fond rgba 12%
- Défaite : `var(--tone-danger, #b9635a)` avec fond rgba 12%
- Nul : `var(--text-muted, #888)` avec fond rgba 6%
- Delta positif : `var(--tone-positive)`
- Delta négatif : `var(--tone-danger)`
- Avertissement (temps partiel) : `var(--tone-warning, #f0ca91)`

### Typographie
- Valeurs numériques : `font-variant-numeric: tabular-nums`
- En-têtes colonne : 0.68rem, uppercase, letter-spacing 0.08em
- Nom adversaire : 0.85rem, font-weight 500
- Score : 0.72rem, muted

### Responsive breakpoints
- `> 768px` : tableau desktop visible, cartes masquées
- `≤ 768px` : tableau masqué, cartes visibles, toolbar en wrap 1 col
- `≤ 480px` : boutons toolbar padding réduit

## Accessibility checklist

- [ ] `role="tablist"` sur `.scene-switcher` (déjà existant)
- [ ] Tab "Matchs" : `aria-selected` implicite via `is-active`
- [ ] Tableau : `aria-label="Historique des matchs"`
- [ ] `<caption class="sr-only">` avec nom joueuse et nombre de résultats
- [ ] En-têtes tri : `aria-sort="ascending|descending|none"`
- [ ] Lignes cliquables : `role="button"`, `tabindex="0"`, `aria-expanded`
- [ ] `@onkeydown` Enter/Space pour expand
- [ ] `title` sur badge résultat (Victoire / Nul / Défaite)
- [ ] Cartes mobile : `role="button"`, `tabindex="0"`, `@onkeydown`
- [ ] `.sr-only` class disponible pour légendes masquées visuellement

## Contraintes visuelles validées

| Contrainte | Statut |
|---|---|
| `PLAYER_GAMES_MATCHCARD_COUNT=0` | PASS — MatchCard retiré de la vue |
| Score absent rendu "0–0" | PASS — affiché "—" |
| Temps 0 rendu "0 min" | PASS — affiché "—" |
| Taux 0/0 rendu "0%" | PASS — affiché "Aucun tir" ou "—" |

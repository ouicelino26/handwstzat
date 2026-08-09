# Player Performance V2 — Critères d'acceptation visuelle

## Navigation par onglets

- [ ] Les onglets internes s'affichent sous la barre de navigation principale (Brief / Performance / Trajectory / Court / Games)
- [ ] Pour une joueuse de champ : 4 onglets visibles — Attaque · Défense · Maîtrise · Discipline
- [ ] Pour une gardienne : 3 onglets visibles — Arrêts · Avec ballon · Discipline
- [ ] L'onglet actif a un fond accentué (`.is-active`), les autres sont transparents avec bordure
- [ ] Cliquer sur un onglet change le contenu sans rechargement et sans décalage de scroll
- [ ] Sur mobile (360px), les onglets scrollent horizontalement sans afficher la scrollbar
- [ ] `aria-selected` est mis à jour à chaque changement d'onglet

## PerformanceMetricRow

- [ ] Chaque ligne affiche : label à gauche, valeur à droite, evidence en dessous (taille réduite, muted)
- [ ] Le contexte (ex: "1,4/match") s'affiche en italique sous l'evidence
- [ ] La valeur positive est en vert (`--tone-positive`), warning en orange (`--tone-warning`), danger en rouge (`--tone-danger`)
- [ ] Un état DATA_MISSING affiche "Donnée non disponible" en muted
- [ ] Un état INSUFFICIENT_SAMPLE affiche "Échantillon limité" en orange
- [ ] La dernière ligne d'un groupe n'a pas de bordure inférieure

## Section Attaque

- [ ] "Buts" affiche la décomposition `X jeu + Y sur 7m` en evidence
- [ ] Le contexte "X/match" s'affiche uniquement quand `MatchesPlayed > 0`
- [ ] Les lignes 7m obtenus et Sanctions obtenues n'apparaissent que si les données V2 sont présentes (`PenaltiesWon` / `SanctionsDrawn`)
- [ ] Les taux affichent les preuves sous forme `numérateur / dénominateur`
- [ ] "Volume fiable" ou "Min. X tirs" s'affiche selon `SampleReliable`
- [ ] Taux 7m avec volume insuffisant affiche "Échantillon limité"

## Section Défense

- [ ] Les interceptions et contres affichent un contexte "/match"
- [ ] Les 7m concédés ont un ton warning quand > 3
- [ ] Les sanctions concédées affichent la décomposition (avert. · × 2 min · exclusions)
- [ ] Si V2 non disponible, fallback sur V1 sans avertissement

## Section Maîtrise

- [ ] Les pertes de balle affichent la décomposition en ChildContent (mauvaises passes, passes pivot)
- [ ] Quand les passes pivot sont DATA_MISSING, `pmr-data-missing` s'affiche en italique
- [ ] Un PerformanceMetricRow "Passes pivot ratées" avec `Value="—"` est affiché (jamais calculé depuis BadPasses)
- [ ] Le ton "warning" s'applique quand pertes/match > 3

## Section Discipline

- [ ] Le bar chart affiche 3 barres (avert., 2 min, excl.) avec les bonnes couleurs (accent, warning, danger)
- [ ] Les barres sont proportionnelles au total de sanctions
- [ ] "7m concédés" est clairement séparé avec le contexte "Distinct des sanctions disciplinaires"
- [ ] Aucun 7m n'est compté dans le total "Total sanctions"

## Section Gardienne — Arrêts

- [ ] "Arrêts total" affiche la décomposition `X jeu + Y sur 7m`
- [ ] "Buts encaissés" affiche la décomposition `X jeu + Y sur 7m`
- [ ] Les 3 taux d'arrêt affichent leurs preuves numérateur/dénominateur
- [ ] L'avertissement échantillon insuffisant s'affiche quand applicable

## Section Gardienne — Avec ballon

- [ ] Les 4 métriques s'affichent : Passes décisives, Buts, Pertes de balle, Tirs manqués
- [ ] Le ton warning s'applique aux pertes de balle > 5

## Responsive

| Breakpoint | Critère |
|------------|---------|
| 1440px | Layout complet, onglets sur une ligne |
| 1024px | Layout complet, onglets sur une ligne |
| 768px | Padding onglets réduit, font-size 0.78rem |
| 360px | font-size label 0.8rem, valeur 0.95rem, onglets scrollables |

## Accessibilité

- [ ] `role="tablist"` sur le conteneur d'onglets avec `aria-label="Sections Performance"`
- [ ] `role="tab"` sur chaque bouton d'onglet
- [ ] `aria-selected="true/false"` mis à jour dynamiquement
- [ ] `aria-controls` pointe vers l'id du panel correspondant
- [ ] `role="tabpanel"` sur chaque panneau de contenu
- [ ] Navigation clavier fonctionnelle (tabulation vers les boutons)

## Non-régressions

- [ ] Brief (overview) : aucun changement visible
- [ ] Trajectory (graphs) : aucun changement visible
- [ ] Court (zones) : aucun changement visible
- [ ] Games (matches) : aucun changement visible
- [ ] Changement de joueur : `PerformanceSection` se réinitialise à "attack" (champ) ou "saves" (gardienne)

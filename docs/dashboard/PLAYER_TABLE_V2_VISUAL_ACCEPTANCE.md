# Player Table V2 — Critères de validation visuelle

Date : 2026-08-07  
Statut : À remplir manuellement après recette visuelle

## Checklist UI générale

- [ ] L'onglet "Classements globaux" affiche un loader `PageLoader` pendant le chargement lazy
- [ ] Le tableau s'affiche correctement une fois les données chargées
- [ ] Changer d'onglet (Ligue, Équipe du jour, etc.) et revenir à "Classements globaux" n'effectue pas de second appel API si le scope n'a pas changé
- [ ] Modifier un filtre (compétition, équipe, saison, journée) invalide le cache et recharge le tableau à l'ouverture suivante de l'onglet

## Groupe joueuses de champ / gardiennes

- [ ] Boutons "Joueuses de champ" et "Gardiennes" affichent le bon compteur
- [ ] Basculer entre les deux groupes efface la recherche et le filtre poste
- [ ] Chips de filtre par poste n'apparaissent que pour les joueuses de champ

## Filtres locaux

- [ ] La recherche par nom fonctionne en temps réel (case-insensitive)
- [ ] Le chip "Tout" est actif par défaut
- [ ] Sélectionner un poste filtre correctement les lignes
- [ ] Un poste avec 0 joueuse dans le scope ne génère pas de chip orphelin

## Sous-onglets de vue

- [ ] Synthèse, Attaque, Défense pour joueuses de champ s'affichent correctement
- [ ] Synthèse, Arrêts, Avec ballon pour gardiennes s'affichent correctement
- [ ] Changer de vue conserve le filtre et la recherche actifs

## Colonnes et cellules

- [ ] Colonne "Joueuse" (identity) est sticky à gauche lors du scroll horizontal
- [ ] En-tête de tableau reste sticky en haut lors du scroll vertical
- [ ] Colonnes numériques sont alignées à droite
- [ ] Les taux affichent la valeur en bold + la preuve (num/dén) en sous-texte
- [ ] Un taux avec dénominateur = 0 affiche "—" et "0/0" (jamais "0%")
- [ ] PenaltiesWon affiche "—" avec title "Non disponible en source V1"
- [ ] SanctionsDrawn affiche "—" avec title "Non disponible en source V1"
- [ ] FailedPivotPasses affiche "—" avec title "Donnée non disponible en V1"
- [ ] SanctionsConceded affiche le total + ⓘ avec tooltip "Avert. X · 2min Y · DQ Z"

## Tri

- [ ] Cliquer sur un en-tête trie la colonne (desc par défaut sauf nom/équipe)
- [ ] Recliquer inverse le tri (toggle asc/desc)
- [ ] L'aria-sort est correctement mis à jour (none / ascending / descending)
- [ ] Les taux null apparaissent en dernier (tri desc)
- [ ] Les colonnes non-triables (PenaltiesWon, SanctionsDrawn, FailedPivotPasses) n'ont pas de bouton sort actif

## Responsive

- [ ] Sur mobile (< 480px) la colonne "Equipe" est masquée (hide-mobile)
- [ ] Sur mobile la barre de recherche occupe toute la largeur
- [ ] Le scroll horizontal du tableau fonctionne sur iOS Safari
- [ ] L'ombre de scroll est visible à droite quand le tableau déborde

## Accessibilité

- [ ] Chaque tableau a une `<caption class="sr-only">` descriptive
- [ ] Les boutons de tri ont un aria-label et aria-sort corrects
- [ ] Les tooltips ⓘ et "—" ont des attributs `title` lisibles par screen readers
- [ ] Les chips de filtre poste ont un `role="group"` et `aria-label`
- [ ] Le loader PageLoader est annoncé correctement

## Performance

- [ ] L'onglet initial (Ligue) charge sans délai visible
- [ ] Le premier clic sur "Classements globaux" déclenche UNE seule requête réseau
- [ ] Pas de requête N+1 visible dans les DevTools réseau

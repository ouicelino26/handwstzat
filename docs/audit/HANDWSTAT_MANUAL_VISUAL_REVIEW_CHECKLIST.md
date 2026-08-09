# HandWStat — Checklist de Revue Visuelle Manuelle

**Date :** 2026-08-01  
**Branche :** feature/handwstat-functional-product-v1  

Cette checklist couvre les éléments qui nécessitent une vérification visuelle manuelle (non automatisable par tests unitaires).

---

## Instructions d'utilisation

Pour chaque item : `✅ OK` / `⚠️ Partiel` / `❌ Défaut` / `N/A` si non applicable.

---

## 1. Dashboard (/dashboard)

- [ ] Le chargement affiche un skeleton/loader visible
- [ ] Les classements s'affichent avec les scores corrects
- [ ] L'AudienceLensSelector change l'affichage des KPIs
- [ ] L'équipe type (TeamOfDay) s'affiche avec les joueuses et scores PIE
- [ ] Le scope (compétition/journée) est persistant après navigation
- [ ] En mode "aucune donnée" : StateCard vide visible
- [ ] En mode erreur API : StateCard erreur avec message lisible

---

## 2. Fiche joueuse (/players)

- [ ] La liste des joueuses se filtre par équipe et poste
- [ ] La fiche s'ouvre sur clic (navigation vers /players/{id})
- [ ] L'en-tête affiche nom, poste, équipe, matchs joués, temps de jeu
- [ ] Le badge AnalyticsSource (v1/v2/unavailable) est visible
- [ ] Les sections offense/défense/passes/sanctions s'affichent
- [ ] Les 7m concédés apparaissent en information séparée (hors total disciplinaire)
- [ ] Le total disciplinaire = Avertissements + 2min + Exclusions (sans 7m concédés)
- [ ] Pour une gardienne : sections arrêts, buts encaissés, zones cadre
- [ ] FailedPivotPasses affiche "Donnée non disponible" (jamais 0)
- [ ] Le lien vers /position-profiles?playerId= fonctionne

---

## 3. Radars et profils de poste (/position-profiles)

- [ ] Le radar s'affiche avec les axes nommés en français
- [ ] Les annotations "Top 10%", "Alerte", "Au-dessus de la médiane" sont en français
- [ ] La couleur des axes reflète la performance (vert/orange/rouge)
- [ ] Le scatter chart affiche la joueuse dans sa cohorte
- [ ] L'histogramme montre la distribution de la cohorte
- [ ] Le tableau détaillé liste toutes les métriques avec percentiles
- [ ] La page est accessible depuis /compare (lien "Role benchmark")
- [ ] Sur mobile : page accessible depuis /compare (vérifier NAV-01)

---

## 4. Comparaison (/compare)

- [ ] Ajout de 2-6 joueuses au plateau
- [ ] Le MultiRadar s'affiche avec les 2-6 profils superposés
- [ ] Les annotations "Au-dessus de la médiane" sont en français
- [ ] Le tableau comparatif s'affiche avec les colonnes joueuses
- [ ] Les filtres de scope (compétition, saison) filtrent les données

---

## 5. Carte des tirs (GoalKpi)

- [ ] Les 24 zones de cadre sont affichées
- [ ] Les zones colorées reflètent l'efficacité (chaud = élevé)
- [ ] Le taux par zone est lisible au survol/clic
- [ ] Les zones avec peu de tirs sont distinguables visuellement
- [ ] La logique miroir est correcte (gauche/droite cohérents)

---

## 6. Équipes (/teams)

- [ ] La liste des équipes s'affiche avec statistiques
- [ ] La fiche équipe s'ouvre avec les KPIs d'équipe
- [ ] Les données DataQualitySummary sont visibles

---

## 7. Matchs (/matches)

- [ ] La liste des matchs s'affiche avec scores et dates
- [ ] Le détail match montre les stats par équipe
- [ ] Les MatchCards sont cliquables et ouvrent le détail

---

## 8. États globaux

- [ ] 404 → NotFound.razor avec CTAs "Retour Dashboard" et "Voir les joueuses"
- [ ] Déconnexion → AccessRequiredCard sur pages protégées
- [ ] UpdateRequired bloque la navigation vers les pages analytics
- [ ] La CommandPalette s'ouvre et retourne des résultats

---

## 9. Accessibilité (test lecteur d'écran)

- [ ] Navigation clavier possible sur toutes les pages
- [ ] Focus trap dans les drawers de filtres
- [ ] Labels ARIA sur les graphiques SVG (radars, zones de tir)
- [ ] Contraste WCAG AA : vérifier avec outil de contraste
- [ ] `alt` ou `aria-label` sur les photos joueuses

---

## 10. Responsive mobile (si build Android disponible)

- [ ] Le rail de navigation s'affiche en bas de l'écran
- [ ] Les fiches joueuses sont lisibles sans défilement horizontal
- [ ] Les KpiTiles s'empilent verticalement sur petit écran
- [ ] Les graphiques (radars, scatter) sont zoomables/scrollables
- [ ] La CommandPalette fonctionne avec le clavier virtuel

---

## 11. Performance (mesures chronométrées)

- [ ] Temps de chargement dashboard ≤ 3s (LAN/Wifi)
- [ ] Transition entre pages ≤ 500ms
- [ ] Fiche joueuse (v2) ≤ 2s
- [ ] Fiche joueuse (fallback v1) ≤ 3s
- [ ] Rotation d'écran sans rechargement complet

---

## Résumé des items critiques

| Item | Priorité | Automatisable |
|------|----------|---------------|
| Total disciplinaire sans 7m concédés | P0 (corrigé) | ✅ Test ajouté |
| Labels français radar | P1 (corrigé) | ❌ Visuel |
| FailedPivotPasses = DATA_MISSING | P0 (déjà OK) | ✅ Test existant |
| NAV-01 position-profiles mobile | P1 | ❌ Visuel |
| Accessibilité SVG | P2 | ❌ Lecteur d'écran |
| Contraste WCAG AA | P2 | ❌ Outil contraste |

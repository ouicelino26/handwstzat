# HandWStat - Plan de reconstruction totale

## 1. Regle de mesure

Baseline UI source : 54 fichiers.

Exclusion :

- `wwwroot/lib/bootstrap/dist/css/bootstrap.min.css`, fichier tiers minifie.

Seuil de succes :

- au moins 49 fichiers UI modifies ;
- layouts, pages et composants critiques reecrits structurellement ;
- aucune validation fondee uniquement sur le nombre de fichiers ;
- preuve de parite fonctionnelle et build Release vert.

La liste nominative est dans `09-ui-source-baseline.md`.

## 2. Strategie

La reconstruction se fait par remplacement de couches :

1. identite native et host ;
2. design tokens ;
3. shell ;
4. primitives ;
5. pages publiques ;
6. workspaces metier ;
7. visualisations ;
8. responsive et accessibilite ;
9. nettoyage et validation.

Les services, contrats et calculs sont conserves sauf correction metier documentee.

## 3. Lot A - Identite native

Fichiers :

- `App.xaml` ;
- `MainPage.xaml` ;
- `Platforms/Windows/App.xaml` ;
- `wwwroot/index.html` ;
- icone et splash via les ressources et le projet.

Actions :

- retirer les couleurs violettes du template ;
- appliquer le fond natif HandWStat ;
- definir les ressources de theme natives ;
- preparer safe areas et plein ecran ;
- remplacer le chargement generique du host.

## 4. Lot B - Studio shell

Fichiers :

- `MainLayout.razor` ;
- `NavMenu.razor` ;
- `PublicLayout.razor` ;
- CSS isoles associes ;
- `Routes.razor`.

Actions :

- supprimer la sidebar actuelle ;
- creer command bar, domain rail, analytics stage, context lens et activity tray ;
- conserver les routes ;
- adapter desktop, tablette et mobile ;
- conserver theme, session, deconnexion et palette.

## 5. Lot C - Design system

Actions :

- remplacer `app.css` par des couches tokens/base/shell/primitives/features ;
- definir themes clair/sombre ;
- definir typographie, espaces, couleurs, rayons, elevations et motion ;
- reconstruire boutons, champs, chips, tableaux et etats ;
- utiliser les CSS isoles pour les composants complexes ;
- supprimer les generations CSS historiques apres migration.

## 6. Lot D - Pages publiques

- connexion editoriale ;
- demo guidee ;
- page introuvable ;
- suppression fonctionnelle de Counter ou transformation en route technique non navigable.

## 7. Lot E - Today

- supprimer les quatre tabs actuels ;
- composer le cockpit ;
- conserver tous les KPI, classements, spotlight, spatial, tableaux et matchs ;
- deplacer les tableaux complets vers une scene dediee.

## 8. Lot F - Entity workspaces

### Athletes

- repertoire data grid ;
- fiche joueuse en scenes ;
- profil poste integre ;
- export conserve ;
- zones et matchs conserves.

### Squads

- repertoire ;
- workspace equipe ;
- effectif ;
- matchs ;
- suppression du detail inline mort apres preuve de parite.

### Games

- directory ;
- Game Room ;
- timeline ;
- spatial ;
- joueuses ;
- scope equipe coherent.

## 9. Lot G - Lab

- tray de comparaison ;
- toutes les visualisations existantes ;
- tableau comparatif ;
- Role Benchmark ;
- multi-radar ;
- exports.

## 10. Lot H - Composants analytiques

Tous les composants partages recoivent une nouvelle structure :

- etats ;
- loaders ;
- KPI ;
- jauges ;
- historique equipe ;
- filtres ;
- scope ;
- match card ;
- player list ;
- tableaux ;
- histogramme ;
- radar ;
- multi-radar ;
- scatter ;
- court map ;
- coach insights ;
- command surface.

Les signatures publiques restent stables autant que possible afin de reduire le risque.

## 11. Lot I - Verification

### Technique

```powershell
dotnet build HandWStat.csproj -f net10.0-windows10.0.19041.0 --configuration Release
```

### Fonctionnelle

- connexion/deconnexion ;
- scope partage ;
- liens `playerId`, `teamId`, `matchId` ;
- filtres et reset ;
- selection joueuse/equipe/match ;
- comparaison 2 a 6 ;
- profil poste ;
- exports ;
- tri ;
- zones ;
- timeline ;
- themes ;
- etats vides/erreur/loading.

### Visuelle

- aucune sidebar de l'ancien type ;
- aucune page reconstruite comme une pile de cartes identique ;
- desktop 1440 et 1920 ;
- tablette 768 et 1024 ;
- mobile 390 et 430 ;
- clair et sombre ;
- reduced motion.

### Couverture

Une commande Git mesure les 54 fichiers :

```powershell
$baseline = Get-Content docs/ux-redesign/ui-source-baseline.txt
$changed = git diff --name-only
$modified = @($baseline | Where-Object { $changed -contains $_ }).Count
$rate = [math]::Round($modified * 100 / $baseline.Count, 1)
```

Seuil : `rate >= 90`.

## 12. Garde-fous

- ne pas modifier une formule pour faciliter un design ;
- ne pas supprimer un export ;
- ne pas perdre un filtre en le deplacant ;
- ne pas remplacer une donnee absente par 50 ;
- ne pas melanger valeur brute et percentile ;
- ne pas masquer la taille de cohorte ;
- ne pas dupliquer le detail match ;
- ne pas considerer une classe CSS changee comme une reconstruction ;
- ne pas modifier le fichier Bootstrap tiers pour gonfler le taux.

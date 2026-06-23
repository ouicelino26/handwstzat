# HandWStat Studio - Etat de l'implementation

Date de reference : 22 juin 2026.

## Reconstruction livree

L'interface historique a ete remplacee par **HandWStat Studio**, un environnement
d'analyse sportive concu autour de cinq espaces de travail : Today, Athletes,
Squads, Games et Lab. La refonte conserve les routes, services, calculs, exports,
filtres et interactions metier tout en changeant la structure visuelle et les
parcours.

### Experience globale

- command bar persistante avec recherche universelle et palette `Ctrl+K` ;
- rail de domaines desktop et dock mobile ;
- scene analytique centrale, lentille contextuelle et suivi d'activite ;
- theme clair et sombre ;
- navigation et densite adaptees au desktop, a la tablette et au tactile ;
- chargements, erreurs, acces restreints et etats vides homogenes.

### Espaces metier

- **Today** : briefing, signaux prioritaires, activite recente et index de donnees ;
- **Athletes** : repertoire, fiche contextuelle, performance, trajectoire, terrain et matchs ;
- **Squads** : identite d'equipe, pulse, effectif et calendrier ;
- **Games** : repertoire, Game Room, histoire du match, terrain et joueuses ;
- **Lab** : configuration de comparaison, visualisations et preuves detaillees ;
- **Role Benchmark** : cohorte, distribution, radar, dispersion et station d'export.

### Systeme de design

- identite graphite, papier mineral, orange signal, vert terrain et bleu electrique ;
- typographies locales expressives, grille fluide et espacements normalises ;
- composants reutilisables pour KPI, tableaux, filtres, graphiques, alertes et etats ;
- graphiques recolores avec une palette semantique sans dependance au violet historique ;
- animations limitees aux transitions utiles avec prise en charge de `prefers-reduced-motion`.

### Simplification technique

- suppression de BlazorBootstrap et jQWidgets ;
- remplacement de la jauge JavaScript par un composant SVG natif ;
- suppression des assets et scripts de template inutilises ;
- catalogue de navigation centralise ;
- CSS applicatif reconstruit autour de tokens ;
- ressources MAUI, icone, splash screen et ecran de demarrage alignes sur Studio.

## Conservation metier

- appels API, services et modeles conserves ;
- scopes competition, equipe, saison et journee conserves ;
- filtres locaux, tris, exports et modes de lecture conserves ;
- calculs statistiques et cohortes de poste conserves ;
- correction du percentile favorable et du scope spatial des deux equipes maintenue ;
- resolution automatique de `HandballManagerCore` depuis `D:\repos\HandballManagerCore`.

## Couverture

La baseline recense 54 fichiers de presentation applicatifs. Les 54 fichiers ont
ete modifies ou remplaces, soit une couverture effective de 100 %.

## Validation

Commande de reference :

```powershell
dotnet build HandWStat.csproj -f net10.0-windows10.0.19041.0 --configuration Release --no-restore
```

Resultat du controle final :

- build Release reussi ;
- 0 erreur ;
- 0 avertissement ;
- Core compile depuis `D:\repos\HandballManagerCore`.

## Validation produit restante

- revue visuelle sur Windows, Android et plusieurs tailles de fenetre ;
- parcours clavier et lecteur d'ecran complet ;
- mesure sur de grands volumes de donnees reels ;
- tests de caracterisation supplementaires sur les calculs et les scopes.

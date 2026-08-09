# Architecture de l'information HandWStat

## Principe directeur

Chaque vue analytique suit le meme ordre :

1. **Ou et quand ?** Scope visible et date d'actualisation.
2. **Peut-on faire confiance ?** Qualite, provenance et volume.
3. **Que se passe-t-il ?** Deux a cinq signaux principaux.
4. **Pourquoi ?** Volumes, formule, comparaison et contexte.
5. **Ou agir ?** Lien vers joueuse, equipe, match ou evenement.

Une vue mobile affiche d'abord les niveaux 1 a 3 ; les preuves et details restent accessibles par sections, tableaux ou liens sans etre telecharges inutilement.

## Navigation cible

```text
Studio
|- Aujourd'hui / Dashboard
|  |- Ligue
|  |- Equipe de la journee (a la demande)
|  |- Joueuse en vue
|  |- Matchs recents
|  `- Classements globaux (a la demande cible)
|- Joueuses
|  |- Annuaire
|  `- Fiche
|     |- Resume et preuves
|     |- Efficacite
|     |- Defense / gardienne
|     |- Spatial
|     |- Tendance
|     `- Historique
|- Equipes
|  |- Resume
|  |- Effectif
|  |- Profil collectif
|  `- Matchs
|- Matchs
|  |- Liste
|  `- Fiche
|     |- Score et scenario
|     |- Joueurs
|     |- Spatial
|     `- Evenements
|- Comparer
|  |- Selection et scope commun
|  |- Resume probant
|  `- Graphiques avances
`- Profils de poste
   |- Cohorte et qualite
   |- Distribution
   |- Radar
   `- Detail / export
```

## Dashboard Phase 0/1A

Ordre effectif recommande :

1. titre court et origine ;
2. equipes actives, cadence offensive et jeu prepare ;
3. scope complet ;
4. qualite des donnees ;
5. filtres ;
6. selecteur de section ;
7. contenu d'une seule section.

La Phase 0 conserve le design existant mais introduit `RateMetricCard`, `AnalysisScopeSummary` et `DataQualitySummary`. L'equipe de la journee est chargee uniquement lorsqu'elle est ouverte.

## Contrat de carte metrique

Une carte de taux doit rendre visibles :

- libelle francais ;
- valeur ou `N/A` ;
- unite ;
- numerateur/denominateur, ou `Volume non fourni par l'API` ;
- minimum et fiabilite ;
- explication courte ;
- icone et texte de tone ;
- lien vers une vue de preuve quand elle existe.

La couleur est redondante, jamais porteuse unique du sens.

## Progressive disclosure et performance

| Niveau | Contenu | Regle de chargement |
|---|---|---|
| 1 | Scope, qualite, 2-5 signaux | Initial |
| 2 | Classement ou resume de section | A l'ouverture de section |
| 3 | Graphiques, spatial, grande table | A la demande et annulable |
| 4 | Evenements, video, export | Action explicite |

Budget cible dashboard : 3 a 5 appels initiaux, une section lourde a la fois, aucune reponse obsolete appliquee apres un changement de filtre.

## Scope commun

Le composant `AnalysisScopeSummary` est la reference de presentation. Il affiche : competition, equipe, saison, journee, periode, nombre de matchs et date de generation/actualisation connue. Le futur contrat API doit aussi fournir le scope resolu, pas seulement la requete demandee.

Le service global `AnalysisScopeService` reste compatible, mais devra accueillir periode, match, poste et identifiant de generation lors d'une evolution additive.

## Qualite et erreurs

Niveaux :

- `Unknown` : `Qualite non renseignee` ; valeur par defaut v1 ;
- `Low` : anomalies ou echantillon critique ;
- `Medium` : donnees utilisables avec reserve ;
- `High` : uniquement sur preuve API explicite.

Une erreur utilisateur contient une action ou une explication courte. Le code technique, le corps HTTP et la stack ne sont jamais affiches. L'identifiant de correlation peut etre montre comme reference de support.

## Mobile

- une colonne pour cartes et resume qualite sous 42 rem ;
- zones tactiles d'au moins 44 px sur boutons/onglets ;
- tables dans un conteneur horizontal avec premiere colonne identifiable ;
- graphiques secondaires non montes tant que leur section est fermee ;
- labels courts, unite toujours associee a la valeur ;
- filtres regroupes dans `details`, scope resume toujours visible.

## Accessibilite

- focus global `:focus-visible` conserve ;
- `prefers-reduced-motion` deja defini dans `wwwroot/app.css` ;
- `aria-live` sur chargements et erreurs ;
- `aria-label` calculé sur chaque carte metrique ;
- icone marquee `aria-hidden` lorsqu'un texte equivalent existe ;
- entetes de tableau avec `scope="col"`, caption et etat de tri dans la phase suivante ;
- textes alternatifs descriptifs sur portraits, logos et cartes spatiales ;
- ordre clavier identique a l'ordre visuel.

## Langue et nomenclature

Le francais est la langue par defaut. Les codes metier/API restent anglais uniquement dans le code et les diagnostics. Les noms ambigus sont qualifies :

- `Taux de tir ouvert` plutot que `Taux de tir` si les 7 m sont exclus ;
- `Balance passes decisives / pertes` en explication de `Ballons valorises` ;
- `Score technique local exploratoire` au lieu de `PIE` tant que l'API ne le contractualise pas ;
- `Impact defensif simplifie` si la tendance ne contient pas les quatre actions du KPI global.

## Liens entre objets

Chaque ligne ou carte doit converger vers :

- joueuse -> `/players` avec selection ;
- equipe -> `/teams` avec selection ;
- match -> `/matches` avec detail ;
- comparaison -> `/compare` avec scope commun.

La navigation parametree et partageable est `EXISTE PARTIELLEMENT`; les identifiants ne sont pas encore portes uniformement dans l'URL. Cette evolution est Phase 1/2, sans refaire toute la navigation dans Phase 0.

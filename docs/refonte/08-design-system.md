# 08 — Design System cible

## 1. Fondations

### 1.1 Palette principale

```css
/* Primaire — Orange handball (maintenu, contraste corrigé) */
--color-primary:         #c94318;   /* ratio 4.7:1 sur blanc — WCAG AA */
--color-primary-hover:   #b33a14;
--color-primary-soft:    rgba(201, 67, 24, 0.12);

/* Secondaire — Teal analytique */
--color-secondary:       #0f766e;
--color-secondary-hover: #0b5f58;
--color-secondary-soft:  rgba(15, 118, 110, 0.12);

/* Fonds */
--color-bg-page:         #f4f6fa;
--color-bg-alt:          #edf0f7;
--color-bg-surface:      #ffffff;
--color-bg-surface-soft: #f8fafc;
--color-bg-muted:        #f1f5f9;
--color-bg-sidebar:      #0b1420;
--color-bg-sidebar-deep: #060e18;
```

### 1.2 Couleurs sémantiques

```css
--color-positive:        #1f8a5b;   /* Succès, good */
--color-positive-soft:   rgba(31, 138, 91, 0.14);
--color-good:            #2e7d5e;
--color-good-soft:       rgba(46, 125, 94, 0.12);
--color-warning:         #c47b14;
--color-warning-soft:    rgba(196, 123, 20, 0.14);
--color-danger:          #c2414a;
--color-danger-soft:     rgba(194, 65, 74, 0.14);
--color-info:            #2563eb;
--color-neutral:         rgba(15, 23, 42, 0.06);
```

### 1.3 Couleurs de données (graphiques)

Séquence de 8 couleurs distinguables en daltonisme :

```
1. #e76f30  Orange handball (primary)
2. #0f766e  Teal (secondary)
3. #3b82f6  Bleu
4. #8b5cf6  Violet
5. #14b8a6  Cyan
6. #f59e0b  Ambre
7. #ec4899  Rose
8. #6b7280  Gris
```

> Ces couleurs ne transmettent jamais d'information **uniquement** par la couleur. Chaque série porte également un label, un pattern ou une forme.

### 1.4 Textes

```css
--color-text-strong:  #0f172a;
--color-text-body:    #374151;
--color-text-muted:   #667085;
--color-text-soft:    #94a3b8;
--color-text-inverse: #f8fafc;
```

### 1.5 Typographie

| Usage | Police | Poids | Taille |
|---|---|---|---|
| Corps | Open Sans | 400 | 0.9rem–1rem |
| Titres | Open Sans | 700 (système fallback) | clamp(1rem, 2vw, 2.4rem) |
| Kickers | Open Sans | 800 | 0.72rem |
| Code | Cascadia Mono, Consolas | 400 | 0.95em |

> Recommandation : ajouter `OpenSans-SemiBold.ttf` (600) et `OpenSans-Bold.ttf` (700) aux ressources embarquées pour éviter le fallback système sur les titres.

### 1.6 Échelle typographique

```
--text-xs:   0.72rem  (kickers, labels minuscules)
--text-sm:   0.82rem  (metadata, captions)
--text-base: 0.9rem   (corps de texte)
--text-md:   1rem     (corps principal)
--text-lg:   1.1rem   (sous-titres)
--text-xl:   1.25rem  (h4)
--text-2xl:  1.5rem   (h3)
--text-3xl:  1.9rem   (h2)
--text-4xl:  2.4rem   (h1)
```

### 1.7 Grille et espacements

```css
--space-1: 0.25rem   /* 4px */
--space-2: 0.5rem    /* 8px */
--space-3: 0.75rem   /* 12px */
--space-4: 1rem      /* 16px */
--space-5: 1.25rem   /* 20px */
--space-6: 1.5rem    /* 24px */
--space-8: 2rem      /* 32px */
--space-10: 2.5rem   /* 40px */
--space-12: 3rem     /* 48px */
--space-16: 4rem     /* 64px */
```

### 1.8 Rayons

```css
--radius-sm:   0.5rem
--radius-md:   0.8rem
--radius-lg:   1rem
--radius-xl:   1.25rem
--radius-2xl:  1.5rem
--radius-pill: 999px
```

### 1.9 Ombres

```css
--shadow-sm: 0 1px 2px rgba(15,23,42,.05), 0 4px 12px rgba(15,23,42,.05);
--shadow-md: 0 4px 16px rgba(15,23,42,.08), 0 1px 4px rgba(15,23,42,.04);
--shadow-lg: 0 12px 36px rgba(15,23,42,.10), 0 3px 8px rgba(15,23,42,.04);
```

### 1.10 Breakpoints

```css
--bp-sm:  640px   /* Mobile large / tablette petite */
--bp-md:  768px   /* Tablette */
--bp-lg:  1024px  /* Desktop compact */
--bp-xl:  1280px  /* Desktop standard */
--bp-2xl: 1536px  /* Desktop large */
```

### 1.11 Animations

```css
--duration-fast:   120ms
--duration-base:   200ms
--duration-slow:   320ms
--ease-standard:   cubic-bezier(0.4, 0, 0.2, 1)
--ease-decelerate: cubic-bezier(0.0, 0.0, 0.2, 1)
--ease-accelerate: cubic-bezier(0.4, 0.0, 1, 1)

@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after { transition-duration: 0.01ms !important; }
}
```

---

## 2. Composants

### 2.1 Boutons

| Variante | Usage | CSS class |
|---|---|---|
| Primary | Action principale | `.btn-primary` |
| Secondary | Action secondaire | `.btn-secondary` |
| Ghost | Action tertiaire / filtres | `.btn-ghost` |
| Danger | Actions destructives | `.btn-danger` |
| Icon | Action icône seule | `.btn-icon` |

**États :** default, hover, focus-visible, active, disabled  
**Tailles :** sm (h32), md (h40, default), lg (h48)  
**Clavier :** Enter/Space pour déclencher l'action

### 2.2 Champs de formulaire

| Variante | Usage |
|---|---|
| Text input | Recherche, saisie libre |
| Select | Dropdown de filtres |
| Date input | Sélecteur de période |

**États :** default, focus, error, disabled  
**Label :** toujours visible, wrappant le champ (pattern existant conservé)

### 2.3 KPI Tile (métriques textuelles)

```
┌─────────────────────────────┐
│  KICKER                     │  ← tone color
│  Valeur principale          │  ← text-2xl, bold
│  Libellé court              │  ← text-sm, muted
│  Contexte / base de calcul  │  ← text-xs, soft (optionnel)
└─────────────────────────────┘
```

**Tones :** neutral, positive, good, warning, danger  
**Tailles :** compact (dashboard), standard (fiche), large (hero)  
**Usage :** ne jamais transmettre le sens uniquement par la couleur → toujours avoir le libellé

### 2.4 BarGauge KPI

Maintenu pour la compatibilité existante. À terme : remplacement par un composant CSS natif (barre de progression stylisée) pour réduire la dépendance jQWidgets.

### 2.5 Onglets

```
[Offense]  [Défense]  [Passes]  [Sanctions]  [Gardienne]
   ^^^
   active : border-bottom couleur primaire
```

**Clavier :** flèches gauche/droite pour naviguer entre onglets  
**Rôles ARIA :** `role="tablist"`, `role="tab"`, `role="tabpanel"`

### 2.6 Tableaux

| Variante | Usage |
|---|---|
| Standard | Tableau de données avec tri |
| Dense | Version compacte pour espaces réduits |
| Heat | Avec coloration relative des cellules (TableHeatToneHelper) |

**Colonnes :** entête cliquable pour tri + indicateur ↑↓  
**Responsive :** scroll horizontal + épinglage colonne 1  
**Accessibilité :** `<th scope="col">`, `aria-sort`

### 2.7 Cartes

| Variante | Usage |
|---|---|
| Panel | Conteneur principal avec titre |
| Mini card | Résumé compact (joueuse, match) |
| Match card | Carte match cliquable |
| State card | État vide / erreur / avertissement |

### 2.8 Skeleton (nouveau)

Chaque section majeure dispose d'un skeleton qui préserve les dimensions finales pour éviter les sauts de mise en page (CLS).

```
Skeleton KPI tile :
┌─────────────────────┐
│  ████░░░░ (kicker)  │
│  ██████████ (valeur)│
│  ████░░ (libellé)   │
└─────────────────────┘
```

### 2.9 Drawer (nouveau)

```
Overlay semi-transparent
└── Panneau 480px (desktop) / fullscreen (mobile)
    ├── Header : titre + bouton fermer
    ├── Corps : scrollable
    └── Footer : actions (optionnel)
```

**Fermeture :** Escape, clic overlay, bouton close  
**Focus trap :** actif quand ouvert

### 2.10 États vides

```
┌─────────────────────────────────────────────────┐
│          [icône SVG contextuelle]               │
│          Titre de l'état                        │
│          Message explicatif court              │
│          [Bouton action si applicable]          │
└─────────────────────────────────────────────────┘
```

Tones : empty (neutre), warning, error

### 2.11 Badge / Chip

Pour les tags de poste, nationalité, compétition :

```
[ALG]  [France]  [Starligue]
```

Tailles : sm, md  
Couleurs : semantic ou neutral

---

## 3. Mode clair / Mode sombre

**Décision :** Le mode sombre est retenu car :
- Les applications de statistiques sportives professionnelles (Wyscout, Hudl, SofaScore) l'utilisent par défaut ou l'offrent
- Les heatmaps spatiales et les graphiques sont plus lisibles sur fond sombre
- Les sessions longues d'analyse sont plus confortables en mode sombre

**Implémentation :** CSS custom properties remappées via `@media (prefers-color-scheme: dark)` ou via une classe `.theme-dark` sur `:root`.

**Contrôle utilisateur :** Toggle dans la topbar ou les paramètres.

**Vérification obligatoire en mode sombre :**
- Tous les tones sémantiques (contrastes minimum 4.5:1)
- Couleurs des graphiques (même palette, ajustement de luminosité)
- Heatmaps (palette inversée ou adaptée)
- Textes sur fond coloré (sidebar, cartes hero)

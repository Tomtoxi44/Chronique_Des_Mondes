# 🎨 Design System - Chronique des Mondes

> **Dernière mise à jour** : 16 octobre 2025  
> **Version** : 1.0.0

---

## 📋 Table des Matières

- [1. Identité de Marque](#1-identité-de-marque)
- [2. Principes de Design](#2-principes-de-design)
- [3. Palette de Couleurs](#3-palette-de-couleurs)
- [4. Typographie](#4-typographie)
- [5. Espacements & Grids](#5-espacements--grids)
- [6. Composants UI](#6-composants-ui)
- [7. Patterns d'Interaction](#7-patterns-dinteraction)
- [8. Iconographie](#8-iconographie)
- [9. Animations & Transitions](#9-animations--transitions)
- [10. Responsive Design](#10-responsive-design)
- [11. Accessibilité](#11-accessibilité)

---

## 1. Identité de Marque

### 1.1 Vision

**Chronique des Mondes** combine l'atmosphère immersive des jeux de rôle fantasy avec une interface moderne et intuitive. Le design doit évoquer l'aventure épique tout en restant professionnel et accessible.

### 1.2 Personnalité de la marque

- **Épique** : Évoque les grandes aventures
- **Accessible** : Simple et intuitif
- **Moderne** : Technologie au service du jeu
- **Communautaire** : Favorise l'interaction entre joueurs

### 1.3 Inspiration

**Références principales** :
- Foundry VTT : Interface épurée et fonctionnelle
- D&D Beyond : Clarté et organisation de l'information
- Discord : Navigation intuitive et moderne

---

## 2. Principes de Design

### 2.1 Hiérarchie visuelle claire

- Titres distincts et lisibles
- Séparation nette entre sections
- Focus sur le contenu principal

### 2.2 Feedback immédiat

- Tous les clics doivent avoir une réponse visuelle
- États chargement visibles
- Messages d'erreur/succès clairs

### 2.3 Cohérence

- Composants réutilisables
- Espacements uniformes
- Comportements prévisibles

### 2.4 Performance

- Animations légères (60fps)
- Chargement progressif
- Optimisation images

---

## 3. Palette de Couleurs

### 3.1 Système de couleurs principales

**Choix officiel : Améthyste 💜**

```css
/* Couleurs de base */
--color-background-primary: #0F0F0F;    /* Noir profond */
--color-background-secondary: #1A1A1A;  /* Gris très foncé */
--color-background-elevated: #252525;   /* Gris foncé élevé */

/* Couleur principale - Améthyste ⭐ */
--color-primary-900: #2D1B3D;           /* Améthyste très foncé */
--color-primary-700: #5A2D7C;           /* Améthyste foncé */
--color-primary-500: #8B5CF6;           /* Améthyste standard - Couleur signature */
--color-primary-300: #A78BFA;           /* Améthyste clair */
--color-primary-100: #DDD6FE;           /* Améthyste très clair */

/* Couleur secondaire - Or mystique */
--color-secondary-700: #92400E;         /* Or foncé */
--color-secondary-500: #D97706;         /* Or standard */
--color-secondary-300: #FCD34D;         /* Or clair */

/* Texte */
--color-text-primary: #F5F5F5;          /* Blanc cassé */
--color-text-secondary: #A8A8A8;        /* Gris moyen */
--color-text-tertiary: #6B6B6B;         /* Gris sombre */
```

**Pourquoi Améthyste ?**
- ✅ Unique et élégant
- ✅ Excellent contraste sur fond noir (WCAG AA compliant)
- ✅ Évoque la magie et le mystère
- ✅ Se distingue des interfaces gaming classiques (vert/rouge/bleu)

### 3.2 Couleurs d'état

```css
/* Success */
--color-success-700: #15803D;
--color-success-500: #22C55E;
--color-success-100: #DCFCE7;

/* Warning */
--color-warning-700: #CA8A04;
--color-warning-500: #EAB308;
--color-warning-100: #FEF9C3;

/* Error */
--color-error-700: #B91C1C;
--color-error-500: #EF4444;
--color-error-100: #FEE2E2;

/* Info */
--color-info-700: #1D4ED8;
--color-info-500: #3B82F6;
--color-info-100: #DBEAFE;
```

### 3.3 Utilisation des couleurs

| Élément | Couleur | Usage |
|---------|---------|-------|
| Background principal | `background-primary` | Corps de page |
| Cards / Panels | `background-secondary` | Conteneurs |
| Modals | `background-elevated` | Overlays |
| Boutons primaires | `primary-500` | Actions principales |
| Boutons secondaires | `background-elevated` + bordure | Actions secondaires |
| Liens | `primary-300` | Texte cliquable |
| Hover sur liens | `primary-100` | État hover |
| Titres | `text-primary` | H1-H6 |
| Corps de texte | `text-secondary` | Paragraphes |
| Labels | `text-tertiary` | Annotations |

---

## 4. Typographie

### 4.1 Polices de caractères

**Titre principal (Headers)** : 
```css
font-family: 'Poppins', sans-serif;
/* Poids : 400 (Regular), 600 (SemiBold), 700 (Bold) */
```

**Corps de texte** :
```css
font-family: 'Inter', sans-serif;
/* Poids : 400 (Regular), 500 (Medium), 600 (SemiBold) */
```

**Monospace (Stats, Code)** :
```css
font-family: 'JetBrains Mono', monospace;
/* Poids : 400 (Regular), 500 (Medium) */
```

> **CDN** : Google Fonts (libre de droits, performance optimisée)

### 4.2 Échelle typographique

```css
/* Display (Hero titles) */
--font-size-display: 4rem;      /* 64px */
--line-height-display: 1.1;
--font-weight-display: 700;

/* Headings */
--font-size-h1: 3rem;           /* 48px */
--line-height-h1: 1.2;
--font-weight-h1: 700;

--font-size-h2: 2.25rem;        /* 36px */
--line-height-h2: 1.25;
--font-weight-h2: 600;

--font-size-h3: 1.875rem;       /* 30px */
--line-height-h3: 1.3;
--font-weight-h3: 600;

--font-size-h4: 1.5rem;         /* 24px */
--line-height-h4: 1.4;
--font-weight-h4: 600;

--font-size-h5: 1.25rem;        /* 20px */
--line-height-h5: 1.4;
--font-weight-h5: 600;

--font-size-h6: 1.125rem;       /* 18px */
--line-height-h6: 1.4;
--font-weight-h6: 600;

/* Body */
--font-size-base: 1rem;         /* 16px */
--line-height-base: 1.6;
--font-weight-base: 400;

--font-size-small: 0.875rem;    /* 14px */
--line-height-small: 1.5;

--font-size-xs: 0.75rem;        /* 12px */
--line-height-xs: 1.4;
```

### 4.3 Exemples d'usage

```html
<h1 class="text-h1">Mes Personnages</h1>
<h2 class="text-h2">Elyndor, Mage Elfe</h2>
<p class="text-base">Description du personnage...</p>
<span class="text-small text-tertiary">Créé le 15 oct. 2025</span>
```

---

## 5. Espacements & Grids

### 5.1 Système d'espacement (8px base)

```css
--spacing-xs: 0.25rem;    /* 4px */
--spacing-sm: 0.5rem;     /* 8px */
--spacing-md: 1rem;       /* 16px */
--spacing-lg: 1.5rem;     /* 24px */
--spacing-xl: 2rem;       /* 32px */
--spacing-2xl: 3rem;      /* 48px */
--spacing-3xl: 4rem;      /* 64px */
```

### 5.2 Layout Grid

```css
/* Container principal */
.container {
  max-width: 1440px;
  margin: 0 auto;
  padding: 0 var(--spacing-lg);
}

/* Grid responsive */
.grid {
  display: grid;
  gap: var(--spacing-lg);
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
}
```

### 5.3 Breakpoints

```css
/* Mobile first approach */
--breakpoint-sm: 640px;   /* Téléphones */
--breakpoint-md: 768px;   /* Tablettes portrait */
--breakpoint-lg: 1024px;  /* Tablettes paysage / petits laptops */
--breakpoint-xl: 1280px;  /* Desktops */
--breakpoint-2xl: 1536px; /* Large desktops */
```

---

## 6. Composants UI

### 6.1 Buttons

#### Styles

**Primary Button** :
```css
.btn-primary {
  background: var(--color-primary-500);
  color: white;
  padding: 0.75rem 1.5rem;
  border-radius: 0.5rem;
  font-weight: 600;
  border: none;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-primary:hover {
  background: var(--color-primary-700);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(139, 92, 246, 0.4);
}

.btn-primary:active {
  transform: translateY(0);
}

.btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}
```

**Secondary Button** :
```css
.btn-secondary {
  background: var(--color-background-elevated);
  color: var(--color-text-primary);
  border: 1px solid var(--color-primary-500);
}

.btn-secondary:hover {
  background: var(--color-primary-900);
  border-color: var(--color-primary-300);
}
```

**Icon Button** :
```css
.btn-icon {
  width: 2.5rem;
  height: 2.5rem;
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 0.5rem;
}
```

#### Tailles
- **Small** : `padding: 0.5rem 1rem; font-size: 0.875rem;`
- **Medium** : `padding: 0.75rem 1.5rem; font-size: 1rem;` (default)
- **Large** : `padding: 1rem 2rem; font-size: 1.125rem;`

### 6.2 Inputs

```css
.input {
  background: var(--color-background-secondary);
  border: 1px solid rgba(255, 255, 255, 0.1);
  color: var(--color-text-primary);
  padding: 0.75rem 1rem;
  border-radius: 0.5rem;
  font-size: 1rem;
  transition: all 0.2s ease;
}

.input:focus {
  outline: none;
  border-color: var(--color-primary-500);
  box-shadow: 0 0 0 3px rgba(139, 92, 246, 0.1);
}

.input:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.input.error {
  border-color: var(--color-error-500);
}
```

### 6.3 Cards

**Style A - Bordures arrondies** (Recommandé pour harmonie avec le reste)

```css
.card {
  background: var(--color-background-secondary);
  border: 1px solid rgba(255, 255, 255, 0.05);
  border-radius: 1rem;
  padding: var(--spacing-lg);
  transition: all 0.2s ease;
}

.card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.4);
  border-color: rgba(139, 92, 246, 0.3);
}

.card-clickable {
  cursor: pointer;
}

.card-clickable:active {
  transform: translateY(0);
}
```

**Character Card Example** :
```css
.character-card {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.character-card-image {
  width: 100%;
  aspect-ratio: 1;
  border-radius: 0.75rem;
  object-fit: cover;
}

.character-card-name {
  font-size: var(--font-size-h4);
  font-weight: 600;
  color: var(--color-text-primary);
}

.character-card-stats {
  display: flex;
  justify-content: space-between;
  font-family: 'JetBrains Mono', monospace;
  color: var(--color-text-secondary);
  font-size: var(--font-size-small);
}
```

### 6.4 Modals

```css
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.8);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background: var(--color-background-elevated);
  border: 1px solid rgba(139, 92, 246, 0.2);
  border-radius: 1rem;
  padding: var(--spacing-xl);
  max-width: 500px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-lg);
}

.modal-title {
  font-size: var(--font-size-h3);
  font-weight: 600;
}

.modal-close {
  background: transparent;
  border: none;
  color: var(--color-text-secondary);
  cursor: pointer;
  font-size: 1.5rem;
}
```

### 6.5 Toast Notifications

```css
.toast {
  position: fixed;
  top: var(--spacing-lg);
  right: var(--spacing-lg);
  background: var(--color-background-elevated);
  border-left: 4px solid var(--color-primary-500);
  border-radius: 0.5rem;
  padding: var(--spacing-md);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.4);
  min-width: 300px;
  animation: slideInRight 0.3s ease;
  z-index: 2000;
}

.toast.success { border-left-color: var(--color-success-500); }
.toast.error { border-left-color: var(--color-error-500); }
.toast.warning { border-left-color: var(--color-warning-500); }
.toast.info { border-left-color: var(--color-info-500); }
```

---

## 7. Patterns d'Interaction

### 7.1 Navigation - Sidebar rétractable

```
Desktop (expanded):
┌────────┬────────────────────────────┐
│ Logo   │  Header                    │
├────────┼────────────────────────────┤
│ 🏠     │                            │
│ Home   │    Main Content            │
│        │                            │
│ 🎭     │                            │
│ Perso  │                            │
│        │                            │
│ 📖     │                            │
│ Camp.  │                            │
│        │                            │
│ [◀]    │                            │
└────────┴────────────────────────────┘

Desktop (collapsed):
┌───┬──────────────────────────────┐
│ 🏠│  Header                       │
├───┼──────────────────────────────┤
│ 🎭│                               │
│ 📖│    Main Content               │
│ ⚔️│                               │
│[▶]│                               │
└───┴──────────────────────────────┘

Mobile (burger menu):
┌────────────────────────────────┐
│ ☰  Logo              👤  ⚙️    │
├────────────────────────────────┤
│                                │
│      Main Content              │
│                                │
└────────────────────────────────┘
```

### 7.2 Hover States

- **Liens** : Changement de couleur + soulignement
- **Boutons** : Élévation légère (2px) + box-shadow
- **Cards** : Élévation (2px) + bordure accent
- **Icons** : Rotation ou scale subtil

### 7.3 Loading States

**Spinner** :
```html
<div class="spinner"></div>
```

**Skeleton** (pour les listes) :
```html
<div class="skeleton skeleton-card"></div>
<div class="skeleton skeleton-text"></div>
```

**Progress Bar** (pour uploads/actions longues) :
```html
<div class="progress-bar">
  <div class="progress-fill" style="width: 60%"></div>
</div>
```

### 7.4 Form Validation

**Erreur inline** :
```html
<div class="form-group">
  <label>Email</label>
  <input type="email" class="input error" />
  <span class="error-message">
    <i class="icon-alert"></i> Email invalide
  </span>
</div>
```

**Success feedback** :
```html
<input type="text" class="input success" />
<span class="success-message">
  <i class="icon-check"></i> Email disponible
</span>
```

---

## 8. Iconographie

### 8.1 Bibliothèque d'icônes

**Recommandation** : **Bootstrap Icons** (libre, cohérent, vaste)

```html
<!-- CDN -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css">

<!-- Usage -->
<i class="bi bi-house-door"></i>
<i class="bi bi-person"></i>
<i class="bi bi-book"></i>
<i class="bi bi-sword"></i>
```

### 8.2 Icônes personnalisées

Pour les icônes spécifiques JDR (dés, classes, races), utiliser **SVG inline** :

```html
<svg class="icon icon-d20" width="24" height="24" viewBox="0 0 24 24">
  <path d="..." fill="currentColor"/>
</svg>
```

### 8.3 Tailles d'icônes

```css
.icon-xs { width: 12px; height: 12px; }
.icon-sm { width: 16px; height: 16px; }
.icon-md { width: 24px; height: 24px; } /* default */
.icon-lg { width: 32px; height: 32px; }
.icon-xl { width: 48px; height: 48px; }
```

---

## 9. Animations & Transitions

### 9.1 Durées

```css
--duration-instant: 0.1s;
--duration-fast: 0.2s;
--duration-normal: 0.3s;
--duration-slow: 0.5s;
```

### 9.2 Easings

```css
--ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);
--ease-out: cubic-bezier(0, 0, 0.2, 1);
--ease-in: cubic-bezier(0.4, 0, 1, 1);
```

### 9.3 Animations communes

**Fade In** :
```css
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}
```

**Slide In Right** :
```css
@keyframes slideInRight {
  from { 
    transform: translateX(100%); 
    opacity: 0;
  }
  to { 
    transform: translateX(0); 
    opacity: 1;
  }
}
```

**Pulse** (pour notifications) :
```css
@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.7; }
}
```

### 9.4 Règles d'usage

- ✅ Transitions sur hover/focus (0.2s)
- ✅ Page transitions (0.3s)
- ✅ Modal open/close (0.3s)
- ❌ Éviter animations longues (> 0.5s)
- ❌ Pas d'animations auto-play en boucle

---

## 10. Responsive Design

### 10.1 Stratégie

**Desktop-First** avec optimisations mobile :
1. Design pour 1440px
2. Adaptations à 1024px (tablette)
3. Adaptations à 768px (tablette portrait)
4. Optimisations à 640px (mobile)

### 10.2 Sidebar responsive

```css
/* Desktop (> 1024px) */
.sidebar {
  width: 250px;
  position: fixed;
  left: 0;
  top: 0;
  height: 100vh;
}

.sidebar.collapsed {
  width: 80px;
}

/* Mobile (< 1024px) */
@media (max-width: 1024px) {
  .sidebar {
    transform: translateX(-100%);
    transition: transform 0.3s ease;
    z-index: 100;
  }
  
  .sidebar.open {
    transform: translateX(0);
  }
  
  .sidebar-overlay {
    display: block;
    background: rgba(0, 0, 0, 0.5);
  }
}
```

### 10.3 Grids responsives

```css
/* Desktop : 4 colonnes */
.character-grid {
  grid-template-columns: repeat(4, 1fr);
}

/* Tablette : 3 colonnes */
@media (max-width: 1280px) {
  .character-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

/* Tablette portrait : 2 colonnes */
@media (max-width: 768px) {
  .character-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

/* Mobile : 1 colonne */
@media (max-width: 640px) {
  .character-grid {
    grid-template-columns: 1fr;
  }
}
```

### 10.4 Typography responsive

```css
/* Desktop */
h1 { font-size: 3rem; }
h2 { font-size: 2.25rem; }
body { font-size: 1rem; }

/* Mobile */
@media (max-width: 768px) {
  h1 { font-size: 2rem; }
  h2 { font-size: 1.5rem; }
  body { font-size: 0.875rem; }
}
```

---

## 11. Accessibilité

### 11.1 Contraste

Ratio minimum WCAG AA : **4.5:1** pour texte normal, **3:1** pour texte large.

Tous les textes sur fond noir respectent ce ratio :
- Texte primaire (#F5F5F5) : ✅ 15.8:1
- Texte secondaire (#A8A8A8) : ✅ 7.9:1
- Accent améthyste (#8B5CF6) : ✅ 5.2:1

### 11.2 Focus visible

```css
*:focus-visible {
  outline: 2px solid var(--color-primary-500);
  outline-offset: 2px;
}
```

### 11.3 Screen readers

```html
<!-- Bouton avec texte caché -->
<button aria-label="Fermer le modal">
  <i class="bi bi-x"></i>
  <span class="sr-only">Fermer</span>
</button>

<!-- Image décorative -->
<img src="..." alt="" aria-hidden="true" />

<!-- Image informative -->
<img src="..." alt="Portrait d'Elyndor le Mage" />
```

### 11.4 Keyboard navigation

- ✅ Tous les éléments interactifs accessibles au clavier
- ✅ Order logique de tabulation
- ✅ Escape pour fermer modals
- ✅ Enter/Space pour activer boutons

### 11.5 ARIA Labels

```html
<nav aria-label="Navigation principale">...</nav>
<section aria-labelledby="section-title">...</section>
<button aria-expanded="false" aria-controls="menu">Menu</button>
```

---

## 12. Implémentation avec Blazor

### 12.1 Structure CSS

```
wwwroot/css/
├── app.css              # Point d'entrée
├── variables.css        # Variables CSS
├── base.css             # Reset & base styles
├── components/
│   ├── buttons.css
│   ├── inputs.css
│   ├── cards.css
│   ├── modals.css
│   └── toast.css
├── layouts/
│   ├── sidebar.css
│   └── header.css
└── utilities.css        # Classes utilitaires
```

### 12.2 Variables CSS

```css
/* variables.css */
:root {
  /* Colors */
  --color-background-primary: #0F0F0F;
  --color-primary-500: #8B5CF6;
  /* ... toutes les autres variables ... */
  
  /* Spacing */
  --spacing-xs: 0.25rem;
  /* ... */
  
  /* Typography */
  --font-size-base: 1rem;
  /* ... */
}
```

### 12.3 Composants Blazor

Utiliser des **CSS isolés** pour chaque composant :

```
CharacterCard.razor
CharacterCard.razor.css
CharacterCard.razor.cs
```

---

## 13. Checklist d'implémentation

### Phase 1 - Fondations
- [ ] Importer polices (Poppins, Inter, JetBrains Mono)
- [ ] Créer fichier `variables.css` avec toutes les variables
- [ ] Créer `base.css` avec reset et styles de base
- [ ] Importer Bootstrap Icons

### Phase 2 - Composants de base
- [ ] Buttons (primary, secondary, icon)
- [ ] Inputs (text, email, password)
- [ ] Labels & form-groups
- [ ] Toast notifications

### Phase 3 - Layout
- [ ] Sidebar avec toggle
- [ ] Header responsive
- [ ] Container principal
- [ ] Responsive breakpoints

### Phase 4 - Composants avancés
- [ ] Cards (character, campaign)
- [ ] Modals
- [ ] Loading states (spinner, skeleton)
- [ ] Progress bars

### Phase 5 - Pages
- [ ] Page d'authentification (Login/Register)
- [ ] Dashboard / Home
- [ ] Liste personnages
- [ ] Détails personnage

---

## 14. Ressources

### Polices
- Poppins : https://fonts.google.com/specimen/Poppins
- Inter : https://fonts.google.com/specimen/Inter
- JetBrains Mono : https://fonts.google.com/specimen/JetBrains+Mono

### Icônes
- Bootstrap Icons : https://icons.getbootstrap.com/

### Outils
- Contrast Checker : https://webaim.org/resources/contrastchecker/
- Color Palette Generator : https://coolors.co/

### Inspiration
- Foundry VTT : https://foundryvtt.com/
- D&D Beyond : https://www.dndbeyond.com/

---

**Document créé pour Chronique des Mondes**  
**Dernière révision** : 16 octobre 2025

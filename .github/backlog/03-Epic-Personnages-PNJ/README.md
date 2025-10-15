# Epic 3 - Personnages, PNJ & Chapitres

## 📋 Informations Générales

- **Phase** : Phase 0 (MVP Core)
- **Priorité** : P0 - Critique
- **Statut** : 📝 Planifié
- **Estimation totale** : 32 Story Points

---

## 🎯 Objectif

Permettre aux joueurs de créer et gérer leurs personnages génériques, aux MJ de créer des PNJ et de structurer le contenu narratif en chapitres avec blocs de texte enrichis.

---

## 📊 Critères d'Acceptation Globaux

- [ ] Un joueur peut créer un personnage générique
- [ ] Un joueur peut modifier/supprimer ses personnages
- [ ] Les personnages ont des champs personnalisables (JSON)
- [ ] Un MJ peut créer des PNJ génériques
- [ ] Les PNJ peuvent être associés à des chapitres
- [ ] Un MJ peut créer des chapitres avec contenu narratif
- [ ] Les chapitres supportent des blocs de texte enrichis
- [ ] L'ordre des chapitres peut être réorganisé

---

## 📝 User Stories

| ID | Titre | Statut | Story Points | Priorité |
|----|-------|--------|--------------|----------|
| [US-023](./US-023-creation-personnage.md) | Création personnage générique | 📝 Planifié | 5 | P0 |
| [US-024](./US-024-modification-personnage.md) | Modification personnage | 📝 Planifié | 3 | P0 |
| [US-025](./US-025-liste-personnages.md) | Liste des personnages | 📝 Planifié | 2 | P0 |
| [US-026](./US-026-suppression-personnage.md) | Suppression personnage | 📝 Planifié | 2 | P0 |
| [US-027](./US-027-creation-pnj.md) | Création PNJ générique | 📝 Planifié | 5 | P0 |
| [US-028](./US-028-association-pnj-chapitre.md) | Association PNJ ↔ Chapitre | 📝 Planifié | 3 | P1 |
| [US-029](./US-029-creation-chapitres.md) | Création de chapitres | 📝 Planifié | 5 | P0 |
| [US-030](./US-030-blocs-narratifs.md) | Blocs de texte narratif | 📝 Planifié | 5 | P1 |
| [US-031](./US-031-reorganisation-chapitres.md) | Réorganisation chapitres | 📝 Planifié | 2 | P2 |

---

## 🏗️ Architecture Technique

### Backend (.NET 10)
- **Endpoints** : `CharacterEndpoints.cs`, `NPCEndpoints.cs`, `ChapterEndpoints.cs`
- **Services** : `CharacterService`, `NPCService`, `ChapterService`
- **Models** : 
  - `Character` (hérite de `BaseCharacter`)
  - `NPC` (hérite de `BaseCharacter`)
  - `Chapter` (relation N-N avec `NPCs`)
  - `NarrativeBlock` (contenu des chapitres)

### Frontend (Blazor Server)
- **Pages** : Characters.razor, CharacterDetail.razor, ChapterEditor.razor
- **Components** : CharacterForm, NPCManager, NarrativeEditor, ChapterList
- **Services** : CharacterService, NPCService, ChapterService

### Base de Données (TPH Inheritance)
- **Table** : `BaseCharacters` (Type discriminator: Player/NPC)
  - Colonnes communes : Name, HP, CustomFields (JSON)
  - Colonnes spécifiques joueurs : OwnerId
  - Colonnes spécifiques PNJ : Behavior, Nickname
- **Table** : `Chapters` (1-N `NarrativeBlocks`, N-N `NPCs`)

---

## 🎮 Fonctionnalités Clés

### Personnages Génériques
- Champs de base : Nom, Points de Vie (HP)
- Champs personnalisables (JSON) : Force, Agilité, etc. définis par le joueur
- Support futur pour D&D, Pathfinder (Phase 3)

### PNJ Génériques
- Champs : Nom, Surnom, Description, HP
- Comportement : Amical, Neutre, Hostile
- Association à des chapitres spécifiques

### Chapitres Narratifs
- Numérotation séquentielle automatique
- Titre du chapitre
- Contenu en blocs de texte enrichis (Markdown/HTML)
- Réorganisation drag-and-drop

---

## 🧪 Tests

### Tests Unitaires
- Création/modification de personnages
- Validation des champs personnalisables
- Gestion des PNJ
- Structure des chapitres

### Tests d'Intégration
- CRUD complet personnages
- Association PNJ ↔ Chapitre
- Réorganisation chapitres

### Tests E2E
- Parcours joueur : Créer personnage → Modifier → Jouer
- Parcours MJ : Créer PNJ → Associer chapitre → Lancer session

---

## 🔗 Dépendances

### Dépend de
- [Epic 1](../01-Epic-Authentification/) - Authentification
- [Epic 2](../02-Epic-Gestion-Parties/) - Campagnes

### Bloque
- [Epic 4](../04-Epic-Combat-Des/) - Combat (nécessite personnages/PNJ)

---

## 📈 Progression

```
Complété : [░░░░░░░░░░] 0% (0/9 US)
Planifié  : 9 US
```

---

## 📚 Documentation Associée

- [MODELE_DONNEES.md](../../instructions/technique/MODELE_DONNEES.md) - TPH Inheritance
- [SPECIFICATION_FONCTIONNELLE.md](../../instructions/SPECIFICATION_FONCTIONNELLE.md) - Personnages & PNJ

---

**Dernière mise à jour** : 15 octobre 2025

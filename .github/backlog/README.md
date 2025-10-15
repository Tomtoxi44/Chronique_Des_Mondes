# 📋 Backlog - Chronique des Mondes

> **Organisation des User Stories par Epics**  
> Ce backlog suit la structure de la ROADMAP et organise le développement en cycles itératifs.

---

## 🎯 Vue d'Ensemble

Ce backlog est organisé en **6 Epics** correspondant aux phases de développement :

| Epic | Phase | Description | Story Points | Statut |
|------|-------|-------------|--------------|--------|
| [Epic 1](./01-Epic-Authentification/) | Phase 0 | Authentification & Gestion Utilisateurs | 34 SP | 🔄 En cours |
| [Epic 2](./02-Epic-Gestion-Parties/) | Phase 0 | Gestion des Parties, Campagnes & Sessions | 48 SP | 📝 Planifié |
| [Epic 3](./03-Epic-Personnages-PNJ/) | Phase 0 | Personnages, PNJ & Chapitres | 32 SP | 📝 Planifié |
| [Epic 4](./04-Epic-Combat-Des/) | Phase 1 | Combat & Système de Dés | 42 SP | 📝 Planifié |
| [Epic 5](./05-Epic-Sorts-Equipements/) | Phase 2 | Sorts & Équipements | 38 SP | 📝 Planifié |
| [Epic 6](./06-Epic-Mode-DnD/) | Phase 3 | Mode D&D (Règles Automatisées) | 54 SP | 📝 Planifié |

**Total** : 248 Story Points estimés

---

## 📊 Légende des Statuts

- 🔄 **En cours** : User Stories en développement actif
- ✅ **Terminé** : Epic complété et validé
- 📝 **Planifié** : Epic prêt à démarrer
- ⏸️ **En attente** : Bloqué par des dépendances

---

## 📐 Structure d'une User Story

Chaque User Story suit ce template standardisé :

```markdown
# US-XXX - [Titre]

## 📝 Description
**En tant que** [rôle]  
**Je veux** [action]  
**Afin de** [bénéfice]

## ✅ Critères d'Acceptation
- [ ] Critère 1
- [ ] Critère 2

## 🧪 Tests
- [ ] Test unitaire 1
- [ ] Test d'intégration 1
- [ ] Test E2E 1

## 🔧 Tâches Techniques
- [ ] Tâche 1
- [ ] Tâche 2

## 🔗 Dépendances
- Dépend de : US-XXX
- Bloque : US-XXX

## 📊 Estimation
Story Points : X
```

---

## 🎯 Priorités

### P0 - Critique (MVP Core)
Fonctionnalités essentielles pour la première version utilisable :
- Authentification
- Création de campagnes
- Création de personnages
- Sessions de jeu basiques

### P1 - Haute
Fonctionnalités importantes pour une expérience complète :
- Système de combat
- Lanceur de dés
- Gestion des PNJ

### P2 - Moyenne
Améliorations et fonctionnalités avancées :
- Sorts et équipements
- Échanges entre joueurs

### P3 - Basse
Optimisations et modes spécialisés :
- Mode D&D avec calculs automatiques
- Modes spécialisés (Pathfinder, Cthulhu, etc.)

---

## 📈 Progression Globale

```
Epic 1 - Authentification        : [██████░░░░] 60% (6/10 US) - 34 SP
Epic 2 - Gestion Parties         : [░░░░░░░░░░]  0% (0/12 US) - 48 SP
Epic 3 - Personnages & PNJ       : [░░░░░░░░░░]  0% (0/9 US)  - 32 SP
Epic 4 - Combat & Dés            : [░░░░░░░░░░]  0% (0/8 US)  - 42 SP
Epic 5 - Sorts & Équipements     : [░░░░░░░░░░]  0% (0/9 US)  - 38 SP
Epic 6 - Mode D&D                : [░░░░░░░░░░]  0% (0/9 US)  - 54 SP

Total : 6/57 User Stories (10.5%) - 248 Story Points estimés
```

---

## 🔄 Processus de Développement

### 1. Sprint Planning
- Sélection des US prioritaires
- Estimation en Story Points
- Assignment aux développeurs

### 2. Développement
- Création de branches feature/US-XXX
- Implémentation selon les critères d'acceptation
- Tests unitaires et d'intégration

### 3. Review
- Validation des critères d'acceptation
- Tests E2E
- Code review

### 4. Déploiement
- Merge dans main
- Déploiement sur environnement de staging
- Validation finale

---

## 📚 Références

- [ROADMAP](../ROADMAP.md) - Planification des phases
- [SPECIFICATION_FONCTIONNELLE](../instructions/SPECIFICATION_FONCTIONNELLE.md) - Spécifications détaillées
- [ARCHITECTURE_TECHNIQUE](../instructions/technique/ARCHITECTURE_TECHNIQUE.md) - Architecture du projet

---

**Dernière mise à jour** : 15 octobre 2025

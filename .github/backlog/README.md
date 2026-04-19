# 📋 Backlog - Chronique des Mondes

> **Organisation des User Stories par Epics**  
> Ce backlog suit la structure de la ROADMAP et organise le développement en cycles itératifs.

---

## 🎯 Vue d'Ensemble

Ce backlog est organisé en **6 Epics** correspondant aux phases de développement :

| Epic | Phase | Description | Statut |
|------|-------|-------------|--------|
| [Epic 1](./01-Epic-Authentification/) | Fondations | Authentification & Gestion Utilisateurs | ✅ Majorité implémentée |
| [Epic 2](./02-Epic-Gestion-Parties/) | Fondations | Gestion des Mondes, Campagnes, Chapitres & Sessions | ✅ Majorité implémentée |
| [Epic 3](./03-Epic-Personnages-PNJ/) | Fondations | Personnages & PNJ | 🔄 Personnages OK, PNJ planifié |
| [Epic 4](./04-Epic-Combat-Des/) | Futur | Combat & Système de Dés | ⏳ Futur |
| [Epic 5](./05-Epic-Sorts-Equipements/) | Futur | Sorts & Équipements | ⏳ Futur |
| [Epic 6](./06-Epic-Mode-DnD/) | Futur | Mode D&D Avancé (Calculs Automatiques) | ⏳ Futur — données basiques implémentées |

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
Epic 1 - Authentification        : [████████░░] 80% - Implémenté (manque reset mdp, validation email)
Epic 2 - Gestion Parties         : [████████░░] 80% - Implémenté (manque SignalR, historique)
Epic 3 - Personnages & PNJ       : [█████░░░░░] 50% - Personnages OK, PNJ planifié
Epic 4 - Combat & Dés            : [░░░░░░░░░░]  0% - Futur
Epic 5 - Sorts & Équipements     : [░░░░░░░░░░]  0% - Futur (inventaire basique implémenté)
Epic 6 - Mode D&D Avancé         : [██░░░░░░░░] 20% - Données basiques (classe, race, stats) implémentées
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

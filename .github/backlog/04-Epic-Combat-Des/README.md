# Epic 4 - Combat & Système de Dés

## 📋 Informations Générales

- **Phase** : Phase 1 (Système de Jeu)
- **Priorité** : P1 - Haute
- **Statut** : 📝 Planifié
- **Estimation totale** : 42 Story Points

---

## 🎯 Objectif

Implémenter un système de combat générique avec gestion des tours, initiatives, et un lanceur de dés intégré pour les jets de compétences et d'attaques.

---

## 📊 Critères d'Acceptation Globaux

- [ ] Un MJ peut déclencher un combat depuis une session
- [ ] Le système calcule l'ordre d'initiative (manuel pour MVP)
- [ ] Les joueurs et PNJ jouent tour par tour
- [ ] Les joueurs peuvent effectuer des actions (attaque, déplacement, action spéciale)
- [ ] Un lanceur de dés intégré permet les jets (d4, d6, d8, d10, d12, d20, d100)
- [ ] Les résultats sont transmis au MJ en temps réel
- [ ] Les PV sont mis à jour manuellement
- [ ] Le combat peut se terminer (victoire/défaite/fuite)
- [ ] Un résumé de combat est généré

---

## 📝 User Stories

| ID | Titre | Statut | Story Points | Priorité |
|----|-------|--------|--------------|----------|
| [US-032](./US-032-declenchement-combat.md) | Déclenchement de combat | 📝 Planifié | 8 | P1 |
| [US-033](./US-033-initiative-manuelle.md) | Calcul initiative manuel | 📝 Planifié | 5 | P1 |
| [US-034](./US-034-gestion-tours.md) | Gestion des tours de combat | 📝 Planifié | 8 | P1 |
| [US-035](./US-035-actions-combat.md) | Actions de combat | 📝 Planifié | 5 | P1 |
| [US-036](./US-036-lanceur-des.md) | Lanceur de dés intégré | 📝 Planifié | 5 | P1 |
| [US-037](./US-037-transmission-resultats.md) | Transmission résultats au MJ | 📝 Planifié | 3 | P1 |
| [US-038](./US-038-etat-combat-temps-reel.md) | État combat temps réel (SignalR) | 📝 Planifié | 5 | P1 |
| [US-039](./US-039-fin-combat.md) | Fin de combat et résumé | 📝 Planifié | 3 | P1 |

---

## 🏗️ Architecture Technique

### Backend (.NET 10)
- **Endpoints** : `CombatEndpoints.cs`, `DiceEndpoints.cs`
- **Services** : `CombatService`, `DiceService`, `InitiativeService`
- **Models** : `Combat`, `CombatParticipant`, `CombatRound`, `DiceRoll`
- **Hubs SignalR** : `CombatHub` (notifications temps réel)

### Frontend (Blazor Server)
- **Pages** : CombatView.razor
- **Components** : 
  - `InitiativeTracker.razor` (ordre des tours)
  - `DiceRoller.razor` (lanceur de dés)
  - `CombatActions.razor` (actions disponibles)
  - `CombatLog.razor` (historique)
- **Services** : CombatService, DiceService, SignalRCombatService

### Base de Données
- **Tables** : 
  - `Combats` (1-N `CombatParticipants`, 1-N `CombatRounds`)
  - `CombatParticipants` (FK → `BaseCharacters`)
  - `DiceRolls` (historique des jets)

---

## 🎲 Système de Dés

### Types de Dés Supportés
- d4, d6, d8, d10, d12, d20, d100
- Notation : `XdY+Z` (ex: 2d6+3)
- Modificateurs : +/- valeur fixe

### Interface de Lancer
```
[Sélection type dé] [Nombre] [Modificateur]
     [d20]            [1]        [+5]
              [LANCER]
         
Résultat : 🎲 17 + 5 = 22
```

### Historique des Lancers
- Affichage en temps réel
- Filtrage par joueur/tour
- Export PDF du combat

---

## ⚔️ Flux de Combat

### 1. Déclenchement
- MJ sélectionne participants (joueurs + PNJ)
- Interface de combat s'ouvre pour tous

### 2. Initiative
- MJ saisit ordre d'initiative manuellement
- OU jets de dés (1d20 + bonus Dex)
- Ordre affiché dans tracker

### 3. Tours de Combat
- Notification "À votre tour !" au joueur actif
- Actions possibles :
  - **Attaque** : Jet d'attaque → Jet de dégâts
  - **Déplacement** : Déplacement libre
  - **Action spéciale** : Sort, compétence, etc.
  - **Passer** : Fin du tour
- MJ applique conséquences manuellement

### 4. Fin de Combat
- MJ termine le combat (victoire/défaite/fuite)
- Résumé généré :
  - Durée, tours, participants
  - Dégâts infligés/reçus
  - Jets de dés effectués

---

## 🧪 Tests

### Tests Unitaires
- Calcul d'initiative
- Validation jets de dés
- Gestion ordre des tours
- Détection fin de combat

### Tests d'Intégration
- Flux complet de combat
- Notifications SignalR
- Persistance état combat

### Tests E2E
- Combat multi-joueurs
- Déconnexion/reconnexion pendant combat
- Ajout participant en cours de combat

---

## 🔗 Dépendances

### Dépend de
- [Epic 2](../02-Epic-Gestion-Parties/) - Sessions
- [Epic 3](../03-Epic-Personnages-PNJ/) - Personnages/PNJ

### Bloque
- [Epic 6](../06-Epic-Mode-DnD/) - Combat D&D automatisé

---

## 📈 Progression

```
Complété : [░░░░░░░░░░] 0% (0/8 US)
Planifié  : 8 US
```

---

## 📚 Documentation Associée

- [SIGNALR_TEMPS_REEL.md](../../instructions/technique/SIGNALR_TEMPS_REEL.md) - CombatHub
- [SPECIFICATION_FONCTIONNELLE.md](../../instructions/SPECIFICATION_FONCTIONNELLE.md) - Combat générique

---

**Dernière mise à jour** : 15 octobre 2025

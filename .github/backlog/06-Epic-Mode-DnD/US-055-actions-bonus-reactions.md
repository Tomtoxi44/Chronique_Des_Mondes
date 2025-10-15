# US-055 - Actions Bonus et Réactions

## 📝 Description

**En tant que** Joueur D&D 5e  
**Je veux** gérer actions, actions bonus et réactions  
**Afin de** respecter l'économie d'actions D&D

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Pendant tour combat, indicateurs :
  - [ ] ✅ Action (disponible/utilisée)
  - [ ] ⚡ Action Bonus (disponible/utilisée)
  - [ ] 🛡️ Réaction (disponible/utilisée)
  - [ ] 🏃 Mouvement (30 pieds par défaut)
- [ ] Types d'actions :
  - [ ] **Action** : Attaque, Lancer Sort (1 action), Aide, etc.
  - [ ] **Action Bonus** : Second vent (Guerrier), Sort 1 action bonus
  - [ ] **Réaction** : Attaque d'opportunité, Bouclier (sort)
- [ ] Validation : Impossible utiliser 2 actions bonus
- [ ] Réinitialisation début tour suivant
- [ ] MJ voit état actions tous participants

### Techniques
- [ ] Extension `CombatParticipant` :
```json
{
  "actionUsed": false,
  "bonusActionUsed": false,
  "reactionUsed": false,
  "movementRemaining": 30
}
```

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CombatService.UseAction_ValidAction_MarksUsed()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Ajouter `CombatParticipant.ActionEconomy` (JSON)
- [ ] Créer `CombatService.UseActionAsync(participantId, actionType)`
- [ ] Réinitialisation début tour

### Frontend
- [ ] Composant `ActionEconomy.razor`
- [ ] Icônes visuelles (checkboxes stylisées)

---

## 📊 Estimation

**Story Points** : 5

---

## ✅ Definition of Done

- [ ] Économie actions fonctionnelle
- [ ] Indicateurs visuels clairs
- [ ] Validation règles D&D
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 15

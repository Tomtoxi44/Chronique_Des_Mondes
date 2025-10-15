# US-037 - Transmission Résultats au MJ

## 📝 Description

**En tant que** Joueur  
**Je veux** transmettre mes résultats de jets au Maître du Jeu  
**Afin que** le MJ valide et applique les effets

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Après jet de dés, bouton "Envoyer au MJ"
- [ ] Notification MJ : "{PlayerName} a lancé 2d6+3 = 12"
- [ ] MJ voit panneau "Jets en Attente"
- [ ] Actions : Approuver, Rejeter, Ignorer
- [ ] Approuver → Jet ajouté au Combat Log
- [ ] Mode auto-approbation (optionnel dans settings)

### Techniques
- [ ] SignalR : `DiceRollSentToGM`
- [ ] Notification temps réel

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CombatService.SendRollToGM_NotifiesGM()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `CombatService.SendRollToGMAsync(combatId, rollDto, userId)`
- [ ] SignalR notification

### Frontend
- [ ] Composant `PendingRolls.razor` (pour MJ)
- [ ] Bouton "Envoyer au MJ" dans DiceRoller

---

## 📊 Estimation

**Story Points** : 3

---

## ✅ Definition of Done

- [ ] Envoi jets au MJ fonctionnel
- [ ] Panneau MJ affiche jets
- [ ] Approbation/rejet
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 10

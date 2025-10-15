# US-038 - État Combat Temps Réel (SignalR)

## 📝 Description

**En tant que** Participant au Combat  
**Je veux** voir l'état du combat mis à jour en temps réel  
**Afin de** suivre les actions sans rafraîchir

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Mise à jour HP instantanée pour tous
- [ ] Actions ajoutées au log en temps réel
- [ ] Tours changés sans latence
- [ ] Indicateurs visuels (animations HP)
- [ ] Reconnexion automatique si déconnexion

### Techniques
- [ ] SignalR : `HPUpdated`, `ActionPerformed`, `TurnChanged`

---

## 🧪 Tests

### Tests d'Intégration
- [ ] SignalR notifications reçues par tous clients

---

## 🔧 Tâches Techniques

### Backend
- [ ] `CombatHub` avec toutes méthodes SignalR

### Frontend
- [ ] `SignalRCombatService` complet
- [ ] Gestion reconnexion

---

## 📊 Estimation

**Story Points** : 5

---

## ✅ Definition of Done

- [ ] SignalR complet et fonctionnel
- [ ] Pas de lag perceptible
- [ ] Reconnexion automatique
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 10

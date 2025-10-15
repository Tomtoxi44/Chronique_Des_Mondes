# US-048 - Commerce et Échange d'Objets

## 📝 Description

**En tant que** Joueur  
**Je veux** échanger des objets avec d'autres joueurs  
**Afin de** optimiser notre équipement de groupe

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Bouton "Échanger" sur objet inventaire
- [ ] Modal sélection destinataire (joueurs session active)
- [ ] Notification destinataire en temps réel (SignalR)
- [ ] Destinataire : Accepter / Refuser
- [ ] Si accepté → Objet transféré entre inventaires
- [ ] Historique échanges dans session
- [ ] MJ voit tous échanges
- [ ] MJ peut forcer transfert objet

### Techniques
- [ ] Endpoint : `POST /api/inventory/{itemId}/transfer`
- [ ] Body :
```json
{
  "fromCharacterId": "guid",
  "toCharacterId": "guid",
  "quantity": 1
}
```
- [ ] SignalR : `ItemTransferRequest`, `ItemTransferred`

---

## 🧪 Tests

### Tests Unitaires
- [ ] `InventoryService.TransferItem_ValidTransfer_TransfersItem()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `InventoryService.RequestTransferAsync(itemId, dto)`
- [ ] Créer `InventoryService.AcceptTransferAsync(requestId)`
- [ ] SignalR notifications

### Frontend
- [ ] Modal `TransferItemModal.razor`
- [ ] Composant `TransferNotification.razor`

---

## 📊 Estimation

**Story Points** : 3

---

## ✅ Definition of Done

- [ ] Échange objets fonctionnel
- [ ] Notifications temps réel
- [ ] Accepter/refuser
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 12

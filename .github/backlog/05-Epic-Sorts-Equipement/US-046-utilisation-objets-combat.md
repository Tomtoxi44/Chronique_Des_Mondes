# US-046 - Utilisation d'Objets en Combat

## 📝 Description

**En tant que** Joueur  
**Je veux** utiliser objets et consommables pendant mon tour  
**Afin de** bénéficier de leurs effets en combat

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Pendant son tour, action "Utiliser Objet"
- [ ] Liste objets inventaire :
  - [ ] Filtré consommables + objets magiques
  - [ ] Affichage charges restantes
- [ ] Sélection objet → Effets appliqués :
  - [ ] Potion soin → Restaure HP (jet dés)
  - [ ] Parchemin → Lance sort (consomme parchemin)
  - [ ] Objet magique → Active pouvoir (consomme charge)
- [ ] Décrémentation quantité/charges
- [ ] Suppression auto si quantité = 0
- [ ] Ajout action au Combat Log
- [ ] Notification temps réel autres joueurs

### Techniques
- [ ] Endpoint : `POST /api/combats/{combatId}/use-item`
- [ ] Body :
```json
{
  "userId": "guid",
  "itemId": "guid",
  "targetId": "guid"
}
```
- [ ] SignalR : `ItemUsed` event

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CombatService.UseItem_Potion_RestoresHP()`
- [ ] `CombatService.UseItem_NoCharges_ThrowsException()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `CombatService.UseItemAsync(combatId, dto)`
- [ ] Parser effets objets
- [ ] Gérer quantités/charges

### Frontend
- [ ] Modal `UseItemModal.razor`

---

## 📊 Estimation

**Story Points** : 5

---

## ✅ Definition of Done

- [ ] Utilisation objets en combat
- [ ] Effets appliqués
- [ ] Gestion charges/quantités
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 12

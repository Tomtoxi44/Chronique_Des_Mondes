# US-043 - Lancer un Sort en Combat

## 📝 Description

**En tant que** Joueur lanceur de sorts  
**Je veux** lancer un sort pendant mon tour de combat  
**Afin d** utiliser mes capacités magiques

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Pendant son tour, bouton "Lancer un Sort"
- [ ] Modal sélection sort :
  - [ ] Filtré par sorts préparés uniquement
  - [ ] Filtré par slots disponibles (niveau)
  - [ ] Affichage temps d'incantation
- [ ] Sélection cible(s) selon portée
- [ ] Si sort nécessite jet attaque → Lanceur dés automatique
- [ ] Si sort nécessite jet sauvegarde → Notification cible(s)
- [ ] Consommation slot automatique
- [ ] Application dégâts/soins automatique
- [ ] Ajout action au Combat Log
- [ ] Effets spéciaux affichés (buffs, debuffs)

### Techniques
- [ ] Endpoint : `POST /api/combats/{combatId}/cast-spell`
- [ ] Body :
```json
{
  "casterId": "guid",
  "spellId": "guid",
  "targetIds": ["guid"],
  "slotLevel": 3
}
```
- [ ] SignalR : `SpellCast` event

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CombatService.CastSpell_ValidSpell_ConsumesSlot()`
- [ ] `CombatService.CastSpell_NoSlotsLeft_ThrowsException()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `CombatService.CastSpellAsync(combatId, dto)`
- [ ] Validation slots disponibles
- [ ] Calcul dégâts/soins
- [ ] Application effets

### Frontend
- [ ] Modal `CastSpellModal.razor`
- [ ] Sélection cibles
- [ ] Animation sort (optionnel)

---

## 📊 Estimation

**Story Points** : 8

---

## ✅ Definition of Done

- [ ] Lancer sorts en combat fonctionnel
- [ ] Consommation slots
- [ ] Jets automatiques
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 11

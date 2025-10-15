# US-047 - Harmonisation (Attunement) d'Objets

## 📝 Description

**En tant que** Joueur  
**Je veux** m'harmoniser avec des objets magiques  
**Afin de** débloquer leurs pouvoirs

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Section "Objets Harmonisés" dans fiche personnage
- [ ] Limite 3 objets harmonisés simultanément (règle D&D)
- [ ] Bouton "S'harmoniser" sur objet magique nécessitant attunement
- [ ] Vérification prérequis (classe, niveau, race, etc.)
- [ ] Processus harmonisation = Repos Court (1 heure RP)
- [ ] Bouton "Rompre Harmonisation"
- [ ] Objets harmonisés :
  - [ ] Badge visuel distinct
  - [ ] Propriétés actives affichées
- [ ] MJ peut forcer harmonisation/rupture

### Techniques
- [ ] JSON field `AttunedItems` dans Character :
```json
{
  "itemIds": ["guid1", "guid2"],
  "maxSlots": 3
}
```

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CharacterService.AttuneItem_ValidItem_AddsToAttuned()`
- [ ] `CharacterService.AttuneItem_ExceedsLimit_ThrowsException()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Ajouter `Character.AttunedItems` (JSON)
- [ ] Créer `CharacterService.AttuneItemAsync(characterId, itemId)`
- [ ] Créer `CharacterService.BreakAttunementAsync(characterId, itemId)`
- [ ] Validation prérequis

### Frontend
- [ ] Composant `AttunedItems.razor`
- [ ] Modal `AttuneItemModal.razor`

---

## 📊 Estimation

**Story Points** : 3

---

## ✅ Definition of Done

- [ ] Harmonisation fonctionnelle
- [ ] Limite 3 objets
- [ ] Vérification prérequis
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 12

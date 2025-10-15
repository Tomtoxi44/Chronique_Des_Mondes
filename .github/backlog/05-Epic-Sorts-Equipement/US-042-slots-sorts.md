# US-042 - Gestion des Slots de Sorts

## 📝 Description

**En tant que** Joueur lanceur de sorts  
**Je veux** voir et gérer mes emplacements de sorts  
**Afin de** savoir combien de sorts je peux encore lancer

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Section "Emplacements de Sorts" dans fiche personnage
- [ ] Affichage par niveau (1-9) :
  - [ ] Nombre total slots
  - [ ] Nombre slots utilisés
  - [ ] Slots restants (visuel cercles remplis/vides)
- [ ] Bouton "Utiliser Slot" → Sélection niveau → Décrémente compteur
- [ ] Bouton "Repos Court" → Restaure slots selon règles classe (futur)
- [ ] Bouton "Repos Long" → Restaure tous slots
- [ ] MJ peut modifier slots manuellement
- [ ] Slots sauvegardés en temps réel

### Techniques
- [ ] JSON field `SpellSlots` dans Character :
```json
{
  "1": { "total": 4, "used": 2 },
  "2": { "total": 3, "used": 1 },
  "3": { "total": 2, "used": 0 }
}
```

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CharacterService.UseSpellSlot_ValidLevel_DecrementsSlot()`
- [ ] `CharacterService.LongRest_RestoresAllSlots()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Ajouter `Character.SpellSlots` (JSON)
- [ ] Créer `CharacterService.UseSpellSlotAsync(characterId, level)`
- [ ] Créer `CharacterService.RestoreSpellSlotsAsync(characterId, restType)`

### Frontend
- [ ] Composant `SpellSlots.razor`
- [ ] Visuel slots (cercles style D&D Beyond)

---

## 📊 Estimation

**Story Points** : 3

---

## ✅ Definition of Done

- [ ] Slots de sorts fonctionnels
- [ ] Utilisation/restauration
- [ ] Visuel intuitif
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 11

# US-054 - Points d'Expérience et Montée de Niveau

## 📝 Description

**En tant que** Joueur D&D 5e  
**Je veux** gagner XP et monter de niveau automatiquement  
**Afin de** progresser selon table XP D&D 5e

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] MJ peut attribuer XP à joueurs :
  - [ ] Fin combat (US-039) → Bouton "Attribuer XP"
  - [ ] Récompense manuelle
- [ ] Table XP D&D 5e (niveau 1-20) :
  - [ ] Niveau 2 : 300 XP
  - [ ] Niveau 3 : 900 XP
  - [ ] Niveau 4 : 2700 XP
  - [ ] Niveau 5 : 6500 XP
  - [ ] ... Niveau 20 : 355 000 XP
- [ ] Montée niveau automatique si XP atteint seuil
- [ ] Notification joueur : "Niveau supérieur atteint !"
- [ ] Modal montée niveau :
  - [ ] Lancer dé de vie + Mod Con → HP max augmente
  - [ ] Nouvelles capacités classe affichées
  - [ ] Sélection sorts si classe lanceur
  - [ ] Augmentation caractéristique (niv 4, 8, 12, 16, 19) ou don
- [ ] Barre progression XP visuelle

### Techniques
- [ ] `Character.Experience` (int)
- [ ] Endpoint : `POST /api/characters/{characterId}/award-xp`

---

## 🧪 Tests

### Tests Unitaires
- [ ] `DndService.AwardXP_ThresholdReached_LevelsUp()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `DndService.AwardXPAsync(characterId, xp)`
- [ ] Créer `DndService.LevelUpAsync(characterId, choices)`
- [ ] Table progression hardcodée

### Frontend
- [ ] Modal `LevelUpModal.razor`
- [ ] Barre XP

---

## 📊 Estimation

**Story Points** : 8

---

## ✅ Definition of Done

- [ ] Attribution XP fonctionnelle
- [ ] Montée niveau automatique
- [ ] Modal niveau avec choix
- [ ] Table XP complète
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 14

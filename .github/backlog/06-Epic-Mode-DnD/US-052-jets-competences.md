# US-052 - Jets de Compétences et Sauvegardes

## 📝 Description

**En tant que** Joueur D&D 5e  
**Je veux** effectuer jets de compétences et sauvegardes avec modificateurs  
**Afin de** résoudre actions selon règles D&D

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Liste 18 compétences D&D :
  - [ ] **Force** : Athlétisme
  - [ ] **Dextérité** : Acrobaties, Discrétion, Escamotage
  - [ ] **Intelligence** : Arcanes, Histoire, Investigation, Nature, Religion
  - [ ] **Sagesse** : Dressage, Intuition, Médecine, Perception, Survie
  - [ ] **Charisme** : Intimidation, Persuasion, Représentation, Tromperie
- [ ] Bouton jet compétence → Lanceur d20 automatique
- [ ] Calcul : d20 + Mod Caractéristique + (Bonus Maîtrise si maîtrisé)
- [ ] Jets de sauvegarde (6 caractéristiques)
- [ ] Expertise (Roublard/Barde) : Double bonus maîtrise
- [ ] Affichage résultat détaillé : "d20(15) + Dex(+2) + Maîtrise(+2) = 19"
- [ ] Historique jets

### Techniques
- [ ] Endpoint : `POST /api/characters/{characterId}/skill-check`
- [ ] Body : `{ "skill": "Stealth", "hasAdvantage": false }`

---

## 🧪 Tests

### Tests Unitaires
- [ ] `DndService.CalculateSkillCheck_ProficientStealth_AddsBonus()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `DndService.RollSkillCheckAsync(characterId, skill, advantage)`
- [ ] Créer `DndService.RollSavingThrowAsync(characterId, ability)`

### Frontend
- [ ] Composant `SkillList.razor`
- [ ] Modal `RollResultModal.razor`

---

## 📊 Estimation

**Story Points** : 5

---

## ✅ Definition of Done

- [ ] Jets compétences fonctionnels
- [ ] 18 compétences D&D
- [ ] Jets sauvegardes
- [ ] Expertise supportée
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 14

# US-049 - Fiche de Personnage D&D 5e

## 📝 Description

**En tant que** Joueur D&D 5e  
**Je veux** une fiche personnage complète conforme aux règles D&D 5e  
**Afin de** gérer mon personnage selon le système officiel

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Activation mode D&D lors création campagne (GameType = DnD5e)
- [ ] Formulaire création personnage D&D :
  - [ ] **Caractéristiques** : Force, Dextérité, Constitution, Intelligence, Sagesse, Charisme (3-20)
  - [ ] **Race** : Humain, Elfe, Nain, etc. (US-050)
  - [ ] **Classe** : Guerrier, Magicien, Clerc, etc. (US-051)
  - [ ] **Niveau** (1-20)
  - [ ] **Background** : Acolyte, Criminel, Sage, etc.
  - [ ] **Alignement** : LB, NB, CB, LN, N, CN, LM, NM, CM
- [ ] Calculs automatiques :
  - [ ] **Modificateurs** : (Score - 10) / 2
  - [ ] **Classe d'Armure (CA)** : 10 + Dex + Armure + Bouclier
  - [ ] **Initiative** : Modificateur Dextérité
  - [ ] **Points de Vie** : Dé de vie classe × niveau + (Mod Con × niveau)
  - [ ] **Bonus Maîtrise** : +2 (niv 1-4), +3 (5-8), +4 (9-12), +5 (13-16), +6 (17-20)
- [ ] Affichage fiche style D&D Beyond

### Techniques
- [ ] Extension Character avec `DndStats` (JSON) :
```json
{
  "strength": 16,
  "dexterity": 14,
  "constitution": 15,
  "intelligence": 8,
  "wisdom": 12,
  "charisma": 10,
  "race": "Human",
  "class": "Fighter",
  "level": 3,
  "background": "Soldier",
  "alignment": "LawfulGood",
  "proficiencyBonus": 2,
  "hitDice": "1d10",
  "initiative": 2,
  "armorClass": 18,
  "speed": 30
}
```

---

## 🧪 Tests

### Tests Unitaires
- [ ] `DndService.CalculateModifier_Score16_Returns3()`
- [ ] `DndService.CalculateProficiencyBonus_Level5_Returns3()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `DndService` avec calculs
- [ ] Validation règles D&D

### Frontend
- [ ] Composant `DndCharacterSheet.razor`
- [ ] Visuel style D&D officiel

---

## 📊 Estimation

**Story Points** : 8

---

## ✅ Definition of Done

- [ ] Fiche D&D fonctionnelle
- [ ] Calculs automatiques
- [ ] Visuel conforme
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 13

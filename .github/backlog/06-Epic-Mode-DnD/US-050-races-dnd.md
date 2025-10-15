# US-050 - Races D&D 5e

## 📝 Description

**En tant que** Joueur D&D 5e  
**Je veux** choisir une race pour mon personnage  
**Afin de** bénéficier des traits raciaux

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Bibliothèque races D&D 5e :
  - [ ] Humain (+1 toutes caractéristiques, Langue bonus)
  - [ ] Elfe (Dex +2, Vision Obscurité, Transe)
  - [ ] Nain (Con +2, Résistance poison, Vision Obscurité)
  - [ ] Halfelin (Dex +2, Chanceux, Brave)
  - [ ] Demi-Elfe (Cha +2, +1 deux stats, Polyvalence compétences)
  - [ ] Demi-Orc (For +2, Con +1, Endurance Implacable)
  - [ ] Tieffelin (Cha +2, Int +1, Résistance feu, Sorts innés)
  - [ ] Gnome (Int +2, Petite taille, Vision Obscurité)
  - [ ] Drakéide (For +2, Cha +1, Arme de souffle)
- [ ] Affichage traits raciaux détaillés
- [ ] Application automatique bonus stats
- [ ] MJ peut créer races custom

### Techniques
- [ ] Table `DndRaces` pré-remplie

---

## 🧪 Tests

### Tests Unitaires
- [ ] `DndService.ApplyRacialBonuses_Human_Adds1ToAll()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `DndRace` :
```csharp
public class DndRace
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StatBonuses { get; set; } // JSON: { "Strength": 2 }
    public int Size { get; set; } // 0=Small, 1=Medium
    public int Speed { get; set; }
    public string Traits { get; set; } // JSON array traits
    public string Languages { get; set; } // JSON array
    public bool IsOfficial { get; set; }
}
```
- [ ] Seed data avec races officielles

---

## 📊 Estimation

**Story Points** : 5

---

## ✅ Definition of Done

- [ ] 9 races officielles disponibles
- [ ] Traits appliqués automatiquement
- [ ] Création races custom
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 13

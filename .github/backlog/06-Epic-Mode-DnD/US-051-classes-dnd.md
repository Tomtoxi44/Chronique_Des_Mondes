# US-051 - Classes D&D 5e

## 📝 Description

**En tant que** Joueur D&D 5e  
**Je veux** choisir une classe pour mon personnage  
**Afin de** définir mes capacités et progression

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Bibliothèque 12 classes officielles :
  - [ ] Barbare (d12, For/Con, Rage)
  - [ ] Barde (d8, Cha, Sorts, Inspiration bardique)
  - [ ] Clerc (d8, Sag, Sorts divins, Canalisation énergie)
  - [ ] Druide (d8, Sag, Sorts nature, Forme sauvage)
  - [ ] Guerrier (d10, For/Dex, Second souffle, Action surge)
  - [ ] Moine (d8, Dex/Sag, Arts martiaux, Ki)
  - [ ] Paladin (d10, For/Cha, Imposition mains, Châtiment)
  - [ ] Rôdeur (d10, Dex/Sag, Ennemi juré, Terrain favori)
  - [ ] Roublard (d8, Dex, Attaque sournoise, Expertise)
  - [ ] Ensorceleur (d6, Cha, Sorts, Métamagie)
  - [ ] Occultiste (d8, Cha, Sorts, Invocations occultes)
  - [ ] Magicien (d6, Int, Sorts arcanes, École magie)
- [ ] Affichage capacités par niveau
- [ ] Dé de vie automatique
- [ ] Maîtrises armes/armures
- [ ] Jets de sauvegarde maîtrisés
- [ ] Compétences de classe (2-4 selon classe)

### Techniques
- [ ] Table `DndClasses` avec progression

---

## 🧪 Tests

### Tests Unitaires
- [ ] `DndService.GetClassFeatures_Fighter_Level3_ReturnsActionSurge()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `DndClass` :
```csharp
public class DndClass
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string HitDice { get; set; } // "1d10"
    public string PrimaryStat { get; set; } // "Strength"
    public string SavingThrowProficiencies { get; set; } // JSON
    public string ArmorProficiencies { get; set; }
    public string WeaponProficiencies { get; set; }
    public string SkillChoices { get; set; } // JSON
    public int SkillChoiceCount { get; set; }
    public string Features { get; set; } // JSON: niveau → features
    public bool IsSpellcaster { get; set; }
    public string? SpellcastingAbility { get; set; }
}
```
- [ ] Seed 12 classes

---

## 📊 Estimation

**Story Points** : 8

---

## ✅ Definition of Done

- [ ] 12 classes disponibles
- [ ] Capacités par niveau
- [ ] Maîtrises appliquées
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 13

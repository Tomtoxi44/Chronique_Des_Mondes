# US-053 - Avantage et Désavantage

## 📝 Description

**En tant que** Joueur D&D 5e  
**Je veux** lancer dés avec avantage ou désavantage  
**Afin de** respecter règles D&D 5e

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Options lanceur dés :
  - [ ] **Normal** : 1d20
  - [ ] **Avantage** : 2d20 (prendre meilleur)
  - [ ] **Désavantage** : 2d20 (prendre pire)
- [ ] Affichage résultats : "Avantage : [12, 18] → 18"
- [ ] Application automatique selon conditions :
  - [ ] Attaque en mêlée contre ennemi à terre → Avantage
  - [ ] Attaque en mêlée avec ennemi invisible → Désavantage
  - [ ] Jets Discrétion avec armure lourde → Désavantage
- [ ] MJ peut forcer avantage/désavantage manuellement
- [ ] Règle : Avantage + Désavantage = Annulation (jet normal)

### Techniques
- [ ] Extension DiceRoller avec `hasAdvantage`, `hasDisadvantage`

---

## 🧪 Tests

### Tests Unitaires
- [ ] `DiceService.RollWithAdvantage_Returns_HigherRoll()`
- [ ] `DiceService.RollWithBoth_Cancels_ReturnsNormalRoll()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Modifier `DiceService.Roll()` :
```csharp
public DiceRollResultDto Roll(DiceRollDto dto)
{
    var rolls = new List<int>();
    
    if (dto.HasAdvantage && !dto.HasDisadvantage)
    {
        rolls.Add(Random.Shared.Next(1, 21));
        rolls.Add(Random.Shared.Next(1, 21));
        return new DiceRollResultDto { Rolls = rolls, Total = rolls.Max() };
    }
    else if (dto.HasDisadvantage && !dto.HasAdvantage)
    {
        rolls.Add(Random.Shared.Next(1, 21));
        rolls.Add(Random.Shared.Next(1, 21));
        return new DiceRollResultDto { Rolls = rolls, Total = rolls.Min() };
    }
    
    // Normal roll
    rolls.Add(Random.Shared.Next(1, 21));
    return new DiceRollResultDto { Rolls = rolls, Total = rolls[0] };
}
```

### Frontend
- [ ] Boutons radio : Normal / Avantage / Désavantage

---

## 📊 Estimation

**Story Points** : 3

---

## ✅ Definition of Done

- [ ] Avantage/Désavantage fonctionnel
- [ ] Annulation mutuelle
- [ ] Affichage clair
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 14

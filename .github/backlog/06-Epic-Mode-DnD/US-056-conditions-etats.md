# US-056 - Conditions et États

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** appliquer conditions D&D aux participants combat  
**Afin de** gérer effets (empoisonné, paralysé, etc.)

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] 15 conditions officielles D&D 5e :
  - [ ] **Aveuglé** : Désavantage attaques, avantage contre
  - [ ] **Charmé** : Ne peut attaquer charmeur
  - [ ] **Assourdi** : Échec auto jets Perception auditive
  - [ ] **Effrayé** : Désavantage si source visible
  - [ ] **Empoigné** : Vitesse 0
  - [ ] **Empoisonné** : Désavantage attaques et jets
  - [ ] **Incapacité** : Aucune action/réaction
  - [ ] **Invisible** : Considéré couvert total
  - [ ] **Paralysé** : Incapacité + échec sauvegarde For/Dex
  - [ ] **Pétrifié** : Paralysé + résistance dégâts
  - [ ] **À Terre** : Désavantage attaque, avantage contre en mêlée
  - [ ] **Entravé** : Vitesse 0, désavantage attaques Dex
  - [ ] **Étourdi** : Incapacité + échec sauvegarde For/Dex
  - [ ] **Inconscient** : À terre, incapacité, échec sauvegardes
  - [ ] **Épuisement** (6 niveaux) : Cumul malus
- [ ] MJ peut appliquer/retirer conditions sur participants
- [ ] Icône badge condition sur participant (hover = détails)
- [ ] Application automatique effets (désavantages, immunités)
- [ ] Durée condition (nombre tours ou permanent)

### Techniques
- [ ] Table `Conditions` pré-remplie
- [ ] Entity `CombatParticipantCondition` :
```csharp
public class CombatParticipantCondition
{
    public Guid Id { get; set; }
    public Guid CombatParticipantId { get; set; }
    public string ConditionName { get; set; }
    public int? RemainingDuration { get; set; } // Tours
    public DateTime AppliedAt { get; set; }
}
```

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CombatService.ApplyCondition_Poisoned_AppliesDisadvantage()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer table `CombatParticipantConditions`
- [ ] Créer `CombatService.ApplyConditionAsync(participantId, condition)`
- [ ] Décompte durée fin tour

### Frontend
- [ ] Modal `ApplyConditionModal.razor`
- [ ] Badges conditions sur participants

---

## 📊 Estimation

**Story Points** : 8

---

## ✅ Definition of Done

- [ ] 15 conditions officielles
- [ ] Application/retrait fonctionnel
- [ ] Effets automatiques
- [ ] Durée gérée
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 15

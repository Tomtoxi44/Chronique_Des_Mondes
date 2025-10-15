# US-035 - Actions de Combat

## 📝 Description

**En tant que** Joueur ou Maître du Jeu  
**Je veux** effectuer des actions pendant mon tour de combat  
**Afin de** interagir avec le système de combat

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Pendant son tour, participant voit panel "Actions Disponibles"
- [ ] Types d'actions :
  - [ ] **Attaque** : Sélection cible + jet dégâts (US-036)
  - [ ] **Déplacement** : Description textuelle (pas de map)
  - [ ] **Action Spéciale** : Texte libre (sort, compétence, etc.)
  - [ ] **Passer** : Ne rien faire
- [ ] Chaque action créée → Ajoutée au Combat Log
- [ ] **MJ** : Peut effectuer actions pour PNJ
- [ ] **Joueurs** : Actions uniquement à leur tour
- [ ] Bouton "Modifier HP" (MJ) pour appliquer dégâts/soins manuellement

### Techniques
- [ ] Endpoint : `POST /api/combats/{combatId}/actions`
- [ ] Body :
```json
{
  "participantId": "guid",
  "actionType": "Attack",
  "targetId": "guid",
  "description": "Attaque à l'épée",
  "damage": 8
}
```
- [ ] Response 201 : ActionDto créée
- [ ] SignalR : Notification action à tous participants

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CombatService.PerformAction_ValidAction_CreatesAction()`
- [ ] `CombatService.PerformAction_NotPlayerTurn_ThrowsException()`
- [ ] `CombatService.UpdateHP_AppliesDamage()`

### Tests d'Intégration
- [ ] `CombatEndpoint_PerformAction_SavesInDatabase()`

### Tests E2E
- [ ] Joueur attaque PNJ → Dégâts appliqués → Log mis à jour

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `CombatAction` :
```csharp
public class CombatAction
{
    public Guid Id { get; set; }
    public Guid CombatId { get; set; }
    public Combat Combat { get; set; }
    
    public Guid ActorId { get; set; } // CombatParticipant
    public CombatParticipant Actor { get; set; }
    
    public Guid? TargetId { get; set; }
    public CombatParticipant? Target { get; set; }
    
    public CombatActionType ActionType { get; set; }
    public string Description { get; set; }
    public int? Damage { get; set; }
    public int RoundNumber { get; set; }
    public DateTime PerformedAt { get; set; }
}

public enum CombatActionType
{
    Attack = 0,
    Movement = 1,
    Special = 2,
    Pass = 3
}
```
- [ ] Créer `CombatService.PerformActionAsync(combatId, dto, userId)`
- [ ] Créer `CombatService.UpdateHPAsync(combatId, participantId, newHP, userId)`
- [ ] Créer endpoints

### Frontend
- [ ] Créer composant `CombatActions.razor`
- [ ] Créer composant `CombatLog.razor`

### Base de Données
- [ ] Migration : Créer table `CombatActions`

---

## 📊 Estimation

**Story Points** : 5

---

## ✅ Definition of Done

- [ ] Actions de combat fonctionnelles
- [ ] Combat Log affiché
- [ ] HP modifiables
- [ ] SignalR notifications
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 10

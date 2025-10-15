# US-034 - Gestion des Tours de Combat

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** gérer le déroulement des tours de combat  
**Afin de** structurer les actions et maintenir l'ordre de jeu

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] InitiativeTracker affiche participant actif (bordure verte + icône ⏸️)
- [ ] **MJ** : Bouton "Tour Suivant" → Passe au participant suivant
- [ ] Si dernier participant → Incrémente round + retour au premier
- [ ] Notification temps réel : "C'est au tour de {Name}"
- [ ] **Joueurs** : Bouton "Terminer mon Tour" (visible uniquement à leur tour)
- [ ] Clic → Désactive actions + notifie MJ
- [ ] MJ voit notification : "{PlayerName} a terminé son tour"
- [ ] Compteur tours par participant (pour statistiques)
- [ ] Bouton "Passer le Tour" (MJ uniquement) si participant absent/inconscient

### Techniques
- [ ] Endpoint : `POST /api/combats/{combatId}/next-turn`
- [ ] Response 200 :
```json
{
  "currentRound": 2,
  "currentTurnIndex": 3,
  "currentParticipant": { ... }
}
```
- [ ] SignalR `CombatHub.TurnChanged` :
```csharp
await Clients.Group($"Combat-{combatId}")
    .SendAsync("TurnChanged", new
    {
        CurrentRound = combat.CurrentRound,
        CurrentTurnIndex = combat.CurrentTurnIndex,
        CurrentParticipant = participant.ToDto()
    });
```

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CombatService.NextTurn_AdvancesToNextParticipant()`
- [ ] `CombatService.NextTurn_LastParticipant_IncrementsRound()`
- [ ] `CombatService.NextTurn_NonGameMaster_ThrowsUnauthorizedException()`
- [ ] `CombatService.EndPlayerTurn_NotifiesGameMaster()`

### Tests d'Intégration
- [ ] `CombatEndpoint_NextTurn_UpdatesDatabase()`
- [ ] `CombatEndpoint_NextTurn_NotifiesViaSignalR()`

### Tests E2E
- [ ] MJ clique "Tour Suivant" → Participant actif change pour tous
- [ ] Joueur clique "Terminer mon Tour" → MJ reçoit notification
- [ ] Dernier tour round 1 → Passe round 2, participant 1

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `CombatRound` (logging) :
```csharp
public class CombatRound
{
    public Guid Id { get; set; }
    public Guid CombatId { get; set; }
    public Combat Combat { get; set; }
    
    public int RoundNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    
    // Actions réalisées ce round (optional, pour historique)
    public ICollection<CombatAction> Actions { get; set; }
}
```
- [ ] Créer `CombatService.NextTurnAsync(combatId, userId)` :
```csharp
public async Task<CombatTurnDto> NextTurnAsync(Guid combatId, Guid userId)
{
    var combat = await _context.Combats
        .Include(c => c.Session.Campaign)
        .Include(c => c.Participants)
        .FirstOrDefaultAsync(c => c.Id == combatId);
    
    if (combat?.Session.Campaign.CreatedBy != userId)
        throw new UnauthorizedException();
    
    // Obtenir participants ordonnés par initiative
    var orderedParticipants = combat.Participants
        .Where(p => !p.IsDefeated)
        .OrderByDescending(p => p.InitiativeScore)
        .ThenBy(p => p.DisplayName)
        .ToList();
    
    if (!orderedParticipants.Any())
        throw new ValidationException("Aucun participant actif");
    
    // Avancer au suivant
    combat.CurrentTurnIndex++;
    
    // Si fin de round
    if (combat.CurrentTurnIndex >= orderedParticipants.Count)
    {
        combat.CurrentRound++;
        combat.CurrentTurnIndex = 0;
        
        // Logger fin de round
        _context.CombatRounds.Add(new CombatRound
        {
            CombatId = combatId,
            RoundNumber = combat.CurrentRound - 1,
            StartedAt = DateTime.UtcNow.AddMinutes(-1), // Approximation
            EndedAt = DateTime.UtcNow
        });
    }
    
    await _context.SaveChangesAsync();
    
    var currentParticipant = orderedParticipants[combat.CurrentTurnIndex];
    
    // Notifier via SignalR
    await _combatHub.Clients.Group($"Combat-{combatId}")
        .SendAsync("TurnChanged", new CombatTurnDto
        {
            CurrentRound = combat.CurrentRound,
            CurrentTurnIndex = combat.CurrentTurnIndex,
            CurrentParticipant = currentParticipant.ToDto()
        });
    
    return new CombatTurnDto
    {
        CurrentRound = combat.CurrentRound,
        CurrentTurnIndex = combat.CurrentTurnIndex,
        CurrentParticipant = currentParticipant.ToDto()
    };
}
```
- [ ] Créer `CombatService.EndPlayerTurnAsync(combatId, userId)` :
```csharp
public async Task EndPlayerTurnAsync(Guid combatId, Guid userId)
{
    var combat = await _context.Combats
        .Include(c => c.Session.Campaign)
        .Include(c => c.Participants)
            .ThenInclude(p => p.Character)
        .FirstOrDefaultAsync(c => c.Id == combatId);
    
    // Vérifier que c'est bien le tour du joueur
    var orderedParticipants = combat.Participants
        .Where(p => !p.IsDefeated)
        .OrderByDescending(p => p.InitiativeScore)
        .ToList();
    
    var currentParticipant = orderedParticipants[combat.CurrentTurnIndex];
    
    if (currentParticipant.Character?.OwnerId != userId)
        throw new UnauthorizedException("Ce n'est pas votre tour");
    
    // Notifier MJ
    await _combatHub.Clients.Group($"Combat-{combatId}")
        .SendAsync("PlayerTurnEnded", new
        {
            ParticipantName = currentParticipant.DisplayName,
            UserId = userId
        });
}
```
- [ ] Créer endpoints :
  - [ ] `POST /api/combats/{combatId}/next-turn` [Authorize(Roles = "GameMaster")]
  - [ ] `POST /api/combats/{combatId}/end-turn` [Authorize]

### Frontend
- [ ] Mettre à jour `InitiativeTracker.razor` :
```razor
<div class="participant-item @GetActiveClass(index) @GetDefeatedClass(participant)">
    <!-- Contenu existant -->
    
    @if (index == CurrentTurnIndex)
    {
        <span class="active-indicator">⏸️ Tour actif</span>
    }
</div>

@code {
    private string GetActiveClass(int index)
    {
        return index == CurrentTurnIndex ? "active" : "";
    }

    private string GetDefeatedClass(CombatParticipantDto p)
    {
        return p.IsDefeated ? "defeated" : "";
    }
}
```
- [ ] Créer composant `TurnControls.razor` :
```razor
<div class="turn-controls">
    <div class="round-indicator">
        <h3>Round @CurrentRound</h3>
    </div>
    
    @if (IsGameMaster)
    {
        <button @onclick="NextTurn" class="btn-primary">
            Tour Suivant →
        </button>
        <button @onclick="SkipTurn" class="btn-secondary">
            Passer le Tour
        </button>
    }
    else if (IsPlayerTurn)
    {
        <button @onclick="EndMyTurn" class="btn-primary">
            Terminer mon Tour
        </button>
    }
    else
    {
        <p class="text-muted">En attente...</p>
    }
</div>

@code {
    [Parameter] public Guid CombatId { get; set; }
    [Parameter] public int CurrentRound { get; set; }
    [Parameter] public bool IsGameMaster { get; set; }
    [Parameter] public bool IsPlayerTurn { get; set; }

    private async Task NextTurn()
    {
        await Http.PostAsync($"/api/combats/{CombatId}/next-turn", null);
    }

    private async Task EndMyTurn()
    {
        await Http.PostAsync($"/api/combats/{CombatId}/end-turn", null);
        Toast.Info("Vous avez terminé votre tour");
    }

    private async Task SkipTurn()
    {
        await NextTurn(); // Même logique
    }
}
```
- [ ] Mettre à jour `SignalRCombatService` :
```csharp
_connection.On<CombatTurnDto>("TurnChanged", turn =>
{
    OnTurnChanged?.Invoke(turn);
});

_connection.On<PlayerTurnEndedDto>("PlayerTurnEnded", data =>
{
    OnPlayerTurnEnded?.Invoke(data);
});
```

### Base de Données
- [ ] Migration : Créer table `CombatRounds`

---

## 🔗 Dépendances

### Dépend de
- [US-033](./US-033-initiative-manuelle.md) - Ordre initiative défini

### Bloque
- [US-035](./US-035-actions-combat.md) - Actions pendant tour

---

## 📊 Estimation

**Story Points** : 8

**Détails** :
- Complexité : Haute (Logique tours, rounds, SignalR)
- Effort : 2-3 jours
- Risques : Synchronisation état combat

---

## 📝 Notes Techniques

### Skip Defeated Participants
```csharp
var orderedParticipants = combat.Participants
    .Where(p => !p.IsDefeated)
    .OrderByDescending(p => p.InitiativeScore)
    .ToList();
```

### CSS Active Participant
```css
.participant-item.active {
    border: 3px solid #4caf50;
    box-shadow: 0 0 10px rgba(76, 175, 80, 0.5);
}

.participant-item.defeated {
    opacity: 0.5;
    filter: grayscale(100%);
}
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Bouton "Tour Suivant" fonctionnel
- [ ] Incrémentation rounds automatique
- [ ] Indication participant actif
- [ ] Joueurs peuvent terminer leur tour
- [ ] SignalR notifications temps réel
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 9

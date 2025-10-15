# US-033 - Calcul Initiative Manuel

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** définir manuellement l'ordre d'initiative du combat  
**Afin de** organiser les tours de jeu

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Dans CombatView, composant "Initiative Tracker"
- [ ] Liste tous participants (Joueurs + PNJ) avec :
  - [ ] Avatar
  - [ ] Nom
  - [ ] HP actuel/max (barre visuelle)
  - [ ] Input "Initiative" (nombre)
- [ ] **MJ** : Peut saisir score initiative pour TOUS les participants
- [ ] **Joueurs** : Peuvent saisir leur propre score uniquement
- [ ] Bouton "Trier par Initiative" (MJ uniquement)
- [ ] Clic → Réorganisation automatique (score descendant)
- [ ] Indicateur visuel du participant actif (bordure verte)
- [ ] Mise à jour temps réel via SignalR
- [ ] Alternative : Bouton "Jet d'Initiative Automatique" (d20 + bonus si D&D)

### Techniques
- [ ] Endpoint : `PATCH /api/combats/{combatId}/initiative`
- [ ] Body :
```json
{
  "participants": [
    {
      "participantId": "guid",
      "initiativeScore": 18
    }
  ]
}
```
- [ ] Response 200 : Liste participants ordonnés
- [ ] SignalR `CombatHub.InitiativeUpdated` :
```csharp
await Clients.Group($"Combat-{combatId}")
    .SendAsync("InitiativeUpdated", orderedParticipants);
```

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CombatService.UpdateInitiative_ValidScores_UpdatesParticipants()`
- [ ] `CombatService.UpdateInitiative_SortsDescending()`
- [ ] `CombatService.UpdateInitiative_PlayerCanOnlyUpdateOwn()`

### Tests d'Intégration
- [ ] `CombatEndpoint_UpdateInitiative_SavesInDatabase()`
- [ ] `CombatEndpoint_UpdateInitiative_NotifiesViaSignalR()`

### Tests E2E
- [ ] MJ saisit initiatives → Clique "Trier" → Ordre mis à jour pour tous
- [ ] Joueur saisit sa propre initiative → MJ la voit instantanément

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `CombatService.UpdateInitiativeAsync(combatId, updates, userId)` :
```csharp
public async Task<List<CombatParticipantDto>> UpdateInitiativeAsync(
    Guid combatId, 
    List<InitiativeUpdateDto> updates, 
    Guid userId)
{
    var combat = await _context.Combats
        .Include(c => c.Session)
            .ThenInclude(s => s.Campaign)
        .Include(c => c.Participants)
            .ThenInclude(p => p.Character)
        .FirstOrDefaultAsync(c => c.Id == combatId);
    
    if (combat == null)
        throw new NotFoundException("Combat non trouvé");
    
    var isGameMaster = combat.Session.Campaign.CreatedBy == userId;
    
    foreach (var update in updates)
    {
        var participant = combat.Participants.FirstOrDefault(p => p.Id == update.ParticipantId);
        if (participant == null) continue;
        
        // Joueur peut uniquement modifier son propre personnage
        if (!isGameMaster && participant.Character?.OwnerId != userId)
            continue;
        
        participant.InitiativeScore = update.InitiativeScore;
    }
    
    await _context.SaveChangesAsync();
    
    // Retourner liste ordonnée
    var orderedParticipants = combat.Participants
        .OrderByDescending(p => p.InitiativeScore)
        .ThenBy(p => p.DisplayName)
        .Select(p => p.ToDto())
        .ToList();
    
    // Notifier via SignalR
    await _combatHub.Clients.Group($"Combat-{combatId}")
        .SendAsync("InitiativeUpdated", orderedParticipants);
    
    return orderedParticipants;
}
```
- [ ] Créer `CombatService.SortByInitiativeAsync(combatId, userId)` :
```csharp
public async Task<List<CombatParticipantDto>> SortByInitiativeAsync(Guid combatId, Guid userId)
{
    var combat = await _context.Combats
        .Include(c => c.Session.Campaign)
        .Include(c => c.Participants)
        .FirstOrDefaultAsync(c => c.Id == combatId);
    
    if (combat?.Session.Campaign.CreatedBy != userId)
        throw new UnauthorizedException();
    
    var orderedParticipants = combat.Participants
        .OrderByDescending(p => p.InitiativeScore)
        .ThenBy(p => p.DisplayName)
        .ToList();
    
    // Notifier via SignalR
    await _combatHub.Clients.Group($"Combat-{combatId}")
        .SendAsync("InitiativeUpdated", orderedParticipants.Select(p => p.ToDto()).ToList());
    
    return orderedParticipants.Select(p => p.ToDto()).ToList();
}
```
- [ ] Créer endpoints :
  - [ ] `PATCH /api/combats/{combatId}/initiative` [Authorize]
  - [ ] `POST /api/combats/{combatId}/initiative/sort` [Authorize(Roles = "GameMaster")]

### Frontend
- [ ] Créer composant `InitiativeTracker.razor` :
```razor
@inject HttpClient Http
@inject SignalRCombatService SignalR

<div class="initiative-tracker">
    <div class="tracker-header">
        <h3>Ordre d'Initiative</h3>
        @if (IsGameMaster)
        {
            <button @onclick="SortByInitiative" class="btn-secondary">
                Trier par Initiative
            </button>
        }
    </div>
    
    <div class="participants-list">
        @foreach (var (participant, index) in OrderedParticipants.Select((p, i) => (p, i)))
        {
            <div class="participant-item @GetActiveClass(index)">
                <img src="@GetAvatar(participant)" 
                     alt="@participant.DisplayName" 
                     class="participant-avatar" />
                
                <div class="participant-info">
                    <h4>@participant.DisplayName</h4>
                    <div class="hp-bar">
                        <div class="hp-fill" style="width: @GetHPPercentage(participant)%"></div>
                        <span>@participant.CurrentHP / @participant.MaxHP HP</span>
                    </div>
                </div>
                
                <div class="initiative-input">
                    <label>Initiative</label>
                    <input type="number" 
                           @bind="participant.InitiativeScore" 
                           @bind:after="() => UpdateInitiative(participant)"
                           disabled="@(!CanEditInitiative(participant))"
                           class="form-control" />
                </div>
            </div>
        }
    </div>
</div>

@code {
    [Parameter] public Guid CombatId { get; set; }
    [Parameter] public List<CombatParticipantDto> Participants { get; set; } = new();
    [Parameter] public bool IsGameMaster { get; set; }
    [Parameter] public Guid CurrentUserId { get; set; }
    [Parameter] public int CurrentTurnIndex { get; set; }

    private List<CombatParticipantDto> OrderedParticipants { get; set; } = new();

    protected override void OnParametersSet()
    {
        OrderedParticipants = Participants
            .OrderByDescending(p => p.InitiativeScore)
            .ThenBy(p => p.DisplayName)
            .ToList();
    }

    protected override async Task OnInitializedAsync()
    {
        SignalR.OnInitiativeUpdated += (participants) =>
        {
            OrderedParticipants = participants;
            StateHasChanged();
        };
    }

    private async Task UpdateInitiative(CombatParticipantDto participant)
    {
        var updates = new List<InitiativeUpdateDto>
        {
            new() { ParticipantId = participant.Id, InitiativeScore = participant.InitiativeScore }
        };
        
        await Http.PatchAsJsonAsync($"/api/combats/{CombatId}/initiative", new { participants = updates });
    }

    private async Task SortByInitiative()
    {
        var response = await Http.PostAsync($"/api/combats/{CombatId}/initiative/sort", null);
        
        if (response.IsSuccessStatusCode)
        {
            Toast.Success("Ordre d'initiative mis à jour");
        }
    }

    private bool CanEditInitiative(CombatParticipantDto participant)
    {
        if (IsGameMaster) return true;
        return participant.CharacterId.HasValue && participant.Character?.OwnerId == CurrentUserId;
    }

    private string GetActiveClass(int index)
    {
        return index == CurrentTurnIndex ? "active" : "";
    }

    private int GetHPPercentage(CombatParticipantDto p)
    {
        if (p.MaxHP == 0) return 0;
        return (int)((double)p.CurrentHP / p.MaxHP * 100);
    }

    private string GetAvatar(CombatParticipantDto p)
    {
        return p.Character?.AvatarUrl ?? p.NPC?.AvatarUrl ?? "/images/default-avatar.png";
    }
}
```
- [ ] Mettre à jour `SignalRCombatService` :
```csharp
public class SignalRCombatService
{
    private HubConnection? _connection;
    
    public event Action<List<CombatParticipantDto>>? OnInitiativeUpdated;

    public async Task ConnectAsync(Guid combatId)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{_config["ApiUrl"]}/hubs/combat")
            .Build();

        _connection.On<List<CombatParticipantDto>>("InitiativeUpdated", participants =>
        {
            OnInitiativeUpdated?.Invoke(participants);
        });

        await _connection.StartAsync();
        await _connection.InvokeAsync("JoinCombat", combatId);
    }

    public async Task DisconnectAsync(Guid combatId)
    {
        if (_connection != null)
        {
            await _connection.InvokeAsync("LeaveCombat", combatId);
            await _connection.StopAsync();
        }
    }
}
```

### Base de Données
- [ ] Aucune modification nécessaire (InitiativeScore déjà dans CombatParticipant)

---

## 🔗 Dépendances

### Dépend de
- [US-032](./US-032-declenchement-combat.md) - Combat démarré

### Bloque
- [US-034](./US-034-gestion-tours.md) - Ordre des tours dépend de l'initiative

---

## 📊 Estimation

**Story Points** : 5

**Détails** :
- Complexité : Moyenne (SignalR, permissions)
- Effort : 1-2 jours
- Risques : Synchronisation temps réel

---

## 📝 Notes Techniques

### Initiative Automatique (Future)
Pour D&D : `InitiativeScore = d20 + ModificateurDexterite`
```csharp
public int RollInitiative(int dexModifier)
{
    return Random.Shared.Next(1, 21) + dexModifier;
}
```

### Tri en Cas d'Égalité
Si 2 participants ont même initiative → Tri alphabétique par nom

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Initiative Tracker affiché
- [ ] MJ peut modifier toutes initiatives
- [ ] Joueurs peuvent modifier leur propre initiative
- [ ] Tri automatique fonctionnel
- [ ] SignalR synchronisation temps réel
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 9

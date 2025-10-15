# US-020 - Auto-Sauvegarde de Session

## 📝 Description

**En tant que** Maître du Jeu ou Joueur  
**Je veux** que l'état de la session soit sauvegardé automatiquement  
**Afin de** ne pas perdre de données en cas de déconnexion ou crash

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Sauvegarde automatique toutes les **2 minutes** pendant session active
- [ ] Sauvegarde immédiate sur événements critiques :
  - [ ] Changement de chapitre
  - [ ] Fin de combat
  - [ ] Modification HP joueur
  - [ ] Ajout objet inventaire
- [ ] Indicateur visuel "Sauvegarde..." (spinner + texte)
- [ ] Indicateur "Sauvegardé ✓" (succès, disparaît après 2s)
- [ ] En cas d'échec : Retry automatique (3 tentatives)
- [ ] Si échec après retries → Notification MJ + log erreur
- [ ] Restauration automatique si MJ/joueur perd connexion et revient

### Techniques
- [ ] Endpoint : `POST /api/sessions/{sessionId}/save`
- [ ] Body : `{ "sessionState": { ... } }` (JSON flexible)
- [ ] Response 200 : `{ "savedAt": "2025-01-22T15:30:00Z" }`
- [ ] Structure `sessionState` :
```json
{
  "currentChapterId": "guid",
  "players": [
    {
      "userId": "guid",
      "characterId": "guid",
      "currentHP": 45,
      "maxHP": 60,
      "temporaryHP": 0,
      "conditions": ["poisoned"],
      "position": { "x": 10, "y": 5 }
    }
  ],
  "combat": {
    "isActive": true,
    "round": 3,
    "turnOrder": ["guid1", "guid2", ...],
    "currentTurnIndex": 1
  },
  "customData": { ... }
}
```
- [ ] Timer côté client : `setInterval(() => autoSave(), 120000)` (2 min)

---

## 🧪 Tests

### Tests Unitaires
- [ ] `SessionService.SaveSessionState_ValidState_UpdatesDatabase()`
- [ ] `SessionService.SaveSessionState_InvalidSessionId_ThrowsNotFoundException()`
- [ ] `SessionService.RestoreSessionState_ReturnsLatestState()`
- [ ] `AutoSaveService.AutoSave_RetriesOnFailure()`

### Tests d'Intégration
- [ ] `SessionEndpoint_SaveState_UpdatesSessionEntity()`
- [ ] `SessionEndpoint_SaveState_CreatesBackup()`

### Tests E2E
- [ ] Session active → Attendre 2 min → Vérifier "Sauvegardé ✓" apparaît
- [ ] MJ change chapitre → Sauvegarde immédiate déclenchée
- [ ] Joueur perd connexion → Reconnexion → État restauré

---

## 🔧 Tâches Techniques

### Backend
- [ ] Modifier entité `Session` :
```csharp
public class Session
{
    // ... Propriétés existantes
    public string SessionState { get; set; } // JSON
    public DateTime? LastSavedAt { get; set; }
}
```
- [ ] Créer `SessionService.SaveSessionStateAsync(sessionId, sessionState)` :
  - [ ] Valider sessionId existe
  - [ ] Mettre à jour Session.SessionState (JSON)
  - [ ] Mettre à jour Session.LastSavedAt
  - [ ] Optionnel : Créer backup dans table `SessionBackups` (historique)
- [ ] Créer `SessionService.RestoreSessionStateAsync(sessionId)` :
  - [ ] Récupérer Session.SessionState
  - [ ] Désérialiser JSON → DTO
- [ ] Créer endpoint `POST /api/sessions/{id}/save` [Authorize]
- [ ] Créer endpoint `GET /api/sessions/{id}/restore` [Authorize]

### Frontend
- [ ] Créer service `AutoSaveService.cs` :
```csharp
public class AutoSaveService : IDisposable
{
    private Timer? _timer;
    private readonly HttpClient _http;
    private readonly ILogger<AutoSaveService> _logger;

    public event Action<SaveStatus>? OnSaveStatusChanged;

    public void StartAutoSave(Guid sessionId, SessionStateDto initialState)
    {
        _timer = new Timer(async _ =>
        {
            await SaveAsync(sessionId);
        }, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
    }

    public async Task SaveAsync(Guid sessionId)
    {
        OnSaveStatusChanged?.Invoke(SaveStatus.Saving);

        var retries = 0;
        while (retries < 3)
        {
            try
            {
                var state = GetCurrentSessionState(); // Récupère état depuis Blazor StateContainer
                await _http.PostAsJsonAsync($"/api/sessions/{sessionId}/save", new { sessionState = state });
                
                OnSaveStatusChanged?.Invoke(SaveStatus.Saved);
                _logger.LogInformation("Session {SessionId} saved successfully", sessionId);
                break;
            }
            catch (Exception ex)
            {
                retries++;
                _logger.LogWarning(ex, "Auto-save attempt {Retry} failed", retries);
                
                if (retries == 3)
                {
                    OnSaveStatusChanged?.Invoke(SaveStatus.Failed);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(5)); // Wait avant retry
                }
            }
        }
    }

    public void Dispose() => _timer?.Dispose();
}

public enum SaveStatus
{
    Idle,
    Saving,
    Saved,
    Failed
}
```
- [ ] Créer `SessionStateContainer.cs` (Scoped) :
```csharp
public class SessionStateContainer
{
    public Guid? CurrentChapterId { get; set; }
    public List<PlayerState> Players { get; set; } = new();
    public CombatState? Combat { get; set; }

    public SessionStateDto ToDto() => new()
    {
        CurrentChapterId = CurrentChapterId,
        Players = Players.Select(p => new PlayerStateDto
        {
            UserId = p.UserId,
            CharacterId = p.CharacterId,
            CurrentHP = p.CurrentHP,
            MaxHP = p.MaxHP
        }).ToList(),
        Combat = Combat // ...
    };
}
```
- [ ] Créer composant `AutoSaveIndicator.razor` :
```razor
@inject AutoSaveService AutoSave

<div class="autosave-indicator">
    @switch (SaveStatus)
    {
        case SaveStatus.Saving:
            <span class="saving">
                <i class="spinner"></i> Sauvegarde...
            </span>
            break;
        case SaveStatus.Saved:
            <span class="saved">
                ✓ Sauvegardé
            </span>
            break;
        case SaveStatus.Failed:
            <span class="failed">
                ❌ Échec sauvegarde
            </span>
            break;
    }
</div>

@code {
    private SaveStatus SaveStatus { get; set; } = SaveStatus.Idle;

    protected override void OnInitialized()
    {
        AutoSave.OnSaveStatusChanged += status =>
        {
            SaveStatus = status;
            StateHasChanged();

            if (status == SaveStatus.Saved)
            {
                // Cacher après 2 secondes
                Task.Delay(2000).ContinueWith(_ =>
                {
                    SaveStatus = SaveStatus.Idle;
                    StateHasChanged();
                });
            }
        };
    }
}
```
- [ ] Intégrer dans `SessionLive.razor` :
  - [ ] Injecter `AutoSaveService`
  - [ ] Démarrer auto-save au OnInitializedAsync
  - [ ] Trigger manuel sur événements critiques
  - [ ] Arrêter auto-save au Dispose

### Base de Données
- [ ] Migration : Ajouter colonnes `SessionState`, `LastSavedAt` à `Sessions`
- [ ] Optionnel : Créer table `SessionBackups` (historique) :
```csharp
public class SessionBackup
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string SessionState { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 🔗 Dépendances

### Dépend de
- [US-018](./US-018-lancement-session.md) - Session entity
- [US-019](./US-019-progression-chapitres.md) - CurrentChapterId

### Bloque
- Aucune

---

## 📊 Estimation

**Story Points** : 3

**Détails** :
- Complexité : Faible-Moyenne (Timer, JSON sérialization)
- Effort : 1 jour
- Risques : Gestion erreurs réseau

---

## 📝 Notes Techniques

### Événements Critiques Déclencheurs
```csharp
// Dans SessionHub ou services
public async Task OnChapterChanged(Guid sessionId)
{
    await _autoSaveService.SaveAsync(sessionId);
}

public async Task OnCombatEnded(Guid sessionId)
{
    await _autoSaveService.SaveAsync(sessionId);
}

public async Task OnPlayerHPChanged(Guid sessionId)
{
    await _autoSaveService.SaveAsync(sessionId);
}
```

### Retry avec Exponential Backoff
```csharp
private async Task<bool> SaveWithRetryAsync(Guid sessionId, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            await SaveAsync(sessionId);
            return true;
        }
        catch
        {
            if (i == maxRetries - 1) return false;
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // 1s, 2s, 4s
        }
    }
    return false;
}
```

### Restauration au Reconnexion
```csharp
protected override async Task OnInitializedAsync()
{
    var sessionId = await GetSessionIdFromRoute();
    var restoredState = await _sessionService.RestoreSessionStateAsync(sessionId);
    
    if (restoredState != null)
    {
        // Hydrater StateContainer
        _stateContainer.CurrentChapterId = restoredState.CurrentChapterId;
        _stateContainer.Players = restoredState.Players;
        // ...
    }
}
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Auto-save toutes les 2 minutes opérationnel
- [ ] Sauvegarde événements critiques fonctionne
- [ ] Retry automatique implémenté
- [ ] Indicateur visuel fonctionnel
- [ ] Restauration au reconnexion fonctionne
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 4

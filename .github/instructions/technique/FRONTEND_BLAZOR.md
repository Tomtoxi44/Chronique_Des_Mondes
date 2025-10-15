# Frontend Blazor - Chronique des Mondes

## Vue d'ensemble

Le frontend est construit avec **Blazor Server** pour bénéficier du rendu côté serveur et de la communication temps réel via SignalR. L'architecture suit le pattern **Component-Handler-Service** pour une séparation claire des responsabilités.

### Principes de conception

1. **Blazor Server** : Rendu côté serveur avec SignalR pour l'interactivité
2. **Component-Handler-Service** : Séparation UI / Logique / API
3. **@rendermode InteractiveServer** : Composants interactifs
4. **CascadingValue** : Partage d'état entre composants
5. **Dependency Injection** : Services injectés dans les composants
6. **SignalR Integration** : Communication temps réel pour sessions/combats

---

## 1. Structure des fichiers

### 1.1 Organisation des composants

```
Cdm.Web/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   ├── MainLayout.razor.css
│   │   ├── NavMenu.razor
│   │   └── NavMenu.razor.css
│   ├── Pages/
│   │   ├── Home.razor
│   │   ├── Characters/
│   │   │   ├── CharacterList.razor
│   │   │   ├── CharacterList.razor.cs (code-behind)
│   │   │   ├── CharacterList.handler.cs (logique métier)
│   │   │   ├── CharacterDetails.razor
│   │   │   ├── CharacterDetails.razor.cs
│   │   │   ├── CharacterDetails.handler.cs
│   │   │   ├── CreateCharacter.razor
│   │   │   └── EditCharacter.razor
│   │   ├── Campaigns/
│   │   │   ├── CampaignList.razor
│   │   │   ├── CampaignDetails.razor
│   │   │   └── CreateCampaign.razor
│   │   ├── Sessions/
│   │   │   ├── SessionView.razor
│   │   │   ├── SessionView.razor.cs
│   │   │   ├── SessionView.handler.cs
│   │   │   └── Combat/
│   │   │       ├── CombatTracker.razor
│   │   │       ├── CombatTracker.razor.cs
│   │   │       └── CombatTracker.handler.cs
│   │   └── Auth/
│   │       ├── Login.razor
│   │       ├── Register.razor
│   │       └── Logout.razor
│   └── Shared/
│       ├── DiceRoller.razor
│       ├── CharacterCard.razor
│       ├── HealthBar.razor
│       ├── LoadingSpinner.razor
│       └── ConfirmDialog.razor
├── Services/
│   ├── ApiClients/
│   │   ├── CharacterApiClient.cs
│   │   ├── CampaignApiClient.cs
│   │   ├── SessionApiClient.cs
│   │   └── CombatApiClient.cs
│   ├── SignalR/
│   │   ├── SessionHubService.cs
│   │   ├── CombatHubService.cs
│   │   └── NotificationHubService.cs
│   └── State/
│       ├── AuthStateProvider.cs
│       ├── GameStateService.cs
│       └── NotificationStateService.cs
└── wwwroot/
    ├── css/
    │   ├── app.css
    │   └── bootstrap/
    ├── js/
    │   └── app.js
    └── images/
```

---

### 1.2 Pattern Component-Handler-Service

**Séparation des responsabilités :**

1. **`.razor`** : Markup et présentation uniquement
2. **`.razor.cs`** : Code-behind, gestion des événements UI, état local
3. **`.handler.cs`** : Logique métier, orchestration des services
4. **Services** : Appels API, SignalR, cache

```
┌────────────────────────────────────────────────────────┐
│              CharacterList.razor                       │
│  → Affichage de la liste (HTML/CSS)                    │
└───────────────────────┬────────────────────────────────┘
                        │
                        ▼
┌────────────────────────────────────────────────────────┐
│           CharacterList.razor.cs                       │
│  → Gestion des événements (clicks, inputs)             │
│  → État local (selectedCharacter, isLoading)           │
└───────────────────────┬────────────────────────────────┘
                        │
                        ▼
┌────────────────────────────────────────────────────────┐
│         CharacterList.handler.cs                       │
│  → Logique métier (filtres, tri, validation)           │
│  → Orchestration des services                          │
└───────────────────────┬────────────────────────────────┘
                        │
                        ▼
┌────────────────────────────────────────────────────────┐
│         CharacterApiClient.cs                          │
│  → Appels HTTP vers l'API                              │
│  → Gestion des erreurs, retry                          │
└────────────────────────────────────────────────────────┘
```

---

## 2. Exemple complet : Liste de personnages

### 2.1 CharacterList.razor

```razor
@page "/characters"
@using Cdm.Web.Components.Pages.Characters
@rendermode InteractiveServer

<PageTitle>Mes Personnages</PageTitle>

<div class="character-list-container">
    <div class="header">
        <h1>Mes Personnages</h1>
        <button class="btn btn-primary" @onclick="NavigateToCreate">
            <i class="bi bi-plus-circle"></i> Nouveau personnage
        </button>
    </div>

    @if (IsLoading)
    {
        <LoadingSpinner />
    }
    else if (ErrorMessage != null)
    {
        <div class="alert alert-danger">
            <i class="bi bi-exclamation-triangle"></i>
            @ErrorMessage
        </div>
    }
    else if (!Characters.Any())
    {
        <div class="empty-state">
            <i class="bi bi-person-x"></i>
            <p>Vous n'avez pas encore de personnages.</p>
            <button class="btn btn-primary" @onclick="NavigateToCreate">
                Créer votre premier personnage
            </button>
        </div>
    }
    else
    {
        <div class="filters">
            <select class="form-select" @bind="SelectedGameType" @bind:after="OnFilterChanged">
                <option value="">Tous les systèmes</option>
                <option value="0">Générique</option>
                <option value="1">D&D 5e</option>
                <option value="2">Skyrim</option>
            </select>
        </div>

        <div class="character-grid">
            @foreach (var character in FilteredCharacters)
            {
                <CharacterCard 
                    Character="@character"
                    OnSelect="@(() => OnCharacterSelected(character))"
                    OnDelete="@(() => OnDeleteRequested(character))" />
            }
        </div>
    }
</div>

@if (ShowDeleteConfirm)
{
    <ConfirmDialog
        Title="Supprimer le personnage"
        Message="@($"Êtes-vous sûr de vouloir supprimer {CharacterToDelete?.Name} ?")"
        OnConfirm="ConfirmDelete"
        OnCancel="CancelDelete" />
}
```

---

### 2.2 CharacterList.razor.cs

```csharp
using Microsoft.AspNetCore.Components;
using Cdm.Web.Models;

namespace Cdm.Web.Components.Pages.Characters;

public partial class CharacterList
{
    [Inject]
    private CharacterListHandler Handler { get; set; } = default!;
    
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;
    
    // État local
    private bool IsLoading { get; set; } = true;
    private string? ErrorMessage { get; set; }
    private List<CharacterDto> Characters { get; set; } = new();
    private List<CharacterDto> FilteredCharacters { get; set; } = new();
    private string SelectedGameType { get; set; } = "";
    
    // État pour la suppression
    private bool ShowDeleteConfirm { get; set; }
    private CharacterDto? CharacterToDelete { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await LoadCharacters();
    }
    
    private async Task LoadCharacters()
    {
        IsLoading = true;
        ErrorMessage = null;
        
        try
        {
            Characters = await Handler.GetCharactersAsync();
            FilteredCharacters = Characters;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors du chargement des personnages : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private void OnFilterChanged()
    {
        FilteredCharacters = Handler.FilterCharacters(Characters, SelectedGameType);
    }
    
    private void OnCharacterSelected(CharacterDto character)
    {
        Navigation.NavigateTo($"/characters/{character.Id}");
    }
    
    private void OnDeleteRequested(CharacterDto character)
    {
        CharacterToDelete = character;
        ShowDeleteConfirm = true;
    }
    
    private async Task ConfirmDelete()
    {
        if (CharacterToDelete == null) return;
        
        IsLoading = true;
        ShowDeleteConfirm = false;
        
        try
        {
            await Handler.DeleteCharacterAsync(CharacterToDelete.Id);
            await LoadCharacters();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors de la suppression : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            CharacterToDelete = null;
        }
    }
    
    private void CancelDelete()
    {
        ShowDeleteConfirm = false;
        CharacterToDelete = null;
    }
    
    private void NavigateToCreate()
    {
        Navigation.NavigateTo("/characters/create");
    }
}
```

---

### 2.3 CharacterList.handler.cs

```csharp
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Models;

namespace Cdm.Web.Components.Pages.Characters;

public class CharacterListHandler
{
    private readonly CharacterApiClient _characterClient;
    private readonly ILogger<CharacterListHandler> _logger;
    
    public CharacterListHandler(
        CharacterApiClient characterClient,
        ILogger<CharacterListHandler> logger)
    {
        _characterClient = characterClient;
        _logger = logger;
    }
    
    public async Task<List<CharacterDto>> GetCharactersAsync()
    {
        _logger.LogInformation("Fetching user characters");
        
        var result = await _characterClient.GetAllAsync();
        
        if (!result.IsSuccess)
        {
            _logger.LogError("Failed to fetch characters: {Error}", result.Error);
            throw new Exception(result.Error);
        }
        
        return result.Data ?? new List<CharacterDto>();
    }
    
    public List<CharacterDto> FilterCharacters(List<CharacterDto> characters, string gameTypeFilter)
    {
        if (string.IsNullOrEmpty(gameTypeFilter))
            return characters;
        
        if (!int.TryParse(gameTypeFilter, out var gameType))
            return characters;
        
        _logger.LogInformation("Filtering characters by GameType: {GameType}", gameType);
        
        return characters.Where(c => (int)c.GameType == gameType).ToList();
    }
    
    public async Task DeleteCharacterAsync(int characterId)
    {
        _logger.LogInformation("Deleting character {CharacterId}", characterId);
        
        var result = await _characterClient.DeleteAsync(characterId);
        
        if (!result.IsSuccess)
        {
            _logger.LogError("Failed to delete character {CharacterId}: {Error}", characterId, result.Error);
            throw new Exception(result.Error);
        }
    }
}
```

---

### 2.4 CharacterApiClient.cs

```csharp
using System.Net.Http.Json;
using Cdm.Web.Models;

namespace Cdm.Web.Services.ApiClients;

public class CharacterApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CharacterApiClient> _logger;
    
    public CharacterApiClient(HttpClient httpClient, ILogger<CharacterApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<Result<List<CharacterDto>>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/characters");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result<List<CharacterDto>>.Failure($"API Error: {error}");
            }
            
            var data = await response.Content.ReadFromJsonAsync<CharacterListResponse>();
            return Result<List<CharacterDto>>.Success(data?.Characters ?? new List<CharacterDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch characters from API");
            return Result<List<CharacterDto>>.Failure("Erreur de connexion à l'API");
        }
    }
    
    public async Task<Result<CharacterDto>> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/characters/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result<CharacterDto>.Failure($"API Error: {error}");
            }
            
            var character = await response.Content.ReadFromJsonAsync<CharacterDto>();
            return Result<CharacterDto>.Success(character!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch character {CharacterId} from API", id);
            return Result<CharacterDto>.Failure("Erreur de connexion à l'API");
        }
    }
    
    public async Task<Result<CharacterDto>> CreateAsync(CreateCharacterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/characters", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result<CharacterDto>.Failure($"API Error: {error}");
            }
            
            var character = await response.Content.ReadFromJsonAsync<CharacterDto>();
            return Result<CharacterDto>.Success(character!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create character via API");
            return Result<CharacterDto>.Failure("Erreur lors de la création du personnage");
        }
    }
    
    public async Task<Result<bool>> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/characters/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result<bool>.Failure($"API Error: {error}");
            }
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete character {CharacterId} via API", id);
            return Result<bool>.Failure("Erreur lors de la suppression du personnage");
        }
    }
}
```

---

## 3. Composants partagés

### 3.1 CharacterCard.razor

```razor
@using Cdm.Web.Models

<div class="character-card" @onclick="OnSelect">
    <div class="character-image">
        @if (!string.IsNullOrEmpty(Character.ImageUrl))
        {
            <img src="@Character.ImageUrl" alt="@Character.Name" />
        }
        else
        {
            <div class="no-image">
                <i class="bi bi-person-circle"></i>
            </div>
        }
    </div>
    
    <div class="character-info">
        <h3 class="character-name">@Character.Name</h3>
        <div class="character-meta">
            <span class="game-type">@GetGameTypeName(Character.GameType)</span>
            <span class="level">Niveau @Character.Level</span>
        </div>
        
        <HealthBar 
            Current="@Character.CurrentHealth" 
            Max="@Character.MaxHealth" />
    </div>
    
    <div class="character-actions">
        <button class="btn btn-sm btn-danger" @onclick="OnDeleteClick" @onclick:stopPropagation="true">
            <i class="bi bi-trash"></i>
        </button>
    </div>
</div>

@code {
    [Parameter]
    public CharacterDto Character { get; set; } = default!;
    
    [Parameter]
    public EventCallback OnSelect { get; set; }
    
    [Parameter]
    public EventCallback OnDelete { get; set; }
    
    private async Task OnDeleteClick()
    {
        await OnDelete.InvokeAsync();
    }
    
    private string GetGameTypeName(GameType gameType) => gameType switch
    {
        GameType.Generic => "Générique",
        GameType.Dnd5e => "D&D 5e",
        GameType.Skyrim => "Skyrim",
        _ => "Inconnu"
    };
}
```

---

### 3.2 HealthBar.razor

```razor
<div class="health-bar-container">
    <div class="health-bar">
        <div class="health-bar-fill" style="width: @HealthPercentage%"></div>
    </div>
    <div class="health-text">
        <span>@Current / @Max PV</span>
    </div>
</div>

@code {
    [Parameter]
    public int Current { get; set; }
    
    [Parameter]
    public int Max { get; set; }
    
    private double HealthPercentage => Max > 0 ? (double)Current / Max * 100 : 0;
}
```

**CSS associé :**
```css
.health-bar-container {
    margin-top: 8px;
}

.health-bar {
    height: 20px;
    background-color: #ddd;
    border-radius: 10px;
    overflow: hidden;
}

.health-bar-fill {
    height: 100%;
    background: linear-gradient(90deg, #ff4444 0%, #44ff44 100%);
    transition: width 0.3s ease;
}

.health-text {
    text-align: center;
    font-size: 12px;
    color: #666;
    margin-top: 4px;
}
```

---

### 3.3 DiceRoller.razor

```razor
@inject SessionHubService SessionHub

<div class="dice-roller">
    <div class="dice-input">
        <input 
            type="text" 
            class="form-control" 
            @bind="DiceNotation" 
            placeholder="1d20+5" 
            @onkeydown="HandleKeyDown" />
        
        <input 
            type="text" 
            class="form-control" 
            @bind="Context" 
            placeholder="Contexte (optionnel)" />
        
        <button class="btn btn-primary" @onclick="RollDice" disabled="@IsRolling">
            @if (IsRolling)
            {
                <span class="spinner-border spinner-border-sm"></span>
            }
            else
            {
                <i class="bi bi-dice-6"></i>
            }
            Lancer
        </button>
    </div>
    
    @if (LastResult != null)
    {
        <div class="dice-result">
            <div class="result-details">
                Résultats: [@string.Join(", ", LastResult.Results)]
                @if (LastResult.Modifiers != 0)
                {
                    <span> + @LastResult.Modifiers</span>
                }
            </div>
            <div class="result-total">
                Total: <strong>@LastResult.TotalResult</strong>
            </div>
        </div>
    }
</div>

@code {
    [Parameter]
    public int SessionId { get; set; }
    
    [Parameter]
    public int CharacterId { get; set; }
    
    private string DiceNotation { get; set; } = "1d20";
    private string Context { get; set; } = "";
    private bool IsRolling { get; set; }
    private DiceRollResult? LastResult { get; set; }
    
    protected override void OnInitialized()
    {
        SessionHub.OnDiceRolled += HandleDiceRolled;
    }
    
    private async Task RollDice()
    {
        if (string.IsNullOrWhiteSpace(DiceNotation))
            return;
        
        IsRolling = true;
        
        try
        {
            await SessionHub.RollDiceAsync(SessionId, CharacterId, DiceNotation, Context);
            Context = ""; // Reset context après le lancer
        }
        finally
        {
            IsRolling = false;
        }
    }
    
    private Task HandleDiceRolled(DiceRolledEventArgs args)
    {
        if (args.CharacterId == CharacterId)
        {
            LastResult = new DiceRollResult
            {
                Results = args.Results,
                Modifiers = args.Modifiers,
                TotalResult = args.TotalResult
            };
            
            StateHasChanged();
        }
        
        return Task.CompletedTask;
    }
    
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await RollDice();
        }
    }
    
    public void Dispose()
    {
        SessionHub.OnDiceRolled -= HandleDiceRolled;
    }
}
```

---

## 4. Gestion de l'authentification

### 4.1 AuthStateProvider.cs

```csharp
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Cdm.Web.Services.State;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly IJwtService _jwtService;
    
    public CustomAuthStateProvider(
        ILocalStorageService localStorage,
        IJwtService jwtService)
    {
        _localStorage = localStorage;
        _jwtService = jwtService;
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        
        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        
        try
        {
            var principal = _jwtService.ValidateToken(token);
            return new AuthenticationState(principal);
        }
        catch
        {
            // Token invalide ou expiré
            await _localStorage.RemoveItemAsync("authToken");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
    
    public async Task MarkUserAsAuthenticated(string token)
    {
        await _localStorage.SetItemAsync("authToken", token);
        
        var principal = _jwtService.ValidateToken(token);
        var authState = new AuthenticationState(principal);
        
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }
    
    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync("authToken");
        
        var anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        NotifyAuthenticationStateChanged(Task.FromResult(anonymous));
    }
}
```

---

### 4.2 Configuration dans Program.cs

```csharp
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Ajouter les services Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Ajouter l'authentification
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>();

// Ajouter les API clients
builder.Services.AddScoped<CharacterApiClient>();
builder.Services.AddScoped<CampaignApiClient>();
builder.Services.AddScoped<SessionApiClient>();

// Ajouter les handlers
builder.Services.AddScoped<CharacterListHandler>();
builder.Services.AddScoped<CharacterDetailsHandler>();

// Ajouter les services SignalR
builder.Services.AddScoped<SessionHubService>();
builder.Services.AddScoped<CombatHubService>();
builder.Services.AddScoped<NotificationHubService>();

// Configurer HttpClient pour l'API
builder.Services.AddHttpClient<CharacterApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7000"); // URL de l'API
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

---

### 4.3 AuthorizationMessageHandler.cs

Ajoute automatiquement le JWT token aux requêtes HTTP.

```csharp
using System.Net.Http.Headers;

namespace Cdm.Web.Services;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    
    public AuthorizationMessageHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}
```

---

## 5. CascadingValue pour le partage d'état

### 5.1 GameStateService.cs

```csharp
namespace Cdm.Web.Services.State;

public class GameStateService
{
    public int? CurrentSessionId { get; private set; }
    public int? CurrentCombatId { get; private set; }
    public int? SelectedCharacterId { get; private set; }
    
    public event Action? OnStateChanged;
    
    public void SetCurrentSession(int sessionId)
    {
        CurrentSessionId = sessionId;
        NotifyStateChanged();
    }
    
    public void SetCurrentCombat(int combatId)
    {
        CurrentCombatId = combatId;
        NotifyStateChanged();
    }
    
    public void SetSelectedCharacter(int characterId)
    {
        SelectedCharacterId = characterId;
        NotifyStateChanged();
    }
    
    public void ClearSession()
    {
        CurrentSessionId = null;
        CurrentCombatId = null;
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
```

---

### 5.2 Utilisation avec CascadingValue

**Dans MainLayout.razor :**
```razor
@inject GameStateService GameState

<CascadingValue Value="@GameState">
    @Body
</CascadingValue>
```

**Dans un composant enfant :**
```razor
@code {
    [CascadingParameter]
    public GameStateService GameState { get; set; } = default!;
    
    protected override void OnInitialized()
    {
        GameState.OnStateChanged += StateHasChanged;
    }
    
    public void Dispose()
    {
        GameState.OnStateChanged -= StateHasChanged;
    }
}
```

---

## 6. Intégration SignalR dans les composants

### 6.1 SessionView.razor

```razor
@page "/sessions/{SessionId:int}"
@inject SessionHubService SessionHub
@inject AuthService AuthService
@implements IAsyncDisposable

<div class="session-container">
    <h2>@sessionName</h2>
    
    <div class="session-content">
        <!-- Chat -->
        <div class="chat-panel">
            <div class="chat-messages">
                @foreach (var message in messages)
                {
                    <div class="message">
                        <strong>@message.Username:</strong> @message.Message
                        <span class="timestamp">@message.Timestamp.ToLocalTime().ToString("HH:mm")</span>
                    </div>
                }
            </div>
            
            <div class="chat-input">
                <input 
                    @bind="newMessage" 
                    @onkeydown="HandleChatKeyDown" 
                    placeholder="Votre message..." />
                <button @onclick="SendMessage">Envoyer</button>
            </div>
        </div>
        
        <!-- Dice Roller -->
        <div class="dice-panel">
            <DiceRoller 
                SessionId="@SessionId" 
                CharacterId="@currentCharacterId" />
        </div>
        
        <!-- Participants -->
        <div class="participants-panel">
            <h3>Participants</h3>
            @foreach (var participant in participants)
            {
                <div class="participant">
                    <i class="bi bi-circle-fill @(participant.IsOnline ? "online" : "offline")"></i>
                    @participant.Username
                </div>
            }
        </div>
    </div>
</div>

@code {
    [Parameter]
    public int SessionId { get; set; }
    
    private string sessionName = "";
    private string newMessage = "";
    private int currentCharacterId;
    private List<MessageDto> messages = new();
    private List<ParticipantDto> participants = new();
    
    protected override async Task OnInitializedAsync()
    {
        // Initialiser SignalR
        var token = await AuthService.GetAccessTokenAsync();
        await SessionHub.InitializeAsync(token);
        
        // S'abonner aux événements
        SessionHub.OnMessageReceived += HandleMessageReceived;
        SessionHub.OnUserJoined += HandleUserJoined;
        SessionHub.OnUserLeft += HandleUserLeft;
        
        // Rejoindre la session
        await SessionHub.JoinSessionAsync(SessionId);
    }
    
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(newMessage))
            return;
        
        await SessionHub.SendMessageAsync(SessionId, newMessage);
        newMessage = "";
    }
    
    private Task HandleMessageReceived(MessageReceivedEventArgs args)
    {
        messages.Add(new MessageDto
        {
            Username = args.Username,
            Message = args.Message,
            Timestamp = args.Timestamp
        });
        
        StateHasChanged();
        return Task.CompletedTask;
    }
    
    private Task HandleUserJoined(UserJoinedEventArgs args)
    {
        participants.Add(new ParticipantDto
        {
            UserId = args.UserId,
            Username = args.Username,
            IsOnline = true
        });
        
        StateHasChanged();
        return Task.CompletedTask;
    }
    
    private Task HandleUserLeft(UserLeftEventArgs args)
    {
        var participant = participants.FirstOrDefault(p => p.UserId == args.UserId);
        if (participant != null)
        {
            participant.IsOnline = false;
        }
        
        StateHasChanged();
        return Task.CompletedTask;
    }
    
    private async Task HandleChatKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await SendMessage();
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        // Se désabonner
        SessionHub.OnMessageReceived -= HandleMessageReceived;
        SessionHub.OnUserJoined -= HandleUserJoined;
        SessionHub.OnUserLeft -= HandleUserLeft;
        
        // Quitter la session
        if (SessionHub.IsConnected)
        {
            await SessionHub.LeaveSessionAsync(SessionId);
        }
    }
}
```

---

## 7. Styles et CSS

### 7.1 Organisation CSS

```
wwwroot/
├── css/
│   ├── app.css (global)
│   ├── components/
│   │   ├── character-card.css
│   │   ├── health-bar.css
│   │   └── dice-roller.css
│   └── pages/
│       ├── character-list.css
│       ├── session-view.css
│       └── combat-tracker.css
```

---

### 7.2 CSS Isolation

Blazor supporte le **CSS isolé** par composant :

**CharacterCard.razor.css :**
```css
.character-card {
    border: 1px solid #ddd;
    border-radius: 8px;
    padding: 16px;
    cursor: pointer;
    transition: all 0.2s ease;
}

.character-card:hover {
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    transform: translateY(-2px);
}

.character-image {
    width: 100%;
    height: 200px;
    overflow: hidden;
    border-radius: 4px;
    margin-bottom: 12px;
}

.character-image img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.no-image {
    display: flex;
    align-items: center;
    justify-content: center;
    height: 100%;
    background-color: #f5f5f5;
    font-size: 64px;
    color: #ccc;
}
```

---

## 8. Bonnes pratiques

### 8.1 Checklist

| ✅ | Pratique |
|----|----------|
| ✅ | Séparation Component-Handler-Service |
| ✅ | Code-behind (.razor.cs) pour la logique UI |
| ✅ | Handlers pour la logique métier |
| ✅ | API Clients pour les appels HTTP |
| ✅ | @rendermode InteractiveServer sur les pages interactives |
| ✅ | CSS isolé par composant (.razor.css) |
| ✅ | EventCallback pour la communication parent-enfant |
| ✅ | CascadingValue pour le partage d'état global |
| ✅ | IAsyncDisposable pour cleanup SignalR |
| ✅ | Loading states et error handling |
| ✅ | Validation côté client (DataAnnotations) |

---

### 8.2 Performance

**Éviter les re-renders inutiles :**
```csharp
// ✅ BON: Utiliser ShouldRender
protected override bool ShouldRender()
{
    return _hasChanges;
}

// ✅ BON: Utiliser @key pour les listes
@foreach (var character in Characters)
{
    <CharacterCard @key="character.Id" Character="@character" />
}
```

**Utiliser StateHasChanged avec parcimonie :**
```csharp
// ❌ MAUVAIS: Appeler StateHasChanged() dans une boucle
foreach (var item in items)
{
    ProcessItem(item);
    StateHasChanged(); // Re-render à chaque itération
}

// ✅ BON: Appeler StateHasChanged() une seule fois
foreach (var item in items)
{
    ProcessItem(item);
}
StateHasChanged(); // Re-render une fois
```

---

## 9. Résumé des patterns

| Pattern | Description |
|---------|-------------|
| **Component-Handler-Service** | Séparation UI / Logique / API |
| **.razor.cs** | Code-behind pour la logique UI |
| **.handler.cs** | Orchestration métier |
| **ApiClient** | Appels HTTP avec retry/error handling |
| **CascadingValue** | Partage d'état entre composants |
| **EventCallback** | Communication parent-enfant |
| **IAsyncDisposable** | Cleanup ressources (SignalR) |
| **CSS Isolation** | Styles scopés par composant |

---

## Prochaines étapes

1. ✅ Implémenter tous les composants Pages/
2. ✅ Créer les composants Shared/ réutilisables
3. ✅ Configurer l'authentification avec AuthStateProvider
4. 🔄 Créer les thèmes CSS (dark mode, Phase 5)
5. 🔄 Optimiser les performances (virtualisation listes, Phase 5)
6. 🔄 Ajouter les tests Playwright (Phase 5)

---

**Document créé le :** 15 octobre 2025  
**Dernière mise à jour :** 15 octobre 2025  
**Version :** 1.0

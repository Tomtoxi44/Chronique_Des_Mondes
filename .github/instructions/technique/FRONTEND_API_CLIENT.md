# 📡 Frontend API Client - Standards et Conventions

> **Dernière mise à jour** : 16 octobre 2025  
> **Version** : 1.0.0

---

## 📋 Table des Matières

- [1. Architecture Générale](#1-architecture-générale)
- [2. Configuration](#2-configuration)
- [3. Authentication & Authorization](#3-authentication--authorization)
- [4. HTTP Client Setup](#4-http-client-setup)
- [5. Error Handling](#5-error-handling)
- [6. Request/Response Patterns](#6-requestresponse-patterns)
- [7. Caching Strategy](#7-caching-strategy)
- [8. Real-time Communication (SignalR)](#8-real-time-communication-signalr)
- [9. Logging & Monitoring](#9-logging--monitoring)
- [10. Best Practices](#10-best-practices)

---

## 1. Architecture Générale

### 1.1 Pattern Service Layer

```
┌─────────────────────────────────────────────────┐
│          Blazor Components (.razor)             │
│  → Affichage UI, événements utilisateur         │
└────────────────────┬────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────┐
│         Code-Behind (.razor.cs)                 │
│  → Gestion état local, appels aux handlers      │
└────────────────────┬────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────┐
│            Handlers (.handler.cs)               │
│  → Logique métier, orchestration                │
└────────────────────┬────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────┐
│         API Clients (AuthApiClient.cs)          │
│  → Appels HTTP, gestion erreurs                 │
└────────────────────┬────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────┐
│              API REST Backend                   │
│  → Endpoints, Business Logic, Database          │
└─────────────────────────────────────────────────┘
```

### 1.2 Structure des fichiers

```
Cdm.Web/
├── Services/
│   ├── ApiClients/
│   │   ├── Base/
│   │   │   ├── BaseApiClient.cs          # Client de base avec méthodes communes
│   │   │   └── ApiClientOptions.cs       # Configuration
│   │   ├── AuthApiClient.cs              # Authentification
│   │   ├── CharacterApiClient.cs         # Personnages
│   │   ├── CampaignApiClient.cs          # Campagnes
│   │   ├── SessionApiClient.cs           # Sessions
│   │   └── CombatApiClient.cs            # Combat
│   ├── SignalR/
│   │   ├── SessionHubClient.cs
│   │   ├── CombatHubClient.cs
│   │   └── NotificationHubClient.cs
│   ├── State/
│   │   ├── CustomAuthStateProvider.cs    # État d'authentification
│   │   ├── AppStateService.cs            # État application
│   │   └── CacheService.cs               # Cache local
│   └── Storage/
│       └── LocalStorageService.cs        # Wrapper localStorage
```

---

## 2. Configuration

### 2.1 appsettings.json

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7001",
    "Timeout": 30,
    "RetryCount": 3,
    "RetryDelay": 1000
  },
  "SignalR": {
    "HubUrls": {
      "Session": "/hubs/session",
      "Combat": "/hubs/combat",
      "Notification": "/hubs/notification"
    },
    "AutoReconnect": true,
    "ReconnectInterval": 5000
  },
  "Cache": {
    "DefaultExpiration": 300,
    "CharactersCacheDuration": 600,
    "CampaignsCacheDuration": 600
  }
}
```

### 2.2 appsettings.Development.json

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7001",
    "Timeout": 60,
    "EnableDetailedErrors": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Cdm.Web.Services": "Debug"
    }
  }
}
```

### 2.3 Injection dans Program.cs

```csharp
// Cdm.Web/Program.cs
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

builder.Services.Configure<SignalRSettings>(
    builder.Configuration.GetSection("SignalR"));
```

---

## 3. Authentication & Authorization

### 3.1 Token JWT - Stockage

**Localisation** : `LocalStorage` pour le MVP

```csharp
// Services/Storage/LocalStorageService.cs
public interface ILocalStorageService
{
    Task<string?> GetItemAsync(string key);
    Task SetItemAsync(string key, string value);
    Task RemoveItemAsync(string key);
}

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    
    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    public async Task<string?> GetItemAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
    }
    
    public async Task SetItemAsync(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }
    
    public async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
```

**Clés de stockage** :
- `auth_token` : JWT token
- `auth_user_id` : User ID
- `auth_user_email` : User email

### 3.2 CustomAuthenticationStateProvider

```csharp
// Services/State/CustomAuthStateProvider.cs
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<CustomAuthStateProvider> _logger;
    
    public CustomAuthStateProvider(
        ILocalStorageService localStorage,
        ILogger<CustomAuthStateProvider> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync("auth_token");
        
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogDebug("No token found, user is anonymous");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        
        try
        {
            var userId = await _localStorage.GetItemAsync("auth_user_id");
            var userEmail = await _localStorage.GetItemAsync("auth_user_email");
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId ?? ""),
                new Claim(ClaimTypes.Email, userEmail ?? ""),
                new Claim(ClaimTypes.Name, userEmail ?? "")
            };
            
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            
            _logger.LogInformation("User authenticated: {Email}", userEmail);
            
            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing authentication token");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
    
    public async Task MarkUserAsAuthenticated(int userId, string email, string token)
    {
        await _localStorage.SetItemAsync("auth_token", token);
        await _localStorage.SetItemAsync("auth_user_id", userId.ToString());
        await _localStorage.SetItemAsync("auth_user_email", email);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, email)
        };
        
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        
        _logger.LogInformation("User marked as authenticated: {Email}", email);
    }
    
    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync("auth_token");
        await _localStorage.RemoveItemAsync("auth_user_id");
        await _localStorage.RemoveItemAsync("auth_user_email");
        
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        
        _logger.LogInformation("User logged out");
    }
}
```

### 3.3 Enregistrement dans Program.cs

```csharp
// Cdm.Web/Program.cs
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
    sp.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
```

---

## 4. HTTP Client Setup

### 4.1 BaseApiClient

```csharp
// Services/ApiClients/Base/BaseApiClient.cs
using System.Net.Http.Json;
using System.Text.Json;

public abstract class BaseApiClient
{
    protected readonly HttpClient _httpClient;
    protected readonly ILogger _logger;
    protected readonly ILocalStorageService _localStorage;
    
    protected BaseApiClient(
        HttpClient httpClient,
        ILogger logger,
        ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _logger = logger;
        _localStorage = localStorage;
    }
    
    protected async Task AddAuthorizationHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
    
    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            await AddAuthorizationHeaderAsync();
            
            _logger.LogDebug("GET {Endpoint}", endpoint);
            
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP GET error for {Endpoint}", endpoint);
            throw;
        }
    }
    
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest data)
    {
        try
        {
            await AddAuthorizationHeaderAsync();
            
            _logger.LogDebug("POST {Endpoint}", endpoint);
            
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP POST error for {Endpoint}", endpoint);
            throw;
        }
    }
    
    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest data)
    {
        try
        {
            await AddAuthorizationHeaderAsync();
            
            _logger.LogDebug("PUT {Endpoint}", endpoint);
            
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP PUT error for {Endpoint}", endpoint);
            throw;
        }
    }
    
    protected async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            await AddAuthorizationHeaderAsync();
            
            _logger.LogDebug("DELETE {Endpoint}", endpoint);
            
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP DELETE error for {Endpoint}", endpoint);
            return false;
        }
    }
}
```

### 4.2 Configuration HttpClient

```csharp
// Cdm.Web/Program.cs
builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] 
        ?? "https://localhost:7001";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<CharacterApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] 
        ?? "https://localhost:7001";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Repeat for other API clients...
```

---

## 5. Error Handling

### 5.1 ApiException

```csharp
// Services/ApiClients/Base/ApiException.cs
public class ApiException : Exception
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }
    public Dictionary<string, string[]>? ValidationErrors { get; }
    
    public ApiException(
        int statusCode, 
        string message, 
        string? errorCode = null,
        Dictionary<string, string[]>? validationErrors = null) 
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
    }
}
```

### 5.2 Gestion des erreurs dans BaseApiClient

```csharp
protected async Task<T?> GetAsync<T>(string endpoint)
{
    try
    {
        await AddAuthorizationHeaderAsync();
        var response = await _httpClient.GetAsync(endpoint);
        
        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponseAsync(response);
        }
        
        return await response.Content.ReadFromJsonAsync<T>();
    }
    catch (ApiException)
    {
        throw; // Re-throw ApiException
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error for GET {Endpoint}", endpoint);
        throw new ApiException(500, "An unexpected error occurred");
    }
}

private async Task HandleErrorResponseAsync(HttpResponseMessage response)
{
    var statusCode = (int)response.StatusCode;
    var content = await response.Content.ReadAsStringAsync();
    
    try
    {
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content);
        
        throw new ApiException(
            statusCode,
            errorResponse?.Error ?? "An error occurred",
            errorResponse?.ErrorCode,
            errorResponse?.ValidationErrors
        );
    }
    catch (JsonException)
    {
        throw new ApiException(statusCode, content);
    }
}
```

### 5.3 ErrorResponse DTO

```csharp
// Shared/DTOs/ErrorResponse.cs
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
```

### 5.4 Affichage des erreurs dans les composants

```csharp
// Components/Pages/Auth/Register.razor.cs
private async Task HandleRegisterAsync()
{
    try
    {
        IsLoading = true;
        ErrorMessage = null;
        
        var response = await _authApiClient.RegisterAsync(registerRequest);
        
        if (response != null)
        {
            // Success
            await _authStateProvider.MarkUserAsAuthenticated(
                response.UserId, 
                response.Email, 
                response.Token);
            
            _navigationManager.NavigateTo("/characters");
        }
    }
    catch (ApiException ex)
    {
        _logger.LogError(ex, "Registration failed");
        
        if (ex.ValidationErrors != null)
        {
            // Afficher erreurs de validation
            ValidationErrors = ex.ValidationErrors;
        }
        else
        {
            // Afficher message d'erreur général
            ErrorMessage = ex.Message;
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error during registration");
        ErrorMessage = "Une erreur inattendue s'est produite. Veuillez réessayer.";
    }
    finally
    {
        IsLoading = false;
    }
}
```

---

## 6. Request/Response Patterns

### 6.1 DTOs côté client

Utiliser les **mêmes DTOs** que l'API backend ou créer des copies dans le projet Web.

```csharp
// Shared/DTOs/Auth/RegisterRequest.cs
public class RegisterRequest
{
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Email invalide")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Le mot de passe est requis")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères, une majuscule, une minuscule, un chiffre et un caractère spécial")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La confirmation est requise")]
    [Compare(nameof(Password), ErrorMessage = "Les mots de passe ne correspondent pas")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class RegisterResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
```

### 6.2 Serialization Options

```csharp
// Program.cs
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
```

---

## 7. Caching Strategy

### 7.1 CacheService

```csharp
// Services/State/CacheService.cs
public interface ICacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    void Remove(string key);
    void Clear();
}

public class CacheService : ICacheService
{
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly ILogger<CacheService> _logger;
    
    public CacheService(ILogger<CacheService> logger)
    {
        _logger = logger;
    }
    
    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.Expiration > DateTime.UtcNow)
            {
                _logger.LogDebug("Cache hit: {Key}", key);
                return (T?)entry.Value;
            }
            
            _logger.LogDebug("Cache expired: {Key}", key);
            _cache.Remove(key);
        }
        
        _logger.LogDebug("Cache miss: {Key}", key);
        return default;
    }
    
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var exp = expiration ?? TimeSpan.FromMinutes(5);
        
        _cache[key] = new CacheEntry
        {
            Value = value,
            Expiration = DateTime.UtcNow.Add(exp)
        };
        
        _logger.LogDebug("Cache set: {Key} (expires in {Minutes}min)", 
            key, exp.TotalMinutes);
    }
    
    public void Remove(string key)
    {
        _cache.Remove(key);
        _logger.LogDebug("Cache removed: {Key}", key);
    }
    
    public void Clear()
    {
        _cache.Clear();
        _logger.LogInformation("Cache cleared");
    }
    
    private class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime Expiration { get; set; }
    }
}
```

### 7.2 Usage dans API Clients

```csharp
public class CharacterApiClient : BaseApiClient
{
    private readonly ICacheService _cache;
    
    public async Task<List<Character>> GetMyCharactersAsync(bool forceRefresh = false)
    {
        const string cacheKey = "my_characters";
        
        if (!forceRefresh)
        {
            var cached = _cache.Get<List<Character>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
        }
        
        var characters = await GetAsync<List<Character>>("/api/characters");
        
        if (characters != null)
        {
            _cache.Set(cacheKey, characters, TimeSpan.FromMinutes(10));
        }
        
        return characters ?? new List<Character>();
    }
}
```

### 7.3 Invalidation du cache

```csharp
// Après création/modification/suppression
await _characterApiClient.CreateCharacterAsync(newCharacter);
_cache.Remove("my_characters"); // Invalider le cache
```

---

## 8. Real-time Communication (SignalR)

### 8.1 SessionHubClient

```csharp
// Services/SignalR/SessionHubClient.cs
using Microsoft.AspNetCore.SignalR.Client;

public class SessionHubClient : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly ILogger<SessionHubClient> _logger;
    
    public event Action<string>? OnPlayerJoined;
    public event Action<string>? OnPlayerLeft;
    public event Action<int>? OnChapterChanged;
    
    public SessionHubClient(
        IConfiguration configuration,
        ILocalStorageService localStorage,
        ILogger<SessionHubClient> logger)
    {
        _logger = logger;
        
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
        var hubUrl = $"{baseUrl}/hubs/session";
        
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    return await localStorage.GetItemAsync("auth_token");
                };
            })
            .WithAutomaticReconnect()
            .Build();
        
        RegisterHandlers();
    }
    
    private void RegisterHandlers()
    {
        _connection.On<string>("PlayerJoined", (playerName) =>
        {
            _logger.LogInformation("Player joined: {PlayerName}", playerName);
            OnPlayerJoined?.Invoke(playerName);
        });
        
        _connection.On<string>("PlayerLeft", (playerName) =>
        {
            _logger.LogInformation("Player left: {PlayerName}", playerName);
            OnPlayerLeft?.Invoke(playerName);
        });
        
        _connection.On<int>("ChapterChanged", (chapterId) =>
        {
            _logger.LogInformation("Chapter changed: {ChapterId}", chapterId);
            OnChapterChanged?.Invoke(chapterId);
        });
    }
    
    public async Task StartAsync()
    {
        if (_connection.State == HubConnectionState.Disconnected)
        {
            await _connection.StartAsync();
            _logger.LogInformation("SignalR connection started");
        }
    }
    
    public async Task StopAsync()
    {
        if (_connection.State == HubConnectionState.Connected)
        {
            await _connection.StopAsync();
            _logger.LogInformation("SignalR connection stopped");
        }
    }
    
    public async Task JoinSessionAsync(int sessionId)
    {
        await _connection.InvokeAsync("JoinSession", sessionId);
        _logger.LogInformation("Joined session {SessionId}", sessionId);
    }
    
    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
```

### 8.2 Enregistrement dans Program.cs

```csharp
builder.Services.AddScoped<SessionHubClient>();
builder.Services.AddScoped<CombatHubClient>();
builder.Services.AddScoped<NotificationHubClient>();
```

---

## 9. Logging & Monitoring

### 9.1 Structured Logging

```csharp
_logger.LogInformation(
    "User {UserId} created character {CharacterId}", 
    userId, 
    characterId);

_logger.LogWarning(
    "Failed login attempt for email {Email}", 
    email);

_logger.LogError(
    exception, 
    "Error fetching characters for user {UserId}", 
    userId);
```

### 9.2 Performance Monitoring

```csharp
using var activity = Activity.StartActivity("FetchCharacters");
activity?.SetTag("userId", userId);

var stopwatch = Stopwatch.StartNew();
var characters = await _characterApiClient.GetMyCharactersAsync();
stopwatch.Stop();

_logger.LogInformation(
    "Fetched {Count} characters in {Ms}ms", 
    characters.Count, 
    stopwatch.ElapsedMilliseconds);
```

---

## 10. Best Practices

### 10.1 Checklist

- ✅ Toujours utiliser `async/await`
- ✅ Gérer les erreurs avec `try/catch`
- ✅ Logger les événements importants
- ✅ Invalider le cache après modifications
- ✅ Utiliser DTOs typés
- ✅ Ajouter le token JWT dans les headers
- ✅ Timeout configuré (30s par défaut)
- ✅ Afficher loading states pendant les appels
- ✅ Feedback utilisateur (toast, messages)
- ✅ Valider côté client ET serveur

### 10.2 Anti-patterns à éviter

- ❌ Appels API synchrones (`GetAwaiter().GetResult()`)
- ❌ Ignorer les exceptions
- ❌ Stocker des données sensibles en clair
- ❌ Oublier de disposer des ressources (HttpClient, SignalR)
- ❌ Appels API répétés sans cache
- ❌ Bloquer le thread UI
- ❌ Logger des données personnelles

---

## 11. Testing

### 11.1 Mock des API Clients

```csharp
// Tests/Mocks/MockAuthApiClient.cs
public class MockAuthApiClient : IAuthApiClient
{
    public Task<RegisterResponse?> RegisterAsync(RegisterRequest request)
    {
        return Task.FromResult(new RegisterResponse
        {
            UserId = 1,
            Email = request.Email,
            Token = "mock_token_123",
            Message = "Success"
        });
    }
}
```

### 11.2 Test d'intégration

```csharp
// Tests/Integration/AuthApiClientTests.cs
[Fact]
public async Task RegisterAsync_ValidData_ReturnsSuccess()
{
    // Arrange
    var client = _factory.CreateClient();
    var authClient = new AuthApiClient(client, _logger, _localStorage);
    
    var request = new RegisterRequest
    {
        Email = "test@example.com",
        Password = "SecurePass123!",
        ConfirmPassword = "SecurePass123!"
    };
    
    // Act
    var response = await authClient.RegisterAsync(request);
    
    // Assert
    Assert.NotNull(response);
    Assert.Equal("test@example.com", response.Email);
    Assert.NotEmpty(response.Token);
}
```

---

**Document créé pour Chronique des Mondes**  
**Dernière révision** : 16 octobre 2025

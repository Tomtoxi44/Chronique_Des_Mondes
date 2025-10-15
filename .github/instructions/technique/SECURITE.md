# Sécurité - Chronique des Mondes

## Vue d'ensemble

La sécurité est implémentée en **plusieurs couches** pour garantir l'intégrité des données et la protection contre les abus. Le système utilise une approche de **défense en profondeur** avec validation à chaque niveau.

### Principes de conception

1. **Multi-couche** : API → Business → Data (3 niveaux de validation)
2. **JWT Authentication** : Tokens signés avec expiration
3. **Authorization Policies** : Vérification fine des permissions
4. **Query Filters** : Isolation automatique des données par utilisateur
5. **Server-side dice rolling** : Anti-triche pour les jets de dés
6. **Rate limiting** : Protection contre les abus (Phase 6)

---

## 1. Authentification JWT

### 1.1 Génération de tokens

**Emplacement :** `Cdm.Common/Services/JwtService.cs`

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

public interface IJwtService
{
    string GenerateToken(int userId, string email);
    ClaimsPrincipal ValidateToken(string token);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;
    
    // ⚠️ TODO: Externaliser dans Azure Key Vault ou User Secrets
    private const string SECRET_KEY = "VotreCléSecrèteTrèsLongueEtComplexe123!@#";
    private const int EXPIRATION_DAYS = 7;
    
    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public string GenerateToken(int userId, string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(SECRET_KEY);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(EXPIRATION_DAYS),
            Issuer = "ChroniqueMondAPI",
            Audience = "ChroniqueMondWeb",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        
        _logger.LogInformation("JWT token generated for user {UserId}", userId);
        
        return tokenString;
    }
    
    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(SECRET_KEY);
        
        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "ChroniqueMondAPI",
                ValidateAudience = true,
                ValidAudience = "ChroniqueMondWeb",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Pas de tolérance sur l'expiration
            };
            
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("Token expired");
            throw;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            _logger.LogError("Invalid token signature");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            throw;
        }
    }
}
```

---

### 1.2 Configuration dans Program.cs

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configuration JWT
var key = Encoding.ASCII.GetBytes("VotreCléSecrèteTrèsLongueEtComplexe123!@#");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = "ChroniqueMondAPI",
        ValidateAudience = true,
        ValidAudience = "ChroniqueMondWeb",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
    // Support pour SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
```

---

### 1.3 Hashage des mots de passe

**Emplacement :** `Cdm.Common/Services/PasswordService.cs`

```csharp
using BCrypt.Net;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordService : IPasswordService
{
    private const int WORK_FACTOR = 12; // Coût de calcul (2^12 itérations)
    
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));
        
        // BCrypt génère automatiquement un salt unique
        return BCrypt.Net.BCrypt.HashPassword(password, WORK_FACTOR);
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;
        
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
```

**Utilisation lors de l'inscription :**
```csharp
var passwordHash = _passwordService.HashPassword(request.Password);
var user = new User
{
    Username = request.Username,
    Email = request.Email,
    PasswordHash = passwordHash
};
```

**Utilisation lors de la connexion :**
```csharp
var user = await _userRepository.GetByEmailAsync(request.Email);
if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
{
    return Result<LoginResponse>.Failure("Invalid credentials");
}
```

---

## 2. Autorisation par Policies

### 2.1 Définition des policies

**Emplacement :** `Cdm.ApiService/Authorization/AuthorizationPolicies.cs`

```csharp
public static class AuthorizationPolicies
{
    public const string IsCharacterOwner = "IsCharacterOwner";
    public const string IsGameMaster = "IsGameMaster";
    public const string IsSessionParticipant = "IsSessionParticipant";
    public const string IsCampaignParticipant = "IsCampaignParticipant";
    
    public static void AddCustomPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(IsCharacterOwner, policy =>
                policy.Requirements.Add(new IsCharacterOwnerRequirement()));
            
            options.AddPolicy(IsGameMaster, policy =>
                policy.Requirements.Add(new IsGameMasterRequirement()));
            
            options.AddPolicy(IsSessionParticipant, policy =>
                policy.Requirements.Add(new IsSessionParticipantRequirement()));
            
            options.AddPolicy(IsCampaignParticipant, policy =>
                policy.Requirements.Add(new IsCampaignParticipantRequirement()));
        });
        
        // Enregistrer les handlers
        services.AddScoped<IAuthorizationHandler, IsCharacterOwnerHandler>();
        services.AddScoped<IAuthorizationHandler, IsGameMasterHandler>();
        services.AddScoped<IAuthorizationHandler, IsSessionParticipantHandler>();
        services.AddScoped<IAuthorizationHandler, IsCampaignParticipantHandler>();
    }
}
```

---

### 2.2 Handler: IsCharacterOwner

Vérifie que l'utilisateur connecté est le propriétaire du personnage.

**Emplacement :** `Cdm.ApiService/Authorization/IsCharacterOwnerHandler.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class IsCharacterOwnerRequirement : IAuthorizationRequirement { }

public class IsCharacterOwnerHandler : AuthorizationHandler<IsCharacterOwnerRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICharacterRepository _characterRepository;
    private readonly ILogger<IsCharacterOwnerHandler> _logger;
    
    public IsCharacterOwnerHandler(
        IHttpContextAccessor httpContextAccessor,
        ICharacterRepository characterRepository,
        ILogger<IsCharacterOwnerHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _characterRepository = characterRepository;
        _logger = logger;
    }
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IsCharacterOwnerRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return;
        }
        
        // Récupérer l'ID du personnage depuis la route
        var characterIdString = httpContext.GetRouteValue("id")?.ToString() 
            ?? httpContext.GetRouteValue("characterId")?.ToString();
        
        if (string.IsNullOrEmpty(characterIdString) || !int.TryParse(characterIdString, out var characterId))
        {
            _logger.LogWarning("Character ID not found in route");
            context.Fail();
            return;
        }
        
        // Récupérer l'ID de l'utilisateur connecté
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("User ID not found in claims");
            context.Fail();
            return;
        }
        
        // Vérifier la propriété
        var character = await _characterRepository.GetByIdAsync(characterId);
        if (character == null)
        {
            _logger.LogWarning("Character {CharacterId} not found", characterId);
            context.Fail();
            return;
        }
        
        if (character.UserId == userId)
        {
            _logger.LogInformation(
                "User {UserId} authorized as owner of character {CharacterId}",
                userId, characterId);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "User {UserId} is not the owner of character {CharacterId} (owner: {OwnerId})",
                userId, characterId, character.UserId);
            context.Fail();
        }
    }
}
```

**Utilisation dans un endpoint :**
```csharp
group.MapGet("/{id}", GetCharacter)
    .RequireAuthorization(AuthorizationPolicies.IsCharacterOwner);

group.MapPut("/{id}", UpdateCharacter)
    .RequireAuthorization(AuthorizationPolicies.IsCharacterOwner);

group.MapDelete("/{id}", DeleteCharacter)
    .RequireAuthorization(AuthorizationPolicies.IsCharacterOwner);
```

---

### 2.3 Handler: IsGameMaster

Vérifie que l'utilisateur est le MJ de la campagne associée.

**Emplacement :** `Cdm.ApiService/Authorization/IsGameMasterHandler.cs`

```csharp
public class IsGameMasterRequirement : IAuthorizationRequirement { }

public class IsGameMasterHandler : AuthorizationHandler<IsGameMasterRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISessionRepository _sessionRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICombatRepository _combatRepository;
    private readonly ILogger<IsGameMasterHandler> _logger;
    
    public IsGameMasterHandler(
        IHttpContextAccessor httpContextAccessor,
        ISessionRepository sessionRepository,
        ICampaignRepository campaignRepository,
        ICombatRepository combatRepository,
        ILogger<IsGameMasterHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _sessionRepository = sessionRepository;
        _campaignRepository = campaignRepository;
        _combatRepository = combatRepository;
        _logger = logger;
    }
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IsGameMasterRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return;
        }
        
        var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        
        // Déterminer l'entité (session, combat, ou campaign)
        var sessionIdString = httpContext.GetRouteValue("sessionId")?.ToString() 
            ?? httpContext.GetRouteValue("id")?.ToString();
        var combatIdString = httpContext.GetRouteValue("combatId")?.ToString();
        var campaignIdString = httpContext.GetRouteValue("campaignId")?.ToString() 
            ?? httpContext.GetRouteValue("id")?.ToString();
        
        int? gameMasterId = null;
        
        // Vérifier via session
        if (!string.IsNullOrEmpty(sessionIdString) && int.TryParse(sessionIdString, out var sessionId))
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session != null)
            {
                var campaign = await _campaignRepository.GetByIdAsync(session.CampaignId);
                gameMasterId = campaign?.GameMasterId;
            }
        }
        // Vérifier via combat
        else if (!string.IsNullOrEmpty(combatIdString) && int.TryParse(combatIdString, out var combatId))
        {
            var combat = await _combatRepository.GetByIdAsync(combatId);
            if (combat != null)
            {
                var session = await _sessionRepository.GetByIdAsync(combat.SessionId);
                var campaign = await _campaignRepository.GetByIdAsync(session.CampaignId);
                gameMasterId = campaign?.GameMasterId;
            }
        }
        // Vérifier via campaign
        else if (!string.IsNullOrEmpty(campaignIdString) && int.TryParse(campaignIdString, out var campaignId))
        {
            var campaign = await _campaignRepository.GetByIdAsync(campaignId);
            gameMasterId = campaign?.GameMasterId;
        }
        
        if (gameMasterId.HasValue && gameMasterId.Value == userId)
        {
            _logger.LogInformation("User {UserId} authorized as Game Master", userId);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {UserId} is not the Game Master", userId);
            context.Fail();
        }
    }
}
```

**Utilisation :**
```csharp
// Démarrer une session
sessionGroup.MapPost("/{id}/start", StartSession)
    .RequireAuthorization(AuthorizationPolicies.IsGameMaster);

// Passer au tour suivant
combatGroup.MapPost("/{id}/next-turn", NextTurn)
    .RequireAuthorization(AuthorizationPolicies.IsGameMaster);

// Infliger des dégâts
combatGroup.MapPost("/{id}/damage", DealDamage)
    .RequireAuthorization(AuthorizationPolicies.IsGameMaster);
```

---

### 2.4 Handler: IsSessionParticipant

Vérifie que l'utilisateur participe à la session (MJ ou joueur).

```csharp
public class IsSessionParticipantRequirement : IAuthorizationRequirement { }

public class IsSessionParticipantHandler : AuthorizationHandler<IsSessionParticipantRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISessionRepository _sessionRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ILogger<IsSessionParticipantHandler> _logger;
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IsSessionParticipantRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return;
        }
        
        var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var sessionIdString = httpContext.GetRouteValue("id")?.ToString() 
            ?? httpContext.GetRouteValue("sessionId")?.ToString();
        
        if (!int.TryParse(sessionIdString, out var sessionId))
        {
            context.Fail();
            return;
        }
        
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            context.Fail();
            return;
        }
        
        var campaign = await _campaignRepository.GetByIdAsync(session.CampaignId);
        
        // Vérifier si MJ
        if (campaign.GameMasterId == userId)
        {
            _logger.LogInformation("User {UserId} authorized as Game Master", userId);
            context.Succeed(requirement);
            return;
        }
        
        // Vérifier si participant
        var isParticipant = await _campaignRepository.IsParticipantAsync(campaign.Id, userId);
        if (isParticipant)
        {
            _logger.LogInformation("User {UserId} authorized as participant", userId);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {UserId} is not a participant of session {SessionId}", userId, sessionId);
            context.Fail();
        }
    }
}
```

---

## 3. Isolation des données (Query Filters)

### 3.1 Configuration EF Core

Les **Query Filters** permettent d'isoler automatiquement les données par utilisateur.

**Emplacement :** `Cdm.Data.Common/AppDbContext.cs`

```csharp
public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public DbSet<Character> Characters { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Filtre global: ne jamais retourner les personnages supprimés
        modelBuilder.Entity<Character>().HasQueryFilter(c => !c.IsDeleted);
        
        // Filtre global: isoler les personnages par utilisateur
        modelBuilder.Entity<Character>().HasQueryFilter(c => 
            c.UserId == GetCurrentUserId());
        
        // Filtre global: isoler les campagnes où l'utilisateur est MJ ou participant
        modelBuilder.Entity<Campaign>().HasQueryFilter(c =>
            c.GameMasterId == GetCurrentUserId() ||
            c.CampaignCharacters.Any(cc => cc.Character.UserId == GetCurrentUserId()));
    }
    
    private int? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
            return null;
        
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return null;
        
        return int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }
}
```

**Désactiver les filtres temporairement (si nécessaire) :**
```csharp
var allCharacters = await _context.Characters
    .IgnoreQueryFilters() // Ignore tous les filtres
    .ToListAsync();
```

---

### 3.2 Validation au niveau Business

Les Query Filters ne suffisent pas toujours. Il faut **valider dans la couche business**.

```csharp
public class CharacterService : ICharacterService
{
    public async Task<Result<Character>> UpdateAsync(int characterId, UpdateCharacterRequest request, int userId)
    {
        // 1. Récupérer le personnage
        var character = await _repository.GetByIdAsync(characterId);
        if (character == null)
        {
            return Result<Character>.Failure("Character not found");
        }
        
        // 2. Vérifier la propriété (défense en profondeur)
        if (character.UserId != userId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to update character {CharacterId} owned by {OwnerId}",
                userId, characterId, character.UserId);
            
            return Result<Character>.Failure("You do not have permission to update this character");
        }
        
        // 3. Valider les données métier
        if (request.CurrentHealth > request.MaxHealth)
        {
            return Result<Character>.Failure("Current health cannot exceed max health");
        }
        
        // 4. Appliquer les modifications
        character.Name = request.Name;
        character.CurrentHealth = request.CurrentHealth;
        character.MaxHealth = request.MaxHealth;
        character.UpdatedAt = DateTime.UtcNow;
        
        await _repository.UpdateAsync(character);
        
        return Result<Character>.Success(character);
    }
}
```

---

## 4. Anti-triche : Lancers de dés serveur

### 4.1 Principe

**Tous les jets de dés sont effectués côté serveur** pour éviter la manipulation des résultats.

```csharp
public class DiceService : IDiceService
{
    private readonly IDiceRollRepository _repository;
    private readonly ILogger<DiceService> _logger;
    
    public async Task<DiceRollResult> RollAsync(DiceRollRequest request)
    {
        // 1. Parser la notation (ex: "2d6+3")
        var (diceCount, diceType, modifier) = ParseNotation(request.DiceNotation);
        
        // 2. Lancer les dés CÔTÉ SERVEUR
        var results = new List<int>();
        for (int i = 0; i < diceCount; i++)
        {
            results.Add(Random.Shared.Next(1, diceType + 1));
        }
        
        var total = results.Sum() + modifier;
        
        // 3. Enregistrer dans la base de données
        var diceRoll = new DiceRoll
        {
            SessionId = request.SessionId,
            CharacterId = request.CharacterId,
            UserId = request.UserId,
            DiceNotation = request.DiceNotation,
            Results = JsonSerializer.Serialize(results),
            Modifiers = modifier,
            TotalResult = total,
            RollType = request.RollType,
            Context = request.Context,
            RolledAt = DateTime.UtcNow
        };
        
        await _repository.CreateAsync(diceRoll);
        
        _logger.LogInformation(
            "User {UserId} rolled {Notation}: {Results} + {Modifier} = {Total}",
            request.UserId, request.DiceNotation, string.Join(", ", results), modifier, total);
        
        return new DiceRollResult
        {
            Id = diceRoll.Id,
            Results = results,
            Modifiers = modifier,
            TotalResult = total,
            RolledAt = diceRoll.RolledAt
        };
    }
    
    private (int diceCount, int diceType, int modifier) ParseNotation(string notation)
    {
        var match = Regex.Match(notation, @"(\d+)d(\d+)([\+\-]\d+)?");
        if (!match.Success)
            throw new ArgumentException($"Invalid dice notation: {notation}");
        
        var diceCount = int.Parse(match.Groups[1].Value);
        var diceType = int.Parse(match.Groups[2].Value);
        var modifier = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
        
        return (diceCount, diceType, modifier);
    }
}
```

**Le client ne peut JAMAIS envoyer le résultat :**
```csharp
// ❌ MAUVAIS: Le client envoie le résultat (risque de triche)
public async Task RollDice(int sessionId, int characterId, int result) { }

// ✅ BON: Le client demande un lancer, le serveur calcule
public async Task RollDice(int sessionId, int characterId, string diceNotation) { }
```

---

### 4.2 Vérification des résultats historiques

Les MJ peuvent vérifier l'historique pour détecter des anomalies.

```csharp
public class DiceStatisticsService
{
    public async Task<DiceAnomalyReport> DetectAnomaliesAsync(int characterId, int sessionId)
    {
        var rolls = await _diceRollRepository.GetByCharacterAndSessionAsync(characterId, sessionId);
        
        // Calculer les statistiques
        var d20Rolls = rolls.Where(r => r.DiceNotation.StartsWith("1d20")).ToList();
        var averageD20 = d20Rolls.Average(r => r.TotalResult - r.Modifiers);
        
        // Moyenne théorique d'1d20 = 10.5
        const double EXPECTED_AVERAGE = 10.5;
        const double ANOMALY_THRESHOLD = 2.0; // Plus de 2 points de différence
        
        var anomalies = new List<string>();
        
        if (Math.Abs(averageD20 - EXPECTED_AVERAGE) > ANOMALY_THRESHOLD)
        {
            anomalies.Add($"Moyenne anormale sur 1d20: {averageD20:F2} (attendu: {EXPECTED_AVERAGE})");
        }
        
        // Vérifier les 20 naturels (devrait être ~5%)
        var nat20Count = d20Rolls.Count(r => r.Results == "[20]");
        var nat20Percentage = (double)nat20Count / d20Rolls.Count * 100;
        
        if (nat20Percentage > 10) // Plus de 10% de critiques
        {
            anomalies.Add($"Trop de 20 naturels: {nat20Percentage:F1}% (attendu: ~5%)");
        }
        
        return new DiceAnomalyReport
        {
            CharacterId = characterId,
            TotalRolls = d20Rolls.Count,
            AverageResult = averageD20,
            CriticalSuccessRate = nat20Percentage,
            Anomalies = anomalies
        };
    }
}
```

---

## 5. Protection des échanges d'équipement

### 5.1 Transaction atomique

Les échanges utilisent des **transactions EF Core** pour garantir l'atomicité.

```csharp
public class EquipmentService : IEquipmentService
{
    public async Task<Result<EquipmentTrade>> AcceptProposalAsync(int proposalId, int userId)
    {
        // Démarrer une transaction
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // 1. Récupérer la proposition
            var proposal = await _context.EquipmentProposals
                .Include(p => p.ProposedItems)
                .Include(p => p.RequestedItems)
                .FirstOrDefaultAsync(p => p.Id == proposalId);
            
            if (proposal == null)
                return Result<EquipmentTrade>.Failure("Proposal not found");
            
            // 2. Vérifier que l'utilisateur est le destinataire
            var toCharacter = await _context.Characters.FindAsync(proposal.ToCharacterId);
            if (toCharacter.UserId != userId)
                return Result<EquipmentTrade>.Failure("You are not the recipient of this trade");
            
            // 3. Vérifier que les items existent toujours
            foreach (var item in proposal.ProposedItems)
            {
                var charEquip = await _context.CharacterEquipment
                    .FirstOrDefaultAsync(ce => 
                        ce.CharacterId == proposal.FromCharacterId &&
                        ce.EquipmentId == item.EquipmentId &&
                        ce.Quantity >= item.Quantity);
                
                if (charEquip == null)
                {
                    await transaction.RollbackAsync();
                    return Result<EquipmentTrade>.Failure(
                        $"Item {item.EquipmentId} is no longer available");
                }
            }
            
            // 4. Transférer les items proposés (From → To)
            foreach (var item in proposal.ProposedItems)
            {
                await TransferItemAsync(proposal.FromCharacterId, proposal.ToCharacterId, item);
            }
            
            // 5. Transférer les items demandés (To → From) si échange
            if (proposal.RequestedItems != null)
            {
                foreach (var item in proposal.RequestedItems)
                {
                    await TransferItemAsync(proposal.ToCharacterId, proposal.FromCharacterId, item);
                }
            }
            
            // 6. Créer l'historique
            var trade = new EquipmentTrade
            {
                ProposalId = proposalId,
                TransactionDetails = JsonSerializer.Serialize(proposal),
                ExecutedAt = DateTime.UtcNow
            };
            _context.EquipmentTrades.Add(trade);
            
            // 7. Mettre à jour le statut de la proposition
            proposal.Status = ProposalStatus.Accepted;
            proposal.ResolvedAt = DateTime.UtcNow;
            
            // 8. Sauvegarder et commit
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            _logger.LogInformation(
                "Equipment trade {ProposalId} completed successfully",
                proposalId);
            
            return Result<EquipmentTrade>.Success(trade);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to complete equipment trade {ProposalId}", proposalId);
            return Result<EquipmentTrade>.Failure("Transaction failed");
        }
    }
    
    private async Task TransferItemAsync(int fromCharacterId, int toCharacterId, EquipmentItem item)
    {
        // Retirer de l'inventaire source
        var fromEquip = await _context.CharacterEquipment
            .FirstOrDefaultAsync(ce => 
                ce.CharacterId == fromCharacterId &&
                ce.EquipmentId == item.EquipmentId);
        
        if (fromEquip.Quantity == item.Quantity)
        {
            _context.CharacterEquipment.Remove(fromEquip);
        }
        else
        {
            fromEquip.Quantity -= item.Quantity;
        }
        
        // Ajouter à l'inventaire destination
        var toEquip = await _context.CharacterEquipment
            .FirstOrDefaultAsync(ce => 
                ce.CharacterId == toCharacterId &&
                ce.EquipmentId == item.EquipmentId);
        
        if (toEquip == null)
        {
            _context.CharacterEquipment.Add(new CharacterEquipment
            {
                CharacterId = toCharacterId,
                EquipmentId = item.EquipmentId,
                Quantity = item.Quantity,
                IsEquipped = false,
                AcquiredAt = DateTime.UtcNow
            });
        }
        else
        {
            toEquip.Quantity += item.Quantity;
        }
    }
}
```

---

## 6. Sécurité SignalR

### 6.1 Authentification JWT pour SignalR

```csharp
// Configuration dans Program.cs
options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        // SignalR envoie le token via query string
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        
        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
        {
            context.Token = accessToken;
        }
        
        return Task.CompletedTask;
    }
};
```

---

### 6.2 Validation d'accès dans les hubs

```csharp
[Authorize] // Tous les hubs nécessitent authentication
public class SessionHub : Hub
{
    public async Task JoinSession(int sessionId)
    {
        var userId = GetUserId();
        
        // Vérifier que l'utilisateur a accès à cette session
        var hasAccess = await _sessionService.UserHasAccessAsync(sessionId, userId);
        if (!hasAccess)
        {
            _logger.LogWarning(
                "User {UserId} attempted to join session {SessionId} without access",
                userId, sessionId);
            
            throw new HubException("Access denied to this session");
        }
        
        // Ajouter au groupe isolé
        var groupName = $"session_{sessionId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} joined session {SessionId}", userId, sessionId);
    }
}
```

---

### 6.3 Isolation par groupes

```csharp
// ✅ BON: Envoyer uniquement au groupe de la session
await Clients.Group($"session_{sessionId}").SendAsync("MessageReceived", message);

// ✅ BON: Envoyer uniquement à un utilisateur spécifique
await Clients.User(userId.ToString()).SendAsync("Notification", notification);

// ❌ MAUVAIS: Envoyer à tous les clients (fuite d'informations)
await Clients.All.SendAsync("MessageReceived", message);
```

---

## 7. Validation des données (FluentValidation)

### 7.1 Configuration

```bash
dotnet add package FluentValidation.AspNetCore
```

```csharp
// Program.cs
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

---

### 7.2 Exemple de validateur

**Emplacement :** `Cdm.ApiService/Validators/CreateCharacterRequestValidator.cs`

```csharp
using FluentValidation;

public class CreateCharacterRequestValidator : AbstractValidator<CreateCharacterRequest>
{
    public CreateCharacterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom est requis")
            .Length(3, 100).WithMessage("Le nom doit contenir entre 3 et 100 caractères")
            .Matches("^[a-zA-Z0-9 '-]+$").WithMessage("Le nom contient des caractères invalides");
        
        RuleFor(x => x.GameType)
            .IsInEnum().WithMessage("Type de jeu invalide");
        
        RuleFor(x => x.MaxHealth)
            .GreaterThan(0).WithMessage("Les PV max doivent être supérieurs à 0")
            .LessThanOrEqualTo(1000).WithMessage("Les PV max ne peuvent pas dépasser 1000");
        
        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("La description ne peut pas dépasser 5000 caractères");
        
        When(x => x.GameType == GameType.Dnd5e, () =>
        {
            RuleFor(x => x.Attributes)
                .NotNull().WithMessage("Les attributs D&D sont requis")
                .Must(HaveValidDndAttributes).WithMessage("Attributs D&D invalides");
        });
    }
    
    private bool HaveValidDndAttributes(string attributesJson)
    {
        try
        {
            var attributes = JsonSerializer.Deserialize<DndAttributes>(attributesJson);
            
            // Vérifier les scores de caractéristiques (1-20 pour D&D 5e)
            if (attributes.abilityScores.STR < 1 || attributes.abilityScores.STR > 20) return false;
            if (attributes.abilityScores.DEX < 1 || attributes.abilityScores.DEX > 20) return false;
            if (attributes.abilityScores.CON < 1 || attributes.abilityScores.CON > 20) return false;
            if (attributes.abilityScores.INT < 1 || attributes.abilityScores.INT > 20) return false;
            if (attributes.abilityScores.WIS < 1 || attributes.abilityScores.WIS > 20) return false;
            if (attributes.abilityScores.CHA < 1 || attributes.abilityScores.CHA > 20) return false;
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

**Utilisation dans un endpoint :**
```csharp
group.MapPost("/", async (
    CreateCharacterRequest request,
    IValidator<CreateCharacterRequest> validator,
    ICharacterService characterService) =>
{
    // Valider
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(new
        {
            error = "Validation failed",
            details = validationResult.Errors.Select(e => e.ErrorMessage)
        });
    }
    
    // Créer
    var result = await characterService.CreateAsync(request);
    
    return result.IsSuccess
        ? Results.Created($"/api/characters/{result.Data.Id}", result.Data)
        : Results.BadRequest(result.Error);
});
```

---

## 8. Rate Limiting (Phase 6)

### 8.1 Configuration

```csharp
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(options =>
{
    // Limite globale: 100 requêtes par minute par IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));
    
    // Politique pour les lancers de dés: 30 par minute
    options.AddFixedWindowLimiter("dice", options =>
    {
        options.PermitLimit = 30;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });
    
    // Politique pour les authentifications: 5 tentatives par 15 minutes
    options.AddFixedWindowLimiter("auth", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromMinutes(15);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });
});

var app = builder.Build();

app.UseRateLimiter();
```

**Utilisation sur un endpoint :**
```csharp
group.MapPost("/roll", RollDice)
    .RequireRateLimiting("dice");

authGroup.MapPost("/login", Login)
    .RequireRateLimiting("auth");
```

---

## 9. Bonnes pratiques de sécurité

### 9.1 Checklist

| ✅ | Pratique |
|----|----------|
| ✅ | Mots de passe hashés avec BCrypt (work factor 12) |
| ✅ | JWT avec expiration (7 jours max) |
| ✅ | HTTPS obligatoire en production |
| ✅ | Authorization policies sur tous les endpoints sensibles |
| ✅ | Query filters EF Core pour isolation des données |
| ✅ | Validation multi-couches (API → Business → Data) |
| ✅ | Lancers de dés côté serveur uniquement |
| ✅ | Transactions atomiques pour les échanges |
| ✅ | Groupes SignalR pour isolation |
| ✅ | FluentValidation pour valider les entrées |
| ⏳ | Rate limiting (Phase 6) |
| ⏳ | Secrets dans Azure Key Vault (Phase 6) |
| ⏳ | Logging des actions sensibles |
| ⏳ | CORS configuré strictement |

---

### 9.2 Secrets et configuration

**❌ MAUVAIS (hardcodé) :**
```csharp
private const string SECRET_KEY = "VotreCléSecrèteTrèsLongueEtComplexe123!@#";
```

**✅ BON (User Secrets en dev, Key Vault en prod) :**
```csharp
// appsettings.Development.json (ignoré par Git)
{
  "Jwt": {
    "SecretKey": "VotreCléSecrèteTrèsLongueEtComplexe123!@#",
    "Issuer": "ChroniqueMondAPI",
    "Audience": "ChroniqueMondWeb",
    "ExpirationDays": 7
  }
}

// Program.cs
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"];
```

**Production avec Azure Key Vault :**
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

---

### 9.3 Logging des actions sensibles

```csharp
_logger.LogWarning(
    "SECURITY: User {UserId} attempted to access character {CharacterId} owned by {OwnerId}",
    userId, characterId, character.UserId);

_logger.LogInformation(
    "AUDIT: User {UserId} deleted character {CharacterId}",
    userId, characterId);

_logger.LogError(
    "SECURITY: Invalid login attempt for email {Email}",
    email);
```

---

## 10. Résumé des couches de sécurité

```
┌─────────────────────────────────────────────────────────┐
│                  Requête HTTP                           │
└─────────────┬───────────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────────┐
│  Couche 1: Authentification JWT                         │
│  → Vérification du token                                │
│  → Extraction des claims (userId, email)                │
└─────────────┬───────────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────────┐
│  Couche 2: Authorization Policies                       │
│  → IsCharacterOwner, IsGameMaster, etc.                 │
│  → Vérification des permissions dans la DB              │
└─────────────┬───────────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────────┐
│  Couche 3: Validation des données (FluentValidation)   │
│  → Format, longueur, contraintes métier                 │
└─────────────┬───────────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────────┐
│  Couche 4: Business Layer                               │
│  → Vérification de propriété (défense en profondeur)    │
│  → Règles métier (GameType compatibility, etc.)         │
└─────────────┬───────────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────────┐
│  Couche 5: Query Filters EF Core                        │
│  → Filtrage automatique par userId                      │
│  → Exclusion des entités supprimées (soft delete)       │
└─────────────┬───────────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────────┐
│                   Base de données                        │
└─────────────────────────────────────────────────────────┘
```

---

## Prochaines étapes

1. ✅ Implémenter tous les Authorization Handlers
2. ✅ Configurer les Query Filters dans AppDbContext
3. ✅ Créer les validateurs FluentValidation
4. 🔄 Externaliser les secrets (Azure Key Vault)
5. 🔄 Implémenter le rate limiting (Phase 6)
6. 🔄 Ajouter le logging d'audit complet
7. 🔄 Configurer CORS strictement
8. 🔄 Tests de sécurité (penetration testing)

---

**Document créé le :** 15 octobre 2025  
**Dernière mise à jour :** 15 octobre 2025  
**Version :** 1.0

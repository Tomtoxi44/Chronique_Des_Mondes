# Standards de Code - Chronique des Mondes

## Vue d'ensemble

Ce document définit les **conventions de codage** et les **bonnes pratiques** à suivre pour garantir la cohérence, la maintenabilité et la qualité du code dans le projet Chronique des Mondes.

### Principes directeurs

1. **Cohérence** : Le code doit être uniforme dans tout le projet
2. **Lisibilité** : Privilégier la clarté à la concision
3. **Maintenabilité** : Faciliter les modifications futures
4. **Testabilité** : Concevoir pour les tests unitaires
5. **Performance** : Optimiser sans sacrifier la lisibilité

---

## 1. Conventions de nommage .NET

### 1.1 Général

| Type | Convention | Exemple |
|------|------------|---------|
| **Namespaces** | PascalCase | `Cdm.Business.Common` |
| **Classes** | PascalCase | `CharacterService` |
| **Interfaces** | PascalCase + préfixe `I` | `ICharacterService` |
| **Méthodes** | PascalCase | `GetCharacterById` |
| **Propriétés** | PascalCase | `CurrentHealth` |
| **Variables locales** | camelCase | `characterId` |
| **Paramètres** | camelCase | `userId` |
| **Champs privés** | camelCase + préfixe `_` | `_characterRepository` |
| **Constantes** | PascalCase ou UPPER_CASE | `MaxHealth` ou `MAX_HEALTH` |
| **Enums** | PascalCase (type et valeurs) | `GameType.Dnd5e` |

---

### 1.2 Exemples

```csharp
// ✅ BON
public class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _characterRepository;
    private readonly ILogger<CharacterService> _logger;
    private const int MaxCharactersPerUser = 50;
    
    public CharacterService(
        ICharacterRepository characterRepository,
        ILogger<CharacterService> logger)
    {
        _characterRepository = characterRepository;
        _logger = logger;
    }
    
    public async Task<Result<Character>> GetCharacterByIdAsync(int characterId, int userId)
    {
        var character = await _characterRepository.GetByIdAsync(characterId);
        
        if (character == null)
        {
            return Result<Character>.Failure("Character not found");
        }
        
        return Result<Character>.Success(character);
    }
}

// ❌ MAUVAIS
public class characterservice // Mauvaise casse
{
    private ICharacterRepository characterRepository; // Manque le _
    private const int max_characters_per_user = 50; // Mauvaise convention
    
    public async Task<Result<Character>> get_character_by_id(int CharacterId) // Mauvaise casse
    {
        var Character = await characterRepository.GetByIdAsync(CharacterId); // Variable en PascalCase
        return Result<Character>.Success(Character);
    }
}
```

---

## 2. Organisation des fichiers

### 2.1 Structure d'un fichier C#

```csharp
// 1. Usings (triés alphabétiquement, System en premier)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Cdm.Data.Common.Models;

// 2. Namespace
namespace Cdm.Business.Common.Services;

// 3. Classe principale
public class CharacterService : ICharacterService
{
    // 4. Champs privés
    private readonly ICharacterRepository _repository;
    private readonly ILogger<CharacterService> _logger;
    
    // 5. Constructeur
    public CharacterService(
        ICharacterRepository repository,
        ILogger<CharacterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    // 6. Propriétés publiques
    public int MaxCharactersPerUser { get; } = 50;
    
    // 7. Méthodes publiques
    public async Task<Result<Character>> GetByIdAsync(int id)
    {
        // Implémentation
    }
    
    // 8. Méthodes privées
    private bool ValidateCharacter(Character character)
    {
        // Implémentation
    }
}
```

---

### 2.2 Nom des fichiers

| Type | Convention | Exemple |
|------|------------|---------|
| **Classe** | Nom de la classe | `CharacterService.cs` |
| **Interface** | Nom de l'interface | `ICharacterService.cs` |
| **Enum** | Nom de l'enum | `GameType.cs` |
| **Record** | Nom du record | `DiceRollResult.cs` |
| **Extension methods** | Nom descriptif + Extensions | `StringExtensions.cs` |

**Un fichier = une classe principale** (sauf records/DTOs liés)

---

## 3. Formatage du code

### 3.1 Indentation et espacement

```csharp
// ✅ BON: Indentation de 4 espaces
public async Task<Result<Character>> CreateAsync(CreateCharacterRequest request)
{
    if (request == null)
    {
        return Result<Character>.Failure("Request cannot be null");
    }
    
    var character = new Character
    {
        Name = request.Name,
        GameType = request.GameType,
        MaxHealth = request.MaxHealth
    };
    
    await _repository.CreateAsync(character);
    
    return Result<Character>.Success(character);
}

// ❌ MAUVAIS: Indentation incohérente
public async Task<Result<Character>> CreateAsync(CreateCharacterRequest request)
{
if (request == null)
{
return Result<Character>.Failure("Request cannot be null");
}
  var character = new Character
  {
      Name = request.Name,
    GameType = request.GameType,
      MaxHealth = request.MaxHealth
  };
await _repository.CreateAsync(character);
return Result<Character>.Success(character);
}
```

---

### 3.2 Accolades

```csharp
// ✅ BON: Accolades sur nouvelle ligne (style Allman)
if (character.CurrentHealth <= 0)
{
    character.IsActive = false;
}

// ✅ BON: Accolades même pour une seule ligne (plus sûr)
if (character == null)
{
    return Result<Character>.Failure("Not found");
}

// ❌ À ÉVITER: Pas d'accolades (risque d'erreurs)
if (character == null)
    return Result<Character>.Failure("Not found");
```

---

### 3.3 Sauts de ligne

```csharp
// ✅ BON: Saut de ligne entre les sections logiques
public async Task<Result<Character>> UpdateAsync(int id, UpdateCharacterRequest request)
{
    // 1. Validation
    if (request == null)
    {
        return Result<Character>.Failure("Request cannot be null");
    }
    
    // 2. Récupération
    var character = await _repository.GetByIdAsync(id);
    if (character == null)
    {
        return Result<Character>.Failure("Character not found");
    }
    
    // 3. Mise à jour
    character.Name = request.Name;
    character.CurrentHealth = request.CurrentHealth;
    character.UpdatedAt = DateTime.UtcNow;
    
    // 4. Sauvegarde
    await _repository.UpdateAsync(character);
    
    return Result<Character>.Success(character);
}
```

---

## 4. Commentaires et documentation

### 4.1 XML Documentation

```csharp
/// <summary>
/// Récupère un personnage par son identifiant.
/// </summary>
/// <param name="characterId">L'identifiant unique du personnage.</param>
/// <param name="userId">L'identifiant de l'utilisateur connecté.</param>
/// <returns>Un <see cref="Result{Character}"/> contenant le personnage si trouvé.</returns>
/// <exception cref="ArgumentException">Si characterId est inférieur ou égal à 0.</exception>
public async Task<Result<Character>> GetByIdAsync(int characterId, int userId)
{
    if (characterId <= 0)
    {
        throw new ArgumentException("Character ID must be greater than 0", nameof(characterId));
    }
    
    var character = await _repository.GetByIdAsync(characterId);
    
    if (character == null)
    {
        return Result<Character>.Failure("Character not found");
    }
    
    if (character.UserId != userId)
    {
        _logger.LogWarning(
            "User {UserId} attempted to access character {CharacterId} owned by {OwnerId}",
            userId, characterId, character.UserId);
        
        return Result<Character>.Failure("Access denied");
    }
    
    return Result<Character>.Success(character);
}
```

---

### 4.2 Commentaires en ligne

```csharp
// ✅ BON: Commentaires pour expliquer le "pourquoi", pas le "quoi"
public int CalculateInitiativeOrder(Character character, int initiativeRoll)
{
    // D&D 5e utilise le modificateur de DEX comme bris d'égalité
    var dexModifier = (character.Attributes.DEX - 10) / 2;
    return initiativeRoll + dexModifier;
}

// ❌ MAUVAIS: Commentaires qui répètent le code
public int CalculateInitiativeOrder(Character character, int initiativeRoll)
{
    // Calculer le modificateur de DEX
    var dexModifier = (character.Attributes.DEX - 10) / 2;
    
    // Retourner l'initiative + modificateur
    return initiativeRoll + dexModifier;
}

// ✅ BON: TODO pour marquer les tâches à faire
public async Task SendEmailAsync(string email, string subject, string body)
{
    // TODO: Implémenter le retry en cas d'échec
    await _emailService.SendAsync(email, subject, body);
}
```

---

### 4.3 Commentaires de section

```csharp
public class CharacterService
{
    #region Fields
    
    private readonly ICharacterRepository _repository;
    private readonly ILogger<CharacterService> _logger;
    
    #endregion
    
    #region Constructor
    
    public CharacterService(
        ICharacterRepository repository,
        ILogger<CharacterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    #endregion
    
    #region Public Methods
    
    public async Task<Result<Character>> GetByIdAsync(int id)
    {
        // Implémentation
    }
    
    #endregion
    
    #region Private Methods
    
    private bool ValidateCharacter(Character character)
    {
        // Implémentation
    }
    
    #endregion
}
```

---

## 5. Gestion des erreurs

### 5.1 Pattern Result<T>

```csharp
// ✅ BON: Utiliser Result<T> pour retourner succès ou échec
public async Task<Result<Character>> CreateCharacterAsync(CreateCharacterRequest request)
{
    try
    {
        var character = new Character
        {
            Name = request.Name,
            GameType = request.GameType
        };
        
        await _repository.CreateAsync(character);
        
        return Result<Character>.Success(character);
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Database error while creating character");
        return Result<Character>.Failure("Database error occurred");
    }
}

// ❌ MAUVAIS: Lancer des exceptions pour les erreurs métier
public async Task<Character> CreateCharacterAsync(CreateCharacterRequest request)
{
    if (string.IsNullOrEmpty(request.Name))
    {
        throw new ValidationException("Name is required"); // Exception pour validation
    }
    
    var character = new Character { Name = request.Name };
    await _repository.CreateAsync(character);
    
    return character;
}
```

---

### 5.2 Logging structuré

```csharp
// ✅ BON: Logging structuré avec placeholders
_logger.LogInformation(
    "Character {CharacterId} created by user {UserId}",
    character.Id,
    userId);

_logger.LogWarning(
    "Failed login attempt for email {Email} from IP {IpAddress}",
    email,
    ipAddress);

_logger.LogError(
    ex,
    "Failed to update character {CharacterId}",
    characterId);

// ❌ MAUVAIS: Interpolation de strings
_logger.LogInformation($"Character {character.Id} created by user {userId}");
```

---

### 5.3 Try-catch approprié

```csharp
// ✅ BON: Catch spécifique et log
public async Task<Result<Character>> DeleteAsync(int id)
{
    try
    {
        var character = await _repository.GetByIdAsync(id);
        if (character == null)
        {
            return Result<Character>.Failure("Character not found");
        }
        
        await _repository.DeleteAsync(id);
        
        return Result<Character>.Success(character);
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Database error while deleting character {CharacterId}", id);
        return Result<Character>.Failure("Failed to delete character");
    }
}

// ❌ MAUVAIS: Catch générique sans log
public async Task<Character> DeleteAsync(int id)
{
    try
    {
        var character = await _repository.GetByIdAsync(id);
        await _repository.DeleteAsync(id);
        return character;
    }
    catch
    {
        return null; // Perte d'information sur l'erreur
    }
}
```

---

## 6. Async/Await

### 6.1 Bonnes pratiques

```csharp
// ✅ BON: Async/await cohérent
public async Task<Result<Character>> GetByIdAsync(int id)
{
    var character = await _repository.GetByIdAsync(id);
    
    if (character == null)
    {
        return Result<Character>.Failure("Not found");
    }
    
    return Result<Character>.Success(character);
}

// ✅ BON: ConfigureAwait(false) dans les bibliothèques
public async Task<Character> GetByIdAsync(int id)
{
    var character = await _repository.GetByIdAsync(id).ConfigureAwait(false);
    return character;
}

// ❌ MAUVAIS: Bloquer avec .Result ou .Wait()
public Character GetById(int id)
{
    var character = _repository.GetByIdAsync(id).Result; // Risque de deadlock
    return character;
}

// ❌ MAUVAIS: async void (sauf event handlers)
public async void DeleteCharacter(int id) // Pas de gestion d'erreur possible
{
    await _repository.DeleteAsync(id);
}
```

---

### 6.2 Nommage des méthodes async

```csharp
// ✅ BON: Suffixe Async pour les méthodes asynchrones
public async Task<Character> GetCharacterAsync(int id) { }
public async Task<List<Character>> GetAllCharactersAsync() { }
public async Task DeleteCharacterAsync(int id) { }

// ❌ MAUVAIS: Pas de suffixe Async
public async Task<Character> GetCharacter(int id) { }
public async Task DeleteCharacter(int id) { }
```

---

## 7. LINQ et collections

### 7.1 Requêtes LINQ

```csharp
// ✅ BON: Syntaxe de méthode (préféré pour la simplicité)
var activeCharacters = characters
    .Where(c => c.IsActive)
    .OrderBy(c => c.Name)
    .ToList();

// ✅ BON: Syntaxe de requête (pour les requêtes complexes)
var charactersByLevel = from c in characters
                        where c.Level >= 5
                        orderby c.Level descending, c.Name
                        select new { c.Name, c.Level };

// ❌ MAUVAIS: Boucle for au lieu de LINQ
var activeCharacters = new List<Character>();
for (int i = 0; i < characters.Count; i++)
{
    if (characters[i].IsActive)
    {
        activeCharacters.Add(characters[i]);
    }
}
```

---

### 7.2 Enumération et matérialisation

```csharp
// ✅ BON: Matérialiser une seule fois
var activeCharacters = characters.Where(c => c.IsActive).ToList();
var count = activeCharacters.Count;
var firstCharacter = activeCharacters.FirstOrDefault();

// ❌ MAUVAIS: Multiples énumérations
var activeCharacters = characters.Where(c => c.IsActive); // IEnumerable
var count = activeCharacters.Count(); // Énumération 1
var firstCharacter = activeCharacters.FirstOrDefault(); // Énumération 2

// ✅ BON: Any() au lieu de Count() pour vérifier l'existence
if (characters.Any(c => c.Level > 10))
{
    // Implémentation
}

// ❌ MAUVAIS: Count() pour vérifier l'existence
if (characters.Where(c => c.Level > 10).Count() > 0)
{
    // Implémentation
}
```

---

## 8. Injection de dépendances

### 8.1 Configuration des services

```csharp
// Program.cs
public static void ConfigureServices(IServiceCollection services)
{
    // Services applicatifs (Scoped pour les services avec état par requête)
    services.AddScoped<ICharacterService, CharacterService>();
    services.AddScoped<ICampaignService, CampaignService>();
    
    // Repositories (Scoped pour DbContext)
    services.AddScoped<ICharacterRepository, CharacterRepository>();
    services.AddScoped<ICampaignRepository, CampaignRepository>();
    
    // Services utilitaires (Singleton pour services sans état)
    services.AddSingleton<IJwtService, JwtService>();
    services.AddSingleton<IPasswordService, PasswordService>();
    
    // Services externes (Transient ou HttpClient)
    services.AddHttpClient<IEmailService, EmailService>();
}
```

---

### 8.2 Injection dans les constructeurs

```csharp
// ✅ BON: Toutes les dépendances dans le constructeur
public class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _repository;
    private readonly ILogger<CharacterService> _logger;
    private readonly IMapper _mapper;
    
    public CharacterService(
        ICharacterRepository repository,
        ILogger<CharacterService> logger,
        IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
}

// ❌ MAUVAIS: Service Locator pattern
public class CharacterService
{
    private readonly IServiceProvider _serviceProvider;
    
    public CharacterService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task DoSomething()
    {
        var repository = _serviceProvider.GetService<ICharacterRepository>(); // Anti-pattern
    }
}
```

---

## 9. Tests unitaires

### 9.1 Conventions de nommage

```csharp
// Pattern: MethodName_Scenario_ExpectedBehavior
[Fact]
public async Task GetByIdAsync_WithValidId_ReturnsCharacter()
{
    // Arrange
    var characterId = 1;
    var expectedCharacter = new Character { Id = characterId, Name = "Test" };
    _repositoryMock.Setup(r => r.GetByIdAsync(characterId))
        .ReturnsAsync(expectedCharacter);
    
    // Act
    var result = await _service.GetByIdAsync(characterId);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedCharacter.Id, result.Data.Id);
}

[Fact]
public async Task GetByIdAsync_WithInvalidId_ReturnsFailure()
{
    // Arrange
    var characterId = 999;
    _repositoryMock.Setup(r => r.GetByIdAsync(characterId))
        .ReturnsAsync((Character?)null);
    
    // Act
    var result = await _service.GetByIdAsync(characterId);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal("Character not found", result.Error);
}
```

---

### 9.2 Organisation des tests

```
Cdm.Tests/
├── Unit/
│   ├── Services/
│   │   ├── CharacterServiceTests.cs
│   │   ├── CampaignServiceTests.cs
│   │   └── DiceServiceTests.cs
│   └── Repositories/
│       ├── CharacterRepositoryTests.cs
│       └── CampaignRepositoryTests.cs
├── Integration/
│   ├── API/
│   │   ├── CharacterEndpointsTests.cs
│   │   └── AuthEndpointsTests.cs
│   └── Database/
│       └── MigrationTests.cs
└── E2E/
    ├── SessionFlowTests.cs
    └── CombatFlowTests.cs
```

---

## 10. Patterns à utiliser

### 10.1 Repository Pattern

```csharp
public interface ICharacterRepository
{
    Task<Character?> GetByIdAsync(int id);
    Task<List<Character>> GetAllAsync();
    Task<Character> CreateAsync(Character character);
    Task UpdateAsync(Character character);
    Task DeleteAsync(int id);
}

public class CharacterRepository : ICharacterRepository
{
    private readonly AppDbContext _context;
    
    public CharacterRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Character?> GetByIdAsync(int id)
    {
        return await _context.Characters
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<Character> CreateAsync(Character character)
    {
        _context.Characters.Add(character);
        await _context.SaveChangesAsync();
        return character;
    }
}
```

---

### 10.2 Strategy Pattern (multi-type)

```csharp
public interface ICharacterService
{
    Task<Result<Character>> CreateAsync(CreateCharacterRequest request);
}

public class GenericCharacterService : ICharacterService
{
    public async Task<Result<Character>> CreateAsync(CreateCharacterRequest request)
    {
        // Logique générique
    }
}

public class DndCharacterService : ICharacterService
{
    public async Task<Result<Character>> CreateAsync(CreateCharacterRequest request)
    {
        // Logique spécifique D&D
    }
}

public class CharacterServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public ICharacterService GetService(GameType gameType)
    {
        return gameType switch
        {
            GameType.Generic => _serviceProvider.GetRequiredService<GenericCharacterService>(),
            GameType.Dnd5e => _serviceProvider.GetRequiredService<DndCharacterService>(),
            GameType.Skyrim => _serviceProvider.GetRequiredService<SkyrimCharacterService>(),
            _ => throw new NotSupportedException($"GameType {gameType} not supported")
        };
    }
}
```

---

### 10.3 Unit of Work Pattern

```csharp
public interface IUnitOfWork : IDisposable
{
    ICharacterRepository Characters { get; }
    ICampaignRepository Campaigns { get; }
    ISessionRepository Sessions { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Characters = new CharacterRepository(context);
        Campaigns = new CampaignRepository(context);
        Sessions = new SessionRepository(context);
    }
    
    public ICharacterRepository Characters { get; }
    public ICampaignRepository Campaigns { get; }
    public ISessionRepository Sessions { get; }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitTransactionAsync()
    {
        await _transaction?.CommitAsync()!;
    }
    
    public async Task RollbackTransactionAsync()
    {
        await _transaction?.RollbackAsync()!;
    }
    
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
```

---

## 11. Anti-patterns à éviter

### 11.1 God Object

```csharp
// ❌ MAUVAIS: Classe qui fait tout
public class GameManager
{
    public void CreateCharacter() { }
    public void UpdateCharacter() { }
    public void DeleteCharacter() { }
    public void CreateCampaign() { }
    public void StartSession() { }
    public void StartCombat() { }
    public void RollDice() { }
    public void SendEmail() { }
    // ... 50 autres méthodes
}

// ✅ BON: Séparation des responsabilités
public class CharacterService { /* Gestion des personnages */ }
public class CampaignService { /* Gestion des campagnes */ }
public class SessionService { /* Gestion des sessions */ }
public class CombatService { /* Gestion des combats */ }
public class DiceService { /* Gestion des dés */ }
public class EmailService { /* Envoi d'emails */ }
```

---

### 11.2 Magic Numbers/Strings

```csharp
// ❌ MAUVAIS: Valeurs hardcodées
if (character.Level > 20)
{
    return "Level cannot exceed 20";
}

// ✅ BON: Constantes nommées
public class GameConstants
{
    public const int MaxCharacterLevel = 20;
    public const int MaxHealthPoints = 1000;
}

if (character.Level > GameConstants.MaxCharacterLevel)
{
    return $"Level cannot exceed {GameConstants.MaxCharacterLevel}";
}
```

---

### 11.3 Primitive Obsession

```csharp
// ❌ MAUVAIS: Utiliser des types primitifs partout
public void SendEmail(string email, string subject, string body) { }

// ✅ BON: Créer des types métier
public record EmailAddress(string Value)
{
    public static EmailAddress Parse(string email)
    {
        if (!email.Contains("@"))
            throw new ArgumentException("Invalid email format");
        
        return new EmailAddress(email);
    }
}

public record Email(EmailAddress To, string Subject, string Body);

public void SendEmail(Email email) { }
```

---

## 12. Configuration EditorConfig

Créer un fichier `.editorconfig` à la racine du projet :

```ini
# EditorConfig pour Chronique des Mondes

root = true

[*]
charset = utf-8
indent_style = space
indent_size = 4
insert_final_newline = true
trim_trailing_whitespace = true

[*.cs]
# Conventions de nommage
dotnet_naming_rule.interface_should_be_begins_with_i.severity = warning
dotnet_naming_rule.interface_should_be_begins_with_i.symbols = interface
dotnet_naming_rule.interface_should_be_begins_with_i.style = begins_with_i

dotnet_naming_rule.private_fields_should_be_camel_case_with_underscore.severity = warning
dotnet_naming_rule.private_fields_should_be_camel_case_with_underscore.symbols = private_fields
dotnet_naming_rule.private_fields_should_be_camel_case_with_underscore.style = camel_case_with_underscore

# Accolades
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true

# Indentation
csharp_indent_case_contents = true
csharp_indent_switch_labels = true

# Espacement
csharp_space_after_keywords_in_control_flow_statements = true
csharp_space_between_method_declaration_parameter_list_parentheses = false

# Préférences de code
csharp_prefer_braces = true:warning
csharp_prefer_simple_using_statement = true:suggestion
dotnet_sort_system_directives_first = true

[*.razor]
indent_size = 4

[*.json]
indent_size = 2

[*.yml]
indent_size = 2
```

---

## 13. Checklist Code Review

### 13.1 Avant de soumettre une PR

- [ ] Le code compile sans warnings
- [ ] Tous les tests unitaires passent
- [ ] Les nouvelles fonctionnalités ont des tests
- [ ] Le code respecte les conventions de nommage
- [ ] Pas de code commenté inutile
- [ ] Pas de console.log ou debug statements
- [ ] Les erreurs sont loggées correctement
- [ ] Les secrets ne sont pas hardcodés
- [ ] La documentation XML est à jour
- [ ] Les migrations EF Core sont générées si besoin

---

### 13.2 Lors de la revue de code

**Performance :**
- [ ] Pas de N+1 queries
- [ ] Utilisation appropriée de async/await
- [ ] LINQ optimisé (Any au lieu de Count, etc.)
- [ ] Pas de boucles inutiles

**Sécurité :**
- [ ] Validation des entrées
- [ ] Authorization policies correctes
- [ ] Pas d'injection SQL
- [ ] Pas de secrets hardcodés

**Maintenabilité :**
- [ ] Code lisible et compréhensible
- [ ] Fonctions courtes (< 50 lignes)
- [ ] Pas de duplication de code
- [ ] Séparation des responsabilités

---

## 14. Outils recommandés

### 14.1 Extensions Visual Studio / VS Code

- **ReSharper / Rider** : Analyse de code avancée
- **SonarLint** : Détection de bugs et code smells
- **CodeMaid** : Nettoyage et formatage automatique
- **Roslynator** : Analyseurs et refactorings C#
- **GitLens** : Historique Git intégré

---

### 14.2 Analyseurs NuGet

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.556">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
</ItemGroup>
```

---

## 15. Commandes utiles

### 15.1 .NET CLI

```bash
# Formater le code
dotnet format

# Analyser le code
dotnet build /p:AnalysisMode=All

# Exécuter les tests avec couverture
dotnet test /p:CollectCoverage=true

# Lister les packages obsolètes
dotnet list package --outdated

# Mettre à jour un package
dotnet add package Cdm.Common
```

---

### 15.2 Git

```bash
# Vérifier le code avant commit
git diff --check

# Stash avec message
git stash push -m "WIP: Character creation"

# Rebase interactif pour nettoyer l'historique
git rebase -i HEAD~3
```

---

## Résumé des bonnes pratiques

| Catégorie | Pratique |
|-----------|----------|
| **Nommage** | PascalCase pour classes/méthodes, camelCase pour variables |
| **Formatage** | 4 espaces, accolades style Allman |
| **Async** | Suffixe Async, pas de .Result/.Wait() |
| **Erreurs** | Result<T> pattern, logging structuré |
| **DI** | Constructor injection, pas de Service Locator |
| **Tests** | Pattern AAA (Arrange-Act-Assert) |
| **LINQ** | Préférer Any() à Count() > 0 |
| **Commentaires** | XML doc publiques, pourquoi pas quoi |

---

## Prochaines étapes

1. ✅ Appliquer EditorConfig sur tous les projets
2. ✅ Configurer les analyseurs (StyleCop, Roslynator)
3. 🔄 Mettre en place CI/CD avec validation du code
4. 🔄 Configurer SonarQube pour l'analyse continue
5. 🔄 Former l'équipe aux conventions
6. 🔄 Effectuer des code reviews régulières

---

**Document créé le :** 15 octobre 2025  
**Dernière mise à jour :** 15 octobre 2025  
**Version :** 1.0

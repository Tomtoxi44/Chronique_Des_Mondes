# Chronique des Mondes - Instructions for AI Agents

## 📋 Project Overview

**Chronique des Mondes** is a tabletop role-playing game management platform built with **.NET 10 + Aspire** that follows a **multi-type architecture** supporting Generic, D&D 5e, and Skyrim game systems.

### Technical Documentation
For detailed technical information, refer to the comprehensive documentation in `.github/instructions/technique/`:

- **[ARCHITECTURE_TECHNIQUE.md](technique/ARCHITECTURE_TECHNIQUE.md)** - Complete architecture overview with 6 phases
- **[MODELE_DONNEES.md](technique/MODELE_DONNEES.md)** - Database schema (17 tables, TPH inheritance, JSON attributes)
- **[API_ENDPOINTS.md](technique/API_ENDPOINTS.md)** - 45+ REST endpoints with authorization
- **[SIGNALR_TEMPS_REEL.md](technique/SIGNALR_TEMPS_REEL.md)** - 3 SignalR hubs (Session, Combat, Notification)
- **[SECURITE.md](technique/SECURITE.md)** - Multi-layer security (JWT, BCrypt, policies, query filters)
- **[FRONTEND_BLAZOR.md](technique/FRONTEND_BLAZOR.md)** - Blazor Server with Component-Handler-Service pattern
- **[STANDARDS_CODE.md](technique/STANDARDS_CODE.md)** - Coding conventions and best practices

## 🎯 Project Vision

**Chronique des Mondes** enables game masters and players to manage RPG campaigns, characters, sessions, and real-time combat with support for multiple game systems (Generic, D&D 5e, Skyrim).

## 📋 Project Configuration

### Core Information
- **Project Name**: Chronique des Mondes
- **Tech Stack**: .NET 10, Aspire, Blazor Server, SignalR, Entity Framework Core 10
- **Architecture**: Multi-type monolith with split database contexts
- **Database**: SQL Server with TPH inheritance
- **Authentication**: JWT with BCrypt password hashing (work factor 12)

### Development Environment
- **Dashboard**: Aspire Dashboard (development only)
- **Main Port**: Configured via Aspire orchestration

#### 📏 Spacing and Formatting
```
# Standards by language:
# C#### 📉 Logging and Observability
**Ba### 🛑 Security Best Practices
**Aligned with {SECURITY_FRAMEWORK} guidance for {TECH_STACK}**d on {LOGGING_FRAMEWORK} and {OBS### 📋 Code Documentation
**{DOCUMENTATION_STANDARD} for {TECH_STACK}**

#### Documentation Rules
- **Language**: ALL COMMENTS IN ENGLISH
- **Public APIs**: {PUBLIC_API_DOC_REQUIREMENT}
- **Function/Method documentation**: {METHOD_DOC_FORMAT}
- **Parameter documentation**: {PARAMETER_DOC_FORMAT}
- **Return values**: {RETURN_VALUE_DOC_FORMAT}
- **Type documentation**: {TYPE_DOC_FORMAT}
- **Formatting rules**: {DOC_FORMATTING_RULES}TACK}**

- **Structured logging**: {STRUCTURED_LOGGING_APPROACH}
- **Business context**: Include {CONTEXT_FIELDS} in every log entry
- **Log levels**: {LOG_LEVELS_HIERARCHY}
- **Security**: Never log {SENSITIVE_DATA_TYPES}
- **Observability tools**: {OBSERVABILITY_TOOLS_USED}es, PascalCase classes, camelCase variables, StyleCop rules
# JavaScript/TypeScript: 2 spaces, camelCase, semicolons, ESLint/Prettier
# Python: 4 spaces, snake_case, PEP 8 compliance, Black formatter
# Java: 4 spaces, PascalCase classes, camelCase methods, Google Style
# Go: tabs, PascalCase exported, camelCase private, gofmt
# Rust: 4 spaces, snake_case, kebab-case packages, rustfmt
```
- **Indentation**: {INDENTATION_STANDARD}
- **Line length**: {LINE_LENGTH_LIMIT} characters max
- **Operator spacing**: {OPERATOR_SPACING_RULE}
- **Comment formatting**: {COMMENT_FORMAT_RULE}
- **Code formatter**: {CODE_FORMATTER_TOOL}*Language-Specific Conventions for {TECH_STACK}**

#### 📏 Spacing and Formatting
```
# Standards by language:
# C#: 4 spaces, PascalCase classes, camelCase variables
# JavaScript/TypeScript: 2 spaces, camelCase, semicolons
# Python: 4 spaces, snake_case, PEP 8 compliance
# Java: 4 spaces, PascalCase classes, camelCase methods
# Go: tabs, PascalCase exported, camelCase private
# Rust: 4 spaces, snake_case, kebab-case packages
```
- **Indentation**: {INDENTATION_STANDARD}
- **Line length**: {LINE_LENGTH_LIMIT} characters max
- **Operator spacing**: {OPERATOR_SPACING_RULE}
- **Comment formatting**: {COMMENT_FORMAT_RULE}**Logging**: {LOGGING_FRAMEWORK} with structured logs
- **Metrics**: Business metrics in `{PROJECT_NAME}.*` namespace
- **Tracing**: {TRACING_IMPLEMENTATION}

### Production Environment
- **Monitoring**: {PRODUCTION_MONITORING}
- **Log Aggregation**: {LOG_AGGREGATION_SERVICE}
- **Metrics Export**: {METRICS_EXPORT_METHOD}
- **Health Checks**: {HEALTH_CHECK_ENDPOINTS}ect Information
- `{PROJECT_NAME}` : Name of your project
- `{PROJECT_DESCRIPTION}` : Brief description of what the project does
- `{TECH_STACK}` : Main technology stack (ex: .NET, Node.js, Python, Java, React, etc.)
- `{ARCHITECTURE_TYPE}` : Architecture type (ex: Microservices, Monolith, Serverless, SPA, etc.)

### 🏗️ Architecture & Infrastructure
- **Solution File**: `Cdm/Cdm.slnx`
- **Build Command**: `dotnet build`
- **Test Command**: `dotnet test`
- **Run Command**: `dotnet run --project Cdm/Cdm.AppHost` (Aspire orchestration)
- **Main Projects**:
  - `Cdm.AppHost` - Aspire orchestration
  - `Cdm.ApiService` - REST API + SignalR hubs
  - `Cdm.Web` - Blazor Server frontend
  - `Cdm.Data.Common` - EF Core (AppDbContext with query filters)
  - `Cdm.Migrations` - EF Core (MigrationsContext for migrations)
  - `Cdm.Business.Common` - Business logic
  - `Cdm.Common` - Shared utilities (EmailService, JwtService, PasswordService)

### 🔐 Authentication & Security
- **Authentication**: JWT tokens (7-day expiry)
- **Password Hashing**: BCrypt (work factor 12)
- **Authorization**: 4 custom policies
  - `IsCharacterOwner` - User owns the character
  - `IsGameMaster` - User is GM of the campaign
  - `IsSessionParticipant` - User participates in session
  - `IsCampaignParticipant` - User is part of campaign
- **Query Filters**: EF Core global filters for data isolation
- **Anti-cheat**: Server-side dice rolling with anomaly detection

### 🗄️ Data Storage
- **Database**: SQL Server
- **Main Context**: `AppDbContext` (runtime with query filters)
- **Migration Context**: `MigrationsContext` (migrations without filters)
- **Pattern**: Split context (see MODELE_DONNEES.md)
- **Main Entities** (17 tables):
  - `Users` - User accounts
  - `Characters` (TPH) - Generic, DndCharacter, SkyrimCharacter
  - `Campaigns` - Campaign management
  - `Sessions` - Game sessions
  - `Combats` - Combat encounters
  - `CombatTurns` - Initiative tracking
  - `Spells` - Spell library + character spells
  - `Equipments` - Equipment library + character inventory
  - `DiceRolls` - Server-validated dice rolls
  - `Notifications` - User notifications

### 🌐 Deployment & Infrastructure
- **Deployment**: Docker containers (Phase 6)
- **Environments**: Development, Production
- **Orchestration**: .NET Aspire
- **Communication**: SignalR with MessagePack protocol

### 📁 Project Structure
```
Cdm/
├── Cdm.slnx                    # Solution file
├── Cdm.AppHost/                # Aspire orchestration
├── Cdm.ApiService/             # REST API + SignalR
│   ├── Endpoints/              # Minimal API endpoints
│   └── Hubs/                   # SignalR hubs (Session, Combat, Notification)
├── Cdm.Web/                    # Blazor Server frontend
│   ├── Components/             # Razor components
│   │   ├── Layout/
│   │   ├── Pages/
│   │   └── Shared/
│   ├── Handlers/               # Component handlers (business logic)
│   └── Services/               # API clients, SignalR services
├── Cdm.Data.Common/            # EF Core (AppDbContext)
│   └── Models/                 # Entity models
├── Cdm.Migrations/             # EF Core (MigrationsContext)
├── Cdm.Business.Common/        # Business services
├── Cdm.Common/                 # Shared utilities
│   ├── Enums/                  # GameType enum
│   └── Services/               # EmailService, JwtService, PasswordService
├── Cdm.Abstraction/            # Interfaces
└── Cdm.ServiceDefaults/        # Aspire defaults
```

### 🎨 UI Technology
- **Framework**: Blazor Server (.NET 10)
- **Pattern**: Component-Handler-Service separation
  - `.razor` files - UI markup only
  - `.razor.cs` files - Component state management
  - `.handler.cs` files - Business logic
  - API clients - HTTP communication
- **Real-time**: SignalR integration in components
- **Styling**: CSS Isolation per component
- **Authentication**: AuthStateProvider with JWT
- **Authorization**: `<AuthorizeView>` with policies

## �️ Architecture Overview

### Critical Architectural Patterns

**Multi-Type Architecture**: The application supports 3 game types through polymorphism:
- **Generic** - Custom game systems
- **Dnd5e** - D&D 5th Edition with D20 mechanics
- **Skyrim** - Elder Scrolls with specific attributes

**TPH Inheritance (Table Per Hierarchy)**:
- Base `Characters` table with `GameType` discriminator
- `GenericCharacter`, `DndCharacter`, `SkyrimCharacter` inherit from `Character`
- JSON columns for type-specific attributes (see MODELE_DONNEES.md)

**Split Context Pattern**:
- `MigrationsContext` - Clean schema for migrations
- `AppDbContext` - Runtime with query filters and configuration
- Prevents circular dependencies and enables global filters

**Component-Handler-Service Pattern** (Blazor):
- **Component (.razor)** - UI markup, no logic
- **Component Code-Behind (.razor.cs)** - State management
- **Handler (.handler.cs)** - Business logic, validation
- **API Client** - HTTP communication with API

**Real-time Communication**:
- **SessionHub** - Chat, dice rolling, equipment trades
- **CombatHub** - Turn management, damage tracking
- **NotificationHub** - User notifications
- Group isolation: `session_{id}`, `combat_{id}`, `user_{id}`

**Security Layers** (see SECURITE.md):
1. JWT authentication (7-day tokens)
2. Authorization policies (4 handlers)
3. EF Core query filters (data isolation)
4. Server-side validation (FluentValidation)
5. Anti-cheat (server dice rolling)

## 🔑 Essential Development Commands

```bash
# Start Aspire orchestration (runs all services)
dotnet run --project Cdm/Cdm.AppHost

# Aspire Dashboard (auto-opens)
# URL: https://localhost:17223 (development only)
# Shows: Logs, Traces, Metrics, Health checks

# Build the solution
dotnet build Cdm/Cdm.slnx

# Run tests
dotnet test

# Database migrations (using MigrationsContext)
cd Cdm/Cdm.Migrations
dotnet ef migrations add MigrationName --context MigrationsContext
dotnet ef database update --context MigrationsContext

# Run migration worker (automated)
dotnet run --project Cdm/Cdm.MigrationsManager

# User secrets (JWT configuration)
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key" --project Cdm/Cdm.Common
dotnet user-secrets set "Jwt:Issuer" "Cdm.ApiService" --project Cdm/Cdm.Common
dotnet user-secrets set "Jwt:Audience" "Cdm.Web" --project Cdm/Cdm.Common

# Azure Communication Services (email invitations)
dotnet user-secrets set "AzureCommunication:ConnectionString" "endpoint=..." --project Cdm/Cdm.Common
```

## 🌐 Deployment Architecture

### Environments
- **Development**: Aspire orchestration, in-memory config, detailed logging
- **Production**: Docker containers, persistent storage, minimal logging

### Phase 6 Production (Future)
- Docker deployment
- HTTPS with Let's Encrypt
- Rate limiting (10 requests/second per user)
- Log aggregation
- Automated backups

## 🔐 Authentication & Authorization

### JWT Configuration
```csharp
// JWT Service (Cdm.Common/Services/JwtService.cs)
// Tokens expire after 7 days
// Includes claims: UserId, Email, Name

// Example token generation
var token = _jwtService.GenerateToken(user.Id, user.Email, user.Name);
```

### Authorization Policies (4 handlers)
```csharp
// IsCharacterOwner - User must own the character
[Authorize(Policy = "IsCharacterOwner")]

// IsGameMaster - User must be GM of the campaign
[Authorize(Policy = "IsGameMaster")]

// IsSessionParticipant - User must participate in session
[Authorize(Policy = "IsSessionParticipant")]

// IsCampaignParticipant - User must be part of campaign
[Authorize(Policy = "IsCampaignParticipant")]
```

### EF Core Query Filters (Data Isolation)
```csharp
// AppDbContext automatically filters data by UserId
modelBuilder.Entity<Character>()
    .HasQueryFilter(c => c.UserId == _currentUserId);

modelBuilder.Entity<Campaign>()
    .HasQueryFilter(c => c.Participants.Any(p => p.UserId == _currentUserId));
```

### Password Security
- **Hashing**: BCrypt with work factor 12
- **Service**: `Cdm.Common/Services/PasswordService.cs`
- **Validation**: Minimum 8 characters, complexity rules

## 🗄️ Database Configuration

### SQL Server - Split Context Pattern

**MigrationsContext** (`Cdm.Migrations/MigrationsContext.cs`):
- Used ONLY for migrations
- Clean schema without query filters
- No navigation properties configured

**AppDbContext** (`Cdm.Data.Common/AppDbContext.cs`):
- Used at runtime
- Global query filters for data isolation
- Full relationship configuration
- Configured via `OnModelCreating`

### 17 Entity Tables (see MODELE_DONNEES.md)

```csharp
// Core entities
Users                    // User accounts with BCrypt passwords
Characters               // TPH: Generic, DndCharacter, SkyrimCharacter
  - GenericAttributes    // JSON column {Strength, Dexterity, ...}
  - DndAttributes        // JSON column {STR, DEX, CON, INT, WIS, CHA}
  - SkyrimAttributes     // JSON column {Health, Magicka, Stamina, ...}

// Campaign management
Campaigns               // Campaign metadata (GameType, GameMasterUserId)
CampaignParticipants    // Many-to-many: Users ↔ Campaigns
Sessions                // Game sessions with InvitationToken
SessionParticipants     // Many-to-many: Users ↔ Sessions

// Combat system
Combats                 // Combat encounters
CombatTurns             // Initiative order with Initiative value
CombatParticipants      // Many-to-many: Characters ↔ Combats

// Game mechanics
Spells                  // Spell library (generic + character-specific)
Equipments              // Equipment library (generic + character inventory)
DiceRolls               // Server-validated rolls (anti-cheat)
Notifications           // User notifications
```

### Migration Commands
```bash
# Generate new migration (using MigrationsContext)
cd Cdm/Cdm.Migrations
dotnet ef migrations add MigrationName --context MigrationsContext

# Apply migrations (using MigrationsContext)
dotnet ef database update --context MigrationsContext

# Automated worker (runs on startup)
dotnet run --project Cdm/Cdm.MigrationsManager
```

## 🎨 User Interface (Blazor Server)

### Component-Handler-Service Pattern

**Structure** (see FRONTEND_BLAZOR.md):
```
Cdm.Web/
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor           # App shell
│   ├── Pages/
│   │   ├── CharacterList.razor        # UI markup only
│   │   ├── CharacterList.razor.cs     # State management
│   │   └── CharacterList.handler.cs   # Business logic
│   └── Shared/
│       ├── CharacterCard.razor        # Reusable card component
│       ├── HealthBar.razor            # Visual health display
│       └── DiceRoller.razor           # Interactive dice rolling
├── Services/
│   ├── ApiClients/
│   │   ├── CharacterApiClient.cs      # HTTP calls to API
│   │   └── CampaignApiClient.cs
│   ├── SignalR/
│   │   ├── SessionHubService.cs       # SignalR client for sessions
│   │   ├── CombatHubService.cs        # SignalR client for combat
│   │   └── NotificationHubService.cs  # SignalR client for notifications
│   └── AuthStateProvider.cs           # JWT token management
└── wwwroot/
    └── css/                            # CSS isolation per component
```

### Blazor-Specific Patterns
- **Rendering**: Server-side rendering with SignalR
- **State Management**: Component state + CascadingValue for global state
- **Authorization**: `<AuthorizeView Policy="IsCharacterOwner">`
- **Real-time Updates**: SignalR integration in components
- **CSS Isolation**: `ComponentName.razor.css` per component
- **Performance**: `@key` directives, `ShouldRender()` optimization

### Example Component Pattern
```csharp
// CharacterList.razor (UI only)
@page "/characters"
@attribute [Authorize]

<div class="character-list">
    @foreach (var character in Characters)
    {
        <CharacterCard Character="@character" 
                      OnDelete="@(() => Handler.DeleteCharacter(character.Id))" />
    }
</div>

// CharacterList.razor.cs (State)
public partial class CharacterList
{
    [Inject] private CharacterListHandler Handler { get; set; }
    private List<Character> Characters { get; set; } = new();
    
    protected override async Task OnInitializedAsync()
    {
        Characters = await Handler.LoadCharactersAsync();
    }
}

// CharacterList.handler.cs (Business logic)
public class CharacterListHandler
{
    private readonly CharacterApiClient _apiClient;
    private readonly ILogger<CharacterListHandler> _logger;
    
    public async Task<List<Character>> LoadCharactersAsync()
    {
        var result = await _apiClient.GetAllAsync();
        if (!result.IsSuccess)
        {
            _logger.LogError("Failed to load characters: {Error}", result.Error);
            return new List<Character>();
        }
        return result.Data;
    }
}
```

## 🔧 Business Modules

### Module Organization
```
Cdm.Business.Common/
├── Services/
│   ├── CharacterService.cs       # Character CRUD
│   ├── CampaignService.cs        # Campaign management
│   ├── SessionService.cs         # Session lifecycle
│   ├── CombatService.cs          # Combat mechanics
│   ├── DiceService.cs            # Server-side rolling
│   └── NotificationService.cs    # User notifications
└── Validators/
    ├── CharacterValidator.cs     # FluentValidation
    ├── CampaignValidator.cs
    └── SessionValidator.cs
```

### Module Security
- **Authorization**: Policies enforced at endpoint level
- **Data Isolation**: Query filters in AppDbContext
- **Validation**: FluentValidation + server-side checks
- **Audit**: Structured logging for sensitive operations

## 📊 Aspire Observability

### Development Dashboard
- **URL**: Auto-opens at `https://localhost:17223` (development only)
- **Features**:
  - Structured logs with business context (UserId, Action, Resource)
  - Distributed traces: API → Database, SignalR connections
  - Service health checks (ApiService, Web, Database)
  - Resource monitoring (CPU, memory, connections)
- **Metrics Namespace**: `Cdm.*`

### Production (Phase 6)
- Dashboard automatically disabled
- OTLP export for external monitoring
- Health endpoints: `/health`, `/alive`
- Log aggregation for debugging

## 🛠️ Essential Coding Conventions

### Configuration Management
```csharp
// Pattern: environment-specific appsettings
appsettings.json                 // Base configuration
appsettings.Development.json     // Development environment
appsettings.Production.json      // Production environment
```

### Aspire Service Defaults
```csharp
// All projects use Cdm.ServiceDefaults
builder.AddServiceDefaults();

// Automatically maps:
app.MapDefaultEndpoints();  // /health, /alive endpoints
```

### Security Patterns (see SECURITE.md)
- **JWT tokens**: 7-day expiry, issued by API, validated by Web
- **BCrypt passwords**: Work factor 12 for hashing
- **Authorization**: 4 custom policies enforced at endpoint level
- **Query filters**: EF Core global filters for data isolation
- **Server-side validation**: FluentValidation + manual checks
- **Anti-cheat**: All dice rolls validated server-side

### Result<T> Pattern
```csharp
// All service methods return Result<T> for consistent error handling
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    
    public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}

// Usage in endpoints
app.MapGet("/api/characters/{id}", async (int id, ICharacterService service) =>
{
    var result = await service.GetByIdAsync(id);
    return result.IsSuccess ? Results.Ok(result.Data) : Results.NotFound(result.Error);
});
```

## 📋 Development Phases (6 phases)

### Current Focus
**Phase 1-3**: Foundation (complete)
- ✅ Database schema with TPH inheritance
- ✅ JWT authentication with BCrypt
- ✅ REST API with 45+ endpoints
- ✅ SignalR hubs (Session, Combat, Notification)
- ✅ Blazor Server frontend with Component-Handler-Service pattern

**Phase 4-5**: In Progress
- � Advanced game mechanics (spells, equipment, trades)
- 🔄 Combat system with initiative tracking
- 🔄 Notification system

**Phase 6**: Future
- ⏳ Docker deployment
- ⏳ Production security hardening
- ⏳ Performance optimization
- ⏳ Monitoring and alerting

## 🔍 Development Watchpoints

### Aspire-Specific
- Dashboard visible in development only
- Automatic service discovery between ApiService and Web
- Health checks: `/health` (detailed), `/alive` (simple)
- Structured logging with business context

### Multi-Type Architecture
- Always validate `GameType` in middleware
- Use TPH inheritance correctly (discriminator on `Characters` table)
- JSON attributes must match game type (Generic/Dnd5e/Skyrim)
- Type-specific validation per game system

### Security
- **Never** bypass authorization policies
- **Always** use `Result<T>` for service methods
- **Always** log sensitive operations (delete, update)
- **Never** expose internal errors to client

### Performance
- Use `@key` directive in Blazor lists
- Implement `ShouldRender()` for expensive components
- SignalR reconnection logic in all hub services
- Query filters add WHERE clauses automatically

This codebase is designed for multi-user tabletop RPG management with real-time collaboration, supporting multiple game systems through polymorphic design.

## 📝 Development Best Practices

### 📐 Coding Standards (.NET 10 + C# 13)
**Based on STANDARDS_CODE.md - Microsoft conventions for Chronique des Mondes**

#### 📏 Spacing and Formatting
- **Indentation**: 4 spaces, no tabs
- **Line length**: 120 characters max (configurable via .editorconfig)
- **Operator spacing**: Spaces around operators (`x + y`)
- **Parenthesis spacing**: No space after `(` or before `)`
- **Comma spacing**: Space after commas, never before
- **Comments**: Single space after `//`
- **Braces**: Allman style (braces on new line)

#### 🏷️ Naming Conventions (C#)
- **Classes**: PascalCase (`CharacterService`, `DndCharacter`)
- **Interfaces**: PascalCase with `I` prefix (`ICharacterService`)
- **Methods**: PascalCase with `Async` suffix (`GetByIdAsync`)
- **Properties**: PascalCase (`CurrentHealth`, `IsActive`)
- **Local variables**: camelCase (`characterId`, `userId`)
- **Parameters**: camelCase (`id`, `request`)
- **Private fields**: camelCase with `_` prefix (`_repository`, `_logger`)
- **Constants**: PascalCase or UPPER_CASE (`MaxHealth` or `MAX_HEALTH`)
- **Enums**: PascalCase type and values (`GameType.Dnd5e`)

#### 📋 File Organization
- **Using directives**: Top of file, System usings first
- **Namespace**: One namespace per file
- **Member order**: Constants → Fields → Constructor → Properties → Public Methods → Private Methods
- **Access modifiers**: Always explicit (public/private/internal)
- **One class per file** (except nested classes/records)

#### 🏗️ Code Structure Example

```csharp
// ✅ Chronique des Mondes style
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Cdm.Data.Common.Models;

namespace Cdm.Business.Common.Services;

/// <summary>
/// Service for managing character operations.
/// </summary>
public class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _repository;
    private readonly ILogger<CharacterService> _logger;
    
    public CharacterService(
        ICharacterRepository repository,
        ILogger<CharacterService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<Result<Character>> GetByIdAsync(int id, int userId)
    {
        if (id <= 0)
        {
            return Result<Character>.Failure("Invalid character ID");
        }
        
        _logger.LogInformation("Retrieving character {CharacterId} for user {UserId}", id, userId);
        
        var character = await _repository.GetByIdAsync(id);
        
        if (character == null)
        {
            return Result<Character>.Failure("Character not found");
        }
        
        if (character.UserId != userId)
        {
            _logger.LogWarning("Unauthorized access attempt to character {CharacterId} by user {UserId}", id, userId);
            return Result<Character>.Failure("Access denied");
        }
        
        return Result<Character>.Success(character);
    }
}
```

### 🏗️ Architecture and Organization
**{ARCHITECTURE_TYPE} patterns with {ARCHITECTURAL_PRINCIPLES}**

- **Separation of concerns**: One component/service = one clear responsibility
- **Configuration management**: {CONFIG_MANAGEMENT_APPROACH}
- **Dependency management**: {DEPENDENCY_INJECTION_PATTERN}
- **Module organization**: {MODULE_ORGANIZATION_PATTERN}
- **Naming conventions**: {NAMESPACE_NAMING_PATTERN}
- **Project structure**:
    - `TanusHub.Configuration.Models` contains only `internal sealed` classes with `init` properties for configuration sections.
    - `TanusHub.Configuration.Abstractions` exposes contracts like `IConfigurationValidator` and is consumed through DI.
    - `TanusHub.Services.Abstractions` gathers business interfaces; concrete implementations live in `TanusHub.Services`.
    - Application mocks live in `TanusHub.MockServices.Core.AppServices` to separate fake services from other `Core` components.

```csharp
// ✅ Typed configuration compliant with repository conventions
namespace TanusHub.Configuration.Models;

using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class ServiceConfiguration
{
    [Required]
    public string HostName { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; init; } = 80;

    public string LocalAddress { get; init; } = string.Empty;

    public TimeSpan HealthCheckTimeout { get; init; } = TimeSpan.FromSeconds(5);
}

// ✅ Contract exposed via TanusHub.Services.Abstractions
namespace TanusHub.Services.Abstractions;

using Microsoft.Extensions.Diagnostics.HealthChecks;

internal interface IServiceHealthChecker
{
    Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken);
}
```

### 📊 Logging and Observability
**Structured logging with ILogger and Aspire Dashboard**

- **Structured logging**: ALWAYS use named parameters
- **Business context**: Include UserId, Action, Resource in every log
- **Log levels**: Debug < Information < Warning < Error < Critical
- **Sensitive data**: NEVER log passwords, tokens, or JWT secrets
- **Aspire integration**: Logs visible in dashboard (development)

```csharp
// ✅ Structured logging examples
_logger.LogInformation("User {UserId} created character {CharacterId}", userId, characterId);
_logger.LogWarning("Failed login attempt for email {Email}", email);
_logger.LogError(ex, "Failed to update character {CharacterId}", characterId);

// ❌ Bad: String interpolation
_logger.LogInformation($"User {userId} created character {characterId}"); // DON'T DO THIS
```

### 🛡️ Security Best Practices
**Multi-layer security for Chronique des Mondes (see SECURITE.md)**

- **JWT secrets**: NEVER hardcode, use user-secrets or environment variables
- **Password hashing**: BCrypt work factor 12 (configured in PasswordService)
- **Authorization**: Use policies at endpoint level (`[Authorize(Policy = "IsCharacterOwner")]`)
- **Data isolation**: Query filters automatically filter by UserId
- **Input validation**: FluentValidation + server-side checks
- **Audit logging**: Log all sensitive operations (delete, update, admin actions)

```csharp
// ✅ Endpoint with authorization policy
app.MapDelete("/api/characters/{id}", 
    [Authorize(Policy = "IsCharacterOwner")] 
    async (int id, ICharacterService service, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    
    var result = await service.DeleteAsync(id, userId);
    
    if (result.IsSuccess)
    {
        _logger.LogWarning("Character {CharacterId} deleted by user {UserId}", id, userId);
        return Results.Ok();
    }
    
    return Results.NotFound(result.Error);
});

// ✅ Blazor XSS protection - Razor automatically encodes
<div>@userContent</div>  <!-- Safe, automatically encoded -->

// ❌ NEVER render raw HTML from user input
<div>@((MarkupString)userContent)</div>  <!-- DANGEROUS -->
```

### ⚡ Performance Best Practices

#### Database Optimization (EF Core + SQL Server)
- **DbContext pooling**: Configured via Aspire
- **Query optimization**: Use `.AsNoTracking()` for read-only queries
- **Eager loading**: Use `.Include()` to avoid N+1 queries
- **Indexes**: Defined in MODELE_DONNEES.md (UserId, GameType, etc.)
- **Query filters**: Automatically add WHERE UserId = @userId

```csharp
// ✅ Optimized query with AsNoTracking
public async Task<List<Character>> GetAllAsync(int userId)
{
    return await _context.Characters
        .AsNoTracking()  // Read-only, no change tracking
        .Where(c => c.UserId == userId)  // Already filtered by query filter
        .OrderBy(c => c.Name)
        .ToListAsync();
}

// ✅ Eager loading to avoid N+1
public async Task<Campaign> GetCampaignWithParticipantsAsync(int id)
{
    return await _context.Campaigns
        .Include(c => c.Participants)
            .ThenInclude(p => p.User)
        .FirstOrDefaultAsync(c => c.Id == id);
}
```

#### Blazor Performance
- **@key directive**: Use in lists to optimize rendering
- **ShouldRender()**: Override to prevent unnecessary re-renders
- **StateHasChanged()**: Call explicitly when needed
- **CSS isolation**: Per-component stylesheets
- **SignalR reconnection**: Automatic with exponential backoff

```csharp
// ✅ Optimized Blazor list with @key
@foreach (var character in Characters)
{
    <CharacterCard @key="character.Id" Character="@character" />
}

// ✅ ShouldRender optimization
protected override bool ShouldRender()
{
    // Only re-render if important state changed
    return _importantStateChanged;
}
```

### 📝 Documentation Standards
**XML documentation for public APIs**

#### Documentation Rules
- **Language**: ALL COMMENTS AND DOCUMENTATION IN ENGLISH
- **Public members**: Require XML documentation
- **`<summary>`**: Brief description (1-2 sentences)
- **`<param>`**: Document all parameters
- **`<returns>`**: Document return value
- **`<exception>`**: Document thrown exceptions

#### C# XML Documentation Examples
```csharp
/// <summary>
/// Retrieves a character by its unique identifier.
/// </summary>
/// <param name="characterId">The unique identifier of the character.</param>
/// <param name="userId">The identifier of the authenticated user.</param>
/// <returns>A Result containing the character if found, or a failure message.</returns>
/// <exception cref="ArgumentException">Thrown when characterId is less than or equal to 0.</exception>
public async Task<Result<Character>> GetByIdAsync(int characterId, int userId)
{
    if (characterId <= 0)
    {
        throw new ArgumentException("Character ID must be greater than 0", nameof(characterId));
    }
    
    // Implementation
}

/// <summary>
/// Generates a JWT token for the specified user.
/// </summary>
/// <param name="userId">The user's unique identifier.</param>
/// <param name="email">The user's email address.</param>
/// <param name="name">The user's display name.</param>
/// <returns>A JWT token string valid for 7 days.</returns>
public string GenerateToken(int userId, string email, string name)
{
    // Implementation
}
```

### 🧪 Testing Standards
**xUnit + FluentAssertions for .NET testing**

- **Pattern**: AAA (Arrange, Act, Assert)
- **Naming**: `MethodName_Scenario_ExpectedBehavior`
- **Async methods**: End with `Async` and return `Task`
- **Assertions**: FluentAssertions for readability

```csharp
// ✅ Unit test example (xUnit)
[Fact]
public async Task GetByIdAsync_WithValidId_ReturnsCharacter()
{
    // Arrange
    var characterId = 1;
    var userId = 10;
    var expectedCharacter = new Character { Id = characterId, UserId = userId, Name = "Test" };
    _repositoryMock.Setup(r => r.GetByIdAsync(characterId))
        .ReturnsAsync(expectedCharacter);
    
    // Act
    var result = await _service.GetByIdAsync(characterId, userId);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.Should().NotBeNull();
    result.Data!.Id.Should().Be(characterId);
}

[Fact]
public async Task GetByIdAsync_WithInvalidId_ReturnsFailure()
{
    // Arrange
    var characterId = 999;
    var userId = 10;
    _repositoryMock.Setup(r => r.GetByIdAsync(characterId))
        .ReturnsAsync((Character?)null);
    
    // Act
    var result = await _service.GetByIdAsync(characterId, userId);
    
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Be("Character not found");
}
```

### 🧰 Code Quality Rules

#### Maintainability
- **Access modifiers**: Always explicit (`public`, `private`, `internal`)
- **One class per file** (except nested classes)
- **Built-in types**: Use `string` instead of `String`
- **Nullable syntax**: Use `int?` instead of `Nullable<int>`
- **Prefer** `string.Empty` over `""`

#### LINQ Best Practices
```csharp
// ✅ Good: Use Any() to check existence
if (characters.Any(c => c.Level > 10))
{
    // Logic
}

// ❌ Bad: Use Count() for existence check
if (characters.Count(c => c.Level > 10) > 0)  // Inefficient
{
    // Logic
}

// ✅ Good: Materialize once
var activeCharacters = characters.Where(c => c.IsActive).ToList();
var count = activeCharacters.Count;
var first = activeCharacters.FirstOrDefault();

// ❌ Bad: Multiple enumerations
var activeCharacters = characters.Where(c => c.IsActive);  // IEnumerable
var count = activeCharacters.Count();  // Enumeration 1
var first = activeCharacters.FirstOrDefault();  // Enumeration 2
```

---

## � Additional Resources

### Technical Documentation
All detailed technical documentation is in `.github/instructions/technique/`:
- **ARCHITECTURE_TECHNIQUE.md** - 6-phase architecture overview
- **MODELE_DONNEES.md** - Complete database schema with SQL DDL
- **API_ENDPOINTS.md** - 45+ REST endpoints with examples
- **SIGNALR_TEMPS_REEL.md** - Real-time hub implementations
- **SECURITE.md** - Multi-layer security architecture
- **FRONTEND_BLAZOR.md** - Blazor patterns and component examples
- **STANDARDS_CODE.md** - Complete coding conventions and best practices

### Quick Links
- **Aspire Dashboard**: https://localhost:17223 (development)
- **Health Endpoints**: `/health` (detailed), `/alive` (simple)
- **EF Core Migrations**: `Cdm.Migrations` project with MigrationsContext
- **User Secrets**: JWT config, Azure Communication Services

---

**Document updated**: October 15, 2025  
**Project**: Chronique des Mondes  
**Version**: 1.0
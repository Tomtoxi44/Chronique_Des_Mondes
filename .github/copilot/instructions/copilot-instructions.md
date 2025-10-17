# Chronique des Mondes - Instructions for AI Agents

## 📋 Project Overview

**Chronique des Mondes** is a platform for creating and managing tabletop RPG campaigns with real-time session management, combat system, and D&D 5e support built with .NET 10 + Aspire + Blazor Server.

## � Documentation References

**IMPORTANT**: Before implementing any feature, always consult these documentation folders:

### � Specifications
- **Location**: `.github/instructions/SPECIFICATION_FONCTIONNELLE.md`
- **Contains**: Business requirements, user stories, functional specifications
- **Use when**: Understanding feature requirements, acceptance criteria, business logic

### 🔧 Technical Documentation
- **Location**: `.github/instructions/technique/`
- **Files**:
  - `ARCHITECTURE_TECHNIQUE.md` - System architecture, project structure
  - `API_ENDPOINTS.md` - REST API contracts, request/response formats
  - `MODELE_DONNEES.md` - Database schema, entity relationships
  - `FRONTEND_BLAZOR.md` - Blazor components, UI guidelines
  - `SIGNALR_TEMPS_REEL.md` - Real-time communication patterns
  - `SECURITE.md` - Security guidelines, authentication, authorization
  - `STANDARDS_CODE.md` - Coding standards, conventions, best practices

### 📋 Backlog
- **Location**: `.github/backlog/`
- **Contains**: All User Stories organized by Epic
- **Use when**: Implementing features, understanding story details, checking acceptance criteria

**Always read the relevant documentation before starting implementation to ensure alignment with project standards and requirements.**

---

## 📋 Project Configuration

### Development Environment

### Project Configuration- **Dashboard**: https://localhost:17223 (Aspire Dashboard)

### 📀 Coding Standards

- **PROJECT_NAME**: Chronique des Mondes  **Language-Specific Conventions for .NET 10 + Blazor Server**

- **TECH_STACK**: .NET 10, Aspire, Blazor Server, Entity Framework Core, SignalR  

- **ARCHITECTURE**: Aspire Distributed Application  #### 📏 Spacing and Formatting

- **DATABASE**: SQL Server (dev), PostgreSQL (prod planned)  ```

- **AUTH**: JWT + BCrypt (MVP)  # Standards by language:

# C#### 📉 Logging and Observability

### 🏗️ Architecture & Commands**Ba### 🛑 Security Best Practices

**Aligned with {SECURITY_FRAMEWORK} guidance for {TECH_STACK}**d on {LOGGING_FRAMEWORK} and {OBS### 📋 Code Documentation

**Solution**: `Cdm.slnx`**{DOCUMENTATION_STANDARD} for {TECH_STACK}**



```bash#### Documentation Rules

# Start development- **Language**: ALL COMMENTS IN ENGLISH

dotnet run --project Cdm/Cdm.AppHost- **Public APIs**: {PUBLIC_API_DOC_REQUIREMENT}

# Dashboard: https://localhost:17223- **Function/Method documentation**: {METHOD_DOC_FORMAT}

# Web: https://localhost:5001- **Parameter documentation**: {PARAMETER_DOC_FORMAT}

# API: https://localhost:5000- **Return values**: {RETURN_VALUE_DOC_FORMAT}

- **Type documentation**: {TYPE_DOC_FORMAT}

# Build & Test- **Formatting rules**: {DOC_FORMATTING_RULES}TACK}**

dotnet build

dotnet test- **Structured logging**: {STRUCTURED_LOGGING_APPROACH}

- **Business context**: Include {CONTEXT_FIELDS} in every log entry

# Migrations- **Log levels**: {LOG_LEVELS_HIERARCHY}

dotnet ef migrations add [Name] --project Cdm/Cdm.Data.Common- **Security**: Never log {SENSITIVE_DATA_TYPES}

dotnet ef database update --project Cdm/Cdm.Data.Common- **Observability tools**: {OBSERVABILITY_TOOLS_USED}es, PascalCase classes, camelCase variables, StyleCop rules

```# JavaScript/TypeScript: 2 spaces, camelCase, semicolons, ESLint/Prettier

# Python: 4 spaces, snake_case, PEP 8 compliance, Black formatter

### 📁 Project Structure# Java: 4 spaces, PascalCase classes, camelCase methods, Google Style

# Go: tabs, PascalCase exported, camelCase private, gofmt

```# Rust: 4 spaces, snake_case, kebab-case packages, rustfmt

Cdm/```

├── Cdm.AppHost/               # Aspire orchestration- **Indentation**: {INDENTATION_STANDARD}

├── Cdm.ApiService/            # REST API- **Line length**: {LINE_LENGTH_LIMIT} characters max

├── Cdm.Web/                   # Blazor Server UI- **Operator spacing**: {OPERATOR_SPACING_RULE}

├── Cdm.Data.Common/           # Data layer + Models- **Comment formatting**: {COMMENT_FORMAT_RULE}

├── Cdm.Business.Common/       # Business services- **Code formatter**: {CODE_FORMATTER_TOOL}*Language-Specific Conventions for {TECH_STACK}**

├── Cdm.Common/                # DTOs + Enums

└── Cdm.Migrations/            # EF migrations#### 📏 Spacing and Formatting

``````

# Standards by language:

---

## 📝 Coding Standards (.NET 10 + StyleCop)

### 📏 Formatting Rules

**Based on StyleCop and .NET Coding Standards**

- **Indentation**: 4 spaces, no tabs (SA1027)
- **Line length**: 120 characters max
- **Braces**: Allman style - braces on new lines (SA1500)
- **Access modifiers**: Always explicit (SA1400)
- **Operator spacing**: Spaces around operators (SA1003)
- **this. prefix**: Mandatory for instance members (SA1101)
- **No single-line statements**: Use braces even for single statements (SA1503)

**Example**:
```csharp
public class UserAccount
{
    private string userId;
    private bool isActive;

    public string DisplayName { get; set; }

    public UserAccount(string userId, string displayName)
    {
        this.userId = userId ?? throw new ArgumentNullException(nameof(userId));
        this.DisplayName = displayName;
        this.isActive = true;
    }

    public void DeactivateAccount()
    {
        this.isActive = false;
    }
}
```

### 🏷️ Naming Conventions

**IMPORTANT**: All naming conventions follow StyleCop and Microsoft C# standards.

- **Classes**: PascalCase (`UserAccount`, `UserService`)
- **Methods**: PascalCase (`RegisterAsync`, `Login`, `UpdateEmail`)
- **Private Fields**: camelCase with `this.` prefix (`this.userId`, `this.emailAddress`, `this.loginAttempts`)
- **Properties**: PascalCase (`DisplayName`, `CreatedAt`, `AccountStatus`)
- **Interfaces**: `IPascalCase` with I prefix (`IUserService`, `IAuthService`)
- **Parameters**: camelCase (`userId`, `password`, `newEmail`)
- **Local Variables**: camelCase (`isPasswordValid`, `maskedLocal`)
- **Constants**: PascalCase (`ConstantValue`)

**Key Rules**:
- Always use `this.` prefix for instance members (StyleCop SA1101)
- Private fields use camelCase WITHOUT underscore prefix when using `this.`
- Use descriptive names in English
- Avoid abbreviations except for well-known terms (DTO, API, etc.)

### 📝 Documentation (MANDATORY - ENGLISH ONLY)

**All XML documentation must be in English and follow StyleCop rules (SA1600-SA1652)**

```csharp
/// <summary>
/// Attempts to authenticate the user with the provided credentials.
/// Increments login attempts on failure.
/// </summary>
/// <param name="password">The password to verify.</param>
/// <returns>True if authentication succeeds; otherwise, false.</returns>
public bool Login(string password)
{
    if (string.IsNullOrWhiteSpace(password))
    {
        return false;
    }

    // Check if account is locked due to too many attempts
    if (this.loginAttempts >= 3)
    {
        this.DeactivateAccount();
        return false;
    }

    // Verify password and handle login logic
    bool isPasswordValid = this.VerifyPassword(password);

    if (isPasswordValid)
    {
        this.ResetLoginAttempts();
        return true;
    }

    this.IncrementLoginAttempts();
    return false;
}
```

**Documentation Rules**:
- `<summary>`: Required for all public members, must be non-empty (SA1604, SA1606)
- `<param>`: Document all parameters (SA1611, SA1614)
- `<returns>`: Required for non-void methods (SA1615, SA1616)
- `<exception>`: Document thrown exceptions when applicable
- Comments must start with capital letter and end with period (SA1628, SA1629)
- Use `this.` prefix when calling instance members (SA1101)### 🔐 Authentication & Security

- `{AUTH_PROVIDER}` : Authentication provider (ex: Azure AD, Auth0, JWT, OAuth2)

### 🏗️ File Organization

**StyleCop-compliant file organization (SA1200-SA1217)**

```csharp
// File header (optional but recommended)
namespace Cdm.Business.Common.Services;

using System;
using System.Threading.Tasks;
using Cdm.Common.DTOs;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides user account management services.
/// This service handles user authentication and account operations.
/// </summary>
public class UserAccountService
{
    // Constants first
    private const int MaxLoginAttempts = 3;
    private const int PasswordMinLength = 8;

    // Private fields
    private readonly ILogger<UserAccountService> logger;
    private readonly string connectionString;

    // Constructor
    public UserAccountService(ILogger<UserAccountService> logger, string connectionString)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // Public properties
    public int TotalAttempts { get; private set; }

    // Public methods
    public async Task<bool> AuthenticateAsync(string username, string password)
    {
        // Implementation
        this.logger.LogInformation("User {Username} authentication attempt", username);
        return await Task.FromResult(true);
    }

    // Private helper methods
    private bool ValidatePassword(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Length >= PasswordMinLength;
    }
}
```

**Organization Rules**:
- Using directives inside namespace (SA1200)
- System usings before others (SA1208)
- Member order: Constants → Fields → Constructors → Properties → Methods (SA1201)
- Access modifiers order: Public → Internal → Protected → Private (SA1202)
- Static members before instance members (SA1204)
- Readonly fields before non-readonly (SA1214)

---

### 📦 DTO Organization

**IMPORTANT**: All DTOs (Data Transfer Objects) must follow these strict organization rules:

#### File Structure
- **One class per file** - Never group multiple DTOs in a single file
- **Separate folders** for request (incoming) vs response (outgoing) DTOs

#### Folder Naming Conventions
```
DTOs/
├── Models/              # Incoming data (API requests, user inputs)
│   ├── RegisterRequest.cs
│   ├── LoginRequest.cs
│   ├── UpdateUserRequest.cs
│   └── CreateCharacterRequest.cs
└── ViewModels/          # Outgoing data (API responses, view data)
    ├── LoginResponse.cs
    ├── UserProfileResponse.cs
    ├── CharacterDetailsResponse.cs
    └── ErrorResponse.cs
```

#### Examples

**RegisterRequest.cs** (in `DTOs/Models/`):
```csharp
namespace Cdm.Business.Abstraction.DTOs.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a user registration request.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's password.
    /// </summary>
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's display nickname.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Nickname { get; set; } = string.Empty;
}
```

**LoginResponse.cs** (in `DTOs/ViewModels/`):
```csharp
namespace Cdm.Business.Abstraction.DTOs.ViewModels;

/// <summary>
/// Represents a successful login response.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets the JWT authentication token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}
```

**Key Rules**:
- Use `Request` suffix for incoming DTOs (Models folder)
- Use `Response` suffix for outgoing DTOs (ViewModels folder)
- Always include XML documentation for each property
- Use Data Annotations for validation on Request DTOs
- Never mix multiple DTOs in one file

---

## 🔐 Security Best Practices

### Password Handling

**Use BCrypt with work factor 12 for all password operations**

```csharp
// Hash password on registration
var hash = BCrypt.Net.BCrypt.HashPassword(password, 12);

// Verify password on login
var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
```

### Input Validation

**Always validate inputs with Data Annotations and server-side checks**

```csharp
public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$")]
    public string Password { get; set; } = string.Empty;
}

// Additional server-side validation in methods
public bool UpdateEmail(string newEmail)
{
    if (string.IsNullOrWhiteSpace(newEmail) || !this.IsValidEmail(newEmail))
    {
        return false;
    }

    this.emailAddress = newEmail;
    return true;
}

private bool IsValidEmail(string email)
{
    return !string.IsNullOrEmpty(email) && email.Contains('@') && email.Contains('.');
}
```

**Validation Rules**:
- Never trust client-side validation alone
- Use Data Annotations for DTOs/requests
- Add custom validation logic in service methods
- Check for null, empty, and whitespace with `string.IsNullOrWhiteSpace()`
- Use `ArgumentNullException` for constructor parameters

---

## 📊 Logging Best Practices

### Structured Logging

**Always use structured logging with named parameters, never string interpolation**

```csharp
// ✅ CORRECT - Structured logging
this.logger.LogInformation("User {UserId} attempted login", userId);
this.logger.LogWarning("Failed login attempt for {Username} - Attempts: {AttemptCount}", username, attemptCount);
this.logger.LogError("Email update failed for user {UserId} - Invalid format: {Email}", userId, email);

// ❌ INCORRECT - String interpolation
this.logger.LogInformation($"User {userId} attempted login");
this.logger.LogWarning($"Failed login attempt for {username}");
```

### Log Levels

**Use appropriate log levels for different scenarios**

- **Debug**: Diagnostic details for development
- **Information**: Normal application flow (login attempts, successful operations)
- **Warning**: Unexpected but handled situations (failed validation, retry attempts)
- **Error**: Operation failures that need attention
- **Critical**: System failures requiring immediate action

**Security Rules**:
- **NEVER log**: Passwords, tokens, API keys, personal sensitive data
- **DO log**: UserIds, action types, resource names, timestamps
- **Mask sensitive data**: Use masked emails or partial data when needed

```csharp
// ✅ CORRECT - Safe logging
public string GetMaskedEmail()
{
    if (string.IsNullOrEmpty(this.emailAddress))
    {
        return string.Empty;
    }

    var parts = this.emailAddress.Split('@');
    if (parts.Length != 2)
    {
        return "***";
    }

    var localPart = parts[0];
    var maskedLocal = localPart.Length > 2 
        ? $"{localPart[0]}***{localPart[^1]}" 
        : "***";

    return $"{maskedLocal}@{parts[1]}";
}

this.logger.LogInformation("Email updated for user {UserId} - Masked: {MaskedEmail}", 
    userId, this.GetMaskedEmail());
```

---

## 💡 Code Quality Best Practices

### Method Design Principles

**Based on examples from the codebase**

1. **Single Responsibility**: Each method should do one thing well
2. **Early Returns**: Validate inputs and return early on failure
3. **Guard Clauses**: Check preconditions at the start of methods
4. **Private Helper Methods**: Extract complex logic into private methods

```csharp
/// <summary>
/// Updates the user's email address.
/// </summary>
/// <param name="newEmail">The new email address.</param>
/// <returns>True if the update succeeds; otherwise, false.</returns>
public bool UpdateEmail(string newEmail)
{
    // Guard clause - early return on invalid input
    if (string.IsNullOrWhiteSpace(newEmail) || !this.IsValidEmail(newEmail))
    {
        return false;
    }

    // Main logic
    this.emailAddress = newEmail;
    return true;
}

// Private helper method for validation logic
private bool IsValidEmail(string email)
{
    return !string.IsNullOrEmpty(email) && email.Contains('@') && email.Contains('.');
}
```

### Property Design

**Use appropriate property patterns**

```csharp
// Auto-properties for simple get/set
public string DisplayName { get; set; }

// Read-only auto-property
public DateTime CreatedAt { get; set; }

// Computed property (expression-bodied)
public string AccountStatus => this.isActive ? "Active" : "Inactive";

// Property with private setter
public int TotalAttempts { get; private set; }
```

### Constructor Best Practices

```csharp
/// <summary>
/// Initializes a new instance of the UserAccount class.
/// </summary>
/// <param name="userId">The unique identifier for the user.</param>
/// <param name="emailAddress">The user's email address.</param>
/// <param name="displayName">The user's display name.</param>
public UserAccount(string userId, string emailAddress, string displayName)
{
    // Validate all required parameters with null checks
    this.userId = userId ?? throw new ArgumentNullException(nameof(userId));
    this.emailAddress = emailAddress ?? throw new ArgumentNullException(nameof(emailAddress));
    this.DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
    
    // Initialize default values
    this.loginAttempts = 0;
    this.isActive = true;
    this.CreatedAt = DateTime.UtcNow;
}
```

**Constructor Rules**:
- Validate all parameters with `ArgumentNullException`
- Use `nameof()` for parameter names in exceptions
- Initialize all fields to valid default states
- Use `this.` prefix for all member assignments

### String Manipulation Best Practices

```csharp
// ✅ Use string.IsNullOrWhiteSpace for validation
if (string.IsNullOrWhiteSpace(password))
{
    return false;
}

// ✅ Use string.IsNullOrEmpty for simple checks
if (string.IsNullOrEmpty(this.emailAddress))
{
    return string.Empty;
}

// ✅ Use string.Empty instead of ""
public string Email { get; set; } = string.Empty;

// ✅ Use modern C# string features
var maskedLocal = localPart.Length > 2 
    ? $"{localPart[0]}***{localPart[^1]}"  // Index from end with ^
    : "***";
```

### Boolean Logic and Control Flow

```csharp
// ✅ Clear boolean variable names
bool isPasswordValid = this.VerifyPassword(password);

// ✅ Use boolean variables for readability
if (isPasswordValid)
{
    this.ResetLoginAttempts();
    return true;
}

// ✅ Explicit comparisons for state checks
if (this.loginAttempts >= 3)
{
    this.DeactivateAccount();
    return false;
}
```

---

## 🚀 API Patterns

- Configuration management: {PROXY_CONFIG_METHOD}

### Minimal API Endpoints

## 🔐 Authentication & Authorization

```csharp

namespace Cdm.ApiService.Endpoints;### {AUTH_PROVIDER} Configuration

```

public static class AuthEndpoints// Authentication roles

{{USER_ROLES}          // Standard user access

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app){ADMIN_ROLES}         // Administrative access

    {{CUSTOM_ROLES}        // Custom role definitions

        var group = app.MapGroup("/api/auth").WithTags("Authentication");```



        group.MapPost("/register", RegisterAsync)### Technology-Specific Configuration

            .WithName("Register")- **Authentication Method**: {AUTH_METHOD}

            .Produces<RegisterResponse>(201)- **Session Management**: {SESSION_CONFIG}

            .Produces<ProblemDetails>(400);- **Role Resolution**: {ROLE_RESOLUTION_METHOD}

    }- **Security Policies**: {SECURITY_POLICIES}



    private static async Task<IResult> RegisterAsync(## 🗄️ Database Configuration

        RegisterRequest request,

        AuthService service)### {DATABASE_TYPE} - Context: `{MAIN_CONTEXT}`

    {```

        try{ENTITY_EXAMPLES}

        {// Example entities based on database type:

            var response = await service.RegisterAsync(request);// SQL: Users, Products, Orders, AuditLogs

            return Results.Created($"/api/users/{response.UserId}", response);// NoSQL: UserDocuments, ProductCatalogs, OrderHistories

        }// Key-Value: CacheEntries, Sessions, Configurations

        catch (InvalidOperationException ex)```

        {

            return Results.BadRequest(new ProblemDetails { Detail = ex.Message });### Database Management Commands

        }```bash

    }# Examples by technology:

}# .NET EF Core: dotnet ef migrations add [Name], dotnet ef database update

```# Node.js Prisma: npx prisma migrate dev, npx prisma generate

# Python Django: python manage.py makemigrations, python manage.py migrate

---# Java Flyway: mvn flyway:migrate, mvn flyway:info

{DATABASE_MIGRATION_COMMANDS}

## 🧪 Testing```



### xUnit + FluentAssertions## 🎨 User Interface ({UI_FRAMEWORK})



```csharp### Component Structure

public sealed class AuthServiceTests```

{{COMPONENT_STRUCTURE}

    [Fact]# Examples by UI framework:

    public async Task RegisterAsync_WithValidData_CreatesUser()# React: src/components/, src/pages/, src/hooks/, src/utils/

    {# Vue.js: src/components/, src/views/, src/composables/, src/stores/

        // Arrange# Angular: src/app/components/, src/app/pages/, src/app/services/

        var service = CreateService();# Blazor: Components/Layout/, Components/Pages/, Components/Shared/

        var request = new RegisterRequest # Flutter: lib/widgets/, lib/screens/, lib/services/, lib/models/

        { ```

            Email = "test@example.com",

            Password = "SecurePass123!"### {UI_FRAMEWORK}-Specific Patterns

        };- **Rendering**: {RENDERING_PATTERN}

        - **State Management**: {STATE_MANAGEMENT}

        // Act- **Authorization**: {UI_AUTHORIZATION_PATTERN}

        var result = await service.RegisterAsync(request);- **Real-time Updates**: {REALTIME_PATTERN}

        

        // Assert## 🔧 Business Modules

        result.Should().NotBeNull();

        result.Email.Should().Be("test@example.com");### {MODULE_STRUCTURE}

    }```

}{BUSINESS_MODULE_1}

```{BUSINESS_MODULE_2}

{BUSINESS_MODULE_3}

---# Examples by domain:

# E-commerce: ProductCatalog, OrderManagement, PaymentProcessing

## 🎯 Git Workflow# CRM: ContactManagement, SalesTracking, ReportGeneration

# Content: UserGenerated, ContentModeration, SearchIndexing

### Branch Strategy# IoT: DeviceManagement, DataCollection, AlertSystem

```

```

main → dev → feature/US-XXX-description### Module Implementation Patterns

```- **Security**: Role-based access control per module

- **Integration**: {MODULE_INTEGRATION_PATTERN}

### Commit Messages- **Configuration**: {MODULE_CONFIG_PATTERN}



```## 📊 Aspire Observability

feat(auth): implement user registration (US-001)

fix(combat): resolve turn calculation bug### Development Dashboard

docs(backlog): add Epic 4 user stories- URL: `https://localhost:17223` (development only).

test(auth): add integration tests- Structured ILogger logs with business context.

```- Business metrics in the `TanusHub.*` namespace.

- Distributed traces: YARP → mock services.

Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`

### Production

---- Dashboard automatically disabled.

- OTLP export via `OTEL_EXPORTER_OTLP_ENDPOINT` variables.

## 🏃 Quick Start- Public health checks with caching.



```bash## 🛠️ Essential Coding Conventions

# Clone

git clone https://github.com/Tomtoxi44/Chronique_Des_Mondes.git### Multi-Environment Configuration

```csharp

# Run// Pattern: environment-specific appsettings

cd Chronique_Des_Mondesappsettings.json                 // Base configuration

dotnet run --project Cdm/Cdm.AppHostappsettings.Development.json     // Development + mock services

appsettings.Server1.json         // server.tanus.eu

# Create feature branchappsettings.Server2.json         // le-pallet.avonde.eu

git checkout dev```

git checkout -b feature/US-XXX-feature-name

```### Custom Service Defaults

```csharp

---// All projects call:

builder.AddServiceDefaults();

## ✅ Checklist

// Automatically maps:

Before committing code, verify:app.MapDefaultEndpoints();  // /health, /alive

app.UseSecurityHeaders();   // Security headers based on environment

- [ ] All comments in ENGLISH```

- [ ] XML documentation on public members

- [ ] Structured logging (no string interpolation)### Security Patterns

- [ ] Server-side validation on all inputs- Mandatory HTTPS with automatic redirections.

- [ ] No sensitive data in logs- Strict CSP tailored for Blazor Server + Azure AD.

- [ ] Tests passing (`dotnet test`)- Automatic security headers (production only).

- [ ] StyleCop rules followed (4 spaces, PascalCase, etc.)- Audit middleware for sensitive actions.

- [ ] BCrypt for passwords (work factor 12)

## 📋 Backlog and Roadmap

---

### Epic-Driven Structure

**Resources**:Nine epics organized across MVP → Interface → Production phases with detailed user stories in `/backlog/`.

- Backlog: `.github/backlog/` (57 User Stories, 6 Epics)

- StyleCop: Microsoft C# conventions### MVP Priority (2–3 weeks)

- Aspire: https://learn.microsoft.com/dotnet/aspire1. Epic 01: Foundation Aspire

2. Epic 02: Mock Services
3. Epic 03: Azure AD Auth
4. Epic 04: YARP Proxy

## 🔍 Development Watchpoints

### Aspire-specific
- Mock services are .NET projects, not containers.
- Dashboard visible in development only.
- Automatic service discovery between services.
- Configuration hot-reload via `IOptionsMonitor`.

### Multi-server
- Same application deployed on two servers with different configuration.
- Modules enabled per server via configuration.
- Automated Let's Encrypt SSL in production.

### Performance
- Local SQLite optimized for personal usage.
- Long sessions (8h) for home usage.
- In-memory metrics with 30-day retention.

This codebase favors simplicity for personal use on a distributed infrastructure with modern .NET Aspire observability built in.

## 📝 Development Best Practices

### 📐 Coding Standards (.editorconfig + StyleCop)
**Based on StyleCop Analyzers and Microsoft C# conventions**

#### 📏 Spacing and Formatting (SA1000–SA1028)
- **Indentation**: 4 spaces, no tabs (SA1027).
- **Line length**: 65 characters max for mobile-friendly documentation.
- **Operator spacing**: Spaces around operators (SA1003).
- **Parenthesis spacing**: No space after `(` or before `)` (SA1008, SA1009).
- **Comma spacing**: Space after commas, never before (SA1001).
- **Comments**: Single space after `//` (SA1005).
- **No multiple spaces**: Avoid consecutive spaces (SA1025).

#### 🏷️ Naming Conventions
```
# Naming by language:
# C#: PascalCase classes, camelCase fields, IPascalCase interfaces
# Java: PascalCase classes, camelCase methods, UPPER_CASE constants
# JavaScript: camelCase variables/functions, PascalCase classes
# Python: snake_case functions/variables, PascalCase classes
# Go: PascalCase exported, camelCase private
# Rust: snake_case functions/variables, PascalCase types
```
- **Classes/Types**: {CLASS_NAMING_CONVENTION}
- **Methods/Functions**: {METHOD_NAMING_CONVENTION}
- **Variables/Fields**: {VARIABLE_NAMING_CONVENTION}
- **Constants**: {CONSTANT_NAMING_CONVENTION}
- **Interface naming**: {INTERFACE_NAMING_CONVENTION}

#### 📋 Organization and Ordering (SA1200–SA1217)
- **Using directives**: At the top of the file, inside the namespace (SA1200).
- **System usings**: Before others (SA1208).
- **Member order**: Constants → Fields → Constructors → Properties → Methods (SA1201).
- **Access modifiers**: Public → Internal → Protected → Private (SA1202).
- **Static before instance** (SA1204).
- **Readonly before non-readonly** (SA1214).

#### 🏗️ Layout and Braces (SA1500–SA1520)
- **Allman style**: Braces on new lines (SA1500).
- **No single-line statements** (SA1501, SA1502).
- **Mandatory braces**: Even for single statements (SA1503).
- **Blank lines**: Single blank line between members (SA1516).
- **No blank lines**: After `{` or before `}` (SA1505, SA1508).

```csharp
// ✅ Style StyleCop + Microsoft
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace TanusHub.Services
{
    /// <summary>
    /// Service for handling user authentication operations.
    /// </summary>
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly ILogger<UserAuthenticationService> logger;
        private readonly ServiceConfiguration configuration;
        
        public UserAuthenticationService(
            ILogger<UserAuthenticationService> logger,
            ServiceConfiguration configuration)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configuration = configuration;
        }
        
        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            }
            
            this.logger.LogInformation("Authenticating user {Username}", username);
            
            // Authentication logic here
            return await this.ProcessAuthenticationAsync(username, password);
        }
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

### 📊 Logging and OpenTelemetry Observability
**Based on Microsoft OpenTelemetry and ILogger recommendations**

- **Structured logging**: Always use named parameters.
- **Business context**: Include UserId, Action, Resource in every log entry.
- **Correct levels**: Debug/Information/Warning/Error/Critical.
- **No sensitive data**: Never log passwords or tokens.
- **OpenTelemetry first-class**: Use native .NET APIs (ILogger, Activity, Meter).

```{TECH_STACK}
// ✅ Structured logging examples by technology:

// C#/.NET
_logger.LogInformation("User {UserName} performed {Action}", userName, action);

// JavaScript/Node.js
logger.info('User performed action', { userName, action, userId });

// Python
logger.info('User %s performed %s', userName, action, extra={'userId': userId});

// Java
log.info("User {} performed {} on resource {}", userName, action, resourceId);

// Go
log.Info("User performed action", "userName", userName, "action", action);

// ❌ Avoid unstructured logs in any language
// logger.info(f"User {userName} did something") // Python bad example
```

### 🛡️ Blazor Server and Azure AD Security
**Aligned with Microsoft Blazor security guidance**

- **Secrets management**: {SECRET_MANAGEMENT_APPROACH}
- **Security headers**: {SECURITY_HEADERS_IMPLEMENTATION}
- **Authentication**: {AUTHENTICATION_METHOD}
- **Authorization**: {AUTHORIZATION_PATTERN}
- **Input validation**: {INPUT_VALIDATION_APPROACH}
- **Session security**: {SESSION_SECURITY_MEASURES}

```csharp
// ✅ Server-side input validation
[Authorize(Policy = "TanusHub-FileManagers")]
public async Task<IActionResult> ProcessFile(FileUploadModel model)
{
    // Mandatory server-side validation
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    // Permission checks
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!await _authService.CanAccessFileAsync(userId, model.Path))
        return Forbid();
    
    // Audit trail for sensitive actions
    _logger.LogWarning("Sensitive action {Action} by user {UserId} on {Resource}",
        nameof(ProcessFile), userId, model.Path);
}

// ✅ XSS protection - never render raw HTML without encoding
@if (!string.IsNullOrEmpty(userContent))
{
    <div>@Html.Raw(Html.Encode(userContent))</div>
}
```

### ⚡ Performance and Optimization
**Based on {PERFORMANCE_FRAMEWORK} best practices for {TECH_STACK}**

#### {DATABASE_TYPE} Optimization
- **Connection management**: {CONNECTION_POOLING_APPROACH}
- **Query optimization**: {QUERY_OPTIMIZATION_TECHNIQUES}
- **Caching strategy**: {CACHING_IMPLEMENTATION}
- **Data access patterns**: {DATA_ACCESS_PATTERNS}
- **Index strategy**: {INDEXING_APPROACH}

```{TECH_STACK}
// ✅ Performance examples by technology:

// .NET Entity Framework
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlite(connectionString), poolSize: 128);

// Node.js with MongoDB
const users = await User.find({ isActive: true })
  .select('id name lastLogin')
  .lean(); // No Mongoose overhead

// Python Django ORM
users = User.objects.filter(is_active=True).only('id', 'name', 'last_login')

// Java JPA with Spring
@Query("SELECT new UserDto(u.id, u.name) FROM User u WHERE u.active = true")
List<UserDto> findActiveUsersSummary();
```

#### Caching and I/O
- **Memory caching**: For frequently accessed data.
- **Response caching**: For static content.
- **Compression**: Automatic Gzip/Brotli.
- **Health checks**: Cache in production.

```csharp
// ✅ Smart caching
public async Task<ServiceStatus> GetServiceStatusAsync(string serviceName)
{
    var cacheKey = $"service-status:{serviceName}";
    
    if (_memoryCache.TryGetValue(cacheKey, out ServiceStatus? cached))
        return cached!;
    
    var status = await _healthChecker.CheckServiceAsync(serviceName);
    
    _memoryCache.Set(cacheKey, status, TimeSpan.FromMinutes(5));
    return status;
}
```

### 🔧 {TECH_STACK} Specific Patterns
**Official {FRAMEWORK_NAME} recommendations**

- **Framework conventions**: {FRAMEWORK_CONVENTIONS}
- **Component organization**: {COMPONENT_ORGANIZATION_PATTERN}
- **Health/Status endpoints**: {HEALTH_ENDPOINTS_PATTERN}
- **Configuration management**: {CONFIG_HOT_RELOAD_PATTERN}
- **Monitoring integration**: {MONITORING_INTEGRATION}

```{TECH_STACK}
// ✅ Framework-specific patterns:

// .NET Aspire
builder.AddServiceDefaults();
app.MapDefaultEndpoints();

// Node.js Express
app.use('/health', healthCheck);
app.use(helmet()); // Security headers

// Python FastAPI
from fastapi import FastAPI
app.add_middleware(CORSMiddleware)

// Java Spring Boot
@EnableConfigurationProperties
@ConditionalOnProperty("feature.enabled")

// React.js
useEffect(() => {
  // Component lifecycle
}, [dependencies]);
```

### 📝 XML Documentation (SA1600–SA1652)
**StyleCop-compliant full documentation**

#### Mandatory Documentation Rules
- **Language**: ALL COMMENTS IN ENGLISH.
- **Public members**: XML documentation required (SA1600).
- **`<summary>` tags**: Mandatory and non-empty (SA1604, SA1606).
- **Parameters**: Document with `<param>` (SA1611, SA1614).
- **Return values**: `<returns>` for non-void (SA1615, SA1616).
- **Generic types**: `<typeparam>` required (SA1618, SA1622).
- **Properties**: `<value>` tag (SA1609, SA1610).
- **Formatting**: Capitalized sentences ending with a period (SA1628, SA1629).

#### Documentation Examples by Language
```{TECH_STACK}
// C# XML Documentation
/// <summary>
/// Provides authentication services for user operations.
/// </summary>
/// <param name="username">The username for authentication.</param>
/// <returns>True if authentication successful.</returns>
Task<bool> AuthenticateAsync(string username);

// JavaScript JSDoc
/**
 * Authenticates a user with provided credentials
 * @param {string} username - The username for authentication
 * @param {string} password - The password for authentication  
 * @returns {Promise<boolean>} True if authentication successful
 * @throws {AuthenticationError} When credentials are invalid
 */
async function authenticate(username, password) {}

// Python Docstrings
def authenticate(username: str, password: str) -> bool:
    """Authenticate user with provided credentials.
    
    Args:
        username: The username for authentication
        password: The password for authentication
        
    Returns:
        True if authentication successful, False otherwise
        
    Raises:
        ValueError: When username or password is empty
    """

// Java Javadoc
/**
 * Authenticates a user with provided credentials.
 * @param username the username for authentication
 * @param password the password for authentication
 * @return true if authentication successful
 * @throws AuthenticationException when credentials are invalid
 */
public boolean authenticate(String username, String password) {}
```

#### File Headers (SA1633–SA1641)
```csharp
// <copyright file="UserAuthenticationService.cs" company="Tanuscorp">
// Copyright (c) Tanuscorp. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.
// </copyright>

using System;
// ...other usings
```

### 🧪 Tests and Validation
**{TESTING_FRAMEWORK} testing approach for {TECH_STACK}**

- **Unit tests**: {UNIT_TESTING_FRAMEWORK} with {TESTING_PATTERN}
- **Integration tests**: {INTEGRATION_TESTING_APPROACH}
- **End-to-end tests**: {E2E_TESTING_FRAMEWORK}
- **Health checks**: {HEALTH_CHECK_IMPLEMENTATION}
- **CI/CD validation**: {CI_CD_VALIDATION_SCRIPTS}
- **Analyzer conventions**:
    - Test classes are `internal sealed` and inherit from utilities (`AIntegrationTestBase`, `IntegrationTestFixture`) when relevant.
    - Async methods end with `Async` and return `Task` or `Task<T>`.
    - FluentAssertions calls are assigned to a discard (`_ =`) to satisfy CA1806.
    - Prefer `static` lambdas in `LoggerFactory.Create` or `Enumerable.Select` whenever there is no capture.
    - Integration tests share the host via `[Collection("Integration Tests")]` to avoid concurrent parallelization.
    - Create test hosts via `TestHostBuilder.CreateTestHostWithServiceDefaults()` to keep configuration aligned with the application.

```{TECH_STACK}
// ✅ Testing examples by technology:

// C# xUnit
[Fact]
public async Task HealthCheck_ReturnsHealthy_WhenServicesUp()
{
    // Arrange, Act, Assert pattern
    var response = await client.GetAsync("/health");
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}

// JavaScript Jest
describe('Authentication Service', () => {
  it('should authenticate valid user', async () => {
    const result = await authService.authenticate('user', 'pass');
    expect(result).toBe(true);
  });
});

// Python pytest
def test_authenticate_valid_user():
    """Test authentication with valid credentials."""
    result = auth_service.authenticate('user', 'password')
    assert result is True

// Java JUnit
@Test
public void testAuthenticateValidUser() {
    Boolean result = authService.authenticate("user", "password");
    assertTrue(result);
}
```

### 📈 Monitoring and Diagnostics
**Azure Monitor and Application Insights integration**

- **Custom metrics**: Use `System.Diagnostics.Metrics.Meter`.
- **Distributed tracing**: `System.Diagnostics.ActivitySource`.
- **Correlation**: Automatic TraceId/SpanId.
- **Alerting**: Thresholds on critical business metrics.

```csharp
// ✅ Business metrics with OpenTelemetry
public class OrderService
{
    private static readonly Meter _meter = new("TanusHub.Orders");
    private static readonly Counter<int> _ordersProcessed = 
        _meter.CreateCounter<int>("orders_processed_total");
    private static readonly Histogram<double> _processingDuration = 
        _meter.CreateHistogram<double>("order_processing_duration_ms");
    
    public async Task ProcessOrderAsync(Order order)
    {
        using var activity = _activitySource.StartActivity();
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await ProcessOrderInternalAsync(order);
            _ordersProcessed.Add(1, new TagList { ["status"] = "success" });
        }
        catch (Exception ex)
        {
            _ordersProcessed.Add(1, new TagList { ["status"] = "error" });
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        finally
        {
            _processingDuration.Record(stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### 🧰 Maintainability and Readability (SA1100–SA1142, SA1400–SA1414)
**StyleCop rules for maintainable code**

#### Code Readability
- **`this.` prefix**: Mandatory for instance members (SA1101).
- **One statement per line** (SA1107).
- **No regions inside members** (SA1123).
- **Built-in types**: Use `string` instead of `String` (SA1121).
- **`string.Empty`**: Instead of `""` (SA1122).
- **Nullable syntax**: `int?` instead of `Nullable<int>` (SA1125).
- **Lambda syntax**: Prefer lambdas over delegates (SA1130).
- **Readable conditions**: Variable on the left (SA1131).

#### Maintainability
- **Access modifiers**: Always explicit (SA1400).
- **Private fields only** (SA1401).
- **One type per file** (SA1402).
- **One namespace per file** (SA1403).
- **`SuppressMessage` justification** required (SA1404).
- **Explicit precedence**: Parentheses in complex expressions (SA1407, SA1408).
- **UTF-8 files with BOM** (SA1412).
- **Trailing commas**: In multi-line initializers (SA1413).

```csharp
// ✅ Code maintenable StyleCop
public sealed class UserService : IUserService
{
    private readonly ILogger<UserService> logger;
    private readonly IUserRepository repository;
    
    public UserService(ILogger<UserService> logger, IUserRepository repository)
    {
        this.logger = logger;
        this.repository = repository;
    }
    
    public async Task<User?> GetUserAsync(int userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("User ID must be positive.", nameof(userId));
        }
        
        this.logger.LogInformation("Retrieving user {UserId}", userId);
        
        var user = await this.repository.GetByIdAsync(userId);
        
        if (user is null)
        {
            this.logger.LogWarning("User {UserId} not found", userId);
            return null;
        }
        
        return user;
    }
    
    // ✅ Explicit precedence in complex expressions
    private static bool IsValidAge(int age, int minAge, int maxAge)
    {
        return (age >= minAge) && (age <= maxAge);
    }
    
    // ✅ Initializer with trailing comma
    private readonly Dictionary<string, string> defaultSettings = new()
    {
        ["timeout"] = "30000",
        ["retries"] = "3",
        ["enableLogging"] = "true", // Trailing comma
    };
}
```

#### LINQ-Specific Rules
- **Clauses on separate lines** (SA1102, SA1103).
- **Consistent indentation** (SA1104, SA1105).

```csharp
// ✅ LINQ StyleCop
var activeUsers = from user in this.repository.Users
                  where user.IsActive
                  where user.LastLoginDate > DateTime.Now.AddDays(-30)
                  orderby user.LastLoginDate descending
                  select new UserDto
                  {
                      Id = user.Id,
                      Name = user.Name,
                      Email = user.Email,
                  };
```

### 🔄 Error Handling and Resilience
**Microsoft resilience patterns**

- **Retry policies**: Exponential backoff with jitter.
- **Circuit breaker**: Protect downstream services.
- **Timeout policies**: Appropriate timeouts per operation type.
- **Fallback values**: Graceful degradation.

```csharp
// ✅ Resilience with Polly
public class ResilientHttpService
{
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    
    public ResilientHttpService()
    {
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + 
                    TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
                onRetry: (outcome, duration, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} after {Duration}ms for {Operation}",
                        retryCount, duration.TotalMilliseconds, context.OperationKey);
                });
    }
}
```

---

## 🚀 How to Use This Template

1. **Replace all variables** in `{}` with values specific to your project
2. **Remove irrelevant sections** that don't apply to your technology stack
3. **Add technology-specific sections** as needed for your project
4. **Customize coding standards** to match your team's preferences
5. **Update examples** to reflect your actual codebase patterns

### Example Configurations

**For a React TypeScript SPA:**
- `{TECH_STACK}` = "React TypeScript"
- `{UI_FRAMEWORK}` = "React with TypeScript"
- `{BUILD_COMMAND}` = "npm run build"
- `{TESTING_FRAMEWORK}` = "Jest + React Testing Library"

**For a Java Spring Boot API:**
- `{TECH_STACK}` = "Java Spring Boot"
- `{ARCHITECTURE_TYPE}` = "REST API with Clean Architecture"  
- `{BUILD_COMMAND}` = "mvn clean install"
- `{TESTING_FRAMEWORK}` = "JUnit 5 + Mockito"

**For a Python FastAPI:**
- `{TECH_STACK}` = "Python FastAPI"
- `{ARCHITECTURE_TYPE}` = "Microservices with Clean Architecture"
- `{BUILD_COMMAND}` = "pip install -r requirements.txt"
- `{TESTING_FRAMEWORK}` = "pytest + httpx"
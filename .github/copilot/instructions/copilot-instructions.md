# Chronique des Mondes - Instructions for AI Agents# Chronique des Mondes - Instructions for AI Agents



## 📋 Project Overview## 📋 Project Configuration

Project configuration for Chronique des Mondes RPG platform

**Chronique des Mondes** is a platform for creating and managing tabletop RPG campaigns with real-time session management, combat system, and D&D 5e support built with .NET 10 + Aspire + Blazor Server.

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

---# C#: 4 spaces, PascalCase classes, camelCase variables

# JavaScript/TypeScript: 2 spaces, camelCase, semicolons

## 📝 Coding Standards (.NET + StyleCop)# Python: 4 spaces, snake_case, PEP 8 compliance

# Java: 4 spaces, PascalCase classes, camelCase methods

### 📏 Formatting# Go: tabs, PascalCase exported, camelCase private

# Rust: 4 spaces, snake_case, kebab-case packages

- **Indentation**: 4 spaces, no tabs```

- **Line length**: 120 characters max- **Indentation**: {INDENTATION_STANDARD}

- **Braces**: Allman style (new lines)- **Line length**: {LINE_LENGTH_LIMIT} characters max

- **Access modifiers**: Always explicit- **Operator spacing**: {OPERATOR_SPACING_RULE}

- **Comment formatting**: {COMMENT_FORMAT_RULE}**Logging**: {LOGGING_FRAMEWORK} with structured logs

### 🏷️ Naming- **Metrics**: Business metrics in `{PROJECT_NAME}.*` namespace

- **Tracing**: {TRACING_IMPLEMENTATION}

- **Classes**: PascalCase (`UserService`)

- **Methods**: PascalCase (`RegisterAsync`)### Production Environment

- **Fields**: `_camelCase` with underscore- **Monitoring**: {PRODUCTION_MONITORING}

- **Properties**: PascalCase (`Email`)- **Log Aggregation**: {LOG_AGGREGATION_SERVICE}

- **Interfaces**: `IPascalCase` with I prefix- **Metrics Export**: {METRICS_EXPORT_METHOD}

- **Health Checks**: {HEALTH_CHECK_ENDPOINTS}ect Information

### 📝 Documentation (MANDATORY - ENGLISH ONLY)- `{PROJECT_NAME}` : Name of your project

- `{PROJECT_DESCRIPTION}` : Brief description of what the project does

```csharp- `{TECH_STACK}` : Main technology stack (ex: .NET, Node.js, Python, Java, React, etc.)

/// <summary>- `{ARCHITECTURE_TYPE}` : Architecture type (ex: Microservices, Monolith, Serverless, SPA, etc.)

/// Registers a new user in the system.

/// </summary>### 🏗️ Architecture & Infrastructure

/// <param name="request">Registration details.</param>- `{SOLUTION_FILE}` : Main solution/project file (ex: TanusHub.sln, package.json, pom.xml)

/// <returns>Registration response with user ID.</returns>- `{BUILD_COMMAND}` : Build command (ex: dotnet build, npm run build, mvn compile)

/// <exception cref="InvalidOperationException">Email already in use.</exception>- `{TEST_COMMAND}` : Test command (ex: dotnet test, npm test, pytest)

public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)- `{RUN_COMMAND}` : Run command (ex: dotnet run, npm start, python main.py)

{- `{MAIN_PORT}` : Main application port

    // Implementation- `{DASHBOARD_URL}` : Development dashboard URL (if applicable)

}

```### 🔐 Authentication & Security

- `{AUTH_PROVIDER}` : Authentication provider (ex: Azure AD, Auth0, JWT, OAuth2)

### 🏗️ File Organization- `{AUTH_TENANT}` : Tenant/domain for authentication

- `{USER_ROLES}` : List of user roles in the system

```csharp- `{ADMIN_ROLES}` : List of admin roles in the system

namespace Cdm.Business.Common.Services;

### 🗄️ Data Storage

using System;- `{DATABASE_TYPE}` : Database type (ex: SQLite, PostgreSQL, MongoDB, MySQL)

using Cdm.Common.DTOs;- `{MAIN_CONTEXT}` : Main database context/connection name

using Microsoft.Extensions.Logging;- `{ENTITY_EXAMPLES}` : Main entities/models in the system



/// <summary>### 🌐 Deployment & Infrastructure

/// Service description.- `{DEPLOYMENT_TARGETS}` : Deployment targets (ex: Azure, AWS, Docker, Kubernetes)

/// </summary>- `{SERVER_ENVIRONMENTS}` : List of environments (ex: Development, Staging, Production)

public class ServiceName- `{PROXY_TYPE}` : Proxy/gateway type if applicable (ex: YARP, nginx, API Gateway)

{

    private const int ConstantValue = 10;### 📁 Project Structure

    - `{SOURCE_FOLDER}` : Source code folder structure

    private readonly ILogger<ServiceName> _logger;- `{COMPONENT_STRUCTURE}` : UI component structure (if applicable)

    - `{MODULE_STRUCTURE}` : Business module organization

    public ServiceName(ILogger<ServiceName> logger)

    {### 🎨 UI Technology (if applicable)

        _logger = logger;- `{UI_FRAMEWORK}` : UI framework (ex: Blazor Server, React, Vue.js, Angular, Flutter)

    }- `{UI_COMPONENT_LIB}` : UI component library used

    - `{STYLING_APPROACH}` : Styling approach (ex: CSS, SCSS, Tailwind, MUI)

    public async Task MethodAsync()

    {## 🎯 Project Vision

        // Implementation

    }**{PROJECT_NAME}** is a {PROJECT_DESCRIPTION} built with {TECH_STACK} that follows a {ARCHITECTURE_TYPE} architecture.

}

```## 🏗️ Architecture Overview



---### Project Structure

```

## 🔐 Security{SOLUTION_FILE}

{SOURCE_FOLDER}

### Password Handling# Example structures by technology:

# .NET: src/{PROJECT_NAME}.Api/, src/{PROJECT_NAME}.Core/, src/{PROJECT_NAME}.Infrastructure/

```csharp# Node.js: src/, lib/, routes/, models/, middleware/

// BCrypt with work factor 12# Python: app/, models/, views/, services/, tests/

var hash = BCrypt.Net.BCrypt.HashPassword(password, 12);# Java: src/main/java/, src/main/resources/, src/test/

var isValid = BCrypt.Net.BCrypt.Verify(password, hash);# React: src/, components/, pages/, hooks/, utils/

``````



### Input Validation### Critical Architectural Patterns



```csharp**{ARCHITECTURE_TYPE} Pattern**: The application follows {ARCHITECTURE_TYPE} principles:

public class RegisterRequest- Development: {DEV_ENVIRONMENT_DESC}

{- Production: {PROD_ENVIRONMENT_DESC}

    [Required(ErrorMessage = "Email is required")]

    [EmailAddress(ErrorMessage = "Invalid email format")]**Service/Component Organization**: Key architectural components:

    [MaxLength(255)]- {SERVICE_PATTERN_1}

    public string Email { get; set; } = string.Empty;- {SERVICE_PATTERN_2} 

- {SERVICE_PATTERN_3}

    [Required]

    [MinLength(8)]**Development Services**: {DEV_SERVICES_DESC}

    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$")]

    public string Password { get; set; } = string.Empty;## 🔑 Essential Development Commands

}

``````bash

# Start development environment

---{RUN_COMMAND}

# Dashboard/UI: {DASHBOARD_URL}

## 📊 Logging

# Build the project

### Structured Logging{BUILD_COMMAND}



```csharp# Run tests

// ✅ Correct{TEST_COMMAND}

_logger.LogInformation("User {UserId} created {Resource}", userId, resource);

# Additional commands based on technology:

// ❌ Incorrect  # .NET: dotnet ef database update, dotnet user-secrets set

_logger.LogInformation($"User {userId} created {resource}");# Node.js: npm install, npm run dev, npm run lint

```# Python: pip install -r requirements.txt, python manage.py migrate

# Java: mvn install, mvn spring-boot:run

### Log Levels# Docker: docker-compose up, docker build

```

- **Debug**: Diagnostic details

- **Information**: Normal flow## 🌐 Deployment Architecture

- **Warning**: Unexpected but handled

- **Error**: Operation failures### {SERVER_ENVIRONMENTS}

- **Critical**: System failures- **{ENVIRONMENT_1}**: {ENVIRONMENT_1_DESC}

- **{ENVIRONMENT_2}**: {ENVIRONMENT_2_DESC}

**Never log**: Passwords, tokens, sensitive personal data- **{ENVIRONMENT_N}**: {ENVIRONMENT_N_DESC}



---### {PROXY_TYPE} Configuration (if applicable)

- Development: {DEV_PROXY_CONFIG}

## 🚀 API Patterns- Production: {PROD_PROXY_CONFIG}

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
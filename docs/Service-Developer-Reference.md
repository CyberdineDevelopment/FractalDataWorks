# Service Developer Reference

Quick reference for all types in a service domain, organized by project layer. Each type includes inheritance chains, generic constraints, and usage guidance.

**See also:** [Service Developer Guide](Service-Developer-Guide.md) for step-by-step tutorials.

---

## Domain.Abstractions Project

### Command Interfaces

#### Base Command Interface
**Required:** Yes (if domain has operations)
**Inheritance:** `ICommand` (from `FractalDataWorks.Services.Abstractions.Commands`)
**Purpose:** Marker interface for all domain commands
**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Abstractions;

/// <summary>
/// Base command interface for all user management operations.
/// </summary>
public interface IUserManagementCommand : ICommand
{
}
```

#### Specific Command Interfaces
**Required:** Yes (one per operation)
**Inheritance:** `I{Domain}Command`
**Purpose:** Define operation contracts with read-only properties
**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Abstractions.Commands;

public interface ICreateUserCommand : IUserManagementCommand
{
    string Username { get; }
    string Email { get; }
    string Password { get; }
    IReadOnlyDictionary<string, object>? Properties { get; }
}
```

---

### Service Interface

**Required:** Yes
**Inheritance:** `IGenericService<TCommand, TConfiguration>`
**Generic Constraints:**
- `TCommand : I{Domain}Command`
- `TConfiguration : I{Domain}Configuration`

**Purpose:** Defines the service contract - all implementations must match signatures
**Note:** All services in a domain MUST have identical public method signatures

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Abstractions.Services;

public interface IUserManagementService : IGenericService<IUserManagementCommand, IUserManagementConfiguration>
{
    // Domain-specific methods (all implementations must have these exact signatures)
    Task<IGenericResult<User>> GetUser(string userId, CancellationToken cancellationToken = default);
    Task<IGenericResult<IEnumerable<User>>> ListUsers(CancellationToken cancellationToken = default);

    // Execute methods inherited from IGenericService
}
```

---

### Configuration Interface

**Required:** No (optional if no configuration needed)
**Inheritance:** `IGenericConfiguration`
**Purpose:** Defines configuration contract for the domain

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Abstractions.Configuration;

public interface IUserManagementConfiguration : IGenericConfiguration
{
    string ConnectionString { get; }
    PasswordPolicyConfiguration PasswordPolicy { get; }
    SessionConfiguration SessionSettings { get; }
}
```

---

### Factory Interface

**Required:** No (optional - can use `GenericServiceFactory<TService, TConfiguration>`)
**Inheritance:** `IServiceFactory<TService, TConfiguration>`
**Generic Constraints:**
- `TService : I{Domain}Service`
- `TConfiguration : I{Domain}Configuration`

**When to use:** Custom factory logic, special initialization requirements
**When to skip:** Simple services can use `GenericServiceFactory`

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Abstractions;

public interface IUserManagementServiceFactory<TService, TConfiguration>
    : IServiceFactory<TService, TConfiguration>
    where TService : IUserManagementService
    where TConfiguration : IUserManagementConfiguration
{
    // Custom factory methods if needed
}
```

---

### Provider Interface

**Required:** Yes
**Inheritance:** `IGenericServiceProvider<TService, TConfiguration>`
**Generic Constraints:**
- `TService : I{Domain}Service`
- `TConfiguration : I{Domain}Configuration`

**Purpose:** Service resolution abstraction

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Abstractions;

public interface IUserManagementServiceProvider
    : IGenericServiceProvider<IUserManagementService, IUserManagementConfiguration>
{
}
```

---

### ServiceType Interface & Base Class

#### ServiceType Interface
**Required:** Yes
**Inheritance:** `IServiceType<TService, TFactory, TConfiguration>`
**Generic Constraints:**
- `TService : I{Domain}Service`
- `TFactory : IServiceFactory<TService, TConfiguration>`
- `TConfiguration : I{Domain}Configuration`

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Abstractions;

public interface IUserManagementType<TService, TConfiguration, TFactory>
    : IServiceType<TService, TFactory, TConfiguration>
    where TService : IUserManagementService
    where TConfiguration : IUserManagementConfiguration
    where TFactory : IServiceFactory<TService, TConfiguration>
{
    // Domain-specific properties
    string UserStoreType { get; }
}
```

#### ServiceType Base Class
**Required:** Yes
**Inheritance:** `ServiceTypeBase<TService, TFactory, TConfiguration>`
**Implements:** `I{Domain}Type<TService, TConfiguration, TFactory>`
**Generic Constraints:**
- `TService : class, I{Domain}Service`
- `TConfiguration : class, I{Domain}Configuration`
- `TFactory : class, IServiceFactory<TService, TConfiguration>`

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Abstractions;

public abstract class UserManagementTypeBase<TService, TConfiguration, TFactory>
    : ServiceTypeBase<TService, TFactory, TConfiguration>,
      IUserManagementType<TService, TConfiguration, TFactory>
    where TService : class, IUserManagementService
    where TConfiguration : class, IUserManagementConfiguration
    where TFactory : class, IServiceFactory<TService, TConfiguration>
{
    public abstract string UserStoreType { get; }

    protected UserManagementTypeBase(
        int id,
        string name,
        string sectionName,
        string displayName,
        string description,
        string? category = null)
        : base(id, name, sectionName, displayName, description, category)
    {
    }
}
```

---

### Messages

**Required:** No (optional)
**Inheritance:** `MessageTemplate<TSeverity>`
**Attributes:** `[MessageCollection]` on base message class
**Purpose:** Structured messages for Results

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Abstractions.Messages;

[MessageCollection("UserManagementMessages", "UserManagementMessages")]
public abstract class UserManagementMessage : MessageTemplate<MessageSeverity>
{
    protected UserManagementMessage(int id, string name, MessageSeverity severity, string message, string? code = null, string? source = null)
        : base(id, name, severity, message, code, source)
    {
    }
}
```

---

### Logging

**Required:** No (optional but recommended)
**Pattern:** Partial static class with `[LoggerMessage]` attributes
**Purpose:** High-performance structured logging

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Abstractions.Logging;

public static partial class UserManagementServiceLog
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "User {Username} created successfully")]
    public static partial void UserCreated(ILogger logger, string username);
}
```

---

### Domain-Specific Interfaces

**Required:** No (optional)
**Purpose:** Domain-specific abstractions (e.g., `IAuthenticationMethod`, `IPasswordHasher`)

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Abstractions;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
```

---

## Domain Project

### ServiceType Collection

**Required:** Yes
**Inheritance:** `ServiceTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory>`
**Attributes:** `[ServiceTypeCollection]`
**Generic Constraints:**
- `TBase : class, IServiceType<TService, TFactory, TConfiguration>`
- `TGeneric : IServiceType<TService, TFactory, TConfiguration>`
- `TService : class, IGenericService`
- `TConfiguration : class, IGenericConfiguration`
- `TFactory : class, IServiceFactory<TService, TConfiguration>`

**Purpose:** Source-generated collection of all service types

**Example:**
```csharp
namespace MyCompany.Services.UserManagement;

[ServiceTypeCollection("IUserManagementType", "UserManagementTypes")]
public static partial class UserManagementTypes
    : ServiceTypeCollectionBase<
        UserManagementTypeBase<IUserManagementService, IUserManagementConfiguration, IServiceFactory<IUserManagementService, IUserManagementConfiguration>>,
        IUserManagementType<IUserManagementService, IUserManagementConfiguration, IServiceFactory<IUserManagementService, IUserManagementConfiguration>>,
        IUserManagementService,
        IUserManagementConfiguration,
        IServiceFactory<IUserManagementService, IUserManagementConfiguration>>
{
    // Source generator provides:
    // - public static IReadOnlyList<IUserManagementType> All { get; }
    // - public static IUserManagementType GetById(int id)
    // - public static IUserManagementType GetByName(string name)
    // - public static void RegisterAll(IServiceCollection services)
}
```

---

### Provider Implementation

**Required:** Yes
**Inheritance:** Can vary (often custom implementation)
**Implements:** `I{Domain}ServiceProvider`
**Purpose:** Service resolution and selection logic

**Example:**
```csharp
namespace MyCompany.Services.UserManagement;

public class UserManagementServiceProvider : IUserManagementServiceProvider
{
    private readonly IServiceProvider _serviceProvider;

    public UserManagementServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IUserManagementService GetService(string? serviceName = null)
    {
        // Resolution logic
    }
}
```

---

### Service Base Class

**Required:** No (optional convenience)
**Inheritance:** `ServiceBase<TCommand, TConfiguration, TService>`
**Generic Constraints:**
- `TCommand : I{Domain}Command`
- `TConfiguration : I{Domain}Configuration`
- `TService : I{Domain}Service`

**When to use:**
- Need domain-specific constructor parameters beyond `ILogger` and `IConfiguration`
- Want to add domain-specific protected helper methods
- Want to enforce domain-wide behavior in the base class

**When to skip:**
- Implementations only need `ILogger` and `IConfiguration` (use `ServiceBase` directly)
- Domain is simple without shared logic

**Example (with domain base):**
```csharp
namespace MyCompany.Services.UserManagement;

public abstract class UserManagementServiceBase<TCommand, TConfiguration, TService>
    : ServiceBase<TCommand, TConfiguration, TService>
    where TCommand : IUserManagementCommand
    where TConfiguration : IUserManagementConfiguration
    where TService : IUserManagementService
{
    protected IPasswordHasher PasswordHasher { get; }

    protected UserManagementServiceBase(
        ILogger<TService> logger,
        TConfiguration configuration,
        IPasswordHasher passwordHasher)
        : base(logger, configuration)
    {
        PasswordHasher = passwordHasher;
    }
}
```

**Example (skip domain base, use ServiceBase directly):**
```csharp
public sealed class SimpleUserService
    : ServiceBase<IUserManagementCommand, IUserManagementConfiguration, SimpleUserService>,
      IUserManagementService
{
    public SimpleUserService(ILogger<SimpleUserService> logger, IUserManagementConfiguration configuration)
        : base(logger, configuration)
    {
    }
}
```

---

### Message Implementations

**Required:** No (optional, only if Messages defined in Abstractions)
**Inheritance:** Domain message base class
**Purpose:** Concrete message instances

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Messages;

public sealed class UserCreatedMessage : UserManagementMessage
{
    public static UserCreatedMessage Instance { get; } = new();

    private UserCreatedMessage()
        : base(1, "UserCreated", MessageSeverity.Information, "User created successfully")
    {
    }
}
```

---

### Logging Implementations

**Required:** No (optional but recommended)
**Pattern:** Partial static class implementing logging signatures from Abstractions
**Attributes:** `[ExcludeFromCodeCoverage]` with justification

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Logging;

/// <ExcludedFromCoverage>Source-generated logging class with no business logic</ExcludedFromCoverage>
[ExcludeFromCodeCoverage(Justification = "Source-generated logging class")]
public static partial class UserManagementServiceLog
{
    // Partial method implementations generated by source generator
}
```

---

## Implementation Project

### Concrete ServiceType

**Required:** Yes
**Inheritance:** `{Domain}TypeBase<TService, TConfiguration, TFactory>`
**Implements:** `IEnumOption<TSelf>`
**Attributes:** `[ServiceTypeOption(typeof({Domain}Types), "ServiceName")]`
**Generic Constraints:**
- `TService : class, I{Domain}Service`
- `TConfiguration : class, I{Domain}Configuration`
- `TFactory : class, I{Domain}ServiceFactory<TService, TConfiguration>`

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Database;

[ServiceTypeOption(typeof(UserManagementTypes), "Database")]
public sealed class DatabaseUserManagementServiceType
    : UserManagementTypeBase<IDatabaseUserManagementService, DatabaseUserManagementConfiguration, IDatabaseUserManagementServiceFactory>,
      IEnumOption<DatabaseUserManagementServiceType>
{
    public static DatabaseUserManagementServiceType Instance { get; } = new();

    private DatabaseUserManagementServiceType()
        : base(
            id: 100,
            name: "Database",
            sectionName: "Services:UserManagement:Database",
            displayName: "Database User Management",
            description: "SQL Server-based user management service",
            category: "UserManagement")
    {
    }

    public override string UserStoreType => "SqlServer";

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IDatabaseUserManagementService, DatabaseUserManagementService>();
        services.AddScoped<IDatabaseUserManagementServiceFactory, DatabaseUserManagementServiceFactory>();
    }

    public override void Configure(IConfiguration configuration)
    {
        // Configuration validation
    }
}
```

---

### Concrete Service

**Required:** Yes
**Inheritance:** `{Domain}ServiceBase<TCommand, TConfiguration, TService>` OR `ServiceBase<TCommand, TConfiguration, TService>`
**Implements:** `I{Domain}Service`
**Generic Constraints:**
- `TCommand : I{Domain}Command`
- `TConfiguration : I{Domain}Configuration`
- `TService : I{Domain}Service`

**Purpose:** Actual service implementation with Execute methods

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Database.Services;

public sealed class DatabaseUserManagementService
    : UserManagementServiceBase<IUserManagementCommand, DatabaseUserManagementConfiguration, DatabaseUserManagementService>,
      IUserManagementService
{
    private readonly UserManagementCommandTranslator _translator;

    public DatabaseUserManagementService(
        ILogger<DatabaseUserManagementService> logger,
        DatabaseUserManagementConfiguration configuration,
        IPasswordHasher passwordHasher,
        UserManagementCommandTranslator translator)
        : base(logger, configuration, passwordHasher)
    {
        _translator = translator;
    }

    public override async Task<IGenericResult> Execute(IUserManagementCommand command, CancellationToken cancellationToken = default)
    {
        return command switch
        {
            ICreateUserCommand createCmd => await _translator.TranslateCreateUser(createCmd, cancellationToken),
            IAuthenticateUserCommand authCmd => await _translator.TranslateAuthenticate(authCmd, cancellationToken),
            _ => GenericResult.Failure($"Unknown command type: {command.GetType().Name}")
        };
    }

    // Implement all domain-specific methods with exact signatures from interface
    public async Task<IGenericResult<User>> GetUser(string userId, CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

---

### Concrete Configuration

**Required:** No (optional if no configuration needed)
**Inheritance:** `ConfigurationBase<TSelf>`
**Implements:** `I{Domain}Configuration`
**Properties:** `{ get; init; }` for immutability

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Database.Configuration;

public class DatabaseUserManagementConfiguration
    : ConfigurationBase<DatabaseUserManagementConfiguration>,
      IUserManagementConfiguration
{
    public string ConnectionString { get; init; } = string.Empty;
    public PasswordPolicyConfiguration PasswordPolicy { get; init; } = new();
    public SessionConfiguration SessionSettings { get; init; } = new();
}
```

---

### Concrete Factory

**Required:** No (optional - can use `GenericServiceFactory<TService, TConfiguration>`)
**Implements:** `I{Domain}ServiceFactory<TService, TConfiguration>`
**Generic Constraints:**
- `TService : I{Domain}Service`
- `TConfiguration : I{Domain}Configuration`

**When to use:** Custom service instantiation logic
**When to skip:** Simple DI resolution (use `GenericServiceFactory`)

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Database;

public class DatabaseUserManagementServiceFactory
    : IDatabaseUserManagementServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseUserManagementServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IDatabaseUserManagementService Create(DatabaseUserManagementConfiguration configuration)
    {
        // Custom creation logic
    }
}
```

---

### Concrete Commands

**Required:** No (optional, only if domain has operations)
**Implements:** `I{Operation}Command` interfaces
**Properties:** `{ get; init; }` for immutability
**Pattern:** Static `Create()` factory methods

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Database.Commands;

public sealed class CreateUserCommand : ICreateUserCommand
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object>? Properties { get; init; }

    public static CreateUserCommand Create(string username, string email, string password, IReadOnlyDictionary<string, object>? properties = null) => new()
    {
        Username = username,
        Email = email,
        Password = password,
        Properties = properties
    };
}
```

---

### Command Translator

**Required:** No (optional but recommended pattern)
**Purpose:** Converts domain commands to technology-specific operations
**Pattern:** Dependency injected into service

**Example:**
```csharp
namespace MyCompany.Services.UserManagement.Database.Translators;

public sealed class UserManagementCommandTranslator
{
    private readonly ILogger<UserManagementCommandTranslator> _logger;
    private readonly DatabaseUserManagementConfiguration _configuration;

    public UserManagementCommandTranslator(
        ILogger<UserManagementCommandTranslator> logger,
        DatabaseUserManagementConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IGenericResult> TranslateCreateUser(ICreateUserCommand command, CancellationToken cancellationToken)
    {
        // Convert domain command to SQL operations
        await using var connection = new SqlConnection(_configuration.ConnectionString);
        // Execute SQL...
        return GenericResult.Success();
    }
}
```

---

## Folder Organization

Projects can use folders for organization without affecting namespaces:

```
MyCompany.Services.UserManagement/
├── ServiceTypes/
│   └── UserManagementTypes.cs              // namespace: MyCompany.Services.UserManagement
├── Providers/
│   └── UserManagementServiceProvider.cs    // namespace: MyCompany.Services.UserManagement
├── Services/
│   └── UserManagementServiceBase.cs        // namespace: MyCompany.Services.UserManagement
├── Messages/
│   └── UserCreatedMessage.cs               // namespace: MyCompany.Services.UserManagement.Messages
└── Logging/
    └── UserManagementLog.cs                // namespace: MyCompany.Services.UserManagement.Logging
```

Folders are purely organizational - each file declares its own namespace explicitly.

---

## Generic Parameter Ordering

**Standard ordering for ServiceTypeBase and related types:**
```
ServiceTypeBase<TService, TFactory, TConfiguration>
IServiceType<TService, TFactory, TConfiguration>
```

**Rationale:** Service type comes first (primary), Factory second (creates service), Configuration last (configures service).

---

## Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Command Interface | `I{Operation}Command` | `ICreateUserCommand` |
| Service Interface | `I{Domain}Service` | `IUserManagementService` |
| Configuration Interface | `I{Domain}Configuration` | `IUserManagementConfiguration` |
| Factory Interface | `I{Domain}ServiceFactory<TService, TConfiguration>` | `IUserManagementServiceFactory<TService, TConfiguration>` |
| Provider Interface | `I{Domain}ServiceProvider` | `IUserManagementServiceProvider` |
| ServiceType Interface | `I{Domain}Type<TService, TConfiguration, TFactory>` | `IUserManagementType<TService, TConfiguration, TFactory>` |
| ServiceType Base | `{Domain}TypeBase<TService, TConfiguration, TFactory>` | `UserManagementTypeBase<TService, TConfiguration, TFactory>` |
| ServiceType Collection | `{Domain}Types` | `UserManagementTypes` |
| Service Base Class | `{Domain}ServiceBase<TCommand, TConfiguration, TService>` | `UserManagementServiceBase<TCommand, TConfiguration, TService>` |
| Concrete ServiceType | `{Implementation}{Domain}ServiceType` | `DatabaseUserManagementServiceType` |
| Concrete Service | `{Implementation}{Domain}Service` | `DatabaseUserManagementService` |
| Concrete Configuration | `{Implementation}Configuration` | `DatabaseUserManagementConfiguration` |
| Concrete Command | `{Operation}Command` | `CreateUserCommand` |
| Translator | `{Domain}CommandTranslator` | `UserManagementCommandTranslator` |

---

## Key Principles

1. **Commands are interfaces** - Concrete classes implement them
2. **All services in a domain have identical public method signatures** - Plus Execute methods
3. **ServiceTypes are singletons** - Static `Instance` property with deterministic GUID IDs
4. **No exceptions for anticipated conditions** - Return `IGenericResult` with `IGenericMessage`
5. **Use source-generated logging** - `[LoggerMessage]` attribute pattern
6. **ImplicitUsings always disabled** - Explicit using statements required
7. **One type per file** - Generic variants of same type allowed together

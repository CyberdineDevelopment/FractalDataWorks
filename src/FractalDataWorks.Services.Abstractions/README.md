# FractalDataWorks.Services.Abstractions

**Domain service contracts for the FractalDataWorks service framework.** This library defines the foundational abstractions for domain-driven service architectures with command-based operations, type-safe configuration, and integrated messaging.

## Table of Contents

1. [Domain Service Architecture](#domain-service-architecture)
2. [Framework Integration](#framework-integration)
3. [Component Rules and Placement](#component-rules-and-placement)
4. [Core Interface Hierarchy](#core-interface-hierarchy)
5. [Command System](#command-system)
6. [Configuration System](#configuration-system)
7. [Factory System](#factory-system)
8. [Provider System](#provider-system)
9. [Message Integration](#message-integration)
10. [Generic Constraints Reference](#generic-constraints-reference)

## Domain Service Architecture

### What is a Domain Service (Service Type)?

A **domain service** (or **service type**) is a logical grouping of related services that handle a specific business area. Unlike individual services, domain services provide:

- **Unified abstraction** over multiple implementations
- **Type-safe service discovery** without knowing specific implementations
- **Domain-specific command hierarchies** that all implementations understand
- **Coordinated configuration and messaging** across the domain

### Domain vs Individual Services

```csharp
// ❌ Individual Service Approach
IUserRegistrationService registrationService;
IUserAuthenticationService authService;
IUserProfileService profileService;

// ✅ Domain Service Approach
IUserManagementService userService; // Can be ANY implementation:
// - DatabaseUserManagementService
// - LdapUserManagementService
// - AzureAdUserManagementService
// - CompositeUserManagementService
```

### Domain Project Structure

Every domain service requires **exactly two projects**:

```
MyCompany.Services.UserManagement.Abstractions/    (netstandard2.0)
MyCompany.Services.UserManagement/                 (net10.0+)
```

#### Why Two Projects?

**Abstractions (`netstandard2.0`)** = **Pure Contracts**
- Maximum compatibility across .NET implementations
- Zero implementation logic
- Consumer dependencies stay minimal
- Stable API surface

**Concrete (target framework)** = **Domain Foundation**
- Base classes for implementations to inherit from
- Source-generated type collections
- Framework-specific optimizations
- Implementation extension points

### Domain Service Provider Pattern

The **DomainServiceProvider** enables the "any implementation" pattern:

```csharp
public interface IUserManagementServiceProvider : IGenericServiceProvider
{
    // Get ANY user management service - don't care which implementation
    Task<IGenericResult<IUserManagementService>> GetService();
    Task<IGenericResult<IUserManagementService>> GetService(string configurationName);
    Task<IGenericResult<TService>> GetService<TService>() where TService : IUserManagementService;
}
```

**Critical Pattern:** Services are resolved by **domain capability**, not specific implementation type.

## Framework Integration

Domain services integrate with three core framework systems to create a cohesive development experience:

### 1. ServiceTypes Collections Project (Source-Generated Discovery)

```csharp
// Auto-generated from domain implementations in solution
public static class UserManagementServices
{
    public static DatabaseUserManagementService Database => new();
    public static LdapUserManagementService Ldap => new();
    public static AzureAdUserManagementService AzureAd => new();

    public static IEnumerable<IUserManagementService> All =>
        new IUserManagementService[] { Database, Ldap, AzureAd };
}
```

**How it Works:**
1. Source generator scans solution for `IUserManagementService` implementations
2. Generates type-safe collections for discovery
3. Enables runtime enumeration and selection
4. Integrates with DI container registration

### 2. Messages System (Structured Communication)

```csharp
// Domain message hierarchy
[MessageCollection("UserManagementMessages")]
public abstract class UserManagementMessage : MessageTemplate<MessageSeverity>, IServiceMessage
{
    protected UserManagementMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, "UserManagement", message, code, null, null) { }
}

// Source-generated factory methods
public static class UserManagementMessages
{
    public static UserCreatedMessage UserCreated(string userId) => new(userId);
    public static ValidationFailedMessage ValidationFailed(string field) => new(field);
}
```

**Integration Points:**
- Messages integrate with `IGenericResult` for structured error handling
- Source-generated factory methods ensure consistent message creation
- Automatic message ID allocation prevents conflicts
- Integrated with logging for structured telemetry

### 3. Logging System (High-Performance Structured Logging)

```csharp
// Source-generated logging methods
[ExcludeFromCodeCoverage]
public static partial class UserManagementServiceLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
        Message = "Creating user {UserId} with email {Email}")]
    public static partial void UserCreationStarted(ILogger logger, string userId, string email);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error,
        Message = "User creation failed for {UserId}: {ErrorMessage}")]
    public static partial void UserCreationFailed(ILogger logger, string userId, string errorMessage, Exception exception);
}
```

**Integration Benefits:**
- Zero-allocation logging with compile-time validation
- Consistent structured data across all domain services
- Automatic event ID management
- Integrated with Messages for correlated telemetry

### System Cohesion

The three systems work together to create a unified development experience:

```csharp
public class UserManagementServiceBase : ServiceBase<UserManagementCommand, UserManagementConfiguration, UserManagementServiceBase>
{
    public override async Task<IGenericResult> Execute(UserManagementCommand command)
    {
        // 1. Logging integration
        UserManagementServiceLog.CommandExecutionStarted(Logger, command.GetType().Name);

        try
        {
            // Execute business logic
            var result = await ProcessCommand(command);

            // 2. Messages integration
            var successMessage = UserManagementMessages.CommandExecuted(command.GetType().Name);
            UserManagementServiceLog.CommandExecutionCompleted(Logger, command.GetType().Name);

            return GenericResult.Success(successMessage);
        }
        catch (Exception ex)
        {
            // 3. Coordinated error handling
            var errorMessage = UserManagementMessages.CommandExecutionFailed(ex.Message);
            UserManagementServiceLog.CommandExecutionFailed(Logger, command.GetType().Name, ex.Message, ex);

            return GenericResult.Failure(errorMessage);
        }
    }
}
```

## Component Rules and Placement

### Strict Placement Rules

#### Abstractions Project (`netstandard2.0`)

| Component | Location | Purpose | Naming Pattern |
|-----------|----------|---------|----------------|
| **Service Interfaces** | `Services/` | Domain contracts | `I{Domain}Service` |
| **Command Base Classes** | `Commands/` | Operation contracts | `{Domain}Command` |
| **Configuration Interfaces** | `Configuration/` | Settings contracts | `I{Domain}Configuration` |
| **Message Base Classes** | `Messages/` | Communication contracts | `{Domain}Message` |
| **Logging Signatures** | `Logging/` | Telemetry contracts | `{Domain}ServiceLog` |

#### Concrete Project (target framework)

| Component | Location | Purpose | Naming Pattern |
|-----------|----------|---------|----------------|
| **Service Base Classes** | `Services/` | Implementation foundations | `{Domain}ServiceBase` |
| **Configuration Classes** | `Configuration/` | Settings implementations | `{Domain}Configuration` |
| **Factory Base Classes** | `Factories/` | Creation patterns | `{Domain}FactoryBase` |
| **ServiceType Registration** | `ServiceTypes/` | Framework integration | `{Domain}ServiceType` |
| **Provider Implementation** | `Registration/` | Service resolution | `{Domain}ServiceProvider` |
| **Registration Options** | `Registration/` | DI configuration | `{Domain}RegistrationOptions` |

### Component Syntax Rules

#### Service Interface Hierarchy

```csharp
// ✅ Correct: Abstractions project
namespace MyCompany.Services.UserManagement.Abstractions.Services;

/// <summary>
/// Defines user management operations for any implementation.
/// </summary>
public interface IUserManagementService : IGenericService<UserManagementCommand, IUserManagementConfiguration>
{
    /// <summary>
    /// Creates a new user account.
    /// </summary>
    Task<IGenericResult<UserCreatedResult>> CreateUser(CreateUserCommand command);

    /// <summary>
    /// Authenticates user credentials.
    /// </summary>
    Task<IGenericResult<AuthenticationResult>> Authenticate(AuthenticateUserCommand command);
}

// ❌ Incorrect: Generic constraints missing
public interface IUserManagementService : IGenericService
// ❌ Incorrect: Wrong namespace
namespace MyCompany.Services.UserManagement.Services;
```

#### Command Hierarchy Rules

```csharp
// ✅ Correct: Abstractions project base command
namespace MyCompany.Services.UserManagement.Abstractions.Commands;

/// <summary>
/// Base class for all user management operations.
/// </summary>
public abstract class UserManagementCommand : ICommand
{
    /// <summary>
    /// Gets or sets the command identifier.
    /// </summary>
    public string CommandId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets when the command was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the user who initiated the command.
    /// </summary>
    public string? InitiatedBy { get; set; }
}

// ✅ Correct: Specific command implementations
/// <summary>
/// Command to create a new user account.
/// </summary>
public class CreateUserCommand : UserManagementCommand
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
}

// ❌ Incorrect: Missing inheritance
public class CreateUserCommand : ICommand
// ❌ Incorrect: Wrong namespace or project
namespace MyCompany.Services.UserManagement.Commands;
```

## Core Interface Hierarchy

### Service Interface Evolution

The framework provides a progressive interface hierarchy that adds capabilities:

```csharp
// Level 1: Basic service identification
public interface IGenericService
{
    string Id { get; }           // Unique instance identifier
    string ServiceType { get; }  // Service type name for logging
    bool IsAvailable { get; }    // Runtime availability status
}

// Level 2: Command execution capability
public interface IGenericService<TCommand> : IGenericService
    where TCommand : ICommand
{
    Task<IGenericResult> Execute(TCommand command);
}

// Level 3: Configuration access and typed execution
public interface IGenericService<TCommand, TConfiguration> : IGenericService<TCommand>
    where TCommand : ICommand
    where TConfiguration : IGenericConfiguration
{
    string Name { get; }                    // Display name from configuration
    TConfiguration Configuration { get; }   // Strongly-typed configuration access

    // Generic execution with typed results
    Task<IGenericResult<T>> Execute<T>(TCommand command);
    Task<IGenericResult<TOut>> Execute<TOut>(TCommand command, CancellationToken cancellationToken);
    Task<IGenericResult> Execute(TCommand command, CancellationToken cancellationToken);
}

// Level 4: Type identification for logging and DI
public interface IGenericService<TCommand, TConfiguration, TService> : IGenericService<TCommand, TConfiguration>
    where TCommand : ICommand
    where TConfiguration : IGenericConfiguration
    where TService : class
{
    // TService provides compile-time type information for:
    // - Generic logging with ILogger<TService>
    // - DI registration with proper lifetimes
    // - Source generator type discovery
    // - Performance profiling and monitoring
}
```

## Command System

### Command Architecture Principles

Commands represent **operations** that can be performed within a domain. They:
- Encapsulate operation data and validation
- Provide a uniform execution model
- Enable cross-cutting concerns (logging, metrics, caching)
- Support command composition and chaining

### Command Hierarchy Design

```csharp
// ✅ Correct: Domain command hierarchy
namespace MyCompany.Services.UserManagement.Abstractions.Commands;

/// <summary>
/// Base class for all user management operations.
/// Provides common audit trail and metadata.
/// </summary>
public abstract class UserManagementCommand : ICommand
{
    /// <summary>
    /// Unique identifier for this command instance.
    /// Used for correlation across logging and monitoring.
    /// </summary>
    public string CommandId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when the command was created.
    /// Used for performance monitoring and audit trails.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Identity of the user who initiated this command.
    /// Used for authorization and audit logging.
    /// </summary>
    public string? InitiatedBy { get; init; }

    /// <summary>
    /// Additional context data for this command.
    /// Used for cross-cutting concerns and extensibility.
    /// </summary>
    public Dictionary<string, object> Context { get; init; } = new();
}

/// <summary>
/// Commands that modify user data.
/// Adds transaction and concurrency control metadata.
/// </summary>
public abstract class UserManagementMutationCommand : UserManagementCommand
{
    /// <summary>
    /// Expected version for optimistic concurrency control.
    /// Prevents lost update scenarios.
    /// </summary>
    public string? ExpectedVersion { get; init; }

    /// <summary>
    /// Whether this command should participate in a transaction.
    /// Enables batch operation support.
    /// </summary>
    public bool RequiresTransaction { get; init; } = true;
}

/// <summary>
/// Commands that only read user data.
/// Optimized for caching and read scaling.
/// </summary>
public abstract class UserManagementQueryCommand : UserManagementCommand
{
    /// <summary>
    /// Whether cached results are acceptable.
    /// Enables performance optimization for read-heavy scenarios.
    /// </summary>
    public bool AllowCachedResults { get; init; } = true;

    /// <summary>
    /// Maximum age of cached results in seconds.
    /// Balances consistency with performance.
    /// </summary>
    public int CacheMaxAgeSeconds { get; init; } = 300;
}
```

## Configuration System

### Configuration Architecture

Configuration in domain services follows a **interface-first, validation-required** pattern:

```csharp
// ✅ Correct: Abstractions project interface
namespace MyCompany.Services.UserManagement.Abstractions.Configuration;

/// <summary>
/// Configuration contract for user management services.
/// Defines all settings required by any implementation.
/// </summary>
public interface IUserManagementConfiguration : IGenericConfiguration
{
    /// <summary>
    /// Primary data store connection string.
    /// Used for user account storage and retrieval.
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    /// Password policy configuration.
    /// Defines complexity requirements and expiration rules.
    /// </summary>
    PasswordPolicy PasswordPolicy { get; }

    /// <summary>
    /// Session management settings.
    /// Controls timeout and concurrent session limits.
    /// </summary>
    SessionConfiguration SessionSettings { get; }
}
```

## Factory System

### Factory Architecture

The factory system provides **type-safe service creation** with **configuration validation**:

```csharp
// ✅ Correct: Use GenericServiceFactory for standard services
namespace MyCompany.Services.UserManagement.ServiceTypes;

public class UserManagementServiceType : ServiceTypeBase<IUserManagementService, UserManagementConfiguration, GenericServiceFactory<IUserManagementService, UserManagementConfiguration>>
{
    public UserManagementServiceType() : base(100, "UserManagement", "UserServices") { }

    public override string SectionName => "Services:UserManagement";
    public override string DisplayName => "User Management Service";
    public override string Description => "Manages user accounts, authentication, and profiles";

    public override void Register(IServiceCollection services)
    {
        // Register the generic factory - no custom factory needed
        services.AddScoped<GenericServiceFactory<IUserManagementService, UserManagementConfiguration>>();

        // Register service implementations
        services.AddTransient<IUserManagementService, DatabaseUserManagementService>();
    }
}
```

## Provider System

### Domain Service Provider Architecture

The **DomainServiceProvider** abstracts service resolution within a domain:

```csharp
// ✅ Correct: Domain service provider interface
namespace MyCompany.Services.UserManagement.Abstractions;

/// <summary>
/// Provides access to user management services without knowing specific implementations.
/// </summary>
public interface IUserManagementServiceProvider : IGenericServiceProvider
{
    /// <summary>
    /// Gets the default user management service.
    /// Implementation selected based on configuration priority.
    /// </summary>
    Task<IGenericResult<IUserManagementService>> GetService();

    /// <summary>
    /// Gets a user management service using a named configuration.
    /// </summary>
    Task<IGenericResult<IUserManagementService>> GetService(string configurationName);

    /// <summary>
    /// Gets a specific user management service implementation.
    /// </summary>
    Task<IGenericResult<TService>> GetService<TService>() where TService : class, IUserManagementService;
}
```

## Message Integration

### Message System Architecture

Messages provide **structured communication** between services and with external systems:

```csharp
// ✅ Correct: Domain message base class
namespace MyCompany.Services.UserManagement.Abstractions.Messages;

/// <summary>
/// Base class for all user management messages.
/// Provides consistent structure and source generation support.
/// </summary>
[MessageCollection("UserManagementMessages")]
public abstract class UserManagementMessage : MessageTemplate<MessageSeverity>, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the UserManagementMessage class.
    /// </summary>
    protected UserManagementMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, "UserManagement", message, code, null, null) { }
}
```

## Generic Constraints Reference

### Service Interface Constraints

```csharp
// ✅ Correct: Progressive constraint addition
public interface IGenericService
// No constraints - basic service contract

public interface IGenericService<TCommand> : IGenericService
    where TCommand : ICommand
// TCommand must implement ICommand - ensures command pattern compliance

public interface IGenericService<TCommand, TConfiguration> : IGenericService<TCommand>
    where TCommand : ICommand
    where TConfiguration : IGenericConfiguration
// TConfiguration must implement IGenericConfiguration - ensures validation and binding support

public interface IGenericService<TCommand, TConfiguration, TService> : IGenericService<TCommand, TConfiguration>
    where TCommand : ICommand
    where TConfiguration : IGenericConfiguration
    where TService : class
// TService must be a reference type - enables logging and DI type identification
```

### Factory Interface Constraints

```csharp
// ✅ Correct: Factory constraint patterns
public interface IServiceFactory<TService> : IServiceFactory
    where TService : class
// TService must be reference type - prevents value type services

public interface IServiceFactory<TService, TConfiguration> : IServiceFactory<TService>
    where TService : class
    where TConfiguration : class, IGenericConfiguration
// TConfiguration must be reference type AND implement IGenericConfiguration
// class constraint required for configuration binding and caching
```

### Constraint Selection Rules

#### Reference Type Constraints (`class`)

**Always required for:**
- Service types (enables DI registration)
- Configuration types (enables binding and caching)
- Factory types (enables reflection and instantiation)

#### Interface Constraints

**ICommand Constraint:**
- Required on command type parameters
- Ensures Execute method compatibility
- Enables command pattern infrastructure

**IGenericService Constraint:**
- Required on service type parameters
- Ensures service contract compliance
- Enables service infrastructure integration

**IGenericConfiguration Constraint:**
- Required on configuration type parameters
- Ensures validation and binding support
- Enables configuration infrastructure integration
# FractalDataWorks Service Development Guide

**Complete guide for creating domain services and implementations in the FractalDataWorks framework.** This covers both domain creation (new business areas) and implementation creation (specific service implementations within existing domains).

**See also:** [Service Developer Reference](Service-Developer-Reference.md) for a complete type reference with inheritance chains and constraints.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Creating a New Domain Service](#creating-a-new-domain-service)
3. [Adding Service Implementations](#adding-service-implementations)
4. [Command System Deep Dive](#command-system-deep-dive)
5. [Framework Integration](#framework-integration)
6. [Testing Strategies](#testing-strategies)
7. [Common Patterns](#common-patterns)
8. [Troubleshooting](#troubleshooting)

## Architecture Overview

### Domain Service vs Implementation

**Domain Service** = Business capability abstraction (UserManagement, OrderProcessing, etc.)
**Implementation** = Specific technology implementation (DatabaseUserService, LdapUserService, etc.)

```csharp
// Domain capability - what can be done
IUserManagementService userService = provider.GetService();

// Could be ANY implementation:
// - DatabaseUserManagementService (SQL Server)
// - LdapUserManagementService (Active Directory)
// - RestUserManagementService (External API)
// - CompositeUserManagementService (Multiple backends)

// Same command syntax regardless of implementation
await userService.Execute(UserManagementCommands.Create(username: "john", email: "john@example.com"));
```

### Core Principles

1. **Commands Unify Domains** - All implementations understand the same command interfaces
2. **TypeCollections Enable Discovery** - Source-generated service lookup and registration
3. **Providers Abstract Selection** - Get services by capability, not implementation type
4. **Execute-Only Interface** - Services only have `Execute(TCommand)` methods
5. **Translator Pattern** - Implementations translate domain commands to technology-specific operations

### Project Structure

Every domain requires **exactly two projects**:

```
MyCompany.Services.UserManagement.Abstractions/  (netstandard2.0)
├── Commands/               # Command interfaces
├── Configuration/          # Configuration contracts
├── Services/              # Service interfaces
├── Messages/              # Message base classes
└── Logging/              # Logging method signatures

MyCompany.Services.UserManagement/              (net10.0)
├── ServiceTypes/          # TypeCollection definitions
├── Registration/          # Provider and registration options
├── Services/             # Service base classes
├── Messages/             # Message implementations
└── Logging/             # Logging implementations
```

## Creating a New Domain Service

### Step 1: Create Domain Projects

```bash
# Create abstractions project (netstandard2.0)
dotnet new classlib -n MyCompany.Services.UserManagement.Abstractions -f netstandard2.0
dotnet sln add MyCompany.Services.UserManagement.Abstractions

# Create concrete project (net10.0)
dotnet new classlib -n MyCompany.Services.UserManagement -f net10.0
dotnet sln add MyCompany.Services.UserManagement
```

### Step 2: Configure Abstractions Project

**MyCompany.Services.UserManagement.Abstractions.csproj:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core Framework Dependencies -->
    <ProjectReference Include="..\FractalDataWorks.Services.Abstractions\FractalDataWorks.Services.Abstractions.csproj" />
    <ProjectReference Include="..\FractalDataWorks.ServiceTypes\FractalDataWorks.ServiceTypes.csproj" />
    <ProjectReference Include="..\FractalDataWorks.EnhancedEnums\FractalDataWorks.EnhancedEnums.csproj" />
    <ProjectReference Include="..\FractalDataWorks.Web.Http.Abstractions\FractalDataWorks.Web.Http.Abstractions.csproj" />

    <!-- Source Generators (Analyzer only) -->
    <ProjectReference Include="..\FractalDataWorks.Messages.SourceGenerators\FractalDataWorks.Messages.SourceGenerators.csproj"
                      OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
    <ProjectReference Include="..\FractalDataWorks.EnhancedEnums.SourceGenerators\FractalDataWorks.EnhancedEnums.SourceGenerators.csproj"
                      OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
    <ProjectReference Include="..\FractalDataWorks.ServiceTypes.SourceGenerators\FractalDataWorks.ServiceTypes.SourceGenerators.csproj"
                      ReferenceOutputAssembly="false" PrivateAssets="all" />

    <!-- Source Generator Base Types -->
    <None Include="..\FractalDataWorks.ServiceTypes.SourceGenerators\bin\$(Configuration)\netstandard2.0\FractalDataWorks.ServiceTypes.SourceGenerators.dll"
          Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false"
          Condition="Exists('..\FractalDataWorks.ServiceTypes.SourceGenerators\bin\$(Configuration)\netstandard2.0\FractalDataWorks.ServiceTypes.SourceGenerators.dll')" />
  </ItemGroup>
</Project>
```

### Step 3: Configure Concrete Project

**MyCompany.Services.UserManagement.csproj:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- Domain Abstractions -->
    <ProjectReference Include="..\MyCompany.Services.UserManagement.Abstractions\MyCompany.Services.UserManagement.Abstractions.csproj" />

    <!-- Core Framework Dependencies -->
    <ProjectReference Include="..\FractalDataWorks.Services.Abstractions\FractalDataWorks.Services.Abstractions.csproj" />
    <ProjectReference Include="..\FractalDataWorks.Services\FractalDataWorks.Services.csproj" />
    <ProjectReference Include="..\FractalDataWorks.ServiceTypes\FractalDataWorks.ServiceTypes.csproj" />
    <ProjectReference Include="..\FractalDataWorks.Configuration.Abstractions\FractalDataWorks.Configuration.Abstractions.csproj" />
    <ProjectReference Include="..\FractalDataWorks.Results\FractalDataWorks.Results.csproj" />

    <!-- Source Generators (Analyzer only) -->
    <ProjectReference Include="..\FractalDataWorks.ServiceTypes.SourceGenerators\FractalDataWorks.ServiceTypes.SourceGenerators.csproj"
                      OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
  </ItemGroup>

  <ItemGroup>
    <!-- Microsoft Dependencies -->
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>
</Project>
```

### Step 4: Create Domain Command Interfaces

**Abstractions/IUserManagementCommand.cs:**

```csharp
using FractalDataWorks.Services.Abstractions.Commands;

namespace MyCompany.Services.UserManagement.Abstractions;

/// <summary>
/// Base command interface for all user management operations.
/// Provides domain unification - all implementations understand these commands.
/// </summary>
public interface IUserManagementCommand : ICommand
{
}
```

**Abstractions/Commands/ICreateUserCommand.cs:**

```csharp
using System.Collections.Generic;

namespace MyCompany.Services.UserManagement.Abstractions.Commands;

/// <summary>
/// Command interface for creating new user accounts.
/// All UserManagement implementations must support this operation.
/// </summary>
public interface ICreateUserCommand : IUserManagementCommand
{
    /// <summary>
    /// Gets the username for the new account.
    /// </summary>
    string Username { get; }

    /// <summary>
    /// Gets the email address for the new account.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Gets the initial password for the account.
    /// </summary>
    string Password { get; }

    /// <summary>
    /// Gets additional user properties for extensibility.
    /// </summary>
    IReadOnlyDictionary<string, object>? Properties { get; }
}
```

**Abstractions/Commands/IAuthenticateUserCommand.cs:**

```csharp
namespace MyCompany.Services.UserManagement.Abstractions.Commands;

/// <summary>
/// Command interface for user authentication operations.
/// </summary>
public interface IAuthenticateUserCommand : IUserManagementCommand
{
    /// <summary>
    /// Gets the username for authentication.
    /// </summary>
    string Username { get; }

    /// <summary>
    /// Gets the password for authentication.
    /// </summary>
    string Password { get; }

    /// <summary>
    /// Gets whether to remember the authentication.
    /// </summary>
    bool RememberMe { get; }
}
```

### Step 5: Create Service Interface

**Abstractions/Services/IUserManagementService.cs:**

```csharp
using FractalDataWorks.Services.Abstractions;
using MyCompany.Services.UserManagement.Abstractions.Configuration;

namespace MyCompany.Services.UserManagement.Abstractions.Services;

/// <summary>
/// Service interface for user management operations.
/// All implementations provide the same command execution interface.
/// </summary>
public interface IUserManagementService : IGenericService<IUserManagementCommand, IUserManagementConfiguration>
{
    // CRITICAL: No domain-specific methods!
    // Everything goes through Execute(TCommand) - this enables implementation abstraction
    // Execute methods are inherited from IGenericService
}
```

### Step 6: Create Configuration Interface

**Abstractions/Configuration/IUserManagementConfiguration.cs:**

```csharp
using FractalDataWorks.Configuration.Abstractions;

namespace MyCompany.Services.UserManagement.Abstractions.Configuration;

/// <summary>
/// Configuration interface for user management services.
/// Defines settings contract that all implementations must support.
/// </summary>
public interface IUserManagementConfiguration : IGenericConfiguration
{
    /// <summary>
    /// Gets the user data store connection string.
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    /// Gets the password policy settings.
    /// </summary>
    PasswordPolicyConfiguration PasswordPolicy { get; }

    /// <summary>
    /// Gets the session management settings.
    /// </summary>
    SessionConfiguration SessionSettings { get; }
}

/// <summary>
/// Password policy configuration.
/// </summary>
public class PasswordPolicyConfiguration
{
    public int MinLength { get; set; } = 8;
    public bool RequireDigit { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = true;
}

/// <summary>
/// Session management configuration.
/// </summary>
public class SessionConfiguration
{
    public int TimeoutMinutes { get; set; } = 30;
    public int MaxConcurrentSessions { get; set; } = 3;
    public bool RequireReauthentication { get; set; } = true;
}
```

### Step 7: Create Service Provider Interface

**Abstractions/IUserManagementProvider.cs:**

```csharp
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;
using MyCompany.Services.UserManagement.Abstractions.Configuration;
using MyCompany.Services.UserManagement.Abstractions.Services;

namespace MyCompany.Services.UserManagement.Abstractions;

/// <summary>
/// Provider interface for user management services.
/// Enables "any implementation" resolution pattern.
/// </summary>
public interface IUserManagementProvider : IGenericServiceProvider
{
    /// <summary>
    /// Gets a user management service using the provided configuration.
    /// Implementation is selected based on configuration type.
    /// </summary>
    Task<IGenericResult<IUserManagementService>> GetService(IUserManagementConfiguration configuration);

    /// <summary>
    /// Gets a user management service by configuration name from appsettings.
    /// </summary>
    Task<IGenericResult<IUserManagementService>> GetService(string configurationName);

    /// <summary>
    /// Gets a specific user management service implementation.
    /// </summary>
    Task<IGenericResult<TService>> GetService<TService>() where TService : class, IUserManagementService;
}
```

### Step 8: Create Message Base Classes

**Abstractions/Messages/UserManagementMessage.cs:**

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

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

### Step 9: Create Logging Signatures

**Abstractions/Logging/UserManagementServiceLog.cs:**

```csharp
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace MyCompany.Services.UserManagement.Abstractions.Logging;

/// <summary>
/// Source-generated logging methods for user management services.
/// Provides high-performance, structured logging with compile-time validation.
/// </summary>
[ExcludeFromCodeCoverage]
public static partial class UserManagementServiceLog
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information,
        Message = "Executing command {CommandType} with ID {CommandId}")]
    public static partial void CommandExecutionStarted(ILogger logger, string commandType, string commandId);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "Command {CommandType} completed successfully in {ElapsedMs}ms")]
    public static partial void CommandExecutionCompleted(ILogger logger, string commandType, long elapsedMs);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Error,
        Message = "Command {CommandType} failed: {ErrorMessage}")]
    public static partial void CommandExecutionFailed(ILogger logger, string commandType, string errorMessage, Exception exception);

    [LoggerMessage(EventId = 2000, Level = LogLevel.Information,
        Message = "Creating user {Username} with email {Email}")]
    public static partial void UserCreationStarted(ILogger logger, string username, string email);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information,
        Message = "User {Username} created successfully with ID {UserId}")]
    public static partial void UserCreated(ILogger logger, string username, string userId);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning,
        Message = "User creation failed for {Username}: {Reason}")]
    public static partial void UserCreationFailed(ILogger logger, string username, string reason);

    [LoggerMessage(EventId = 3000, Level = LogLevel.Information,
        Message = "Authentication attempt for user {Username}")]
    public static partial void AuthenticationAttempt(ILogger logger, string username);

    [LoggerMessage(EventId = 3001, Level = LogLevel.Information,
        Message = "Authentication successful for user {Username}")]
    public static partial void AuthenticationSucceeded(ILogger logger, string username);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Warning,
        Message = "Authentication failed for user {Username}: {Reason}")]
    public static partial void AuthenticationFailed(ILogger logger, string username, string reason);
}
```

### Step 10: Create Concrete Domain Foundation

**ServiceTypes/UserManagementTypes.cs:**

```csharp
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using MyCompany.Services.UserManagement.Abstractions;
using MyCompany.Services.UserManagement.Abstractions.Configuration;
using MyCompany.Services.UserManagement.Abstractions.Services;

namespace MyCompany.Services.UserManagement.ServiceTypes;

/// <summary>
/// Collection of user management service types.
/// Generated by ServiceTypeCollectionGenerator with high-performance lookups.
/// Source generator discovers all IUserManagementService implementations in the solution.
/// </summary>
[ServiceTypeCollection(typeof(UserManagementTypeBase<,,>), typeof(IUserManagementServiceType), typeof(UserManagementTypes))]
public partial class UserManagementTypes : ServiceTypeCollectionBase<UserManagementTypeBase<IUserManagementService, IUserManagementConfiguration, IUserManagementServiceFactory<IUserManagementService, IUserManagementConfiguration>>, IUserManagementServiceType, IUserManagementService, IUserManagementConfiguration, IUserManagementServiceFactory<IUserManagementService, IUserManagementConfiguration>>
{
    // Source generator creates:
    // - static UserManagementTypes Name(string typeName) => lookup by name
    // - static UserManagementTypes All => all registered types
    // - Type ServiceType, ConfigurationType, FactoryType properties
    // - High-performance lookup tables
}
```

**Registration/UserManagementProvider.cs:**

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Results;
using MyCompany.Services.UserManagement.Abstractions;
using MyCompany.Services.UserManagement.Abstractions.Configuration;
using MyCompany.Services.UserManagement.Abstractions.Services;
using MyCompany.Services.UserManagement.ServiceTypes;

namespace MyCompany.Services.UserManagement.Registration;

/// <summary>
/// Implementation of user management provider.
/// Uses UserManagementTypes for service lookup and creation.
/// </summary>
public sealed class UserManagementProvider : IUserManagementProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserManagementProvider> _logger;

    public UserManagementProvider(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<UserManagementProvider> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a user management service using the provided configuration.
    /// Uses UserManagementTypes for dynamic service lookup.
    /// </summary>
    public async Task<IGenericResult<IUserManagementService>> GetService(IUserManagementConfiguration configuration)
    {
        if (configuration == null)
        {
            return GenericResult.Failure<IUserManagementService>("Configuration cannot be null");
        }

        try
        {
            // Look up service type by configuration's UserManagementType property
            var serviceType = UserManagementTypes.Name(configuration.UserManagementType);
            if (serviceType.IsEmpty)
            {
                return GenericResult.Failure<IUserManagementService>(
                    $"Unknown user management type: {configuration.UserManagementType}");
            }

            // Get factory from DI and create service
            var factory = _serviceProvider.GetService(serviceType.FactoryType) as IUserManagementServiceFactory;
            if (factory == null)
            {
                return GenericResult.Failure<IUserManagementService>(
                    $"No factory registered for user management type: {configuration.UserManagementType}");
            }

            return await factory.CreateService(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user management service");
            return GenericResult.Failure<IUserManagementService>(ex.Message);
        }
    }

    /// <summary>
    /// Gets a user management service by configuration name from appsettings.
    /// </summary>
    public async Task<IGenericResult<IUserManagementService>> GetService(string configurationName)
    {
        if (string.IsNullOrEmpty(configurationName))
        {
            return GenericResult.Failure<IUserManagementService>("Configuration name cannot be null or empty");
        }

        try
        {
            // Load configuration section
            var section = _configuration.GetSection($"Services:UserManagement:{configurationName}");
            if (!section.Exists())
            {
                return GenericResult.Failure<IUserManagementService>(
                    $"Configuration section not found: Services:UserManagement:{configurationName}");
            }

            // Get service type from configuration
            var userManagementTypeName = section["UserManagementType"];
            if (string.IsNullOrEmpty(userManagementTypeName))
            {
                return GenericResult.Failure<IUserManagementService>(
                    $"UserManagementType not specified in configuration section: {configurationName}");
            }

            // Look up service type and bind configuration
            var serviceType = UserManagementTypes.Name(userManagementTypeName);
            if (serviceType.IsEmpty)
            {
                return GenericResult.Failure<IUserManagementService>(
                    $"Unknown user management type: {userManagementTypeName}");
            }

            var config = section.Get(serviceType.ConfigurationType) as IUserManagementConfiguration;
            if (config == null)
            {
                return GenericResult.Failure<IUserManagementService>(
                    $"Failed to bind configuration to {serviceType.ConfigurationType?.Name}");
            }

            return await GetService(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user management service by name: {ConfigurationName}", configurationName);
            return GenericResult.Failure<IUserManagementService>(ex.Message);
        }
    }

    /// <summary>
    /// Gets a specific user management service implementation.
    /// </summary>
    public async Task<IGenericResult<TService>> GetService<TService>() where TService : class, IUserManagementService
    {
        try
        {
            var service = _serviceProvider.GetService<TService>();
            if (service == null)
            {
                return GenericResult.Failure<TService>($"Service not registered: {typeof(TService).Name}");
            }

            return GenericResult.Success(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get specific user management service: {ServiceType}", typeof(TService).Name);
            return GenericResult.Failure<TService>(ex.Message);
        }
    }
}
```

## Adding Service Implementations

### Step 1: Create Implementation Project

```bash
# Create implementation project
dotnet new classlib -n MyCompany.Services.UserManagement.Database -f net10.0
dotnet sln add MyCompany.Services.UserManagement.Database
```

### Step 2: Configure Implementation Project

**MyCompany.Services.UserManagement.Database.csproj:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- Domain Dependencies -->
    <ProjectReference Include="..\MyCompany.Services.UserManagement.Abstractions\MyCompany.Services.UserManagement.Abstractions.csproj" />
    <ProjectReference Include="..\MyCompany.Services.UserManagement\MyCompany.Services.UserManagement.csproj" />

    <!-- Framework Dependencies -->
    <ProjectReference Include="..\FractalDataWorks.Services\FractalDataWorks.Services.csproj" />
    <ProjectReference Include="..\FractalDataWorks.ServiceTypes\FractalDataWorks.ServiceTypes.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Database-specific dependencies -->
    <PackageReference Include="Microsoft.Data.SqlClient" />
    <PackageReference Include="Dapper" />
  </ItemGroup>
</Project>
```

### Step 3: Create Command Implementations

**Commands/CreateUserCommand.cs:**

```csharp
using System.Collections.Generic;
using MyCompany.Services.UserManagement.Abstractions;
using MyCompany.Services.UserManagement.Abstractions.Commands;

namespace MyCompany.Services.UserManagement.Database.Commands;

/// <summary>
/// Database implementation of create user command.
/// Implements the domain command interface for SQL Server operations.
/// </summary>
public sealed class CreateUserCommand : ICreateUserCommand
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object>? Properties { get; init; }

    /// <summary>
    /// Creates a command instance using collection expressions.
    /// </summary>
    public static CreateUserCommand Create(string username, string email, string password,
        IReadOnlyDictionary<string, object>? properties = null) => new()
    {
        Username = username,
        Email = email,
        Password = password,
        Properties = properties ?? new Dictionary<string, object>()
    };
}
```

### Step 4: Create Service Implementation

**Services/DatabaseUserManagementService.cs:**

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using MyCompany.Services.UserManagement.Abstractions;
using MyCompany.Services.UserManagement.Abstractions.Commands;
using MyCompany.Services.UserManagement.Abstractions.Configuration;
using MyCompany.Services.UserManagement.Abstractions.Services;
using MyCompany.Services.UserManagement.Abstractions.Logging;
using MyCompany.Services.UserManagement.Database.Translators;

namespace MyCompany.Services.UserManagement.Database.Services;

/// <summary>
/// SQL Server implementation of user management service.
/// Translates domain commands to SQL operations via CommandTranslator.
/// </summary>
public sealed class DatabaseUserManagementService : ServiceBase<IUserManagementCommand, IUserManagementConfiguration, DatabaseUserManagementService>,
    IUserManagementService
{
    private readonly UserManagementCommandTranslator _translator;

    public DatabaseUserManagementService(
        ILogger<DatabaseUserManagementService> logger,
        IUserManagementConfiguration configuration,
        UserManagementCommandTranslator translator)
        : base(logger, configuration)
    {
        _translator = translator ?? throw new ArgumentNullException(nameof(translator));
    }

    /// <summary>
    /// Executes domain commands by translating them to SQL operations.
    /// CRITICAL: This is the ONLY method - no domain-specific methods!
    /// </summary>
    public override async Task<IGenericResult> Execute(IUserManagementCommand command, CancellationToken cancellationToken = default)
    {
        var commandType = command.GetType().Name;
        var commandId = Guid.NewGuid().ToString();

        UserManagementServiceLog.CommandExecutionStarted(Logger, commandType, commandId);

        try
        {
            // TRANSLATOR PATTERN: Convert domain command to SQL operation
            var result = command switch
            {
                ICreateUserCommand createCmd => await _translator.TranslateCreateUser(createCmd, cancellationToken),
                IAuthenticateUserCommand authCmd => await _translator.TranslateAuthenticate(authCmd, cancellationToken),
                _ => GenericResult.Failure($"Unknown command type: {commandType}")
            };

            if (result.IsSuccess)
            {
                UserManagementServiceLog.CommandExecutionCompleted(Logger, commandType, 0); // Add timing
            }
            else
            {
                UserManagementServiceLog.CommandExecutionFailed(Logger, commandType, result.Error, new InvalidOperationException(result.Error));
            }

            return result;
        }
        catch (Exception ex)
        {
            UserManagementServiceLog.CommandExecutionFailed(Logger, commandType, ex.Message, ex);
            return GenericResult.Failure($"Command execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes domain commands with typed results.
    /// </summary>
    public override async Task<IGenericResult<T>> Execute<T>(IUserManagementCommand command, CancellationToken cancellationToken = default)
    {
        var result = await Execute(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return GenericResult<T>.Failure(result.Error);
        }

        if (result.Value is T typedValue)
        {
            return GenericResult<T>.Success(typedValue);
        }

        return GenericResult<T>.Failure($"Result type mismatch. Expected {typeof(T).Name}, got {result.Value?.GetType().Name ?? "null"}");
    }
}
```

### Step 5: Create Command Translator

**Translators/UserManagementCommandTranslator.cs:**

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Dapper;
using FractalDataWorks.Results;
using MyCompany.Services.UserManagement.Abstractions.Commands;
using MyCompany.Services.UserManagement.Abstractions.Configuration;
using MyCompany.Services.UserManagement.Abstractions.Logging;

namespace MyCompany.Services.UserManagement.Database.Translators;

/// <summary>
/// Translates domain commands to SQL Server operations.
/// This is where domain commands become technology-specific.
/// </summary>
public sealed class UserManagementCommandTranslator
{
    private readonly IUserManagementConfiguration _configuration;
    private readonly ILogger<UserManagementCommandTranslator> _logger;

    public UserManagementCommandTranslator(
        IUserManagementConfiguration configuration,
        ILogger<UserManagementCommandTranslator> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Translates domain CreateUser command to SQL INSERT operation.
    /// </summary>
    public async Task<IGenericResult> TranslateCreateUser(ICreateUserCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            UserManagementServiceLog.UserCreationStarted(_logger, command.Username, command.Email);

            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            // Hash password (implementation-specific)
            var passwordHash = HashPassword(command.Password);

            // SQL operation (implementation-specific)
            var sql = """
                INSERT INTO Users (Username, Email, PasswordHash, CreatedAt)
                OUTPUT INSERTED.UserId
                VALUES (@Username, @Email, @PasswordHash, @CreatedAt)
                """;

            var userId = await connection.QuerySingleAsync<int>(sql, new
            {
                command.Username,
                command.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            });

            UserManagementServiceLog.UserCreated(_logger, command.Username, userId.ToString());
            return GenericResult.Success($"User created with ID: {userId}");
        }
        catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
        {
            UserManagementServiceLog.UserCreationFailed(_logger, command.Username, "Username or email already exists");
            return GenericResult.Failure("Username or email already exists");
        }
        catch (Exception ex)
        {
            UserManagementServiceLog.UserCreationFailed(_logger, command.Username, ex.Message);
            return GenericResult.Failure($"Failed to create user: {ex.Message}");
        }
    }

    /// <summary>
    /// Translates domain Authenticate command to SQL SELECT operation.
    /// </summary>
    public async Task<IGenericResult> TranslateAuthenticate(IAuthenticateUserCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            UserManagementServiceLog.AuthenticationAttempt(_logger, command.Username);

            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            // SQL operation (implementation-specific)
            var sql = """
                SELECT UserId, PasswordHash, IsActive
                FROM Users
                WHERE Username = @Username OR Email = @Username
                """;

            var user = await connection.QuerySingleOrDefaultAsync(sql, new { command.Username });

            if (user == null || !user.IsActive)
            {
                UserManagementServiceLog.AuthenticationFailed(_logger, command.Username, "User not found or inactive");
                return GenericResult.Failure("Authentication failed");
            }

            // Verify password (implementation-specific)
            if (!VerifyPassword(command.Password, user.PasswordHash))
            {
                UserManagementServiceLog.AuthenticationFailed(_logger, command.Username, "Invalid password");
                return GenericResult.Failure("Authentication failed");
            }

            UserManagementServiceLog.AuthenticationSucceeded(_logger, command.Username);
            return GenericResult.Success($"Authentication successful for user: {user.UserId}");
        }
        catch (Exception ex)
        {
            UserManagementServiceLog.AuthenticationFailed(_logger, command.Username, ex.Message);
            return GenericResult.Failure($"Authentication error: {ex.Message}");
        }
    }

    private static string HashPassword(string password)
    {
        // Implementation-specific password hashing
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        // Implementation-specific password verification
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
```

### Step 6: Create ServiceType Registration

**ServiceTypes/DatabaseUserManagementServiceType.cs:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.Services;
using MyCompany.Services.UserManagement.Abstractions.Configuration;
using MyCompany.Services.UserManagement.Abstractions.Services;
using MyCompany.Services.UserManagement.Database.Services;
using MyCompany.Services.UserManagement.Database.Translators;

namespace MyCompany.Services.UserManagement.Database.ServiceTypes;

/// <summary>
/// ServiceType registration for database user management implementation.
/// Source generator will discover this and add it to UserManagementTypes collection.
/// </summary>
public sealed class DatabaseUserManagementServiceType : UserManagementTypeBase<IUserManagementService, IUserManagementConfiguration, GenericServiceFactory<IUserManagementService, IUserManagementConfiguration>>
{
    public DatabaseUserManagementServiceType() : base(100, "Database", "UserManagement") { }

    public override string SectionName => "Services:UserManagement:Database";
    public override string DisplayName => "Database User Management Service";
    public override string Description => "SQL Server-based user management with full CRUD operations";

    public override void Register(IServiceCollection services)
    {
        // Register the generic factory
        services.AddScoped<GenericServiceFactory<IUserManagementService, IUserManagementConfiguration>>();

        // Register the specific implementation
        services.AddScoped<IUserManagementService, DatabaseUserManagementService>();

        // Register implementation-specific dependencies
        services.AddScoped<UserManagementCommandTranslator>();
    }
}
```

## Command System Deep Dive

### Command as Domain Unification

Commands are the **critical unification point** that makes domain services work. They define the **shared vocabulary** that all implementations understand:

```csharp
// SAME command syntax across ALL implementations
var createCommand = UserManagementCommands.Create(
    username: "john",
    email: "john@example.com",
    password: "SecurePass123!"
);

// Database implementation translates to SQL INSERT
await databaseService.Execute(createCommand);

// LDAP implementation translates to LDAP Add
await ldapService.Execute(createCommand);

// REST implementation translates to HTTP POST
await restService.Execute(createCommand);

// ALL use the SAME command interface!
```

### Command Interface Design Rules

**Rule 1: Commands are Interfaces**
```csharp
// ✅ Correct - Interface with read-only properties
public interface ICreateUserCommand : IUserManagementCommand
{
    string Username { get; }
    string Email { get; }
    string Password { get; }
}

// ❌ Wrong - Abstract class
public abstract class CreateUserCommand : IUserManagementCommand
```

**Rule 2: Read-Only Properties**
```csharp
// ✅ Correct - Read-only properties
public interface ICreateUserCommand : IUserManagementCommand
{
    string Username { get; }        // No setter
    string Email { get; }           // No setter
    string Password { get; }        // No setter
}

// ❌ Wrong - Mutable properties
public interface ICreateUserCommand : IUserManagementCommand
{
    string Username { get; set; }   // NO!
}
```

**Rule 3: Collection Expressions for Complex Properties**
```csharp
// ✅ Correct - Use collection expressions and read-only types
public interface ICreateUserCommand : IUserManagementCommand
{
    IReadOnlyDictionary<string, object>? Properties { get; }
    string[]? Roles { get; }
}

// Implementation uses collection expressions
public static CreateUserCommand Create(string username, string email, string password) => new()
{
    Username = username,
    Email = email,
    Password = password,
    Properties = [],                    // Collection expression
    Roles = ["User", "Member"]          // Collection expression
};
```

### Command Hierarchies

**Organize Commands by Operation Type:**

```csharp
// Base domain command
public interface IUserManagementCommand : ICommand { }

// Query operations (read-only)
public interface IUserManagementQueryCommand : IUserManagementCommand
{
    bool AllowCachedResults { get; }
    int CacheMaxAgeSeconds { get; }
}

// Mutation operations (write operations)
public interface IUserManagementMutationCommand : IUserManagementCommand
{
    string? ExpectedVersion { get; }     // Optimistic concurrency
    bool RequiresTransaction { get; }    // Transaction participation
}

// Specific operations
public interface IGetUserCommand : IUserManagementQueryCommand
{
    string UserId { get; }
}

public interface ICreateUserCommand : IUserManagementMutationCommand
{
    string Username { get; }
    string Email { get; }
    string Password { get; }
}
```

### Command Implementation Pattern

**Each implementation creates concrete command classes:**

```csharp
// Database implementation
namespace MyCompany.Services.UserManagement.Database.Commands;

public sealed class CreateUserCommand : ICreateUserCommand
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? ExpectedVersion { get; init; }
    public bool RequiresTransaction { get; init; } = true;

    public static CreateUserCommand Create(string username, string email, string password) => new()
    {
        Username = username,
        Email = email,
        Password = password
    };
}
```

### Command Collections (Future Enhancement)

Commands can be organized into TypeCollections for factory pattern access:

```csharp
// Would be source-generated
public static class UserManagementCommands
{
    public static CreateUserCommand Create(string username, string email, string password)
        => CreateUserCommand.Create(username, email, password);

    public static AuthenticateUserCommand Authenticate(string username, string password)
        => AuthenticateUserCommand.Create(username, password);

    public static GetUserCommand Get(string userId)
        => GetUserCommand.Create(userId);
}

// Usage becomes very clean
await service.Execute(UserManagementCommands.Create("john", "john@example.com", "SecurePass123!"));
await service.Execute(UserManagementCommands.Authenticate("john", "SecurePass123!"));
```

## Framework Integration

### ServiceTypes Collections Integration

**Automatic Service Discovery:**
```csharp
// Source generator scans solution for IUserManagementService implementations
// Generates UserManagementTypes static class

// Runtime service lookup
var serviceType = UserManagementTypes.Name("Database");
if (!serviceType.IsEmpty)
{
    // Get factory type: typeof(GenericServiceFactory<IUserManagementService, DatabaseUserConfiguration>)
    var factoryType = serviceType.FactoryType;

    // Get configuration type: typeof(DatabaseUserConfiguration)
    var configurationType = serviceType.ConfigurationType;

    // Get service type: typeof(IUserManagementService)
    var serviceInterfaceType = serviceType.ServiceType;
}

// Enumerate all implementations
foreach (var type in UserManagementTypes.All)
{
    Console.WriteLine($"{type.Name}: {type.Description}");
}
```

**DI Registration Integration:**
```csharp
// Each ServiceType handles its own registration
public override void Register(IServiceCollection services)
{
    // Framework services
    services.AddScoped<GenericServiceFactory<IUserManagementService, IUserManagementConfiguration>>();

    // Implementation-specific services
    services.AddScoped<IUserManagementService, DatabaseUserManagementService>();
    services.AddScoped<UserManagementCommandTranslator>();

    // Implementation-specific dependencies
    services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
    services.AddScoped<IUserRepository, SqlUserRepository>();
}
```

### Messages Integration

**Structured Communication:**
```csharp
// Domain messages (abstractions project)
[MessageCollection("UserManagementMessages")]
public abstract class UserManagementMessage : MessageTemplate<MessageSeverity>, IServiceMessage
{
    protected UserManagementMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, "UserManagement", message, code, null, null) { }
}

// Specific messages (concrete project)
public sealed class UserCreatedMessage : UserManagementMessage
{
    public UserCreatedMessage(string userId, string username)
        : base(1001, "UserCreated", MessageSeverity.Information,
               $"User account created: {username} (ID: {userId})", "USER_CREATED") { }
}

// Source-generated factory methods
public static class UserManagementMessages
{
    public static UserCreatedMessage UserCreated(string userId, string username)
        => new(userId, username);

    public static AuthenticationFailedMessage AuthenticationFailed(string username, string reason)
        => new(username, reason);
}
```

**Results Integration:**
```csharp
// Services return Results with Messages
public async Task<IGenericResult> Execute(ICreateUserCommand command)
{
    try
    {
        var userId = await CreateUserInDatabase(command);

        // Success with structured message
        var message = UserManagementMessages.UserCreated(userId, command.Username);
        return GenericResult.Success(message);
    }
    catch (Exception ex)
    {
        // Failure with structured message
        var message = UserManagementMessages.UserCreationFailed(command.Username, ex.Message);
        return GenericResult.Failure(message);
    }
}
```

### Logging Integration

**High-Performance Structured Logging:**
```csharp
// Source-generated logging methods
[LoggerMessage(EventId = 2000, Level = LogLevel.Information,
    Message = "Creating user {Username} with email {Email}")]
public static partial void UserCreationStarted(ILogger logger, string username, string email);

// Automatic event ID management prevents conflicts
[LoggerMessage(EventId = 2001, Level = LogLevel.Information,
    Message = "User {Username} created successfully with ID {UserId}")]
public static partial void UserCreated(ILogger logger, string username, string userId);
```

**Logging-Message Integration:**
```csharp
public async Task<IGenericResult> TranslateCreateUser(ICreateUserCommand command)
{
    // Log start with structured data
    UserManagementServiceLog.UserCreationStarted(_logger, command.Username, command.Email);

    try
    {
        var userId = await ExecuteSqlInsert(command);

        // Log success AND create message
        UserManagementServiceLog.UserCreated(_logger, command.Username, userId);
        var message = UserManagementMessages.UserCreated(userId, command.Username);

        return GenericResult.Success(message);
    }
    catch (Exception ex)
    {
        // Log failure AND create message
        UserManagementServiceLog.UserCreationFailed(_logger, command.Username, ex.Message);
        var message = UserManagementMessages.UserCreationFailed(command.Username, ex.Message);

        return GenericResult.Failure(message);
    }
}
```

## Testing Strategies

### Domain Contract Testing

**Test command interfaces work across implementations:**

```csharp
public abstract class UserManagementServiceContractTests<TService, TConfiguration>
    where TService : class, IUserManagementService
    where TConfiguration : class, IUserManagementConfiguration
{
    protected abstract TService CreateService(TConfiguration configuration);
    protected abstract TConfiguration CreateValidConfiguration();

    [Fact]
    public async Task Execute_CreateUserCommand_ShouldSucceed()
    {
        // Arrange
        var configuration = CreateValidConfiguration();
        var service = CreateService(configuration);
        var command = CreateUserCommand.Create("testuser", "test@example.com", "SecurePass123!");

        // Act
        var result = await service.Execute(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Message.ShouldContain("User created");
    }

    [Fact]
    public async Task Execute_DuplicateUsername_ShouldFail()
    {
        // Test that ALL implementations handle duplicate usernames consistently
        var configuration = CreateValidConfiguration();
        var service = CreateService(configuration);
        var command1 = CreateUserCommand.Create("duplicate", "first@example.com", "Pass123!");
        var command2 = CreateUserCommand.Create("duplicate", "second@example.com", "Pass456!");

        // Act
        var result1 = await service.Execute(command1);
        var result2 = await service.Execute(command2);

        // Assert
        result1.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeFalse();
        result2.Error.ShouldContain("already exists");
    }
}

// Concrete test classes inherit contract tests
public class DatabaseUserManagementServiceTests : UserManagementServiceContractTests<DatabaseUserManagementService, DatabaseUserConfiguration>
{
    protected override DatabaseUserManagementService CreateService(DatabaseUserConfiguration configuration)
    {
        var logger = new Mock<ILogger<DatabaseUserManagementService>>().Object;
        var translator = new Mock<UserManagementCommandTranslator>().Object;
        return new DatabaseUserManagementService(logger, configuration, translator);
    }

    protected override DatabaseUserConfiguration CreateValidConfiguration()
    {
        return new DatabaseUserConfiguration
        {
            ConnectionString = "Server=localhost;Database=TestDB;Trusted_Connection=true;",
            UserManagementType = "Database"
        };
    }

    // Implementation-specific tests
    [Fact]
    public async Task DatabaseSpecific_SqlInjection_ShouldBeBlocked()
    {
        // Database-specific security tests
    }
}
```

### Command Validation Testing

**Ensure commands are validated consistently:**

```csharp
public class CreateUserCommandValidationTests
{
    [Theory]
    [InlineData("", "Valid email required")]
    [InlineData("ab", "Username too short")]
    [InlineData("toolongusernameexceedslimit", "Username too long")]
    public void Create_InvalidUsername_ShouldFailValidation(string username, string expectedError)
    {
        // Arrange
        var command = CreateUserCommand.Create(username, "valid@example.com", "ValidPass123!");

        // Act
        var validationResult = ValidateCommand(command);

        // Assert
        validationResult.IsValid.ShouldBeFalse();
        validationResult.Errors.ShouldContain(e => e.ErrorMessage.Contains(expectedError));
    }

    [Theory]
    [InlineData("notanemail", "Invalid email format")]
    [InlineData("", "Email required")]
    public void Create_InvalidEmail_ShouldFailValidation(string email, string expectedError)
    {
        // Test email validation
    }

    private ValidationResult ValidateCommand<T>(T command) where T : IUserManagementCommand
    {
        var validator = new CommandValidator<T>();
        return validator.Validate(command);
    }
}
```

### Provider Integration Testing

**Test the provider pattern:**

```csharp
public class UserManagementProviderIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public UserManagementProviderIntegrationTests()
    {
        var services = new ServiceCollection();

        // Register all UserManagement implementations
        foreach (var serviceType in UserManagementTypes.All)
        {
            serviceType.Register(services);
        }

        _serviceProvider = services.BuildServiceProvider();
        _configuration = CreateTestConfiguration();
    }

    [Theory]
    [InlineData("Database")]
    [InlineData("Ldap")]
    [InlineData("Rest")]
    public async Task GetService_ByTypeName_ShouldReturnCorrectImplementation(string typeName)
    {
        // Arrange
        var provider = new UserManagementProvider(_serviceProvider, _configuration, Mock.Of<ILogger<UserManagementProvider>>());

        // Act
        var result = await provider.GetService(typeName);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Verify it's the correct implementation type
        var expectedType = UserManagementTypes.Name(typeName);
        result.Value.ShouldBeOfType(expectedType.ServiceType);
    }

    [Fact]
    public async Task GetService_WithConfiguration_ShouldExecuteCommands()
    {
        // Test end-to-end: Provider → Service → Command execution
        var provider = new UserManagementProvider(_serviceProvider, _configuration, Mock.Of<ILogger<UserManagementProvider>>());

        var serviceResult = await provider.GetService("Database");
        serviceResult.IsSuccess.ShouldBeTrue();

        var service = serviceResult.Value;
        var command = CreateUserCommand.Create("integrationtest", "test@example.com", "Pass123!");

        var commandResult = await service.Execute(command);
        commandResult.IsSuccess.ShouldBeTrue();
    }
}
```

## Common Patterns

### Configuration Binding Pattern

**appsettings.json structure:**
```json
{
  "Services": {
    "UserManagement": {
      "Default": {
        "UserManagementType": "Database",
        "ConnectionString": "Server=localhost;Database=UserDB;Trusted_Connection=true;",
        "PasswordPolicy": {
          "MinLength": 12,
          "RequireDigit": true,
          "RequireUppercase": true,
          "RequireLowercase": true,
          "RequireNonAlphanumeric": true
        },
        "SessionSettings": {
          "TimeoutMinutes": 30,
          "MaxConcurrentSessions": 3
        }
      },
      "Ldap": {
        "UserManagementType": "Ldap",
        "LdapServer": "ldap://dc.company.com",
        "BaseDn": "DC=company,DC=com",
        "ServiceAccount": "CN=ServiceAccount,OU=Services,DC=company,DC=com"
      }
    }
  }
}
```

**Configuration Usage:**
```csharp
// Get service by configuration name
var service = await provider.GetService("Default");    // Uses Database
var ldapService = await provider.GetService("Ldap");   // Uses LDAP

// Configuration is automatically bound based on UserManagementType
var dbConfig = configuration.GetSection("Services:UserManagement:Default").Get<DatabaseUserConfiguration>();
var ldapConfig = configuration.GetSection("Services:UserManagement:Ldap").Get<LdapUserConfiguration>();
```

### Dependency Injection Pattern

**Registration in Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Register all UserManagement service types
foreach (var serviceType in UserManagementTypes.All)
{
    serviceType.Register(builder.Services);
}

// Register the provider
builder.Services.AddScoped<IUserManagementProvider, UserManagementProvider>();

// Register shared dependencies
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

var app = builder.Build();
```

**Usage in Controllers:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserManagementProvider _provider;

    public UsersController(IUserManagementProvider provider)
    {
        _provider = provider;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // Get service (could be any implementation)
        var serviceResult = await _provider.GetService();
        if (!serviceResult.IsSuccess)
        {
            return StatusCode(500, serviceResult.Error);
        }

        // Execute domain command
        var command = CreateUserCommand.Create(request.Username, request.Email, request.Password);
        var result = await serviceResult.Value.Execute(command);

        return result.IsSuccess
            ? Ok(new { Message = result.Message })
            : BadRequest(new { Error = result.Error });
    }
}
```

### Error Handling Pattern

**Structured Error Responses:**
```csharp
public async Task<IGenericResult> Execute(IUserManagementCommand command)
{
    try
    {
        var result = command switch
        {
            ICreateUserCommand createCmd => await ProcessCreateUser(createCmd),
            IAuthenticateUserCommand authCmd => await ProcessAuthenticate(authCmd),
            _ => GenericResult.Failure(UserManagementMessages.UnknownCommand(command.GetType().Name))
        };

        return result;
    }
    catch (ValidationException ex)
    {
        var message = UserManagementMessages.ValidationFailed(ex.Message);
        return GenericResult.Failure(message);
    }
    catch (SecurityException ex)
    {
        var message = UserManagementMessages.SecurityViolation(ex.Message);
        return GenericResult.Failure(message);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Unexpected error executing command: {CommandType}", command.GetType().Name);
        var message = UserManagementMessages.UnexpectedError(command.GetType().Name);
        return GenericResult.Failure(message);
    }
}
```

### Performance Patterns

**Caching Implementation:**
```csharp
public sealed class CachedUserManagementService : IUserManagementService
{
    private readonly IUserManagementService _innerService;
    private readonly IMemoryCache _cache;

    public async Task<IGenericResult> Execute(IUserManagementCommand command)
    {
        // Cache read operations
        if (command is IUserManagementQueryCommand queryCmd && queryCmd.AllowCachedResults)
        {
            var cacheKey = GenerateCacheKey(command);
            if (_cache.TryGetValue(cacheKey, out IGenericResult cachedResult))
            {
                return cachedResult;
            }

            var result = await _innerService.Execute(command);
            if (result.IsSuccess)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromSeconds(queryCmd.CacheMaxAgeSeconds));
            }
            return result;
        }

        // Invalidate cache for mutations
        if (command is IUserManagementMutationCommand)
        {
            var result = await _innerService.Execute(command);
            if (result.IsSuccess)
            {
                InvalidateRelatedCacheEntries(command);
            }
            return result;
        }

        return await _innerService.Execute(command);
    }
}
```

**Decorator Registration:**
```csharp
public override void Register(IServiceCollection services)
{
    // Register base service
    services.AddScoped<DatabaseUserManagementService>();

    // Register decorated service
    services.AddScoped<IUserManagementService>(provider =>
    {
        var baseService = provider.GetRequiredService<DatabaseUserManagementService>();
        var cache = provider.GetRequiredService<IMemoryCache>();
        return new CachedUserManagementService(baseService, cache);
    });
}
```

## Troubleshooting

### Common Issues

**Issue: Service Not Found**
```
Error: "Unknown user management type: Database"
```

**Solution:** Verify ServiceType registration
```csharp
// Check that ServiceType is registered
foreach (var type in UserManagementTypes.All)
{
    Console.WriteLine($"Registered: {type.Name}");
}

// Verify ServiceType registration
public class DatabaseUserManagementServiceType : UserManagementTypeBase<...>
{
    public DatabaseUserManagementServiceType() : base(100, "Database", "UserManagement") { }
    //                                                      ^^^^^^^^
    //                                                      This name must match configuration
}
```

**Issue: Command Not Recognized**
```
Error: "Unknown command type: CreateUserCommand"
```

**Solution:** Verify command inheritance and switch statement
```csharp
// Ensure command implements domain interface
public sealed class CreateUserCommand : ICreateUserCommand // Must implement interface

// Ensure service handles command type
public override async Task<IGenericResult> Execute(IUserManagementCommand command)
{
    return command switch
    {
        ICreateUserCommand createCmd => await ProcessCreate(createCmd),  // Must match interface
        IAuthenticateUserCommand authCmd => await ProcessAuth(authCmd),
        _ => GenericResult.Failure($"Unknown command type: {command.GetType().Name}")
    };
}
```

**Issue: Configuration Binding Fails**
```
Error: "Failed to bind configuration to DatabaseUserConfiguration"
```

**Solution:** Verify appsettings.json structure and property names
```json
{
  "Services": {
    "UserManagement": {
      "Default": {
        "UserManagementType": "Database",    // Must match ServiceType name
        "ConnectionString": "...",           // Must match property name exactly
        "PasswordPolicy": {                  // Nested objects must match
          "MinLength": 12                    // Property names case-sensitive
        }
      }
    }
  }
}
```

**Issue: Source Generator Not Running**
```
Error: "UserManagementTypes does not contain a definition for 'Name'"
```

**Solution:** Verify source generator references
```xml
<!-- Ensure analyzer reference -->
<ProjectReference Include="..\FractalDataWorks.ServiceTypes.SourceGenerators\FractalDataWorks.ServiceTypes.SourceGenerators.csproj"
                  OutputItemType="Analyzer" ReferenceOutputAssembly="false" />

<!-- Ensure TypeCollection attribute -->
[ServiceTypeCollection(typeof(UserManagementTypeBase<,,>), typeof(IUserManagementServiceType), typeof(UserManagementTypes))]
public partial class UserManagementTypes : ServiceTypeCollectionBase<...>
```

### Debugging Tips

**Enable Source Generator Diagnostics:**
```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

**Verify Service Registration:**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register services
    foreach (var serviceType in UserManagementTypes.All)
    {
        Console.WriteLine($"Registering: {serviceType.Name}");
        serviceType.Register(services);
    }

    // Verify registration
    var provider = services.BuildServiceProvider();
    var factory = provider.GetService<IUserManagementServiceFactory>();
    Console.WriteLine($"Factory registered: {factory != null}");
}
```

**Test Configuration Binding:**
```csharp
[Fact]
public void Configuration_ShouldBindCorrectly()
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

    var section = configuration.GetSection("Services:UserManagement:Default");
    var config = section.Get<DatabaseUserConfiguration>();

    config.ShouldNotBeNull();
    config.UserManagementType.ShouldBe("Database");
    config.ConnectionString.ShouldNotBeEmpty();
}
```

### Modern C# Patterns Reference

**Collection Expressions:**
```csharp
// ✅ Use collection expressions
string[] roles = ["User", "Admin"];
List<string> permissions = ["Read", "Write"];
Dictionary<string, object> properties = [];

// ❌ Don't use old syntax
string[] roles = new string[] { "User", "Admin" };
List<string> permissions = new List<string> { "Read", "Write" };
Dictionary<string, object> properties = new Dictionary<string, object>();
```

**No Async Suffix:**
```csharp
// ✅ Correct - Task return type makes it obvious
public async Task<IGenericResult> Execute(ICommand command)
public async Task<IGenericResult<T>> Execute<T>(ICommand command)

// ❌ Wrong - Redundant Async suffix
public async Task<IGenericResult> ExecuteAsync(ICommand command)
```

**Init-Only Properties:**
```csharp
// ✅ Use init for immutable commands
public sealed class CreateUserCommand : ICreateUserCommand
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

// ❌ Don't use set for command properties
public string Username { get; set; } = string.Empty;
```

This comprehensive guide covers the complete domain service development workflow, from creating new domains to adding implementations, with emphasis on the command system as the domain unification mechanism and proper framework integration patterns.
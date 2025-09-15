# FractalDataWorks.Services.SecretManagement.Abstractions

Comprehensive abstractions and interfaces for secure secret management operations in the FractalDataWorks platform, providing provider-agnostic secret storage and retrieval capabilities.

## Overview

This project defines the core abstractions for the FractalDataWorks secret management system, enabling secure storage and retrieval of sensitive data across multiple secret providers (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault, etc.). It uses a command-based pattern with proper security practices including secure disposal and access patterns.

## Architecture

The secret management abstractions follow both the framework's **minimal interface pattern** and comprehensive security requirements:

- **Core Interface**: `ISecretService` extends `IFractalService<ISecretCommand>` 
- **Command Contract**: `ISecretCommand` defines the secret command structure with validation
- **Configuration Base**: `ISecretManagementConfiguration` provides configuration contract
- **Security-First**: Secure value containers with proper disposal patterns
- **Command Pattern**: Provider-agnostic operations with validation and batch support

## Key Components

### Core Interfaces

#### `ISecretManager`
- **Base Interface**: `IFractalService`
- **Purpose**: Main interface for secret management operations using command-based pattern
- **Methods**:
  - `Execute()` - Execute untyped secret commands
  - `Execute<TResult>()` - Execute typed secret commands
  - `ExecuteBatch()` - Execute multiple commands as batch
  - `ValidateCommand()` - Pre-flight command validation
  - `HealthCheckAsync()` - Provider health verification

```csharp
public interface ISecretManager : IFractalService
{
    Task<IFdwResult<object?>> Execute(ISecretCommand command, CancellationToken cancellationToken = default);
    Task<IFdwResult<TResult>> Execute<TResult>(ISecretCommand<TResult> command, CancellationToken cancellationToken = default);
    Task<IFdwResult<ISecretBatchResult>> ExecuteBatch(IReadOnlyList<ISecretCommand> commands, CancellationToken cancellationToken = default);
    IFdwResult ValidateCommand(ISecretCommand command);
    Task<IFdwResult<ISecretManagerHealth>> HealthCheckAsync(CancellationToken cancellationToken = default);
}
```

#### `ISecretService<TSecretCommand>`
- **Base Interfaces**: `ISecretService`, `IFractalService<TSecretCommand>`
- **Purpose**: Interface for provider-specific secret service implementations
- **Generic Constraint**: `where TSecretCommand : ISecretCommand`
- **Features**: Handles service-specific authentication, API calls, and error handling

```csharp
public interface ISecretService : IFractalService
{
    // Inherits all service functionality from IFractalService<T>
    // No additional methods - follows minimal interface design
}

public interface ISecretService<TSecretCommand> : ISecretService, IFractalService<TSecretCommand>
    where TSecretCommand : ISecretCommand
{
}
```

#### `ISecretCommand` and `ISecretCommand<TResult>`
- **Purpose**: Command interfaces for secret operations with validation
- **Properties**: CommandId, CommandType, Container, SecretKey, Parameters, Metadata
- **Methods**: `Validate()`, `WithParameters()`, `WithMetadata()`
- **Features**: Extensible command pattern with fluent API

### Secure Value Container

#### `SecretValue`
- **Type**: `sealed class` implementing `IDisposable`
- **Purpose**: Secure container for sensitive data with proper disposal patterns
- **Security Features**:
  - Uses `SecureString` for string data
  - Secure memory clearing for binary data
  - Automatic disposal patterns
  - Secure accessor methods with callbacks
  - Protection against memory dumps
- **Data Types**: Supports both string and binary secrets
- **Properties**: Key, Version, CreatedAt, ModifiedAt, ExpiresAt, Metadata, IsExpired

```csharp
public sealed class SecretValue : IDisposable
{
    public string Key { get; }
    public string? Version { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset ModifiedAt { get; }
    public DateTimeOffset? ExpiresAt { get; }
    public bool IsBinary { get; }
    public bool IsExpired { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
    
    public string GetStringValue();
    public byte[] GetBinaryValue();
    public TResult AccessStringValue<TResult>(Func<string, TResult> accessor);
    public TResult AccessBinaryValue<TResult>(Func<byte[], TResult> accessor);
}
```

### Command Implementations

#### `SecretCommandBase`
- **Type**: `abstract class`
- **Interfaces**: `ISecretCommand`
- **Purpose**: Base implementation for all secret commands with common functionality
- **Features**:
  - Parameter and metadata management
  - Validation framework using FluentValidation
  - Command copying with new parameters/metadata
  - Timeout support
  - Provider-agnostic operation definitions

#### Concrete Command Classes

##### `GetSecretCommand`
- **Type**: `sealed class`
- **Base Class**: `SecretCommandBase`
- **Interfaces**: `ISecretCommand<SecretValue>`
- **Purpose**: Retrieve secret values with version and metadata options
- **Properties**: `Version`, `IncludeMetadata`
- **Factory Methods**: `Latest()`, `ForVersion()`
- **Features**: Non-modifying operation (IsSecretModifying = false)

```csharp
public sealed class GetSecretCommand : SecretCommandBase, ISecretCommand<SecretValue>
{
    public string? Version { get; }
    public bool IncludeMetadata { get; }
    
    public static GetSecretCommand Latest(string? container, string secretKey, bool includeMetadata = false, TimeSpan? timeout = null);
    public static GetSecretCommand ForVersion(string? container, string secretKey, string version, bool includeMetadata = false, TimeSpan? timeout = null);
}
```

##### Other Command Types
- `SetSecretCommand` - Store or update secret values (modifying operation)
- `DeleteSecretCommand` - Remove secrets from storage (modifying operation)
- `ListSecretsCommand` - List available secrets in a container (non-modifying)
- `GetSecretVersionsCommand` - List all versions of a specific secret (non-modifying)

### Health and Metrics

#### Health Monitoring Interfaces
- `ISecretManagerHealth` - Overall secret manager health
- `ISecretProviderHealth` - Individual provider health status
- `ISecretServiceHealth` - Service-level health metrics

#### Metrics and Performance Interfaces
- `ISecretOperationMetrics` - Operation-level metrics
- `ISecretServiceMetrics` - Service-level performance metrics  
- `ISecretServiceThroughput` - Throughput and capacity metrics

#### `HealthStatus`
- **Type**: Enum defining health states
- **Values**: Healthy, Degraded, Unhealthy, Unknown

```csharp
public enum HealthStatus
{
    Healthy, Degraded, Unhealthy, Unknown
}
```

### Configuration and Management

#### `ISecretManagementConfiguration`
- **Purpose**: Configuration interface for secret management settings
- **Features**: Provider-specific configuration with validation

#### Container and Metadata Management
- `ISecretContainer` - Secret container/vault abstraction with logical grouping
- `ISecretContainerUsage` - Container usage tracking and limits
- `ISecretMetadata` - Secret metadata management without exposing values

```csharp
public interface ISecretContainer
{
    // Logical grouping of secrets (vault, key ring, etc.)
    // Provides container-level operations
}

public interface ISecretMetadata
{
    // Secret information without exposing the actual value
    // Created date, expiry, tags, permissions
}
```

### Service Infrastructure

#### `SecretManagementServiceBase`
- **Type**: Base class for service implementations
- **Purpose**: Common functionality for secret service implementations with typed constraints

#### Service Type Management
- `ISecretManagementServiceType` - Service type definitions
- `SecretManagementServiceType` - Enhanced enum for service types
- `SecretManagementServiceTypes` - Collection of available service types

#### Factory Pattern
- `ISecretServiceFactory` - Factory interface for creating secret services

### Batch Operations

#### `ISecretBatchResult`
- **Purpose**: Results from batch secret operations
- **Features**: Success/failure tracking for multiple commands

```csharp
public interface ISecretBatchResult
{
    // Results from batch secret operations
    // Success/failure tracking for multiple commands
}
```

### Logging Infrastructure

#### `SecretProviderBaseLog`
- **Type**: Partial class with high-performance logging
- **Purpose**: Source-generated logging methods for secret providers
- **Features**: Structured logging with security-conscious message formatting

## Dependencies

### Project References
- `FractalDataWorks.Services` - Base service implementations
- `FractalDataWorks.Services.Abstractions` - Service abstractions and patterns

### Framework Dependencies
- `Microsoft.Extensions.Logging` - Logging abstractions
- `Microsoft.Extensions.DependencyInjection` - Dependency injection
- `FluentValidation` - Command validation framework
- `System.Security` - SecureString and cryptographic operations

## Usage Patterns

### Basic Secret Operations
```csharp
// Retrieve a secret
var getCommand = GetSecretCommand.Latest("my-vault", "database-password", includeMetadata: true);
var result = await secretManager.Execute<SecretValue>(getCommand, cancellationToken);

if (result.IsSuccess)
{
    using var secret = result.Value;
    // Secure access to secret value
    var connectionString = secret.AccessStringValue(password => 
        $"Server=myserver;Password={password};");
    
    // Secret is automatically disposed when using block exits
}

// Store a secret
var setCommand = new SetSecretCommand(
    container: "my-vault",
    secretKey: "api-key", 
    secretValue: new SecretValue("api-key", "secret-api-key-value"),
    parameters: new Dictionary<string, object?>
    {
        ["ExpiresAt"] = DateTimeOffset.UtcNow.AddYears(1)
    });

await secretManager.Execute(setCommand, cancellationToken);
```

### Batch Operations
```csharp
// Execute multiple operations together
var commands = new List<ISecretCommand>
{
    GetSecretCommand.Latest("vault1", "secret1"),
    GetSecretCommand.Latest("vault1", "secret2"), 
    new ListSecretsCommand("vault1")
};

var batchResult = await secretManager.ExecuteBatch(commands, cancellationToken);
if (batchResult.IsSuccess)
{
    var results = batchResult.Value;
    foreach (var commandResult in results.Results)
    {
        if (commandResult.IsSuccess)
        {
            // Process individual command result
        }
    }
}
```

### Service Registration
```csharp
services.Configure<AzureKeyVaultConfiguration>(config =>
{
    config.VaultUri = "https://my-vault.vault.azure.net/";
    config.ClientId = "client-id";
    config.ClientSecret = "client-secret";
});

services.AddSingleton<ISecretService<ISecretCommand>, AzureKeyVaultService>();
services.AddSingleton<ISecretManager, SecretManager>();
```

### Command Validation
```csharp
// Pre-flight command validation
var command = GetSecretCommand.Latest("my-vault", "secret-key");
var validationResult = secretManager.ValidateCommand(command);

if (validationResult.IsSuccess)
{
    // Command is valid, safe to execute
    var result = await secretManager.Execute<SecretValue>(command, cancellationToken);
}
else
{
    // Handle validation errors
    Logger.LogError("Command validation failed: {Message}", validationResult.Message);
}
```

### Health Monitoring
```csharp
// Check secret management system health
var healthResult = await secretManager.HealthCheckAsync(cancellationToken);
if (healthResult.IsSuccess)
{
    var health = healthResult.Value;
    foreach (var providerHealth in health.ProviderHealthStatuses)
    {
        Logger.LogInformation("Provider {Provider}: {Status}", 
            providerHealth.Key, providerHealth.Value.Status);
    }
}
```

## Security Features

### Secure Memory Handling
- `SecretValue` uses `SecureString` for string secrets
- Automatic memory clearing for binary data
- Secure disposal patterns prevent memory dumps
- Callback-based secure access methods

### Access Control
- Command-based validation before execution
- Provider-specific access control integration
- Audit trail through structured logging
- Timeout protections for long-running operations

### Data Protection
- Support for secret versioning and rotation
- Expiration date handling and validation
- Metadata encryption capabilities (provider-dependent)
- Secure parameter passing through command pattern

## Architecture Notes

### Command Pattern Benefits
- Provider-agnostic operation definitions
- Consistent validation and error handling
- Batch operation support
- Extensible parameter and metadata system
- Testable command composition

### Provider Abstraction
- Multiple secret providers through common interface
- Provider-specific configurations and capabilities
- Fallback and redundancy support
- Health monitoring across all providers

### Security-First Design
- Secure-by-default value containers
- Automatic resource disposal
- Memory clearing for sensitive data
- Structured logging without sensitive data exposure

## Generic Constraints

The type hierarchy flows from most general to most specific:

```
IFractalService<T>
    ↓
ISecretService : IFractalService<ISecretCommand>
    ↓  
SecretManagementServiceBase<TCommand, TConfiguration, TService>
    ↓
ConcreteSecretService<SpecificCommand, SpecificConfiguration, ConcreteSecretService>
```

## Framework Integration

This abstraction integrates with other FractalDataWorks services:

- **Authentication**: Uses secrets for API keys and certificates
- **FdwConnections**: Stores connection strings and credentials
- **DataGateways**: Secures database passwords and keys
- **Configuration**: Provides secure configuration values

## Extension Points

### Custom Commands
New secret operations can be added by:
1. Implementing `ISecretCommand` or extending `SecretCommandBase`
2. Adding validation logic for the operation
3. Registering with the command processor

### Custom Providers
New secret providers can be integrated by:
1. Implementing `ISecretService<TSecretCommand>`
2. Extending `SecretManagementServiceBase`
3. Providing provider-specific configuration
4. Registering with the service factory

### Custom Validation
Command validation can be extended through:
- Custom validation rules in command implementations
- Provider-specific validation logic
- Access control integration points
- Audit and compliance validation

## Design Philosophy

These abstractions are **intentionally comprehensive yet minimal**:

- ✅ Define domain boundaries through interfaces
- ✅ Provide type safety through base classes
- ✅ Enable service discovery and dependency injection
- ✅ Prioritize security over convenience
- ✅ Comprehensive command pattern for operations
- ✅ Secure disposal and memory management
- ❌ No implementation logic in abstractions
- ❌ No complex inheritance hierarchies
- ❌ No secret values exposed in interfaces

## Performance Considerations

- Batch operations reduce provider round-trips
- Secure disposal minimizes memory pressure
- Command caching and reuse patterns
- Async/await throughout for non-blocking operations
- Connection pooling through provider implementations
- High-performance logging with source generators

## Code Coverage Exclusions

- Source-generated logging classes (marked with appropriate attributes)
- Security-critical disposal methods (tested through integration tests)
- Platform-specific secure memory operations
- Provider-specific authentication flows (tested through provider tests)

## Evolution

This package will grow organically as the framework evolves:

- New interfaces added when patterns emerge
- Base classes extended with new constraints as needed
- Security features enhanced based on best practices
- Backward compatibility maintained for existing implementations

The comprehensive yet minimal design ensures maximum flexibility for concrete implementations while providing a consistent, secure service pattern across the FractalDataWorks ecosystem.
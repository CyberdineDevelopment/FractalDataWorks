# FractalDataWorks.Services.SecretManagers.AzureKeyVault

Azure Key Vault implementation of secret management services for the FractalDataWorks platform, providing secure storage and retrieval of sensitive data using Microsoft Azure Key Vault.

## Overview

This project provides a complete implementation of the secret management abstractions for Microsoft Azure Key Vault. It includes comprehensive authentication options, enhanced enum-based command processing, secure secret handling, and full integration with Azure's security and compliance features.

## Key Components

### Core Implementation

#### `AzureKeyVaultService`
- **Type**: `sealed class`
- **Base Class**: `SecretManagerServiceBase<ISecretCommand, AzureKeyVaultConfiguration, AzureKeyVaultService>`
- **Purpose**: Primary service implementation for Azure Key Vault operations
- **Features**:
  - Multiple authentication methods (managed identity, service principal, certificate, device code)
  - Enhanced enum-based command processing
  - Comprehensive error handling with Azure-specific exceptions
  - High-performance logging with structured events
  - Automatic secret name sanitization for Azure compliance
  - Support for secret versioning and metadata

### Configuration Management

#### `AzureKeyVaultConfiguration`
- **Type**: `sealed class`
- **Interfaces**: `ISecretManagerConfiguration`
- **Purpose**: Comprehensive configuration for Azure Key Vault connectivity and authentication
- **Key Properties**:
  - `VaultUri` - Azure Key Vault instance URI
  - `AuthenticationMethod` - Authentication strategy selection
  - `TenantId`, `ClientId`, `ClientSecret` - Service principal authentication
  - `CertificatePath`, `CertificatePassword` - Certificate-based authentication
  - `ManagedIdentityId` - Managed identity resource identifier
  - `Timeout` - Operation timeout configuration
  - `RetryPolicy` - Retry behavior settings
  - `EnableTracing` - Distributed tracing support
  - `ValidateOnStartup` - Configuration validation settings
  - `MaxSecretsPerPage` - Pagination limits (max 25 per Azure limits)
  - `IncludeDeletedByDefault` - Default behavior for deleted secrets

#### `AzureKeyVaultConfigurationValidator`
- **Type**: FluentValidation-based validator
- **Purpose**: Validates configuration completeness and correctness
- **Features**: Authentication-specific validation rules and Azure-specific constraints

### Authentication Support

The service supports multiple Azure authentication methods:

#### Managed Identity
```csharp
var config = new AzureKeyVaultConfiguration
{
    VaultUri = "https://myvault.vault.azure.net/",
    AuthenticationMethod = "ManagedIdentity",
    ManagedIdentityId = "optional-identity-id" // For multiple identities
};
```

#### Service Principal (Client Secret)
```csharp
var config = new AzureKeyVaultConfiguration
{
    VaultUri = "https://myvault.vault.azure.net/",
    AuthenticationMethod = "ServicePrincipal",
    TenantId = "your-tenant-id",
    ClientId = "your-client-id", 
    ClientSecret = "your-client-secret"
};
```

#### Certificate-Based Authentication
```csharp
var config = new AzureKeyVaultConfiguration
{
    VaultUri = "https://myvault.vault.azure.net/",
    AuthenticationMethod = "Certificate",
    TenantId = "your-tenant-id",
    ClientId = "your-client-id",
    CertificatePath = "/path/to/certificate.pfx",
    CertificatePassword = "certificate-password"
};
```

#### Device Code Authentication
```csharp
var config = new AzureKeyVaultConfiguration
{
    VaultUri = "https://myvault.vault.azure.net/",
    AuthenticationMethod = "DeviceCode"
};
```

### Enhanced Enum Command System

The service uses an enhanced enum pattern for command processing:

#### Command Type Definitions
- `GetSecret` - Retrieve secret values with version and metadata options
- `SetSecret` - Store or update secrets with metadata and expiration
- `DeleteSecret` - Remove secrets with optional permanent deletion
- `ListSecrets` - List available secrets with filtering and pagination
- `GetSecretVersions` - List all versions of a specific secret

#### Command Implementations
- `AzureKeyVaultCommand` - Base command implementation
- `AzureKeyVaultCommandValidator` - Command-specific validation
- `SecretCommandTypeBase` - Enhanced enum base class
- `SecretCommandTypeCollectionBase` - Command type collection management

### Metadata and Value Types

#### `AzureKeyVaultSecretMetadata`
- **Purpose**: Azure-specific secret metadata implementation
- **Properties**: Name, Version, CreatedOn, UpdatedOn, ExpiresOn, Enabled, Tags
- **Features**: Immutable metadata representation with Azure-specific properties

### Logging Infrastructure

#### `AzureKeyVaultServiceLog`
- **Type**: Partial class with source-generated logging methods
- **Purpose**: High-performance structured logging for Azure Key Vault operations
- **Events**:
  - Command execution lifecycle
  - Secret operations (retrieved, set, deleted, listed)
  - Azure-specific error conditions (access denied, not found)
  - Version management operations
  - Authentication and connection events

## Dependencies

### Project References
- `FractalDataWorks.Services.SecretManagers.Abstractions` - Secret management contracts
- `FractalDataWorks.EnhancedEnums` - Enhanced enum support
- `FractalDataWorks.EnhancedEnums.SourceGenerators` - Build-time enum generation

### Package References
- `Azure.Security.KeyVault.Secrets` - Azure Key Vault SDK
- `Azure.Identity` - Azure authentication and credential management
- `Microsoft.Extensions.DependencyInjection.Abstractions` - DI support
- `Microsoft.Extensions.Logging` - Logging infrastructure
- `Microsoft.Extensions.Options` - Configuration options pattern

## Usage Patterns

### Service Registration
```csharp
services.Configure<AzureKeyVaultConfiguration>(config =>
{
    config.VaultUri = "https://myvault.vault.azure.net/";
    config.AuthenticationMethod = "ManagedIdentity";
    config.EnableTracing = true;
    config.ValidateOnStartup = true;
    config.MaxSecretsPerPage = 25;
    config.Timeout = TimeSpan.FromSeconds(30);
});

services.AddSingleton<AzureKeyVaultService>();
```

### Basic Secret Operations
```csharp
var service = serviceProvider.GetRequiredService<AzureKeyVaultService>();

// Get latest version of a secret
var getCommand = GetSecretCommand.Latest(
    container: null, // Uses default vault from config
    secretKey: "database-password",
    includeMetadata: true
);

var result = await service.Execute<SecretValue>(getCommand, cancellationToken);
if (result.IsSuccess)
{
    using var secret = result.Value;
    var password = secret.GetStringValue();
    // Use the password securely
}

// Set a secret with expiration
var setCommand = new SetSecretCommand(
    container: null,
    secretKey: "api-key",
    secretValue: new SecretValue("api-key", "secret-value"),
    parameters: new Dictionary<string, object?>
    {
        ["Description"] = "API key for external service",
        ["ExpirationDate"] = DateTimeOffset.UtcNow.AddYears(1),
        ["Tags"] = new Dictionary<string, string>
        {
            ["Environment"] = "Production",
            ["Service"] = "ExternalAPI"
        }
    }
);

await service.Execute(setCommand, cancellationToken);
```

### Advanced Operations
```csharp
// Get specific version of a secret
var versionCommand = GetSecretCommand.ForVersion(
    container: null,
    secretKey: "database-password", 
    version: "specific-version-id"
);

// List secrets with filtering
var listCommand = new ListSecretsCommand(
    container: null,
    parameters: new Dictionary<string, object?>
    {
        ["MaxResults"] = 10,
        ["Filter"] = "database",
        ["IncludeDeleted"] = false
    }
);

// Delete with permanent purge
var deleteCommand = new DeleteSecretCommand(
    container: null,
    secretKey: "old-api-key",
    parameters: new Dictionary<string, object?>
    {
        ["PermanentDelete"] = true
    }
);
```

### Batch Operations
```csharp
var commands = new List<ISecretCommand>
{
    GetSecretCommand.Latest(null, "secret1"),
    GetSecretCommand.Latest(null, "secret2"),
    new GetSecretVersionsCommand(null, "versioned-secret")
};

var batchResult = await secretManager.ExecuteBatch(commands, cancellationToken);
```

## Architecture Notes

### Enhanced Enum Pattern
The service uses enhanced enums for command processing, providing:
- Type-safe command definitions
- Centralized command validation
- Extensible command execution logic
- Source-generated command collections

### Azure-Specific Features
- **Secret Name Sanitization**: Automatically converts secret names to Azure-compliant format (alphanumeric and hyphens)
- **Soft Delete Support**: Handles Azure's soft delete feature with permanent purge options
- **Version Management**: Full support for Azure Key Vault's secret versioning
- **Metadata Integration**: Azure-specific metadata including tags and properties
- **Compliance**: Built-in support for Azure security and compliance features

### Error Handling
Comprehensive error handling for Azure-specific scenarios:
- `RequestFailedException` handling with status code interpretation
- Access denied scenarios (403 errors)
- Resource not found scenarios (404 errors)
- Retry logic with exponential backoff
- Structured error logging without sensitive data exposure

## Security Features

### Credential Management
- **Multiple Authentication Methods**: Support for various Azure authentication patterns
- **Secure Credential Handling**: No credential logging or exposure
- **Certificate Support**: X.509 certificate-based authentication
- **Managed Identity Integration**: First-class support for Azure managed identities

### Secret Security
- **Automatic Sanitization**: Secret names sanitized for Azure compliance
- **Secure Disposal**: Integration with SecretValue disposal patterns
- **Metadata Protection**: Secure handling of secret metadata
- **Version Control**: Support for secret rotation and versioning

### Access Control
- **Azure RBAC Integration**: Leverages Azure Role-Based Access Control
- **Least Privilege**: Configuration supports minimal required permissions
- **Audit Trail**: Comprehensive logging for security auditing
- **Compliance**: Built-in support for regulatory compliance requirements

## Performance Considerations

- **Connection Reuse**: SecretClient instances are reused across operations
- **Pagination**: Efficient handling of large secret lists
- **Async Operations**: Full async/await throughout for non-blocking operations
- **Structured Logging**: High-performance logging with minimal allocations
- **Retry Logic**: Configurable retry policies for transient failures

## Code Coverage Exclusions

### Integration-Dependent Code
Methods requiring actual Azure Key Vault connections should be excluded from unit test coverage:

- **Authentication Methods**: `CreateManagedIdentityCredential()`, `CreateServicePrincipalCredential()`, `CreateCertificateCredential()`, `CreateDeviceCodeCredential()` - Require Azure authentication infrastructure
- **Azure SDK Operations**: Methods that directly call Azure SDK APIs require integration testing
- **Secret Client Creation**: `CreateSecretClient()` - Requires valid Azure credentials and network access
- **Configuration Validation**: Complex validation requiring Azure service validation

### Infrastructure Code
- Source-generated logging classes (marked with appropriate attributes)
- Enhanced enum infrastructure and collections
- Azure SDK wrapper methods requiring live service connections

## Known Limitations and TODOs

1. **Generic Return Type Handling**: The `Execute<TOut>` method contains a design issue (line 134) returning `default!` that needs architectural review
2. **Certificate Loading**: Certificate credential creation needs enhanced certificate loading logic
3. **Retry Policy**: Advanced retry policies are configured but implementation could be enhanced
4. **Health Monitoring**: Health check implementation needs to be added to the service

## Azure Key Vault Integration

### Required Permissions
The service requires the following Azure Key Vault permissions:
- **Secrets**: Get, Set, List, Delete (and Purge for permanent deletion)
- **Secret Versions**: Get, List
- **Metadata**: Read access to secret properties

### Compliance and Security
- Integrates with Azure's compliance and governance features
- Supports Azure's audit logging and monitoring
- Compatible with Azure Policy and security controls
- Leverages Azure's encryption and key management infrastructure

## Extensibility

### Custom Command Types
New Azure-specific operations can be added through:
1. Extending the enhanced enum command system
2. Adding new command type definitions
3. Implementing Azure-specific validation logic

### Advanced Authentication
Additional authentication methods can be integrated by:
1. Extending the credential creation logic
2. Adding new authentication method configurations
3. Implementing custom TokenCredential providers

The implementation provides a secure, scalable, and Azure-native solution for secret management within the FractalDataWorks ecosystem.
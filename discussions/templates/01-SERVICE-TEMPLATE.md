# Service Project Template Documentation

> **Version**: 1.0
> **Last Updated**: 2025-10-14
> **Architecture**: FractalDataWorks Developer Kit

## Table of Contents

1. [What is a Service?](#1-what-is-a-service)
2. [Service Components](#2-service-components)
3. [Dependencies](#3-dependencies)
4. [ServiceType Pattern](#4-servicetype-pattern)
5. [Example Implementation](#5-example-implementation)
6. [Template Parameters](#6-template-parameters)
7. [Common Patterns](#7-common-patterns)
8. [Common Mistakes to Avoid](#8-common-mistakes-to-avoid)

---

## 1. What is a Service?

### Purpose and Role

A **Service** in the FractalDataWorks architecture is a self-contained unit of business functionality that:

- Executes domain-specific commands through a command pattern
- Maintains its own configuration and lifecycle
- Integrates with dependency injection containers
- Provides consistent error handling through Results pattern
- Supports high-performance logging through source generators

### Difference Between Service and Connection

| Aspect | Service | Connection |
|--------|---------|------------|
| **Purpose** | Business logic execution | Data source connectivity |
| **Lifetime** | Typically Scoped or Singleton | Typically Scoped (manages resources) |
| **Examples** | Authentication, SecretManagers, Scheduling | MsSql, Rest, GraphQL |
| **Commands** | Domain operations (Login, Logout) | Data operations (Query, Execute) |
| **Base Class** | `ServiceBase<TCommand, TConfiguration, TService>` | `ConnectionServiceBase<TCommand, TConfiguration, TService, TTranslator>` |

### When to Create a New Service vs Extending Existing

**Create a NEW service when:**
- You need a distinct domain area (e.g., Authentication, Scheduling, SecretManagers)
- The service has different command types from existing services
- The service requires unique configuration properties
- The service has different lifecycle requirements

**Extend EXISTING service when:**
- You're adding a new provider/implementation (e.g., AzureKeyVault extends SecretManagers)
- The command types and configuration pattern match
- You're adding functionality to an existing domain

---

## 2. Service Components

### Project Structure

A complete service implementation consists of TWO projects:

```
src/
├── FractalDataWorks.Services.{Domain}.Abstractions/
│   ├── FractalDataWorks.Services.{Domain}.Abstractions.csproj (netstandard2.0)
│   ├── I{Domain}Service.cs
│   ├── {Domain}ServiceBase.cs
│   ├── I{Domain}Configuration.cs
│   ├── {Domain}TypeBase.cs
│   ├── {Domain}Types.cs (partial, source-generated)
│   ├── Commands/
│   │   ├── I{Domain}Command.cs
│   │   └── {Domain}Commands.cs
│   ├── Messages/
│   │   ├── {Domain}Message.cs
│   │   ├── {Domain}MessageCollectionBase.cs
│   │   └── {Domain}Messages.cs (source-generated)
│   └── Factories/
│       └── I{Domain}ServiceFactory.cs
│
└── FractalDataWorks.Services.{Domain}.{Provider}/
    ├── FractalDataWorks.Services.{Domain}.{Provider}.csproj (net10.0)
    ├── {Provider}{Domain}Service.cs
    ├── {Provider}{Domain}Type.cs
    ├── {Provider}{Domain}Configuration.cs
    ├── {Provider}{Domain}ServiceFactory.cs
    ├── Commands/
    └── Logging/
        └── {Provider}{Domain}ServiceLog.cs
```

### Required Files in Abstractions Project

#### 1. Service Interface (`I{Domain}Service.cs`)

```csharp
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.{Domain}.Abstractions;

/// <summary>
/// Main interface for {domain} service operations.
/// </summary>
public interface I{Domain}Service : IGenericService
{
    // Domain-specific methods if needed
}
```

#### 2. Service Base Class (`{Domain}ServiceBase.cs`)

```csharp
using Microsoft.Extensions.Logging;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.{Domain};

/// <summary>
/// Base class for {domain} service implementations.
/// </summary>
public abstract class {Domain}ServiceBase<TCommand, TConfiguration, TService>
    : ServiceBase<TCommand, TConfiguration, TService>
    where TCommand : I{Domain}Command
    where TConfiguration : class, I{Domain}Configuration
    where TService : class
{
    protected {Domain}ServiceBase(ILogger<TService> logger, TConfiguration configuration)
        : base(logger, configuration)
    {
    }
}
```

#### 3. Configuration Interface (`I{Domain}Configuration.cs`)

```csharp
using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Services.{Domain}.Abstractions;

/// <summary>
/// Configuration interface for {domain} services.
/// </summary>
public interface I{Domain}Configuration : IGenericConfiguration
{
    /// <summary>
    /// Gets the {domain} type name for service resolution.
    /// </summary>
    string {Domain}Type { get; }

    // Additional domain-specific configuration properties
}
```

#### 4. ServiceType Base Class (`{Domain}TypeBase.cs`)

```csharp
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.{Domain}.Abstractions;

/// <summary>
/// Base class for {domain} service type definitions.
/// </summary>
public abstract class {Domain}TypeBase<TService, TFactory, TConfiguration> :
    ServiceTypeBase<TService, TFactory, TConfiguration>,
    I{Domain}Type<TService, TFactory, TConfiguration>
    where TService : class, I{Domain}Service
    where TFactory : class, I{Domain}ServiceFactory<TService, TConfiguration>
    where TConfiguration : class, I{Domain}Configuration
{
    // Domain-specific metadata properties
    public virtual int Priority => 50;

    protected {Domain}TypeBase(
        int id,
        string name,
        string sectionName,
        string displayName,
        string description,
        string? category = null)
        : base(id, name, sectionName, displayName, description, category ?? "{Domain}")
    {
    }
}
```

#### 5. ServiceType Collection (`{Domain}Types.cs`)

```csharp
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;

namespace FractalDataWorks.Services.{Domain}.Abstractions;

/// <summary>
/// Collection of {domain} service types.
/// Generated by ServiceTypeCollectionGenerator.
/// </summary>
[ServiceTypeCollection(
    typeof({Domain}TypeBase<,,>),
    typeof(I{Domain}Type),
    typeof({Domain}Types))]
public partial class {Domain}Types :
    ServiceTypeCollectionBase<
        {Domain}TypeBase<I{Domain}Service, I{Domain}ServiceFactory<I{Domain}Service, I{Domain}Configuration>, I{Domain}Configuration>,
        I{Domain}Type,
        I{Domain}Service,
        I{Domain}Configuration,
        I{Domain}ServiceFactory<I{Domain}Service, I{Domain}Configuration>>
{
}
```

#### 6. Message Collection (`Messages/{Domain}MessageCollectionBase.cs`)

```csharp
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.{Domain}.Abstractions.Messages;

/// <summary>
/// Collection definition to generate {Domain}Messages static class.
/// </summary>
[MessageCollection("{Domain}Messages", ReturnType = typeof(IServiceMessage))]
public abstract class {Domain}MessageCollectionBase : MessageCollectionBase<{Domain}Message>
{
}
```

#### 7. Command Interface (`Commands/I{Domain}Command.cs`)

```csharp
using FractalDataWorks.Abstractions;

namespace FractalDataWorks.Services.{Domain}.Abstractions;

/// <summary>
/// Base interface for {domain} commands.
/// </summary>
public interface I{Domain}Command : IGenericCommand
{
    // Common command properties
}
```

#### 8. Factory Interface (`Factories/I{Domain}ServiceFactory.cs`)

```csharp
using FractalDataWorks.Abstractions;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.{Domain}.Abstractions;

/// <summary>
/// Non-generic marker interface for {domain} service factories.
/// </summary>
public interface I{Domain}ServiceFactory : IServiceFactory
{
}

/// <summary>
/// Interface for {domain} service factories with service type.
/// </summary>
public interface I{Domain}ServiceFactory<TService> :
    I{Domain}ServiceFactory,
    IServiceFactory<TService>
    where TService : class, I{Domain}Service
{
}

/// <summary>
/// Interface for {domain} service factories with configuration.
/// </summary>
public interface I{Domain}ServiceFactory<TService, TConfiguration> :
    I{Domain}ServiceFactory<TService>,
    IServiceFactory<TService, TConfiguration>
    where TService : class, I{Domain}Service
    where TConfiguration : class, I{Domain}Configuration
{
}
```

### Required Files in Implementation Project

#### 1. Service Implementation (`{Provider}{Domain}Service.cs`)

```csharp
using Microsoft.Extensions.Logging;
using FractalDataWorks.Results;
using FractalDataWorks.Services.{Domain};
using FractalDataWorks.Services.{Domain}.Abstractions;

namespace FractalDataWorks.Services.{Domain}.{Provider};

/// <summary>
/// {Provider} implementation of {domain} service.
/// </summary>
public sealed class {Provider}{Domain}Service :
    {Domain}ServiceBase<I{Domain}Command, I{Domain}Configuration, {Provider}{Domain}Service>
{
    public {Provider}{Domain}Service(
        ILogger<{Provider}{Domain}Service> logger,
        I{Domain}Configuration configuration)
        : base(logger, configuration)
    {
    }

    public override async Task<IGenericResult> Execute(
        I{Domain}Command command,
        CancellationToken cancellationToken)
    {
        // Implementation
        return GenericResult.Success("Command executed successfully");
    }

    public override async Task<IGenericResult<T>> Execute<T>(
        I{Domain}Command command,
        CancellationToken cancellationToken)
    {
        // Implementation
        return GenericResult<T>.Success(default(T), "Command executed successfully");
    }
}
```

#### 2. ServiceType Implementation (`{Provider}{Domain}Type.cs`)

```csharp
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.{Domain}.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.{Domain}.{Provider};

/// <summary>
/// Service type definition for {Provider} {domain}.
/// </summary>
[ServiceTypeOption(typeof({Domain}Types), "{Provider}")]
public sealed class {Provider}{Domain}Type :
    {Domain}TypeBase<I{Domain}Service, I{Provider}{Domain}ServiceFactory, I{Domain}Configuration>
{
    public {Provider}{Domain}Type()
        : base(
            id: 1,
            name: "{Provider}",
            sectionName: "{Provider}{Domain}",
            displayName: "{Provider} {Domain}",
            description: "{Provider} implementation of {domain} service",
            category: "{Domain}")
    {
    }

    public override int Priority => 90;

    public override void Configure(IConfiguration configuration)
    {
        // Provider-specific configuration
    }

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<I{Provider}{Domain}ServiceFactory, {Provider}{Domain}ServiceFactory>();
        services.AddScoped<I{Domain}Service, {Provider}{Domain}Service>();
    }
}
```

#### 3. Factory Implementation (`{Provider}{Domain}ServiceFactory.cs`)

```csharp
using Microsoft.Extensions.Logging;
using FractalDataWorks.Services;
using FractalDataWorks.Services.{Domain}.Abstractions;

namespace FractalDataWorks.Services.{Domain}.{Provider};

/// <summary>
/// Factory for creating {Provider} {domain} service instances.
/// </summary>
public sealed class {Provider}{Domain}ServiceFactory :
    GenericServiceFactory<I{Domain}Service, I{Domain}Configuration>,
    I{Provider}{Domain}ServiceFactory
{
    public {Provider}{Domain}ServiceFactory(
        ILogger<GenericServiceFactory<I{Domain}Service, I{Domain}Configuration>> logger)
        : base(logger)
    {
    }

    public {Provider}{Domain}ServiceFactory()
        : base()
    {
    }
}
```

#### 4. Source-Generated Logging (`Logging/{Provider}{Domain}ServiceLog.cs`)

```csharp
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.{Domain}.{Provider}.Logging;

/// <summary>
/// High-performance source-generated logging for {Provider}{Domain}Service.
/// </summary>
public static partial class {Provider}{Domain}ServiceLog
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Executing {CommandType} command")]
    public static partial void ExecutingCommand(ILogger logger, string commandType);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Error,
        Message = "Command execution failed: {ErrorMessage}")]
    public static partial void CommandExecutionFailed(ILogger logger, string errorMessage);
}
```

---

## 3. Dependencies

### Abstractions Project (netstandard2.0)

**Required Project References:**

```xml
<ItemGroup>
  <ProjectReference Include="..\FractalDataWorks.Services.Abstractions\FractalDataWorks.Services.Abstractions.csproj" />
  <ProjectReference Include="..\FractalDataWorks.ServiceTypes\FractalDataWorks.ServiceTypes.csproj" />
  <ProjectReference Include="..\FractalDataWorks.Collections\FractalDataWorks.Collections.csproj" />

  <!-- Source generators - NOT exported to avoid circular dependencies -->
  <ProjectReference Include="..\FractalDataWorks.ServiceTypes.SourceGenerators\FractalDataWorks.ServiceTypes.SourceGenerators.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
  <ProjectReference Include="..\FractalDataWorks.Messages.SourceGenerators\FractalDataWorks.Messages.SourceGenerators.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
  <ProjectReference Include="..\FractalDataWorks.Collections.SourceGenerators\FractalDataWorks.Collections.SourceGenerators.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false"
                    PrivateAssets="none" />
</ItemGroup>
```

**Required NuGet Packages:**

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
</ItemGroup>
```

**For NuGet Package Distribution (add to Abstractions project):**

```xml
<ItemGroup>
  <!-- Explicitly embed generator DLLs for consumers -->
  <None Include="..\FractalDataWorks.Collections.SourceGenerators\bin\$(Configuration)\netstandard2.0\FractalDataWorks.Collections.SourceGenerators.dll"
        Pack="true"
        PackagePath="analyzers/dotnet/cs"
        Visible="false"
        Condition="Exists('..\FractalDataWorks.Collections.SourceGenerators\bin\$(Configuration)\netstandard2.0\FractalDataWorks.Collections.SourceGenerators.dll')" />

  <None Include="..\FractalDataWorks.Messages.SourceGenerators\bin\$(Configuration)\netstandard2.0\FractalDataWorks.Messages.SourceGenerators.dll"
        Pack="true"
        PackagePath="analyzers/dotnet/cs"
        Visible="false"
        Condition="Exists('..\FractalDataWorks.Messages.SourceGenerators\bin\$(Configuration)\netstandard2.0\FractalDataWorks.Messages.SourceGenerators.dll')" />

  <None Include="..\FractalDataWorks.ServiceTypes.SourceGenerators\bin\$(Configuration)\netstandard2.0\FractalDataWorks.ServiceTypes.SourceGenerators.dll"
        Pack="true"
        PackagePath="analyzers/dotnet/cs"
        Visible="false"
        Condition="Exists('..\FractalDataWorks.ServiceTypes.SourceGenerators\bin\$(Configuration)\netstandard2.0\FractalDataWorks.ServiceTypes.SourceGenerators.dll')" />
</ItemGroup>
```

### Implementation Project (net10.0 or multi-target)

**Required Project References:**

```xml
<ItemGroup>
  <ProjectReference Include="..\FractalDataWorks.Services.{Domain}.Abstractions\FractalDataWorks.Services.{Domain}.Abstractions.csproj" />
  <ProjectReference Include="..\FractalDataWorks.Services\FractalDataWorks.Services.csproj" />
</ItemGroup>
```

**Provider-Specific Packages** (examples):

```xml
<ItemGroup>
  <!-- For Azure providers -->
  <PackageReference Include="Azure.Identity" />
  <PackageReference Include="Azure.Security.KeyVault.Secrets" />

  <!-- For database providers -->
  <PackageReference Include="Microsoft.Data.SqlClient" />

  <!-- Common -->
  <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
</ItemGroup>
```

### Critical Source Generator Configuration

**IMPORTANT**: Source generators MUST be referenced with specific attributes:

```xml
<ProjectReference Include="..\FractalDataWorks.{Generator}.SourceGenerators\..."
                  OutputItemType="Analyzer"      <!-- Makes it a code generator -->
                  ReferenceOutputAssembly="false" <!-- Don't reference the DLL -->
                  PrivateAssets="all" />          <!-- Don't flow to consumers (optional) -->
```

**Why these attributes matter:**

- `OutputItemType="Analyzer"` - Tells MSBuild this is a Roslyn analyzer/generator
- `ReferenceOutputAssembly="false"` - Prevents circular dependencies
- `PrivateAssets="all"` - Keeps generators internal (use `PrivateAssets="none"` if packaging)

---

## 4. ServiceType Pattern

### ServiceTypeOption Attribute

The `[ServiceTypeOption]` attribute marks a concrete service type for source generation:

```csharp
[ServiceTypeOption(typeof({Domain}Types), "{Provider}")]
public sealed class {Provider}{Domain}Type : {Domain}TypeBase<...>
{
    // Implementation
}
```

**Parameters:**
- `collectionType`: The partial class that will contain this option (e.g., `typeof(AuthenticationTypes)`)
- `name`: The property/method name in generated collection (e.g., "AzureEntra" generates `AuthenticationTypes.AzureEntra`)

### ServiceTypeCollection Attribute

The `[ServiceTypeCollection]` attribute marks the collection class for generation:

```csharp
[ServiceTypeCollection(
    typeof({Domain}TypeBase<,,>),  // Base type to search for
    typeof(I{Domain}Type),         // Return type for generated members
    typeof({Domain}Types))]        // The collection class itself
public partial class {Domain}Types : ServiceTypeCollectionBase<...>
{
}
```

### Generated Collection Usage

The source generator creates static members for each `[ServiceTypeOption]`:

```csharp
// Generated code (example)
public partial class AuthenticationTypes
{
    /// <summary>
    /// Azure Entra authentication type.
    /// </summary>
    public static IAuthenticationType AzureEntra =>
        new AzureEntraAuthenticationType();
}
```

**Usage in code:**

```csharp
// Access service types
var authType = AuthenticationTypes.AzureEntra;

// Get all types
var allTypes = AuthenticationTypes.GetAll();

// Lookup by name
var type = AuthenticationTypes.FromName("AzureEntra");
```

### DI Lifetime Registration

Services specify their DI lifetime in the `Register` method:

```csharp
public override void Register(IServiceCollection services)
{
    // Singleton - one instance for application lifetime
    services.AddSingleton<IMyService, MyService>();

    // Scoped - one instance per request/scope
    services.AddScoped<IMyService, MyService>();

    // Transient - new instance every time
    services.AddTransient<IMyService, MyService>();
}
```

**Guidelines:**
- **Singleton**: Stateless services, caches, configuration
- **Scoped**: Database connections, per-request services
- **Transient**: Lightweight, stateful operations

---

## 5. Example Implementation

### Complete Secret Manager Service Example

This example shows a simple secret manager service with all required components.

#### Abstractions Project Structure

```
FractalDataWorks.Services.SecretManagers.Abstractions/
├── ISecretManager.cs
├── ISecretManagerConfiguration.cs
├── ISecretManagerCommand.cs
├── SecretManagerTypeBase.cs
├── SecretManagerTypes.cs
├── Messages/
│   ├── SecretManagerMessage.cs
│   └── SecretManagerMessageCollectionBase.cs
└── Factories/
    └── ISecretManagerServiceFactory.cs
```

#### Key Files

**ISecretManager.cs**

```csharp
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.SecretManagers.Abstractions;

public interface ISecretManager : IGenericService
{
    Task<IGenericResult<string>> GetSecret(
        string secretName,
        CancellationToken cancellationToken = default);
}
```

**SecretManagerTypeBase.cs**

```csharp
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.SecretManagers.Abstractions;

public abstract class SecretManagerTypeBase<TService, TFactory, TConfiguration> :
    ServiceTypeBase<TService, TFactory, TConfiguration>,
    ISecretManagerType<TService, TFactory, TConfiguration>
    where TService : class, ISecretManager
    where TFactory : class, ISecretManagerServiceFactory<TService, TConfiguration>
    where TConfiguration : class, ISecretManagerConfiguration
{
    public string[] SupportedSecretStores { get; }
    public bool SupportsRotation { get; }
    public bool SupportsVersioning { get; }
    public virtual int Priority => 50;

    protected SecretManagerTypeBase(
        int id,
        string name,
        string sectionName,
        string displayName,
        string description,
        string[] supportedSecretStores,
        bool supportsRotation,
        bool supportsVersioning,
        string? category = null)
        : base(id, name, sectionName, displayName, description, category ?? "Secret Management")
    {
        SupportedSecretStores = supportedSecretStores;
        SupportsRotation = supportsRotation;
        SupportsVersioning = supportsVersioning;
    }
}
```

**SecretManagerTypes.cs**

```csharp
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;

namespace FractalDataWorks.Services.SecretManagers.Abstractions;

[ServiceTypeCollection(
    typeof(SecretManagerTypeBase<,,>),
    typeof(ISecretManagerType),
    typeof(SecretManagerTypes))]
public partial class SecretManagerTypes :
    ServiceTypeCollectionBase<
        SecretManagerTypeBase<ISecretManager, ISecretManagerServiceFactory<ISecretManager, ISecretManagerConfiguration>, ISecretManagerConfiguration>,
        ISecretManagerType,
        ISecretManager,
        ISecretManagerConfiguration,
        ISecretManagerServiceFactory<ISecretManager, ISecretManagerConfiguration>>
{
}
```

#### Implementation Project Structure

```
FractalDataWorks.Services.SecretManagers.AzureKeyVault/
├── AzureKeyVaultService.cs
├── AzureKeyVaultType.cs
├── AzureKeyVaultConfiguration.cs
├── AzureKeyVaultServiceFactory.cs
└── Logging/
    └── AzureKeyVaultServiceLog.cs
```

**AzureKeyVaultType.cs**

```csharp
using FractalDataWorks.ServiceTypes.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.SecretManagers.AzureKeyVault;

[ServiceTypeOption(typeof(SecretManagerTypes), "AzureKeyVault")]
public sealed class AzureKeyVaultType :
    SecretManagerTypeBase<ISecretManager, IAzureKeyVaultServiceFactory, ISecretManagerConfiguration>
{
    public AzureKeyVaultType()
        : base(
            id: 1,
            name: "AzureKeyVault",
            sectionName: "AzureKeyVault",
            displayName: "Azure Key Vault",
            description: "Azure Key Vault secret management",
            supportedSecretStores: ["AzureKeyVault"],
            supportsRotation: true,
            supportsVersioning: true,
            category: "Secret Management")
    {
    }

    public override int Priority => 90;

    public override void Configure(IConfiguration configuration)
    {
        // Azure Key Vault specific configuration
    }

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IAzureKeyVaultServiceFactory, AzureKeyVaultServiceFactory>();
        services.AddScoped<ISecretManager, AzureKeyVaultService>();
    }
}
```

---

## 6. Template Parameters

When creating a new service from this template, replace the following parameters:

| Parameter | Description | Example |
|-----------|-------------|---------|
| `{Domain}` | Service domain area | Authentication, SecretManagers, Scheduling |
| `{Provider}` | Implementation provider | AzureEntra, AzureKeyVault, Local |
| `{domain}` | Lowercase domain | authentication, secretmanagers, scheduling |
| `{Command}` | Command type name | AuthenticationCommand, SecretManagerCommand |
| `{Configuration}` | Configuration type name | AuthenticationConfiguration, SecretManagerConfiguration |

### Full Replacement Example

For an **Email Service with SendGrid Provider**:

- `{Domain}` = Email
- `{Provider}` = SendGrid
- `{domain}` = email
- `{Command}` = EmailCommand
- `{Configuration}` = EmailConfiguration

Generated files:
- `FractalDataWorks.Services.Email.Abstractions/IEmailService.cs`
- `FractalDataWorks.Services.Email.SendGrid/SendGridEmailService.cs`
- `FractalDataWorks.Services.Email.SendGrid/SendGridEmailType.cs`

---

## 7. Common Patterns

### Factory Pattern

All services use the factory pattern for creation:

```csharp
public sealed class MyServiceFactory :
    GenericServiceFactory<IMyService, IMyConfiguration>,
    IMyServiceFactory
{
    public MyServiceFactory(ILogger<GenericServiceFactory<IMyService, IMyConfiguration>> logger)
        : base(logger)
    {
    }
}
```

**Benefits:**
- Consistent service creation
- Automatic logging
- FastGenericNew for performance
- Fallback to Activator.CreateInstance

### Provider Pattern

Services support multiple providers through ServiceTypes:

```csharp
// Define providers
[ServiceTypeOption(typeof(EmailTypes), "SendGrid")]
public sealed class SendGridEmailType : EmailTypeBase<...> { }

[ServiceTypeOption(typeof(EmailTypes), "Smtp")]
public sealed class SmtpEmailType : EmailTypeBase<...> { }

// Use at runtime
var emailType = EmailTypes.FromName(configuration.EmailType);
emailType.Register(services);
```

### Async Operations

All service operations are async:

```csharp
public override async Task<IGenericResult> Execute(
    TCommand command,
    CancellationToken cancellationToken)
{
    // Use ConfigureAwait(false) for library code
    var result = await SomeOperationAsync(command).ConfigureAwait(false);

    // Check cancellation
    cancellationToken.ThrowIfCancellationRequested();

    return result;
}
```

### Error Handling with Results

Use Results pattern instead of exceptions:

```csharp
// Good - Railway-Oriented Programming
public async Task<IGenericResult<string>> GetSecretAsync(string key)
{
    if (string.IsNullOrEmpty(key))
    {
        return GenericResult<string>.Failure(
            SecretManagerMessages.SecretKeyRequired());
    }

    try
    {
        var secret = await vault.GetSecretAsync(key);
        return GenericResult<string>.Success(secret.Value, "Secret retrieved");
    }
    catch (Exception ex)
    {
        return GenericResult<string>.Failure(
            SecretManagerMessages.SecretRetrievalFailed(key, ex.Message));
    }
}

// Bad - throwing exceptions
public async Task<string> GetSecretAsync(string key)
{
    if (string.IsNullOrEmpty(key))
        throw new ArgumentException("Key required"); // Don't do this

    return await vault.GetSecretAsync(key);
}
```

### Logging Patterns

Use source-generated logging for performance:

```csharp
// Define in {Service}Log.cs
public static partial class MyServiceLog
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Processing {CommandType} for user {UserId}")]
    public static partial void ProcessingCommand(
        ILogger logger,
        string commandType,
        string userId);
}

// Use in service
MyServiceLog.ProcessingCommand(Logger, command.GetType().Name, command.UserId);
```

**Event ID Ranges by Domain:**
- 1000-1999: Authentication
- 2000-2999: SecretManagers
- 3000-3999: Connections
- 4000-4999: Data
- 5000-5999: Execution

---

## 8. Common Mistakes to Avoid

### 1. Not Using ServiceTypeCollection

**Wrong:**

```csharp
// Manual collection - don't do this
public static class AuthenticationTypes
{
    public static readonly IAuthenticationType AzureEntra = new AzureEntraAuthenticationType();
    public static readonly IAuthenticationType Local = new LocalAuthenticationType();
}
```

**Right:**

```csharp
// Use source generator
[ServiceTypeCollection(typeof(AuthenticationTypeBase<,,>), typeof(IAuthenticationType), typeof(AuthenticationTypes))]
public partial class AuthenticationTypes : ServiceTypeCollectionBase<...>
{
}

[ServiceTypeOption(typeof(AuthenticationTypes), "AzureEntra")]
public sealed class AzureEntraAuthenticationType : AuthenticationTypeBase<...>
{
}
```

### 2. Incorrect Source Generator Setup

**Wrong:**

```xml
<!-- Missing OutputItemType or ReferenceOutputAssembly -->
<ProjectReference Include="..\Generator\Generator.csproj" />
```

**Right:**

```xml
<ProjectReference Include="..\Generator\Generator.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

### 3. Missing MessageCollection

**Wrong:**

```csharp
// Direct message instantiation
return GenericResult.Failure("Configuration not found");
```

**Right:**

```csharp
// Use message collection
return GenericResult.Failure(
    AuthenticationMessages.ConfigurationNotFound(configName));
```

### 4. Improper Async Patterns

**Wrong:**

```csharp
// Blocking async calls
public IGenericResult Execute(TCommand command)
{
    var result = ExecuteAsync(command).Result; // Deadlock risk!
    return result;
}
```

**Right:**

```csharp
// Async all the way
public async Task<IGenericResult> Execute(TCommand command, CancellationToken cancellationToken)
{
    var result = await ExecuteAsync(command).ConfigureAwait(false);
    return result;
}
```

### 5. Wrong Target Framework for Abstractions

**Wrong:**

```xml
<TargetFramework>net10.0</TargetFramework> <!-- In Abstractions project -->
```

**Right:**

```xml
<TargetFramework>netstandard2.0</TargetFramework> <!-- Abstractions must be netstandard2.0 -->
```

### 6. Not Implementing Both Execute Overloads

**Wrong:**

```csharp
// Only implementing one signature
public override async Task<IGenericResult> Execute(TCommand command, CancellationToken cancellationToken)
{
    // Implementation
}
// Missing: Execute<T> overload
```

**Right:**

```csharp
// Implement both signatures
public override async Task<IGenericResult> Execute(TCommand command, CancellationToken cancellationToken)
{
    // Non-generic implementation
}

public override async Task<IGenericResult<T>> Execute<T>(TCommand command, CancellationToken cancellationToken)
{
    // Generic implementation
}
```

### 7. Forgetting to Call base Constructor

**Wrong:**

```csharp
public MyServiceType() // Missing base call
{
    // Initialization
}
```

**Right:**

```csharp
public MyServiceType()
    : base(id: 1, name: "MyService", sectionName: "MyService",
           displayName: "My Service", description: "Description")
{
    // Additional initialization
}
```

### 8. Not Disposing Resources

**Wrong:**

```csharp
public class MyService : ServiceBase<TCommand, TConfiguration, TService>
{
    private readonly HttpClient _httpClient = new HttpClient(); // Never disposed
}
```

**Right:**

```csharp
public class MyService : ServiceBase<TCommand, TConfiguration, TService>
{
    private readonly HttpClient _httpClient;

    public MyService(ILogger logger, TConfiguration config, HttpClient httpClient)
        : base(logger, config)
    {
        _httpClient = httpClient; // Injected, managed by DI
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Dispose owned resources
        }
        base.Dispose(disposing);
    }
}
```

---

## Appendix A: Quick Start Checklist

When creating a new service, use this checklist:

- [ ] Create Abstractions project (netstandard2.0)
- [ ] Create Implementation project (net10.0)
- [ ] Add required project references
- [ ] Add source generator references with correct attributes
- [ ] Create `I{Domain}Service.cs`
- [ ] Create `{Domain}ServiceBase.cs`
- [ ] Create `I{Domain}Configuration.cs`
- [ ] Create `{Domain}TypeBase.cs`
- [ ] Create `{Domain}Types.cs` with `[ServiceTypeCollection]`
- [ ] Create `I{Domain}Command.cs`
- [ ] Create message collection with `[MessageCollection]`
- [ ] Create factory interfaces
- [ ] Create provider implementation
- [ ] Create provider ServiceType with `[ServiceTypeOption]`
- [ ] Create provider factory
- [ ] Add source-generated logging
- [ ] Build and verify source generation
- [ ] Write unit tests

---

## Appendix B: Related Documentation

- [ServiceTypes Architecture](./02-SERVICETYPES-ARCHITECTURE.md)
- [Source Generators Guide](./03-SOURCE-GENERATORS.md)
- [Message Collections](./04-MESSAGE-COLLECTIONS.md)
- [Results Pattern](./05-RESULTS-PATTERN.md)

---

**Document Version**: 1.0
**Last Updated**: 2025-10-14
**Maintained By**: FractalDataWorks Architecture Team

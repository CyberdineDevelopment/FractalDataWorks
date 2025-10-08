# FractalDataWorks Developer Kit - Project Reference

**Complete catalog of all projects with purpose, dependencies, and usage examples**

## Table of Contents

- [Foundation Layer](#foundation-layer)
- [Infrastructure Layer](#infrastructure-layer)
- [Service Core Layer](#service-core-layer)
- [Domain Services Layer](#domain-services-layer)
- [Web Integration Layer](#web-integration-layer)
- [Development Tools Layer](#development-tools-layer)
- [MCP Tools Layer](#mcp-tools-layer)
- [Data Layer](#data-layer)
- [Templates](#templates)
- [Test Projects](#test-projects)

## Foundation Layer

### FractalDataWorks.Abstractions

**Purpose**: Core interfaces and base types for the entire framework

**Target Framework**: netstandard2.0

**Key Types**:
- `ITypedOption` - Base interface for type-safe options
- `INamedType` - Interface for types with names and IDs
- Core extension methods

**Dependencies**: None (pure abstractions)

**Usage Example**:
```csharp
public interface IMyCustomOption : ITypedOption
{
    string DisplayName { get; }
    int Priority { get; }
}
```

---

### FractalDataWorks.CodeBuilder.Abstractions

**Purpose**: Abstractions for code generation infrastructure

**Target Framework**: netstandard2.0

**Key Types**:
- `ICodeBuilder` - Interface for code generation
- `ISymbolAnalyzer` - Interface for Roslyn symbol analysis
- `ISourceGenerator` - Base interface for source generators

**Dependencies**:
- Microsoft.CodeAnalysis.Common
- Microsoft.CodeAnalysis.CSharp

**Usage**: Used by all source generators as base infrastructure

---

### FractalDataWorks.CodeBuilder.CSharp

**Purpose**: C# code generation utilities

**Target Framework**: netstandard2.0

**Key Types**:
- `CSharpCodeBuilder` - Fluent API for C# code generation
- `NamespaceBuilder` - Namespace generation
- `ClassBuilder` - Class generation
- `MethodBuilder` - Method generation

**Dependencies**:
- FractalDataWorks.CodeBuilder.Abstractions
- Microsoft.CodeAnalysis.CSharp

**Usage Example**:
```csharp
var builder = new CSharpCodeBuilder();
builder.Namespace("MyNamespace")
    .Class("MyClass")
    .Public()
    .Method("MyMethod", "void")
    .Public()
    .Body("Console.WriteLine(\"Hello\");");

string code = builder.ToString();
```

---

### FractalDataWorks.CodeBuilder.Analysis

**Purpose**: Base code analysis infrastructure

**Target Framework**: netstandard2.0

**Key Types**:
- `SymbolAnalyzer` - Roslyn symbol analysis
- `TypeDiscovery` - Type discovery utilities
- `AttributeAnalyzer` - Attribute discovery

**Dependencies**:
- FractalDataWorks.CodeBuilder.Abstractions
- Microsoft.CodeAnalysis.Common

---

### FractalDataWorks.CodeBuilder.Analysis.CSharp

**Purpose**: C#-specific code analysis

**Target Framework**: netstandard2.0

**Key Types**:
- `CSharpSymbolAnalyzer` - C# symbol analysis
- `CSharpTypeDiscovery` - C# type discovery
- `CSharpAttributeAnalyzer` - C# attribute analysis

**Dependencies**:
- FractalDataWorks.CodeBuilder.Analysis
- FractalDataWorks.CodeBuilder.CSharp
- Microsoft.CodeAnalysis.CSharp

---

## Infrastructure Layer

### FractalDataWorks.EnhancedEnums

**Purpose**: Type-safe enum pattern with rich behavior

**Target Framework**: netstandard2.0

**Key Types**:
- `EnumOptionBase<T>` - Base class for enhanced enums
- `EnumCollectionBase<T>` - Base class for enum collections
- `IEnumOption` - Interface for enum options

**Dependencies**:
- FractalDataWorks.Abstractions

**Public API Surface**:
- `EnumOptionBase<T>` constructor: `protected EnumOptionBase(int id, string name)`
- `EnumCollectionBase<T>.All()` - Get all enum options
- `EnumCollectionBase<T>.GetById(int id)` - O(1) lookup by ID
- `EnumCollectionBase<T>.GetByName(string name)` - O(1) lookup by name
- `EnumCollectionBase<T>.Empty()` - Get empty/default instance

**Usage Example**:
```csharp
public sealed class Priority : EnumOptionBase<Priority>
{
    public static readonly Priority Low = new(1, "Low");
    public static readonly Priority Medium = new(2, "Medium");
    public static readonly Priority High = new(3, "High");

    private Priority(int id, string name) : base(id, name) { }
}

[EnumCollection("PriorityCollection")]
public partial class PriorityCollection : EnumCollectionBase<Priority>
{
    // Source generator populates _all and provides methods
}

// Usage
var all = PriorityCollection.All();
var high = PriorityCollection.GetById(3);
var medium = PriorityCollection.GetByName("Medium");
```

---

### FractalDataWorks.Collections

**Purpose**: High-performance type collections with O(1) lookups

**Target Framework**: net10.0

**Key Types**:
- `TypeCollectionBase<TBase>` - Single-generic collection base
- `TypeCollectionBase<TBase, TGeneric>` - Dual-generic collection base
- `TypeCollectionAttribute` - Marks collections for generation
- `TypeOptionAttribute` - Marks types for collection inclusion

**Dependencies**:
- FractalDataWorks.Abstractions

**Public API Surface**:
- `TypeCollectionBase<TBase, TGeneric>.All()` - Returns `FrozenSet<TGeneric>`
- `TypeCollectionBase<TBase, TGeneric>.GetById(int id)` - Returns `TGeneric` or Empty
- `TypeCollectionBase<TBase, TGeneric>.GetByName(string name)` - Returns `TGeneric` or Empty
- `TypeCollectionBase<TBase, TGeneric>.Empty()` - Returns default instance
- Static properties for each discovered type

**Usage Example**:
```csharp
// In Abstractions project
[TypeCollection(typeof(SecurityMethodBase), typeof(ISecurityMethod), typeof(SecurityMethods))]
public partial class SecurityMethods : TypeCollectionBase<SecurityMethodBase, ISecurityMethod>
{
}

// In any project
[TypeOption(typeof(SecurityMethods), "JWT")]
public sealed class JwtSecurityMethod : SecurityMethodBase
{
    public JwtSecurityMethod() : base(1, "JWT", true) { }
}

// Usage
var all = SecurityMethods.All(); // FrozenSet<ISecurityMethod>
var jwt = SecurityMethods.Jwt; // Static property (generated)
var byName = SecurityMethods.GetByName("JWT");
```

---

### FractalDataWorks.Messages

**Purpose**: Structured messaging system for services

**Target Framework**: netstandard2.0

**Key Types**:
- `MessageTemplate<TSeverity>` - Base class for messages
- `IRecMessage` - Message interface
- `MessageSeverity` - Severity enum (Information, Warning, Error, Critical)
- `MessageCollectionAttribute` - Marks message collections

**Dependencies**:
- FractalDataWorks.EnhancedEnums

**Public API Surface**:
- `MessageTemplate<TSeverity>` constructor: `protected MessageTemplate(int id, string name, TSeverity severity, string message, string? code, string? source)`
- `IRecMessage.Message` - Human-readable message
- `IRecMessage.Code` - Unique message code
- `IRecMessage.Source` - Message source component
- `IRecMessage.Severity` - Message severity level

**Usage Example**:
```csharp
[MessageCollection("UserServiceMessages")]
public abstract class UserServiceMessage : MessageTemplate<MessageSeverity>
{
    protected UserServiceMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, "UserService", message, code, null, null) { }
}

public sealed class UserCreatedMessage : UserServiceMessage
{
    public static UserCreatedMessage Instance { get; } = new();

    private UserCreatedMessage()
        : base(1001, "UserCreated", MessageSeverity.Information, "User created successfully", "USER_CREATED") { }
}
```

---

### FractalDataWorks.Results

**Purpose**: Result pattern for Railway-Oriented Programming

**Target Framework**: netstandard2.0

**Key Types**:
- `IGenericResult` - Non-generic result interface
- `IGenericResult<T>` - Generic result interface
- `GenericResult` - Non-generic result implementation
- `GenericResult<T>` - Generic result implementation
- `NonResult` - Unit type for operations without values

**Dependencies**:
- FractalDataWorks.Messages

**Public API Surface**:
```csharp
// Non-generic result
public interface IGenericResult
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    bool IsEmpty { get; }
    bool Error { get; }
    string Message { get; }
    IReadOnlyList<IRecMessage> Messages { get; }
}

// Generic result
public interface IGenericResult<T> : IGenericResult
{
    T Value { get; }
    IGenericResult<TNew> Map<TNew>(Func<T, TNew> mapper);
    TResult Match<TResult>(Func<T, TResult> success, Func<string, TResult> failure);
}

// Factory methods
GenericResult.Success(string message)
GenericResult.Failure(string message)
GenericResult<T>.Success(T value, string message)
GenericResult<T>.Failure<T>(string message)
```

**Usage Example**:
```csharp
public IGenericResult<User> GetUser(int id)
{
    if (id <= 0)
        return GenericResult<User>.Failure("Invalid user ID");

    var user = _repository.GetById(id);
    if (user == null)
        return GenericResult<User>.Failure($"User {id} not found");

    return GenericResult<User>.Success(user, "User retrieved successfully");
}

// Functional composition
var result = GetUser(123)
    .Map(user => user.Email)
    .Match(
        success: email => $"Email: {email}",
        failure: error => $"Error: {error}"
    );
```

---

### FractalDataWorks.Configuration.Abstractions

**Purpose**: Configuration contracts

**Target Framework**: netstandard2.0

**Key Types**:
- `IGenericConfiguration` - Base configuration interface
- Configuration validation interfaces

**Dependencies**: None

**Usage Example**:
```csharp
public interface IEmailConfiguration : IGenericConfiguration
{
    string SmtpServer { get; }
    int Port { get; }
    bool EnableSsl { get; }
}
```

---

### FractalDataWorks.Configuration

**Purpose**: Configuration implementation with validation

**Target Framework**: net10.0

**Key Types**:
- `ConfigurationBase<T>` - Base class for configurations
- FluentValidation integration

**Dependencies**:
- FractalDataWorks.Configuration.Abstractions
- FractalDataWorks.Results
- FluentValidation

**Usage Example**:
```csharp
public class EmailConfiguration : ConfigurationBase<EmailConfiguration>, IEmailConfiguration
{
    public string SmtpServer { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public bool EnableSsl { get; init; } = true;

    public override IGenericResult<ValidationResult> Validate()
    {
        var validator = new EmailConfigurationValidator();
        return GenericResult<ValidationResult>.Success(validator.Validate(this));
    }
}
```

---

## Service Core Layer

### FractalDataWorks.ServiceTypes

**Purpose**: Service type discovery and registration

**Target Framework**: net10.0

**Key Types**:
- `ServiceTypeBase<TService, TFactory, TConfiguration>` - Base class for service types
- `ServiceTypeCollectionBase<...>` - Base class for service type collections
- `ServiceTypeCollectionAttribute` - Marks collections for generation
- `ServiceTypeOptionAttribute` - Marks service types for inclusion

**Dependencies**:
- FractalDataWorks.EnhancedEnums
- FractalDataWorks.Collections
- FractalDataWorks.Configuration.Abstractions
- Microsoft.Extensions.DependencyInjection.Abstractions

**Public API Surface**:
```csharp
public abstract class ServiceTypeBase<TService, TFactory, TConfiguration>
    where TService : class
    where TFactory : class
    where TConfiguration : class
{
    public int Id { get; }
    public string Name { get; }
    public string SectionName { get; }
    public string DisplayName { get; }
    public string Description { get; }

    public abstract Type ServiceType { get; }
    public abstract Type ConfigurationType { get; }
    public abstract Type? FactoryType { get; }

    public abstract void Register(IServiceCollection services);
    public abstract void Configure(IConfiguration configuration);
}
```

**Usage Example**:
```csharp
[ServiceTypeCollection(typeof(EmailServiceTypeBase<,,>), typeof(IEmailServiceType), typeof(EmailServiceTypes))]
public partial class EmailServiceTypes : ServiceTypeCollectionBase<...>
{
}

[ServiceTypeOption(typeof(EmailServiceTypes), "SmtpEmail")]
public sealed class SmtpEmailServiceType : EmailServiceTypeBase<IEmailService, EmailConfiguration, IEmailServiceFactory>
{
    public SmtpEmailServiceType() : base(1, "SmtpEmail", "Services:Email:Smtp",
        "SMTP Email Service", "Email service using SMTP protocol") { }

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IEmailServiceFactory, SmtpEmailServiceFactory>();
    }
}

// Usage
foreach (var serviceType in EmailServiceTypes.All())
{
    serviceType.Register(services);
}
```

---

### FractalDataWorks.Services.Abstractions

**Purpose**: Service contracts and command interfaces

**Target Framework**: netstandard2.0

**Key Types**:
- `ICommand` - Base command interface
- `IGenericService<TCommand, TConfiguration>` - Generic service interface
- `IGenericServiceProvider` - Service provider interface
- `IServiceFactory<TService, TConfiguration>` - Factory interface

**Dependencies**:
- FractalDataWorks.Abstractions
- FractalDataWorks.Results.Abstractions
- FractalDataWorks.Configuration.Abstractions

**Public API Surface**:
```csharp
public interface IGenericService<TCommand, TConfiguration>
    where TCommand : ICommand
    where TConfiguration : IGenericConfiguration
{
    Task<IGenericResult> Execute(TCommand command, CancellationToken cancellationToken = default);
    Task<IGenericResult<T>> Execute<T>(TCommand command, CancellationToken cancellationToken = default);
}
```

---

### FractalDataWorks.Services

**Purpose**: Service base classes and lifecycle management

**Target Framework**: net10.0

**Key Types**:
- `ServiceBase<TCommand, TConfiguration, TService>` - Abstract base for services
- `ServiceFactoryBase<TService, TConfiguration>` - Base factory with validation
- `GenericServiceFactory<TService, TConfiguration>` - Generic factory implementation

**Dependencies**:
- FractalDataWorks.Services.Abstractions
- FractalDataWorks.ServiceTypes
- FractalDataWorks.Configuration
- FractalDataWorks.Results
- FractalDataWorks.Messages
- FastGenericNew (for high-performance instantiation)
- Microsoft.Extensions.Logging.Abstractions

**Public API Surface**:
```csharp
public abstract class ServiceBase<TCommand, TConfiguration, TService>
    where TCommand : ICommand
    where TConfiguration : IGenericConfiguration
    where TService : class
{
    protected ILogger<TService> Logger { get; }
    protected TConfiguration Configuration { get; }

    protected ServiceBase(ILogger<TService> logger, TConfiguration configuration);

    public abstract Task<IGenericResult> Execute(TCommand command, CancellationToken cancellationToken = default);
    public abstract Task<IGenericResult<T>> Execute<T>(TCommand command, CancellationToken cancellationToken = default);
}
```

**Usage Example**:
```csharp
public class EmailService : ServiceBase<IEmailCommand, EmailConfiguration, EmailService>
{
    private readonly ISmtpClient _smtpClient;

    public EmailService(ILogger<EmailService> logger, EmailConfiguration configuration, ISmtpClient smtpClient)
        : base(logger, configuration)
    {
        _smtpClient = smtpClient;
    }

    public override async Task<IGenericResult> Execute(IEmailCommand command, CancellationToken cancellationToken = default)
    {
        if (command is SendEmailCommand sendCmd)
        {
            await _smtpClient.SendAsync(sendCmd.To, sendCmd.Subject, sendCmd.Body);
            return GenericResult.Success("Email sent successfully");
        }

        return GenericResult.Failure($"Unknown command type: {command.GetType().Name}");
    }
}
```

---

## Domain Services Layer

### FractalDataWorks.Services.Authentication.Abstractions

**Purpose**: Authentication service contracts

**Target Framework**: netstandard2.0

**Key Types**:
- `IAuthenticationCommand` - Base authentication command
- `IAuthenticationService` - Authentication service interface
- `IAuthenticationConfiguration` - Configuration interface

**Dependencies**:
- FractalDataWorks.Services.Abstractions

---

### FractalDataWorks.Services.Authentication

**Purpose**: Authentication service infrastructure

**Target Framework**: net10.0

**Key Types**:
- `AuthenticationServiceBase` - Base class for auth services
- `AuthenticationTypes` - Service type collection

**Dependencies**:
- FractalDataWorks.Services.Authentication.Abstractions
- FractalDataWorks.Services

---

### FractalDataWorks.Services.Authentication.Entra

**Purpose**: Azure AD (Entra ID) authentication implementation

**Target Framework**: net10.0

**Key Types**:
- `EntraAuthenticationService` - Entra authentication implementation
- `EntraAuthenticationConfiguration` - Entra-specific configuration

**Dependencies**:
- FractalDataWorks.Services.Authentication
- Azure.Identity
- Microsoft.Graph

---

### FractalDataWorks.Services.Connections.Abstractions

**Purpose**: Database and external connection contracts

**Target Framework**: netstandard2.0

**Key Types**:
- `IConnectionInfo` - Connection metadata
- `IGenericConnection` - Generic connection interface
- `IConnectionConfiguration` - Connection configuration
- `IDataProviderFactory<T>` - Provider factory interface

**Dependencies**:
- FractalDataWorks.Services.Abstractions

---

### FractalDataWorks.Services.Connections

**Purpose**: Connection service infrastructure

**Target Framework**: net10.0

**Key Types**:
- `ConnectionTypeBase` - Base class for connection types
- `ConnectionTypes` - Service type collection

**Dependencies**:
- FractalDataWorks.Services.Connections.Abstractions
- FractalDataWorks.Services

---

### FractalDataWorks.Services.Connections.MsSql

**Purpose**: SQL Server connection implementation

**Target Framework**: net10.0

**Key Types**:
- `SqlServerConnection` - SQL Server connection
- `SqlServerConnectionInfo` - Connection metadata
- `SqlServerDataProvider` - Data provider

**Dependencies**:
- FractalDataWorks.Services.Connections
- Microsoft.Data.SqlClient
- Dapper

**Usage Example**:
```csharp
var config = new SqlServerConnectionConfiguration
{
    ConnectionString = "Server=localhost;Database=MyDb;Trusted_Connection=true;",
    CommandTimeout = 30
};

var connection = new SqlServerConnection(logger, config);
var result = await connection.Execute(new QueryCommand("SELECT * FROM Users"));
```

---

### FractalDataWorks.Services.Connections.Rest

**Purpose**: REST API connection implementation

**Target Framework**: net10.0

**Key Types**:
- `RestConnection` - REST client connection
- `RestConnectionConfiguration` - REST-specific configuration

**Dependencies**:
- FractalDataWorks.Services.Connections
- System.Net.Http

---

### FractalDataWorks.Services.Data.Abstractions

**Purpose**: Data access service contracts

**Target Framework**: netstandard2.0

**Key Types**:
- `IDataCommand` - Base data command
- `IDataService` - Data service interface

**Dependencies**:
- FractalDataWorks.Services.Abstractions

---

### FractalDataWorks.Services.DataGateway.Abstractions

**Purpose**: Data gateway pattern contracts

**Target Framework**: netstandard2.0

**Key Types**:
- `IDataGateway` - Gateway interface
- `IDataGatewayCommand` - Gateway command interface

**Dependencies**:
- FractalDataWorks.Services.Abstractions

---

### FractalDataWorks.Services.DataGateway

**Purpose**: Data gateway pattern implementation

**Target Framework**: net10.0

**Key Types**:
- `DataGatewayBase` - Base gateway implementation

**Dependencies**:
- FractalDataWorks.Services.DataGateway.Abstractions
- FractalDataWorks.Services

---

### FractalDataWorks.Services.Execution.Abstractions

**Purpose**: Execution pipeline contracts

**Target Framework**: netstandard2.0

**Key Types**:
- `IExecutionCommand` - Execution command interface
- `IExecutionService` - Execution service interface

**Dependencies**:
- FractalDataWorks.Services.Abstractions

---

### FractalDataWorks.Services.Execution

**Purpose**: Execution pipeline implementation

**Target Framework**: net10.0

**Key Types**:
- `ExecutionServiceBase` - Base execution service
- `ExecutionPipeline` - Pipeline orchestration

**Dependencies**:
- FractalDataWorks.Services.Execution.Abstractions
- FractalDataWorks.Services

---

### FractalDataWorks.Services.Scheduling.Abstractions

**Purpose**: Job scheduling contracts

**Target Framework**: netstandard2.0

**Key Types**:
- `ISchedulingCommand` - Scheduling command
- `ISchedulingService` - Scheduler service interface

**Dependencies**:
- FractalDataWorks.Services.Abstractions

---

### FractalDataWorks.Services.Scheduling

**Purpose**: Job scheduling implementation

**Target Framework**: net10.0

**Key Types**:
- `SchedulingServiceBase` - Base scheduler service

**Dependencies**:
- FractalDataWorks.Services.Scheduling.Abstractions
- FractalDataWorks.Services

---

### FractalDataWorks.Services.SecretManagers.Abstractions

**Purpose**: Secret management contracts

**Target Framework**: netstandard2.0

**Key Types**:
- `ISecretManagerCommand` - Secret command interface
- `ISecretManagerService` - Secret manager interface

**Dependencies**:
- FractalDataWorks.Services.Abstractions

---

### FractalDataWorks.Services.SecretManagers

**Purpose**: Secret management infrastructure

**Target Framework**: net10.0

**Key Types**:
- `SecretManagerBase` - Base secret manager

**Dependencies**:
- FractalDataWorks.Services.SecretManagers.Abstractions
- FractalDataWorks.Services

---

### FractalDataWorks.Services.SecretManagers.AzureKeyVault

**Purpose**: Azure Key Vault secret management

**Target Framework**: net10.0

**Key Types**:
- `AzureKeyVaultSecretManager` - Key Vault implementation

**Dependencies**:
- FractalDataWorks.Services.SecretManagers
- Azure.Security.KeyVault.Secrets
- Azure.Identity

---

### FractalDataWorks.Services.Transformations.Abstractions

**Purpose**: Data transformation contracts

**Target Framework**: netstandard2.0

**Key Types**:
- `ITransformationCommand` - Transformation command
- `ITransformationService` - Transformation service interface

**Dependencies**:
- FractalDataWorks.Services.Abstractions

---

### FractalDataWorks.Services.Transformations

**Purpose**: Data transformation implementation

**Target Framework**: net10.0

**Key Types**:
- `TransformationServiceBase` - Base transformation service

**Dependencies**:
- FractalDataWorks.Services.Transformations.Abstractions
- FractalDataWorks.Services

---

## Web Integration Layer

### FractalDataWorks.Web.Http.Abstractions

**Purpose**: HTTP abstractions and contracts

**Target Framework**: netstandard2.0

**Key Types**:
- `IHttpCommand` - HTTP command interface
- `ISecurityMethod` - Security method interface
- `SecurityMethods` - Security method collection

**Dependencies**:
- FractalDataWorks.Services.Abstractions
- FractalDataWorks.Collections

**Usage Example**:
```csharp
// Get security method
var jwt = SecurityMethods.Jwt;
var apiKey = SecurityMethods.ApiKey;

// Configure endpoint security
var endpoint = new EndpointConfiguration
{
    Path = "/api/users",
    SecurityMethod = SecurityMethods.GetByName("JWT")
};
```

---

### FractalDataWorks.Web.RestEndpoints

**Purpose**: REST endpoint utilities

**Target Framework**: net10.0

**Key Types**:
- `RestEndpointBuilder` - Fluent API for REST endpoints
- `EndpointConfiguration` - Endpoint configuration

**Dependencies**:
- FractalDataWorks.Web.Http.Abstractions
- Microsoft.AspNetCore.Http

---

## Development Tools Layer

### FractalDataWorks.EnhancedEnums.SourceGenerators

**Purpose**: Source generator for enhanced enum collections

**Target Framework**: netstandard2.0

**Key Features**:
- Generates static collections from EnumOptionBase<T> types
- Supports cross-assembly discovery with GlobalEnumCollection
- ILRepack for dependency merging

**Dependencies**:
- FractalDataWorks.CodeBuilder.CSharp
- FractalDataWorks.EnhancedEnums
- Microsoft.CodeAnalysis.CSharp
- ILRepack.Lib.MSBuild.Task

**Generated Code**:
- Static `_all` field population
- `All()` method
- `GetById(int)` and `GetByName(string)` methods
- Factory methods

---

### FractalDataWorks.EnhancedEnums.Analyzers

**Purpose**: Code analyzers for enhanced enums

**Target Framework**: netstandard2.0

**Key Diagnostics**:
- EE001: EnumOption must inherit from EnumOptionBase
- EE002: EnumCollection must have matching generic parameters
- EE003: Duplicate enum option IDs detected

**Dependencies**:
- FractalDataWorks.CodeBuilder.Abstractions
- Microsoft.CodeAnalysis.CSharp

---

### FractalDataWorks.EnhancedEnums.CodeFixes

**Purpose**: Automatic code fixes for enhanced enums

**Target Framework**: netstandard2.0

**Key Fixes**:
- Auto-correct inheritance hierarchy
- Fix generic parameter mismatches
- Resolve duplicate ID conflicts

**Dependencies**:
- FractalDataWorks.EnhancedEnums.Analyzers
- Microsoft.CodeAnalysis.CSharp

---

### FractalDataWorks.Collections.SourceGenerators

**Purpose**: Source generator for type collections

**Target Framework**: netstandard2.0

**Key Features**:
- Generates collections with O(1) lookups
- Explicit targeting via [TypeOption] attribute
- FrozenDictionary/FrozenSet on .NET 8+

**Dependencies**:
- FractalDataWorks.CodeBuilder.CSharp
- FractalDataWorks.Collections
- Microsoft.CodeAnalysis.CSharp

**Generated Code**:
- Static properties for each type
- `All()` returning FrozenSet<T>
- `GetById(int)` and `GetByName(string)` with FrozenDictionary
- Factory methods with constructor overloads

---

### FractalDataWorks.Collections.Analyzers

**Purpose**: Code analyzers for type collections

**Target Framework**: netstandard2.0

**Key Diagnostics**:
- TC001: Type missing [TypeOption] attribute
- TC002: TGeneric mismatch with defaultReturnType
- TC003: TBase mismatch with baseType
- TC004: Duplicate option names

---

### FractalDataWorks.Collections.CodeFixes

**Purpose**: Automatic code fixes for collections

**Target Framework**: netstandard2.0

**Key Fixes**:
- Add missing [TypeOption] attributes
- Fix generic parameter mismatches
- Resolve naming conflicts

---

### FractalDataWorks.ServiceTypes.SourceGenerators

**Purpose**: Source generator for service type collections

**Target Framework**: netstandard2.0

**Key Features**:
- Generates service type collections
- DI registration methods
- Factory method overloads

**Dependencies**:
- FractalDataWorks.CodeBuilder.CSharp
- FractalDataWorks.ServiceTypes
- Microsoft.CodeAnalysis.CSharp

**Generated Code**:
- `All()` collection
- `GetByName(string)` and `GetById(int)`
- `RegisterAll(IServiceCollection)`
- Factory methods for each service type

---

### FractalDataWorks.ServiceTypes.Analyzers

**Purpose**: Code analyzers for service types

**Target Framework**: netstandard2.0

**Key Diagnostics**:
- ST001: ServiceType missing required methods
- ST002: Invalid generic constraints
- ST003: Missing DI registration

---

### FractalDataWorks.ServiceTypes.CodeFixes

**Purpose**: Automatic code fixes for service types

**Target Framework**: netstandard2.0

**Key Fixes**:
- Add missing Register() method
- Fix generic constraints
- Add DI registration

---

### FractalDataWorks.Messages.SourceGenerators

**Purpose**: Source generator for message collections

**Target Framework**: netstandard2.0

**Key Features**:
- Generates message collections
- Factory methods for each message type

**Dependencies**:
- FractalDataWorks.CodeBuilder.CSharp
- FractalDataWorks.Messages
- Microsoft.CodeAnalysis.CSharp

**Generated Code**:
- Static message collection
- Factory methods
- Message lookup methods

---

## MCP Tools Layer

### FractalDataWorks.MCP.Abstractions

**Purpose**: Model Context Protocol base contracts

**Target Framework**: net10.0

**Key Types**:
- `IMcpTool` - Base tool interface
- `IMcpToolProvider` - Tool provider interface

**Dependencies**:
- FractalDataWorks.Services.Abstractions

---

### FractalDataWorks.McpTools.Abstractions

**Purpose**: MCP tool abstractions

**Target Framework**: net10.0

**Key Types**:
- Tool category interfaces
- Tool metadata types

**Dependencies**:
- FractalDataWorks.MCP.Abstractions

---

### FractalDataWorks.McpTools.CodeAnalysis

**Purpose**: Code analysis tools for MCP

**Target Framework**: net10.0

**Key Tools**:
- `GetDiagnosticsTool` - Get compiler diagnostics
- `GetFileDiagnosticsTool` - File-specific diagnostics
- `AnalyzeCodeTool` - Code quality analysis

**Dependencies**:
- FractalDataWorks.McpTools.Abstractions
- Microsoft.CodeAnalysis

---

### FractalDataWorks.McpTools.Refactoring

**Purpose**: Code refactoring tools for MCP

**Target Framework**: net10.0

**Key Tools**:
- `RenameSymbolTool` - Rename symbols
- `ExtractMethodTool` - Extract method refactoring
- `ApplyCodeFixTool` - Apply code fixes

**Dependencies**:
- FractalDataWorks.McpTools.Abstractions
- Microsoft.CodeAnalysis.CSharp

---

### FractalDataWorks.McpTools.SessionManagement

**Purpose**: MCP session lifecycle management

**Target Framework**: net10.0

**Key Tools**:
- `StartSessionTool` - Initialize session
- `EndSessionTool` - Terminate session
- `PauseSessionTool` - Pause session
- `ResumeSessionTool` - Resume session

**Dependencies**:
- FractalDataWorks.McpTools.Abstractions

---

### FractalDataWorks.McpTools.VirtualEditing

**Purpose**: Non-destructive code editing

**Target Framework**: net10.0

**Key Tools**:
- `ApplyVirtualEditTool` - Apply virtual edit
- `CommitChangesTool` - Commit changes
- `RollbackChangesTool` - Rollback changes
- `GetPendingChangesTool` - View pending changes

**Dependencies**:
- FractalDataWorks.McpTools.Abstractions
- Microsoft.CodeAnalysis

---

### FractalDataWorks.McpTools.TypeAnalysis

**Purpose**: Type system analysis tools

**Target Framework**: net10.0

**Key Tools**:
- `GetTypeDetailsTool` - Type information
- `SearchTypesTool` - Type search
- `FindDuplicateTypesTool` - Find duplicates

**Dependencies**:
- FractalDataWorks.McpTools.Abstractions
- Microsoft.CodeAnalysis

---

### FractalDataWorks.McpTools.ProjectDependencies

**Purpose**: Project dependency analysis

**Target Framework**: net10.0

**Key Tools**:
- `GetProjectDependenciesTool` - Get dependencies
- `GetImpactAnalysisTool` - Impact analysis
- `GetCompilationOrderTool` - Build order

**Dependencies**:
- FractalDataWorks.McpTools.Abstractions

---

### FractalDataWorks.McpTools.ServerManagement

**Purpose**: MCP server lifecycle management

**Target Framework**: net10.0

**Key Tools**:
- `GetServerInfoTool` - Server information
- `ShutdownServerTool` - Graceful shutdown
- `GetCacheStatsTool` - Cache statistics

**Dependencies**:
- FractalDataWorks.McpTools.Abstractions

---

## Data Layer

### FractalDataWorks.Data

**Purpose**: Data access utilities

**Target Framework**: net10.0

**Key Types**:
- Data mapping utilities
- Connection helpers

**Dependencies**:
- FractalDataWorks.Services.Data.Abstractions

---

### FractalDataWorks.DataContainers.Abstractions

**Purpose**: Data container contracts

**Target Framework**: netstandard2.0

**Key Types**:
- `IDataContainer` - Container interface

**Dependencies**:
- FractalDataWorks.Abstractions

---

### FractalDataWorks.DataContainers

**Purpose**: Data container implementations

**Target Framework**: net10.0

**Key Types**:
- `CsvDataContainer` - CSV data handling
- `JsonDataContainer` - JSON data handling
- `XmlDataContainer` - XML data handling

**Dependencies**:
- FractalDataWorks.DataContainers.Abstractions

---

### FractalDataWorks.DataSets.Abstractions

**Purpose**: Dataset contracts

**Target Framework**: netstandard2.0

**Key Types**:
- `IDataSet` - Dataset interface

**Dependencies**:
- FractalDataWorks.Abstractions

---

### FractalDataWorks.DataStores.Abstractions

**Purpose**: Data store contracts

**Target Framework**: netstandard2.0

**Key Types**:
- `IDataStore` - Store interface

**Dependencies**:
- FractalDataWorks.Abstractions

---

### FractalDataWorks.DataStores

**Purpose**: Data store implementations

**Target Framework**: net10.0

**Key Types**:
- File-based data stores
- Memory data stores

**Dependencies**:
- FractalDataWorks.DataStores.Abstractions

---

## Templates

### FractalDataWorks.Service Template

**Purpose**: Create new service implementation

**Type**: dotnet template

**Usage**:
```bash
dotnet new FractalDataWorks-service -n MyCompany.Services.MyService
```

**Generated Structure**:
- Abstractions project (netstandard2.0)
- Core project (net10.0)
- ServiceType and provider setup

---

### FractalDataWorks.Service.Domain Template

**Purpose**: Create new service domain

**Type**: dotnet template

**Usage**:
```bash
dotnet new FractalDataWorks-service-domain -n MyCompany.Services.MyDomain
```

**Generated Structure**:
- Abstractions project with command interfaces
- Core project with ServiceType collection
- Base classes and provider pattern

---

## Test Projects

### FractalDataWorks.Collections.Tests

**Purpose**: Tests for Collections framework

**Target Framework**: net10.0

**Test Framework**: xUnit v3

**Key Test Areas**:
- TypeCollection generation
- Lookup performance
- Cross-assembly discovery

---

### FractalDataWorks.EnhancedEnums.Tests

**Purpose**: Tests for Enhanced Enums

**Target Framework**: net10.0

**Test Framework**: xUnit v3

**Key Test Areas**:
- Enum collection generation
- Cross-assembly discovery
- Lookup methods

---

### FractalDataWorks.Messages.Tests

**Purpose**: Tests for Messages framework

**Target Framework**: net10.0

**Test Framework**: xUnit v3

**Key Test Areas**:
- Message collection generation
- Message severity handling
- Factory methods

---

### FractalDataWorks.Results.Tests

**Purpose**: Tests for Results pattern

**Target Framework**: net10.0

**Test Framework**: xUnit v3

**Key Test Areas**:
- Result creation and validation
- Map/Match functionality
- Message integration

---

### FractalDataWorks.ServiceTypes.Tests

**Purpose**: Tests for ServiceTypes framework

**Target Framework**: net10.0

**Test Framework**: xUnit v3

**Key Test Areas**:
- Service type discovery
- DI registration
- Factory methods

---

### FractalDataWorks.Services.Tests

**Purpose**: Tests for Services framework

**Target Framework**: net10.0

**Test Framework**: xUnit v3

**Key Test Areas**:
- Service lifecycle
- Command execution
- Configuration validation

---

### FractalDataWorks.MCP.Tests

**Purpose**: Tests for MCP tools

**Target Framework**: net10.0

**Test Framework**: xUnit v3

**Key Test Areas**:
- Tool execution
- Session management
- Virtual editing

---

## Summary

The FractalDataWorks Developer Kit consists of **64 projects** organized into:

- **7 Foundation projects** - Core abstractions and code builders
- **12 Infrastructure projects** - Enhanced enums, collections, messages, results, configuration
- **4 Service Core projects** - ServiceTypes and Services
- **23 Domain Service projects** - Authentication, Connections, Data, Execution, Scheduling, Secrets, Transformations
- **3 Web Integration projects** - HTTP abstractions and REST endpoints
- **12 Development Tools projects** - Source generators, analyzers, and code fixes
- **8 MCP Tools projects** - AI-assisted development tooling
- **7 Data Layer projects** - Data containers, sets, and stores
- **2 Template projects** - dotnet new templates
- **7 Test projects** - Comprehensive test coverage

All projects follow consistent patterns for dependency injection, configuration management, Railway-Oriented Programming with results, and source generation for type safety and performance.

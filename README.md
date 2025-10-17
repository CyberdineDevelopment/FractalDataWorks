# FractalDataWorks Developer Kit

A comprehensive .NET toolkit for building enterprise applications with Railway-Oriented Programming, type-safe plugin architectures, and zero-overhead source generation.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-preview-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](LICENSE)

## Overview

The FractalDataWorks Developer Kit provides foundational infrastructure for enterprise .NET applications, emphasizing:

- **Railway-Oriented Programming** - Explicit error handling with `IGenericResult<T>` instead of exceptions
- **Type-Safe Plugin Architecture** - Extensible systems via TypeCollections and ServiceTypes
- **Source Generation** - Zero-runtime-cost abstractions with Roslyn source generators
- **High Performance** - O(1) lookups with FrozenDictionary, minimal allocations
- **Cross-Assembly Discovery** - Plugin types discovered at compile-time across assemblies

## Key Features

### 🚂 Railway-Oriented Programming
Explicit error handling without exceptions:

```csharp
using System.Threading.Tasks;
using FractalDataWorks.Results;

public IGenericResult<User> GetUser(int id)
{
    if (id <= 0)
        return GenericResult<User>.Failure("Invalid user ID");

    var user = await _repository.GetById(id);
    return user != null
        ? GenericResult<User>.Success(user)
        : GenericResult<User>.Failure($"User {id} not found");
}

// Compositional error handling
var email = await GetUser(123)
    .Map(user => user.Email)
    .Match(
        success: email => $"Email: {email}",
        failure: error => $"Error: {error}"
    );
```

### 🔌 TypeCollections (Plugin Architecture)
Cross-project type discovery enabling downstream extensibility:

```csharp
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

// Abstractions project - collection
[TypeCollection(typeof(SecurityMethodBase), typeof(ISecurityMethod), typeof(SecurityMethods))]
public partial class SecurityMethods : TypeCollectionBase<SecurityMethodBase, ISecurityMethod> { }

// Concrete project - base type
public abstract class SecurityMethodBase : ISecurityMethod { }

// Any project (even downstream!) - type option
[TypeOption(typeof(SecurityMethods), "CustomAuth")]
public sealed class CustomAuthMethod : SecurityMethodBase { }

// Usage with O(1) lookups
var all = SecurityMethods.All();           // FrozenSet<ISecurityMethod>
var jwt = SecurityMethods.Jwt;             // Static property (generated)
var custom = SecurityMethods.GetByName("CustomAuth");  // O(1) lookup
```

### 🎯 ServiceTypes (Service Plugin Framework)
High-performance service registration with progressive generic constraints:

```csharp
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;

// Service type with dual inheritance pattern
[ServiceTypeOption(typeof(ConnectionTypes), "PostgreSql")]
public sealed class PostgreSqlConnectionType :
    ConnectionTypeBase<IGenericConnection, PostgreSqlConfiguration, IPostgreSqlConnectionFactory>,
    IConnectionType<IGenericConnection, PostgreSqlConfiguration, IPostgreSqlConnectionFactory>
{
    public static PostgreSqlConnectionType Instance { get; } = new();

    private PostgreSqlConnectionType() : base(id: 3, name: "PostgreSql", category: "Database") { }

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IPostgreSqlConnectionFactory, PostgreSqlConnectionFactory>();
        services.AddScoped<PostgreSqlService>();
    }
}

// Automatic registration
ConnectionTypes.RegisterAll(services);
```

### ✨ EnhancedEnums
Type-safe, extensible enums with rich behavior:

```csharp
using System.Net;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

// Basic enhanced enum
public sealed class Priority : EnumOptionBase<Priority>
{
    public static readonly Priority Low = new(1, "Low", urgencyLevel: 1);
    public static readonly Priority High = new(3, "High", urgencyLevel: 10);

    public int UrgencyLevel { get; }

    private Priority(int id, string name, int urgencyLevel) : base(id, name)
    {
        UrgencyLevel = urgencyLevel;
    }
}

// Extended enums wrap existing C# enums
[ExtendEnum(typeof(HttpStatusCode))]
public sealed class EnhancedHttpStatusCode : ExtendedEnumOptionBase<EnhancedHttpStatusCode, HttpStatusCode>
{
    public bool IsSuccess => (int)EnumValue >= 200 && (int)EnumValue < 300;
    public bool IsClientError => (int)EnumValue >= 400 && (int)EnumValue < 500;
}
```

### 📦 Universal Data Commands
Single API for data access across any backend (SQL, REST, Files, GraphQL):

```csharp
using System.Threading.Tasks;
using FractalDataWorks.Commands.Data;

// Write once
var command = new QueryCommand<Customer>("Customers")
{
    Filter = new FilterExpression
    {
        Conditions =
        [
            new FilterCondition
            {
                PropertyName = nameof(Customer.IsActive),
                Operator = FilterOperators.Equal,
                Value = true
            }
        ]
    },
    Paging = new PagingExpression { Skip = 0, Take = 50 }
};

// Execute against any backend via translators
var sqlResult = await sqlConnection.ExecuteAsync(command);    // → T-SQL
var restResult = await restConnection.ExecuteAsync(command);  // → OData
var fileResult = await fileConnection.ExecuteAsync(command);  // → JSON filtering
```

## Architecture

### Core Framework Components

```
Foundation Layer (netstandard2.0)
├── Abstractions              # Core interfaces and base types
├── Results                   # Railway-oriented programming
├── Messages                  # Structured messaging
└── Configuration.Abstractions # Configuration contracts

Type System Layer (netstandard2.0;net10.0)
├── Collections               # TypeCollection framework
├── EnhancedEnums            # EnhancedEnum base classes
└── ServiceTypes             # ServiceType plugin framework

Source Generation Layer (netstandard2.0)
├── Collections.SourceGenerators    # TypeCollection code generation
├── EnhancedEnums.SourceGenerators  # EnhancedEnum code generation
└── ServiceTypes.SourceGenerators   # ServiceType code generation

Code Quality Layer (netstandard2.0)
├── Collections.Analyzers     # Compile-time validation
├── EnhancedEnums.Analyzers   # Rule enforcement
└── ServiceTypes.Analyzers    # Pattern compliance

Service Infrastructure Layer (net10.0)
├── Services                  # Service base classes
├── Services.Abstractions     # Service contracts
├── Services.Execution        # Execution pipeline
├── Services.Connections      # Connection management
├── Services.Scheduling       # Job scheduling
├── Services.Authentication   # Authentication services
├── Services.SecretManagers   # Secret management
└── Services.Transformations  # Data transformations

Data Layer (netstandard2.0;net10.0)
├── Commands.Data.Abstractions # Universal command interfaces
├── Commands.Data             # Command implementations
├── Data.DataStores          # DataStore abstractions
└── Data.DataSets            # DataSet implementations (planned)

Utilities Layer
├── CodeBuilder              # Code generation utilities
└── DependencyInjection      # DI extensions
```

### Project Organization

Projects follow consistent naming patterns:
- `*.Abstractions` - Interfaces and contracts only
- `*.SourceGenerators` - Roslyn source generators
- `*.Analyzers` - Compile-time code analysis
- `*.CodeFixes` - Automatic code fix providers

## Quick Start

### Installation

```bash
# Core packages
dotnet add package FractalDataWorks.Abstractions
dotnet add package FractalDataWorks.Results
dotnet add package FractalDataWorks.Collections

# Source generators (include for code generation)
dotnet add package FractalDataWorks.Collections.SourceGenerators
dotnet add package FractalDataWorks.EnhancedEnums.SourceGenerators

# Service infrastructure
dotnet add package FractalDataWorks.Services
dotnet add package FractalDataWorks.ServiceTypes
```

### Basic Example

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Results;
using FractalDataWorks.Services;

// Define configuration
public interface IEmailConfiguration : IGenericConfiguration
{
    string SmtpServer { get; }
    int Port { get; }
}

// Implement service with Railway-Oriented Programming
public class EmailService : ServiceBase<IEmailCommand, IEmailConfiguration, EmailService>
{
    public async Task<IGenericResult> SendEmail(string to, string subject, string body)
    {
        if (string.IsNullOrEmpty(to))
            return GenericResult.Failure("Recipient email is required");

        try
        {
            await _smtpClient.SendAsync(to, subject, body);
            return GenericResult.Success("Email sent successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to send email");
            return GenericResult.Failure($"Failed to send email: {ex.Message}");
        }
    }
}
```

## Building

### Requirements

- .NET 10.0 SDK (or .NET 8.0+ for netstandard2.0 projects)
- C# preview language features enabled
- Visual Studio 2024, Rider 2024.3, or VS Code with C# extension

### Build Commands

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Build with specific configuration
dotnet build -c Debug      # Fast iteration, minimal analyzers
dotnet build -c Alpha      # Basic checks
dotnet build -c Beta       # Full analyzers, warnings as errors
dotnet build -c Release    # Production build

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Pack NuGet packages
dotnet pack -c Release -o ./artifacts
```

### Configuration Levels

The solution uses progressive build configurations (defined in `Directory.Build.props`):

- **Debug** - Fast builds, no analyzers (rapid development)
- **Experimental** - Minimal analyzers (prototyping)
- **Alpha** - Basic checks (early testing)
- **Beta** - Full analyzers, warnings as errors (quality gate)
- **Preview** - Strict checks (release candidate)
- **Release** - Production-ready with all validations

## Testing

Tests use xUnit v3 with Shouldly for assertions:

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/FractalDataWorks.Collections.Tests/

# Run single test
dotnet test --filter "FullyQualifiedName~TypeCollection_GetByName_ReturnsCorrectType"

# Exclude integration tests
dotnet test --filter "Category!=Integration"
```

## Documentation

- **[Architecture Guide](discussions/ARCHITECTURE_SUMMARY.md)** - System architecture and design decisions
- **[TypeCollections Guide](src/FractalDataWorks.Collections/README.md)** - Plugin architecture patterns
- **[ServiceTypes Guide](src/FractalDataWorks.ServiceTypes/README.md)** - Service registration framework
- **[EnhancedEnums Guide](src/FractalDataWorks.EnhancedEnums/README.md)** - Type-safe enum pattern
- **[DataCommands Guide](src/FractalDataWorks.Commands.Data/README.md)** - Universal data access
- **[Results Guide](src/FractalDataWorks.Results/README.md)** - Railway-oriented programming

Each project contains a comprehensive README with implementation details and examples.

## Design Principles

### No Enums, No Switch Statements
Use TypeCollections or EnhancedEnums instead:

```csharp
using FractalDataWorks.Collections.Attributes;

// ❌ Don't use enums
public enum FilterOperator { Equal, NotEqual, GreaterThan }

// ✅ Use TypeCollections
[TypeOption(typeof(FilterOperators), "Equal")]
public sealed class EqualOperator : FilterOperatorBase
{
    public EqualOperator() : base(id: 1, name: "Equal", sqlOperator: "=") { }
}
```

### Railway-Oriented Over Exceptions
Return results instead of throwing for anticipated failures:

```csharp
using System;
using FractalDataWorks.Results;

// ❌ Don't throw for business logic
public User GetUser(int id)
{
    if (id <= 0) throw new ArgumentException("Invalid ID");
    return _repository.Get(id) ?? throw new NotFoundException();
}

// ✅ Return results
public IGenericResult<User> GetUser(int id)
{
    if (id <= 0)
        return GenericResult<User>.Failure("Invalid ID");

    var user = _repository.Get(id);
    return user != null
        ? GenericResult<User>.Success(user)
        : GenericResult<User>.Failure("User not found");
}
```

### Source Generation Over Reflection
Generate code at compile-time:

```csharp
using System.Linq;
using System.Reflection;
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

// ❌ Don't use runtime reflection
var types = Assembly.GetTypes().Where(t => t.IsAssignableTo<IPlugin>());

// ✅ Use source generation
[TypeCollection(typeof(PluginBase), typeof(IPlugin), typeof(Plugins))]
public partial class Plugins : TypeCollectionBase<PluginBase, IPlugin> { }

var all = Plugins.All();  // FrozenSet populated at compile-time
```

## Performance

The framework is optimized for high performance:

- **O(1) Lookups** - FrozenDictionary/FrozenSet for collection operations
- **Zero Reflection** - Source generation eliminates runtime reflection
- **Minimal Allocations** - Value types and pooling where appropriate
- **Compile-Time Safety** - Catch errors at build time, not runtime
- **Fast Service Creation** - FastGenericNew for instantiation

Benchmark results show TypeCollections with FrozenDictionary outperform dictionary and array lookups by 2-3x.

## Contributing

We welcome contributions! Please:

1. Review the [Architecture Guide](discussions/ARCHITECTURE_SUMMARY.md)
2. Follow the patterns in [CLAUDE.md](.claude/CLAUDE.md)
3. Ensure all tests pass
4. Add tests for new functionality
5. Update relevant documentation

### Code Standards

- Target `netstandard2.0` for abstractions
- Multi-target `netstandard2.0;net10.0` for implementations where needed
- Use Railway-Oriented Programming (return `IGenericResult<T>`)
- No enums - use TypeCollections or EnhancedEnums
- No switch statements - use properties and visitor pattern
- Source-generated logging with `[LoggerMessage]`
- Full XML documentation on public APIs

## License

Licensed under the Apache-2.0 License. See [LICENSE](LICENSE) for details.

## Acknowledgments

Built with modern .NET technologies:
- .NET 10.0 / .NET Standard 2.0
- C# preview language features
- Roslyn Source Generators
- xUnit v3
- Shouldly
- FluentValidation

---

**FractalDataWorks Developer Kit** - Building better software through type safety, explicit error handling, and zero-overhead abstractions.

# FractalDataWorks Developer Kit

**A comprehensive .NET framework for building service-oriented applications with Railway-Oriented Programming, advanced source generation, and type-safe extensibility.**

[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)]()
[![C# Version](https://img.shields.io/badge/C%23-12-brightgreen)]()
[![License](https://img.shields.io/badge/license-MIT-green)]()

## Overview

The FractalDataWorks Developer Kit is a modular, extensible framework built on modern .NET principles. It provides a complete foundation for enterprise applications with compile-time safety, runtime performance, and exceptional developer experience.

### Core Features

- **Service-Oriented Architecture** - Plugin-based service framework with command execution and configuration management
- **Railway-Oriented Programming** - Explicit error handling with Result pattern instead of exceptions
- **Enhanced Enums** - Type-safe enum pattern with source-generated collections and cross-assembly discovery
- **Type Collections** - High-performance, extensible type discovery system with O(1) lookups using FrozenDictionary/FrozenSet
- **Source Generators** - Compile-time code generation for collections, messages, and service types
- **Configuration Management** - Strongly-typed configuration with FluentValidation integration
- **Structured Messaging** - Rich messaging system with severity levels and structured logging
- **MCP Integration** - Model Context Protocol tooling for AI-assisted development

## Quick Start

### Installation

```bash
# Core framework packages
dotnet add package FractalDataWorks.Results
dotnet add package FractalDataWorks.Services
dotnet add package FractalDataWorks.Configuration
dotnet add package FractalDataWorks.Collections

# Source generators (required for code generation)
dotnet add package FractalDataWorks.ServiceTypes.SourceGenerators
dotnet add package FractalDataWorks.Collections.SourceGenerators
dotnet add package FractalDataWorks.EnhancedEnums.SourceGenerators
```

### Basic Usage

```csharp
// Define a service with Railway-Oriented Programming
public class UserService : ServiceBase<IUserCommand, UserConfiguration, UserService>
{
    public async Task<IGenericResult<User>> GetUser(int id)
    {
        if (id <= 0)
            return GenericResult<User>.Failure("Invalid user ID");

        var user = await _repository.GetById(id);
        if (user == null)
            return GenericResult<User>.Failure($"User {id} not found");

        return GenericResult<User>.Success(user, "User retrieved successfully");
    }
}

// Use functional composition with Map/Match
var result = await userService.GetUser(123)
    .Map(user => user.Email)
    .Match(
        success: email => $"Email: {email}",
        failure: error => $"Error: {error}"
    );
```

## Documentation

### Getting Started

- [Architecture Overview](docs/Architecture-Overview.md) - High-level architecture and design principles
- [Project Reference](docs/Project-Reference.md) - Complete catalog of all projects with purpose and usage
- [Migration Guide](docs/Migration-Guide.md) - Upgrade guide for breaking changes and deprecated patterns

### Developer Guides

- [Service Developer Guide](docs/Service-Developer-Guide.md) - Creating domain services and implementations
- [Service Developer Reference](docs/Service-Developer-Reference.md) - Complete type reference for service development
- [Source Generator Guide](docs/Source-Generator-Guide.md) - Working with source generators
- [Template Usage Guide](docs/Template-Usage-Guide.md) - Using dotnet new templates
- [Testing Guide](docs/Testing-Guide.md) - Testing strategies and best practices

### Advanced Topics

- [MCP Tools Documentation](docs/MCP-Tools-Guide.md) - AI-assisted development with Model Context Protocol
- [Performance Optimization](docs/Performance-Guide.md) - Optimizing service performance
- [Security Best Practices](docs/Security-Guide.md) - Security patterns and guidelines

## Project Structure

The framework is organized into logical layers:

### Foundation Layer (netstandard2.0)
Core abstractions and base types with minimal dependencies:
- `FractalDataWorks.Abstractions` - Core interfaces and base types
- `FractalDataWorks.CodeBuilder.*` - Code generation infrastructure
- `FractalDataWorks.Services.Abstractions` - Service contracts

### Infrastructure Layer
Fundamental building blocks using modern .NET features:
- `FractalDataWorks.EnhancedEnums` - Type-safe enum pattern
- `FractalDataWorks.Collections` - High-performance type collections with O(1) lookups
- `FractalDataWorks.Messages` - Structured messaging system
- `FractalDataWorks.Results` - Result pattern implementation
- `FractalDataWorks.Configuration` - Configuration management with validation

### Service Core Layer
Service-oriented architecture foundation:
- `FractalDataWorks.ServiceTypes` - Service type discovery and registration
- `FractalDataWorks.Services` - Service base classes and lifecycle management

### Domain Services Layer
Business domain implementations:
- `FractalDataWorks.Services.Authentication.*` - Authentication services
- `FractalDataWorks.Services.Connections.*` - Database and external connections
- `FractalDataWorks.Services.Data.*` - Data access services
- `FractalDataWorks.Services.Execution.*` - Execution pipeline services
- `FractalDataWorks.Services.Scheduling.*` - Job scheduling services
- `FractalDataWorks.Services.SecretManagers.*` - Secret management services
- `FractalDataWorks.Services.Transformations.*` - Data transformation services

### Development Tools Layer
Compile-time code generation and analysis:
- `FractalDataWorks.*.SourceGenerators` - Source generators for each domain
- `FractalDataWorks.*.Analyzers` - Roslyn analyzers for code quality
- `FractalDataWorks.*.CodeFixes` - Automatic code fixes

### MCP Tools Layer
AI-assisted development tooling:
- `FractalDataWorks.McpTools.CodeAnalysis` - Code analysis tools
- `FractalDataWorks.McpTools.Refactoring` - Refactoring tools
- `FractalDataWorks.McpTools.VirtualEditing` - Non-destructive editing
- `FractalDataWorks.McpTools.SessionManagement` - Session lifecycle management

## Key Concepts

### Railway-Oriented Programming

Operations return `IGenericResult` or `IGenericResult<T>` instead of throwing exceptions:

```csharp
public IGenericResult<User> GetUser(int id)
{
    if (id <= 0)
        return GenericResult<User>.Failure("Invalid user ID");

    var user = _repository.GetById(id);
    if (user == null)
        return GenericResult<User>.Failure($"User {id} not found");

    return GenericResult<User>.Success(user);
}
```

**Benefits:**
- Explicit error handling in method signatures
- No hidden control flow from exceptions
- Composable with Map/Match methods
- Performance benefits (no exception overhead)

### Source Generation Over Reflection

Generate code at compile-time instead of using runtime reflection:

```csharp
[TypeCollection(typeof(SecurityMethodBase), typeof(ISecurityMethod), typeof(SecurityMethods))]
public partial class SecurityMethods : TypeCollectionBase<SecurityMethodBase, ISecurityMethod>
{
    // Source generator creates:
    // - Static properties for each type
    // - All() method returning FrozenSet<ISecurityMethod>
    // - GetById(int) and GetByName(string) with O(1) lookups
    // - Factory methods with all constructor overloads
}

[TypeOption(typeof(SecurityMethods), "JWT")]
public sealed class JwtSecurityMethod : SecurityMethodBase
{
    public JwtSecurityMethod() : base(1, "JWT", true) { }
}

// Usage with IntelliSense support
var jwt = SecurityMethods.Jwt;  // Static property generated
var all = SecurityMethods.All();  // FrozenSet<ISecurityMethod>
var byName = SecurityMethods.GetByName("JWT");  // O(1) lookup
```

**Benefits:**
- Zero runtime reflection overhead
- Compile-time type safety
- IntelliSense support for generated code
- Better performance with pre-computed lookups

### Plugin Architecture

Framework extensibility without modifying core code:

```csharp
// In Abstractions project
[ServiceTypeCollection(typeof(EmailServiceTypeBase<,,>), typeof(IEmailServiceType), typeof(EmailServiceTypes))]
public partial class EmailServiceTypes : ServiceTypeCollectionBase<...>
{
}

// In ANY implementation project
[ServiceTypeOption(typeof(EmailServiceTypes), "SmtpEmail")]
public sealed class SmtpEmailServiceType : EmailServiceTypeBase<...>
{
    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IEmailService, SmtpEmailService>();
    }
}

// Automatic discovery and registration
EmailServiceTypes.RegisterAll(services);
```

**Benefits:**
- Open/Closed Principle compliance
- Easy to add new implementations
- No framework modifications required
- Compile-time validation of plugins

## Examples

### Creating a Service

```csharp
// 1. Define configuration
public interface IEmailConfiguration : IGenericConfiguration
{
    string SmtpServer { get; }
    int Port { get; }
    bool EnableSsl { get; }
}

// 2. Create service interface
public interface IEmailService : IGenericService<IEmailCommand, IEmailConfiguration>
{
    Task<IGenericResult> SendEmail(string to, string subject, string body);
}

// 3. Implement service
public class SmtpEmailService : ServiceBase<IEmailCommand, IEmailConfiguration, SmtpEmailService>, IEmailService
{
    public SmtpEmailService(ILogger<SmtpEmailService> logger, IEmailConfiguration configuration)
        : base(logger, configuration)
    {
    }

    public async Task<IGenericResult> SendEmail(string to, string subject, string body)
    {
        try
        {
            // Send email logic
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

### Using Enhanced Enums

```csharp
// Define enum options
public sealed class Priority : EnumOptionBase<Priority>
{
    public static readonly Priority Low = new(1, "Low");
    public static readonly Priority Medium = new(2, "Medium");
    public static readonly Priority High = new(3, "High");

    private Priority(int id, string name) : base(id, name) { }
}

// Source-generated collection
[EnumCollection("PriorityCollection")]
public sealed partial class PriorityCollection : EnumCollectionBase<Priority>
{
    // Generator populates _all and provides methods
}

// Usage
var all = PriorityCollection.All();
var high = PriorityCollection.GetById(3);
var medium = PriorityCollection.GetByName("Medium");
```

### Type Collections

```csharp
// Define base type
public abstract class DataStoreBase : IDataStore
{
    protected DataStoreBase(int id, string name) { }
}

// Create collection
[TypeCollection(typeof(DataStoreBase), typeof(IDataStore), typeof(DataStores))]
public partial class DataStores : TypeCollectionBase<DataStoreBase, IDataStore>
{
}

// Add implementations from any project
[TypeOption(typeof(DataStores), "SqlServer")]
public class SqlServerDataStore : DataStoreBase
{
    public SqlServerDataStore() : base(1, "SqlServer") { }
}

[TypeOption(typeof(DataStores), "FileSystem")]
public class FileSystemDataStore : DataStoreBase
{
    public FileSystemDataStore() : base(2, "FileSystem") { }
}

// Usage
var all = DataStores.All();  // FrozenSet<IDataStore>
var sqlServer = DataStores.SqlServer;  // Static property
var byName = DataStores.GetByName("SqlServer");  // O(1) lookup
```

## Templates

Create new services and domains using dotnet templates:

```bash
# Install templates
dotnet new install FractalDataWorks.Templates

# Create new service domain
dotnet new fdw-service-domain -n MyCompany.Services.MyDomain

# Create service implementation
dotnet new fdw-service -n MyCompany.Services.MyDomain.SqlServer
```

## Testing

The framework uses xUnit v3 with Shouldly for assertions:

```csharp
public class UserServiceTests
{
    [Fact]
    public async Task GetUser_ValidId_ReturnsUser()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.GetUser(123);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(123);
    }

    [Fact]
    public async Task GetUser_InvalidId_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.GetUser(-1);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Invalid user ID");
    }
}
```

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Requirements

- .NET 10.0 SDK or later
- C# 12 language features
- Visual Studio 2024, Rider 2024.3, or VS Code with C# extension

### Building the Solution

```bash
# Restore packages
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test

# Pack NuGet packages
dotnet pack -c Release -o ./artifacts
```

## Performance

The framework is optimized for performance:

- **O(1) Lookups**: FrozenDictionary/FrozenSet for collections
- **Zero Reflection**: Source generation eliminates runtime reflection
- **Minimal Allocations**: Value types and ArrayPool usage where appropriate
- **High-Performance Logging**: Source-generated logging methods
- **Fast Object Creation**: FastGenericNew for service instantiation

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- **Documentation**: [docs/](docs/)
- **Issues**: [GitHub Issues](https://github.com/FractalDataWorks/DeveloperKit/issues)
- **Discussions**: [GitHub Discussions](https://github.com/FractalDataWorks/DeveloperKit/discussions)

## Acknowledgments

Built with modern .NET technologies:
- .NET 10.0
- C# 12
- Roslyn Source Generators
- xUnit v3
- FluentValidation
- Shouldly

---

**FractalDataWorks Developer Kit** - Building better software through Railway-Oriented Programming and compile-time safety.

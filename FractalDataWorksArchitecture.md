# FractalDataWorks Developer Kit Architecture

## Overview

The FractalDataWorks Developer Kit is a comprehensive .NET framework built on Railway-Oriented Programming principles, providing:
- Service-oriented architecture with plugin support
- Enhanced Enums with source generation
- Type-safe collections and lookups
- MCP (Model Context Protocol) integration
- Connection management abstractions
- Result pattern with structured messaging

## Core Components

### 1. Results Pattern (Railway-Oriented Programming)

The framework implements multiple result interfaces for different scenarios:

#### IResult / IResult<T>
Basic result pattern with success/failure states:
- `IsSuccess` / `IsFailure` - Boolean state indicators
- `Error` - Error message for failures
- `ErrorCode` - Optional error code
- `Message` - Additional context message
- `Value` - Return value for generic version

#### IFdwResult / IFdwResult<T>
Enhanced result pattern with message collections:
- Extends `IGenericResult` interface
- `IsEmpty` - Indicates if result contains data
- `Error` - Boolean error indicator
- `Messages` - Collection of `IFdwMessage` objects
- Supports multiple messages with severity levels

#### FdwResult Implementation
Concrete implementation providing:
- Static factory methods for Success/Failure
- Message collection management
- Integration with `FractalMessage` system
- Support for correlation IDs and metadata

### 2. Message System

#### IFdwMessage Interface
Structured messages with:
- `Message` - Human-readable text
- `Code` - Unique identifier
- `Source` - Originating component
- Extends `IEnumOption` for Enhanced Enum integration

#### FractalMessage Class
Concrete implementation with:
- `MessageSeverity` enum (Information, Warning, Error, Critical)
- `Timestamp` and `CorrelationId` tracking
- `Metadata` dictionary for extensibility
- GUID-based unique identification

### 3. ServiceTypes Pattern

Service registration and management system:

#### ServiceTypeBase Abstract Class
Base class for all service types providing:
- Service registration via DI container
- Configuration binding from IConfiguration
- Factory pattern support
- Display metadata (name, description, category)

#### Service Lifecycle
1. Service inherits from `ServiceTypeBase`
2. Override `Register()` to configure DI
3. Override `Configure()` for settings
4. ServiceType property returns concrete service class
5. ConfigurationType for settings POCO (optional)
6. FactoryType for factory pattern (optional)

### 4. Enhanced Enums System

Type-safe enum pattern with source generation:

#### Core Interfaces
- `IEnumOption` - Base interface for enum values
  - `Id` - Unique integer identifier
  - `Name` - String representation
- `IEnumCollection<T>` - Collection of enum options
  - Lookup by ID, name, or custom attributes
  - LINQ-queryable collections

#### Attributes
- `[EnumOption]` - Marks enum value classes
- `[TypeCollection]` - Generates collection for type
- `[GlobalTypeCollection]` - Cross-assembly collection
- `[TypeLookup]` - Custom lookup properties

#### Source Generators
Automatically generates:
- Collection classes with static instances
- Lookup methods for all marked properties
- Type-safe accessors
- Serialization support

### 5. MCP Plugin Architecture

Model Context Protocol integration for AI tooling:

#### IMcpTool Interface
Base interface for MCP tools:
- `Name`, `Description`, `Category` - Metadata
- `IsEnabled` - Runtime enable/disable
- `Priority` - Execution ordering
- `ExecuteAsync()` - Main execution with Result pattern
- `ValidateArgumentsAsync()` - Pre-execution validation

#### Plugin Service Pattern
Each plugin category has:
- `ServiceType` class extending `ServiceTypeBase`
- `ToolService` class managing tool collection
- Individual tool implementations of `IMcpTool`

#### Current Plugin Categories
- **CodeAnalysis** - Code analysis and diagnostics
- **SessionManagement** - Workspace session handling
- **VirtualEditing** - Non-destructive code edits
- **Refactoring** - Code transformation tools
- **TypeAnalysis** - Type system analysis
- **ProjectDependencies** - Dependency graph tools
- **ServerManagement** - MCP server lifecycle

### 6. Connection Abstractions

Database and external service connectivity:

#### IConnectionInfo Interface
Connection metadata and configuration:
- Connection string management
- Provider-specific settings
- Credential handling

#### IDataProviderFactory<T>
Factory pattern for data providers:
- Provider registration
- Connection pooling
- Transaction management

#### Concrete Implementations
- **MsSql** - SQL Server provider
  - `SqlServerConnectionInfo`
  - `SqlServerDataProvider`
  - Result mapping utilities
- Additional providers follow same pattern

### 7. Collections System

Type-safe collections with source generation:

#### TypeCollectionBase
Base class for generated collections:
- Static instance management
- Thread-safe initialization
- Lookup method generation

#### TypeOptionBase
Base class for collection items:
- ID and Name properties
- Equality comparison
- ToString override

#### Source Generator Pipeline
1. Scans for `[TypeCollection]` attributes
2. Generates partial classes
3. Creates static lookup methods
4. Implements IEnumerable interface

## Architecture Principles

### 1. Railway-Oriented Programming
- All operations return Result types
- Explicit error handling without exceptions
- Chainable operations via Map/Match methods
- No null references in public APIs

### 2. Dependency Injection First
- All services registered via ServiceTypeBase
- Configuration through IConfiguration
- Factory pattern for complex initialization
- Scoped/Singleton/Transient lifecycle support

### 3. Source Generation Over Reflection
- Compile-time code generation
- Type-safe at build time
- No runtime reflection overhead
- IntelliSense support for generated code

### 4. Plugin Architecture
- Loose coupling via interfaces
- Runtime discovery and loading
- Category-based organization
- Priority-based execution

### 5. Structured Messaging
- All messages implement IFdwMessage
- Severity levels for filtering
- Correlation IDs for tracing
- Metadata for extensibility

## Development Workflow

### Creating a New Service

1. **Define Service Interface**
```csharp
public interface IMyService
{
    Task<IFdwResult<MyData>> GetDataAsync(int id);
}
```

2. **Implement Service**
```csharp
public class MyService : IMyService
{
    public async Task<IFdwResult<MyData>> GetDataAsync(int id)
    {
        // Implementation
        return FdwResult<MyData>.Success(data);
    }
}
```

3. **Create ServiceType**
```csharp
public class MyServiceType : ServiceTypeBase
{
    public override Type ServiceType => typeof(MyService);

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IMyService, MyService>();
    }
}
```

### Creating Enhanced Enums

1. **Define Enum Class**
```csharp
[TypeCollection]
public class Status : TypeOptionBase
{
    public static readonly Status Active = new(1, "Active");
    public static readonly Status Inactive = new(2, "Inactive");

    private Status(int id, string name) : base(id, name) { }
}
```

2. **Use Generated Collection**
```csharp
var status = StatusCollection.GetById(1);
var allStatuses = StatusCollection.GetAll();
```

### Creating MCP Tools

1. **Implement IMcpTool**
```csharp
public class MyAnalysisTool : IMcpTool
{
    public string Name => "MyAnalysis";
    public string Category => "CodeAnalysis";

    public async Task<IFdwResult<object>> ExecuteAsync(
        object? arguments,
        CancellationToken cancellationToken)
    {
        // Tool implementation
        return FdwResult<object>.Success(result);
    }
}
```

2. **Register in ToolService**
```csharp
public class CodeAnalysisToolService
{
    private void RegisterTools()
    {
        _tools.Add(new MyAnalysisTool());
    }
}
```

## Best Practices

### Error Handling
- Always return Result types from public methods
- Use specific error codes for programmatic handling
- Include context in error messages
- Chain results with Map/Match methods

### Service Design
- Keep services focused on single responsibility
- Use interfaces for all public contracts
- Inject dependencies via constructor
- Configure via IConfiguration

### Enhanced Enums
- Use for fixed sets of values
- Add lookup properties via attributes
- Keep immutable after construction
- Use source generation for collections

### MCP Tools
- Validate arguments before execution
- Return structured results
- Use categories for organization
- Implement cancellation support

### Performance
- Prefer source generation over reflection
- Use async/await for I/O operations
- Cache expensive computations
- Profile generated code

## Migration Guide

### From Exceptions to Results
```csharp
// Before
public Data GetData(int id)
{
    if (id <= 0)
        throw new ArgumentException("Invalid ID");
    return repository.GetById(id);
}

// After
public IFdwResult<Data> GetData(int id)
{
    if (id <= 0)
        return FdwResult<Data>.Failure("Invalid ID", "INVALID_ID");

    var data = repository.GetById(id);
    return FdwResult<Data>.Success(data);
}
```

### From Enums to Enhanced Enums
```csharp
// Before
public enum Status { Active = 1, Inactive = 2 }

// After
[TypeCollection]
public class Status : TypeOptionBase
{
    public static readonly Status Active = new(1, "Active");
    public static readonly Status Inactive = new(2, "Inactive");
}
```

### From Direct DI to ServiceTypes
```csharp
// Before
services.AddScoped<IMyService, MyService>();

// After
public class MyServiceType : ServiceTypeBase
{
    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IMyService, MyService>();
    }
}
```

## Troubleshooting

### Common Issues

1. **Source generators not running**
   - Clean and rebuild solution
   - Check generator package references
   - Verify target framework compatibility

2. **Result pattern overhead**
   - Use value types where appropriate
   - Consider caching result instances
   - Profile memory allocation

3. **ServiceType not registering**
   - Ensure ServiceType has public constructor
   - Check assembly scanning configuration
   - Verify ServiceTypeBase inheritance

4. **Enhanced Enum lookups failing**
   - Rebuild to regenerate collections
   - Check attribute placement
   - Verify unique IDs and names

## Future Enhancements

### Planned Features
- Distributed tracing integration
- GraphQL schema generation
- OpenAPI specification generation
- Performance metrics collection
- Advanced caching strategies

### Under Consideration
- Actor model integration
- Event sourcing support
- CQRS pattern helpers
- Saga orchestration
- Multi-tenancy support

## Contributing

### Code Standards
- Follow Railway-Oriented Programming
- Write unit tests for all public APIs
- Document public interfaces
- Use source generation where applicable
- Maintain backwards compatibility

### Submission Process
1. Fork repository
2. Create feature branch
3. Write tests first (TDD)
4. Implement feature
5. Update documentation
6. Submit pull request

## License

[Include license information]

## Support

[Include support contact information]
# Naming Conventions

## Project and Assembly Naming

### Core Framework Pattern
```
FractalDataWorks.{Category}[.{Subcategory}][.{Implementation}]
```

Examples:
- `FractalDataWorks.Services` - Core services framework
- `FractalDataWorks.Services.Abstractions` - Service interfaces and base classes
- `FractalDataWorks.Services.DataGateway.Abstractions` - Data provider domain abstractions
- `FractalDataWorks.Services.DataGateway.MsSql` - SQL Server implementation
- `FractalDataWorks.EnhancedEnums` - Enhanced enums core functionality
- `FractalDataWorks.EnhancedEnums.SourceGenerators` - Source generator implementation

### Project Structure Rules
1. **Abstractions projects** contain interfaces, base classes, and contracts
2. **Implementation projects** contain concrete implementations
3. **Extensions projects** contain optional integrations or extensions
4. **Tests projects** follow the pattern `{ProjectName}.Tests`

## Namespace Organization

### Standard Namespace Hierarchy
```
FractalDataWorks.{Category}.{Subcategory}[.{Implementation}]
    ├── Abstractions/         // Interfaces and contracts
    ├── Configuration/         // Configuration classes
    ├── EnhancedEnums/        // Enhanced enum definitions
    ├── Factories/            // Factory implementations
    ├── Messages/             // Message definitions
    └── Services/             // Service implementations
```

Examples:
```csharp
namespace FractalDataWorks.Services.DataGateway.MsSql.Services;
namespace FractalDataWorks.Services.Authentication.Abstractions.Configuration;
namespace FractalDataWorks.Transformations.Parallel.EnhancedEnums;
```

## Enhanced Enum Naming

### Base Class Naming
- Abstract base classes: `{Purpose}Base` or `{Purpose}`
- Collection base classes: `{Purpose}CollectionBase`

```csharp
// Base classes
public abstract class ServiceTypeBase : EnumOptionBase<ServiceTypeBase>
public abstract class MessageBase : EnumOptionBase<MessageBase>

// Collection bases  
public abstract class ServiceTypeCollectionBase : EnumCollectionBase<ServiceTypeBase>
public abstract class MessageCollectionBase : EnumCollectionBase<MessageBase>
```

### Enum Option Naming
- Concrete implementations: Descriptive names without "Base" suffix
- Use PascalCase for all enum options

```csharp
[EnumOption("DataGateway")]
public sealed class DataGatewayService : ServiceTypeBase

[EnumOption("Authentication")]  
public sealed class AuthenticationService : ServiceTypeBase

[EnumOption("ProcessingFailed")]
public sealed class ProcessingFailed : MessageBase
```

### Generated Collection Naming
- Collections: `{PluralPurpose}` (e.g., `ServiceTypes`, `Messages`)
- Use descriptive plurals that clearly indicate the collection's purpose

## Service Naming

### Interface Naming
```csharp
public interface I{Domain}Service : IFractalService<{Domain}Executor, {Domain}Configuration>
public interface I{Domain}ServiceFactory : IServiceFactory<I{Domain}Service, {Domain}Configuration>
```

Examples:
```csharp
public interface IDataGatewayService : IFractalService<DataGatewayExecutor, DataGatewayConfiguration>
public interface IAuthenticationServiceFactory : IServiceFactory<IAuthenticationService, AuthenticationConfiguration>
```

### Implementation Naming
```csharp
public class {Domain}Service : ServiceBase<{Domain}Executor, {Domain}Configuration>, I{Domain}Service
public class {Domain}ServiceFactory : ServiceFactoryBase<I{Domain}Service, {Domain}Configuration>, I{Domain}ServiceFactory
```

Examples:
```csharp
public class DataGatewayService : ServiceBase<DataGatewayExecutor, DataGatewayConfiguration>, IDataGatewayService
public class MsSqlDataGatewayFactory : ServiceFactoryBase<IDataGatewayService, MsSqlConfiguration>, IDataGatewayServiceFactory
```

## Configuration Naming

### Configuration Classes
```csharp
public class {Purpose}Configuration : ConfigurationBase<{Purpose}Configuration>
```

Examples:
```csharp
public class DataGatewayConfiguration : ConfigurationBase<DataGatewayConfiguration>
public class MsSqlConfiguration : ConfigurationBase<MsSqlConfiguration>
public class TransformationConfiguration : ConfigurationBase<TransformationConfiguration>
```

### Configuration Sections
- Use PascalCase for section names
- Match the configuration class name without "Configuration" suffix

```csharp
public class DataGatewayConfiguration : ConfigurationBase<DataGatewayConfiguration>
{
    public override string SectionName => "DataGateway";  // Not "DataGatewayConfiguration"
}
```

## Message Naming

### Message Classes
- Use descriptive action or state names
- Follow PascalCase convention
- Avoid generic names like "Error" or "Success"

```csharp
[EnumOption]
public sealed class ConnectionFailed : DataGatewayMessageBase

[EnumOption]
public sealed class ConfigurationValidationFailed : ServiceMessageBase

[EnumOption] 
public sealed class TransformationCompleted : TransformationMessageBase
```

### Message Collection Naming
```csharp
// Generated collections
public static class DataGatewayMessages
public static class ServiceMessages  
public static class TransformationMessages
```

## File and Directory Naming

### Directory Structure
```
src/
├── FractalDataWorks.{Category}/
│   ├── Abstractions/
│   ├── Configuration/
│   ├── EnhancedEnums/
│   ├── Services/
│   └── Messages/
├── FractalDataWorks.{Category}.{Subcategory}/
└── FractalDataWorks.{Category}.{Subcategory}.{Implementation}/
```

### File Naming Rules
1. **One primary type per file** - File name matches the primary type name
2. **Interface files** - Match interface name exactly (`IDataGatewayService.cs`)
3. **Implementation files** - Match implementation name exactly (`DataGatewayService.cs`)
4. **Enhanced Enum files** - Match enum option name exactly (`ProcessingFailed.cs`)
5. **Collection base files** - Include "CollectionBase" suffix (`ServiceTypeCollectionBase.cs`)

### Special File Naming
```
ServiceCollectionExtensions.cs        // DI registration extensions
{Domain}Messages.cs                   // Message constants/helpers (if needed)
{Domain}Constants.cs                  // Domain-specific constants
GlobalUsings.cs                       // Global using directives
```

## Method and Property Naming

### Service Methods
```csharp
// Primary operations - use descriptive verbs with "Async" suffix
Task<IFdwResult<T>> ProcessAsync(...)
Task<IFdwResult<T>> ExecuteAsync(...)
Task<IFdwResult<T>> TransformAsync(...)

// Validation methods
Task<IFdwResult<ValidationResult>> ValidateConfigurationAsync(...)
Task<IFdwResult> ValidateInputAsync(...)

// Query methods  
Task<IFdwResult<T>> GetStatusAsync(...)
Task<IFdwResult<T>> GetResultAsync(...)

// Management methods
Task<IFdwResult> StartAsync(...)
Task<IFdwResult> StopAsync(...)
Task<IFdwResult> CancelAsync(...)
```

### Configuration Properties
```csharp
// Use descriptive, specific names
public string ConnectionString { get; set; }
public TimeSpan CommandTimeout { get; set; }
public int MaxRetryAttempts { get; set; }
public bool EnableLogging { get; set; }

// Avoid generic names
public string Value { get; set; }     // ❌ Too generic
public string Data { get; set; }      // ❌ Too generic  
public object Config { get; set; }    // ❌ Too generic
```

### Enhanced Enum Properties
```csharp
// Business logic properties - use descriptive names
public abstract bool RequiresAuthentication { get; }
public abstract TimeSpan DefaultTimeout { get; }
public abstract int Priority { get; }

// Avoid property names that conflict with base class
public string Name { get; }     // ❌ Conflicts with EnumOptionBase.Name
public int Id { get; }          // ❌ Conflicts with EnumOptionBase.Id
```

## Constants and Static Values

### Constant Naming
```csharp
// Use SCREAMING_SNAKE_CASE for constants
public const string DEFAULT_CONNECTION_STRING = "Server=localhost;Database=Test;";
public const int MAX_RETRY_ATTEMPTS = 3;
public const double DEFAULT_TIMEOUT_MINUTES = 5.0;

// Static readonly fields use PascalCase
public static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);
public static readonly ImmutableList<string> SupportedFormats = ImmutableList.Create("json", "xml");
```

### Message Templates
```csharp
// Use descriptive template names
public const string PROCESSING_FAILED_TEMPLATE = "Processing failed: {0}";
public const string INVALID_CONFIGURATION_TEMPLATE = "Configuration validation failed: {0}";
public const string CONNECTION_TIMEOUT_TEMPLATE = "Connection timed out after {0} seconds";
```

## Best Practices

### 1. Consistency
- Follow established patterns within the same domain
- Use consistent terminology across related components
- Maintain naming consistency between interfaces and implementations

### 2. Clarity
- Use descriptive names that clearly indicate purpose
- Avoid abbreviations unless they are well-established (e.g., "Http", "Sql")
- Prefer longer, clear names over short, cryptic ones

### 3. Avoid Conflicts
- Don't use names that conflict with .NET Framework types
- Avoid generic names like "Manager", "Handler", "Helper"
- Use domain-specific terminology when possible

### 4. Future-Proofing
- Use names that won't become misleading as functionality evolves
- Avoid implementation-specific names in abstractions
- Consider how names will scale with additional implementations
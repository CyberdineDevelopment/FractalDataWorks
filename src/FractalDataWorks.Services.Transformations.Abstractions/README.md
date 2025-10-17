# FractalDataWorks.Services.Transformations.Abstractions

**Core abstractions and interfaces for data transformation services within the FractalDataWorks framework.**

## Overview

This project provides the foundational abstractions for data transformation services in the FractalDataWorks ecosystem. It defines comprehensive contracts that transformation service providers must implement, including request/response models, service type definitions, and configuration interfaces for handling complex data transformation operations.

## Architecture

The transformation abstractions follow the framework's enhanced service pattern with rich interface support:

- **Service Type Management**: `ITransformationServiceType` with comprehensive metadata and capabilities
- **Provider Abstractions**: `ITransformationProvider` for transformation service implementations  
- **Request/Response Contracts**: `ITransformationRequest` and `ITransformationResult` with immutable update patterns
- **Engine Support**: `ITransformationEngine` for complex, multi-step transformations
- **Configuration**: `ITransformationsConfiguration` for service configuration
- **Enhanced Enums**: `TransformationServiceTypes<TSelf, TFactory>` base class for service type collections

## Key Types

### Service Type Management

#### ITransformationServiceType
Interface that extends `IServiceType` with transformation-specific metadata:
- **SupportedInputTypes** - Array of supported input data types (e.g., "JSON", "XML", "CSV")
- **SupportedOutputTypes** - Array of supported output data types  
- **SupportedCategories** - Array of transformation categories (e.g., "Mapping", "Filtering")
- **SupportsParallelExecution** - Boolean indicating parallel processing support
- **SupportsTransformationCaching** - Boolean indicating caching support
- **SupportsPipelineMode** - Boolean indicating pipeline processing support
- **MaxInputSizeBytes** - Maximum input size this service can handle
- **Priority** - Numeric priority for provider selection

#### TransformationServiceTypes<TSelf, TFactory>
Abstract base class for service type enumerations using the Enhanced Enums pattern. Marked with `[StaticEnumCollection]` attribute for source generation.

### Core Provider Interface

#### ITransformationProvider
Non-generic marker interface extending `IFractalService`.

#### ITransformationProvider<TTransformationRequest>
Main transformation provider interface with comprehensive capabilities:

```csharp
public interface ITransformationProvider<TTransformationRequest> : ITransformationProvider, IFractalService<TTransformationRequest>
{
    // Capability advertisement
    IReadOnlyList<string> SupportedInputTypes { get; }
    IReadOnlyList<string> SupportedOutputTypes { get; }
    IReadOnlyList<string> TransformationCategories { get; }
    int Priority { get; }
    
    // Validation and execution
    IGenericResult ValidateTransformation(string inputType, string outputType, string? transformationCategory = null);
    Task<IGenericResult<TOutput>> Transform<TOutput>(ITransformationRequest transformationRequest);
    Task<IGenericResult<object?>> Transform(ITransformationRequest transformationRequest);
    
    // Advanced capabilities
    Task<IGenericResult<ITransformationEngine>> CreateEngineAsync(ITransformationEngineConfiguration configuration);
    Task<IGenericResult<ITransformationMetrics>> GetTransformationMetricsAsync();
}
```

### Request/Response Contracts

#### ITransformationRequest
Comprehensive interface extending `ICommand` for transformation requests with immutable update pattern:

```csharp
public interface ITransformationRequest : ICommand
{
    // Request identification
    string RequestId { get; }
    
    // Input/output definition
    object? InputData { get; }
    string InputType { get; }
    string OutputType { get; }
    string? TransformationCategory { get; }
    Type ExpectedResultType { get; }
    TimeSpan? Timeout { get; }
    
    // Configuration and context
    new IReadOnlyDictionary<string, object> Configuration { get; }
    IReadOnlyDictionary<string, object> Options { get; }
    ITransformationContext? Context { get; }
    
    // Immutable update methods
    ITransformationRequest WithInputData(object? newInputData, string? newInputType = null);
    ITransformationRequest WithOutputType(string newOutputType, Type? newExpectedResultType = null);
    ITransformationRequest WithConfiguration(IReadOnlyDictionary<string, object> newConfiguration);
    ITransformationRequest WithOptions(IReadOnlyDictionary<string, object> newOptions);
}
```

**Key Features:**
- Extends `ICommand` from the core FractalDataWorks framework
- Comprehensive request metadata with input/output types and categories
- Immutable update pattern for thread-safe request modification
- Context support for security and pipeline information
- Rich configuration and options dictionaries

#### ITransformationEngine
Interface for complex, multi-step transformation engines with lifecycle management:

```csharp
public interface ITransformationEngine
{
    // Engine identification and state
    string EngineId { get; }
    string EngineType { get; }
    bool IsRunning { get; }
    
    // Engine operations
    Task<IGenericResult<ITransformationResult>> ExecuteTransformationAsync(
        ITransformationRequest request, 
        CancellationToken cancellationToken = default);
    Task<IGenericResult> StartAsync(CancellationToken cancellationToken = default);
    Task<IGenericResult> StopAsync(CancellationToken cancellationToken = default);
}
```

### Supporting Interfaces

#### ITransformationContext
Context interface providing additional information for transformations (definition not shown in abstractions layer).

#### ITransformationEngineConfiguration
Configuration interface for transformation engines (definition not shown in abstractions layer).

#### ITransformationMetrics
Interface for transformation performance metrics (definition not shown in abstractions layer).

#### ITransformationResult
Interface for transformation operation results (definition not shown in abstractions layer).

#### ITransformationsConfiguration
Simple configuration interface extending `IFractalConfiguration`:

```csharp
public interface ITransformationsConfiguration : IFractalConfiguration 
{
    // Base configuration interface - specific configurations defined by implementations
}
```

## Dependencies

### Project References
- **FractalDataWorks.Services** - Core service framework functionality

### Package References
None - this is a pure abstraction layer with no external dependencies.

## Usage Patterns

### Service Type Definition
```csharp
using FractalDataWorks.Services.Transformations.Abstractions;

// Concrete service types are defined in implementation projects, extending the base class
public sealed class MyTransformationServiceType :
    TransformationServiceType<MyTransformationServiceType, ITransformationProvider, ITransformationsConfiguration, IMyFactory>
{
    public MyTransformationServiceType()
        : base(
            id: 1,
            name: "MyTransformation",
            description: "Custom transformation service",
            supportedInputTypes: ["JSON", "XML"],
            supportedOutputTypes: ["CSV", "Object"],
            supportedCategories: ["Mapping", "Conversion"],
            supportsParallelExecution: true,
            supportsTransformationCaching: false,
            supportsPipelineMode: true,
            maxInputSizeBytes: 10485760L, // 10MB
            priority: 50)
    {
    }
}
```

### Provider Implementation
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Services.Transformations.Abstractions;
using FractalDataWorks.Results;

public class MyTransformationProvider : ITransformationProvider<ITransformationRequest>
{
    public IReadOnlyList<string> SupportedInputTypes => ["JSON", "XML"];
    public IReadOnlyList<string> SupportedOutputTypes => ["CSV", "Object"];
    public IReadOnlyList<string> TransformationCategories => ["Mapping", "Conversion"];
    public int Priority => 50;

    public IGenericResult ValidateTransformation(string inputType, string outputType, string? category = null)
    {
        // Validate if this provider can handle the transformation
    }

    public async Task<IGenericResult<TOutput>> Transform<TOutput>(ITransformationRequest request)
    {
        // Perform the actual transformation
    }

    // Additional interface members...
}
```

### Request Usage
```csharp
using System.Collections.Generic;
using FractalDataWorks.Services.Transformations.Abstractions;

// Transformation requests use immutable update pattern
var request = originalRequest
    .WithInputData(newData, "JSON")
    .WithOutputType("CSV")
    .WithConfiguration(new Dictionary<string, object> { ["mappingRules"] = rules });
```

## Code Coverage Exclusions

The following patterns should be excluded from code coverage as they represent infrastructure, boilerplate, or generated code:

```xml
<ExcludeFromCodeCoverage>
  <!-- Enhanced Enums source-generated classes -->
  <Attribute>FractalDataWorks.EnhancedEnums.Attributes.StaticEnumCollectionAttribute</Attribute>
  
  <!-- Base abstract classes used for framework patterns -->
  <Class>TransformationServiceTypes</Class>
  
  <!-- Simple property-only interfaces -->
  <Method>*.get_*</Method>
  <Method>*.set_*</Method>
</ExcludeFromCodeCoverage>
```

## Architecture Notes

### Enhanced Enums Pattern
This project uses the FractalDataWorks Enhanced Enums pattern for service type definitions. The `TransformationServiceTypes<TSelf, TFactory>` base class is marked with `[StaticEnumCollection]` attribute, enabling source generation of strongly-typed service type collections.

### Service Framework Integration
All interfaces extend the core FractalDataWorks service framework patterns:
- `ITransformationProvider` extends `IFractalService`
- `ITransformationRequest` extends `ICommand` 
- `ITransformationsConfiguration` extends `IFractalConfiguration`
- Results follow `IGenericResult<T>` pattern

### Immutable Request Pattern
The `ITransformationRequest` interface provides immutable update methods (`WithInputData`, `WithOutputType`, etc.) that return new instances rather than modifying existing ones, ensuring thread safety and predictable behavior.

## Implementation Status

✅ **Complete**: All core abstractions are fully defined with comprehensive XML documentation  
✅ **Complete**: Service type management with Enhanced Enums integration  
✅ **Complete**: Provider interfaces with capability advertisement and validation  
✅ **Complete**: Request/response contracts with immutable update patterns  
✅ **Complete**: Engine abstractions for complex transformation scenarios  
✅ **Complete**: Integration with core FractalDataWorks service framework patterns  

This abstraction layer is production-ready and provides a solid foundation for implementing transformation services within the FractalDataWorks ecosystem.
# FractalDataWorks.Services.Transformations

## Overview

This project provides concrete service type definitions and collections for data transformation services within the FractalDataWorks framework. It implements the transformation service type patterns defined in the abstractions layer, providing a standard transformation service type that supports common data transformation scenarios.

## Key Types

### Service Type Definitions

#### StandardTransformationServiceType
A concrete service type definition for the standard transformation service implementation:

```csharp
public sealed class StandardTransformationServiceType : 
    TransformationServiceType<StandardTransformationServiceType, ITransformationProvider, ITransformationsConfiguration, IServiceFactory<ITransformationProvider, ITransformationsConfiguration>>
{
    // Configured with comprehensive transformation capabilities
}
```

**Capabilities:**
- **Supported Input Types**: JSON, XML, CSV, Object, Dictionary, Stream
- **Supported Output Types**: JSON, XML, CSV, Object, Dictionary, Stream  
- **Supported Categories**: Mapping, Filtering, Validation, Aggregation, Formatting, Conversion
- **Parallel Execution**: Supported
- **Transformation Caching**: Supported
- **Pipeline Mode**: Supported
- **Maximum Input Size**: 100MB (104,857,600 bytes)
- **Priority**: 75

### Enhanced Enums Collections

#### TransformationServiceTypesCollection
A source-generated collection class that provides strongly-typed access to transformation service types:

```csharp
[EnumCollection(CollectionName = "TransformationServiceTypes", DefaultGenericReturnType = typeof(IServiceType))]
public sealed class TransformationServiceTypesCollection : 
    TransformationServiceTypes<StandardTransformationServiceType, IServiceFactory<ITransformationProvider, ITransformationsConfiguration>>
{
    // Source generator automatically creates:
    // - TransformationServiceTypes.Standard (returns IServiceType)
    // - TransformationServiceTypes.All (collection)
    // - TransformationServiceTypes.GetById(int id)
    // - TransformationServiceTypes.GetByName(string name)
}
```

## Dependencies

### Project References
- **FractalDataWorks.Services.Transformations.Abstractions** - Core transformation abstractions
- **FractalDataWorks.Services** - Core service framework functionality

### Package References
None - inherits dependencies from referenced projects.

## Usage Patterns

### Accessing Service Types

```csharp
// Get the standard transformation service type
var standardType = TransformationServiceTypes.Standard;

// Access service type properties
Console.WriteLine($"Service: {standardType.Name}");
Console.WriteLine($"Description: {standardType.Description}");
Console.WriteLine($"Supports Parallel: {standardType.SupportsParallelExecution}");
Console.WriteLine($"Max Input Size: {standardType.MaxInputSizeBytes} bytes");

// Get all available service types
var allTypes = TransformationServiceTypes.All;

// Find service type by ID or name
var typeById = TransformationServiceTypes.GetById(1);
var typeByName = TransformationServiceTypes.GetByName("StandardTransformation");
```

### Service Registration

```csharp
// Service types can be used with dependency injection
services.AddTransformationService(TransformationServiceTypes.Standard);

// Or accessed for factory creation
var factory = serviceProvider.GetService<IServiceFactory<ITransformationProvider, ITransformationsConfiguration>>();
var provider = await factory.CreateServiceAsync(TransformationServiceTypes.Standard, configuration);
```

## Generated Code

The `TransformationServiceTypesCollection` class uses the Enhanced Enums source generator to automatically create:

- **Static Access Properties**: `TransformationServiceTypes.Standard`
- **Collection Access**: `TransformationServiceTypes.All`
- **Lookup Methods**: `GetById(int)`, `GetByName(string)`
- **Type Safety**: Returns `IServiceType` interface for framework compatibility

## Code Coverage Exclusions

The following patterns should be excluded from code coverage as they represent infrastructure or generated code:

```xml
<ExcludeFromCodeCoverage>
  <!-- Enhanced Enums source-generated classes -->
  <Attribute>FractalDataWorks.EnhancedEnums.Attributes.EnumCollectionAttribute</Attribute>
  
  <!-- Source-generated collection classes -->
  <Class>TransformationServiceTypesCollection</Class>
  
  <!-- Service type constructors (configuration only) -->
  <Method>StandardTransformationServiceType..ctor</Method>
  
  <!-- Factory implementation type getters -->
  <Method>*.GetFactoryImplementationType</Method>
</ExcludeFromCodeCoverage>
```

## Architecture Notes

### Enhanced Enums Pattern
This project implements the FractalDataWorks Enhanced Enums pattern where service types are strongly-typed objects with rich metadata rather than simple enumerations. The source generator creates static collections and lookup methods automatically.

### Factory Integration
The `StandardTransformationServiceType` uses the generic service factory by default but can be overridden to use custom factory implementations as needed.

### Configuration Inheritance
Service types inherit comprehensive configuration from the base `TransformationServiceType` class, including input/output type definitions, capability flags, and resource limits.

## Implementation Status

✅ **Complete**: Standard transformation service type with comprehensive capabilities  
✅ **Complete**: Enhanced Enums collection with source generation  
✅ **Complete**: Integration with generic service factory pattern  
✅ **Complete**: Full metadata specification for transformation capabilities  

## Extending Service Types

To add new transformation service types:

1. Create a new service type class extending `TransformationServiceType`
2. Configure supported input/output types and capabilities
3. Add the service type to the collection class
4. Implement corresponding transformation provider if needed

Example:
```csharp
public sealed class AdvancedTransformationServiceType : 
    TransformationServiceType<AdvancedTransformationServiceType, ITransformationProvider, ITransformationsConfiguration, IAdvancedFactory>
{
    public AdvancedTransformationServiceType() 
        : base(
            id: 2,
            name: "AdvancedTransformation",
            description: "Advanced transformation service with ML capabilities",
            supportedInputTypes: new[] { "JSON", "Parquet", "Avro" },
            supportedOutputTypes: new[] { "JSON", "Parquet", "TensorFlow" },
            supportedCategories: new[] { "MachineLearning", "Analytics", "Prediction" },
            supportsParallelExecution: true,
            supportsTransformationCaching: true,
            supportsPipelineMode: true,
            maxInputSizeBytes: 1073741824L, // 1GB
            priority: 90)
    {
    }
}
```

This project provides the concrete service type definitions needed to register and use transformation services within the FractalDataWorks framework.
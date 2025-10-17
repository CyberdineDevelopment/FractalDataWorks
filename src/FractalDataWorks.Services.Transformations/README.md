# FractalDataWorks.Services.Transformations

## Overview

The Transformations framework provides ServiceType auto-discovery for data transformation engines with unified interfaces that work across different transformation providers and processing patterns.

## Features

- **ServiceType Auto-Discovery**: Add transformation packages and they're automatically registered
- **Universal Transformation Interface**: Same API works with all transformation engines
- **Dynamic Engine Creation**: Transformation services created via factories
- **Source-Generated Collections**: High-performance engine lookup

## Quick Start

### 1. Install Packages

```xml
<ProjectReference Include="..\FractalDataWorks.Services.Transformations\FractalDataWorks.Services.Transformations.csproj" />
<ProjectReference Include="..\FractalDataWorks.Services.Transformations.Parallel\FractalDataWorks.Services.Transformations.Parallel.csproj" />
```

### 2. Register Services

```csharp
using FractalDataWorks.Services.Transformations;
using Microsoft.Extensions.DependencyInjection;

// Program.cs - Zero-configuration registration
builder.Services.AddScoped<IGenericTransformationProvider, GenericTransformationProvider>();

// Single line registers ALL discovered transformation types
TransformationTypes.Register(builder.Services);
```

### 3. Configure Transformations

```json
{
  "Transformations": {
    "DataProcessor": {
      "TransformationType": "Parallel",
      "MaxConcurrency": 8,
      "BatchSize": 1000,
      "EnableRetry": true,
      "MaxRetryAttempts": 3
    }
  }
}
```

### 4. Use Universal Transformations

```csharp
using FractalDataWorks.Common;
using FractalDataWorks.Services.Transformations;

public class DataProcessingService
{
    private readonly IGenericTransformationProvider _transformationProvider;

    public DataProcessingService(IGenericTransformationProvider transformationProvider)
    {
        _transformationProvider = transformationProvider;
    }

    public async Task<IGenericResult<List<ProcessedData>>> ProcessDataAsync(List<RawData> rawData)
    {
        var engineResult = await _transformationProvider.GetTransformationEngine("DataProcessor");
        if (!engineResult.IsSuccess)
            return GenericResult<List<ProcessedData>>.Failure(engineResult.Error);

        using var engine = engineResult.Value;

        // Universal transformation - works with any engine
        var transformationResult = await engine.TransformAsync<RawData, ProcessedData>(
            rawData,
            data => new ProcessedData
            {
                Id = data.Id,
                ProcessedValue = data.RawValue.ToUpper(),
                ProcessedAt = DateTimeOffset.UtcNow
            });

        return transformationResult;
    }
}
```

## Available Transformation Types

| Package | Transformation Type | Purpose |
|---------|-------------------|---------|
| `FractalDataWorks.Services.Transformations.Parallel` | Parallel | High-performance parallel processing |
| `FractalDataWorks.Services.Transformations.Sequential` | Sequential | Sequential data processing |
| `FractalDataWorks.Services.Transformations.Streaming` | Streaming | Real-time stream processing |

## How Auto-Discovery Works

1. **Source Generator Scans**: `[ServiceTypeCollection]` attribute triggers compile-time discovery
2. **Finds Implementations**: Scans referenced assemblies for types inheriting from `TransformationTypeBase`
3. **Generates Collections**: Creates `TransformationTypes.All`, `TransformationTypes.Name()`, etc.
4. **Self-Registration**: Each transformation type handles its own DI registration

## Adding Custom Transformation Types

```csharp
using FractalDataWorks.Services.Transformations;
using Microsoft.Extensions.DependencyInjection;

// 1. Create your transformation type (singleton pattern)
public sealed class CustomTransformationType : TransformationTypeBase<IGenericTransformationEngine, CustomTransformationConfiguration, ICustomTransformationFactory>
{
    public static CustomTransformationType Instance { get; } = new();

    private CustomTransformationType() : base(4, "Custom", "Transformation Engines") { }

    public override Type FactoryType => typeof(ICustomTransformationFactory);

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<ICustomTransformationFactory, CustomTransformationFactory>();
        services.AddScoped<CustomTransformationProcessor>();
        services.AddScoped<CustomDataValidator>();
    }
}

// 2. Add package reference - source generator automatically discovers it
// 3. TransformationTypes.Register(services) will include it automatically
```

## Common Transformation Patterns

### Batch Processing

```csharp
using FractalDataWorks.Common;
using FractalDataWorks.Services.Transformations;

public async Task<IGenericResult> ProcessLargeDatatAsync(IEnumerable<DataRecord> records)
{
    var engineResult = await _transformationProvider.GetTransformationEngine("Parallel");
    if (!engineResult.IsSuccess)
        return GenericResult.Failure(engineResult.Error);

    using var engine = engineResult.Value;

    // Process in batches for memory efficiency
    var result = await engine.TransformBatchAsync<DataRecord, ProcessedRecord>(
        records,
        batchSize: 1000,
        transformer: batch => batch.Select(ProcessRecord).ToList());

    return result;
}
```

### Streaming Transformations

```csharp
using FractalDataWorks.Common;
using FractalDataWorks.Services.Transformations;

public async Task<IGenericResult> ProcessStreamAsync(IAsyncEnumerable<StreamData> dataStream)
{
    var engineResult = await _transformationProvider.GetTransformationEngine("Streaming");
    if (!engineResult.IsSuccess)
        return GenericResult.Failure(engineResult.Error);

    using var engine = engineResult.Value;

    // Real-time stream processing
    await foreach (var item in dataStream)
    {
        var transformResult = await engine.TransformAsync(item, TransformStreamItem);
        if (!transformResult.IsSuccess)
            // Handle transformation errors
            continue;
    }

    return GenericResult.Success();
}
```

## Architecture Benefits

- **Engine Agnostic**: Switch transformation engines without code changes
- **Zero Configuration**: Add package reference, get functionality
- **Type Safety**: Compile-time validation of transformation types
- **Performance**: Source-generated collections use FrozenDictionary
- **Scalability**: Each transformation type manages its own processing strategy

For complete architecture details, see [Services.Abstractions README](../FractalDataWorks.Services.Abstractions/README.md).
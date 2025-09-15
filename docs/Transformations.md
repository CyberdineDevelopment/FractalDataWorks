# Transformations Framework

## Overview
The FractalDataWorks Transformations framework provides comprehensive data processing capabilities with both sequential and parallel processing options for large-scale data transformation operations.

## Architecture

### Three-Layer System
1. **FractalDataWorks.Transformations.Abstractions** - Core interfaces and base classes
2. **FractalDataWorks.Transformations** - Simple sequential transformations  
3. **FractalDataWorks.Transformations.Parallel** - High-performance parallel transformations using TPL Dataflow

## Available Transformations

### Simple Transformations (Sequential Processing)

For smaller datasets or operations requiring sequential processing:

#### TextMapper
```csharp
[EnumOption("TextMapper")]
public sealed class TextMapper : SimpleTransformationTypeBase<string, string>
{
    public override string Transform(string input)
    {
        return input?.ToUpperInvariant() ?? string.Empty;
    }
}
```

#### DataFilter
```csharp
[EnumOption("DataFilter")]
public sealed class DataFilter : SimpleTransformationTypeBase<bool, object>
{
    public override bool Transform(object input)
    {
        return input != null;
    }
}
```

### Parallel Transformations (High-Throughput Processing)

| Transformation | Purpose | Supported Formats |
|---|---|---|
| **ParallelMapper** | Map/transform data in parallel | CSV, JSON, XML, Parquet |
| **ParallelFilter** | Filter data based on predicates | CSV, JSON, XML, Parquet |
| **ParallelAggregator** | Aggregate data across streams | CSV, JSON, XML, Parquet |
| **ParallelConverter** | Convert between data formats | CSV, JSON, XML, Parquet |
| **ParallelDeduplicator** | Remove duplicate records | CSV, JSON, XML, Parquet |
| **ParallelEnricher** | Enrich data with additional info | CSV, JSON, XML, Parquet |
| **ParallelJoiner** | Join multiple data streams | CSV, JSON, XML, Parquet |
| **ParallelProcessor** | Generic parallel processing | CSV, JSON, XML, Parquet |
| **ParallelSorter** | Sort large datasets | CSV, JSON, XML, Parquet |
| **ParallelValidator** | Validate data in parallel | CSV, JSON, XML, Parquet |
| **BatchProcessor** | Process data in optimized batches | CSV, JSON, XML, Parquet, Avro |
| **DataflowTransformer** | Advanced TPL Dataflow operations | CSV, JSON, XML, Parquet |
| **PartitionedProcessor** | Process using partitioning | CSV, JSON, XML, Parquet |
| **PipelineProcessor** | Chain transformations in pipeline | CSV, JSON, XML, Parquet |
| **StreamingTransformer** | Real-time streaming processing | CSV, JSON, XML, Parquet |

## Configuration

### Simple Transformation Configuration
```csharp
public class SimpleTransformationConfiguration : ConfigurationBase<SimpleTransformationConfiguration>
{
    public override string SectionName => "SimpleTransformation";
    
    public string InputSource { get; set; } = string.Empty;
    public string OutputDestination { get; set; } = string.Empty;
    public int BatchSize { get; set; } = 1000;
    public string InputFormat { get; set; } = "json";
    public string OutputFormat { get; set; } = "json";
}
```

### Parallel Transformation Configuration
```csharp
public class ParallelTransformationConfiguration : ConfigurationBase<ParallelTransformationConfiguration>
{
    public override string SectionName => "ParallelTransformation";
    
    public string InputSource { get; set; } = string.Empty;
    public string OutputDestination { get; set; } = string.Empty;
    public int BatchSize { get; set; } = 5000;
    public int MaxParallelism { get; set; } = Environment.ProcessorCount;
    public int BufferCapacity { get; set; } = 10000;
    public bool EnableRetries { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
}
```

## Performance Characteristics

### Simple Transformations
- **Throughput**: ~2,000 records/second
- **Memory Efficiency**: 95% (very efficient)
- **CPU Intensity**: 30% (low CPU usage)
- **I/O Intensity**: 20% (minimal I/O)
- **Optimal Batch Size**: 1,000 records
- **Scaling**: Sequential processing only

### Parallel Transformations
- **Throughput**: ~50,000+ records/second (varies by type)
- **Memory Efficiency**: 70-85% (efficient with buffering)
- **CPU Intensity**: 70-90% (high CPU utilization)
- **I/O Intensity**: 40-60% (moderate to high I/O)
- **Optimal Batch Size**: 5,000-10,000 records
- **Scaling**: Linear scaling with CPU cores

## Usage Patterns

### Simple Transformation Service
```csharp
public class SimpleTransformationService : ServiceBase<SimpleTransformationProvider, SimpleTransformationConfiguration>
{
    public async Task<IFdwResult<TransformationResult>> TransformAsync(
        SimpleTransformationConfiguration config, 
        CancellationToken cancellationToken = default)
    {
        using var activity = StartActivity();
        
        var validationResult = await ValidateConfigurationAsync(config, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return FdwResult<TransformationResult>.Failure(validationResult.Message);
        }
        
        var result = await Executor.ExecuteAsync(config, cancellationToken);
        return FdwResult<TransformationResult>.Success(result);
    }
}
```

### Resource Estimation
```csharp
public class TransformationResourceEstimate
{
    public long EstimatedMemoryUsage { get; set; }        // Bytes
    public TimeSpan EstimatedCpuTime { get; set; }        // Processing time
    public long EstimatedIoOperations { get; set; }       // I/O operation count
    public int RecommendedParallelism { get; set; }       // Optimal thread count
}
```

## Status and Monitoring

### Transformation Status
Available statuses: Pending, Queued, Running, Completed, CompletedWithWarnings, Failed, Cancelled, Paused

### Validation Severity
Available severities: Info, Warning, Error, Critical

## Best Practices

### Choosing Transformation Types

**Use Simple Transformations for:**
- Small datasets (< 10,000 records)
- Operations requiring sequential processing
- Simple text transformations
- Low-resource environments

**Use Parallel Transformations for:**
- Large datasets (> 100,000 records)
- CPU-intensive operations
- Independent record processing
- High-throughput requirements

### Configuration Guidelines

1. **Batch Size Optimization**
   - Simple: 1,000 records for optimal memory usage
   - Parallel: 5,000-10,000 records for throughput

2. **Parallelism Settings**
   - Start with `Environment.ProcessorCount`
   - Monitor CPU utilization and adjust
   - Consider I/O bottlenecks for file operations

3. **Buffer Capacity**
   - Set to 2-3x batch size for optimal throughput
   - Increase for high-latency I/O operations

### Error Handling
```csharp
public class RetryConfiguration
{
    public int MaxAttempts { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(1);
    public double BackoffMultiplier { get; set; } = 2.0;
}

public class DeadLetterQueueConfiguration  
{
    public bool Enabled { get; set; } = true;
    public string QueuePath { get; set; } = string.Empty;
    public int MaxRetainedMessages { get; set; } = 1000;
}
```

## Creating Custom Transformations

### Custom Simple Transformation
```csharp
[EnumOption("CustomTextProcessor")]
public sealed class CustomTextProcessor : SimpleTransformationTypeBase<string, string>
{
    public CustomTextProcessor() : base(100, "CustomTextProcessor", TransformationCategory.DataTransformation) { }

    public override string Transform(string input)
    {
        return input?.Trim().Replace(" ", "_") ?? string.Empty;
    }
}
```

### Custom Parallel Transformation
```csharp
[EnumOption("CustomParallelProcessor")]
public sealed class CustomParallelProcessor : ParallelTransformationType
{
    public CustomParallelProcessor() : base(200, "CustomParallelProcessor") { }
    
    public override IReadOnlyList<string> SupportedInputFormats => new[] { "csv", "json", "custom" };
    public override IReadOnlyList<string> SupportedOutputFormats => new[] { "csv", "json", "custom" };
}
```

## Integration with Enhanced Enums

```csharp
[EnumCollection("TransformationTypes", EnumGenerationMode.Singletons, EnumStorageMode.Dictionary)]
public abstract class TransformationTypeCollectionBase : EnumCollectionBase<TransformationTypeBase>
{
}

// Usage
var textMapper = TransformationTypes.TextMapper();
var parallelMapper = ParallelTransformationTypes.ParallelMapper();
```
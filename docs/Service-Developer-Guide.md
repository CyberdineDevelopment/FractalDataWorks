# FractalDataWorks Service Development Guide

## Quick Start: Setting Up a New Service Project

This guide walks junior developers through creating production-ready services using the FractalDataWorks framework. Follow these steps to build robust, scalable services with minimal boilerplate.

## Table of Contents

1. [Project Setup](#project-setup)
2. [Creating Your First Service](#creating-your-first-service)
3. [Configuration Management](#configuration-management)
4. [Service Registration](#service-registration)
5. [Using Services](#using-services)
6. [Testing Services](#testing-services)
7. [Common Patterns](#common-patterns)
8. [Troubleshooting](#troubleshooting)

## Project Setup

### Step 1: Create a New Project

```bash
# Create a new class library project
dotnet new classlib -n MyCompany.Services.DataProcessor -f net10.0

# Add to solution
dotnet sln add MyCompany.Services.DataProcessor/MyCompany.Services.DataProcessor.csproj
```

**Note:** This guide uses .NET 10.0 (Release Candidate). Adjust the target framework as needed for your environment.

### Step 2: Add FractalDataWorks Dependencies

Edit your `.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core Framework -->
    <ProjectReference Include="..\FractalDataWorks.Services\FractalDataWorks.Services.csproj" />
    <ProjectReference Include="..\FractalDataWorks.Services.Abstractions\FractalDataWorks.Services.Abstractions.csproj" />
    <ProjectReference Include="..\FractalDataWorks.ServiceTypes\FractalDataWorks.ServiceTypes.csproj" />

    <!-- Source Generators (Analyzers only, not referenced) -->
    <ProjectReference Include="..\FractalDataWorks.ServiceTypes.SourceGenerators\FractalDataWorks.ServiceTypes.SourceGenerators.csproj"
                      OutputItemType="Analyzer"
                      ReferenceOutputAssembly="false" />
    <ProjectReference Include="..\FractalDataWorks.Collections.SourceGenerators\FractalDataWorks.Collections.SourceGenerators.csproj"
                      OutputItemType="Analyzer"
                      ReferenceOutputAssembly="false" />

    <!-- Configuration & Results -->
    <ProjectReference Include="..\FractalDataWorks.Configuration\FractalDataWorks.Configuration.csproj" />
    <ProjectReference Include="..\FractalDataWorks.Results\FractalDataWorks.Results.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Required NuGet packages -->
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" />
    <PackageReference Include="FluentValidation" />
  </ItemGroup>
</Project>
```

### Step 3: Project Structure

Create the following folder structure:

```
MyCompany.Services.DataProcessor/
├── Commands/
│   ├── ProcessDataCommand.cs
│   └── QueryDataCommand.cs
├── Configuration/
│   ├── DataProcessorConfiguration.cs
│   └── DataProcessorConfigurationValidator.cs
├── Services/
│   ├── DataProcessorService.cs
│   └── IDataProcessorService.cs
├── Factories/
│   └── DataProcessorFactory.cs
├── ServiceTypes/
│   └── DataProcessorServiceType.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

## Creating Your First Service

### Step 1: Define the Service Interface

```csharp
// Services/IDataProcessorService.cs
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Results;
using MyCompany.Services.DataProcessor.Commands;
using MyCompany.Services.DataProcessor.Configuration;

namespace MyCompany.Services.DataProcessor.Services;

/// <summary>
/// Interface for data processing operations.
/// </summary>
public interface IDataProcessorService : IFdwService<DataCommand, DataProcessorConfiguration>
{
    /// <summary>
    /// Processes a batch of data records.
    /// </summary>
    Task<IFdwResult<ProcessingResult>> ProcessBatchAsync(ProcessDataCommand command);

    /// <summary>
    /// Queries processed data.
    /// </summary>
    Task<IFdwResult<QueryResult>> QueryAsync(QueryDataCommand command);
}
```

### Step 2: Create the Command Classes

```csharp
// Commands/DataCommand.cs
using FractalDataWorks.Services.Abstractions.Commands;

namespace MyCompany.Services.DataProcessor.Commands;

/// <summary>
/// Base command for all data operations.
/// </summary>
public abstract class DataCommand : ICommand
{
    /// <summary>
    /// Gets or sets the command identifier.
    /// </summary>
    public string CommandId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the timestamp when the command was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the user who initiated the command.
    /// </summary>
    public string? InitiatedBy { get; set; }
}

// Commands/ProcessDataCommand.cs
namespace MyCompany.Services.DataProcessor.Commands;

/// <summary>
/// Command to process a batch of data records.
/// </summary>
public class ProcessDataCommand : DataCommand
{
    /// <summary>
    /// Gets or sets the data records to process.
    /// </summary>
    public List<DataRecord> Records { get; set; } = new();

    /// <summary>
    /// Gets or sets processing options.
    /// </summary>
    public ProcessingOptions Options { get; set; } = new();
}

/// <summary>
/// Represents a single data record.
/// </summary>
public class DataRecord
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Processing configuration options.
/// </summary>
public class ProcessingOptions
{
    public bool ValidateInput { get; set; } = true;
    public bool EnableParallelProcessing { get; set; } = false;
    public int MaxParallelism { get; set; } = 4;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
}
```

### Step 3: Create the Configuration

```csharp
// Configuration/DataProcessorConfiguration.cs
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using FluentValidation.Results;

namespace MyCompany.Services.DataProcessor.Configuration;

/// <summary>
/// Configuration for the data processor service.
/// </summary>
public class DataProcessorConfiguration : IFdwConfiguration
{
    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public string Name { get; set; } = "DataProcessor";

    /// <summary>
    /// Gets or sets the connection string for the data store.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum batch size for processing.
    /// </summary>
    public int MaxBatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the processing timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets whether to enable caching.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache duration in minutes.
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 5;

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    public IFdwResult<ValidationResult> Validate()
    {
        var validator = new DataProcessorConfigurationValidator();
        var result = validator.Validate(this);

        if (result.IsValid)
        {
            return FdwResult<ValidationResult>.Success(result);
        }

        var errors = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        return FdwResult<ValidationResult>.Failure(errors);
    }
}

// Configuration/DataProcessorConfigurationValidator.cs
using FluentValidation;

namespace MyCompany.Services.DataProcessor.Configuration;

/// <summary>
/// Validator for DataProcessorConfiguration.
/// </summary>
public class DataProcessorConfigurationValidator : AbstractValidator<DataProcessorConfiguration>
{
    public DataProcessorConfigurationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Service name is required");

        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("Connection string is required")
            .Must(BeAValidConnectionString)
            .WithMessage("Invalid connection string format");

        RuleFor(x => x.MaxBatchSize)
            .InclusiveBetween(1, 10000)
            .WithMessage("Batch size must be between 1 and 10000");

        RuleFor(x => x.TimeoutSeconds)
            .InclusiveBetween(1, 3600)
            .WithMessage("Timeout must be between 1 second and 1 hour");

        RuleFor(x => x.CacheDurationMinutes)
            .InclusiveBetween(1, 1440)
            .When(x => x.EnableCaching)
            .WithMessage("Cache duration must be between 1 minute and 24 hours");
    }

    private bool BeAValidConnectionString(string connectionString)
    {
        // Basic validation - in production, use proper connection string validation
        return !string.IsNullOrWhiteSpace(connectionString)
            && connectionString.Contains("=");
    }
}
```

### Step 4: Implement the Service

```csharp
// Services/DataProcessorService.cs
using System.Collections.Concurrent;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;
using MyCompany.Services.DataProcessor.Commands;
using MyCompany.Services.DataProcessor.Configuration;

namespace MyCompany.Services.DataProcessor.Services;

/// <summary>
/// Implementation of the data processor service.
/// </summary>
public class DataProcessorService : ServiceBase<DataCommand, DataProcessorConfiguration, DataProcessorService>,
    IDataProcessorService
{
    private readonly ConcurrentDictionary<string, object> _cache;
    private readonly SemaphoreSlim _processingLock;

    /// <summary>
    /// Initializes a new instance of the DataProcessorService.
    /// </summary>
    public DataProcessorService(
        ILogger<DataProcessorService> logger,
        DataProcessorConfiguration configuration)
        : base(logger, configuration)
    {
        _cache = new ConcurrentDictionary<string, object>();
        _processingLock = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Processes a batch of data records.
    /// </summary>
    public async Task<IFdwResult<ProcessingResult>> ProcessBatchAsync(ProcessDataCommand command)
    {
        Logger.LogInformation("Processing batch with {Count} records", command.Records.Count);

        try
        {
            // Validate batch size
            if (command.Records.Count > Configuration.MaxBatchSize)
            {
                return FdwResult<ProcessingResult>.Failure(
                    $"Batch size {command.Records.Count} exceeds maximum {Configuration.MaxBatchSize}");
            }

            // Apply timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Configuration.TimeoutSeconds));

            // Process records
            var results = new List<ProcessedRecord>();

            if (command.Options.EnableParallelProcessing)
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = command.Options.MaxParallelism,
                    CancellationToken = cts.Token
                };

                await Parallel.ForEachAsync(command.Records, parallelOptions, async (record, ct) =>
                {
                    var processed = await ProcessSingleRecordAsync(record, ct);
                    lock (results)
                    {
                        results.Add(processed);
                    }
                });
            }
            else
            {
                foreach (var record in command.Records)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    var processed = await ProcessSingleRecordAsync(record, cts.Token);
                    results.Add(processed);
                }
            }

            var result = new ProcessingResult
            {
                ProcessedCount = results.Count,
                SuccessCount = results.Count(r => r.Success),
                FailedCount = results.Count(r => !r.Success),
                ProcessedRecords = results,
                ProcessingTime = DateTime.UtcNow - command.CreatedAt
            };

            Logger.LogInformation("Batch processing completed. Success: {Success}, Failed: {Failed}",
                result.SuccessCount, result.FailedCount);

            return FdwResult<ProcessingResult>.Success(result);
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("Batch processing timed out after {Timeout} seconds", Configuration.TimeoutSeconds);
            return FdwResult<ProcessingResult>.Failure("Processing timeout exceeded");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing batch");
            return FdwResult<ProcessingResult>.Failure($"Processing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Queries processed data.
    /// </summary>
    public async Task<IFdwResult<QueryResult>> QueryAsync(QueryDataCommand command)
    {
        Logger.LogInformation("Querying data with criteria: {Criteria}", command.Criteria);

        try
        {
            // Check cache if enabled
            if (Configuration.EnableCaching)
            {
                var cacheKey = $"query_{command.GetHashCode()}";
                if (_cache.TryGetValue(cacheKey, out var cached) && cached is QueryResult cachedResult)
                {
                    Logger.LogDebug("Returning cached query result");
                    return FdwResult<QueryResult>.Success(cachedResult);
                }
            }

            // Simulate async data query
            await Task.Delay(100);

            var result = new QueryResult
            {
                Records = GenerateSampleData(command.Criteria),
                TotalCount = 42,
                PageNumber = command.PageNumber,
                PageSize = command.PageSize
            };

            // Cache result if enabled
            if (Configuration.EnableCaching)
            {
                var cacheKey = $"query_{command.GetHashCode()}";
                _cache.TryAdd(cacheKey, result);

                // Schedule cache expiration
                _ = Task.Delay(TimeSpan.FromMinutes(Configuration.CacheDurationMinutes))
                    .ContinueWith(_ => _cache.TryRemove(cacheKey, out _));
            }

            return FdwResult<QueryResult>.Success(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error querying data");
            return FdwResult<QueryResult>.Failure($"Query failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Base implementation for command execution.
    /// </summary>
    public override async Task<IFdwResult> Execute(DataCommand command)
    {
        return command switch
        {
            ProcessDataCommand processCmd => await ProcessBatchAsync(processCmd),
            QueryDataCommand queryCmd => await QueryAsync(queryCmd),
            _ => FdwResult.Failure($"Unknown command type: {command.GetType().Name}")
        };
    }

    /// <summary>
    /// Generic command execution with typed result.
    /// </summary>
    public override async Task<IFdwResult<TOut>> Execute<TOut>(DataCommand command)
    {
        var result = await Execute(command);

        if (result.Error)
        {
            return FdwResult<TOut>.Failure(result.Error);
        }

        if (result.Value is TOut typedValue)
        {
            return FdwResult<TOut>.Success(typedValue);
        }

        return FdwResult<TOut>.Failure($"Cannot cast result to {typeof(TOut).Name}");
    }

    private async Task<ProcessedRecord> ProcessSingleRecordAsync(DataRecord record, CancellationToken cancellationToken)
    {
        await _processingLock.WaitAsync(cancellationToken);
        try
        {
            // Simulate processing
            await Task.Delay(10, cancellationToken);

            return new ProcessedRecord
            {
                Id = record.Id,
                Success = true,
                ProcessedAt = DateTime.UtcNow,
                Result = $"Processed: {record.Content}"
            };
        }
        catch (Exception ex)
        {
            return new ProcessedRecord
            {
                Id = record.Id,
                Success = false,
                ProcessedAt = DateTime.UtcNow,
                Error = ex.Message
            };
        }
        finally
        {
            _processingLock.Release();
        }
    }

    private List<DataRecord> GenerateSampleData(string criteria)
    {
        // Generate sample data based on criteria
        return Enumerable.Range(1, 10)
            .Select(i => new DataRecord
            {
                Id = $"record_{i}",
                Content = $"Sample data {i} matching {criteria}",
                Metadata = new Dictionary<string, object>
                {
                    ["created"] = DateTime.UtcNow.AddDays(-i),
                    ["criteria"] = criteria
                }
            })
            .ToList();
    }
}

// Supporting classes for results
public class ProcessingResult
{
    public int ProcessedCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<ProcessedRecord> ProcessedRecords { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
}

public class ProcessedRecord
{
    public string Id { get; set; } = string.Empty;
    public bool Success { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string? Result { get; set; }
    public string? Error { get; set; }
}

public class QueryResult
{
    public List<DataRecord> Records { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
```

### Step 5: Create the Factory

```csharp
// Factories/DataProcessorFactory.cs
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;
using MyCompany.Services.DataProcessor.Configuration;
using MyCompany.Services.DataProcessor.Services;

namespace MyCompany.Services.DataProcessor.Factories;

/// <summary>
/// Factory for creating DataProcessorService instances.
/// </summary>
/// <remarks>
/// Note: ServiceFactoryBase has minimal constraints:
/// - TService only requires 'class' (not IFdwService)
/// - TConfiguration requires 'class, IFdwConfiguration'
/// This allows flexibility in service implementation.
/// </remarks>
public class DataProcessorFactory : ServiceFactoryBase<IDataProcessorService, DataProcessorConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the DataProcessorFactory.
    /// </summary>
    public DataProcessorFactory(ILogger<DataProcessorFactory> logger) : base(logger)
    {
        // Base class handles all the heavy lifting
        // FastGenericNew will automatically create instances
        // Configuration validation is handled automatically
    }

    // That's it! The base class does everything else:
    // - Validates configuration
    // - Uses FastGenericNew for performance
    // - Logs creation events
    // - Returns proper Result types
}
```

### Step 6: Create the ServiceType

```csharp
// ServiceTypes/DataProcessorServiceType.cs
using FractalDataWorks.ServiceTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyCompany.Services.DataProcessor.Configuration;
using MyCompany.Services.DataProcessor.Factories;
using MyCompany.Services.DataProcessor.Services;

namespace MyCompany.Services.DataProcessor.ServiceTypes;

/// <summary>
/// Service type definition for the data processor service.
/// </summary>
public class DataProcessorServiceType : ServiceTypeBase<IDataProcessorService, DataProcessorConfiguration, DataProcessorFactory>
{
    /// <summary>
    /// Initializes a new instance of the DataProcessorServiceType.
    /// </summary>
    public DataProcessorServiceType() : base(100, "DataProcessor", "Processing")
    {
    }

    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public override string SectionName => "Services:DataProcessor";

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public override string DisplayName => "Data Processor Service";

    /// <summary>
    /// Gets the description.
    /// </summary>
    public override string Description => "Processes and queries data records with configurable batching and caching";

    /// <summary>
    /// Registers the service with dependency injection.
    /// </summary>
    public override void Register(IServiceCollection services)
    {
        // Register the factory
        services.AddSingleton<DataProcessorFactory>();

        // Register the service as transient (new instance per request)
        services.AddTransient<IDataProcessorService, DataProcessorService>();

        // Optionally register a factory method for direct DI resolution
        services.AddTransient(provider =>
        {
            var factory = provider.GetRequiredService<DataProcessorFactory>();
            var config = provider.GetRequiredService<IConfiguration>();
            var serviceConfig = config.GetSection(SectionName).Get<DataProcessorConfiguration>()
                ?? new DataProcessorConfiguration();

            var result = factory.Create(serviceConfig);
            return result.IsSuccess ? result.Value! : throw new InvalidOperationException(result.Error);
        });
    }

    /// <summary>
    /// Configures the service type.
    /// </summary>
    public override void Configure(IConfiguration configuration)
    {
        // Validate configuration exists
        var section = configuration.GetSection(SectionName);
        if (!section.Exists())
        {
            throw new InvalidOperationException($"Configuration section '{SectionName}' not found");
        }

        // Bind and validate configuration
        var config = section.Get<DataProcessorConfiguration>();
        if (config == null)
        {
            throw new InvalidOperationException($"Failed to bind configuration for '{SectionName}'");
        }

        var validationResult = config.Validate();
        if (!validationResult.IsSuccess)
        {
            throw new InvalidOperationException($"Configuration validation failed: {validationResult.Error}");
        }
    }
}
```

## Configuration Management

### Step 1: Add Configuration to appsettings.json

```json
{
  "Services": {
    "DataProcessor": {
      "Name": "MainDataProcessor",
      "ConnectionString": "Server=localhost;Database=DataDB;Integrated Security=true",
      "MaxBatchSize": 500,
      "TimeoutSeconds": 180,
      "EnableCaching": true,
      "CacheDurationMinutes": 10
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "MyCompany.Services.DataProcessor": "Debug"
    }
  }
}
```

### Step 2: Environment-Specific Configuration

Create `appsettings.Development.json`:

```json
{
  "Services": {
    "DataProcessor": {
      "ConnectionString": "Server=dev-server;Database=DataDB_Dev;Integrated Security=true",
      "MaxBatchSize": 100,
      "EnableCaching": false
    }
  }
}
```

Create `appsettings.Production.json`:

```json
{
  "Services": {
    "DataProcessor": {
      "ConnectionString": "#{Azure.KeyVault.DataProcessorConnectionString}#",
      "MaxBatchSize": 1000,
      "TimeoutSeconds": 300,
      "CacheDurationMinutes": 30
    }
  }
}
```

## Service Registration

### Step 1: Create Extension Methods

```csharp
// Extensions/ServiceCollectionExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using MyCompany.Services.DataProcessor.Factories;
using MyCompany.Services.DataProcessor.Services;
using MyCompany.Services.DataProcessor.ServiceTypes;

namespace MyCompany.Services.DataProcessor.Extensions;

/// <summary>
/// Extension methods for service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds data processor services to the service collection.
    /// </summary>
    public static IServiceCollection AddDataProcessorService(this IServiceCollection services)
    {
        // Register the service type (which handles all registrations)
        var serviceType = new DataProcessorServiceType();
        serviceType.Register(services);

        return services;
    }

    /// <summary>
    /// Adds data processor services with custom configuration.
    /// </summary>
    public static IServiceCollection AddDataProcessorService(
        this IServiceCollection services,
        Action<DataProcessorConfiguration> configure)
    {
        // Register base services
        services.AddDataProcessorService();

        // Override configuration
        services.Configure<DataProcessorConfiguration>(configure);

        return services;
    }
}
```

### Step 2: Register in Program.cs or Startup.cs

```csharp
// Program.cs (minimal API)
using MyCompany.Services.DataProcessor.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddLogging();

// Add data processor service
builder.Services.AddDataProcessorService();

// Or with inline configuration
builder.Services.AddDataProcessorService(config =>
{
    config.MaxBatchSize = 200;
    config.EnableCaching = true;
});

// If using ServiceTypes collection (automatic discovery)
ServiceTypes.RegisterAll(builder.Services);

var app = builder.Build();

// Configure pipeline
app.UseRouting();
app.MapControllers();

app.Run();
```

## Using Services

### Option 1: Direct Factory Usage

```csharp
using Microsoft.AspNetCore.Mvc;
using MyCompany.Services.DataProcessor.Commands;
using MyCompany.Services.DataProcessor.Configuration;
using MyCompany.Services.DataProcessor.Factories;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly DataProcessorFactory _factory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataController> _logger;

    public DataController(
        DataProcessorFactory factory,
        IConfiguration configuration,
        ILogger<DataController> logger)
    {
        _factory = factory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessData([FromBody] List<DataRecord> records)
    {
        // Get configuration
        var config = _configuration.GetSection("Services:DataProcessor")
            .Get<DataProcessorConfiguration>() ?? new DataProcessorConfiguration();

        // Create service instance
        var serviceResult = _factory.Create(config);
        if (!serviceResult.IsSuccess)
        {
            return BadRequest(serviceResult.Error);
        }

        var service = serviceResult.Value!;

        // Create and execute command
        var command = new ProcessDataCommand
        {
            Records = records,
            Options = new ProcessingOptions
            {
                EnableParallelProcessing = true,
                MaxParallelism = 4
            }
        };

        var result = await service.ProcessBatchAsync(command);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return StatusCode(500, result.Error);
    }
}
```

### Option 2: Dependency Injection

```csharp
[ApiController]
[Route("api/[controller]")]
public class DataControllerV2 : ControllerBase
{
    private readonly IDataProcessorService _dataProcessor;
    private readonly ILogger<DataControllerV2> _logger;

    public DataControllerV2(
        IDataProcessorService dataProcessor,
        ILogger<DataControllerV2> logger)
    {
        _dataProcessor = dataProcessor;
        _logger = logger;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessData([FromBody] List<DataRecord> records)
    {
        var command = new ProcessDataCommand
        {
            Records = records,
            InitiatedBy = User.Identity?.Name
        };

        var result = await _dataProcessor.ProcessBatchAsync(command);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(500, result.Error);
    }

    [HttpGet("query")]
    public async Task<IActionResult> QueryData(
        [FromQuery] string criteria,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var command = new QueryDataCommand
        {
            Criteria = criteria,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _dataProcessor.QueryAsync(command);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(500, result.Error);
    }
}
```

### Option 3: Service Provider Pattern

```csharp
using FractalDataWorks.Services;

[ApiController]
[Route("api/[controller]")]
public class DataControllerV3 : ControllerBase
{
    private readonly IServiceFactoryProvider _serviceProvider;

    public DataControllerV3(IServiceFactoryProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpPost("process/{serviceName}")]
    public async Task<IActionResult> ProcessWithNamedService(
        string serviceName,
        [FromBody] List<DataRecord> records)
    {
        // Get service by name from configuration
        var serviceResult = await _serviceProvider.GetService<IDataProcessorService>(serviceName);

        if (!serviceResult.IsSuccess)
        {
            return NotFound($"Service '{serviceName}' not found");
        }

        var service = serviceResult.Value!;
        var command = new ProcessDataCommand { Records = records };
        var result = await service.ProcessBatchAsync(command);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(500, result.Error);
    }
}
```

## Testing Services

### Unit Testing

```csharp
using Xunit;
using Shouldly;
using Microsoft.Extensions.Logging;
using Moq;
using MyCompany.Services.DataProcessor.Services;
using MyCompany.Services.DataProcessor.Configuration;
using MyCompany.Services.DataProcessor.Commands;

public class DataProcessorServiceTests
{
    private readonly ILogger<DataProcessorService> _logger;
    private readonly DataProcessorConfiguration _configuration;
    private readonly DataProcessorService _service;

    public DataProcessorServiceTests()
    {
        _logger = new Mock<ILogger<DataProcessorService>>().Object;
        _configuration = new DataProcessorConfiguration
        {
            Name = "TestProcessor",
            ConnectionString = "Server=test;Database=test;",
            MaxBatchSize = 10,
            TimeoutSeconds = 30,
            EnableCaching = false
        };
        _service = new DataProcessorService(_logger, _configuration);
    }

    [Fact]
    public async Task ProcessBatchAsync_WithValidData_ShouldSucceed()
    {
        // Arrange
        var command = new ProcessDataCommand
        {
            Records = new List<DataRecord>
            {
                new() { Id = "1", Content = "Test 1" },
                new() { Id = "2", Content = "Test 2" }
            }
        };

        // Act
        var result = await _service.ProcessBatchAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ProcessedCount.ShouldBe(2);
        result.Value.SuccessCount.ShouldBe(2);
    }

    [Fact]
    public async Task ProcessBatchAsync_ExceedingMaxBatchSize_ShouldFail()
    {
        // Arrange
        var records = Enumerable.Range(1, 20)
            .Select(i => new DataRecord { Id = i.ToString(), Content = $"Test {i}" })
            .ToList();

        var command = new ProcessDataCommand { Records = records };

        // Act
        var result = await _service.ProcessBatchAsync(command);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("exceeds maximum");
    }

    [Fact]
    public async Task QueryAsync_WithCachingDisabled_ShouldNotCache()
    {
        // Arrange
        var command = new QueryDataCommand
        {
            Criteria = "test",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result1 = await _service.QueryAsync(command);
        var result2 = await _service.QueryAsync(command);

        // Assert
        result1.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();
        // With caching disabled, both calls should execute the query
        result1.Value.ShouldNotBeSameAs(result2.Value);
    }
}
```

### Integration Testing

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

public class DataProcessorIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DataProcessorIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override configuration for testing
                services.Configure<DataProcessorConfiguration>(config =>
                {
                    config.ConnectionString = "Server=testserver;Database=testdb;";
                    config.EnableCaching = false;
                });
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ProcessEndpoint_ShouldProcessDataSuccessfully()
    {
        // Arrange
        var records = new[]
        {
            new { Id = "1", Content = "Test 1", Metadata = new Dictionary<string, object>() },
            new { Id = "2", Content = "Test 2", Metadata = new Dictionary<string, object>() }
        };

        var json = JsonSerializer.Serialize(records);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/data/process", content);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ProcessingResult>(responseContent);

        result.ShouldNotBeNull();
        result.ProcessedCount.ShouldBe(2);
        result.SuccessCount.ShouldBe(2);
    }
}
```

## Common Patterns

### Pattern 1: Service Composition

```csharp
/// <summary>
/// Composite service that orchestrates multiple services.
/// </summary>
public class DataPipelineService : ServiceBase<PipelineCommand, PipelineConfiguration, DataPipelineService>
{
    private readonly IDataProcessorService _processor;
    private readonly IDataValidatorService _validator;
    private readonly IDataStorageService _storage;

    public DataPipelineService(
        ILogger<DataPipelineService> logger,
        PipelineConfiguration configuration,
        IDataProcessorService processor,
        IDataValidatorService validator,
        IDataStorageService storage)
        : base(logger, configuration)
    {
        _processor = processor;
        _validator = validator;
        _storage = storage;
    }

    public override async Task<IFdwResult> Execute(PipelineCommand command)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(command.Data);
        if (!validationResult.IsSuccess)
            return validationResult;

        // Process
        var processingResult = await _processor.ProcessBatchAsync(
            new ProcessDataCommand { Records = command.Data });
        if (!processingResult.IsSuccess)
            return processingResult;

        // Store
        var storageResult = await _storage.StoreAsync(processingResult.Value);

        return storageResult;
    }
}
```

### Pattern 2: Service Decorators

```csharp
/// <summary>
/// Decorator that adds caching to any service.
/// </summary>
public class CachedServiceDecorator<TService> : IFdwService
    where TService : IFdwService
{
    private readonly TService _innerService;
    private readonly IMemoryCache _cache;
    private readonly ILogger _logger;

    public CachedServiceDecorator(
        TService innerService,
        IMemoryCache cache,
        ILogger<CachedServiceDecorator<TService>> logger)
    {
        _innerService = innerService;
        _cache = cache;
        _logger = logger;
    }

    public string Id => _innerService.Id;
    public string ServiceType => $"Cached<{_innerService.ServiceType}>";
    public bool IsAvailable => _innerService.IsAvailable;

    public async Task<IFdwResult> Execute(ICommand command)
    {
        var cacheKey = $"{ServiceType}_{command.GetHashCode()}";

        if (_cache.TryGetValue<IFdwResult>(cacheKey, out var cached))
        {
            _logger.LogDebug("Cache hit for {Key}", cacheKey);
            return cached!;
        }

        var result = await _innerService.Execute(command);

        if (result.IsSuccess)
        {
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        }

        return result;
    }
}
```

### Pattern 3: Service Health Checks

```csharp
/// <summary>
/// Health check for services.
/// </summary>
public class ServiceHealthCheck<TService> : IHealthCheck
    where TService : IFdwService
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceHealthCheck(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var service = _serviceProvider.GetService<TService>();

            if (service == null)
                return HealthCheckResult.Unhealthy("Service not registered");

            if (!service.IsAvailable)
                return HealthCheckResult.Degraded("Service not available");

            // Optionally perform a test operation
            var testCommand = new HealthCheckCommand();
            var result = await service.Execute(testCommand);

            return result.IsSuccess
                ? HealthCheckResult.Healthy("Service is healthy")
                : HealthCheckResult.Unhealthy($"Service check failed: {result.Error}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Service health check failed", ex);
        }
    }
}

// Register in Program.cs
builder.Services.AddHealthChecks()
    .AddCheck<ServiceHealthCheck<IDataProcessorService>>("data_processor");
```

## Troubleshooting

### Issue: Service Creation Fails

**Symptoms**: Factory returns failure result when creating service.

**Common Causes**:
1. Invalid configuration
2. Missing dependencies in DI container
3. Constructor parameter mismatch

**Solution**:
```csharp
// Enable detailed logging
builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Debug);

// Check configuration validation
var config = new DataProcessorConfiguration { /* ... */ };
var validationResult = config.Validate();
if (!validationResult.IsSuccess)
{
    Console.WriteLine($"Validation errors: {validationResult.Error}");
}

// Verify DI registration
var services = builder.Services.BuildServiceProvider();
var factory = services.GetService<DataProcessorFactory>();
if (factory == null)
{
    Console.WriteLine("Factory not registered!");
}
```

### Issue: Configuration Not Loading

**Symptoms**: Configuration values are default/empty.

**Solution**:
```csharp
// Verify configuration section exists
var config = builder.Configuration;
var section = config.GetSection("Services:DataProcessor");
if (!section.Exists())
{
    Console.WriteLine("Configuration section missing!");
}

// Check binding
var boundConfig = section.Get<DataProcessorConfiguration>();
Console.WriteLine($"Loaded config: {JsonSerializer.Serialize(boundConfig)}");
```

### Issue: Source Generators Not Working

**Symptoms**: ServiceTypes collection is empty or not generated.

**Solution**:
1. Clean and rebuild solution
2. Check .csproj references:
```xml
<ProjectReference Include="...\FractalDataWorks.ServiceTypes.SourceGenerators.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```
3. Enable source generator logging:
```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)\GeneratedFiles</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

### Issue: Performance Problems

**Symptoms**: Service operations are slow.

**Solutions**:
1. Enable FastGenericNew:
```csharp
// Ensure FastGenericNew package is installed
<PackageReference Include="FastGenericNew.SourceGenerator" PrivateAssets="all" />
```

2. Use appropriate service lifetime:
```csharp
// Singleton for stateless services
services.AddSingleton<IStatelessService, StatelessService>();

// Scoped for per-request state
services.AddScoped<IRequestService, RequestService>();

// Transient for lightweight services
services.AddTransient<ILightweightService, LightweightService>();
```

3. Enable caching where appropriate:
```csharp
services.AddMemoryCache();
services.Decorate<IDataProcessorService, CachedServiceDecorator<IDataProcessorService>>();
```

## Best Practices Summary

1. **Always validate configuration** - Use FluentValidation for comprehensive validation
2. **Use structured logging** - Include context in all log messages
3. **Return Result types** - Never throw exceptions from services
4. **Implement cancellation** - Support CancellationToken in all async operations
5. **Design for testability** - Use interfaces and dependency injection
6. **Document thoroughly** - XML documentation on all public members
7. **Follow naming conventions** - Consistent naming across the codebase
8. **Leverage source generation** - Use ServiceTypes for automatic discovery
9. **Monitor service health** - Implement health checks for production
10. **Cache appropriately** - Balance performance with data freshness

## Next Steps

1. Explore advanced patterns in the samples directory
2. Review the architecture documentation
3. Join the developer community discussions
4. Contribute improvements back to the framework

## Resources

- [Services Framework Documentation](./Services-Documentation.md)
- [Connections Framework Documentation](./Connections-Documentation.md)
- [API Reference](./api/index.html)
- [Sample Projects](../samples/)
- [GitHub Repository](https://github.com/fractaldataworks/developerkit)

---

*This guide is maintained by the FractalDataWorks Engineering Team. For questions or improvements, please submit an issue or pull request.*
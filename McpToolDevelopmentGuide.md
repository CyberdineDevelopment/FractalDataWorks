# MCP Tool Development Guide

## Overview

This guide provides comprehensive instructions for developing MCP (Model Context Protocol) tools within the FractalDataWorks framework. MCP tools are plugins that extend the framework's capabilities for AI-assisted development, code analysis, and automation.

## Architecture Overview

### Core Components

```
FractalDataWorks.McpTools.Abstractions/
├── IMcpTool.cs                 # Base interface for all tools
└── (additional abstractions)

FractalDataWorks.McpTools.{Category}/
├── {Category}ServiceType.cs    # Service registration
├── {Category}ToolService.cs    # Tool management
└── Tools/                       # Individual tool implementations
    ├── {ToolName}Tool.cs
    └── ...

FractalDataWorks.Results/
├── FdwResult.cs                 # Result pattern implementation
└── IFdwResult.cs                # Result interfaces
```

## Creating a New MCP Tool

### Step 1: Choose or Create a Category

MCP tools are organized by category. Current categories:
- **CodeAnalysis** - Static analysis and diagnostics
- **SessionManagement** - Workspace and session handling
- **VirtualEditing** - Non-destructive code modifications
- **Refactoring** - Code transformation and restructuring
- **TypeAnalysis** - Type system analysis
- **ProjectDependencies** - Dependency graph analysis
- **ServerManagement** - MCP server lifecycle

### Step 2: Create the Tool Class

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.McpTools.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Messages;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.McpTools.CodeAnalysis.Tools;

/// <summary>
/// Analyzes code complexity metrics for methods and classes.
/// </summary>
public class ComplexityAnalyzerTool : IMcpTool
{
    private readonly ILogger<ComplexityAnalyzerTool> _logger;

    public ComplexityAnalyzerTool(ILogger<ComplexityAnalyzerTool> logger)
    {
        _logger = logger;
    }

    public string Name => "analyze_complexity";

    public string Description => "Analyzes cyclomatic complexity and code metrics";

    public string Category => "CodeAnalysis";

    public bool IsEnabled => true;

    public int Priority => 100; // Lower = higher priority

    public async Task<IFdwResult<object>> ExecuteAsync(
        object? arguments,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate and parse arguments
            var validationResult = await ValidateArgumentsAsync(arguments, cancellationToken);
            if (validationResult.IsFailure)
            {
                return FdwResult<object>.Failure(validationResult.Message);
            }

            var args = arguments as ComplexityAnalyzerArguments;

            // Perform analysis
            _logger.LogInformation("Analyzing complexity for {FilePath}", args?.FilePath);

            var metrics = await AnalyzeComplexityAsync(args!, cancellationToken);

            // Return successful result
            var successMessage = new FractalMessage(
                MessageSeverity.Information,
                $"Analyzed {metrics.MethodCount} methods",
                "COMPLEXITY_ANALYZED",
                Name);

            return FdwResult<object>.Success(metrics, successMessage);
        }
        catch (OperationCanceledException)
        {
            return FdwResult<object>.Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing complexity");
            return FdwResult<object>.Failure($"Analysis failed: {ex.Message}");
        }
    }

    public async Task<IFdwResult> ValidateArgumentsAsync(
        object? arguments,
        CancellationToken cancellationToken = default)
    {
        if (arguments == null)
        {
            return FdwResult.Failure("Arguments are required");
        }

        if (arguments is not ComplexityAnalyzerArguments args)
        {
            return FdwResult.Failure("Invalid argument type");
        }

        if (string.IsNullOrWhiteSpace(args.FilePath))
        {
            return FdwResult.Failure("FilePath is required");
        }

        // Additional validation
        await Task.CompletedTask; // Async validation if needed

        return FdwResult.Success();
    }

    private async Task<ComplexityMetrics> AnalyzeComplexityAsync(
        ComplexityAnalyzerArguments args,
        CancellationToken cancellationToken)
    {
        // Implementation details
        await Task.Delay(100, cancellationToken); // Simulate work

        return new ComplexityMetrics
        {
            FilePath = args.FilePath,
            MethodCount = 10,
            AverageComplexity = 5.2,
            MaxComplexity = 15
        };
    }
}

// Argument models
public class ComplexityAnalyzerArguments
{
    public string FilePath { get; set; } = string.Empty;
    public bool IncludeTests { get; set; }
    public int ComplexityThreshold { get; set; } = 10;
}

// Result models
public class ComplexityMetrics
{
    public string FilePath { get; set; } = string.Empty;
    public int MethodCount { get; set; }
    public double AverageComplexity { get; set; }
    public int MaxComplexity { get; set; }
    public List<MethodComplexity> Methods { get; set; } = new();
}

public class MethodComplexity
{
    public string Name { get; set; } = string.Empty;
    public int Complexity { get; set; }
    public int LineCount { get; set; }
}
```

### Step 3: Register the Tool

Add the tool to the appropriate ToolService:

```csharp
namespace FractalDataWorks.McpTools.CodeAnalysis;

public class CodeAnalysisToolService
{
    private readonly ILogger<CodeAnalysisToolService> _logger;
    private readonly List<IMcpTool> _tools;
    private readonly IServiceProvider _serviceProvider;

    public CodeAnalysisToolService(
        ILogger<CodeAnalysisToolService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        ServiceName = "Code Analysis Tools";
        Category = "CodeAnalysis";

        _tools = new List<IMcpTool>();
        RegisterTools();
    }

    public string ServiceName { get; }
    public string Category { get; }

    public IEnumerable<IMcpTool> GetTools() => _tools
        .Where(t => t.IsEnabled)
        .OrderBy(t => t.Priority);

    private void RegisterTools()
    {
        // Register individual tools
        _tools.Add(ActivatorUtilities.CreateInstance<ComplexityAnalyzerTool>(_serviceProvider));
        _tools.Add(ActivatorUtilities.CreateInstance<DuplicateCodeDetectorTool>(_serviceProvider));
        // Add more tools...

        _logger.LogInformation("Registered {Count} code analysis tools", _tools.Count);
    }
}
```

### Step 4: Create the ServiceType

```csharp
using System;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.McpTools.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.McpTools.CodeAnalysis;

public class CodeAnalysisServiceType : ServiceTypeBase
{
    public CodeAnalysisServiceType() : base(1002, "CodeAnalysis", "Development Tools")
    {
    }

    public override Type ServiceType => typeof(CodeAnalysisToolService);

    public override Type? ConfigurationType => typeof(CodeAnalysisConfiguration);

    public override Type? FactoryType => null;

    public override string SectionName => "MCP:CodeAnalysis";

    public override string DisplayName => "Code Analysis Tools";

    public override string Description => "Code analysis tools and diagnostic services";

    public override void Register(IServiceCollection services)
    {
        // Register configuration
        services.Configure<CodeAnalysisConfiguration>(
            Configuration.GetSection(SectionName));

        // Register service
        services.AddSingleton<CodeAnalysisToolService>();

        // Register individual tools if needed
        services.AddTransient<ComplexityAnalyzerTool>();
    }

    public override void Configure(IConfiguration configuration)
    {
        base.Configure(configuration);
        // Additional configuration if needed
    }
}

public class CodeAnalysisConfiguration
{
    public bool EnableComplexityAnalysis { get; set; } = true;
    public int DefaultComplexityThreshold { get; set; } = 10;
    public List<string> ExcludedPaths { get; set; } = new();
}
```

## Advanced Tool Patterns

### 1. Stateful Tools with Sessions

```csharp
public class SessionAwareTool : IMcpTool
{
    private readonly ISessionManager _sessionManager;
    private readonly ConcurrentDictionary<Guid, ToolSession> _sessions;

    public async Task<IFdwResult<object>> ExecuteAsync(
        object? arguments,
        CancellationToken cancellationToken)
    {
        var args = arguments as SessionArguments;

        // Get or create session
        var session = await GetOrCreateSessionAsync(args!.SessionId);

        try
        {
            // Perform operation with session context
            return await ExecuteWithSessionAsync(session, args, cancellationToken);
        }
        finally
        {
            // Update session state
            await UpdateSessionAsync(session);
        }
    }

    private async Task<ToolSession> GetOrCreateSessionAsync(Guid? sessionId)
    {
        if (sessionId.HasValue && _sessions.TryGetValue(sessionId.Value, out var existing))
        {
            return existing;
        }

        var newSession = new ToolSession
        {
            Id = sessionId ?? Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            State = new Dictionary<string, object>(StringComparer.Ordinal)
        };

        _sessions.TryAdd(newSession.Id, newSession);
        return newSession;
    }
}
```

### 2. Batch Processing Tools

```csharp
public class BatchProcessingTool : IMcpTool
{
    public async Task<IFdwResult<object>> ExecuteAsync(
        object? arguments,
        CancellationToken cancellationToken)
    {
        var args = arguments as BatchArguments;
        var results = new List<BatchItemResult>();
        var errors = new List<IFdwMessage>();

        // Process items in parallel with controlled concurrency
        var semaphore = new SemaphoreSlim(args!.MaxConcurrency);
        var tasks = args.Items.Select(async item =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = await ProcessItemAsync(item, cancellationToken);
                lock (results)
                {
                    if (result.IsSuccess)
                        results.Add(result.Value!);
                    else
                        errors.Add(new FractalMessage(
                            MessageSeverity.Error,
                            result.Message,
                            "BATCH_ITEM_FAILED",
                            Name));
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        // Return aggregate result
        var batchResult = new BatchResult
        {
            ProcessedCount = results.Count,
            FailedCount = errors.Count,
            Items = results
        };

        return errors.Count > 0
            ? FdwResult<object>.Success(batchResult, errors)
            : FdwResult<object>.Success(batchResult);
    }
}
```

### 3. Pipeline Tools

```csharp
public class PipelineTool : IMcpTool
{
    private readonly List<IPipelineStage> _stages;

    public async Task<IFdwResult<object>> ExecuteAsync(
        object? arguments,
        CancellationToken cancellationToken)
    {
        var context = new PipelineContext(arguments);

        // Execute pipeline stages
        foreach (var stage in _stages)
        {
            if (!stage.ShouldExecute(context))
                continue;

            var stageResult = await stage.ExecuteAsync(context, cancellationToken);

            if (stageResult.IsFailure)
            {
                return FdwResult<object>.Failure(
                    $"Pipeline failed at stage {stage.Name}: {stageResult.Message}");
            }

            context.AddStageResult(stage.Name, stageResult.Value);

            // Check if pipeline should continue
            if (stage.IsTerminal && stageResult.Value is bool shouldContinue && !shouldContinue)
            {
                break;
            }
        }

        return FdwResult<object>.Success(context.GetFinalResult());
    }
}

public interface IPipelineStage
{
    string Name { get; }
    bool IsTerminal { get; }
    bool ShouldExecute(PipelineContext context);
    Task<IFdwResult<object>> ExecuteAsync(
        PipelineContext context,
        CancellationToken cancellationToken);
}
```

### 4. Caching Tools

```csharp
public class CachedAnalysisTool : IMcpTool
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public async Task<IFdwResult<object>> ExecuteAsync(
        object? arguments,
        CancellationToken cancellationToken)
    {
        var args = arguments as AnalysisArguments;
        var cacheKey = GenerateCacheKey(args!);

        // Check cache first
        if (_cache.TryGetValue<AnalysisResult>(cacheKey, out var cached))
        {
            _logger.LogDebug("Returning cached result for {Key}", cacheKey);
            return FdwResult<object>.Success(
                cached,
                "Result retrieved from cache");
        }

        // Perform analysis
        var result = await PerformAnalysisAsync(args, cancellationToken);

        if (result.IsSuccess)
        {
            // Cache successful results
            _cache.Set(cacheKey, result.Value, _cacheExpiration);
        }

        return result;
    }

    private string GenerateCacheKey(AnalysisArguments args)
    {
        // Generate deterministic cache key
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(args)));
        return $"analysis_{Convert.ToBase64String(hash)}";
    }
}
```

## Testing MCP Tools

### Unit Testing

```csharp
using Xunit;
using Shouldly;
using Microsoft.Extensions.Logging;
using NSubstitute;

public class ComplexityAnalyzerToolTests
{
    private readonly ComplexityAnalyzerTool _tool;
    private readonly ILogger<ComplexityAnalyzerTool> _logger;

    public ComplexityAnalyzerToolTests()
    {
        _logger = Substitute.For<ILogger<ComplexityAnalyzerTool>>();
        _tool = new ComplexityAnalyzerTool(_logger);
    }

    [Fact]
    public async Task ExecuteAsync_ValidArguments_ReturnsSuccess()
    {
        // Arrange
        var args = new ComplexityAnalyzerArguments
        {
            FilePath = "/test/file.cs",
            ComplexityThreshold = 10
        };

        // Act
        var result = await _tool.ExecuteAsync(args);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var metrics = result.Value as ComplexityMetrics;
        metrics.ShouldNotBeNull();
        metrics.FilePath.ShouldBe("/test/file.cs");
    }

    [Fact]
    public async Task ValidateArgumentsAsync_NullArguments_ReturnsFailure()
    {
        // Act
        var result = await _tool.ValidateArgumentsAsync(null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldContain("required");
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequested_ReturnsCancelled()
    {
        // Arrange
        var args = new ComplexityAnalyzerArguments { FilePath = "/test/file.cs" };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _tool.ExecuteAsync(args, cts.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Message.ShouldContain("cancelled");
    }
}
```

### Integration Testing

```csharp
public class CodeAnalysisIntegrationTests : IClassFixture<McpToolFixture>
{
    private readonly McpToolFixture _fixture;

    public CodeAnalysisIntegrationTests(McpToolFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CodeAnalysisToolService_RegistersAllTools()
    {
        // Arrange
        var service = _fixture.GetService<CodeAnalysisToolService>();

        // Act
        var tools = service.GetTools().ToList();

        // Assert
        tools.ShouldNotBeEmpty();
        tools.ShouldAllBe(t => t.Category == "CodeAnalysis");
        tools.ShouldAllBe(t => t.IsEnabled);
    }

    [Fact]
    public async Task ComplexityAnalyzer_AnalyzesRealCode()
    {
        // Arrange
        var tool = _fixture.GetTool<ComplexityAnalyzerTool>();
        var testFile = Path.Combine(_fixture.TestDataPath, "ComplexMethod.cs");

        var args = new ComplexityAnalyzerArguments
        {
            FilePath = testFile,
            ComplexityThreshold = 5
        };

        // Act
        var result = await tool.ExecuteAsync(args);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var metrics = result.Value as ComplexityMetrics;
        metrics!.Methods.ShouldContain(m => m.Complexity > 5);
    }
}

public class McpToolFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    public string TestDataPath { get; }

    public McpToolFixture()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .Build();

        // Register services
        var serviceType = new CodeAnalysisServiceType();
        serviceType.Configure(configuration);
        serviceType.Register(services);

        ServiceProvider = services.BuildServiceProvider();
        TestDataPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData");
    }

    public T GetService<T>() where T : notnull
        => ServiceProvider.GetRequiredService<T>();

    public T GetTool<T>() where T : IMcpTool
    {
        var service = GetService<CodeAnalysisToolService>();
        return service.GetTools().OfType<T>().First();
    }

    public void Dispose()
    {
        (ServiceProvider as IDisposable)?.Dispose();
    }
}
```

## Error Handling Best Practices

### 1. Use Structured Messages

```csharp
public async Task<IFdwResult<object>> ExecuteAsync(
    object? arguments,
    CancellationToken cancellationToken)
{
    try
    {
        // Operation
        return FdwResult<object>.Success(result);
    }
    catch (FileNotFoundException ex)
    {
        var errorMessage = new FractalMessage(
            MessageSeverity.Error,
            $"File not found: {ex.FileName}",
            "FILE_NOT_FOUND",
            Name)
        {
            Metadata = new Dictionary<string, object>
            {
                ["FilePath"] = ex.FileName ?? string.Empty,
                ["StackTrace"] = ex.StackTrace ?? string.Empty
            }
        };

        return FdwResult<object>.Failure(errorMessage);
    }
    catch (UnauthorizedAccessException ex)
    {
        return FdwResult<object>.Failure(new FractalMessage(
            MessageSeverity.Error,
            "Access denied to requested resource",
            "ACCESS_DENIED",
            Name));
    }
}
```

### 2. Aggregate Multiple Errors

```csharp
public async Task<IFdwResult<object>> ExecuteAsync(
    object? arguments,
    CancellationToken cancellationToken)
{
    var errors = new List<IFdwMessage>();
    var warnings = new List<IFdwMessage>();

    // Validation phase
    foreach (var validator in _validators)
    {
        var validationResult = await validator.ValidateAsync(arguments);
        if (validationResult.IsFailure)
        {
            errors.AddRange(validationResult.Messages);
        }
    }

    if (errors.Count > 0)
    {
        return FdwResult<object>.Failure(errors);
    }

    // Processing with warning collection
    var processResult = await ProcessAsync(arguments, warnings);

    return warnings.Count > 0
        ? FdwResult<object>.Success(processResult, warnings)
        : FdwResult<object>.Success(processResult);
}
```

### 3. Provide Recovery Options

```csharp
public async Task<IFdwResult<object>> ExecuteAsync(
    object? arguments,
    CancellationToken cancellationToken)
{
    var result = await TryPrimaryOperationAsync(arguments, cancellationToken);

    if (result.IsFailure && IsRecoverable(result))
    {
        _logger.LogWarning("Primary operation failed, attempting fallback");

        var fallbackResult = await TryFallbackOperationAsync(arguments, cancellationToken);

        if (fallbackResult.IsSuccess)
        {
            var warningMessage = new FractalMessage(
                MessageSeverity.Warning,
                "Operation succeeded using fallback method",
                "FALLBACK_USED",
                Name);

            return FdwResult<object>.Success(fallbackResult.Value, warningMessage);
        }
    }

    return result;
}

private bool IsRecoverable(IFdwResult<object> result)
{
    // Check if error is recoverable
    return result.Messages.Any(m =>
        m.Code == "NETWORK_ERROR" ||
        m.Code == "TIMEOUT");
}
```

## Performance Optimization

### 1. Async All the Way

```csharp
public async Task<IFdwResult<object>> ExecuteAsync(
    object? arguments,
    CancellationToken cancellationToken)
{
    // Use ConfigureAwait(false) for library code
    var data = await LoadDataAsync().ConfigureAwait(false);

    // Use ValueTask for hot paths
    var processed = await ProcessDataFastAsync(data).ConfigureAwait(false);

    // Parallel processing for independent operations
    var tasks = processed.Select(item =>
        TransformItemAsync(item, cancellationToken));

    var results = await Task.WhenAll(tasks).ConfigureAwait(false);

    return FdwResult<object>.Success(results);
}

private async ValueTask<ProcessedData> ProcessDataFastAsync(RawData data)
{
    // Fast synchronous path
    if (data.IsCached)
    {
        return GetFromCache(data.Id);
    }

    // Async path only when necessary
    return await ComputeAsync(data).ConfigureAwait(false);
}
```

### 2. Resource Management

```csharp
public class ResourceIntensiveTool : IMcpTool, IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ObjectPool<WorkBuffer> _bufferPool;

    public ResourceIntensiveTool()
    {
        _semaphore = new SemaphoreSlim(Environment.ProcessorCount);
        _bufferPool = new DefaultObjectPool<WorkBuffer>(
            new WorkBufferPoolPolicy(),
            Environment.ProcessorCount * 2);
    }

    public async Task<IFdwResult<object>> ExecuteAsync(
        object? arguments,
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        var buffer = _bufferPool.Get();

        try
        {
            return await ProcessWithBufferAsync(buffer, arguments, cancellationToken);
        }
        finally
        {
            _bufferPool.Return(buffer);
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}
```

### 3. Lazy Initialization

```csharp
public class ExpensiveInitializationTool : IMcpTool
{
    private readonly Lazy<ExpensiveResource> _resource;

    public ExpensiveInitializationTool()
    {
        _resource = new Lazy<ExpensiveResource>(
            () => InitializeExpensiveResource(),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async Task<IFdwResult<object>> ExecuteAsync(
        object? arguments,
        CancellationToken cancellationToken)
    {
        // Resource only initialized on first use
        var resource = _resource.Value;
        return await resource.ProcessAsync(arguments, cancellationToken);
    }

    private ExpensiveResource InitializeExpensiveResource()
    {
        // Expensive initialization
        return new ExpensiveResource();
    }
}
```

## Deployment and Configuration

### Configuration Schema

```json
{
  "MCP": {
    "CodeAnalysis": {
      "EnableComplexityAnalysis": true,
      "DefaultComplexityThreshold": 10,
      "ExcludedPaths": [
        "*/bin/*",
        "*/obj/*",
        "*/node_modules/*"
      ],
      "Tools": {
        "ComplexityAnalyzer": {
          "Enabled": true,
          "Priority": 100,
          "MaxFileSize": 1048576,
          "Timeout": "00:00:30"
        }
      }
    }
  }
}
```

### Environment-Specific Configuration

```csharp
public class EnvironmentAwareTool : IMcpTool
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public bool IsEnabled => _environment.IsProduction()
        ? _configuration.GetValue<bool>("MCP:ProductionTools:Enabled")
        : _configuration.GetValue<bool>("MCP:DevelopmentTools:Enabled");

    public async Task<IFdwResult<object>> ExecuteAsync(
        object? arguments,
        CancellationToken cancellationToken)
    {
        if (_environment.IsDevelopment())
        {
            // Additional logging/debugging in development
            _logger.LogDebug("Executing with arguments: {Arguments}",
                JsonSerializer.Serialize(arguments));
        }

        // Execute based on environment
        return _environment.IsProduction()
            ? await ExecuteProductionAsync(arguments, cancellationToken)
            : await ExecuteDevelopmentAsync(arguments, cancellationToken);
    }
}
```

### Health Checks

```csharp
public class HealthCheckableTool : IMcpTool, IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify tool can execute
            var testResult = await ValidateArgumentsAsync(
                new TestArguments(),
                cancellationToken);

            return testResult.IsSuccess
                ? HealthCheckResult.Healthy("Tool is operational")
                : HealthCheckResult.Unhealthy($"Tool validation failed: {testResult.Message}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Tool health check failed",
                ex);
        }
    }
}
```

## Monitoring and Telemetry

### Metrics Collection

```csharp
public class InstrumentedTool : IMcpTool
{
    private readonly IMetrics _metrics;
    private readonly ILogger<InstrumentedTool> _logger;

    public async Task<IFdwResult<object>> ExecuteAsync(
        object? arguments,
        CancellationToken cancellationToken)
    {
        using var activity = Activity.StartActivity("MCP.Tool.Execute");
        activity?.SetTag("tool.name", Name);
        activity?.SetTag("tool.category", Category);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await PerformOperationAsync(arguments, cancellationToken);

            // Record metrics
            _metrics.Measure.Counter.Increment(
                "mcp.tool.executions",
                new MetricTags("tool", Name, "status", "success"));

            _metrics.Measure.Timer.Time(
                "mcp.tool.duration",
                stopwatch.Elapsed,
                new MetricTags("tool", Name));

            return result;
        }
        catch (Exception ex)
        {
            _metrics.Measure.Counter.Increment(
                "mcp.tool.executions",
                new MetricTags("tool", Name, "status", "failure"));

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}
```

### Structured Logging

```csharp
public class WellLoggedTool : IMcpTool
{
    public async Task<IFdwResult<object>> ExecuteAsync(
        object? arguments,
        CancellationToken cancellationToken)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["ToolName"] = Name,
            ["CorrelationId"] = Guid.NewGuid(),
            ["Arguments"] = arguments
        }))
        {
            _logger.LogInformation(
                "Starting tool execution for {ToolName}",
                Name);

            try
            {
                var result = await ProcessAsync(arguments, cancellationToken);

                _logger.LogInformation(
                    "Tool execution completed successfully for {ToolName} in {Duration}ms",
                    Name,
                    stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Tool execution failed for {ToolName} after {Duration}ms",
                    Name,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }
}
```

## Security Considerations

### Input Validation

```csharp
public class SecureTool : IMcpTool
{
    private readonly IValidator<ToolArguments> _validator;

    public async Task<IFdwResult> ValidateArgumentsAsync(
        object? arguments,
        CancellationToken cancellationToken)
    {
        var args = arguments as ToolArguments;

        // Validate structure
        var validationResult = await _validator.ValidateAsync(args!, cancellationToken);
        if (!validationResult.IsValid)
        {
            return FdwResult.Failure(string.Join(", ",
                validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        // Sanitize paths
        if (!IsPathSafe(args!.FilePath))
        {
            return FdwResult.Failure("Invalid file path");
        }

        // Check permissions
        if (!await HasPermissionAsync(args.FilePath))
        {
            return FdwResult.Failure("Insufficient permissions");
        }

        return FdwResult.Success();
    }

    private bool IsPathSafe(string path)
    {
        // Prevent path traversal
        var fullPath = Path.GetFullPath(path);
        var allowedRoot = Path.GetFullPath("/allowed/root");
        return fullPath.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase);
    }
}
```

### Secret Management

```csharp
public class SecretAwareTool : IMcpTool
{
    private readonly ISecretManager _secretManager;

    public async Task<IFdwResult<object>> ExecuteAsync(
        object? arguments,
        CancellationToken cancellationToken)
    {
        var args = arguments as SecureArguments;

        // Never log secrets
        _logger.LogInformation("Executing secure operation for user {UserId}",
            args!.UserId); // OK to log
        // Never: _logger.LogInformation($"Using key {args.ApiKey}");

        // Retrieve secrets securely
        var apiKey = await _secretManager.GetSecretAsync(
            args.ApiKeyName,
            cancellationToken);

        // Use and dispose secrets properly
        using (apiKey)
        {
            return await PerformSecureOperationAsync(
                apiKey.Value,
                cancellationToken);
        }
    }
}
```

## Troubleshooting Guide

### Common Issues

1. **Tool not appearing in service**
   - Verify tool is registered in ToolService.RegisterTools()
   - Check IsEnabled property returns true
   - Ensure Category matches service category

2. **Argument validation failures**
   - Log the actual argument type received
   - Verify JSON deserialization settings
   - Check for required properties

3. **Cancellation not working**
   - Pass CancellationToken to all async operations
   - Use cancellationToken.ThrowIfCancellationRequested()
   - Configure appropriate timeouts

4. **Memory leaks**
   - Dispose IDisposable resources
   - Clear collections in stateful tools
   - Use weak references for caches

### Debug Helpers

```csharp
#if DEBUG
public class DebugTool : IMcpTool
{
    public async Task<IFdwResult<object>> ExecuteAsync(
        object? arguments,
        CancellationToken cancellationToken)
    {
        // Enable detailed debugging in debug builds
        Debugger.Break();

        // Log detailed state
        _logger.LogDebug("Current state: {State}",
            JsonSerializer.Serialize(_internalState,
                new JsonSerializerOptions { WriteIndented = true }));

        // Add debug metadata to result
        var result = await ProcessAsync(arguments, cancellationToken);

        if (result.IsSuccess && result.Value is IDictionary<string, object> dict)
        {
            dict["_debug"] = new
            {
                ExecutionTime = DateTime.UtcNow,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                ProcessId = Process.GetCurrentProcess().Id
            };
        }

        return result;
    }
}
#endif
```

## Summary

MCP tools in the FractalDataWorks framework provide a structured way to extend functionality while maintaining consistency with the Railway-Oriented Programming paradigm. Key takeaways:

1. **Always use Result types** - Never throw exceptions from public methods
2. **Implement proper validation** - Validate arguments before processing
3. **Use structured messages** - Provide rich error information via IFdwMessage
4. **Follow category conventions** - Organize tools by functional area
5. **Write comprehensive tests** - Unit and integration tests are essential
6. **Consider performance** - Use async patterns and resource pooling
7. **Implement monitoring** - Add metrics and structured logging
8. **Ensure security** - Validate inputs and handle secrets properly

For additional examples and patterns, refer to the existing tool implementations in the FractalDataWorks.McpTools.* projects.
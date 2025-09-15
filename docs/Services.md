# Services Framework

## Overview
The FractalDataWorks Services framework provides a structured approach to building scalable, maintainable services with built-in configuration, validation, logging, and error handling.

## Base Service Implementation

All services inherit from `ServiceBase<TExecutor, TConfiguration>`:

```csharp
public abstract class ServiceBase<TExecutor, TConfiguration> : IFractalService<TExecutor, TConfiguration>
    where TExecutor : class
    where TConfiguration : class
{
    protected TExecutor Executor { get; }
    protected ILogger Logger { get; }
    
    protected ServiceBase(TExecutor executor, ILogger logger)
    {
        Executor = executor ?? throw new ArgumentNullException(nameof(executor));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    protected IDisposable? StartActivity([CallerMemberName] string? operationName = null)
    {
        return Activity.StartActivity($"{GetType().Name}.{operationName}");
    }
}
```

## Service Interface Pattern

```csharp
public interface IMyDomainService : IFractalService<MyDomainExecutor, MyDomainConfiguration>
{
    // Primary operations
    Task<IFdwResult<MyDomainResult>> ProcessAsync(MyDomainConfiguration config, CancellationToken cancellationToken = default);
    
    // Validation operations
    Task<IFdwResult<ValidationResult>> ValidateConfigurationAsync(MyDomainConfiguration config, CancellationToken cancellationToken = default);
    
    // Query operations
    Task<IFdwResult<MyDomainStatus>> GetStatusAsync(string processId, CancellationToken cancellationToken = default);
    
    // Management operations
    Task<IFdwResult> CancelAsync(string processId, CancellationToken cancellationToken = default);
}
```

## Configuration Pattern

All service configurations inherit from `ConfigurationBase<T>`:

```csharp
public class MyServiceConfiguration : ConfigurationBase<MyServiceConfiguration>
{
    public override string SectionName => "MyService";
    
    public string Source { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxRetries { get; set; } = 3;
    
    protected override IValidator<MyServiceConfiguration> GetValidator()
    {
        return new MyServiceConfigurationValidator();
    }
}
```

### Configuration Validation

Using FluentValidation for type-safe configuration validation:

```csharp
public class MyServiceConfigurationValidator : AbstractValidator<MyServiceConfiguration>
{
    public MyServiceConfigurationValidator()
    {
        RuleFor(x => x.Source)
            .NotEmpty()
            .WithMessage("Source cannot be empty");
            
        RuleFor(x => x.Timeout)
            .GreaterThan(TimeSpan.Zero)
            .WithMessage("Timeout must be greater than zero");
            
        RuleFor(x => x.MaxRetries)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MaxRetries must be non-negative");
    }
}
```

## Service Implementation Example

```csharp
public class MyDomainService : ServiceBase<MyDomainExecutor, MyDomainConfiguration>, IMyDomainService
{
    public MyDomainService(MyDomainExecutor executor, ILogger<MyDomainService> logger) 
        : base(executor, logger) 
    {
    }
    
    public async Task<IFdwResult<MyDomainResult>> ProcessAsync(
        MyDomainConfiguration config, 
        CancellationToken cancellationToken = default)
    {
        using var activity = StartActivity();
        
        LogOperationStart(nameof(ProcessAsync), config);
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var validationResult = await ValidateConfigurationAsync(config, cancellationToken);
            if (!validationResult.IsSuccess)
            {
                return FdwResult<MyDomainResult>.Failure(
                    MyDomainMessages.InvalidConfiguration(validationResult.Message));
            }
            
            var result = await Executor.ExecuteAsync(config, cancellationToken);
            
            LogOperationComplete(nameof(ProcessAsync), stopwatch.Elapsed);
            return FdwResult<MyDomainResult>.Success(result);
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("Operation was cancelled");
            return FdwResult<MyDomainResult>.Failure(
                MyDomainMessages.OperationCancelled());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing request");
            return FdwResult<MyDomainResult>.Failure(
                MyDomainMessages.ProcessingFailed(ex.Message));
        }
    }
}
```

## Factory Pattern

Services use factory classes for creation and lifecycle management:

```csharp
public interface IMyDomainServiceFactory : IServiceFactory<IMyDomainService, MyDomainConfiguration>
{
    Task<IFdwResult<IMyDomainService>> CreateServiceAsync(
        MyDomainConfiguration configuration, 
        CancellationToken cancellationToken = default);
}

public class MyDomainServiceFactory : ServiceFactoryBase<IMyDomainService, MyDomainConfiguration>, IMyDomainServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public MyDomainServiceFactory(IServiceProvider serviceProvider, ILogger<MyDomainServiceFactory> logger)
        : base(logger)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<IFdwResult<IMyDomainService>> CreateServiceAsync(
        MyDomainConfiguration configuration, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var executor = _serviceProvider.GetRequiredService<MyDomainExecutor>();
            var logger = _serviceProvider.GetRequiredService<ILogger<MyDomainService>>();
            var service = new MyDomainService(executor, logger);
            
            return FdwResult<IMyDomainService>.Success(service);
        }
        catch (Exception ex)
        {
            return FdwResult<IMyDomainService>.Failure(
                MyDomainMessages.ServiceCreationFailed(ex.Message));
        }
    }
}
```

## Enhanced Enum Integration

Services integrate with Enhanced Enums for type-safe service type definitions:

```csharp
[EnumOption("MyDomainService")]
public sealed class MyDomainServiceType : ServiceTypeBase<IMyDomainService, MyDomainConfiguration, IMyDomainServiceFactory>
{
    public MyDomainServiceType() : base(1, "MyDomainService") { }
    
    public override bool SupportsScheduling => true;
    public override bool RequiresFactory => true;
    public override TimeSpan DefaultTimeout => TimeSpan.FromMinutes(10);
}
```

## Message Pattern

Services use Enhanced Enum messages for consistent error reporting:

```csharp
[EnumOption]
public sealed class ProcessingFailed : MyDomainMessageBase
{
    public ProcessingFailed() : base(2001, nameof(ProcessingFailed), MessageSeverity.Error, 
        "Processing failed: {0}", "PROCESSING_FAILED") { }
}

[EnumOption] 
public sealed class InvalidConfiguration : MyDomainMessageBase
{
    public InvalidConfiguration() : base(2002, nameof(InvalidConfiguration), MessageSeverity.Error,
        "Configuration validation failed: {0}", "INVALID_CONFIGURATION") { }
}

// Usage in service
return FdwResult<MyResult>.Failure(MyDomainMessages.ProcessingFailed(ex.Message));
```

## Service Registration

Services are registered using extension methods:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyDomainServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register configuration
        services.Configure<MyDomainConfiguration>(
            configuration.GetSection("MyDomain"));
            
        // Register executor
        services.AddScoped<MyDomainExecutor>();
        
        // Register service
        services.AddScoped<IMyDomainService, MyDomainService>();
        
        // Register factory
        services.AddScoped<IMyDomainServiceFactory, MyDomainServiceFactory>();
        
        return services;
    }
}
```

## Best Practices

### 1. Configuration Management
- Always validate configurations using FluentValidation
- Use environment-specific configuration sections
- Provide sensible defaults for optional settings

### 2. Error Handling
- Use Enhanced Enum messages for consistent error reporting
- Log errors with appropriate severity levels
- Include correlation IDs for distributed tracing

### 3. Resource Management
- Use `using` statements for activities and disposable resources
- Implement proper cancellation token handling
- Monitor and log performance metrics

### 4. Testing
- Mock executors for unit testing services
- Use configuration builders for test setup
- Test both success and failure scenarios

### 5. Dependency Injection
- Register services with appropriate lifetimes (Scoped, Singleton, Transient)
- Use factory patterns for complex service creation
- Validate service registration through health checks
# Services Framework

## Overview
The FractalDataWorks Services framework provides a ServiceType-based plugin architecture for building scalable, maintainable services with automatic discovery, registration, and type-safe configuration.

## Service Architecture Overview

The framework uses ServiceTypes to define, discover, and manage services automatically:

1. **ServiceType Implementation**: Define service capabilities as singleton types
2. **Auto-Discovery**: Source generators discover and register ServiceTypes
3. **Interface-Based Usage**: Access services through interfaces for polymorphism
4. **Configuration Management**: Dynamic configuration loading using ServiceType metadata

## Base Service Implementation

Services implement the `IFdwService` interface hierarchy:

```csharp
public interface IFdwService
{
    // Base service contract
}

public interface IFdwService<TCommand> : IFdwService
{
    Task<IFdwResult> ExecuteAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface IFdwService<TCommand, TConfiguration> : IFdwService<TCommand>
{
    Task<IFdwResult> ExecuteAsync(TCommand command, TConfiguration configuration, CancellationToken cancellationToken = default);
}
```

## ServiceType Implementation Pattern

### 1. Define ServiceType (Singleton)

```csharp
public sealed class MyDomainServiceType :
    ServiceTypeBase<IMyDomainService, MyDomainConfiguration, IMyDomainServiceFactory>
{
    public static MyDomainServiceType Instance { get; } = new();

    private MyDomainServiceType() : base(id: 1, name: "MyDomain", category: "Business") { }

    public override string SectionName => "MyDomain";
    public override string DisplayName => "My Domain Service";
    public override string Description => "Handles core business logic for My Domain operations.";

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IMyDomainServiceFactory, MyDomainServiceFactory>();
        services.AddScoped<MyDomainService>();
        services.AddScoped<MyDomainExecutor>();
    }

    public override void Configure(IConfiguration configuration)
    {
        var config = configuration.GetSection(SectionName).Get<MyDomainConfiguration>();
        if (config == null)
            throw new InvalidOperationException($"Configuration section '{SectionName}' not found");

        // Validate configuration
        var validator = new MyDomainConfigurationValidator();
        var validationResult = validator.Validate(config);
        if (!validationResult.IsValid)
            throw new InvalidOperationException($"Invalid configuration: {string.Join(", ", validationResult.Errors)}");
    }
}
```

### 2. Define Service Interface

```csharp
public interface IMyDomainService : IFdwService<MyDomainCommand, MyDomainConfiguration>
{
    // Primary operations
    Task<IFdwResult<MyDomainResult>> ProcessAsync(MyDomainCommand command, CancellationToken cancellationToken = default);

    // Validation operations
    Task<IFdwResult<ValidationResult>> ValidateConfigurationAsync(MyDomainConfiguration config, CancellationToken cancellationToken = default);

    // Query operations
    Task<IFdwResult<MyDomainStatus>> GetStatusAsync(string processId, CancellationToken cancellationToken = default);

    // Management operations
    Task<IFdwResult> CancelAsync(string processId, CancellationToken cancellationToken = default);
}
```

### 3. Define Configuration Class

Configuration classes implement appropriate interfaces and support validation:

```csharp
public class MyDomainConfiguration : IServiceConfiguration
{
    public string Source { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxRetries { get; set; } = 3;
    public bool EnableLogging { get; set; } = true;
}

public class MyDomainConfigurationValidator : AbstractValidator<MyDomainConfiguration>
{
    public MyDomainConfigurationValidator()
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
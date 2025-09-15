# Creating New Services

## Table of Contents
- [Overview](#overview)
- [Service Creation Process](#service-creation-process)
- [Step-by-Step Guide](#step-by-step-guide)
- [Integration with Framework Components](#integration-with-framework-components)
- [Making Services Schedulable](#making-services-schedulable)
- [Making Services Executable](#making-services-executable)
- [Testing Your Service](#testing-your-service)
- [Best Practices](#best-practices)

## Overview

Creating a new service in the FractalDataWorks framework follows a systematic approach that ensures consistency, type safety, and maintainability. This guide walks through creating a complete service implementation from interfaces to dependency injection registration.

## Service Creation Process

### 1. Architecture Decision
First, determine your service's place in the layered architecture:
- **Core Layer**: Framework-level services (rare)
- **Domain Abstractions Layer**: Domain-specific interfaces and base classes
- **Implementation Layer**: Concrete technology-specific implementations

### 2. Component Requirements
Every complete service implementation requires:
- Service interface
- Service implementation 
- Configuration class
- Executor/Provider class
- Enhanced Enum service type
- Factory implementation
- Message definitions
- Dependency injection registration

## Step-by-Step Guide

### Step 1: Define Domain Abstractions

Create the domain abstractions layer first:

```csharp
// File: FractalDataWorks.Services.MyDomain.Abstractions/IMyDomainService.cs
namespace FractalDataWorks.Services.MyDomain.Abstractions;

public interface IMyDomainService : IFractalService<MyDomainExecutor, MyDomainConfiguration>
{
    Task<IFdwResult<MyDomainResult>> ProcessAsync(
        MyDomainConfiguration config, 
        CancellationToken cancellationToken = default);
        
    Task<IFdwResult<MyDomainStatus>> GetStatusAsync(
        string processId, 
        CancellationToken cancellationToken = default);
}
```

### Step 2: Create Configuration Class

```csharp
// File: FractalDataWorks.Services.MyDomain.Abstractions/Configuration/MyDomainConfiguration.cs
namespace FractalDataWorks.Services.MyDomain.Abstractions.Configuration;

public class MyDomainConfiguration : ConfigurationBase<MyDomainConfiguration>
{
    public override string SectionName => "MyDomain";
    
    public string ConnectionString { get; set; } = string.Empty;
    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxRetryAttempts { get; set; } = 3;
    public bool EnableDetailedLogging { get; set; } = false;
    
    protected override IValidator<MyDomainConfiguration> GetValidator()
    {
        return new MyDomainConfigurationValidator();
    }
}

public class MyDomainConfigurationValidator : AbstractValidator<MyDomainConfiguration>
{
    public MyDomainConfigurationValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("ConnectionString cannot be empty");
            
        RuleFor(x => x.CommandTimeout)
            .GreaterThan(TimeSpan.Zero)
            .WithMessage("CommandTimeout must be positive");
            
        RuleFor(x => x.MaxRetryAttempts)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MaxRetryAttempts must be non-negative");
    }
}
```

### Step 3: Define Enhanced Enum Service Type

```csharp
// File: FractalDataWorks.Services.MyDomain.Abstractions/EnhancedEnums/MyDomainServiceTypeBase.cs
namespace FractalDataWorks.Services.MyDomain.Abstractions.EnhancedEnums;

public abstract class MyDomainServiceTypeBase<TService, TConfiguration, TFactory> 
    : ServiceTypeBase<TService, TConfiguration, TFactory>
    where TService : class, IMyDomainService
    where TConfiguration : MyDomainConfiguration  
    where TFactory : class, IServiceFactory<TService, TConfiguration>
{
    protected MyDomainServiceTypeBase(int id, string name) : base(id, name) { }
    
    // Domain-specific properties
    public abstract bool RequiresAuthentication { get; }
    public abstract TimeSpan DefaultProcessingTimeout { get; }
}

[EnumCollection("MyDomainServiceTypes", EnumGenerationMode.Singletons, EnumStorageMode.Dictionary)]
public abstract class MyDomainServiceTypeCollectionBase 
    : EnumCollectionBase<MyDomainServiceTypeBase<IMyDomainService, MyDomainConfiguration, IMyDomainServiceFactory>>
{
}
```

### Step 4: Create Messages

First, add the required project references to your `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\FractalDataWorks.Messages\FractalDataWorks.Messages.csproj" />
  <ProjectReference Include="..\FractalDataWorks.Messages.SourceGenerators\FractalDataWorks.Messages.SourceGenerators.csproj" 
                    OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
</ItemGroup>
```

Then create your message classes:

```csharp
// File: FractalDataWorks.Services.MyDomain.Abstractions/Messages/MyDomainMessage.cs
using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.MyDomain.Abstractions.Messages;

/// <summary>
/// Base class for MyDomain messages.
/// </summary>
public abstract class MyDomainMessage : ServiceMessage
{
    protected MyDomainMessage(int id, string name, MessageSeverity severity, string message, string? code = null)
        : base(id, name, severity, message, code) { }
}

/// <summary>
/// Message for processing failures.
/// </summary>
[Message("ProcessingFailed")]
public sealed class ProcessingFailedMessage : MyDomainMessage
{
    public ProcessingFailedMessage() 
        : base(3001, "ProcessingFailed", MessageSeverity.Error, 
               "MyDomain processing failed", "MYDOMAIN_PROCESSING_FAILED") { }
    
    public ProcessingFailedMessage(string reason) 
        : base(3001, "ProcessingFailed", MessageSeverity.Error, 
               $"MyDomain processing failed: {reason}", "MYDOMAIN_PROCESSING_FAILED") { }
}

/// <summary>
/// Message for invalid configuration.
/// </summary>
[Message("InvalidConfiguration")]
public sealed class InvalidConfigurationMessage : MyDomainMessage
{
    public InvalidConfigurationMessage() 
        : base(3002, "InvalidConfiguration", MessageSeverity.Error,
               "MyDomain configuration is invalid", "MYDOMAIN_INVALID_CONFIGURATION") { }
    
    public InvalidConfigurationMessage(string details) 
        : base(3002, "InvalidConfiguration", MessageSeverity.Error,
               $"MyDomain configuration is invalid: {details}", "MYDOMAIN_INVALID_CONFIGURATION") { }
}

/// <summary>
/// Collection definition for source generation.
/// </summary>
[MessageCollection("MyDomainMessages", ReturnType = typeof(IServiceMessage))]
public abstract class MyDomainMessageCollectionBase : MessageCollectionBase<MyDomainMessage>
{
    // Source generator will populate this automatically
}
```

### Step 5: Create Implementation Layer

```csharp
// File: FractalDataWorks.Services.MyDomain.SqlServer/MyDomainService.cs
namespace FractalDataWorks.Services.MyDomain.SqlServer;

public class MyDomainService : ServiceBase<MyDomainExecutor, MyDomainConfiguration>, IMyDomainService
{
    public MyDomainService(MyDomainExecutor executor, ILogger<MyDomainService> logger) 
        : base(executor, logger) { }
    
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
            Logger.LogWarning("MyDomain operation was cancelled");
            return FdwResult<MyDomainResult>.Failure(
                MyDomainMessages.OperationCancelled());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in MyDomain processing");
            return FdwResult<MyDomainResult>.Failure(
                MyDomainMessages.ProcessingFailed(ex.Message));
        }
    }
    
    public async Task<IFdwResult<MyDomainStatus>> GetStatusAsync(
        string processId, 
        CancellationToken cancellationToken = default)
    {
        using var activity = StartActivity();
        
        try
        {
            var status = await Executor.GetStatusAsync(processId, cancellationToken);
            return FdwResult<MyDomainStatus>.Success(status);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting status for process {ProcessId}", processId);
            return FdwResult<MyDomainStatus>.Failure(
                MyDomainMessages.ProcessingFailed(ex.Message));
        }
    }
}
```

### Step 6: Create Executor

```csharp
// File: FractalDataWorks.Services.MyDomain.SqlServer/MyDomainExecutor.cs
namespace FractalDataWorks.Services.MyDomain.SqlServer;

public class MyDomainExecutor
{
    private readonly ILogger<MyDomainExecutor> _logger;
    private readonly IDbConnectionFactory _connectionFactory;
    
    public MyDomainExecutor(ILogger<MyDomainExecutor> logger, IDbConnectionFactory connectionFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }
    
    public async Task<MyDomainResult> ExecuteAsync(
        MyDomainConfiguration config, 
        CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(config.ConnectionString, cancellationToken);
        
        // Implementation-specific logic here
        var result = new MyDomainResult
        {
            ProcessId = Guid.NewGuid().ToString(),
            CompletedAt = DateTime.UtcNow,
            Status = "Completed"
        };
        
        return result;
    }
    
    public async Task<MyDomainStatus> GetStatusAsync(
        string processId, 
        CancellationToken cancellationToken = default)
    {
        // Implementation-specific status checking
        return new MyDomainStatus
        {
            ProcessId = processId,
            Status = "Running",
            Progress = 0.75
        };
    }
}
```

### Step 7: Create Enhanced Enum Implementation

```csharp
// File: FractalDataWorks.Services.MyDomain.SqlServer/EnhancedEnums/SqlServerMyDomainType.cs
namespace FractalDataWorks.Services.MyDomain.SqlServer.EnhancedEnums;

[EnumOption("SqlServerMyDomain")]
public sealed class SqlServerMyDomainType : MyDomainServiceTypeBase<IMyDomainService, MyDomainConfiguration, IMyDomainServiceFactory>
{
    public SqlServerMyDomainType() : base(1, "SqlServerMyDomain") { }
    
    public override bool RequiresAuthentication => true;
    public override TimeSpan DefaultProcessingTimeout => TimeSpan.FromMinutes(10);
    public override bool SupportsScheduling => true;
    public override string Description => "SQL Server-based MyDomain service implementation";
}
```

### Step 8: Create Factory

```csharp
// File: FractalDataWorks.Services.MyDomain.SqlServer/MyDomainServiceFactory.cs
namespace FractalDataWorks.Services.MyDomain.SqlServer;

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
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
    
    public async Task<IFdwResult<IMyDomainService>> CreateServiceAsync(
        MyDomainConfiguration configuration, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate configuration
            var validationResult = await ValidateConfigurationAsync(configuration, cancellationToken);
            if (!validationResult.IsSuccess)
            {
                return FdwResult<IMyDomainService>.Failure(validationResult.Message);
            }
            
            // Create dependencies
            var executor = _serviceProvider.GetRequiredService<MyDomainExecutor>();
            var logger = _serviceProvider.GetRequiredService<ILogger<MyDomainService>>();
            
            // Create service
            var service = new MyDomainService(executor, logger);
            
            return FdwResult<IMyDomainService>.Success(service);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create MyDomain service");
            return FdwResult<IMyDomainService>.Failure(
                MyDomainMessages.ServiceCreationFailed(ex.Message));
        }
    }
}
```

### Step 9: Dependency Injection Registration

```csharp
// File: FractalDataWorks.Services.MyDomain.SqlServer/ServiceCollectionExtensions.cs
namespace FractalDataWorks.Services.MyDomain.SqlServer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyDomainSqlServerServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register configuration
        services.Configure<MyDomainConfiguration>(
            configuration.GetSection("MyDomain"));
            
        // Register executor and dependencies
        services.AddScoped<MyDomainExecutor>();
        services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
        
        // Register service and factory
        services.AddScoped<IMyDomainService, MyDomainService>();
        services.AddScoped<IMyDomainServiceFactory, MyDomainServiceFactory>();
        
        return services;
    }
}
```

## Integration with Framework Components

### Enhanced Enums Integration
Your service automatically integrates with the Enhanced Enums system:
```csharp
// Type-safe service selection
var serviceType = MyDomainServiceTypes.SqlServerMyDomain();
var factory = serviceProvider.GetRequiredService<IMyDomainServiceFactory>();
var service = await factory.CreateServiceAsync(config);
```

### Messages Integration  
Use generated message collections for consistent error handling:
```csharp
// Type-safe message creation
return FdwResult<MyResult>.Failure(MyDomainMessages.ProcessingFailed("Connection timeout"));
```

### Configuration Integration
Leverage the configuration system for environment-specific settings:
```csharp
// appsettings.json
{
  "MyDomain": {
    "ConnectionString": "Server=localhost;Database=Test;",
    "CommandTimeout": "00:05:00",
    "MaxRetryAttempts": 3
  }
}
```

### Transformations Integration
If your service processes data, integrate with the transformations framework:
```csharp
public async Task<IFdwResult<TransformationResult>> ProcessDataAsync(
    MyDomainConfiguration config,
    CancellationToken cancellationToken = default)
{
    var transformationType = TransformationTypes.TextMapper();
    var transformationService = _serviceProvider.GetRequiredService<ITransformationService>();
    
    return await transformationService.ExecuteAsync(transformationType, config, cancellationToken);
}
```

## Making Services Schedulable

To make your service work with scheduling systems:

### 1. Implement Scheduling Interface
```csharp
public interface ISchedulableMyDomainService : IMyDomainService, ISchedulableService
{
    Task<IFdwResult<ScheduleInfo>> GetScheduleInfoAsync(CancellationToken cancellationToken = default);
    Task<IFdwResult> ValidateScheduleAsync(ScheduleConfiguration schedule, CancellationToken cancellationToken = default);
}
```

### 2. Add Scheduling Properties to Enhanced Enum
```csharp
[EnumOption("SqlServerMyDomain")]
public sealed class SqlServerMyDomainType : MyDomainServiceTypeBase<IMyDomainService, MyDomainConfiguration, IMyDomainServiceFactory>
{
    public override bool SupportsScheduling => true;
    public override TimeSpan MinimumScheduleInterval => TimeSpan.FromMinutes(1);
    public override SchedulingStrategy DefaultStrategy => SchedulingStrategy.Cron;
}
```

## Making Services Executable

For services that need to be executable from command line or batch processes:

### 1. Implement Executable Interface
```csharp
public interface IExecutableMyDomainService : IMyDomainService, IExecutableService
{
    Task<IFdwResult<ExecutionResult>> ExecuteAsync(
        string[] args, 
        CancellationToken cancellationToken = default);
        
    Task<IFdwResult<ExecutionInfo>> GetExecutionInfoAsync(CancellationToken cancellationToken = default);
}
```

### 2. Add Command-Line Support
```csharp
public class MyDomainCommand : ICommand<MyDomainConfiguration>
{
    public string Name => "mydomain";
    public string Description => "Executes MyDomain processing operations";
    
    public async Task<IFdwResult> ExecuteAsync(
        MyDomainConfiguration config, 
        CancellationToken cancellationToken = default)
    {
        var serviceFactory = _serviceProvider.GetRequiredService<IMyDomainServiceFactory>();
        var service = await serviceFactory.CreateServiceAsync(config, cancellationToken);
        
        if (!service.IsSuccess)
            return FdwResult.Failure(service.Message);
            
        return await service.Value.ProcessAsync(config, cancellationToken);
    }
}
```

## Testing Your Service

### 1. Unit Tests
```csharp
public class MyDomainServiceTests
{
    [Fact]
    public async Task ProcessAsync_WithValidConfiguration_ShouldReturnSuccess()
    {
        // Arrange
        var config = new MyDomainConfiguration { ConnectionString = "test" };
        var executor = new Mock<MyDomainExecutor>();
        var logger = new Mock<ILogger<MyDomainService>>();
        var service = new MyDomainService(executor.Object, logger.Object);
        
        executor.Setup(x => x.ExecuteAsync(It.IsAny<MyDomainConfiguration>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new MyDomainResult { ProcessId = "test-123" });
        
        // Act
        var result = await service.ProcessAsync(config);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ProcessId.ShouldBe("test-123");
    }
}
```

### 2. Integration Tests
```csharp
public class MyDomainServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Service_Integration_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMyDomainSqlServerServices(Configuration);
        var serviceProvider = services.BuildServiceProvider();
        
        // Act
        var factory = serviceProvider.GetRequiredService<IMyDomainServiceFactory>();
        var service = await factory.CreateServiceAsync(TestConfiguration);
        
        // Assert
        service.IsSuccess.ShouldBeTrue();
        service.Value.ShouldNotBeNull();
    }
}
```

## Best Practices

### 1. Follow Naming Conventions
- Use consistent naming patterns across all components
- Follow the established project structure
- Use descriptive names that indicate purpose and scope

### 2. Implement Proper Error Handling
- Use Enhanced Enum messages for all errors
- Include sufficient context in error messages
- Log errors with appropriate severity levels

### 3. Configuration Validation
- Always validate configurations using FluentValidation
- Provide clear validation error messages
- Test configuration validation thoroughly

### 4. Resource Management
- Use `using` statements for disposable resources
- Implement proper cancellation token handling
- Monitor resource usage and performance

### 5. Documentation
- Document all public interfaces and methods
- Include usage examples in XML documentation
- Update architectural documentation when adding new domains
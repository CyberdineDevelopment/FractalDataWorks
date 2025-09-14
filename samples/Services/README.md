# FractalDataWorks Services Examples

This directory contains comprehensive examples demonstrating the FractalDataWorks Services framework patterns and capabilities.

## Overview

The FractalDataWorks Services framework provides:
- **ServiceBase Pattern** - Base service class with automatic validation, logging, and error handling
- **ServiceMessages System** - Type-safe, structured messaging using Enhanced Enums with source generation
- **Configuration Registry** - Centralized configuration management with FractalConfigurationBase
- **Command Pattern** - Structured command execution with comprehensive validation
- **Enhanced Enums Integration** - Rich enum types with business logic and automatic collections
- **Dependency Injection** - Full DI container integration with service factories
- **Result Pattern** - Consistent error handling using ServiceMessages and FractalResult

## Examples

### ConfigurationExample

Demonstrates comprehensive configuration patterns:
- **Basic Configuration** - Simple configuration loading and usage
- **Validated Configuration** - Configuration with FluentValidation rules
- **Hosted Configuration** - Configuration with dependency injection and hosting
- **Multi-Source Configuration** - Loading configuration from multiple sources

**Key Features Demonstrated:**
- `ConfigurationBase<T>` inheritance
- FluentValidation integration
- Enhanced Enums in configuration
- Microsoft.Extensions.Hosting integration
- Configuration binding from multiple sources

**Run the example:**
```bash
cd ConfigurationExample
dotnet run
```

### ServicePatterns

Demonstrates comprehensive service architecture patterns:
- **ServiceBase Implementation** - Services using ServiceBase with automatic validation and logging
- **ServiceMessages Integration** - Type-safe error handling using Enhanced Enum ServiceMessages
- **Configuration Registry** - Centralized configuration management patterns
- **Command Execution** - Command-based service execution with validation
- **Hosted Service** - Services in dependency injection with orchestration

**Key Features Demonstrated:**
- `ServiceBase<TCommand, TConfiguration, TService>` implementation
- `ServiceMessages` system with Enhanced Enums and source generation
- `FractalConfigurationBase` with FluentValidation integration
- Command pattern with comprehensive validation
- Service orchestration and dependency injection
- Structured logging with ServiceMessages integration
- Cross-assembly ServiceMessages discovery

**Run the example:**
```bash
cd ServicePatterns
dotnet run
```

## Architecture Patterns

### ServiceMessages Pattern

```csharp
// ServiceMessage definition using Enhanced Enums
[EnumOption]
public sealed class EmailValidationFailed : ServiceMessageBase
{
    public EmailValidationFailed() 
        : base(1010, nameof(EmailValidationFailed), MessageSeverity.Error,
               "Email validation failed for {0}: {1}", "EMAIL_VALIDATION_FAILED") { }
               
    public override ServiceMessageBase WithSeverity(MessageSeverity severity)
    {
        return new EmailValidationFailed();
    }
}

// Collection registry for source generation
[EnumCollection(CollectionName = "ServiceMessages")]
public abstract class ServiceMessageRegistry : EnumCollectionBase<ServiceMessageBase> { }

// Usage in services
public class EmailService : ServiceBase<SendEmailCommand, EmailConfiguration, EmailService>
{
    protected override async Task<IFractalResult<T>> ExecuteCore<T>(SendEmailCommand command)
    {
        if (!IsValidEmail(command.ToAddress))
        {
            return FractalResult<T>.Failure(
                ServiceMessages.EmailValidationFailed.Format(command.ToAddress, "Invalid format"));
        }
        
        // Service logic...
        return FractalResult<T>.Success(result);
    }
}
```

### Configuration Pattern

```csharp
public class EmailConfiguration : FractalConfigurationBase
{
    public override string SectionName => "EmailService";
    
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    
    public override ValidationResult Validate()
    {
        var validator = new EmailConfigurationValidator();
        return validator.Validate(this);
    }
}
```

### Command Pattern

```csharp
public class ProcessDataCommand : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public Guid CorrelationId { get; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
    public IFractalConfiguration? Configuration { get; set; }
    
    public string Data { get; set; } = string.Empty;
    
    public ValidationResult Validate()
    {
        var validator = new ProcessDataCommandValidator();
        return validator.Validate(this);
    }
}
```

### ServiceBase Pattern

```csharp
public class DataProcessingService : ServiceBase<ProcessDataCommand, DataProcessingConfiguration, DataProcessingService>
{
    private readonly IDataProcessor _processor;
    
    public DataProcessingService(
        ILogger<DataProcessingService> logger, 
        DataProcessingConfiguration configuration,
        IDataProcessor processor)
        : base(logger, configuration)
    {
        _processor = processor;
    }
    
    protected override async Task<IFractalResult<T>> ExecuteCore<T>(ProcessDataCommand command)
    {
        try
        {
            // ServiceBase handles validation automatically
            var result = await _processor.ProcessAsync(command.Data);
            
            Logger.LogInformation("Data processing completed for {CorrelationId}", command.CorrelationId);
            return FractalResult<T>.Success((T)(object)result);
        }
        catch (ValidationException ex)
        {
            return FractalResult<T>.Failure(
                ServiceMessages.ValidationFailed.Format("DataProcessing", ex.Message));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Data processing failed for {CorrelationId}", command.CorrelationId);
            return FractalResult<T>.Failure(
                ServiceMessages.OperationFailed.Format("DataProcessing"));
        }
    }
}
```

### Enhanced Enum Integration

```csharp
public sealed class Priority : EnumOptionBase<Priority>
{
    public static readonly Priority Low = new(1, "Low", TimeSpan.FromHours(24));
    public static readonly Priority Normal = new(2, "Normal", TimeSpan.FromHours(1));
    public static readonly Priority High = new(3, "High", TimeSpan.FromMinutes(15));
    
    public TimeSpan MaxProcessingTime { get; }
    
    private Priority(int id, string name, TimeSpan maxProcessingTime)
        : base(id, name)
    {
        MaxProcessingTime = maxProcessingTime;
    }
}
```

## Dependencies

All examples use these FractalDataWorks packages:
- **Core Services**: `FractalDataWorks.Services` - ServiceBase and ServiceMessages system
- **Enhanced Enums**: `FractalDataWorks.EnhancedEnums` - Enhanced Enum foundation
- **Source Generation**: `FractalDataWorks.EnhancedEnums.SourceGenerators` - Automatic ServiceMessages collection
- **Configuration**: `FractalDataWorks.Configuration` - FractalConfigurationBase and registry patterns
- **Messaging**: `FractalDataWorks.Messages` - IFractalMessage and MessageSeverity
- **Results**: `FractalDataWorks.Results` - FractalResult patterns
- **Abstractions**: Various abstraction packages for interfaces

Plus standard Microsoft packages:
- `Microsoft.Extensions.Hosting`
- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Logging`
- `FluentValidation`

## Running the Examples

Each example can be run independently:

```bash
# Configuration patterns
cd ConfigurationExample && dotnet run

# Service patterns  
cd ServicePatterns && dotnet run
```

All examples use local package references from `../../LocalPackages` to demonstrate the latest framework capabilities.
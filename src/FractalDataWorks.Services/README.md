# FractalDataWorks.Services

Core service foundation for the FractalDataWorks framework providing comprehensive service-oriented architecture patterns with dependency injection, configuration management, command execution, and Railway-Oriented Programming.

## Features

- **ServiceBase<TCommand, TConfiguration, TService>** - Abstract base class for consistent service implementation
- **ServiceFactoryBase<TService, TConfiguration>** - Type-safe factory with high-performance instantiation via FastGenericNew
- **Configuration Management** - Integrated FluentValidation for configuration validation
- **Message System** - Comprehensive messaging for service operations with source-generated collections
- **Structured Logging** - High-performance logging with LoggerMessage delegates
- **Result Pattern** - Railway-Oriented Programming with IGenericResult<T>
- **Thread Safety** - All base classes designed for concurrent usage

## Installation

```xml
<PackageReference Include="FractalDataWorks.Services" Version="1.0.0" />
```

## Quick Start

### 1. Define Your Command

```csharp
using FractalDataWorks.Services.Abstractions.Commands;

public interface IEmailCommand : ICommand
{
    string To { get; }
    string Subject { get; }
    string Body { get; }
}

public class SendEmailCommand : IEmailCommand
{
    public required string To { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public required string From { get; init; }
    public bool IsHtml { get; init; }
}
```

### 2. Create Configuration

```csharp
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results.Abstractions;
using FluentValidation;
using FluentValidation.Results;

public class EmailConfiguration : IGenericConfiguration
{
    public string Name { get; set; } = "EmailService";
    public required string SmtpServer { get; set; }
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public required string Username { get; set; }
    public required string Password { get; set; }

    public IGenericResult<ValidationResult> Validate()
    {
        var validator = new EmailConfigurationValidator();
        return GenericResult<ValidationResult>.Success(validator.Validate(this));
    }
}

public class EmailConfigurationValidator : AbstractValidator<EmailConfiguration>
{
    public EmailConfigurationValidator()
    {
        RuleFor(x => x.SmtpServer).NotEmpty();
        RuleFor(x => x.Port).InclusiveBetween(1, 65535);
    }
}
```

### 3. Implement Your Service

```csharp
using FractalDataWorks.Services;
using FractalDataWorks.Results.Abstractions;
using Microsoft.Extensions.Logging;

public class EmailService : ServiceBase<IEmailCommand, EmailConfiguration, EmailService>
{
    public EmailService(ILogger<EmailService> logger, EmailConfiguration configuration)
        : base(logger, configuration)
    {
    }

    public override async Task<IGenericResult> Execute(IEmailCommand command)
    {
        try
        {
            if (command is not SendEmailCommand sendCmd)
                return GenericResult.Failure("Invalid command type");

            // Send email logic here
            await SendEmail(sendCmd);

            Logger.LogInformation("Email sent to {To}", sendCmd.To);
            return GenericResult.Success("Email sent successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to send email");
            return GenericResult.Failure($"Failed to send email: {ex.Message}");
        }
    }

    public override async Task<IGenericResult<TOut>> Execute<TOut>(IEmailCommand command)
    {
        // Implement for commands that return values
        return GenericResult<TOut>.Failure("This service does not return values");
    }
}
```

### 4. Create Factory

```csharp
using FractalDataWorks.Services;
using FractalDataWorks.Results.Abstractions;
using Microsoft.Extensions.Logging;

public class EmailServiceFactory : ServiceFactoryBase<EmailService, EmailConfiguration>
{
    private readonly ILogger<EmailService> _serviceLogger;

    public EmailServiceFactory(
        ILogger<EmailServiceFactory> factoryLogger,
        ILogger<EmailService> serviceLogger)
        : base(factoryLogger)
    {
        _serviceLogger = serviceLogger;
    }

    public override IGenericResult<EmailService> Create(EmailConfiguration configuration)
    {
        // Validation is handled by base class
        // FastGenericNew creates the instance efficiently
        return base.Create(configuration);
    }
}
```

### 5. Register with DI

```csharp
using Microsoft.Extensions.DependencyInjection;

services.AddScoped<EmailServiceFactory>();
services.AddScoped<EmailService>();
services.AddSingleton(new EmailConfiguration
{
    SmtpServer = "smtp.gmail.com",
    Port = 587,
    EnableSsl = true,
    Username = "your-email@gmail.com",
    Password = "your-app-password"
});
```

## Key Components

### ServiceBase
- Provides common service lifecycle management
- Automatic configuration and command validation
- Structured logging with correlation IDs
- Exception handling and result wrapping
- Performance metrics collection

### ServiceFactoryBase
- Type-safe service creation
- Automatic configuration validation via FluentValidation
- Uses FastGenericNew for high-performance instantiation
- Comprehensive error handling and logging

### Message System
The library includes comprehensive message types for service operations:

- **Factory Messages** - Service creation results
- **Configuration Messages** - Configuration validation messages
- **Registration Messages** - Service registration messages
- **Service Messages** - Service operation messages

All messages are source-generated for type safety and consistency.

### Logging Infrastructure
- `ServiceBaseLog` - Core service operation logging
- `ServiceFactoryLog` - Factory creation logging
- `ServiceProviderBaseLog` - Provider operation logging
- `ServiceRegistrationLog` - Registration logging
- `PerformanceMetrics` - Performance measurement

## Service Lifecycle

1. **Factory Creation** - Factory validates configuration
2. **Service Instantiation** - FastGenericNew creates instance
3. **Command Execution** - Command validated and executed
4. **Result Wrapping** - Success/failure wrapped in IGenericResult
5. **Logging** - Operations logged with structured data

## Dependencies

- `FractalDataWorks.Services.Abstractions` - Service interfaces
- `FractalDataWorks.Configuration.Abstractions` - Configuration contracts
- `FractalDataWorks.Results` - Result pattern implementation
- `FractalDataWorks.Messages` - Message infrastructure
- `FractalDataWorks.ServiceTypes` - Service type discovery
- `FractalDataWorks.EnhancedEnums` - Type-safe enumerations
- `FastGenericNew` - High-performance instantiation
- `FluentValidation` - Configuration validation
- `Microsoft.Extensions.DependencyInjection.Abstractions` - DI integration
- `Microsoft.Extensions.Logging.Abstractions` - Logging

## Best Practices

1. **Always validate configuration** in factory Create methods
2. **Use structured logging** for all operations
3. **Return Result types** instead of throwing exceptions
4. **Implement proper disposal** patterns where needed
5. **Use CancellationToken** for async operations
6. **Register services** with appropriate lifetimes

## Advanced Features

### Custom Validation

```csharp
using FractalDataWorks.Results.Abstractions;

public override IGenericResult<EmailService> Create(EmailConfiguration configuration)
{
    var baseResult = base.Create(configuration);
    if (baseResult.Error) return baseResult;

    // Add custom validation
    if (!IsSmtpServerReachable(configuration.SmtpServer))
        return GenericResult<EmailService>.Failure("SMTP server unreachable");

    return baseResult;
}
```

### Service Registration Extensions

```csharp
using Microsoft.Extensions.DependencyInjection;

services.AddServiceFactory<EmailService, EmailConfiguration, EmailServiceFactory>(options =>
{
    options.Lifetime = ServiceLifetime.Scoped;
    options.RegisterFactory = true;
});
```

## Documentation

- [Services Architecture](../../docs/Services.md)
- [Developer Guide](../../docs/DeveloperGuide-ServiceSetup.md)
- [API Reference](https://docs.FractalDataWorks.com/api/services)

## License

Copyright © FractalDataWorks Electric Cooperative. All rights reserved.
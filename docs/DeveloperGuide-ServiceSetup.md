# FractalDataWorks Service Development Guide

## Table of Contents

1. [Quick Start](#quick-start)
2. [Setting Up a Service Project](#setting-up-a-service-project)
3. [Creating Your First Service](#creating-your-first-service)
4. [Working with Connections](#working-with-connections)
5. [Using Collections and ServiceTypes](#using-collections-and-servicetypes)
6. [Complete Example](#complete-example)
7. [Best Practices](#best-practices)
8. [Troubleshooting](#troubleshooting)

## Quick Start

### Prerequisites

- .NET 8.0 or later
- Visual Studio 2022 or JetBrains Rider
- NuGet package source configured for FractalDataWorks packages

### Install Required Packages

```xml
<ItemGroup>
  <!-- Core service framework -->
  <PackageReference Include="FractalDataWorks.Services" Version="1.0.0" />
  <PackageReference Include="FractalDataWorks.Services.Abstractions" Version="1.0.0" />

  <!-- For connection support -->
  <PackageReference Include="FractalDataWorks.Services.Connections" Version="1.0.0" />
  <PackageReference Include="FractalDataWorks.Services.Connections.Abstractions" Version="1.0.0" />

  <!-- For source generation -->
  <PackageReference Include="FractalDataWorks.ServiceTypes.SourceGenerators" Version="1.0.0"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

## Setting Up a Service Project

### 1. Create Project Structure

```
MyCompany.Services/
├── MyCompany.Services.Abstractions/
│   ├── Commands/
│   ├── Configuration/
│   └── Interfaces/
├── MyCompany.Services/
│   ├── Services/
│   ├── Factories/
│   └── ServiceTypes/
└── MyCompany.Services.Tests/
```

### 2. Create the Abstractions Project

```xml
<!-- MyCompany.Services.Abstractions.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="FractalDataWorks.Services.Abstractions" />
    <PackageReference Include="FractalDataWorks.Configuration.Abstractions" />
    <PackageReference Include="FluentValidation" />
  </ItemGroup>
</Project>
```

### 3. Create the Implementation Project

```xml
<!-- MyCompany.Services.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\MyCompany.Services.Abstractions\MyCompany.Services.Abstractions.csproj" />

    <PackageReference Include="FractalDataWorks.Services" />
    <PackageReference Include="FractalDataWorks.ServiceTypes" />
    <PackageReference Include="FractalDataWorks.ServiceTypes.SourceGenerators"
                      OutputItemType="Analyzer"
                      ReferenceOutputAssembly="false" />
    <PackageReference Include="FastGenericNew.SourceGenerator"
                      PrivateAssets="all"
                      IncludeAssets="runtime; build; native; contentfiles; analyzers; buildtransitive" />
  </ItemGroup>
</Project>
```

## Creating Your First Service

### Step 1: Define the Command

```csharp
// MyCompany.Services.Abstractions/Commands/IEmailCommand.cs
using FractalDataWorks.Services.Abstractions.Commands;

namespace MyCompany.Services.Abstractions.Commands;

public interface IEmailCommand : ICommand
{
    string To { get; }
    string Subject { get; }
    string Body { get; }
}

public class SendEmailCommand : IEmailCommand
{
    public string To { get; init; }
    public string Subject { get; init; }
    public string Body { get; init; }
    public string From { get; init; }
    public bool IsHtml { get; init; }
}
```

### Step 2: Create the Configuration

```csharp
// MyCompany.Services.Abstractions/Configuration/EmailConfiguration.cs
using FluentValidation;
using FractalDataWorks.Configuration.Abstractions;

namespace MyCompany.Services.Abstractions.Configuration;

public class EmailConfiguration : IGenericConfiguration
{
    public string Name { get; set; } = "EmailService";
    public string SmtpServer { get; set; }
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; }
    public string Password { get; set; }
    public string DefaultFrom { get; set; }
    public int TimeoutSeconds { get; set; } = 30;

    public IGenericResult<ValidationResult> Validate()
    {
        var validator = new EmailConfigurationValidator();
        var result = validator.Validate(this);
        return GenericResult<ValidationResult>.Success(result);
    }
}

public class EmailConfigurationValidator : AbstractValidator<EmailConfiguration>
{
    public EmailConfigurationValidator()
    {
        RuleFor(x => x.SmtpServer)
            .NotEmpty().WithMessage("SMTP server is required")
            .Must(BeValidHost).WithMessage("Invalid SMTP server address");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535");

        RuleFor(x => x.Username)
            .NotEmpty().When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Username is required when password is provided");

        RuleFor(x => x.DefaultFrom)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.DefaultFrom))
            .WithMessage("Invalid email address format");

        RuleFor(x => x.TimeoutSeconds)
            .GreaterThan(0).WithMessage("Timeout must be greater than 0");
    }

    private bool BeValidHost(string host)
    {
        return !string.IsNullOrWhiteSpace(host) &&
               !host.Contains(" ") &&
               Uri.CheckHostName(host) != UriHostNameType.Unknown;
    }
}
```

### Step 3: Implement the Service

```csharp
// MyCompany.Services/Services/EmailService.cs
using System.Net;
using System.Net.Mail;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;
using MyCompany.Services.Abstractions.Commands;
using MyCompany.Services.Abstractions.Configuration;

namespace MyCompany.Services.Services;

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
            if (command is not SendEmailCommand sendCommand)
            {
                return GenericResult.Failure("Invalid command type");
            }

            using var client = new SmtpClient(Configuration.SmtpServer, Configuration.Port)
            {
                EnableSsl = Configuration.EnableSsl,
                Timeout = Configuration.TimeoutSeconds * 1000,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrEmpty(Configuration.Username))
            {
                client.Credentials = new NetworkCredential(Configuration.Username, Configuration.Password);
            }

            var message = new MailMessage
            {
                From = new MailAddress(sendCommand.From ?? Configuration.DefaultFrom),
                Subject = sendCommand.Subject,
                Body = sendCommand.Body,
                IsBodyHtml = sendCommand.IsHtml
            };

            message.To.Add(sendCommand.To);

            await client.SendMailAsync(message);

            Logger.LogInformation("Email sent successfully to {Recipient}", sendCommand.To);
            return GenericResult.Success("Email sent successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to send email");
            return GenericResult.Failure($"Failed to send email: {ex.Message}");
        }
    }

    public override Task<IGenericResult<TOut>> Execute<TOut>(IEmailCommand command)
    {
        return Task.FromResult(GenericResult<TOut>.Failure("This service does not return values"));
    }
}
```

### Step 4: Create the Factory

```csharp
// MyCompany.Services/Factories/EmailServiceFactory.cs
using FractalDataWorks.Services;
using FractalDataWorks.Results;
using Microsoft.Extensions.Logging;
using MyCompany.Services.Services;
using MyCompany.Services.Abstractions.Configuration;

namespace MyCompany.Services.Factories;

public interface IEmailServiceFactory : IServiceFactory<EmailService, EmailConfiguration>
{
}

public class EmailServiceFactory : ServiceFactoryBase<EmailService, EmailConfiguration>, IEmailServiceFactory
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
        try
        {
            // Validate configuration
            var validationResult = configuration.Validate();
            if (validationResult.Error || !validationResult.Value.IsValid)
            {
                return GenericResult<EmailService>.Failure("Configuration validation failed");
            }

            // Create service instance
            var service = new EmailService(_serviceLogger, configuration);

            Logger.LogInformation("Email service created successfully");
            return GenericResult<EmailService>.Success(service);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create email service");
            return GenericResult<EmailService>.Failure($"Service creation failed: {ex.Message}");
        }
    }
}
```

### Step 5: Define the ServiceType

```csharp
// MyCompany.Services/ServiceTypes/EmailServiceType.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using MyCompany.Services.Services;
using MyCompany.Services.Factories;
using MyCompany.Services.Abstractions.Configuration;

namespace MyCompany.Services.ServiceTypes;

public sealed class EmailServiceType : ServiceTypeBase<EmailService, EmailConfiguration, IEmailServiceFactory>
{
    public static EmailServiceType Instance { get; } = new();

    private EmailServiceType() : base(1, "Email", "Communication") { }

    public override string SectionName => "Email";
    public override string DisplayName => "Email Service";
    public override string Description => "Provides email sending capabilities via SMTP";

    public override void Register(IServiceCollection services)
    {
        // Register factory
        services.AddScoped<IEmailServiceFactory, EmailServiceFactory>();

        // Register service
        services.AddScoped<EmailService>();

        // Register any supporting services
        services.AddSingleton<IEmailValidator, EmailValidator>();
    }

    public override void Configure(IConfiguration configuration)
    {
        var section = configuration.GetSection($"Services:{SectionName}");
        if (section.Exists())
        {
            var config = section.Get<EmailConfiguration>();
            var validationResult = config?.Validate();

            if (validationResult?.Error == true)
            {
                throw new InvalidOperationException(
                    $"Email service configuration is invalid: {validationResult.Message}");
            }
        }
    }
}
```

### Step 6: Create the ServiceType Collection

```csharp
// MyCompany.Services/ServiceTypes/ServiceTypes.cs
using FractalDataWorks.ServiceTypes.Attributes;

namespace MyCompany.Services.ServiceTypes;

[ServiceTypeCollection("IServiceType", "ServiceTypes")]
public static partial class ServiceTypes
{
    // The source generator will populate this with all discovered ServiceTypes
    // It will include methods like:
    // - public static IReadOnlyList<IServiceType> All { get; }
    // - public static IServiceType GetByName(string name)
    // - public static void RegisterAll(IServiceCollection services)
}
```

## Working with Connections

### Creating a Custom Connection Type

```csharp
// MyCompany.Services/Connections/ApiConnectionType.cs
using FractalDataWorks.Services.Connections.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace MyCompany.Services.Connections;

public sealed class ApiConnectionType : ConnectionTypeBase<IGenericConnection, ApiConfiguration, IApiConnectionFactory>
{
    public static ApiConnectionType Instance { get; } = new();

    private ApiConnectionType() : base(10, "RestApi", "External APIs") { }

    public override Type FactoryType => typeof(IApiConnectionFactory);

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IApiConnectionFactory, ApiConnectionFactory>();
        services.AddScoped<ApiConnection>();
        services.AddHttpClient();
    }

    public override void Configure(IConfiguration configuration)
    {
        // Validate API configurations
    }
}
```

### Implementing the Connection

```csharp
// MyCompany.Services/Connections/ApiConnection.cs
using FractalDataWorks.Services.Connections.Abstractions;

namespace MyCompany.Services.Connections;

public class ApiConnection : ConnectionServiceBase<IApiCommand, ApiConfiguration, ApiConnection>
{
    private readonly HttpClient _httpClient;

    public ApiConnection(ApiConfiguration configuration, IHttpClientFactory httpClientFactory)
        : base(configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(configuration.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(configuration.TimeoutSeconds);
    }

    protected override async Task<IGenericResult> OpenCoreAsync()
    {
        try
        {
            // Test connection with a health check endpoint
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode
                ? GenericResult.Success()
                : GenericResult.Failure($"API returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return GenericResult.Failure($"Connection failed: {ex.Message}");
        }
    }

    public override async Task<IGenericResult<T>> Execute<T>(IApiCommand command)
    {
        try
        {
            var response = await ExecuteHttpRequest(command);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<T>(content);
                return GenericResult<T>.Success(result);
            }

            return GenericResult<T>.Failure($"API error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return GenericResult<T>.Failure($"Request failed: {ex.Message}");
        }
    }
}
```

## Using Collections and ServiceTypes

### Define a Collection of Options

```csharp
// MyCompany.Services/Options/LogLevelOption.cs
using FractalDataWorks.Collections;

namespace MyCompany.Services.Options;

public abstract class LogLevelOption : TypeOptionBase<LogLevelOption>
{
    public abstract LogLevel Level { get; }
    public abstract ConsoleColor Color { get; }

    protected LogLevelOption(int id, string name) : base(id, name, "Logging") { }
}

public sealed class DebugLogLevel : LogLevelOption
{
    public static DebugLogLevel Instance { get; } = new();
    private DebugLogLevel() : base(1, "Debug") { }
    public override LogLevel Level => LogLevel.Debug;
    public override ConsoleColor Color => ConsoleColor.Gray;
}

public sealed class InfoLogLevel : LogLevelOption
{
    public static InfoLogLevel Instance { get; } = new();
    private InfoLogLevel() : base(2, "Information") { }
    public override LogLevel Level => LogLevel.Information;
    public override ConsoleColor Color => ConsoleColor.White;
}

public sealed class WarningLogLevel : LogLevelOption
{
    public static WarningLogLevel Instance { get; } = new();
    private WarningLogLevel() : base(3, "Warning") { }
    public override LogLevel Level => LogLevel.Warning;
    public override ConsoleColor Color => ConsoleColor.Yellow;
}

public sealed class ErrorLogLevel : LogLevelOption
{
    public static ErrorLogLevel Instance { get; } = new();
    private ErrorLogLevel() : base(4, "Error") { }
    public override LogLevel Level => LogLevel.Error;
    public override ConsoleColor Color => ConsoleColor.Red;
}
```

### Create the Collection

```csharp
// MyCompany.Services/Options/LogLevels.cs
using FractalDataWorks.Collections.Attributes;

namespace MyCompany.Services.Options;

[TypeCollection("LogLevelOption", "LogLevels")]
public static partial class LogLevels
{
    // Generated methods will include:
    // - public static IReadOnlyList<LogLevelOption> All { get; }
    // - public static LogLevelOption GetByName(string name)
    // - public static LogLevelOption GetById(int id)

    // Custom helper methods
    public static LogLevelOption FromLogLevel(LogLevel level)
    {
        return All.FirstOrDefault(x => x.Level == level) ?? InfoLogLevel.Instance;
    }

    public static void ConfigureLogging(ILoggingBuilder builder, string levelName)
    {
        var logLevel = GetByName(levelName);
        if (logLevel != null)
        {
            builder.SetMinimumLevel(logLevel.Level);
        }
    }
}
```

## Complete Example

### Startup Configuration

```csharp
// Program.cs or Startup.cs
using MyCompany.Services.ServiceTypes;
using FractalDataWorks.Services.Connections;

var builder = WebApplication.CreateBuilder(args);

// Register all service types
ServiceTypes.RegisterAll(builder.Services);

// Register all connection types
ConnectionTypes.Register(builder.Services);

// Register the connection provider
builder.Services.AddSingleton<IGenericConnectionProvider, GenericConnectionProvider>();

// Configure services from appsettings
var configuration = builder.Configuration;
foreach (var serviceType in ServiceTypes.All)
{
    serviceType.Configure(configuration);
}

var app = builder.Build();

// Use services
app.MapPost("/api/email/send", async (
    SendEmailCommand command,
    IEmailServiceFactory factory,
    IConfiguration config) =>
{
    var emailConfig = config.GetSection("Services:Email").Get<EmailConfiguration>();
    var serviceResult = factory.Create(emailConfig);

    if (serviceResult.Error)
        return Results.BadRequest(serviceResult.Message);

    var result = await serviceResult.Value.Execute(command);

    return result.IsSuccess
        ? Results.Ok(result.Message)
        : Results.BadRequest(result.Message);
});

app.Run();
```

### Configuration File

```json
{
  "Services": {
    "Email": {
      "Name": "PrimaryEmailService",
      "SmtpServer": "smtp.gmail.com",
      "Port": 587,
      "EnableSsl": true,
      "Username": "your-email@gmail.com",
      "Password": "your-app-password",
      "DefaultFrom": "noreply@company.com",
      "TimeoutSeconds": 30
    }
  },
  "Connections": {
    "MainDatabase": {
      "ConnectionType": "MsSql",
      "ConnectionString": "Server=localhost;Database=MyApp;Integrated Security=true;",
      "CommandTimeout": 30
    },
    "ApiGateway": {
      "ConnectionType": "RestApi",
      "BaseUrl": "https://api.company.com",
      "TimeoutSeconds": 60,
      "ApiKey": "your-api-key"
    }
  }
}
```

## Best Practices

### 1. Service Design

- **Single Responsibility**: Each service should have one clear purpose
- **Command Pattern**: Use specific command types for different operations
- **Configuration Validation**: Always validate configuration in factories
- **Result Pattern**: Return IGenericResult instead of throwing exceptions
- **Logging**: Use structured logging with appropriate log levels

### 2. Dependency Injection

- **Appropriate Lifetimes**: Use Scoped for services, Singleton for factories
- **Interface Segregation**: Define minimal interfaces for dependencies
- **Factory Pattern**: Use factories for complex service creation
- **Configuration Binding**: Leverage IOptions<T> for configuration

### 3. Error Handling

```csharp
public override async Task<IGenericResult> Execute(ICommand command)
{
    try
    {
        // Validate command
        if (command == null)
            return GenericResult.Failure("Command cannot be null");

        // Execute operation
        var result = await PerformOperation(command);

        // Check result
        if (result.Success)
        {
            Logger.LogInformation("Operation completed successfully");
            return GenericResult.Success("Operation completed");
        }

        Logger.LogWarning("Operation failed: {Reason}", result.Reason);
        return GenericResult.Failure(result.Reason);
    }
    catch (OperationCanceledException)
    {
        Logger.LogInformation("Operation was cancelled");
        return GenericResult.Failure("Operation cancelled");
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Unexpected error during operation");
        return GenericResult.Failure($"Unexpected error: {ex.Message}");
    }
}
```

### 4. Testing

```csharp
// MyCompany.Services.Tests/EmailServiceTests.cs
using Xunit;
using Shouldly;
using Microsoft.Extensions.Logging;
using NSubstitute;

public class EmailServiceTests
{
    [Fact]
    public async Task Execute_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var logger = Substitute.For<ILogger<EmailService>>();
        var configuration = new EmailConfiguration
        {
            SmtpServer = "test.smtp.com",
            Port = 587,
            EnableSsl = true
        };

        var service = new EmailService(logger, configuration);
        var command = new SendEmailCommand
        {
            To = "test@example.com",
            Subject = "Test",
            Body = "Test email"
        };

        // Act
        var result = await service.Execute(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Message.ShouldContain("successfully");
    }
}
```

## Troubleshooting

### Common Issues

#### 1. Service Type Not Discovered

**Problem**: ServiceTypes.All doesn't include your service type

**Solution**:
- Ensure the class inherits from `ServiceTypeBase`
- Check that it has a static Instance property
- Rebuild the project to trigger source generation
- Check for source generator errors in the Error List

#### 2. Configuration Validation Fails

**Problem**: Service creation fails due to configuration validation

**Solution**:
- Check the validation rules in your validator
- Ensure all required fields are present in appsettings.json
- Use the debugger to inspect the validation result
- Log the validation errors for debugging

#### 3. Connection State Issues

**Problem**: Connection shows as unavailable

**Solution**:
- Check that OpenAsync() was called successfully
- Verify the connection state transitions
- Look for exceptions in the logs
- Test the connection with TestConnectionAsync()

#### 4. Source Generator Not Working

**Problem**: Collections or ServiceTypes not generating

**Solution**:
- Ensure the source generator package is referenced correctly
- Check OutputItemType="Analyzer" is set
- Clean and rebuild the solution
- Check for compiler errors that might block generation
- Verify the target framework is compatible

### Debugging Tips

1. **Enable Detailed Logging**:
```csharp
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

2. **Use Diagnostic Source Generators**:
```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

3. **Inspect Generated Code**:
Look in `obj/Generated/` directory for source-generated files

4. **Test in Isolation**:
Create unit tests for individual services before integration

## Additional Resources

- [Services Architecture Documentation](Services.md)
- [Connections Pattern Documentation](Connections.md)
- [Collections and TypeOptions Guide](Collections.md)
- [ServiceTypes Pattern Guide](ServiceTypes.md)
- [API Reference Documentation](https://docs.FractalDataWorks.com/api/)

## Support

For questions or issues:
- GitHub Issues: https://github.com/FractalDataWorks/developerkit/issues
- Documentation: https://docs.FractalDataWorks.com/
- Email: support@FractalDataWorks.com
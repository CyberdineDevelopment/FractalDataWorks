# Getting Started with FractalDataWorks Framework

## Table of Contents
- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Quick Start Examples](#quick-start-examples)
- [Creating Your First Service](#creating-your-first-service)
- [Integration with Existing Projects](#integration-with-existing-projects)
- [Next Steps](#next-steps)

## Overview

The FractalDataWorks Framework provides a comprehensive foundation for building scalable .NET applications with:
- Type-safe Enhanced Enums with business logic
- Configuration-driven services with validation
- Consistent error handling and messaging
- High-performance data transformations

## Prerequisites

- **.NET 10.0** (with fallback to .NET Standard 2.0 for some packages)
- **Visual Studio 2022** or **VS Code** with C# extension
- **NuGet Package Manager**

## Installation

### Package Selection Guide

| Package | Purpose | When to Use |
|---------|---------|-------------|
| `FractalDataWorks.Messages` | Message framework core | Type-safe messaging system |
| `FractalDataWorks.Messages.SourceGenerators` | Message factory generation | Auto-generate message collections |
| `FractalDataWorks.EnhancedEnums` | Core Enhanced Enums | Foundation for type safety |
| `FractalDataWorks.EnhancedEnums.SourceGenerators` | Auto-generated collections | When using Enhanced Enums |
| `FractalDataWorks.Services` | Service framework | Building scalable services |
| `FractalDataWorks.Configuration` | Typed configuration | Service configuration management |
| `FractalDataWorks.Transformations` | Data processing | Data transformation pipelines |

### Basic Installation

For projects in this solution, add project references in your `.csproj`:

```xml
<ItemGroup>
  <!-- Messages framework with source generation -->
  <ProjectReference Include="..\FractalDataWorks.Messages\FractalDataWorks.Messages.csproj" />
  <ProjectReference Include="..\FractalDataWorks.Messages.SourceGenerators\FractalDataWorks.Messages.SourceGenerators.csproj" 
                    OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
  
  <!-- Services framework -->
  <ProjectReference Include="..\FractalDataWorks.Services\FractalDataWorks.Services.csproj" />
  
  <!-- Configuration framework -->
  <ProjectReference Include="..\FractalDataWorks.Configuration\FractalDataWorks.Configuration.csproj" />
</ItemGroup>
```

**Note:** The `OutputItemType="Analyzer"` is required for source generators to work correctly.

### Advanced Installation

```bash
# For data processing applications
dotnet add package FractalDataWorks.Transformations
dotnet add package FractalDataWorks.Transformations.Parallel

# For code generation scenarios
dotnet add package FractalDataWorks.CodeBuilder
dotnet add package FractalDataWorks.CodeBuilder.Analysis
```

## Quick Start Examples

### 1. Your First Enhanced Enum

```csharp
// Define the base enum
public abstract class Priority : EnumOptionBase<Priority>
{
    protected Priority(int id, string name) : base(id, name) { }
    public abstract int Level { get; }
    public abstract string Color { get; }
}

// Define enum options
[EnumOption("High")]
public sealed class HighPriority : Priority
{
    public HighPriority() : base(1, "High") { }
    public override int Level => 100;
    public override string Color => "red";
}

[EnumOption("Medium")]
public sealed class MediumPriority : Priority
{
    public MediumPriority() : base(2, "Medium") { }
    public override int Level => 50;
    public override string Color => "yellow";
}

// Configure collection generation
[EnumCollection("Priorities", EnumGenerationMode.Singletons, EnumStorageMode.Dictionary)]
public abstract class PriorityCollection : EnumCollectionBase<Priority> { }

// Usage - automatically generated
var highPriority = Priorities.High();
var allPriorities = Priorities.All();
var byLevel = allPriorities.OrderByDescending(p => p.Level);
```

### 2. Simple Service Configuration

```csharp
public class EmailServiceConfiguration : ConfigurationBase<EmailServiceConfiguration>
{
    public override string SectionName => "EmailService";
    
    public string SmtpHost { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    
    protected override IValidator<EmailServiceConfiguration> GetValidator()
    {
        return new EmailServiceConfigurationValidator();
    }
}

public class EmailServiceConfigurationValidator : AbstractValidator<EmailServiceConfiguration>
{
    public EmailServiceConfigurationValidator()
    {
        RuleFor(x => x.SmtpHost).NotEmpty().WithMessage("SMTP host is required");
        RuleFor(x => x.Port).InclusiveBetween(1, 65535).WithMessage("Port must be valid");
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
    }
}
```

### 3. Basic Service Implementation

```csharp
public interface IEmailService : IFractalService<EmailExecutor, EmailServiceConfiguration>
{
    Task<IFdwResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}

public class EmailService : ServiceBase<EmailExecutor, EmailServiceConfiguration>, IEmailService
{
    public EmailService(EmailExecutor executor, ILogger<EmailService> logger) 
        : base(executor, logger) { }
    
    public async Task<IFdwResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        using var activity = StartActivity();
        
        try
        {
            await Executor.SendEmailAsync(to, subject, body, cancellationToken);
            Logger.LogInformation("Email sent successfully to {Recipient}", to);
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to send email to {Recipient}", to);
            return FdwResult.Failure($"Email delivery failed: {ex.Message}");
        }
    }
}
```

## Creating Your First Service

Follow this step-by-step guide to create a complete service:

### Step 1: Define Enhanced Enum Messages

```csharp
public abstract class EmailMessageBase : EnumOptionBase<EmailMessageBase>, IMessage
{
    protected EmailMessageBase(int id, string name, MessageSeverity severity, string template, string code)
        : base(id, name)
    {
        Severity = severity;
        Template = template;
        Code = code;
    }
    
    public MessageSeverity Severity { get; }
    public string Template { get; }
    public string Code { get; }
}

[EnumOption]
public sealed class EmailSentSuccessfully : EmailMessageBase
{
    public EmailSentSuccessfully() : base(1, nameof(EmailSentSuccessfully), 
        MessageSeverity.Info, "Email sent successfully to {0}", "EMAIL_SENT") { }
}

[EnumOption]
public sealed class EmailDeliveryFailed : EmailMessageBase  
{
    public EmailDeliveryFailed() : base(2, nameof(EmailDeliveryFailed), 
        MessageSeverity.Error, "Email delivery failed: {0}", "EMAIL_FAILED") { }
}

[EnumCollection("EmailMessages", EnumGenerationMode.Singletons, EnumStorageMode.Dictionary)]
public abstract class EmailMessageCollection : EnumCollectionBase<EmailMessageBase> { }
```

### Step 2: Create Configuration and Validation

```csharp
// Configuration already shown above

// Add to appsettings.json
{
  "EmailService": {
    "SmtpHost": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

### Step 3: Implement Service Executor

```csharp
public class EmailExecutor
{
    private readonly EmailServiceConfiguration _config;
    private readonly ILogger<EmailExecutor> _logger;
    
    public EmailExecutor(IOptions<EmailServiceConfiguration> config, ILogger<EmailExecutor> logger)
    {
        _config = config.Value;
        _logger = logger;
    }
    
    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        using var client = new SmtpClient(_config.SmtpHost, _config.Port);
        client.EnableSsl = _config.UseSsl;
        client.Credentials = new NetworkCredential(_config.Username, _config.Password);
        
        var message = new MailMessage(_config.Username, to, subject, body);
        await client.SendMailAsync(message, cancellationToken);
    }
}
```

### Step 4: Register Services

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register configuration
        services.Configure<EmailServiceConfiguration>(configuration.GetSection("EmailService"));
        
        // Register executor and service
        services.AddScoped<EmailExecutor>();
        services.AddScoped<IEmailService, EmailService>();
        
        return services;
    }
}

// In Program.cs
builder.Services.AddEmailServices(builder.Configuration);
```

### Step 5: Use the Service

```csharp
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly IEmailService _emailService;
    
    public NotificationController(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
    {
        var result = await _emailService.SendEmailAsync(
            request.Email, 
            request.Subject, 
            request.Body);
        
        if (result.IsSuccess)
        {
            return Ok(new { Message = "Email sent successfully" });
        }
        
        return BadRequest(new { Error = result.Message });
    }
}
```

## Integration with Existing Projects

### Adding to Existing ASP.NET Core Application

1. **Install packages** using the package selection guide above
2. **Add configuration sections** to your appsettings.json
3. **Register services** in your Program.cs or Startup.cs
4. **Replace string constants** with Enhanced Enums gradually
5. **Implement service patterns** for new features

### Migration Strategy

#### Phase 1: Foundation (Week 1)
- Install FractalDataWorks.EnhancedEnums.SourceGenerators
- Replace critical string constants with Enhanced Enums
- Add configuration validation to key services

#### Phase 2: Services (Week 2-3)  
- Implement service base classes for new services
- Add Enhanced Enum messages for error handling
- Migrate existing services to use typed configuration

#### Phase 3: Data Processing (Week 4)
- Add transformation capabilities for data pipelines
- Implement parallel processing for performance-critical operations

### Integration Examples

#### With Entity Framework
```csharp
public class UserService : ServiceBase<UserRepository, UserServiceConfiguration>
{
    public async Task<IFdwResult<User>> CreateUserAsync(CreateUserRequest request)
    {
        // Enhanced Enum validation
        if (!UserRoles.IsValid(request.Role))
        {
            return FdwResult<User>.Failure(UserMessages.InvalidRole(request.Role));
        }
        
        // Use executor (repository) for data operations
        var user = await Executor.CreateAsync(request);
        return FdwResult<User>.Success(user);
    }
}
```

#### With MediatR
```csharp
public class CreateUserHandler : IRequestHandler<CreateUserCommand, IFdwResult<User>>
{
    private readonly IUserService _userService;
    
    public async Task<IFdwResult<User>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var config = new UserServiceConfiguration { /* populate from request */ };
        return await _userService.CreateUserAsync(config, cancellationToken);
    }
}
```

#### With SignalR
```csharp
public class NotificationHub : Hub
{
    private readonly INotificationService _notificationService;
    
    public async Task SendNotification(string message)
    {
        var result = await _notificationService.BroadcastAsync(message);
        
        if (result.IsSuccess)
        {
            await Clients.All.SendAsync("NotificationSent", NotificationMessages.BroadcastSuccessful());
        }
        else
        {
            await Clients.Caller.SendAsync("Error", result.Message);
        }
    }
}
```

## Next Steps

### Learning Path
1. **📖 Read [Architecture Documentation](Architecture.md)** - Understand the layered architecture
2. **🔧 Study [Enhanced Enums Guide](EnhancedEnums.md)** - Master type-safe enumerations  
3. **⚙️ Review [Services Framework](Services.md)** - Learn service patterns and best practices
4. **🔄 Explore [Transformations](Transformations.md)** - For data processing scenarios

### Advanced Topics
- **[Naming Conventions](NamingConventions.md)** - Maintain consistency across large projects
- **[CodeBuilder](CodeBuilder.md)** - Programmatic code generation and analysis
- **[Dependency Injection](DependencyInjection.md)** - Advanced DI patterns and service registration

### Sample Projects
- **Simple Enhanced Enums**: `samples/EnhancedEnums/SimpleExample/`
- **Service Patterns**: `samples/Services/ServicePatterns/`
- **Configuration Management**: `samples/Services/ConfigurationExample/`

### Community and Support
- **Issues**: Report bugs and feature requests via GitHub Issues
- **Discussions**: Join architectural discussions and share patterns
- **Contributions**: Follow the contributing guidelines for code submissions
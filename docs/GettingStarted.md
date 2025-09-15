# Getting Started with FractalDataWorks Developer Kit

## Table of Contents
- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Quick Start Examples](#quick-start-examples)
- [Creating Your First Service](#creating-your-first-service)
- [Integration with Existing Projects](#integration-with-existing-projects)
- [Next Steps](#next-steps)

## Overview

The FractalDataWorks Developer Kit provides a comprehensive foundation for building scalable .NET applications with:
- ServiceType auto-discovery with source generation
- Universal command patterns that work across all backends
- Type-safe Enhanced Enums with business logic
- Configuration-driven services with validation
- Railway-oriented programming for robust error handling
- High-performance data transformations

## Prerequisites

- **.NET 10.0** (with fallback to .NET Standard 2.0 for some packages)
- **Visual Studio 2022** or **VS Code** with C# extension
- **NuGet Package Manager**

## Installation

### Package Selection Guide

| Package | Purpose | When to Use |
|---------|---------|-------------|
| `FractalDataWorks.Services.Connections` | Connection framework | Universal data access across backends |
| `FractalDataWorks.Services.Connections.MsSql` | SQL Server provider | SQL Server database connections |
| `FractalDataWorks.Services.Authentication` | Authentication services | User authentication across providers |
| `FractalDataWorks.Services.SecretManagement` | Secret storage | Secure configuration and keys |
| `FractalDataWorks.Services.Transformations` | Data processing | Data transformation pipelines |
| `FractalDataWorks.ServiceTypes` | ServiceType framework | Auto-discovery and plugin architecture |
| `FractalDataWorks.ServiceTypes.SourceGenerators` | Source generation | Auto-generate service collections |
| `FractalDataWorks.EnhancedEnums` | Enhanced Enums | Type-safe enumerations |
| `FractalDataWorks.Messages` | Messaging framework | Type-safe messaging system |
| `FractalDataWorks.Configuration` | Configuration management | Validated service configuration |

### Basic Installation

For projects in this solution, add project references in your `.csproj`:

```xml
<ItemGroup>
  <!-- ServiceType framework with source generation -->
  <ProjectReference Include="..\FractalDataWorks.ServiceTypes\FractalDataWorks.ServiceTypes.csproj" />
  <ProjectReference Include="..\FractalDataWorks.ServiceTypes.SourceGenerators\FractalDataWorks.ServiceTypes.SourceGenerators.csproj"
                    OutputItemType="Analyzer" ReferenceOutputAssembly="false" />

  <!-- Connection services -->
  <ProjectReference Include="..\FractalDataWorks.Services.Connections\FractalDataWorks.Services.Connections.csproj" />
  <ProjectReference Include="..\FractalDataWorks.Services.Connections.MsSql\FractalDataWorks.Services.Connections.MsSql.csproj" />

  <!-- Configuration framework -->
  <ProjectReference Include="..\FractalDataWorks.Configuration\FractalDataWorks.Configuration.csproj" />
</ItemGroup>
```

**Note:** The `OutputItemType="Analyzer"` is required for source generators to work correctly.

### Advanced Installation

```bash
# For comprehensive service support
dotnet add package FractalDataWorks.Services.Authentication
dotnet add package FractalDataWorks.Services.SecretManagement
dotnet add package FractalDataWorks.Services.Transformations

# For data access
dotnet add package FractalDataWorks.DataStores
dotnet add package FractalDataWorks.Services.DataGateway
```

## Quick Start Examples

### 1. ServiceType Auto-Discovery

```csharp
// In Program.cs - Zero-configuration registration
var builder = WebApplication.CreateBuilder(args);

// Register connection provider
builder.Services.AddScoped<IFdwConnectionProvider, FdwConnectionProvider>();

// Single line registers ALL discovered connection types
ConnectionTypes.Register(builder.Services);

// Register other service domains
AuthenticationTypes.Register(builder.Services);
SecretManagementTypes.Register(builder.Services);

var app = builder.Build();

// Use connections via provider
app.MapGet("/users", async (IFdwConnectionProvider connectionProvider) =>
{
    var connection = await connectionProvider.GetConnection("Database");
    if (connection.IsSuccess)
    {
        using var conn = connection.Value;
        var command = new QueryCommand("SELECT * FROM Users");
        return await conn.Execute<List<User>>(command);
    }
    return Results.Problem(connection.Error);
});
```

### 2. Your First Enhanced Enum

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

### 3. Connection Configuration

```csharp
// appsettings.json
{
  "Connections": {
    "Database": {
      "ConnectionType": "MsSql",
      "ConnectionId": "MainDatabase",
      "ConnectionString": "Server=localhost;Database=MyApp;Integrated Security=true;",
      "CommandTimeout": 30,
      "MaxPoolSize": 100
    },
    "Analytics": {
      "ConnectionType": "MsSql",
      "ConnectionId": "AnalyticsDb",
      "ConnectionString": "Server=analytics;Database=Analytics;Integrated Security=true;"
    }
  }
}

// Usage - ConnectionProvider resolves by name
var connection = await connectionProvider.GetConnection("Database");
if (connection.IsSuccess)
{
    using var conn = connection.Value;
    // Universal command works with any backend
    var result = await conn.Execute<User>(universalCommand);
}
```

### 4. Creating a Custom ConnectionType

```csharp
// 1. Create your ConnectionType (singleton pattern)
public sealed class PostgreSqlConnectionType : ConnectionTypeBase<IFdwConnection, PostgreSqlConfiguration, IPostgreSqlConnectionFactory>
{
    public static PostgreSqlConnectionType Instance { get; } = new();

    private PostgreSqlConnectionType() : base(2, "PostgreSql", "Database Connections") { }

    public override Type FactoryType => typeof(IPostgreSqlConnectionFactory);

    public override void Register(IServiceCollection services)
    {
        // Register factory and dependencies
        services.AddScoped<IPostgreSqlConnectionFactory, PostgreSqlConnectionFactory>();
        services.AddScoped<PostgreSqlCommandTranslator>();
        services.AddScoped<PostgreSqlExpressionTranslator>();
    }
}

// 2. Add to your project - source generator automatically discovers it
// 3. ConnectionTypes.Register(services) will include it automatically
// 4. ConnectionProvider can now create PostgreSQL connections
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

### Step 4: Register Services with ServiceType Auto-Discovery

```csharp
// In Program.cs - ServiceType pattern handles registration
var builder = WebApplication.CreateBuilder(args);

// Register connection provider
builder.Services.AddScoped<IFdwConnectionProvider, FdwConnectionProvider>();

// Auto-discover and register all connection types
ConnectionTypes.Register(builder.Services);

// Register other service domains
AuthenticationTypes.Register(builder.Services);
SecretManagementTypes.Register(builder.Services);
TransformationTypes.Register(builder.Services);

var app = builder.Build();
```

### Step 5: Use Connections with Universal Commands

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IFdwConnectionProvider _connectionProvider;

    public UsersController(IFdwConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        // Get connection via provider (auto-resolved from configuration)
        var connectionResult = await _connectionProvider.GetConnection("Database");

        if (!connectionResult.IsSuccess)
            return Problem(connectionResult.Error);

        using var connection = connectionResult.Value;

        // Universal command - works with any backend
        var command = new QueryCommand("SELECT * FROM Users WHERE Active = @active")
            .WithParameter("active", true);

        var result = await connection.Execute<List<User>>(command);

        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error);
    }
}
```

## Integration with Existing Projects

### Adding to Existing ASP.NET Core Application

1. **Install ServiceType framework** using the package selection guide above
2. **Add ServiceType auto-discovery** to your Program.cs
3. **Configure connections** in your appsettings.json
4. **Replace direct database access** with ConnectionProvider gradually
5. **Implement universal commands** for new features

### Migration Strategy

#### Phase 1: Foundation (Week 1)
- Install FractalDataWorks.ServiceTypes and source generators
- Add ConnectionTypes.Register() to Program.cs
- Configure connection strings in appsettings.json

#### Phase 2: Data Access (Week 2-3)
- Replace direct database access with ConnectionProvider
- Implement universal commands for data operations
- Add connection-specific providers (MsSql, PostgreSql, etc.)

#### Phase 3: Extended Services (Week 4)
- Add Authentication, SecretManagement, and other service types
- Implement service-specific auto-discovery patterns
- Add custom ServiceTypes for domain-specific functionality

### Integration Examples

#### With Entity Framework
```csharp
public class UserService
{
    private readonly IFdwConnectionProvider _connectionProvider;

    public UserService(IFdwConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<IFdwResult<User>> CreateUserAsync(CreateUserRequest request)
    {
        var connectionResult = await _connectionProvider.GetConnection("Database");
        if (!connectionResult.IsSuccess)
            return FdwResult<User>.Failure(connectionResult.Error);

        using var connection = connectionResult.Value;

        // Universal command - translates to appropriate SQL
        var command = new CreateCommand("Users")
            .WithValue("Name", request.Name)
            .WithValue("Email", request.Email);

        var result = await connection.Execute<User>(command);
        return result;
    }
}
```

#### With MediatR
```csharp
public class CreateUserHandler : IRequestHandler<CreateUserCommand, IFdwResult<User>>
{
    private readonly IFdwConnectionProvider _connectionProvider;

    public CreateUserHandler(IFdwConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<IFdwResult<User>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var connectionResult = await _connectionProvider.GetConnection("Database");
        if (!connectionResult.IsSuccess)
            return FdwResult<User>.Failure(connectionResult.Error);

        using var connection = connectionResult.Value;
        var command = new CreateCommand("Users").WithValues(request.Values);
        return await connection.Execute<User>(command);
    }
}
```

#### With SignalR
```csharp
public class NotificationHub : Hub
{
    private readonly IFdwConnectionProvider _connectionProvider;

    public NotificationHub(IFdwConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task SendNotification(string message)
    {
        // Store notification using universal command
        var connectionResult = await _connectionProvider.GetConnection("Database");
        if (connectionResult.IsSuccess)
        {
            using var connection = connectionResult.Value;
            var command = new CreateCommand("Notifications")
                .WithValue("Message", message)
                .WithValue("Timestamp", DateTimeOffset.UtcNow);

            var result = await connection.Execute<Notification>(command);

            if (result.IsSuccess)
            {
                await Clients.All.SendAsync("NotificationSent", result.Value);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Error);
            }
        }
    }
}
```

## Next Steps

### Learning Path
1. **📖 Read [Services.Abstractions README](../src/FractalDataWorks.Services.Abstractions/README.md)** - Complete ServiceType architecture guide
2. **🔧 Study [Enhanced Enums Guide](EnhancedEnums.md)** - Master type-safe enumerations
3. **⚙️ Review [Services Framework](Services.md)** - Learn service patterns and best practices
4. **🔄 Explore [Transformations](Transformations.md)** - For data processing scenarios

### Advanced Topics
- **[ServiceType Auto-Discovery](ServiceTypes.md)** - Source generation and plugin architecture
- **[Universal Commands](UniversalCommands.md)** - Cross-platform data access patterns
- **[Connection Management](Connections.md)** - Dynamic service creation and lifecycle

### Sample Projects
- **ServiceType Auto-Discovery**: `samples/Services/Service.Implementation/`
- **Connection Patterns**: `samples/Web/FractalDataWorks.Web.Demo/`
- **Universal Commands**: `samples/Services/Service.Implementation/samples/ConnectionExample/`

### Community and Support
- **Issues**: Report bugs and feature requests via GitHub Issues
- **Discussions**: Join architectural discussions and share patterns
- **Contributions**: Follow the contributing guidelines for code submissions
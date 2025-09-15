# Dependency Injection Patterns

## Table of Contents
- [Overview](#overview)
- [Service Registration Patterns](#service-registration-patterns)
- [Configuration Integration](#configuration-integration)
- [Factory Patterns](#factory-patterns)
- [Enhanced Enum Integration](#enhanced-enum-integration)
- [Advanced Scenarios](#advanced-scenarios)
- [Best Practices](#best-practices)
- [Integration Examples](#integration-examples)

## Overview

The FractalDataWorks Framework uses dependency injection extensively to provide clean separation of concerns, testability, and flexible service composition. All services follow consistent registration patterns that integrate seamlessly with .NET's built-in DI container.

## Service Registration Patterns

### Core Service Registration

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyDomainServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // 1. Register configuration with validation
        services.Configure<MyDomainConfiguration>(
            configuration.GetSection("MyDomain"));
        services.AddSingleton<IValidator<MyDomainConfiguration>, MyDomainConfigurationValidator>();
        
        // 2. Register executor (business logic)
        services.AddScoped<MyDomainExecutor>();
        
        // 3. Register service
        services.AddScoped<IMyDomainService, MyDomainService>();
        
        // 4. Register factory (if needed)
        services.AddScoped<IMyDomainServiceFactory, MyDomainServiceFactory>();
        
        return services;
    }
}
```

### Layered Service Registration

For domain-specific services that build on core abstractions:

```csharp
public static class DataGatewayServiceCollectionExtensions
{
    public static IServiceCollection AddDataGatewayServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Core data provider services
        services.AddCoreDataGatewayServices(configuration);
        
        // SQL Server implementation
        services.AddSqlServerDataGateway(configuration);
        
        // MySQL implementation (optional)
        services.AddMySqlDataGateway(configuration);
        
        return services;
    }
    
    private static IServiceCollection AddCoreDataGatewayServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<DataGatewayConfiguration>(
            configuration.GetSection("DataGateway"));
        services.AddSingleton<IValidator<DataGatewayConfiguration>, DataGatewayConfigurationValidator>();
        services.AddScoped<IDataGatewayFactory, DataGatewayFactory>();
        
        return services;
    }
    
    private static IServiceCollection AddSqlServerDataGateway(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<SqlServerConfiguration>(
            configuration.GetSection("SqlServer"));
        services.AddScoped<SqlServerExecutor>();
        services.AddScoped<ISqlServerService, SqlServerService>();
        
        return services;
    }
}
```

## Configuration Integration

### Configuration Binding with Validation

```csharp
public class EmailServiceConfiguration : ConfigurationBase<EmailServiceConfiguration>
{
    public override string SectionName => "EmailService";
    
    public string SmtpHost { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    
    protected override IValidator<EmailServiceConfiguration> GetValidator()
    {
        return new EmailServiceConfigurationValidator();
    }
}

// Registration with automatic validation
services.Configure<EmailServiceConfiguration>(configuration.GetSection("EmailService"));
services.AddSingleton<IValidator<EmailServiceConfiguration>, EmailServiceConfigurationValidator>();

// Post-configure validation
services.PostConfigure<EmailServiceConfiguration>(config =>
{
    var validator = new EmailServiceConfigurationValidator();
    var result = validator.Validate(config);
    if (!result.IsValid)
    {
        throw new InvalidOperationException($"EmailService configuration is invalid: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}");
    }
});
```

### Environment-Specific Configuration

```csharp
public static IServiceCollection AddEnvironmentSpecificServices(
    this IServiceCollection services, 
    IConfiguration configuration, 
    IWebHostEnvironment environment)
{
    if (environment.IsDevelopment())
    {
        // Development-specific services
        services.AddScoped<IEmailService, MockEmailService>();
        services.Configure<EmailServiceConfiguration>(config =>
        {
            config.UseSsl = false; // Allow non-SSL in development
        });
    }
    else if (environment.IsProduction())
    {
        // Production-specific services
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.Configure<EmailServiceConfiguration>(config =>
        {
            config.UseSsl = true; // Enforce SSL in production
        });
    }
    
    return services;
}
```

## Factory Patterns

### Service Factory Registration

```csharp
public interface IMyServiceFactory : IServiceFactory<IMyService, MyServiceConfiguration>
{
    Task<IFdwResult<IMyService>> CreateServiceAsync(
        MyServiceConfiguration configuration,
        CancellationToken cancellationToken = default);
}

public class MyServiceFactory : ServiceFactoryBase<IMyService, MyServiceConfiguration>, IMyServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public MyServiceFactory(IServiceProvider serviceProvider, ILogger<MyServiceFactory> logger)
        : base(logger)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<IFdwResult<IMyService>> CreateServiceAsync(
        MyServiceConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate configuration
            var validator = _serviceProvider.GetRequiredService<IValidator<MyServiceConfiguration>>();
            var validationResult = await validator.ValidateAsync(configuration, cancellationToken);
            
            if (!validationResult.IsValid)
            {
                return FdwResult<IMyService>.Failure(
                    MyMessages.ConfigurationValidationFailed(string.Join(", ", validationResult.Errors)));
            }
            
            // Create service with dependencies
            var executor = _serviceProvider.GetRequiredService<MyExecutor>();
            var logger = _serviceProvider.GetRequiredService<ILogger<MyService>>();
            var service = new MyService(executor, logger);
            
            return FdwResult<IMyService>.Success(service);
        }
        catch (Exception ex)
        {
            return FdwResult<IMyService>.Failure(MyMessages.ServiceCreationFailed(ex.Message));
        }
    }
}

// Registration
services.AddScoped<IMyServiceFactory, MyServiceFactory>();
```

### Generic Factory Pattern

```csharp
public interface IGenericServiceFactory<TService, TConfiguration>
    where TService : class
    where TConfiguration : class
{
    Task<IFdwResult<TService>> CreateAsync(TConfiguration configuration, CancellationToken cancellationToken = default);
}

public class GenericServiceFactory<TService, TConfiguration> : IGenericServiceFactory<TService, TConfiguration>
    where TService : class
    where TConfiguration : class
{
    private readonly IServiceProvider _serviceProvider;
    
    public GenericServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<IFdwResult<TService>> CreateAsync(TConfiguration configuration, CancellationToken cancellationToken = default)
    {
        // Generic service creation logic
        var service = _serviceProvider.GetRequiredService<TService>();
        return FdwResult<TService>.Success(service);
    }
}

// Registration
services.AddScoped(typeof(IGenericServiceFactory<,>), typeof(GenericServiceFactory<,>));
```

## Enhanced Enum Integration

### Enhanced Enum Service Registration

```csharp
public static IServiceCollection AddEnhancedEnumServices(this IServiceCollection services)
{
    // Register Enhanced Enum collections as singletons
    services.AddSingleton<ServiceTypeCollectionBase>();
    services.AddSingleton<MessageCollectionBase>();
    services.AddSingleton<ValidationSeverityCollectionBase>();
    
    // Register Enhanced Enum resolvers
    services.AddScoped<IEnumResolver<ServiceTypeBase>, ServiceTypeResolver>();
    services.AddScoped<IMessageResolver, MessageResolver>();
    
    return services;
}
```

### Message System Integration

```csharp
public interface IMessageService
{
    IFdwResult<string> FormatMessage<TMessage>(TMessage message, params object[] args) 
        where TMessage : IMessage;
    IFdwResult<string> GetLocalizedMessage<TMessage>(TMessage message, CultureInfo culture, params object[] args)
        where TMessage : IMessage;
}

public class MessageService : IMessageService
{
    private readonly IServiceProvider _serviceProvider;
    
    public MessageService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IFdwResult<string> FormatMessage<TMessage>(TMessage message, params object[] args) 
        where TMessage : IMessage
    {
        try
        {
            var formatted = string.Format(message.Template, args);
            return FdwResult<string>.Success(formatted);
        }
        catch (Exception ex)
        {
            return FdwResult<string>.Failure($"Failed to format message: {ex.Message}");
        }
    }
}

// Registration
services.AddScoped<IMessageService, MessageService>();
```

## Advanced Scenarios

### Service Discovery and Registration

```csharp
public static IServiceCollection AddServicesByConvention(
    this IServiceCollection services, 
    Assembly assembly)
{
    var serviceTypes = assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract)
        .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IFractalService<,>)))
        .ToList();
    
    foreach (var serviceType in serviceTypes)
    {
        var interfaceType = serviceType.GetInterfaces()
            .FirstOrDefault(i => i.Name == $"I{serviceType.Name}");
        
        if (interfaceType != null)
        {
            services.AddScoped(interfaceType, serviceType);
        }
    }
    
    return services;
}
```

### Conditional Service Registration

```csharp
public static IServiceCollection AddConditionalServices(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    var features = configuration.GetSection("Features").Get<FeatureConfiguration>();
    
    if (features.EnableEmailService)
    {
        services.AddEmailServices(configuration);
    }
    
    if (features.EnableSmsService)
    {
        services.AddSmsServices(configuration);
    }
    
    if (features.EnableTransformations)
    {
        services.AddTransformationServices(configuration);
    }
    
    return services;
}
```

### Decorator Pattern Registration

```csharp
public class CachingEmailService : IEmailService
{
    private readonly IEmailService _inner;
    private readonly IMemoryCache _cache;
    
    public CachingEmailService(IEmailService inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }
    
    public async Task<IFdwResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"email:{to}:{subject.GetHashCode()}";
        
        if (_cache.TryGetValue(cacheKey, out var cachedResult))
        {
            return (IFdwResult)cachedResult;
        }
        
        var result = await _inner.SendEmailAsync(to, subject, body, cancellationToken);
        
        if (result.IsSuccess)
        {
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        }
        
        return result;
    }
}

// Registration with decorator
services.AddScoped<SmtpEmailService>();
services.AddScoped<IEmailService>(provider => 
    new CachingEmailService(
        provider.GetRequiredService<SmtpEmailService>(),
        provider.GetRequiredService<IMemoryCache>()));
```

## Best Practices

### 1. Service Lifetime Management

```csharp
// Singleton: Stateless services, configuration, caching
services.AddSingleton<IMemoryCache, MemoryCache>();
services.AddSingleton<IMessageService, MessageService>();

// Scoped: Per-request services, database contexts, user-specific state
services.AddScoped<IEmailService, EmailService>();
services.AddScoped<IUserService, UserService>();

// Transient: Lightweight services, short-lived operations
services.AddTransient<IValidator<EmailConfiguration>, EmailConfigurationValidator>();
services.AddTransient<EmailExecutor>();
```

### 2. Configuration Validation

```csharp
// Always validate critical configurations at startup
services.PostConfigure<DatabaseConfiguration>(config =>
{
    if (string.IsNullOrEmpty(config.ConnectionString))
    {
        throw new InvalidOperationException("Database connection string is required");
    }
});

// Use options validation for runtime checking
services.AddOptions<EmailServiceConfiguration>()
    .Bind(configuration.GetSection("EmailService"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### 3. Service Registration Organization

```csharp
// Group related services in extension methods
public static class ServiceRegistration
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddEnhancedEnumServices()
            .AddConfigurationServices(configuration)
            .AddMessagingServices(configuration)
            .AddLoggingServices(configuration);
    }
    
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddEmailServices(configuration)
            .AddNotificationServices(configuration)
            .AddTransformationServices(configuration);
    }
}
```

### 4. Health Check Integration

```csharp
public static IServiceCollection AddServiceHealthChecks(this IServiceCollection services, IConfiguration configuration)
{
    services.AddHealthChecks()
        .AddCheck<EmailServiceHealthCheck>("email-service")
        .AddCheck<DatabaseHealthCheck>("database")
        .AddCheck<TransformationServiceHealthCheck>("transformations");
    
    return services;
}

public class EmailServiceHealthCheck : IHealthCheck
{
    private readonly IEmailService _emailService;
    
    public EmailServiceHealthCheck(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Perform health check logic
            var config = new EmailServiceConfiguration(); // Test configuration
            var result = await _emailService.ValidateConfigurationAsync(config, cancellationToken);
            
            return result.IsSuccess 
                ? HealthCheckResult.Healthy("Email service is healthy")
                : HealthCheckResult.Unhealthy("Email service configuration is invalid");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Email service health check failed", ex);
        }
    }
}
```

## Integration Examples

### ASP.NET Core Integration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add FractalDataWorks services
builder.Services
    .AddCoreServices(builder.Configuration)
    .AddDomainServices(builder.Configuration)
    .AddServiceHealthChecks(builder.Configuration);

// Add framework services
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure pipeline
app.UseHealthChecks("/health");
app.MapControllers();

app.Run();
```

### Console Application Integration

```csharp
// Program.cs
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddLogging(builder => builder.AddConsole());
services.AddCoreServices(configuration);
services.AddDomainServices(configuration);

var serviceProvider = services.BuildServiceProvider();

// Use services
var emailService = serviceProvider.GetRequiredService<IEmailService>();
var result = await emailService.SendEmailAsync("user@example.com", "Test", "Hello World");

if (result.IsSuccess)
{
    Console.WriteLine("Email sent successfully!");
}
else
{
    Console.WriteLine($"Email failed: {result.Message}");
}
```

### Worker Service Integration

```csharp
public class EmailWorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailWorkerService> _logger;
    
    public EmailWorkerService(IServiceProvider serviceProvider, ILogger<EmailWorkerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            
            // Process email queue
            var result = await ProcessEmailQueue(emailService, stoppingToken);
            
            if (!result.IsSuccess)
            {
                _logger.LogError("Email processing failed: {Error}", result.Message);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}

// Registration in Program.cs
builder.Services.AddHostedService<EmailWorkerService>();
```

### Testing Integration

```csharp
public class EmailServiceTests
{
    private readonly ServiceProvider _serviceProvider;
    
    public EmailServiceTests()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["EmailService:SmtpHost"] = "localhost",
                ["EmailService:Port"] = "587",
                ["EmailService:UseSsl"] = "false"
            })
            .Build();
        
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddEmailServices(configuration);
        
        _serviceProvider = services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task SendEmailAsync_WithValidConfiguration_ShouldSucceed()
    {
        // Arrange
        var emailService = _serviceProvider.GetRequiredService<IEmailService>();
        
        // Act
        var result = await emailService.SendEmailAsync("test@example.com", "Test", "Hello");
        
        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
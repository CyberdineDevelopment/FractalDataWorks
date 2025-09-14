using FractalDataWorks.Configuration;
using FractalDataWorks.Services;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Results;
using FractalDataWorks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ServicePatterns;

/// <summary>
/// Demonstrates FractalDataWorks Service patterns with configuration,
/// commands, Enhanced Enums, and comprehensive service architecture.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("FractalDataWorks Service Patterns Examples");
        Console.WriteLine("==========================================\n");

        await RunBasicServiceExample();
        await RunConfiguredServiceExample();
        await RunCommandServiceExample();
        await RunHostedServiceExample();
    }

    /// <summary>
    /// Demonstrates basic service usage with configuration
    /// </summary>
    static async Task RunBasicServiceExample()
    {
        Console.WriteLine("⚙️ Basic Service Example");
        Console.WriteLine("========================");

        var config = new EmailConfiguration
        {
            Name = "Email Service",
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            Username = "noreply@example.com",
            EnableSsl = true,
            TimeoutSeconds = 30
        };

        using var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        
        var logger = loggerFactory.CreateLogger<EmailService>();
        var emailService = new EmailService(logger, config);

        var command = new SendEmailCommand
        {
            To = "user@example.com",
            Subject = "Test Email",
            Body = "This is a test email from the service pattern example.",
            Priority = EmailPriority.Normal
        };

        var result = await emailService.Execute(command);
        
        Console.WriteLine($"Email service result: {(result.IsSuccess ? "Success" : "Failed")}");
        if (!result.IsSuccess)
        {
            Console.WriteLine($"Error: {result.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates service with comprehensive configuration validation
    /// </summary>
    static async Task RunConfiguredServiceExample()
    {
        Console.WriteLine("📋 Configured Service Example");
        Console.WriteLine("=============================");

        // Test invalid configuration
        var invalidConfig = new DatabaseServiceConfiguration
        {
            Name = "", // Invalid
            ConnectionString = "invalid", // Too short
            MaxRetryAttempts = -1, // Invalid
            TimeoutSeconds = 0 // Invalid
        };

        var validation = invalidConfig.Validate();
        Console.WriteLine($"Configuration validation: {(validation.IsValid ? "Valid" : "Invalid")}");
        
        if (!validation.IsValid)
        {
            Console.WriteLine("Validation errors:");
            foreach (var error in validation.Errors)
            {
                Console.WriteLine($"  • {error.PropertyName}: {error.ErrorMessage}");
            }
        }

        // Test valid configuration
        var validConfig = new DatabaseServiceConfiguration
        {
            Name = "UserDatabase",
            ConnectionString = "Server=localhost;Database=Users;Integrated Security=true;",
            MaxRetryAttempts = 3,
            TimeoutSeconds = 30,
            EnableConnectionPooling = true
        };

        var validValidation = validConfig.Validate();
        Console.WriteLine($"\nValid configuration: {validValidation.IsValid}");
        
        if (validValidation.IsValid)
        {
            Console.WriteLine($"Database: {validConfig.Name}");
            Console.WriteLine($"Timeout: {validConfig.TimeoutSeconds}s");
            Console.WriteLine($"Retry attempts: {validConfig.MaxRetryAttempts}");
            Console.WriteLine($"Connection pooling: {validConfig.EnableConnectionPooling}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates command-based service execution
    /// </summary>
    static async Task RunCommandServiceExample()
    {
        Console.WriteLine("🎯 Command Service Example");
        Console.WriteLine("==========================");

        var config = new UserServiceConfiguration
        {
            Name = "User Management Service",
            CacheExpirationMinutes = 30,
            MaxConcurrentOperations = 10,
            EnableAuditLogging = true
        };

        using var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        
        var logger = loggerFactory.CreateLogger<UserService>();
        var userService = new UserService(logger, config);

        // Execute different types of commands
        var createCommand = new CreateUserCommand
        {
            Username = "john.doe",
            Email = "john.doe@example.com",
            Role = UserRole.Standard,
            IsActive = true
        };

        var createResult = await userService.ExecuteCreateUser(createCommand);
        Console.WriteLine($"Create user result: {(createResult.IsSuccess ? "Success" : "Failed")}");

        var updateCommand = new UpdateUserCommand
        {
            UserId = 1,
            Role = UserRole.Administrator,
            IsActive = true
        };

        var updateResult = await userService.ExecuteUpdateUser(updateCommand);
        Console.WriteLine($"Update user result: {(updateResult.IsSuccess ? "Success" : "Failed")}");

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates services in a hosted application with dependency injection
    /// </summary>
    static async Task RunHostedServiceExample()
    {
        Console.WriteLine("🏗️ Hosted Service Example");
        Console.WriteLine("==========================");

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register configuration instances
                var emailConfig = new EmailConfiguration
                {
                    Name = "Production Email Service",
                    SmtpHost = "prod-smtp.example.com",
                    SmtpPort = 587,
                    EnableSsl = true,
                    TimeoutSeconds = 45
                };
                services.AddSingleton(emailConfig);

                var userConfig = new UserServiceConfiguration
                {
                    Name = "Production User Service",
                    CacheExpirationMinutes = 60,
                    MaxConcurrentOperations = 20,
                    EnableAuditLogging = true
                };
                services.AddSingleton(userConfig);

                // Register services
                services.AddSingleton<EmailService>();
                services.AddSingleton<UserService>();
                services.AddSingleton<ServiceOrchestrator>();
            })
            .Build();

        var orchestrator = host.Services.GetRequiredService<ServiceOrchestrator>();
        await orchestrator.DemonstrateServiceIntegration();

        Console.WriteLine();
    }
}

/// <summary>
/// Email service configuration
/// </summary>
public class EmailConfiguration : ConfigurationBase<EmailConfiguration>
{
    public override string SectionName => "Email";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;

    protected override IValidator<EmailConfiguration>? GetValidator()
    {
        return new EmailConfigurationValidator();
    }
}

/// <summary>
/// Database service configuration
/// </summary>
public class DatabaseServiceConfiguration : ConfigurationBase<DatabaseServiceConfiguration>
{
    public override string SectionName => "DatabaseService";

    public string ConnectionString { get; set; } = string.Empty;
    public int MaxRetryAttempts { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
    public bool EnableConnectionPooling { get; set; } = true;
    public int MaxPoolSize { get; set; } = 100;

    protected override IValidator<DatabaseServiceConfiguration>? GetValidator()
    {
        return new DatabaseServiceConfigurationValidator();
    }
}

/// <summary>
/// User service configuration
/// </summary>
public class UserServiceConfiguration : ConfigurationBase<UserServiceConfiguration>
{
    public override string SectionName => "UserService";

    public int CacheExpirationMinutes { get; set; } = 30;
    public int MaxConcurrentOperations { get; set; } = 10;
    public bool EnableAuditLogging { get; set; } = true;
    public UserRole DefaultRole { get; set; } = UserRole.Standard;

    protected override IValidator<UserServiceConfiguration>? GetValidator()
    {
        return new UserServiceConfigurationValidator();
    }
}

/// <summary>
/// Email priority Enhanced Enum
/// </summary>
public sealed class EmailPriority : EnumOptionBase<EmailPriority>
{
    public static readonly EmailPriority Low = new(1, "Low", TimeSpan.FromHours(24), false);
    public static readonly EmailPriority Normal = new(2, "Normal", TimeSpan.FromHours(1), false);
    public static readonly EmailPriority High = new(3, "High", TimeSpan.FromMinutes(15), true);
    public static readonly EmailPriority Critical = new(4, "Critical", TimeSpan.FromMinutes(5), true);

    public TimeSpan MaxDelay { get; }
    public bool RequiresImmediate { get; }

    private EmailPriority(int id, string name, TimeSpan maxDelay, bool requiresImmediate)
        : base(id, name)
    {
        MaxDelay = maxDelay;
        RequiresImmediate = requiresImmediate;
    }
}

/// <summary>
/// User role Enhanced Enum
/// </summary>
public sealed class UserRole : EnumOptionBase<UserRole>
{
    public static readonly UserRole Guest = new(1, "Guest", 0, false, false);
    public static readonly UserRole Standard = new(2, "Standard", 1, true, false);
    public static readonly UserRole Premium = new(3, "Premium", 2, true, true);
    public static readonly UserRole Administrator = new(4, "Administrator", 3, true, true);

    public int AccessLevel { get; }
    public bool CanCreateUsers { get; }
    public bool HasAdvancedFeatures { get; }

    private UserRole(int id, string name, int accessLevel, bool canCreateUsers, bool hasAdvancedFeatures)
        : base(id, name)
    {
        AccessLevel = accessLevel;
        CanCreateUsers = canCreateUsers;
        HasAdvancedFeatures = hasAdvancedFeatures;
    }
}

/// <summary>
/// Send email command
/// </summary>
public class SendEmailCommand : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public Guid CorrelationId { get; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
    public IFractalConfiguration? Configuration { get; set; }

    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;
    public DateTime RequestedSendTime { get; set; } = DateTime.UtcNow;

    public FractalValidationResult Validate()
    {
        var validator = new SendEmailCommandValidator();
        return validator.Validate(this);
    }
}

/// <summary>
/// Create user command
/// </summary>
public class CreateUserCommand : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public Guid CorrelationId { get; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
    public IFractalConfiguration? Configuration { get; set; }

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Standard;
    public bool IsActive { get; set; } = true;

    public FractalValidationResult Validate()
    {
        var validator = new CreateUserCommandValidator();
        return validator.Validate(this);
    }
}

/// <summary>
/// Update user command
/// </summary>
public class UpdateUserCommand : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public Guid CorrelationId { get; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
    public IFractalConfiguration? Configuration { get; set; }

    public int UserId { get; set; }
    public UserRole Role { get; set; } = UserRole.Standard;
    public bool IsActive { get; set; } = true;

    public FractalValidationResult Validate()
    {
        var validator = new UpdateUserCommandValidator();
        return validator.Validate(this);
    }
}

/// <summary>
/// Email service implementation
/// </summary>
public class EmailService : IFractalService<SendEmailCommand>
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailConfiguration _configuration;

    public string Id { get; } = Guid.NewGuid().ToString();
    public string ServiceType => nameof(EmailService);
    public bool IsAvailable => _configuration?.IsEnabled ?? false;

    public EmailService(ILogger<EmailService> logger, EmailConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IFractalResult> Execute(SendEmailCommand command)
    {
        _logger.LogInformation("Sending email to {To} with priority {Priority}", 
            command.To, command.Priority.Name);

        try
        {
            // Validate command
            var validation = command.Validate();
            if (!validation.IsValid)
            {
                var errors = string.Join(", ", validation.Errors.Select(e => e.ErrorMessage));
                return FractalResult.Failure($"Command validation failed: {errors}");
            }

            // Simulate email sending based on priority
            var delay = command.Priority.RequiresImmediate ? 100 : 500;
            await Task.Delay(delay);

            _logger.LogInformation("Email sent successfully to {To}", command.To);
            return FractalResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", command.To);
            return FractalResult.Failure($"Email send failure: {ex.Message}");
        }
    }
}

/// <summary>
/// User service implementation for multiple command types
/// </summary>
public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly UserServiceConfiguration _configuration;

    public string Id { get; } = Guid.NewGuid().ToString();
    public string ServiceType => nameof(UserService);
    public bool IsAvailable => _configuration?.IsEnabled ?? false;

    public UserService(ILogger<UserService> logger, UserServiceConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IFractalResult> ExecuteCreateUser(CreateUserCommand command)
    {
        _logger.LogInformation("Creating user {Username} with role {Role}", 
            command.Username, command.Role.Name);

        try
        {
            // Validate command
            var validation = command.Validate();
            if (!validation.IsValid)
            {
                var errors = string.Join(", ", validation.Errors.Select(e => e.ErrorMessage));
                return FractalResult.Failure($"Command validation failed: {errors}");
            }

            // Simulate user creation
            await Task.Delay(200);

            if (_configuration.EnableAuditLogging)
            {
                _logger.LogInformation("Audit: User {Username} created with role {Role}", 
                    command.Username, command.Role.Name);
            }

            return FractalResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user {Username}", command.Username);
            return FractalResult.Failure($"User creation failure: {ex.Message}");
        }
    }

    public async Task<IFractalResult> ExecuteUpdateUser(UpdateUserCommand command)
    {
        _logger.LogInformation("Updating user {UserId} to role {Role}", 
            command.UserId, command.Role.Name);

        try
        {
            // Validate command
            var validation = command.Validate();
            if (!validation.IsValid)
            {
                var errors = string.Join(", ", validation.Errors.Select(e => e.ErrorMessage));
                return FractalResult.Failure($"Command validation failed: {errors}");
            }

            // Simulate user update
            await Task.Delay(150);

            if (_configuration.EnableAuditLogging)
            {
                _logger.LogInformation("Audit: User {UserId} updated to role {Role}", 
                    command.UserId, command.Role.Name);
            }

            return FractalResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user {UserId}", command.UserId);
            return FractalResult.Failure($"User update failure: {ex.Message}");
        }
    }
}

/// <summary>
/// Service orchestrator for demonstrating service integration
/// </summary>
public class ServiceOrchestrator
{
    private readonly EmailService _emailService;
    private readonly UserService _userService;
    private readonly ILogger<ServiceOrchestrator> _logger;

    public ServiceOrchestrator(
        EmailService emailService, 
        UserService userService, 
        ILogger<ServiceOrchestrator> logger)
    {
        _emailService = emailService;
        _userService = userService;
        _logger = logger;
    }

    public async Task DemonstrateServiceIntegration()
    {
        _logger.LogInformation("Demonstrating service integration");

        // Create a user
        var createCommand = new CreateUserCommand
        {
            Username = "jane.smith",
            Email = "jane.smith@example.com",
            Role = UserRole.Premium
        };

        var createResult = await _userService.ExecuteCreateUser(createCommand);
        Console.WriteLine($"User creation: {(createResult.IsSuccess ? "Success" : "Failed")}");

        // Send welcome email if user creation succeeded
        if (createResult.IsSuccess)
        {
            var emailCommand = new SendEmailCommand
            {
                To = createCommand.Email,
                Subject = "Welcome to Our Service",
                Body = $"Welcome {createCommand.Username}! Your account has been created with {createCommand.Role.Name} privileges.",
                Priority = EmailPriority.High
            };

            var emailResult = await _emailService.Execute(emailCommand);
            Console.WriteLine($"Welcome email: {(emailResult.IsSuccess ? "Sent" : "Failed")}");
        }

        Console.WriteLine("Service integration demonstration completed");
    }
}

// Validators
public class EmailConfigurationValidator : AbstractValidator<EmailConfiguration>
{
    public EmailConfigurationValidator()
    {
        RuleFor(x => x.SmtpHost).NotEmpty().WithMessage("SMTP host is required");
        RuleFor(x => x.SmtpPort).InclusiveBetween(1, 65535).WithMessage("SMTP port must be between 1 and 65535");
        RuleFor(x => x.TimeoutSeconds).GreaterThan(0).WithMessage("Timeout must be positive");
    }
}

public class DatabaseServiceConfigurationValidator : AbstractValidator<DatabaseServiceConfiguration>
{
    public DatabaseServiceConfigurationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.ConnectionString).NotEmpty().MinimumLength(10).WithMessage("Valid connection string is required");
        RuleFor(x => x.MaxRetryAttempts).GreaterThanOrEqualTo(0).WithMessage("MaxRetryAttempts cannot be negative");
        RuleFor(x => x.TimeoutSeconds).GreaterThan(0).WithMessage("TimeoutSeconds must be positive");
        RuleFor(x => x.MaxPoolSize).GreaterThan(0).When(x => x.EnableConnectionPooling).WithMessage("MaxPoolSize must be positive when pooling is enabled");
    }
}

public class UserServiceConfigurationValidator : AbstractValidator<UserServiceConfiguration>
{
    public UserServiceConfigurationValidator()
    {
        RuleFor(x => x.CacheExpirationMinutes).GreaterThan(0).WithMessage("Cache expiration must be positive");
        RuleFor(x => x.MaxConcurrentOperations).GreaterThan(0).WithMessage("Max concurrent operations must be positive");
    }
}

// Command validators
public class SendEmailCommandValidator : AbstractValidator<SendEmailCommand>
{
    public SendEmailCommandValidator()
    {
        RuleFor(x => x.To).NotEmpty().WithMessage("To address is required");
        RuleFor(x => x.Subject).NotEmpty().WithMessage("Subject is required");
        RuleFor(x => x.Body).NotEmpty().WithMessage("Email body is required");
    }
}

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid email address is required");
    }
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Valid user ID is required");
    }
}

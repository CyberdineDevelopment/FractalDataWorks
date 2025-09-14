using FractalDataWorks.Configuration;
using FractalDataWorks.EnhancedEnums;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConfigurationExample;

/// <summary>
/// Demonstrates FractalDataWorks Configuration patterns with various configuration sources,
/// validation, and dependency injection integration.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("FractalDataWorks Configuration Examples");
        Console.WriteLine("======================================\n");

        await RunBasicConfigurationExample();
        await RunValidatedConfigurationExample();
        await RunHostedConfigurationExample();
        await RunMultiSourceConfigurationExample();
    }

    /// <summary>
    /// Demonstrates basic configuration loading and usage
    /// </summary>
    static async Task RunBasicConfigurationExample()
    {
        Console.WriteLine("📋 Basic Configuration Example");
        Console.WriteLine("==============================");

        // Create basic configuration
        var appConfig = new ApplicationConfiguration
        {
            Name = "Sample Application",
            Version = "1.0.0",
            Environment = ApplicationEnvironment.Development,
            IsEnabled = true,
            MaxRetries = 3,
            TimeoutSeconds = 30
        };

        // Validate configuration
        var validationResult = appConfig.Validate();
        Console.WriteLine($"Configuration valid: {validationResult.IsValid}");
        
        if (validationResult.IsValid)
        {
            Console.WriteLine($"Application: {appConfig.Name} v{appConfig.Version}");
            Console.WriteLine($"Environment: {appConfig.Environment.Name}");
            Console.WriteLine($"Timeout: {appConfig.TimeoutSeconds}s");
            Console.WriteLine($"Max Retries: {appConfig.MaxRetries}");
        }

        await Task.CompletedTask;
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates configuration with validation failures
    /// </summary>
    static async Task RunValidatedConfigurationExample()
    {
        Console.WriteLine("✅ Configuration Validation Example");
        Console.WriteLine("===================================");

        // Create invalid configuration to demonstrate validation
        var invalidConfig = new DatabaseConfiguration
        {
            Name = "", // Invalid - required field
            ConnectionString = "invalid", // Invalid - too short
            MaxPoolSize = -1, // Invalid - negative value
            CommandTimeoutSeconds = 0 // Invalid - must be positive
        };

        var validationResult = invalidConfig.Validate();
        Console.WriteLine($"Configuration valid: {validationResult.IsValid}");
        
        if (!validationResult.IsValid)
        {
            Console.WriteLine("Validation errors:");
            foreach (var error in validationResult.Errors)
            {
                Console.WriteLine($"  • {error.PropertyName}: {error.ErrorMessage}");
            }
        }

        // Create valid configuration
        var validConfig = new DatabaseConfiguration
        {
            Name = "ProductionDB",
            ConnectionString = "Server=prod-server;Database=MyApp;Trusted_Connection=true;",
            MaxPoolSize = 100,
            CommandTimeoutSeconds = 30,
            IsEnabled = true
        };

        var validResult = validConfig.Validate();
        Console.WriteLine($"\nValid configuration: {validResult.IsValid}");
        if (validResult.IsValid)
        {
            Console.WriteLine($"Database: {validConfig.Name}");
            Console.WriteLine($"Pool Size: {validConfig.MaxPoolSize}");
            Console.WriteLine($"Timeout: {validConfig.CommandTimeoutSeconds}s");
        }

        await Task.CompletedTask;
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates configuration with dependency injection and hosting
    /// </summary>
    static async Task RunHostedConfigurationExample()
    {
        Console.WriteLine("🏗️ Hosted Configuration Example");
        Console.WriteLine("================================");

        // Build host with configuration
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Configure application configuration
                services.Configure<ApplicationConfiguration>(options =>
                {
                    options.Name = "Hosted Application";
                    options.Version = "2.0.0";
                    options.Environment = ApplicationEnvironment.Production;
                    options.MaxRetries = 5;
                    options.TimeoutSeconds = 60;
                });

                services.AddSingleton<ConfigurationService>();
            })
            .Build();

        // Use configuration service
        var configService = host.Services.GetRequiredService<ConfigurationService>();
        await configService.DemonstrateConfigurationUsage();

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates loading configuration from multiple sources
    /// </summary>
    static async Task RunMultiSourceConfigurationExample()
    {
        Console.WriteLine("🔧 Multi-Source Configuration Example");
        Console.WriteLine("====================================");

        // Build configuration from multiple sources
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Application:Name"] = "Multi-Source App",
                ["Application:Version"] = "1.0.0",
                ["Application:Environment"] = "Development",
                ["Database:ConnectionString"] = "Server=localhost;Database=Test;"
            })
            .Build();

        // Bind to configuration objects
        var appConfig = new ApplicationConfiguration();
        configuration.GetSection("Application").Bind(appConfig);

        var dbConfig = new DatabaseConfiguration();
        configuration.GetSection("Database").Bind(dbConfig);
        dbConfig.Name = "TestDB"; // Set required field not in config source
        dbConfig.MaxPoolSize = 50;
        dbConfig.CommandTimeoutSeconds = 15;

        Console.WriteLine("Loaded from configuration sources:");
        Console.WriteLine($"App: {appConfig.Name} v{appConfig.Version}");
        Console.WriteLine($"Environment: {appConfig.Environment?.Name ?? "Unknown"}");
        Console.WriteLine($"Database: {dbConfig.Name}");
        Console.WriteLine($"Connection: {dbConfig.ConnectionString}");

        await Task.CompletedTask;
        Console.WriteLine();
    }
}

/// <summary>
/// Application configuration with Enhanced Enums and validation
/// </summary>
public class ApplicationConfiguration : ConfigurationBase<ApplicationConfiguration>
{
    public override string SectionName => "Application";

    public string Version { get; set; } = "1.0.0";
    public ApplicationEnvironment Environment { get; set; } = ApplicationEnvironment.Development;
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;

    protected override IValidator<ApplicationConfiguration>? GetValidator()
    {
        return new ApplicationConfigurationValidator();
    }
}

/// <summary>
/// Database configuration with comprehensive validation
/// </summary>
public class DatabaseConfiguration : ConfigurationBase<DatabaseConfiguration>
{
    public override string SectionName => "Database";

    public string ConnectionString { get; set; } = string.Empty;
    public int MaxPoolSize { get; set; } = 100;
    public int CommandTimeoutSeconds { get; set; } = 30;
    public bool EnableRetryOnFailure { get; set; } = true;
    public int MaxRetryCount { get; set; } = 3;

    protected override IValidator<DatabaseConfiguration>? GetValidator()
    {
        return new DatabaseConfigurationValidator();
    }
}

/// <summary>
/// Application environment Enhanced Enum
/// </summary>
public sealed class ApplicationEnvironment : EnumOptionBase<ApplicationEnvironment>
{
    public static readonly ApplicationEnvironment Development = new(1, "Development", false, true);
    public static readonly ApplicationEnvironment Staging = new(2, "Staging", true, true);
    public static readonly ApplicationEnvironment Production = new(3, "Production", true, false);

    public bool RequiresSsl { get; }
    public bool AllowDebug { get; }

    private ApplicationEnvironment(int id, string name, bool requiresSsl, bool allowDebug)
        : base(id, name)
    {
        RequiresSsl = requiresSsl;
        AllowDebug = allowDebug;
    }
}

/// <summary>
/// Validator for application configuration
/// </summary>
public class ApplicationConfigurationValidator : AbstractValidator<ApplicationConfiguration>
{
    public ApplicationConfigurationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Application name is required")
            .MaximumLength(100)
            .WithMessage("Application name cannot exceed 100 characters");

        RuleFor(x => x.Version)
            .NotEmpty()
            .WithMessage("Version is required")
            .Matches(@"^\d+\.\d+\.\d+$")
            .WithMessage("Version must be in format x.y.z");

        RuleFor(x => x.MaxRetries)
            .GreaterThan(0)
            .WithMessage("MaxRetries must be greater than 0")
            .LessThanOrEqualTo(10)
            .WithMessage("MaxRetries cannot exceed 10");

        RuleFor(x => x.TimeoutSeconds)
            .GreaterThan(0)
            .WithMessage("TimeoutSeconds must be greater than 0")
            .LessThanOrEqualTo(300)
            .WithMessage("TimeoutSeconds cannot exceed 300 seconds");
    }
}

/// <summary>
/// Validator for database configuration
/// </summary>
public class DatabaseConfigurationValidator : AbstractValidator<DatabaseConfiguration>
{
    public DatabaseConfigurationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Database name is required");

        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("Connection string is required")
            .MinimumLength(10)
            .WithMessage("Connection string appears to be too short");

        RuleFor(x => x.MaxPoolSize)
            .GreaterThan(0)
            .WithMessage("MaxPoolSize must be greater than 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("MaxPoolSize cannot exceed 1000");

        RuleFor(x => x.CommandTimeoutSeconds)
            .GreaterThan(0)
            .WithMessage("CommandTimeoutSeconds must be greater than 0")
            .LessThanOrEqualTo(3600)
            .WithMessage("CommandTimeoutSeconds cannot exceed 1 hour");

        When(x => x.EnableRetryOnFailure, () =>
        {
            RuleFor(x => x.MaxRetryCount)
                .GreaterThan(0)
                .WithMessage("MaxRetryCount must be greater than 0 when retry is enabled")
                .LessThanOrEqualTo(10)
                .WithMessage("MaxRetryCount cannot exceed 10");
        });
    }
}

/// <summary>
/// Service demonstrating configuration usage in dependency injection
/// </summary>
public class ConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;
    }

    public async Task DemonstrateConfigurationUsage()
    {
        _logger.LogInformation("Configuration service demonstration");
        
        // In a real application, configuration would be injected via IOptions<T>
        var config = new ApplicationConfiguration
        {
            Name = "Injected App",
            Version = "1.0.0",
            Environment = ApplicationEnvironment.Production
        };

        _logger.LogInformation("Using configuration: {Name} v{Version} in {Environment}", 
            config.Name, config.Version, config.Environment.Name);

        Console.WriteLine($"Service using configuration: {config.Name} v{config.Version}");
        Console.WriteLine($"Environment: {config.Environment.Name}");
        Console.WriteLine($"SSL Required: {config.Environment.RequiresSsl}");
        Console.WriteLine($"Debug Allowed: {config.Environment.AllowDebug}");

        await Task.CompletedTask;
    }
}

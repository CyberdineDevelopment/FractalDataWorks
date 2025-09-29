# FractalDataWorks.Configuration

Configuration management with FluentValidation integration, configuration sources, and base classes for the FractalDataWorks framework.

## Overview

FractalDataWorks.Configuration provides core configuration management functionality:

**Core Components:**
- **FractalConfigurationBase** - Base class for all framework configurations with lifecycle hooks
- **ConfigurationBase<T>** - Generic base class with FluentValidation integration and cloning support
- **ConfigurationSourceBase** - Abstract base for implementing configuration data sources
- **JsonConfigurationSource** - File-based JSON configuration source implementation
- **ConfigurationValidatorBase<T>** - Base class for FluentValidation validators with common rules

**Key Features:**
- **FluentValidation Integration** - Built-in validation using `FluentValidation.Results.ValidationResult`
- **Configuration Sources** - Extensible source system for loading/saving configurations
- **Lifecycle Management** - Timestamps, modification tracking, and initialization hooks
- **Type Safety** - Strongly-typed configurations with generic constraints
- **Clone Support** - Deep cloning capabilities for configuration instances

## Core Configuration Classes

### FractalConfigurationBase - Framework Foundation

FractalConfigurationBase provides the base functionality for all configurations in the FractalDataWorks framework:

```csharp
/// <summary>
/// Base class for all configuration objects in the FractalDataWorks framework.
/// Provides common functionality for configuration validation and serialization.
/// </summary>
public abstract class FractalConfigurationBase : IFractalConfiguration
{
    /// <summary>
    /// Gets or sets the unique identifier for this configuration instance.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name of this configuration for lookup and display.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether this configuration is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Gets the section name for this configuration.
    /// </summary>
    public abstract string SectionName { get; }

    /// <summary>
    /// Validates the configuration settings.
    /// </summary>
    /// <returns>A GenericResult containing the FluentValidation ValidationResult.</returns>
    public virtual IGenericResult<ValidationResult> Validate()
    {
        // Default implementation returns success - derived classes should override
        return GenericResult<ValidationResult>.Success(new ValidationResult());
    }

    /// <summary>
    /// Initializes the configuration with default values.
    /// </summary>
    protected virtual void InitializeDefaults() { }

    /// <summary>
    /// Called after the configuration has been loaded and validated.
    /// </summary>
    protected virtual void OnConfigurationLoaded() { }
}
```

### ConfigurationBase<T> - Enhanced Configuration with Validation

ConfigurationBase<T> extends FractalConfigurationBase with additional functionality including validation integration, timestamping, and cloning:

```csharp
/// <summary>
/// Base class for all configuration types in the Rec framework.
/// </summary>
/// <typeparam name="TConfiguration">The derived configuration type.</typeparam>
public abstract class ConfigurationBase<TConfiguration> : FractalConfigurationBase
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
{
    /// <summary>
    /// Gets the timestamp when this configuration was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when this configuration was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Validates the configuration using FluentValidation.
    /// </summary>
    /// <returns>A GenericResult containing the FluentValidation ValidationResult.</returns>
    public override IGenericResult<ValidationResult> Validate()
    {
        var validator = GetValidator();
        if (validator == null)
        {
            // No validator configured, return success
            return GenericResult<ValidationResult>.Success(new ValidationResult());
        }

        var validationResult = validator.Validate((TConfiguration)this);
        // Always return success with the validation result
        // The caller should check validationResult.IsValid to determine if validation passed
        return GenericResult<ValidationResult>.Success(validationResult);
    }

    /// <summary>
    /// Gets the validator for this configuration type.
    /// </summary>
    /// <returns>The validator instance or null if no validation is required.</returns>
    protected virtual IValidator<TConfiguration>? GetValidator()
    {
        return null;
    }

    /// <summary>
    /// Marks this configuration as modified.
    /// </summary>
    protected void MarkAsModified()
    {
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a clone of this configuration.
    /// </summary>
    /// <returns>A cloned instance of the configuration.</returns>
    public virtual TConfiguration Clone()
    {
        var clone = new TConfiguration();
        CopyTo(clone);
        return clone;
    }

    /// <summary>
    /// Copies the properties of this configuration to another instance.
    /// </summary>
    /// <param name="target">The target configuration.</param>
    protected virtual void CopyTo(TConfiguration target)
    {
        target.Id = Id;
        target.Name = Name;
        target.IsEnabled = IsEnabled;
        target.CreatedAt = CreatedAt;
        target.ModifiedAt = ModifiedAt;
    }
}
```

### Example Configuration Implementation

Create strongly-typed configurations with built-in validation:

```csharp
public class EmailServiceConfiguration : ConfigurationBase<EmailServiceConfiguration>
{
    public override string SectionName => "EmailService";
    
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
    
    protected override IValidator<EmailServiceConfiguration>? GetValidator()
    {
        return new EmailServiceConfigurationValidator();
    }
    
    protected override void InitializeDefaults()
    {
        SmtpPort = 587;
        EnableSsl = true;
        TimeoutSeconds = 30;
    }
}

// FluentValidation validator using the base validator
public class EmailServiceConfigurationValidator : ConfigurationValidatorBase<EmailServiceConfiguration>
{
    public EmailServiceConfigurationValidator()
    {
        RuleFor(x => x.SmtpHost)
            .NotEmpty()
            .WithMessage("SMTP host is required");
            
        RuleFor(x => x.SmtpPort)
            .InclusiveBetween(1, 65535)
            .WithMessage("SMTP port must be between 1 and 65535");
            
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required");
            
        RuleFor(x => x.TimeoutSeconds)
            .GreaterThan(0)
            .WithMessage("Timeout must be positive");
    }
}
```

## Configuration Sources

Configuration sources provide the mechanism for loading, saving, and managing configuration data from various storage backends.

### ConfigurationSourceBase - Abstract Source Implementation

```csharp
/// <summary>
/// Base implementation of a configuration source.
/// </summary>
public abstract class ConfigurationSourceBase : IFractalConfigurationSource
{
    private readonly ILogger _logger;

    protected ConfigurationSourceBase(ILogger logger, string name)
    {
        _logger = logger;
        Name = name;
    }

    /// <summary>
    /// Gets the name of this configuration source.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this source supports write operations.
    /// </summary>
    public abstract bool IsWritable { get; }

    /// <summary>
    /// Gets a value indicating whether this source supports automatic reload.
    /// </summary>
    public abstract bool SupportsReload { get; }

    /// <summary>
    /// Occurs when the configuration source changes.
    /// </summary>
    public event EventHandler<ConfigurationSourceChangedEventArgs>? Changed;

    /// <summary>
    /// Loads configurations from this source.
    /// </summary>
    public abstract Task<IGenericResult<IEnumerable<TConfiguration>>> Load<TConfiguration>()
        where TConfiguration : IFractalConfiguration;

    /// <summary>
    /// Saves a configuration to this source.
    /// </summary>
    public virtual Task<IGenericResult<TConfiguration>> Save<TConfiguration>(TConfiguration configuration)
        where TConfiguration : IFractalConfiguration
    {
        if (!IsWritable)
        {
            return Task.FromResult<IGenericResult<TConfiguration>>(
                GenericResult<TConfiguration>.Failure<TConfiguration>($"Configuration source '{Name}' is read-only"));
        }

        return SaveCore(configuration);
    }

    /// <summary>
    /// Core implementation of save operation.
    /// </summary>
    protected abstract Task<IGenericResult<TConfiguration>> SaveCore<TConfiguration>(TConfiguration configuration)
        where TConfiguration : IFractalConfiguration;

    /// <summary>
    /// Core implementation of delete operation.
    /// </summary>
    protected abstract Task<IGenericResult<NonResult>> DeleteCore<TConfiguration>(int id)
        where TConfiguration : IFractalConfiguration;

    /// <summary>
    /// Raises the Changed event.
    /// </summary>
    protected void OnChanged(
        ConfigurationChangeTypeBase changeType,
        Type configurationType,
        int? configurationId = null)
    {
        var args = new ConfigurationSourceChangedEventArgs(changeType, configurationType, configurationId);
        Changed?.Invoke(this, args);
        
        ConfigurationSourceBaseLog.ConfigurationChanged(_logger, Name, changeType, configurationType.Name);
    }
}
```

### JsonConfigurationSource - File-Based Configuration Storage

```csharp
/// <summary>
/// Configuration source that reads and writes JSON files.
/// </summary>
public class JsonConfigurationSource : ConfigurationSourceBase
{
    private readonly string _basePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonConfigurationSource(ILogger<JsonConfigurationSource> logger, string basePath)
        : base(logger, "JSON")
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        Directory.CreateDirectory(_basePath);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public override bool IsWritable => true;
    public override bool SupportsReload => false;

    public override async Task<IGenericResult<IEnumerable<TConfiguration>>> Load<TConfiguration>()
    {
        var typeName = typeof(TConfiguration).Name;
        var pattern = $"{typeName}_*.json";
        var files = Directory.GetFiles(_basePath, pattern);

        var configurations = new List<TConfiguration>();

        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file).ConfigureAwait(false);
                var config = JsonSerializer.Deserialize<TConfiguration>(json, _jsonOptions);

                if (config != null)
                {
                    configurations.Add(config);
                }
            }
            catch (Exception ex)
            {
                // Log but continue loading other files
                ConfigurationSourceBaseLog.LoadFailed(Logger, Name, ex.Message);
            }
        }

        return GenericResult<IEnumerable<TConfiguration>>.Success(configurations);
    }

    protected override async Task<IGenericResult<TConfiguration>> SaveCore<TConfiguration>(TConfiguration configuration)
    {
        var fileName = GetFileName(configuration);
        var filePath = Path.Combine(_basePath, fileName);

        try
        {
            var json = JsonSerializer.Serialize(configuration, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);

            return GenericResult<TConfiguration>.Success(configuration);
        }
        catch (Exception ex)
        {
            return GenericResult<TConfiguration>.Failure($"Error saving configuration: {ex.Message}");
        }
    }

    private static string GetFileName<TConfiguration>(TConfiguration configuration)
        where TConfiguration : IFractalConfiguration
    {
        // Try to get ID if configuration has it
        int configId = 0;
        if (configuration is object obj && obj.GetType().GetProperty("Id") is { } idProperty)
        {
            configId = idProperty.GetValue(obj) as int? ?? 0;
        }
        var typeName = typeof(TConfiguration).Name;
        return $"{typeName}_{configId}.json";
    }
}
```

## Configuration Validation

### ConfigurationValidatorBase<T> - Common Validation Rules

The framework provides a base validator class with common validation rules:

```csharp
/// <summary>
/// Base validator for configuration types.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration to validate.</typeparam>
public abstract class ConfigurationValidatorBase<TConfiguration> : AbstractValidator<TConfiguration>
    where TConfiguration : ConfigurationBase<TConfiguration>, IFractalConfiguration, new()
{
    protected ConfigurationValidatorBase()
    {
        // Common validation rules for all configurations
        RuleFor(c => c.Id)
            .GreaterThan(0)
            .When(c => c.Id != 0)
            .WithMessage("Configuration ID must be greater than 0");

        RuleFor(c => c.Name)
            .NotEmpty()
            .WithMessage("Configuration name is required")
            .MaximumLength(100)
            .WithMessage("Configuration name must not exceed 100 characters");

        RuleFor(c => c.CreatedAt)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Created date cannot be in the future");

        RuleFor(c => c.ModifiedAt)
            .GreaterThanOrEqualTo(c => c.CreatedAt)
            .When(c => c.ModifiedAt.HasValue)
            .WithMessage("Modified date must be after created date");
    }
}
```

## Usage Examples

### Creating a Configuration

Implement configurations by extending ConfigurationBase<T>:

```csharp
public class DatabaseConfiguration : ConfigurationBase<DatabaseConfiguration>
{
    public override string SectionName => "Database";
    
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public bool EnableLogging { get; set; } = true;
    
    protected override IValidator<DatabaseConfiguration>? GetValidator()
    {
        return new DatabaseConfigurationValidator();
    }
    
    protected override void InitializeDefaults()
    {
        CommandTimeout = 30;
        MaxRetries = 3;
        EnableLogging = true;
    }
}

public class DatabaseConfigurationValidator : ConfigurationValidatorBase<DatabaseConfiguration>
{
    public DatabaseConfigurationValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty().WithMessage("Connection string is required")
            .Must(BeValidConnectionString).WithMessage("Invalid connection string format");
            
        RuleFor(x => x.CommandTimeout)
            .InclusiveBetween(1, 300).WithMessage("Command timeout must be between 1 and 300 seconds");
            
        RuleFor(x => x.MaxRetries)
            .InclusiveBetween(0, 10).WithMessage("Max retries must be between 0 and 10");
    }
    
    private bool BeValidConnectionString(string connectionString)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return !string.IsNullOrEmpty(builder.DataSource);
        }
        catch
        {
            return false;
        }
    }
}
```

### Simple Configuration (Using FractalConfigurationBase)

For basic configurations without enhanced features:

```csharp
public class SimpleConfiguration : FractalConfigurationBase
{
    public override string SectionName => "Simple";
    
    public string Value { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    public override IGenericResult<ValidationResult> Validate()
    {
        var validationResult = new ValidationResult();
        
        if (string.IsNullOrEmpty(Value))
        {
            validationResult.Errors.Add(new ValidationFailure(nameof(Value), "Value is required"));
        }
        
        return GenericResult<ValidationResult>.Success(validationResult);
    }
}
```

### Working with Configuration Sources

```csharp
// Using JSON configuration source
var logger = serviceProvider.GetRequiredService<ILogger<JsonConfigurationSource>>();
var jsonSource = new JsonConfigurationSource(logger, @"C:\Config");

// Load configurations
var loadResult = await jsonSource.Load<DatabaseConfiguration>();
if (loadResult.IsValid)
{
    foreach (var config in loadResult.Value)
    {
        Console.WriteLine($"Loaded config: {config.Name} (ID: {config.Id})");
    }
}

// Save a configuration
var newConfig = new DatabaseConfiguration
{
    Id = 1,
    Name = "Primary Database",
    ConnectionString = "Server=localhost;Database=MyApp;",
    CommandTimeout = 60
};

var saveResult = await jsonSource.Save(newConfig);
if (saveResult.IsValid)
{
    Console.WriteLine($"Configuration saved: {saveResult.Value.Name}");
}

// Load specific configuration by ID
var loadByIdResult = await jsonSource.Load<DatabaseConfiguration>(1);
if (loadByIdResult.IsValid)
{
    Console.WriteLine($"Loaded by ID: {loadByIdResult.Value.Name}");
}
```

### Validation Examples

Validating configurations using the built-in validation system:

```csharp
var config = new DatabaseConfiguration
{
    Id = 1,
    Name = "Primary Database",
    ConnectionString = "Server=localhost;Database=MyApp;",
    CommandTimeout = 60,
    MaxRetries = 3
};

// Validate using the configuration's Validate method
var validationResult = config.Validate();
if (validationResult.IsValid && validationResult.Value.IsValid)
{
    Console.WriteLine("Configuration is valid");
}
else if (validationResult.IsValid)
{
    Console.WriteLine("Configuration validation errors:");
    foreach (var error in validationResult.Value.Errors)
    {
        Console.WriteLine($"- {error.PropertyName}: {error.ErrorMessage}");
    }
}
else
{
    Console.WriteLine($"Validation system error: {validationResult.Message}");
}
```

### Configuration Lifecycle and Cloning

```csharp
var originalConfig = new DatabaseConfiguration
{
    Id = 1,
    Name = "Original",
    ConnectionString = "Server=localhost;Database=Original;"
};

// Clone the configuration
var clonedConfig = originalConfig.Clone();
clonedConfig.Name = "Cloned";
clonedConfig.ConnectionString = "Server=localhost;Database=Cloned;";

Console.WriteLine($"Original: {originalConfig.Name}");  // Output: "Original"
Console.WriteLine($"Cloned: {clonedConfig.Name}");      // Output: "Cloned"

// Timestamps are automatically managed
Console.WriteLine($"Created: {originalConfig.CreatedAt}");
Console.WriteLine($"Modified: {originalConfig.ModifiedAt ?? DateTime.MinValue}");
```

## Integration with Services

Configurations integrate with the FractalDataWorks service pattern:

```csharp
public class DatabaseService : ServiceBase<DatabaseCommand, DatabaseConfiguration, DatabaseService>
{
    public DatabaseService(ILogger<DatabaseService> logger, DatabaseConfiguration configuration)
        : base(logger, configuration)
    {
        // Validate configuration during service construction
        var validationResult = configuration.Validate();
        if (!validationResult.IsValid || !validationResult.Value.IsValid)
        {
            var errors = validationResult.Value.Errors
                .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                .ToList();
            var errorMessage = string.Join("; ", errors);
            throw new ArgumentException($"Invalid configuration: {errorMessage}");
        }
        
        // Access configuration via this.Configuration
        Logger.LogInformation("DatabaseService initialized with connection timeout: {Timeout}s", 
            Configuration.CommandTimeout);
    }
    
    public async Task<IGenericResult<string>> TestConnection()
    {
        try
        {
            using var connection = new SqlConnection(Configuration.ConnectionString);
            await connection.OpenAsync();
            return GenericResult<string>.Success("Connection successful");
        }
        catch (Exception ex)
        {
            return GenericResult<string>.Failure($"Connection failed: {ex.Message}");
        }
    }
}
```

## Advanced Features

### Configuration Event Handling

Configuration sources support change events:

```csharp
var jsonSource = new JsonConfigurationSource(logger, @"C:\Config");

// Subscribe to configuration changes
jsonSource.Changed += (sender, args) =>
{
    Console.WriteLine($"Configuration {args.ChangeType} for {args.ConfigurationType.Name}");
    if (args.ConfigurationId.HasValue)
    {
        Console.WriteLine($"Configuration ID: {args.ConfigurationId.Value}");
    }
};

// Changes will trigger events when configurations are saved/deleted
var config = new DatabaseConfiguration { Id = 1, Name = "Test" };
await jsonSource.Save(config);  // Triggers Added event
```

### Custom Configuration Sources

Implement custom configuration sources by extending ConfigurationSourceBase:

```csharp
public class DatabaseConfigurationSource : ConfigurationSourceBase
{
    private readonly string _connectionString;
    
    public DatabaseConfigurationSource(ILogger<DatabaseConfigurationSource> logger, string connectionString)
        : base(logger, "Database")
    {
        _connectionString = connectionString;
    }
    
    public override bool IsWritable => true;
    public override bool SupportsReload => false;
    
    public override async Task<IGenericResult<IEnumerable<TConfiguration>>> Load<TConfiguration>()
    {
        var configurations = new List<TConfiguration>();
        
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var typeName = typeof(TConfiguration).Name;
            using var command = new SqlCommand($"SELECT ConfigData FROM Configurations WHERE ConfigType = @type", connection);
            command.Parameters.AddWithValue("@type", typeName);
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var json = reader.GetString("ConfigData");
                var config = JsonSerializer.Deserialize<TConfiguration>(json);
                if (config != null)
                {
                    configurations.Add(config);
                }
            }
            
            return GenericResult<IEnumerable<TConfiguration>>.Success(configurations);
        }
        catch (Exception ex)
        {
            return GenericResult<IEnumerable<TConfiguration>>.Failure($"Failed to load configurations: {ex.Message}");
        }
    }
    
    protected override async Task<IGenericResult<TConfiguration>> SaveCore<TConfiguration>(TConfiguration configuration)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var json = JsonSerializer.Serialize(configuration);
            var typeName = typeof(TConfiguration).Name;
            
            using var command = new SqlCommand(
                "INSERT OR REPLACE INTO Configurations (ConfigType, ConfigId, ConfigData) VALUES (@type, @id, @data)", 
                connection);
            command.Parameters.AddWithValue("@type", typeName);
            command.Parameters.AddWithValue("@id", configuration.Id);
            command.Parameters.AddWithValue("@data", json);
            
            await command.ExecuteNonQueryAsync();
            
            OnChanged(ConfigurationChangeTypes.Added, typeof(TConfiguration), configuration.Id);
            return GenericResult<TConfiguration>.Success(configuration);
        }
        catch (Exception ex)
        {
            return GenericResult<TConfiguration>.Failure($"Failed to save configuration: {ex.Message}");
        }
    }
    
    protected override async Task<IGenericResult<NonResult>> DeleteCore<TConfiguration>(int id)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var typeName = typeof(TConfiguration).Name;
            using var command = new SqlCommand(
                "DELETE FROM Configurations WHERE ConfigType = @type AND ConfigId = @id", 
                connection);
            command.Parameters.AddWithValue("@type", typeName);
            command.Parameters.AddWithValue("@id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return GenericResult<NonResult>.Failure("Configuration not found");
            }
            
            OnChanged(ConfigurationChangeTypes.Deleted, typeof(TConfiguration), id);
            return GenericResult<NonResult>.Success(NonResult.Value);
        }
        catch (Exception ex)
        {
            return GenericResult<NonResult>.Failure($"Failed to delete configuration: {ex.Message}");
        }
    }
}
```

## Installation

```xml
<PackageReference Include="FractalDataWorks.Configuration" />
```

## Dependencies

- **FractalDataWorks.Configuration.Abstractions** - Core interfaces and contracts
- **Microsoft.Extensions.Configuration** - .NET configuration system integration
- **Microsoft.Extensions.Configuration.Binder** - Configuration binding support
- **Microsoft.Extensions.DependencyInjection.Abstractions** - Dependency injection abstractions
- **Microsoft.Extensions.Logging.Abstractions** - Logging abstractions
- **Microsoft.Extensions.Options** - Options pattern support
- **FluentValidation** - Configuration validation framework

## Best Practices

1. **Choose the Right Base Class**:
   - Use `ConfigurationBase<T>` for configurations that need validation, cloning, and timestamp management
   - Use `FractalConfigurationBase` for simple configurations with basic lifecycle needs

2. **Implement Validation**:
   - Use `ConfigurationValidatorBase<T>` as the base for your validators to get common validation rules
   - Override `GetValidator()` in `ConfigurationBase<T>` to return your custom validator
   - Always check both `validationResult.IsValid` and `validationResult.Value.IsValid`

3. **Handle Configuration Sources**:
   - Implement `ConfigurationSourceBase` for custom storage backends
   - Use `JsonConfigurationSource` for file-based configuration storage
   - Handle exceptions in source operations and return appropriate `GenericResult<T>` objects

4. **Configuration Design**:
   - Set sensible defaults in property initializers
   - Use the `IsEnabled` property for feature flags
   - Implement `InitializeDefaults()` for complex default value logic
   - Implement `OnConfigurationLoaded()` for post-load initialization

5. **Error Handling**:
   - Always check `GenericResult<T>.IsValid` before using the result
   - Log configuration errors appropriately using the provided logging methods
   - Validate configurations before using them in services

6. **Testing**:
   - Test configuration validation logic thoroughly
   - Test configuration source load/save operations
   - Use the clone functionality for creating test data variations

## Code Coverage Exclusions

The following classes are excluded from code coverage testing as they contain no business logic:

- **ConfigurationSourceBase** - Abstract base class with no business logic (marked with `[ExcludeFromCodeCoverage]`)
- **ConfigurationValidatorBase<T>** - Abstract base class with no business logic (marked with `[ExcludeFromCodeCoverage]`) 
- **ConfigurationSourceBaseLog** - Source-generated logging class (marked with `[ExcludeFromCodeCoverage]`)
- **ConfigurationProviderLog** - Source-generated logging class (if present, would be marked with `[ExcludeFromCodeCoverage]`)

## Testing Examples

```csharp
[Fact]
public void Configuration_Should_Be_Invalid_With_Empty_ConnectionString()
{
    var config = new DatabaseConfiguration
    {
        ConnectionString = string.Empty
    };
    
    var validationResult = config.Validate();
    
    Assert.True(validationResult.IsValid); // GenericResult is valid
    Assert.False(validationResult.Value.IsValid); // But validation failed
    Assert.Contains(validationResult.Value.Errors, 
        e => e.PropertyName == nameof(DatabaseConfiguration.ConnectionString));
}

[Fact]
public async Task JsonSource_Should_Load_And_Save_Configurations()
{
    var tempPath = Path.GetTempPath();
    var logger = Mock.Of<ILogger<JsonConfigurationSource>>();
    var source = new JsonConfigurationSource(logger, tempPath);
    
    // Save a configuration
    var config = new DatabaseConfiguration
    {
        Id = 1,
        Name = "Test Config",
        ConnectionString = "Server=test;Database=test;"
    };
    
    var saveResult = await source.Save(config);
    Assert.True(saveResult.IsValid);
    
    // Load configurations
    var loadResult = await source.Load<DatabaseConfiguration>();
    Assert.True(loadResult.IsValid);
    Assert.Contains(loadResult.Value, c => c.Id == 1 && c.Name == "Test Config");
}

[Fact]
public void ConfigurationBase_Should_Support_Cloning()
{
    var original = new DatabaseConfiguration
    {
        Id = 1,
        Name = "Original",
        ConnectionString = "Server=original;"
    };
    
    var clone = original.Clone();
    
    Assert.NotSame(original, clone);
    Assert.Equal(original.Id, clone.Id);
    Assert.Equal(original.Name, clone.Name);
    Assert.Equal(original.ConnectionString, clone.ConnectionString);
    
    // Modifying clone doesn't affect original
    clone.Name = "Modified";
    Assert.Equal("Original", original.Name);
    Assert.Equal("Modified", clone.Name);
}
```
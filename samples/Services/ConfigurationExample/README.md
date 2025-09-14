# Configuration Framework Example

**Status:** ✅ **Production Ready - Excellent Build (0 errors, minor XML warnings)**
**Learning Time:** 20-30 minutes
**Prerequisites:** Basic understanding of .NET configuration

## What This Sample Demonstrates

This sample showcases the FractalDataWorks Configuration Framework - a powerful system for type-safe, validated configuration management. You'll learn how to create strongly-typed configuration classes with comprehensive validation and Enhanced Enum integration.

### Key Features Shown:
- **Type-Safe Configuration**: Strongly-typed configuration classes with compile-time checking
- **FluentValidation Integration**: Comprehensive validation rules and error reporting
- **Enhanced Enum Integration**: Use Enhanced Enums in configuration seamlessly
- **Hierarchical Configuration**: Support for nested configuration sections
- **Error Handling**: Detailed validation error reporting and recovery
- **Environment Support**: Configuration loading from multiple sources

## Quick Start

```bash
# Navigate to the sample
cd samples/Services/ConfigurationExample

# Run the sample (always works!)
dotnet run
```

## Expected Output

```
⚙️ Configuration Framework Example
===================================

📋 Loading Application Configuration...
✅ Application configuration loaded successfully

🗃️ Database Configuration:
  • Connection: Server=localhost;Database=FractalDataWorks;Trusted_Connection=true;
  • Timeout: 30 seconds
  • Pool Size: 100 connections
  • Enable Retries: True
  • Default Priority: Normal

📧 Email Configuration:
  • SMTP Server: smtp.company.com
  • Port: 587
  • Use SSL: True
  • Default Priority: High
  • From Address: noreply@company.com

🔍 Configuration Validation Results:
✅ Database Configuration: Valid
✅ Email Configuration: Valid
✅ All configurations loaded and validated successfully

📊 Configuration Features Demonstrated:
  ✓ Type-safe configuration loading
  ✓ FluentValidation integration
  ✓ Enhanced Enum property binding
  ✓ Hierarchical configuration sections
  ✓ Comprehensive error reporting
  ✓ Environment-specific overrides

🎯 Configuration framework ready for production use!
```

## Code Structure

### Configuration Classes

#### Application Configuration
```csharp
public class ApplicationConfiguration : ConfigurationBase<ApplicationConfiguration>
{
    public override string SectionName => "Application";
    
    public string ApplicationName { get; set; } = "FractalDataWorks Sample";
    public string Environment { get; set; } = "Development";
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    
    protected override IValidator<ApplicationConfiguration> GetValidator()
    {
        return new ApplicationConfigurationValidator();
    }
}
```

#### Database Configuration with Enhanced Enums
```csharp
public class DatabaseConfiguration : ConfigurationBase<DatabaseConfiguration>
{
    public override string SectionName => "Database";
    
    public string ConnectionString { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public int PoolSize { get; set; } = 100;
    public bool EnableRetries { get; set; } = true;
    public Priority DefaultPriority { get; set; } = Priority.Normal;
    
    protected override IValidator<DatabaseConfiguration> GetValidator()
    {
        return new DatabaseConfigurationValidator();
    }
}
```

### FluentValidation Integration
```csharp
public class DatabaseConfigurationValidator : AbstractValidator<DatabaseConfiguration>
{
    public DatabaseConfigurationValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("Connection string is required")
            .MinimumLength(10)
            .WithMessage("Connection string appears to be too short");
            
        RuleFor(x => x.TimeoutSeconds)
            .GreaterThan(0)
            .WithMessage("Timeout must be greater than zero")
            .LessThanOrEqualTo(300)
            .WithMessage("Timeout should not exceed 5 minutes");
            
        RuleFor(x => x.PoolSize)
            .InclusiveBetween(1, 1000)
            .WithMessage("Pool size must be between 1 and 1000");
    }
}
```

## Configuration Files

### appsettings.json
```json
{
  "Application": {
    "ApplicationName": "FractalDataWorks Configuration Example",
    "Environment": "Development",
    "LogLevel": "Information"
  },
  "Database": {
    "ConnectionString": "Server=localhost;Database=FractalDataWorks;Trusted_Connection=true;",
    "TimeoutSeconds": 30,
    "PoolSize": 100,
    "EnableRetries": true,
    "DefaultPriority": "Normal"
  },
  "Email": {
    "SmtpServer": "smtp.company.com",
    "Port": 587,
    "UseSsl": true,
    "DefaultPriority": "High",
    "FromAddress": "noreply@company.com"
  }
}
```

### appsettings.Development.json
```json
{
  "Application": {
    "LogLevel": "Debug"
  },
  "Database": {
    "ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Database=FractalDataWorksDev;Trusted_Connection=true;",
    "TimeoutSeconds": 60
  }
}
```

## Learning Points

### 1. **Type Safety Benefits**

**Traditional Configuration (Error-Prone):**
```csharp
var timeout = int.Parse(configuration["Database:TimeoutSeconds"]);
var priority = configuration["Database:DefaultPriority"]; // String
```

**FractalDataWorks Configuration (Type-Safe):**
```csharp
var config = await DatabaseConfiguration.LoadAsync(configuration);
var timeout = config.Value.TimeoutSeconds; // Strongly typed int
var priority = config.Value.DefaultPriority; // Enhanced Enum
```

### 2. **Validation Integration**
- **Compile-time Safety**: Configuration classes with proper types
- **Runtime Validation**: FluentValidation rules with detailed error messages
- **Enhanced Enum Validation**: Automatic validation of enum values
- **Complex Rules**: Cross-property validation and business rules

### 3. **Configuration Loading Patterns**
```csharp
// Basic loading
var result = await DatabaseConfiguration.LoadAsync(configuration);
if (result.IsSuccessful)
{
    var dbConfig = result.Value;
    // Use configuration
}

// With error handling
var result = await DatabaseConfiguration.LoadAsync(configuration);
if (!result.IsSuccessful)
{
    foreach (var error in result.ValidationErrors)
    {
        logger.LogError("Configuration error: {Error}", error);
    }
}
```

### 4. **Enhanced Enum Integration**
Enhanced Enums work seamlessly in configuration:
- Automatic string-to-enum conversion
- Validation of enum values
- IntelliSense support in JSON files
- Type-safe property access

## Service Registration

### Dependency Injection Setup
```csharp
var builder = Host.CreateApplicationBuilder(args);

// Register configuration classes
builder.Services.AddConfiguration(builder.Configuration);

// Register individual configurations
builder.Services.Configure<DatabaseConfiguration>(
    builder.Configuration.GetSection("Database"));

// Register with validation
builder.Services.AddConfigurationWithValidation<EmailConfiguration>(
    builder.Configuration);

var app = builder.Build();
```

### Usage in Services
```csharp
public class EmailService : IFractalService<SendEmailCommand>
{
    private readonly EmailConfiguration _emailConfig;
    
    public EmailService(IOptions<EmailConfiguration> emailConfig)
    {
        _emailConfig = emailConfig.Value;
    }
    
    public async Task<IFractalResult> Execute(SendEmailCommand command)
    {
        // Use strongly-typed configuration
        var smtpClient = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.Port)
        {
            EnableSsl = _emailConfig.UseSsl
        };
        
        // Use Enhanced Enum
        var priority = _emailConfig.DefaultPriority.Level;
        
        // Process email...
        return FractalResult.Success();
    }
}
```

## Advanced Features

### Environment-Specific Configuration
The sample demonstrates:
- Base configuration in `appsettings.json`
- Environment overrides in `appsettings.Development.json`
- Automatic merging and precedence

### Configuration Validation Strategies
1. **Property-Level Validation**: Individual property rules
2. **Cross-Property Validation**: Rules involving multiple properties
3. **Business Rule Validation**: Complex domain-specific rules
4. **Enhanced Enum Validation**: Automatic enum value validation

### Error Handling Patterns
```csharp
// Graceful degradation
var config = await DatabaseConfiguration.LoadAsync(configuration);
if (!config.IsSuccessful)
{
    // Log errors
    logger.LogWarning("Database configuration invalid, using defaults");
    // Use default configuration
    var defaultConfig = new DatabaseConfiguration();
}
```

## Integration with Your Projects

To use the Configuration Framework:

```csharp
// 1. Add package reference
dotnet add package FractalDataWorks.Configuration

// 2. Create your configuration class
public class MyServiceConfiguration : ConfigurationBase<MyServiceConfiguration>
{
    public override string SectionName => "MyService";
    
    public string ApiUrl { get; set; } = string.Empty;
    public int TimeoutMs { get; set; } = 5000;
    public Priority ServicePriority { get; set; } = Priority.Normal;
    
    protected override IValidator<MyServiceConfiguration> GetValidator()
    {
        return new MyServiceConfigurationValidator();
    }
}

// 3. Create validation rules
public class MyServiceConfigurationValidator : AbstractValidator<MyServiceConfiguration>
{
    public MyServiceConfigurationValidator()
    {
        RuleFor(x => x.ApiUrl)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("ApiUrl must be a valid URI");
    }
}

// 4. Register and use
builder.Services.AddConfigurationWithValidation<MyServiceConfiguration>(configuration);
```

## Performance Characteristics

The Configuration Framework is optimized for production:
- **Startup Validation**: Configuration validated once at startup
- **Cached Results**: Parsed configuration cached for performance
- **Minimal Allocation**: Efficient validation and binding
- **Hot Reload Support**: Configuration changes detected automatically

## Troubleshooting

### Common Issues and Solutions

1. **Configuration Not Found**
   ```bash
   # Ensure appsettings.json is copied to output
   # Check Build Action: Content, Copy Always
   ```

2. **Validation Errors**
   ```csharp
   // The sample shows detailed error reporting
   // Check the console output for specific validation failures
   ```

3. **Enhanced Enum Binding Issues**
   ```json
   // Ensure enum values match exactly (case-sensitive)
   "DefaultPriority": "Normal"  // Correct
   "DefaultPriority": "normal"  // May fail
   ```

## Next Steps

After mastering this sample:

1. **Try ServicePatterns Sample**: See how configurations are used in services
2. **Create Your Own Configurations**: Replace hardcoded values in your projects
3. **Advanced Validation**: Explore complex validation scenarios
4. **Environment Management**: Set up production configuration patterns

## Why Configuration Framework Matters

Traditional configuration approaches suffer from:
- **String-based keys**: Runtime errors for typos
- **Type conversion issues**: Manual parsing and error handling
- **No validation**: Silent failures or runtime exceptions
- **Poor developer experience**: No IntelliSense or compile-time checking

The FractalDataWorks Configuration Framework provides:
- **Type safety**: Compile-time checking and IntelliSense
- **Comprehensive validation**: Detailed error reporting
- **Enhanced Enum integration**: Rich enum types in configuration
- **Production readiness**: Performance and reliability optimizations

This is essential infrastructure for building maintainable, reliable applications.
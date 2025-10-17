# FractalDataWorks.Configuration

Generic multi-tenant hierarchical configuration provider for Microsoft.Extensions.Configuration.

## Overview

FractalDataWorks.Configuration provides a flexible configuration system that separates **data loading** (Sources) from **data processing** (Provider). This enables easy integration with various storage backends while maintaining a consistent configuration interface.

**Architecture:**
- **FdwConfigurationProvider** - Generic provider that flattens and merges hierarchical configuration data
- **ConfigurationBase<T>** - Base class for strongly-typed configuration objects
- **IGenericConfiguration** - Interface for configuration metadata (in FractalDataWorks.Abstractions)
- **Configuration Sources** - Pluggable data sources (SQL Server, JSON, etc.)

**Key Features:**
- **Multi-Tenant Hierarchy** - Four levels: DEFAULT → APPLICATION → TENANT → USER
- **Source-Agnostic Provider** - Works with any data source via delegate pattern
- **Type-Safe Configurations** - Generic base class with automatic ServiceType identification
- **Microsoft.Extensions.Configuration Integration** - Standard .NET configuration system

## Core Components

### IGenericConfiguration - Base Interface

Located in `FractalDataWorks.Abstractions`:

```csharp
namespace FractalDataWorks.Abstractions;

/// <summary>
/// Base interface for all configuration types in the FractalDataWorks framework.
/// Provides identity and section metadata for configuration objects.
/// </summary>
public interface IGenericConfiguration
{
    /// <summary>
    /// Gets or sets the unique identifier for this configuration instance.
    /// </summary>
    int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of this configuration for lookup and display.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets the section name for this configuration (e.g., "Email", "Database").
    /// This is used when binding to IConfiguration sections.
    /// </summary>
    string SectionName { get; }
}
```

### IGenericConfiguration<T> - Generic Variant

Located in `FractalDataWorks.Configuration`:

```csharp
/// <summary>
/// Generic configuration interface with type-safe service type identification.
/// </summary>
/// <typeparam name="T">The concrete configuration type.</typeparam>
public interface IGenericConfiguration<T> : IGenericConfiguration
    where T : IGenericConfiguration<T>
{
    /// <summary>
    /// Gets the service type identifier for this configuration.
    /// Automatically set to the type name by ConfigurationBase{T}.
    /// </summary>
    string ServiceType { get; }
}
```

### ConfigurationBase<T> - Typed Configuration Base Class

```csharp
using System;
using FractalDataWorks.Abstractions;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Base class for all configuration types in the FractalDataWorks framework.
/// Provides common metadata properties and automatic service type identification.
/// </summary>
/// <typeparam name="T">The derived configuration type (for type-safe chaining).</typeparam>
public abstract class ConfigurationBase<T> : IGenericConfiguration<T>
    where T : ConfigurationBase<T>
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
    /// Gets the section name for this configuration in appsettings or database.
    /// Must be overridden by derived classes.
    /// </summary>
    /// <example>"Email", "Database", "Notification"</example>
    public abstract string SectionName { get; }

    /// <summary>
    /// Gets the service type identifier for this configuration.
    /// Automatically set to typeof(T).Name.
    /// </summary>
    public string ServiceType { get; } = typeof(T).Name;

    /// <summary>
    /// Gets the timestamp when this configuration instance was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when this configuration was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Marks this configuration as modified by setting ModifiedAt to current UTC time.
    /// </summary>
    protected void MarkAsModified()
    {
        ModifiedAt = DateTime.UtcNow;
    }
}
```

### FdwConfigurationProvider - Generic Provider

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Generic multi-tenant configuration provider that flattens hierarchical configuration data.
/// Works with any IConfigurationSource that provides hierarchical data.
/// </summary>
public class FdwConfigurationProvider : ConfigurationProvider
{
    private readonly IConfigurationSource _source;
    private readonly Func<IDictionary<int, IDictionary<string, object>>> _loadHierarchy;
    private readonly string _sectionName;

    /// <summary>
    /// Initializes a new instance of the provider.
    /// </summary>
    /// <param name="source">The configuration source.</param>
    /// <param name="loadHierarchy">Function to load hierarchical data from the source.</param>
    /// <param name="sectionName">Optional section name prefix for configuration keys.</param>
    public FdwConfigurationProvider(
        IConfigurationSource source,
        Func<IDictionary<int, IDictionary<string, object>>> loadHierarchy,
        string sectionName = "")
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _loadHierarchy = loadHierarchy ?? throw new ArgumentNullException(nameof(loadHierarchy));
        _sectionName = sectionName ?? string.Empty;
    }

    /// <summary>
    /// Loads configuration data by calling the source's LoadHierarchy function
    /// and flattening the result.
    /// </summary>
    public override void Load()
    {
        var hierarchy = _loadHierarchy();
        Data = FlattenHierarchy(hierarchy);
    }

    /// <summary>
    /// Flattens hierarchical configuration into a single dictionary.
    /// Merges levels in order: DEFAULT (0) → APPLICATION (1) → TENANT (2) → USER (3).
    /// Higher levels override lower levels.
    /// </summary>
    private Dictionary<string, string?> FlattenHierarchy(
        IDictionary<int, IDictionary<string, object>> hierarchy)
    {
        var flattened = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        // Merge in order: DEFAULT (0) → APPLICATION (1) → TENANT (2) → USER (3)
        for (int level = 0; level < 4; level++)
        {
            if (hierarchy.TryGetValue(level, out var levelData))
            {
                foreach (var kvp in levelData)
                {
                    // Skip hierarchy metadata columns
                    if (IsMetadataColumn(kvp.Key))
                    {
                        continue;
                    }

                    // Add with optional section prefix
                    var key = string.IsNullOrEmpty(_sectionName)
                        ? kvp.Key
                        : $"{_sectionName}:{kvp.Key}";

                    flattened[key] = kvp.Value?.ToString() ?? string.Empty;
                }
            }
        }

        return flattened;
    }

    /// <summary>
    /// Determines if a column is metadata that should be excluded from configuration.
    /// </summary>
    private static bool IsMetadataColumn(string columnName)
    {
        return columnName switch
        {
            "Id" or "Level" or "TenantId" or "UserId" or "CreatedAt" or "ModifiedAt" => true,
            _ => false
        };
    }
}
```

## Hierarchical Multi-Tenant Configuration

The configuration system supports a 4-level hierarchy that allows progressive overriding:

| Level | Name | Purpose | Example Use Case |
|-------|------|---------|------------------|
| 0 | DEFAULT | System-wide defaults | Factory defaults for all tenants |
| 1 | APPLICATION | Application-level settings | Environment-specific (Dev/Staging/Prod) |
| 2 | TENANT | Tenant-specific overrides | Per-customer customization |
| 3 | USER | User-specific settings | Individual user preferences |

**Merge Behavior:**
- Values are merged from level 0 → 1 → 2 → 3
- Higher-level values override lower-level values
- Only non-metadata columns are included in the final configuration

**Metadata Columns (Excluded):**
- `Id` - Configuration row identifier
- `Level` - Hierarchy level (0-3)
- `TenantId` - Tenant identifier (null for levels 0-1)
- `UserId` - User identifier (null for levels 0-2)
- `CreatedAt` - Timestamp when created
- `ModifiedAt` - Timestamp when modified

## Creating Configurations

### Example: Email Configuration

```csharp
using FractalDataWorks.Configuration;

public class EmailConfiguration : ConfigurationBase<EmailConfiguration>
{
    public override string SectionName => "Email";

    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
}
```

### Example: Database Configuration

```csharp
public class DatabaseConfiguration : ConfigurationBase<DatabaseConfiguration>
{
    public override string SectionName => "Database";

    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public bool EnableLogging { get; set; } = true;
}
```

## Configuration Sources

Configuration sources implement `IConfigurationSource` and provide a `LoadHierarchy()` method that returns hierarchical data. The source is responsible for querying the data store and returning a dictionary keyed by level.

### Source Contract

```csharp
public interface IConfigurationSource
{
    IConfigurationProvider Build(IConfigurationBuilder builder);
}

// Sources should implement LoadHierarchy:
IDictionary<int, IDictionary<string, object>> LoadHierarchy();
```

### Available Sources

- **FractalDataWorks.Configuration.Sources.MsSql** - SQL Server source using SqlKata

See individual source packages for usage examples.

## Usage with Microsoft.Extensions.Configuration

### Basic Setup

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Configuration.Sources.MsSql;

// Register SqlKata query factory
services.AddSqlKataQueryFactory(connectionString);

// Build configuration
var configuration = new ConfigurationBuilder()
    .Add(new MsSqlConfigurationSource
    {
        ServiceProvider = serviceProvider,
        SectionName = "Email",
        TenantId = "tenant-123",
        UserId = "user-456",
        TableName = "EmailConfiguration"
    })
    .Build();

// Bind to strongly-typed configuration
var emailConfig = new EmailConfiguration();
configuration.GetSection("Email").Bind(emailConfig);
```

### Multi-Source Configuration

```csharp
var configuration = new ConfigurationBuilder()
    // Add multiple configuration sources
    .Add(new MsSqlConfigurationSource
    {
        SectionName = "Email",
        TenantId = tenantId,
        TableName = "EmailConfiguration"
    })
    .Add(new MsSqlConfigurationSource
    {
        SectionName = "Database",
        TenantId = tenantId,
        TableName = "DatabaseConfiguration"
    })
    // Add JSON for local overrides
    .AddJsonFile("appsettings.json", optional: true)
    .Build();
```

## Architecture Principles

### 1. Separation of Concerns

**Source Responsibilities:**
- Connect to data store (SQL Server, JSON file, etc.)
- Execute queries/reads
- Transform raw data into hierarchy dictionary
- Handle connection/query errors

**Provider Responsibilities:**
- Receive hierarchy data via delegate
- Flatten multi-level structure
- Merge levels in correct order
- Filter metadata columns
- Apply section name prefixes

### 2. Delegate Pattern

The provider accepts a `Func<IDictionary<int, IDictionary<string, object>>>` delegate, allowing the source to control data loading while the provider handles processing:

```csharp
// Source provides the delegate:
public IConfigurationProvider Build(IConfigurationBuilder builder)
{
    return new FdwConfigurationProvider(this, LoadHierarchy, SectionName);
}

// Provider calls it during Load():
public override void Load()
{
    var hierarchy = _loadHierarchy();  // Calls source's LoadHierarchy method
    Data = FlattenHierarchy(hierarchy);
}
```

### 3. No Repository Pattern

Sources directly query the data store without an additional repository abstraction. This keeps the architecture simple and follows the single responsibility principle.

## Example: Multi-Tenant Email Configuration

### Database Schema

```sql
CREATE TABLE EmailConfiguration
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Level INT NOT NULL,                  -- 0=DEFAULT, 1=APP, 2=TENANT, 3=USER
    TenantId NVARCHAR(50) NULL,         -- NULL for levels 0-1
    UserId NVARCHAR(50) NULL,           -- NULL for levels 0-2
    SmtpHost NVARCHAR(100),
    SmtpPort INT,
    Username NVARCHAR(100),
    Password NVARCHAR(255),
    EnableSsl BIT,
    TimeoutSeconds INT,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedAt DATETIME2 NULL
);
```

### Sample Data

```sql
-- Level 0: DEFAULT - System-wide defaults
INSERT INTO EmailConfiguration (Level, SmtpHost, SmtpPort, EnableSsl, TimeoutSeconds)
VALUES (0, 'smtp.gmail.com', 587, 1, 30);

-- Level 1: APPLICATION - Production environment settings
INSERT INTO EmailConfiguration (Level, SmtpHost, SmtpPort)
VALUES (1, 'smtp.company.com', 25);

-- Level 2: TENANT - Tenant-specific override
INSERT INTO EmailConfiguration (Level, TenantId, SmtpHost, Username, Password)
VALUES (2, 'acme-corp', 'smtp.acme.com', 'noreply@acme.com', 'encrypted-password');

-- Level 3: USER - User preference
INSERT INTO EmailConfiguration (Level, TenantId, UserId, TimeoutSeconds)
VALUES (3, 'acme-corp', 'john.doe', 60);
```

### Result After Merge

For `TenantId = "acme-corp"` and `UserId = "john.doe"`:

```json
{
  "Email:SmtpHost": "smtp.acme.com",        // From TENANT (level 2)
  "Email:SmtpPort": "25",                   // From APPLICATION (level 1)
  "Email:Username": "noreply@acme.com",     // From TENANT (level 2)
  "Email:Password": "encrypted-password",   // From TENANT (level 2)
  "Email:EnableSsl": "True",                // From DEFAULT (level 0)
  "Email:TimeoutSeconds": "60"              // From USER (level 3)
}
```

## Installation

```xml
<PackageReference Include="FractalDataWorks.Configuration" />
<PackageReference Include="FractalDataWorks.Configuration.Sources.MsSql" />
```

## Dependencies

- **FractalDataWorks.Abstractions** - Contains `IGenericConfiguration` interface
- **Microsoft.Extensions.Configuration** - .NET configuration system
- **Microsoft.Extensions.Configuration.Abstractions** - Configuration abstractions

## Best Practices

1. **Use ConfigurationBase<T>** for strongly-typed configurations with automatic ServiceType
2. **Override SectionName** to match your configuration section naming convention
3. **Leverage the hierarchy** - put shared defaults at DEFAULT level, tenant customizations at TENANT level
4. **Use metadata wisely** - Id, Name, CreatedAt, ModifiedAt are automatically excluded from configuration values
5. **Keep sources focused** - Sources should only handle data loading, not processing
6. **Test multi-tenant scenarios** - Verify different TenantId/UserId combinations produce correct merged values

## Related Packages

- **FractalDataWorks.Configuration.Sources.MsSql** - SQL Server configuration source
- **FractalDataWorks.Abstractions** - Core interfaces including IGenericConfiguration

## See Also

- [Microsoft.Extensions.Configuration Documentation](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration)
- [Options Pattern in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)

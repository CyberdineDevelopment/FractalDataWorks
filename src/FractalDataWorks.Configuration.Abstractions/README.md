# FractalDataWorks.Configuration.Abstractions

Base interfaces and contracts for the FractalDataWorks hierarchical configuration system.

## What's Included

- `IGenericConfiguration` - Base interface for configuration identity
- `IGenericConfiguration<T>` - Generic interface with service type
- `ConfigurationBase<T>` - Optional base class with automatic metadata

## Purpose

This package provides **minimal abstractions** only. It has:
- ✅ Zero dependencies (except Microsoft.Extensions.Configuration.Abstractions)
- ✅ No circular dependencies
- ✅ No entity classes
- ✅ No repositories
- ✅ No FluentValidation

## When to Use

**Inherit from `ConfigurationBase<T>`** when you want automatic metadata:

```csharp
using FractalDataWorks.Configuration.Abstractions;

public class EmailSettings : ConfigurationBase<EmailSettings>
{
    public override string SectionName => "Email";

    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
}

// ServiceType is automatically set to "EmailSettings"
```

**Implement `IGenericConfiguration`** when you want full control:

```csharp
using FractalDataWorks.Configuration.Abstractions;

public class DatabaseSettings : IGenericConfiguration
{
    public int Id { get; set; }
    public string Name { get; set; } = "Database";
    public string SectionName => "Database";

    public string ConnectionString { get; set; } = string.Empty;
}
```

**Use plain POCOs** when you don't need configuration metadata:

```csharp
using System;

public class CacheSettings  // No inheritance
{
    public int MaxSize { get; set; } = 1000;
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(5);
}
```

## What You Get

### IGenericConfiguration

```csharp
using FractalDataWorks.Configuration.Abstractions;

int Id { get; set; }           // Unique identifier
string Name { get; set; }       // Display name
string SectionName { get; }     // Configuration section (e.g., "Email")
```

### IGenericConfiguration<T>

Adds:
```csharp
using FractalDataWorks.Configuration.Abstractions;

string ServiceType { get; }     // Type identifier (e.g., "EmailSettings")
```

### ConfigurationBase<T>

Adds:
```csharp
using System;
using FractalDataWorks.Configuration.Abstractions;

DateTime CreatedAt { get; }     // Creation timestamp
DateTime? ModifiedAt { get; set; }  // Modification timestamp
```

## Next Steps

1. Install provider package: `FractalDataWorks.Configuration.Providers.SqlServer`
2. Define your settings POCO (inheriting `ConfigurationBase<T>`)
3. Create database entity and repository
4. Register with DI container

See the [full documentation](../../docs/CONFIGURATION_SYSTEM.md) for complete usage guide.

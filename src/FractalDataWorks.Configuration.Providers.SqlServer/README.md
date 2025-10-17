# FractalDataWorks.Configuration.Providers.SqlServer

Hierarchical multi-tenant configuration provider for SQL Server using SqlKata.

## Features

- ✅ 4-level hierarchical configuration (DEFAULT → APPLICATION → TENANT → USER)
- ✅ Parallel loading for performance
- ✅ SqlKata query builder (no Entity Framework required)
- ✅ Plugs into Microsoft.Extensions.Configuration
- ✅ Works with IOptions/IOptionsSnapshot/IOptionsMonitor
- ✅ Generic repository base class

## Quick Start

### 1. Install Package

```bash
dotnet add package FractalDataWorks.Configuration.Providers.SqlServer
```

### 2. Create Database Table

```sql
CREATE TABLE EmailConfigurations (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    Level INT NOT NULL,
    TenantId NVARCHAR(100) NULL,
    UserId NVARCHAR(100) NULL,

    -- Your configuration properties
    SmtpHost NVARCHAR(200) NOT NULL,
    SmtpPort INT NOT NULL,
    EnableSsl BIT NOT NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedAt DATETIME2 NULL,

    CONSTRAINT UQ_EmailConfig_Hierarchy UNIQUE (Level, TenantId, UserId)
);
```

### 3. Create Repository

```csharp
using System;
using System.Collections.Generic;
using SqlKata.Execution;

public class EmailConfigurationRepository
    : HierarchicalConfigurationRepositoryBase<EmailConfigurationEntity>
{
    public EmailConfigurationRepository(QueryFactory queryFactory)
        : base(queryFactory)
    {
    }

    protected override string GetTableName() => "EmailConfigurations";

    protected override EmailConfigurationEntity MapFromDictionary(
        IDictionary<string, object> row)
    {
        return new EmailConfigurationEntity
        {
            Id = Convert.ToInt64(row["Id"]),
            Level = Convert.ToInt32(row["Level"]),
            SmtpHost = row["SmtpHost"]?.ToString() ?? "",
            SmtpPort = Convert.ToInt32(row["SmtpPort"]),
            EnableSsl = Convert.ToBoolean(row["EnableSsl"])
        };
    }
}
```

### 4. Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;

builder.Services.AddSqlKataQueryFactory(
    builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddScoped<EmailConfigurationRepository>();
```

### 5. Use in Service

```csharp
using System.Threading.Tasks;

public class EmailService
{
    private readonly EmailConfigurationRepository _repo;

    public async Task SendEmailAsync()
    {
        var hierarchy = await _repo.GetHierarchyAsync(tenantId, userId);

        // hierarchy[0] = DEFAULT
        // hierarchy[1] = APPLICATION
        // hierarchy[2] = TENANT (if tenantId provided)
        // hierarchy[3] = USER (if userId provided)
    }
}
```

## Documentation

See [full documentation](../../docs/CONFIGURATION_SYSTEM.md) for complete usage guide.

# FractalDataWorks Developer Kit - Template Usage Guide

**Complete guide to using dotnet templates for rapid service development**

## Table of Contents

- [Overview](#overview)
- [Template Installation](#template-installation)
- [FractalDataWorks.Service Template](#fractaldataworksservice-template)
- [FractalDataWorks.Service.Domain Template](#fractaldataworksservicedomain-template)
- [Template Customization](#template-customization)
- [Common Scenarios](#common-scenarios)
- [Visual Studio Integration](#visual-studio-integration)

## Overview

The FractalDataWorks Developer Kit provides dotnet templates for rapid scaffolding of service implementations following framework conventions and best practices.

### Available Templates

| Template | Short Name | Purpose |
|----------|-----------|---------|
| **FractalDataWorks Service** | `FractalDataWorks-service` | Create a new service implementation within an existing domain |
| **FractalDataWorks Service Domain** | `FractalDataWorks-service-domain` | Create a complete new service domain with abstractions and infrastructure |

## Template Installation

### From NuGet (Production)

```bash
# Install from NuGet
dotnet new install FractalDataWorks.Templates

# Verify installation
dotnet new list | grep FractalDataWorks
```

### From Local Package (Development)

```bash
# Build templates package
cd templates/package
dotnet pack -c Release

# Install locally
dotnet new install ./bin/Release/FractalDataWorks.Templates.1.0.0.nupkg

# Verify installation
dotnet new list FractalDataWorks
```

### From Source (Development)

```bash
# Install directly from template directory
cd templates
dotnet new install .

# Uninstall
dotnet new uninstall FractalDataWorks.Templates
```

### Update Templates

```bash
# Uninstall old version
dotnet new uninstall FractalDataWorks.Templates

# Install new version
dotnet new install FractalDataWorks.Templates
```

## FractalDataWorks.Service Template

### Purpose

Creates a new service implementation within an existing service domain. Use this template when you need to add a new provider/implementation to an existing domain (e.g., adding PostgreSQL support to the Connections domain).

### Quick Start

```bash
# Create new service implementation
dotnet new FractalDataWorks-service -n MyCompany.Services.Connections.PostgreSql -o src/MyCompany.Services.Connections.PostgreSql

# Navigate to project
cd src/MyCompany.Services.Connections.PostgreSql
```

### Template Parameters

| Parameter | Short | Description | Default |
|-----------|-------|-------------|---------|
| `--domain` | `-d` | Domain name (e.g., Connections, Authentication) | *(required)* |
| `--implementation` | `-i` | Implementation name (e.g., PostgreSql, AzureAd) | *(required)* |
| `--framework` | `-f` | Target framework | `net10.0` |
| `--abstractions-reference` | `-ar` | Path to abstractions project | `../Domain.Abstractions` |
| `--core-reference` | `-cr` | Path to core project | `../Domain` |

### Generated Structure

```
MyCompany.Services.Connections.PostgreSql/
├── MyCompany.Services.Connections.PostgreSql.csproj
├── ServiceTypes/
│   └── PostgreSqlConnectionType.cs          # ServiceType registration
├── Services/
│   └── PostgreSqlConnectionService.cs       # Service implementation
├── Commands/
│   └── PostgreSqlCommands.cs                # Concrete command implementations
├── Configuration/
│   └── PostgreSqlConnectionConfiguration.cs # Configuration implementation
├── Factories/
│   └── PostgreSqlConnectionFactory.cs       # Factory implementation
├── Translators/
│   └── PostgreSqlCommandTranslator.cs       # Command translator
└── README.md
```

### Example Usage

#### 1. Create PostgreSQL Connection Implementation

```bash
dotnet new FractalDataWorks-service \
  -n FractalDataWorks.Services.Connections.PostgreSql \
  -d Connections \
  -i PostgreSql \
  -ar ../FractalDataWorks.Services.Connections.Abstractions \
  -cr ../FractalDataWorks.Services.Connections \
  -o src/FractalDataWorks.Services.Connections.PostgreSql
```

#### 2. Generated ServiceType

```csharp
// ServiceTypes/PostgreSqlConnectionType.cs
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.PostgreSql.ServiceTypes;

[ServiceTypeOption(typeof(ConnectionTypes), "PostgreSql")]
public sealed class PostgreSqlConnectionType
    : ConnectionTypeBase<IGenericConnection, IConnectionConfiguration, IConnectionFactory<IGenericConnection, IConnectionConfiguration>>
{
    public PostgreSqlConnectionType()
        : base(200, "PostgreSql", "Services:Connections:PostgreSql",
               "PostgreSQL Connection", "PostgreSQL database connection service", "Database")
    {
    }

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IGenericConnection, PostgreSqlConnectionService>();
        services.AddScoped<IConnectionFactory<IGenericConnection, IConnectionConfiguration>, PostgreSqlConnectionFactory>();
        services.AddScoped<PostgreSqlCommandTranslator>();
    }
}
```

#### 3. Generated Service

```csharp
// Services/PostgreSqlConnectionService.cs
using FractalDataWorks.Services;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.PostgreSql.Services;

public sealed class PostgreSqlConnectionService
    : ServiceBase<IConnectionCommand, IConnectionConfiguration, PostgreSqlConnectionService>,
      IGenericConnection
{
    private readonly PostgreSqlCommandTranslator _translator;

    public PostgreSqlConnectionService(
        ILogger<PostgreSqlConnectionService> logger,
        IConnectionConfiguration configuration,
        PostgreSqlCommandTranslator translator)
        : base(logger, configuration)
    {
        _translator = translator ?? throw new ArgumentNullException(nameof(translator));
    }

    public override async Task<IGenericResult> Execute(IConnectionCommand command, CancellationToken cancellationToken = default)
    {
        return command switch
        {
            IConnectCommand connectCmd => await _translator.TranslateConnect(connectCmd, cancellationToken),
            IDisconnectCommand disconnectCmd => await _translator.TranslateDisconnect(disconnectCmd, cancellationToken),
            _ => GenericResult.Failure($"Unknown command type: {command.GetType().Name}")
        };
    }

    public override async Task<IGenericResult<T>> Execute<T>(IConnectionCommand command, CancellationToken cancellationToken = default)
    {
        var result = await Execute(command, cancellationToken);
        return result.IsSuccess
            ? GenericResult<T>.Success((T)result.Value!)
            : GenericResult<T>.Failure(result.Message);
    }

    // Implement domain-specific methods from IGenericConnection
    public async Task<IGenericResult> Connect(string connectionString, CancellationToken cancellationToken = default)
    {
        return await _translator.TranslateConnect(connectionString, cancellationToken);
    }

    public async Task<IGenericResult> Disconnect(CancellationToken cancellationToken = default)
    {
        return await _translator.TranslateDisconnect(cancellationToken);
    }
}
```

#### 4. Add to Solution

```bash
# Add project to solution
dotnet sln add src/FractalDataWorks.Services.Connections.PostgreSql/FractalDataWorks.Services.Connections.PostgreSql.csproj

# Restore packages
dotnet restore

# Build
dotnet build
```

## FractalDataWorks.Service.Domain Template

### Purpose

Creates a complete new service domain with abstractions and infrastructure projects. Use this template when creating an entirely new business domain (e.g., UserManagement, OrderProcessing).

### Quick Start

```bash
# Create new service domain
dotnet new FractalDataWorks-service-domain -n MyCompany.Services.UserManagement -o src/MyCompany.Services.UserManagement

# This creates:
# - MyCompany.Services.UserManagement.Abstractions/
# - MyCompany.Services.UserManagement/
```

### Template Parameters

| Parameter | Short | Description | Default |
|-----------|-------|-------------|---------|
| `--domain-name` | `-dn` | Domain name (e.g., UserManagement) | *(required)* |
| `--company` | `-c` | Company/organization name | `MyCompany` |
| `--abstractions-framework` | `-af` | Abstractions target framework | `netstandard2.0` |
| `--core-framework` | `-cf` | Core target framework | `net10.0` |
| `--include-sample-commands` | `-sc` | Include sample command interfaces | `true` |
| `--include-sample-messages` | `-sm` | Include sample message definitions | `true` |

### Generated Structure

The template creates **two projects**:

#### Abstractions Project (netstandard2.0)

```
MyCompany.Services.UserManagement.Abstractions/
├── MyCompany.Services.UserManagement.Abstractions.csproj
├── Commands/
│   ├── IUserManagementCommand.cs              # Base command interface
│   ├── ICreateUserCommand.cs                  # Sample command
│   └── IAuthenticateUserCommand.cs            # Sample command
├── Services/
│   └── IUserManagementService.cs              # Service interface
├── Configuration/
│   └── IUserManagementConfiguration.cs        # Configuration interface
├── IUserManagementProvider.cs                 # Provider interface
├── IUserManagementServiceType.cs              # ServiceType interface
├── UserManagementTypeBase.cs                  # ServiceType base class
├── Messages/
│   └── UserManagementMessage.cs               # Message base class
└── Logging/
    └── UserManagementServiceLog.cs            # Logging signatures
```

#### Core Project (net10.0)

```
MyCompany.Services.UserManagement/
├── MyCompany.Services.UserManagement.csproj
├── ServiceTypes/
│   └── UserManagementTypes.cs                 # ServiceType collection
├── Registration/
│   ├── UserManagementProvider.cs              # Provider implementation
│   └── UserManagementRegistrationOptions.cs   # Registration options
├── Services/
│   └── UserManagementServiceBase.cs           # Service base class (optional)
├── Messages/
│   ├── UserCreatedMessage.cs                  # Sample message
│   └── AuthenticationFailedMessage.cs         # Sample message
└── README.md
```

### Example Usage

#### 1. Create UserManagement Domain

```bash
dotnet new FractalDataWorks-service-domain \
  -n MyCompany.Services.UserManagement \
  -dn UserManagement \
  -c MyCompany \
  -o src/MyCompany.Services.UserManagement
```

#### 2. Generated Command Interface

```csharp
// Abstractions/Commands/IUserManagementCommand.cs
namespace MyCompany.Services.UserManagement.Abstractions;

public interface IUserManagementCommand : ICommand
{
}

// Abstractions/Commands/ICreateUserCommand.cs
namespace MyCompany.Services.UserManagement.Abstractions.Commands;

public interface ICreateUserCommand : IUserManagementCommand
{
    string Username { get; }
    string Email { get; }
    string Password { get; }
    IReadOnlyDictionary<string, object>? Properties { get; }
}
```

#### 3. Generated Service Interface

```csharp
// Abstractions/Services/IUserManagementService.cs
using FractalDataWorks.Services.Abstractions;
using MyCompany.Services.UserManagement.Abstractions.Configuration;

namespace MyCompany.Services.UserManagement.Abstractions.Services;

public interface IUserManagementService
    : IGenericService<IUserManagementCommand, IUserManagementConfiguration>
{
    // Domain-specific methods
    Task<IGenericResult<User>> GetUser(string userId, CancellationToken cancellationToken = default);
    Task<IGenericResult<User>> CreateUser(string username, string email, string password, CancellationToken cancellationToken = default);
    Task<IGenericResult<bool>> AuthenticateUser(string username, string password, CancellationToken cancellationToken = default);

    // Execute methods inherited from IGenericService
}
```

#### 4. Generated ServiceType Collection

```csharp
// ServiceTypes/UserManagementTypes.cs
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;

namespace MyCompany.Services.UserManagement.ServiceTypes;

[ServiceTypeCollection(typeof(UserManagementTypeBase<,,>), typeof(IUserManagementServiceType), typeof(UserManagementTypes))]
public partial class UserManagementTypes
    : ServiceTypeCollectionBase<
        UserManagementTypeBase<IUserManagementService, IUserManagementConfiguration, IUserManagementServiceFactory<IUserManagementService, IUserManagementConfiguration>>,
        IUserManagementServiceType,
        IUserManagementService,
        IUserManagementConfiguration,
        IUserManagementServiceFactory<IUserManagementService, IUserManagementConfiguration>>
{
    // Source generator creates:
    // - static UserManagementTypes All() => all registered service types
    // - static UserManagementTypes GetByName(string name) => lookup by name
    // - static void RegisterAll(IServiceCollection services) => register all services
}
```

#### 5. Add Projects to Solution

```bash
# Add both projects to solution
dotnet sln add src/MyCompany.Services.UserManagement.Abstractions/MyCompany.Services.UserManagement.Abstractions.csproj
dotnet sln add src/MyCompany.Services.UserManagement/MyCompany.Services.UserManagement.csproj

# Build
dotnet build
```

#### 6. Create First Implementation

Now use the `FractalDataWorks-service` template to create implementations:

```bash
dotnet new FractalDataWorks-service \
  -n MyCompany.Services.UserManagement.Database \
  -d UserManagement \
  -i Database \
  -ar ../MyCompany.Services.UserManagement.Abstractions \
  -cr ../MyCompany.Services.UserManagement \
  -o src/MyCompany.Services.UserManagement.Database
```

## Template Customization

### Modifying Templates

Templates are located in `templates/` directory:

```
templates/
├── FractalDataWorks.Service/
│   ├── template.json              # Template configuration
│   ├── .template.config/
│   │   └── template.json          # Metadata
│   └── content/                   # Template files
│       ├── ServiceType.cs
│       ├── Service.cs
│       └── ...
└── FractalDataWorks.Service.Domain/
    ├── template.json
    └── ...
```

### Customizing Parameters

Edit `template.json` to add custom parameters:

```json
{
  "author": "FractalDataWorks Electric Cooperative",
  "classifications": ["FractalDataWorks", "Service", "Domain"],
  "name": "FractalDataWorks Service Domain",
  "identity": "FractalDataWorks.Service.Domain",
  "shortName": "FractalDataWorks-service-domain",
  "tags": {
    "language": "C#",
    "type": "project"
  },
  "symbols": {
    "domainName": {
      "type": "parameter",
      "datatype": "string",
      "description": "The name of the service domain",
      "replaces": "DomainName",
      "fileRename": "DomainName"
    },
    "customParameter": {
      "type": "parameter",
      "datatype": "bool",
      "description": "Your custom parameter",
      "defaultValue": "false"
    }
  }
}
```

### Testing Custom Templates

```bash
# Install local template
dotnet new install ./templates/FractalDataWorks.Service.Domain

# Test template
dotnet new FractalDataWorks-service-domain -n TestProject -dn Test

# Uninstall
dotnet new uninstall FractalDataWorks.Service.Domain
```

## Common Scenarios

### Scenario 1: Adding New Authentication Provider

```bash
# 1. Create Azure AD authentication implementation
dotnet new FractalDataWorks-service \
  -n FractalDataWorks.Services.Authentication.AzureAd \
  -d Authentication \
  -i AzureAd \
  -ar ../FractalDataWorks.Services.Authentication.Abstractions \
  -cr ../FractalDataWorks.Services.Authentication \
  -o src/FractalDataWorks.Services.Authentication.AzureAd

# 2. Add to solution
dotnet sln add src/FractalDataWorks.Services.Authentication.AzureAd

# 3. Implement required methods in:
# - Services/AzureAdAuthenticationService.cs
# - Translators/AzureAdCommandTranslator.cs
# - Configuration/AzureAdAuthenticationConfiguration.cs

# 4. Build and test
dotnet build
dotnet test
```

### Scenario 2: Creating New Business Domain

```bash
# 1. Create domain projects
dotnet new FractalDataWorks-service-domain \
  -n MyCompany.Services.OrderProcessing \
  -dn OrderProcessing \
  -c MyCompany \
  -o src/MyCompany.Services.OrderProcessing

# 2. Add to solution
dotnet sln add src/MyCompany.Services.OrderProcessing.Abstractions
dotnet sln add src/MyCompany.Services.OrderProcessing

# 3. Create first implementation (e.g., database)
dotnet new FractalDataWorks-service \
  -n MyCompany.Services.OrderProcessing.Database \
  -d OrderProcessing \
  -i Database \
  -ar ../MyCompany.Services.OrderProcessing.Abstractions \
  -cr ../MyCompany.Services.OrderProcessing \
  -o src/MyCompany.Services.OrderProcessing.Database

# 4. Add to solution
dotnet sln add src/MyCompany.Services.OrderProcessing.Database

# 5. Customize commands and services
# Edit generated files to match your business requirements
```

### Scenario 3: Creating Multiple Implementations

```bash
# Create database implementation
dotnet new FractalDataWorks-service \
  -n Services.Data.SqlServer \
  -d Data \
  -i SqlServer

# Create API implementation
dotnet new FractalDataWorks-service \
  -n Services.Data.RestApi \
  -d Data \
  -i RestApi

# Create file-based implementation
dotnet new FractalDataWorks-service \
  -n Services.Data.FileSystem \
  -d Data \
  -i FileSystem

# All implementations automatically discovered by DataTypes collection
```

## Visual Studio Integration

### Using Templates in Visual Studio

If you've installed the VSIX extension:

1. **File → New → Project**
2. Search for "FractalDataWorks"
3. Select template:
   - **FractalDataWorks Service** - New implementation
   - **FractalDataWorks Service Domain** - New domain
4. Configure options in wizard
5. Click **Create**

### VSIX Installation

```powershell
# Install VSIX from package
.\install\install.ps1 -IncludeVsix

# Or install manually
start dist\FractalDataWorks.Templates.Extension.vsix
```

The VSIX provides:
- **GUI wizard** with parameter selection
- **IntelliSense** for template parameters
- **Project templates** in New Project dialog
- **Item templates** for adding files to existing projects

### Template Properties in Visual Studio

When creating a project via Visual Studio, the wizard shows:

- **Domain Name** (required)
- **Implementation Name** (for service template)
- **Target Framework** (net10.0, net8.0, etc.)
- **Include Sample Code** (checkboxes for commands, messages, etc.)
- **Company Name** (for namespace generation)

---

## Summary

The FractalDataWorks templates provide:

1. **Rapid Scaffolding**: Create complete domains and implementations in seconds
2. **Best Practices**: Generated code follows framework conventions
3. **Consistency**: All generated code uses the same patterns
4. **Customizable**: Templates can be modified for specific needs
5. **IDE Integration**: Visual Studio support via VSIX extension

### Quick Reference

**Create new domain**:
```bash
dotnet new FractalDataWorks-service-domain -n MyCompany.Services.MyDomain -dn MyDomain
```

**Create new implementation**:
```bash
dotnet new FractalDataWorks-service -n MyCompany.Services.MyDomain.MyImpl -d MyDomain -i MyImpl
```

**List installed templates**:
```bash
dotnet new list | grep FractalDataWorks
```

**Update templates**:
```bash
dotnet new uninstall FractalDataWorks.Templates && dotnet new install FractalDataWorks.Templates
```

Use these templates to accelerate development while maintaining consistency with the FractalDataWorks framework architecture.

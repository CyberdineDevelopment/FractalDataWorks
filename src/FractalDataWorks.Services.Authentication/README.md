# FractalDataWorks.Services.Authentication

## Overview

The Authentication framework provides ServiceType auto-discovery for authentication providers with unified interfaces that work across Azure Entra, Auth0, custom providers, and more.

## Features

- **ServiceType Auto-Discovery**: Add authentication packages and they're automatically registered
- **Universal Authentication Interface**: Same API works with all providers
- **Dynamic Provider Creation**: Authentication services created via factories
- **Source-Generated Collections**: High-performance provider lookup

## Quick Start

### 1. Install Packages

```xml
<ProjectReference Include="..\FractalDataWorks.Services.Authentication\FractalDataWorks.Services.Authentication.csproj" />
<ProjectReference Include="..\FractalDataWorks.Services.Authentication.AzureEntra\FractalDataWorks.Services.Authentication.AzureEntra.csproj" />
```

### 2. Register Services

```csharp
// Program.cs - Zero-configuration registration
builder.Services.AddScoped<IGenericAuthenticationProvider, GenericAuthenticationProvider>();

// Single line registers ALL discovered authentication types
AuthenticationTypes.Register(builder.Services);
```

### 3. Configure Authentication

```json
{
  "Authentication": {
    "AzureEntra": {
      "AuthenticationType": "AzureEntra",
      "TenantId": "your-tenant-id",
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret"
    }
  }
}
```

### 4. Use Universal Authentication

```csharp
public class UserController : ControllerBase
{
    private readonly IGenericAuthenticationProvider _authProvider;

    public UserController(IGenericAuthenticationProvider authProvider)
    {
        _authProvider = authProvider;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var authResult = await _authProvider.GetAuthenticator("AzureEntra");
        if (!authResult.IsSuccess)
            return Problem(authResult.Error);

        using var authenticator = authResult.Value;
        var result = await authenticator.AuthenticateAsync(request.Username, request.Password);

        return result.IsSuccess
            ? Ok(new { Token = result.Value.Token })
            : Unauthorized(new { Error = result.Error });
    }
}
```

## Available Authentication Types

| Package | Authentication Type | Purpose |
|---------|-------------------|---------|
| `FractalDataWorks.Services.Authentication.AzureEntra` | AzureEntra | Azure Active Directory |
| `FractalDataWorks.Services.Authentication.Auth0` | Auth0 | Auth0 authentication |
| `FractalDataWorks.Services.Authentication.Custom` | Custom | Custom authentication providers |

## How Auto-Discovery Works

1. **Source Generator Scans**: `[ServiceTypeCollection]` attribute triggers compile-time discovery
2. **Finds Implementations**: Scans referenced assemblies for types inheriting from `AuthenticationTypeBase`
3. **Generates Collections**: Creates `AuthenticationTypes.All`, `AuthenticationTypes.Name()`, etc.
4. **Self-Registration**: Each authentication type handles its own DI registration

## Adding Custom Authentication Types

```csharp
// 1. Create your authentication type (singleton pattern)
public sealed class CustomAuthenticationType : AuthenticationTypeBase<IGenericAuthenticator, CustomAuthenticationConfiguration, ICustomAuthenticationFactory>
{
    public static CustomAuthenticationType Instance { get; } = new();

    private CustomAuthenticationType() : base(3, "Custom", "Authentication Providers") { }

    public override Type FactoryType => typeof(ICustomAuthenticationFactory);

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<ICustomAuthenticationFactory, CustomAuthenticationFactory>();
        services.AddScoped<CustomTokenValidator>();
        services.AddScoped<CustomUserResolver>();
    }
}

// 2. Add package reference - source generator automatically discovers it
// 3. AuthenticationTypes.Register(services) will include it automatically
```

## Architecture Benefits

- **Provider Agnostic**: Switch authentication providers without code changes
- **Zero Configuration**: Add package reference, get functionality
- **Type Safety**: Compile-time validation of authentication types
- **Performance**: Source-generated collections use FrozenDictionary
- **Scalability**: Each authentication type manages its own dependencies

For complete architecture details, see [Services.Abstractions README](../FractalDataWorks.Services.Abstractions/README.md).
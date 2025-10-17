# FractalDataWorks.Services.SecretManager

## Overview

The Secret Management framework provides ServiceType auto-discovery for secret storage providers with unified interfaces that work across Azure Key Vault, HashiCorp Vault, AWS Secrets Manager, and more.

## Features

- **ServiceType Auto-Discovery**: Add secret management packages and they're automatically registered
- **Universal Secret Interface**: Same API works with all secret storage providers
- **Dynamic Provider Creation**: Secret services created via factories
- **Source-Generated Collections**: High-performance provider lookup

## Quick Start

### 1. Install Packages

```xml
<ProjectReference Include="..\FractalDataWorks.Services.SecretManager\FractalDataWorks.Services.SecretManagers.csproj" />
<ProjectReference Include="..\FractalDataWorks.Services.SecretManagers.AzureKeyVault\FractalDataWorks.Services.SecretManagers.AzureKeyVault.csproj" />
```

### 2. Register Services

```csharp
using FractalDataWorks.Services.SecretManagers;
using Microsoft.Extensions.DependencyInjection;

// Program.cs - Zero-configuration registration
builder.Services.AddScoped<IGenericSecretProvider, GenericSecretProvider>();

// Single line registers ALL discovered secret management types
SecretManagerTypes.Register(builder.Services);
```

### 3. Configure Secret Management

```json
{
  "SecretManager": {
    "AzureKeyVault": {
      "SecretType": "AzureKeyVault",
      "VaultUrl": "https://your-vault.vault.azure.net/",
      "TenantId": "your-tenant-id",
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret"
    }
  }
}
```

### 4. Use Universal Secret Management

```csharp
using FractalDataWorks.Common;
using FractalDataWorks.Services.SecretManagers;

public class EmailService
{
    private readonly IGenericSecretProvider _secretProvider;

    public EmailService(IGenericSecretProvider secretProvider)
    {
        _secretProvider = secretProvider;
    }

    public async Task<IGenericResult> SendEmailAsync(string to, string subject, string body)
    {
        var secretResult = await _secretProvider.GetSecretManager("AzureKeyVault");
        if (!secretResult.IsSuccess)
            return GenericResult.Failure(secretResult.Error);

        using var secretManager = secretResult.Value;

        // Universal secret retrieval - works with any provider
        var smtpPassword = await secretManager.GetSecretAsync("smtp-password");
        if (!smtpPassword.IsSuccess)
            return GenericResult.Failure(smtpPassword.Error);

        // Use the secret for email sending
        var emailResult = await SendEmailWithPassword(to, subject, body, smtpPassword.Value);
        return emailResult;
    }
}
```

## Available Secret Management Types

| Package | Secret Type | Purpose |
|---------|------------|---------|
| `FractalDataWorks.Services.SecretManagers.AzureKeyVault` | AzureKeyVault | Azure Key Vault |
| `FractalDataWorks.Services.SecretManagers.HashiCorpVault` | HashiCorpVault | HashiCorp Vault |
| `FractalDataWorks.Services.SecretManagers.AwsSecretsManager` | AwsSecretsManager | AWS Secrets Manager |

## How Auto-Discovery Works

1. **Source Generator Scans**: `[ServiceTypeCollection]` attribute triggers compile-time discovery
2. **Finds Implementations**: Scans referenced assemblies for types inheriting from `SecretManagerTypeBase`
3. **Generates Collections**: Creates `SecretManagerTypes.All`, `SecretManagerTypes.Name()`, etc.
4. **Self-Registration**: Each secret management type handles its own DI registration

## Adding Custom Secret Management Types

```csharp
using FractalDataWorks.Services.SecretManagers;
using Microsoft.Extensions.DependencyInjection;

// 1. Create your secret management type (singleton pattern)
public sealed class CustomVaultType : SecretManagerTypeBase<IGenericSecretManager, CustomVaultConfiguration, ICustomVaultFactory>
{
    public static CustomVaultType Instance { get; } = new();

    private CustomVaultType() : base(4, "CustomVault", "Secret Management Providers") { }

    public override Type FactoryType => typeof(ICustomVaultFactory);

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<ICustomVaultFactory, CustomVaultFactory>();
        services.AddScoped<CustomVaultClient>();
        services.AddScoped<CustomSecretEncryption>();
    }
}

// 2. Add package reference - source generator automatically discovers it
// 3. SecretManagerTypes.Register(services) will include it automatically
```

## Architecture Benefits

- **Provider Agnostic**: Switch secret providers without code changes
- **Zero Configuration**: Add package reference, get functionality
- **Type Safety**: Compile-time validation of secret management types
- **Performance**: Source-generated collections use FrozenDictionary
- **Security**: Unified interface ensures consistent secret handling patterns

For complete architecture details, see [Services.Abstractions README](../FractalDataWorks.Services.Abstractions/README.md).
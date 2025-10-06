using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.SecretManagers.Abstractions;
using FractalDataWorks.Services.SecretManager;

namespace FractalDataWorks.Services.SecretManagers.AzureKeyVault;

/// <summary>
/// Service type definition for Azure Key Vault secret management implementation.
/// </summary>
[ServiceTypeOption(typeof(SecretManagerServiceTypes), "AzureKeyVault")]
public sealed class AzureKeyVaultServiceType : SecretManagerTypeBase<ISecretManager, ISecretManagerServiceFactory<ISecretManager, ISecretManagerConfiguration>, ISecretManagerConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultServiceType"/> class.
    /// </summary>
    public AzureKeyVaultServiceType()
        : base(
            id: Guid.Parse("8F7E6D5C-4B3A-2190-EDCB-A09876543210").GetHashCode(), // Deterministic from type name
            name: "AzureKeyVault",
            sectionName: "Services:SecretManagers:AzureKeyVault",
            displayName: "Azure Key Vault Secret Manager",
            description: "Microsoft Azure Key Vault secret management service",
            supportedSecretStores: ["AzureKeyVault", "Azure Key Vault", "KeyVault", "Microsoft Azure Key Vault"],
            supportedSecretTypes: ["Secret", "Password", "ApiKey", "ConnectionString", "Certificate"],
            supportsRotation: true,
            supportsVersioning: true,
            supportsSoftDelete: true,
            supportsAccessPolicies: true,
            maxSecretSizeBytes: 25000) // Azure Key Vault limit
    {
    }

    /// <summary>
    /// Registers services required by this service type with the DI container.
    /// </summary>
    public override void Register(IServiceCollection services)
    {
        // Register the configuration
        services.AddOptions<AzureKeyVaultConfiguration>()
            .BindConfiguration(SectionName)
            .ValidateDataAnnotations();

        // Register the service factory
        services.AddScoped<ISecretManagerServiceFactory<ISecretManager, ISecretManagerConfiguration>, SecretManagerServiceFactory<ISecretManager, ISecretManagerConfiguration>>();

        // Register the secret management service
        services.AddScoped<ISecretManager, AzureKeyVaultService>();
        services.AddScoped<AzureKeyVaultService>();
    }

    /// <summary>
    /// Configures the service type using the provided configuration.
    /// </summary>
    public override void Configure(IConfiguration configuration)
    {
        // Configuration is handled by the options pattern in Register
        // Additional runtime configuration can be added here if needed
    }
}

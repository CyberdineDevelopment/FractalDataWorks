using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.SecretManagement.Abstractions;

namespace FractalDataWorks.Services.SecretManagement;

/// <summary>
/// ServiceType collection for all secret management types.
/// The source generator will discover all SecretManagementTypeBase implementations.
/// </summary>
[ServiceTypeCollection("ISecretManagementType", "SecretManagementTypes")]
public static partial class SecretManagementTypes
{
    /// <summary>
    /// Registers all discovered secret management types with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void Register(IServiceCollection services)
    {
        // Register each discovered secret management type
        foreach (var secretManagementType in All)
        {
            secretManagementType.Register(services);
        }
    }

    /// <summary>
    /// Registers all discovered secret management types with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Custom configuration action for special cases.</param>
    public static void Register(IServiceCollection services, Action<SecretManagementRegistrationOptions> configure)
    {
        var options = new SecretManagementRegistrationOptions();
        configure(options);

        // Register each discovered secret management type with custom options
        foreach (var secretManagementType in All)
        {
            secretManagementType.Register(services);

            // Apply custom configuration if needed
            if (options.CustomConfigurations.TryGetValue(secretManagementType.Name, out var customConfig))
            {
                customConfig(services, secretManagementType);
            }
        }
    }
}

/// <summary>
/// Configuration options for secret management registration.
/// </summary>
public class SecretManagementRegistrationOptions
{
    /// <summary>
    /// Custom configurations for specific secret management types.
    /// </summary>
    public Dictionary<string, Action<IServiceCollection, ISecretManagementType>> CustomConfigurations { get; } = new();

    /// <summary>
    /// Configure a specific secret management type.
    /// </summary>
    /// <param name="secretManagementTypeName">The name of the secret management type (e.g., "AzureKeyVault").</param>
    /// <param name="configure">Custom configuration action.</param>
    public void Configure(string secretManagementTypeName, Action<IServiceCollection, ISecretManagementType> configure)
    {
        CustomConfigurations[secretManagementTypeName] = configure;
    }
}
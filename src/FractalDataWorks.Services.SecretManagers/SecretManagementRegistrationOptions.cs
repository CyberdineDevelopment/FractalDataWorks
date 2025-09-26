using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.SecretManagers.Abstractions;

namespace FractalDataWorks.Services.SecretManager;

/// <summary>
/// Configuration options for secret management registration.
/// </summary>
public class SecretManagerRegistrationOptions
{
    /// <summary>
    /// Custom configurations for specific secret management types.
    /// </summary>
    public Dictionary<string, Action<IServiceCollection, ISecretManagerType>> CustomConfigurations { get; } = new();

    /// <summary>
    /// Configure a specific secret management type.
    /// </summary>
    /// <param name="SecretManagerTypeName">The name of the secret management type (e.g., "AzureKeyVault").</param>
    /// <param name="configure">Custom configuration action.</param>
    public void Configure(string SecretManagerTypeName, Action<IServiceCollection, ISecretManagerType> configure)
    {
        CustomConfigurations[SecretManagerTypeName] = configure;
    }
}
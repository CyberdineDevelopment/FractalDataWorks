using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.SecretManagement.Abstractions;

namespace FractalDataWorks.Services.SecretManagement;

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
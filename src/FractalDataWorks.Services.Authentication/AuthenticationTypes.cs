using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// ServiceType collection for all authentication types.
/// The source generator will discover all AuthenticationTypeBase implementations.
/// </summary>
[ServiceTypeCollection("IAuthenticationType", "AuthenticationTypes")]
public static partial class AuthenticationTypes
{
    /// <summary>
    /// Registers all discovered authentication types with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void Register(IServiceCollection services)
    {
        // Register each discovered authentication type
        foreach (var authenticationType in All)
        {
            authenticationType.Register(services);
        }
    }

    /// <summary>
    /// Registers all discovered authentication types with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Custom configuration action for special cases.</param>
    public static void Register(IServiceCollection services, Action<AuthenticationRegistrationOptions> configure)
    {
        var options = new AuthenticationRegistrationOptions();
        configure(options);

        // Register each discovered authentication type with custom options
        foreach (var authenticationType in All)
        {
            authenticationType.Register(services);

            // Apply custom configuration if needed
            if (options.CustomConfigurations.TryGetValue(authenticationType.Name, out var customConfig))
            {
                customConfig(services, authenticationType);
            }
        }
    }
}

/// <summary>
/// Configuration options for authentication registration.
/// </summary>
public class AuthenticationRegistrationOptions
{
    /// <summary>
    /// Custom configurations for specific authentication types.
    /// </summary>
    public Dictionary<string, Action<IServiceCollection, IAuthenticationType>> CustomConfigurations { get; } = new();

    /// <summary>
    /// Configure a specific authentication type.
    /// </summary>
    /// <param name="authenticationTypeName">The name of the authentication type (e.g., "AzureEntra").</param>
    /// <param name="configure">Custom configuration action.</param>
    public void Configure(string authenticationTypeName, Action<IServiceCollection, IAuthenticationType> configure)
    {
        CustomConfigurations[authenticationTypeName] = configure;
    }
}
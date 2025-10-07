using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Authentication.Abstractions;

namespace FractalDataWorks.Services.Authentication;

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
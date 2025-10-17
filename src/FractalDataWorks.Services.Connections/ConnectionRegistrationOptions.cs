using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections;

/// <summary>
/// Configuration options for connection registration.
/// </summary>
public class ConnectionRegistrationOptions
{
    /// <summary>
    /// Custom configurations for specific connection types.
    /// </summary>
    public IDictionary<string, Action<IServiceCollection, IConnectionType>> CustomConfigurations { get; } = new Dictionary<string, Action<IServiceCollection, IConnectionType>>(StringComparer.Ordinal);

    /// <summary>
    /// Configure a specific connection type.
    /// </summary>
    /// <param name="connectionTypeName">The name of the connection type (e.g., "MsSql").</param>
    /// <param name="configure">Custom configuration action.</param>
    public void Configure(string connectionTypeName, Action<IServiceCollection, IConnectionType> configure)
    {
        CustomConfigurations[connectionTypeName] = configure;
    }
}
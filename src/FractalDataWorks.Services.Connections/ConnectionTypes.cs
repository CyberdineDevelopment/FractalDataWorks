using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections;

/// <summary>
/// ServiceType collection for all connection types.
/// The source generator will discover all ConnectionTypeBase implementations.
/// </summary>
[ServiceTypeCollection("IConnectionType", "ConnectionTypes")]
public static partial class ConnectionTypes
{
    /// <summary>
    /// Registers all discovered connection types with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void Register(IServiceCollection services)
    {
        // Register each discovered connection type
        foreach (var connectionType in All)
        {
            connectionType.Register(services);
        }
    }

    /// <summary>
    /// Registers all discovered connection types with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Custom configuration action for special cases.</param>
    public static void Register(IServiceCollection services, Action<ConnectionRegistrationOptions> configure)
    {
        var options = new ConnectionRegistrationOptions();
        configure(options);

        // Register each discovered connection type with custom options
        foreach (var connectionType in All)
        {
            connectionType.Register(services);

            // Apply custom configuration if needed
            if (options.CustomConfigurations.TryGetValue(connectionType.Name, out var customConfig))
            {
                customConfig(services, connectionType);
            }
        }
    }
}

/// <summary>
/// Configuration options for connection registration.
/// </summary>
public class ConnectionRegistrationOptions
{
    /// <summary>
    /// Custom configurations for specific connection types.
    /// </summary>
    public Dictionary<string, Action<IServiceCollection, IConnectionType>> CustomConfigurations { get; } = new();

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
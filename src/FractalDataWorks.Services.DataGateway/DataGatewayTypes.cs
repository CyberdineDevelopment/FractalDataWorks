using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.DataGateway.Abstractions;

namespace FractalDataWorks.Services.DataGateway;

/// <summary>
/// ServiceType collection for all data gateway types.
/// The source generator will discover all DataGatewayTypeBase implementations.
/// </summary>
[ServiceTypeCollection("IDataGatewayType", "DataGatewayTypes")]
public static partial class DataGatewayTypes
{
    /// <summary>
    /// Registers all discovered data gateway types with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void Register(IServiceCollection services)
    {
        // Register each discovered data gateway type
        foreach (var dataGatewayType in All)
        {
            dataGatewayType.Register(services);
        }
    }

    /// <summary>
    /// Registers all discovered data gateway types with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Custom configuration action for special cases.</param>
    public static void Register(IServiceCollection services, Action<DataGatewayRegistrationOptions> configure)
    {
        var options = new DataGatewayRegistrationOptions();
        configure(options);

        // Register each discovered data gateway type with custom options
        foreach (var dataGatewayType in All)
        {
            dataGatewayType.Register(services);

            // Apply custom configuration if needed
            if (options.CustomConfigurations.TryGetValue(dataGatewayType.Name, out var customConfig))
            {
                customConfig(services, dataGatewayType);
            }
        }
    }
}

/// <summary>
/// Configuration options for data gateway registration.
/// </summary>
public class DataGatewayRegistrationOptions
{
    /// <summary>
    /// Custom configurations for specific data gateway types.
    /// </summary>
    public Dictionary<string, Action<IServiceCollection, IDataGatewayType>> CustomConfigurations { get; } = new();

    /// <summary>
    /// Configure a specific data gateway type.
    /// </summary>
    /// <param name="dataGatewayTypeName">The name of the data gateway type (e.g., "Sql").</param>
    /// <param name="configure">Custom configuration action.</param>
    public void Configure(string dataGatewayTypeName, Action<IServiceCollection, IDataGatewayType> configure)
    {
        CustomConfigurations[dataGatewayTypeName] = configure;
    }
}
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.DataStores.Abstractions;

namespace FractalDataWorks.DataStores;

/// <summary>
/// ServiceType collection for all data store types.
/// The source generator will discover all DataStoreTypeBase implementations.
/// </summary>
[ServiceTypeCollection("IDataStoreType", "DataStoreTypes")]
public static partial class DataStoreTypes
{
    /// <summary>
    /// Registers all discovered data store types with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void Register(IServiceCollection services)
    {
        // Register each discovered data store type
        foreach (var dataStoreType in All)
        {
            dataStoreType.Register(services);
        }
    }

    /// <summary>
    /// Registers all discovered data store types with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Custom configuration action for special cases.</param>
    public static void Register(IServiceCollection services, Action<DataStoreRegistrationOptions> configure)
    {
        var options = new DataStoreRegistrationOptions();
        configure(options);

        // Register each discovered data store type with custom options
        foreach (var dataStoreType in All)
        {
            dataStoreType.Register(services);

            // Apply custom configuration if needed
            if (options.CustomConfigurations.TryGetValue(dataStoreType.Name, out var customConfig))
            {
                customConfig(services, dataStoreType);
            }
        }
    }
}

/// <summary>
/// Configuration options for data store registration.
/// </summary>
public class DataStoreRegistrationOptions
{
    /// <summary>
    /// Custom configurations for specific data store types.
    /// </summary>
    public Dictionary<string, Action<IServiceCollection, IDataStoreType>> CustomConfigurations { get; } = new();

    /// <summary>
    /// Configure a specific data store type.
    /// </summary>
    /// <param name="dataStoreTypeName">The name of the data store type (e.g., "SqlServer", "File").</param>
    /// <param name="configure">Custom configuration action.</param>
    public void Configure(string dataStoreTypeName, Action<IServiceCollection, IDataStoreType> configure)
    {
        CustomConfigurations[dataStoreTypeName] = configure;
    }
}
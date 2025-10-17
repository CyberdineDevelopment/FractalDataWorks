using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Data.DataStores.Abstractions;

namespace FractalDataWorks.Data.DataStores;

/// <summary>
/// Configuration options for data store registration.
/// </summary>
public sealed class DataStoreRegistrationOptions
{
    /// <summary>
    /// Custom configurations for specific data store types.
    /// </summary>
    public IDictionary<string, Action<IServiceCollection, IDataStoreType>> CustomConfigurations { get; } = new Dictionary<string, Action<IServiceCollection, IDataStoreType>>(StringComparer.Ordinal);

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
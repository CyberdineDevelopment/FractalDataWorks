using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Data.Abstractions;

namespace FractalDataWorks.Services.Data;

/// <summary>
/// Configuration options for data service registration.
/// </summary>
public class DataRegistrationOptions
{
    /// <summary>
    /// Custom configurations for specific data types.
    /// </summary>
    public Dictionary<string, Action<IServiceCollection, IDataType>> CustomConfigurations { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Configure a specific data type.
    /// </summary>
    /// <param name="dataTypeName">The name of the data type (e.g., "SqlServer").</param>
    /// <param name="configure">Custom configuration action.</param>
    public void Configure(string dataTypeName, Action<IServiceCollection, IDataType> configure)
    {
        CustomConfigurations[dataTypeName] = configure;
    }
}
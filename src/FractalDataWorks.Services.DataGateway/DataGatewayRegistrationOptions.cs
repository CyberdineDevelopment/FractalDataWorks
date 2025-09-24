using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.DataGateway.Abstractions;

namespace FractalDataWorks.Services.DataGateway;

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
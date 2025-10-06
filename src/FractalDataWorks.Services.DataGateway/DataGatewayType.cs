using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.DataGateway.Abstractions;

namespace FractalDataWorks.Services.DataGateway;

/// <summary>
/// ServiceType for the DataGateway service.
/// </summary>
[ServiceTypeOption(typeof(DataGatewayTypes), "DataGateway")]
public sealed class DataGatewayType : ServiceTypeBase<IDataGateway, IServiceFactory<IDataGateway, IDataGatewayConfiguration>, IDataGatewayConfiguration>
{
    private static readonly Guid ServiceId = new Guid("E8F5C3A7-9B2D-4E6F-A1C5-7D8E9F0B1C2D");

    /// <summary>
    /// Gets the singleton instance of the DataGateway service type.
    /// </summary>
    public static DataGatewayType Instance { get; } = new();

    private DataGatewayType()
        : base(
            id: ServiceId,
            name: "DataGateway",
            sectionName: "DataGateway",
            displayName: "Data Gateway",
            description: "Routes data commands to appropriate connections",
            category: "Data Access")
    {
    }

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        // Configuration handled by base class
    }

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IDataGateway, DataGatewayService>();
        services.AddScoped<IServiceFactory<IDataGateway, IDataGatewayConfiguration>, GenericServiceFactory<IDataGateway, IDataGatewayConfiguration>>();
    }
}

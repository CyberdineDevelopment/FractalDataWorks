using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services;
using FractalDataWorks.Services.DOMAIN_NAME.Abstractions.Configuration;
using FractalDataWorks.Services.DOMAIN_NAME.Abstractions.Providers;
using FractalDataWorks.Services.DOMAIN_NAME.Abstractions.ServiceTypes;

namespace NAMESPACE;

/// <summary>
/// ServiceType definition for SERVICETYPE_NAME DOMAIN_NAME service.
/// </summary>
[ServiceTypeOption(typeof(DOMAIN_NAMETypes), "SERVICETYPE_NAME")]
public sealed class SERVICETYPE_NAMEServiceType
    : DOMAIN_NAMETypeBase<IDOMAIN_NAMEService, IGenericServiceFactory<IDOMAIN_NAMEService, IDOMAIN_NAMEConfiguration>, IDOMAIN_NAMEConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SERVICETYPE_NAMEServiceType"/> class.
    /// </summary>
    public SERVICETYPE_NAMEServiceType()
        : base(
            id: 1, // TODO: Generate deterministic GUID from type name
            name: "SERVICETYPE_NAME",
            category: "DOMAIN_NAME Services")
    {
    }

    /// <inheritdoc/>
    public override int Priority => 50;

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        // Register the generic factory
        services.AddScoped<IGenericServiceFactory<IDOMAIN_NAMEService, IDOMAIN_NAMEConfiguration>,
            GenericServiceFactory<IDOMAIN_NAMEService, IDOMAIN_NAMEConfiguration>>();

        // Register the service
        services.AddScoped<IDOMAIN_NAMEService, SERVICETYPE_NAMEService>();

        // TODO: Register additional dependencies (translators, validators, etc.)
    }

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        // Configuration validation can be added here if needed
        // The configuration section is: Services:DOMAIN_NAME:SERVICETYPE_NAME
    }
}

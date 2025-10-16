using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services;
using FractalDataWorks.Services.DomainName.Abstractions.Configuration;
using FractalDataWorks.Services.DomainName.Abstractions.Providers;
using FractalDataWorks.Services.DomainName.Abstractions.ServiceTypes;

namespace FractalDataWorks.Services.DomainName.ServiceTypes;

/// <summary>
/// Default service type definition for DomainName services.
/// </summary>
[ServiceTypeOption(typeof(DomainNameTypes), "DefaultDomainName")]
public sealed class DefaultDomainNameServiceType
    : DomainNameTypeBase<IDomainNameService, IGenericServiceFactory<IDomainNameService, IDomainNameConfiguration>, IDomainNameConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultDomainNameServiceType"/> class.
    /// </summary>
    public DefaultDomainNameServiceType()
        : base(
            id: 1, // TODO: Generate deterministic GUID from type name
            name: "DefaultDomainName",
            category: "DomainName Services")
    {
    }

    /// <inheritdoc/>
    public override int Priority => 50;

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        // Register the generic factory
        services.AddScoped<IGenericServiceFactory<IDomainNameService, IDomainNameConfiguration>,
            GenericServiceFactory<IDomainNameService, IDomainNameConfiguration>>();

        // Register the DomainName service
        services.AddScoped<IDomainNameService, DefaultDomainNameService>();
    }

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        // Configuration validation can be added here if needed
        // The configuration section is: Services:DomainName:Default
    }
}

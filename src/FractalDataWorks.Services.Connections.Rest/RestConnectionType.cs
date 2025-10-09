using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.Rest;

/// <summary>
/// REST connection type for HTTP-based REST API connections.
/// Inherits from ServiceTypeBase for discovery by ServiceTypeCollectionGenerator.
/// </summary>
[ServiceTypeOption(typeof(ConnectionTypes), "Rest")]
public sealed class RestConnectionType : 
    ConnectionTypeBase<RestService, RestConnectionConfiguration, RestConnectionFactory>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestConnectionType"/> class.
    /// </summary>
    public RestConnectionType()
        : base(1, "REST", "Connections:REST", "REST API Connection", "REST API connection for HTTP-based REST endpoints", "HTTP Connection")
    {
    }

    /// <summary>
    /// Registers REST connection services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    public override void Register(IServiceCollection services)
    {
        services.AddHttpClient("RestService");
        services.AddScoped<RestService>();
        services.AddSingleton<RestConnectionFactory>();
    }

    /// <summary>
    /// Configures the REST connection type with the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration to use for REST connections.</param>
    public override void Configure(IConfiguration configuration)
    {
        var restConfig = configuration.GetSection(SectionName).Get<RestConnectionConfiguration>();

        // Validate configuration or store it for later use
        if (restConfig != null)
        {
            // Configuration validation logic here
            // Could store in a static field or configuration service
        }
    }
}
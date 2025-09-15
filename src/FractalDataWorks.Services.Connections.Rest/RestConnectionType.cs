using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.Rest;

/// <summary>
/// REST connection type for HTTP-based REST API connections.
/// Inherits from ServiceTypeBase for discovery by ServiceTypeCollectionGenerator.
/// </summary>
public sealed class RestConnectionType : 
    ConnectionTypeBase<RestService, RestConnectionConfiguration, RestConnectionFactory>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestConnectionType"/> class.
    /// </summary>
    public RestConnectionType() 
        : base(1, "REST", "Http")
    {
    }

    public override string SectionName => "Connections:REST";
    public override string DisplayName => "REST API Connection";
    public override string Description => "REST API connection for HTTP-based REST endpoints";

    public override void Register(IServiceCollection services)
    {
        services.AddHttpClient<RestService>();
        services.AddScoped<RestService>();
        services.AddSingleton<RestConnectionFactory>();
    }

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
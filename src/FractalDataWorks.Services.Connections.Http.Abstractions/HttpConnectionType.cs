using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.Http.Abstractions;

/// <summary>
/// Service type definition for HTTP connections.
/// Provides metadata and factory creation for HTTP-based connections.
/// </summary>
public sealed class HttpConnectionType : ConnectionTypeBase<IFdwConnection, IConnectionConfiguration, IConnectionFactory<IFdwConnection, IConnectionConfiguration>>
{
    /// <summary>
    /// Gets the singleton instance of the HTTP connection type.
    /// </summary>
    public static HttpConnectionType Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpConnectionType"/> class.
    /// </summary>
    private HttpConnectionType() : base(1, "Http", "HTTP Connections")
    {
    }

    /// <inheritdoc/>
    public override Type FactoryType => typeof(IConnectionFactory<IFdwConnection, IConnectionConfiguration>);

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        // Register HTTP specific services
        // services.AddScoped<IHttpConnectionFactory, HttpConnectionFactory>();
        // services.AddScoped<HttpService>();
    }

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        // HTTP specific configuration setup if needed
        // This could set up HTTP clients, timeouts, etc.
    }
}
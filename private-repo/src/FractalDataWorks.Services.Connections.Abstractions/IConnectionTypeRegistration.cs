using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Interface for connection types that support registration and configuration.
/// </summary>
public interface IConnectionTypeRegistration
{
    /// <summary>
    /// Registers the connection service and its dependencies with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    void Register(IServiceCollection services);

    /// <summary>
    /// Configures the connection service using the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration to use for setup.</param>
    void Configure(IConfiguration configuration);
}
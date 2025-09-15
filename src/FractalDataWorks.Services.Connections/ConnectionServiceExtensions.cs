using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections;

/// <summary>
/// Extension methods for registering connection services with dependency injection.
/// </summary>
public static class ConnectionServiceExtensions
{
    /// <summary>
    /// Registers the connection provider and all discovered connection types with the service collection.
    /// This method uses the generated ConnectionTypes static class to discover and register all connection types.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configuration">The configuration for setting up connection types.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConnectionProvider(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register all discovered connection types using the generated ConnectionTypes class
        // This will be populated by the ServiceTypeCollectionGenerator
        foreach (var connectionType in ConnectionTypes.All())
        {
            // Each connection type registers its own services (factories, services, etc.)
            connectionType.Register(services);
            
            // Each connection type configures itself using the provided configuration
            connectionType.Configure(configuration);
        }

        // Register the connection provider
        services.AddSingleton<IFdwConnectionProvider, FdwConnectionProvider>();

        return services;
    }

    /// <summary>
    /// Registers the connection provider without auto-discovering connection types.
    /// Use this method when you want to manually control which connection types are registered.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConnectionProviderOnly(this IServiceCollection services)
    {
        services.AddSingleton<IFdwConnectionProvider, FdwConnectionProvider>();
        return services;
    }
}
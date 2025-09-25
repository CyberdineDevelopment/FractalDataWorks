using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.DataStores.Abstractions;

/// <summary>
/// Represents a data store type definition in the service type architecture.
/// </summary>
public interface IDataStoreType : IServiceType
{
    /// <summary>
    /// Gets the section name for configuration in appsettings.json.
    /// </summary>
    string SectionName { get; }

    /// <summary>
    /// Gets the display name for this data store type.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the description of what this data store type provides.
    /// </summary>
    new string Description { get; }

    /// <summary>
    /// Registers the services required by this data store type with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    void Register(IServiceCollection services);

    /// <summary>
    /// Configures the data store type using the provided configuration.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    void Configure(IConfiguration configuration);
}
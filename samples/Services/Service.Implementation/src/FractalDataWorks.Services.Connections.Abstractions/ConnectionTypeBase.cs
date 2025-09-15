using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Basic connection type base for sample.
/// NOTE: In production, use ConnectionTypeBase from FractalDataWorks.Services.Connections.Abstractions
/// which inherits from ServiceTypeBase and implements proper ServiceType patterns.
/// </summary>
public abstract class ConnectionTypeBase<TService, TConfiguration, TFactory>
    where TService : class, IFdwConnection
    where TConfiguration : class, IConnectionConfiguration
    where TFactory : class, IConnectionFactory<TService, TConfiguration>
{
    /// <summary>
    /// Gets the unique identifier for this connection service type.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this connection service type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the category for this connection type.
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTypeBase{TService,TConfiguration,TFactory}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this connection service type.</param>
    /// <param name="name">The name of this connection service type.</param>
    /// <param name="category">The category for this connection type.</param>
    protected ConnectionTypeBase(int id, string name, string category)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Category = category ?? throw new ArgumentNullException(nameof(category));
    }

    /// <summary>
    /// Gets the factory type for creating connection service instances.
    /// </summary>
    public abstract Type FactoryType { get; }

    /// <summary>
    /// Registers the connection services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public abstract void Register(IServiceCollection services);

    /// <summary>
    /// Configures the connection services using the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public abstract void Configure(IConfiguration configuration);
}
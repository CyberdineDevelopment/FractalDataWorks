using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes.Attributes;

namespace FractalDataWorks.ServiceTypes;

/// <summary>
/// Base class for service type definitions that supports category-based hierarchical organization.
/// ServiceTypes define the contract and configuration for services in the plugin architecture.
/// The ServiceTypeCollectionGenerator will automatically discover all concrete types that inherit from ServiceTypeBase.
/// </summary>
public abstract class ServiceTypeBase
{
    /// <summary>
    /// Gets the unique identifier for this service type.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this service type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the category for this service type (e.g., "Database", "Cache", "Messaging").
    /// </summary>
    public virtual string Category { get; }

    /// <summary>
    /// Gets the service type interface or class that implementations must satisfy.
    /// </summary>
    [TypeLookup("GetByServiceType")]
    public abstract Type ServiceType { get; }

    /// <summary>
    /// Gets the configuration type used to configure this service.
    /// </summary>
    public abstract Type? ConfigurationType { get; }

    /// <summary>
    /// Gets the factory type for creating service instances.
    /// </summary>
    public abstract Type? FactoryType { get; }

    /// <summary>
    /// Gets the configuration section name for appsettings.json.
    /// </summary>
    public abstract string SectionName { get; }

    /// <summary>
    /// Gets the display name for this service type.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Gets the description of what this service type provides.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Registers the services required by this service type with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    public abstract void Register(IServiceCollection services);

    /// <summary>
    /// Configures the service type using the provided configuration.
    /// This method is called during application startup to allow the service type to validate
    /// or prepare its configuration.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public abstract void Configure(IConfiguration configuration);

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="category">The category of the service type (optional, defaults to "Default").</param>
    protected ServiceTypeBase(int id, string name, string? category = null)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Category = category ?? "Default";
    }
}

/// <summary>
/// Generic base class for service type definitions with specific service interface.
/// </summary>
/// <typeparam name="TService">The service interface type</typeparam>
public abstract class ServiceTypeBase<TService> : ServiceTypeBase
    where TService : class
{
    /// <summary>
    /// Gets the service type interface.
    /// </summary>
    public override Type ServiceType => typeof(TService);

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeBase{TService}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="category">The category of the service type (optional).</param>
    protected ServiceTypeBase(int id, string name, string? category = null) : base(id, name, category)
    {
    }
}

/// <summary>
/// Generic base class for service type definitions with specific service interface and configuration.
/// </summary>
/// <typeparam name="TService">The service interface type</typeparam>
/// <typeparam name="TConfiguration">The configuration type</typeparam>
public abstract class ServiceTypeBase<TService, TConfiguration> : ServiceTypeBase<TService>
    where TService : class
    where TConfiguration : class
{
    /// <summary>
    /// Gets the configuration type.
    /// </summary>
    public override Type ConfigurationType => typeof(TConfiguration);

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeBase{TService, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="category">The category of the service type (optional).</param>
    protected ServiceTypeBase(int id, string name, string? category = null) : base(id, name, category)
    {
    }
}

/// <summary>
/// Generic base class for service type definitions with specific service interface, configuration, and factory.
/// </summary>
/// <typeparam name="TService">The service interface type</typeparam>
/// <typeparam name="TConfiguration">The configuration type</typeparam>
/// <typeparam name="TFactory">The factory type</typeparam>
public abstract class ServiceTypeBase<TService, TConfiguration, TFactory> : ServiceTypeBase<TService, TConfiguration>
    where TService : class
    where TConfiguration : class
    where TFactory : class
{
    /// <summary>
    /// Gets the factory type.
    /// </summary>
    public override Type FactoryType => typeof(TFactory);

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeBase{TService, TConfiguration, TFactory}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="category">The category of the service type (optional).</param>
    protected ServiceTypeBase(int id, string name, string? category = null) : base(id, name, category)
    {
    }
}
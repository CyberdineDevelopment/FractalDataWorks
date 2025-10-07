using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.ServiceTypes.Attributes;

namespace FractalDataWorks.ServiceTypes;

/// <summary>
/// Base class for service type definitions that supports category-based hierarchical organization.
/// ServiceTypes define the contract and configuration for services in the plugin architecture.
/// The ServiceTypeCollectionGenerator will automatically discover all concrete types that inherit from ServiceTypeBase.
/// </summary>
/// <typeparam name="TService">The service interface type</typeparam>
/// <typeparam name="TConfiguration">The configuration type</typeparam>
/// <typeparam name="TFactory">The factory type</typeparam>
public abstract class ServiceTypeBase<TService, TFactory, TConfiguration> : ServiceTypeBase<TService, TFactory>, IServiceType<TService, TFactory>
    where TService : class, IGenericService
    where TConfiguration : class, IGenericConfiguration
    where TFactory : class, IServiceFactory<TService, TConfiguration>
{

    /// <summary>
    /// Gets the configuration type used to configure this service.
    /// </summary>
    [ServiceTypeLookup("FromConfigurationType")]
    public Type ConfigurationType => typeof(TConfiguration);

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeBase{TService, TConfiguration, TFactory}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="sectionName">The configuration section name for appsettings.json.</param>
    /// <param name="displayName">The display name for this service type.</param>
    /// <param name="description">The description of what this service type provides.</param>
    /// <param name="category">The category of the service type (optional, defaults to "Default").</param>
    protected ServiceTypeBase(
        int id,
        string name,
        string sectionName,
        string displayName,
        string description,
        string? category = null)
        : base(id, name,sectionName,displayName,description,category)
    {

    }
}

/// <summary>
/// Base class for service type definitions that supports category-based hierarchical organization.
/// ServiceTypes define the contract and configuration for services in the plugin architecture.
/// The ServiceTypeCollectionGenerator will automatically discover all concrete types that inherit from ServiceTypeBase.
/// </summary>
/// <typeparam name="TService">The service interface type</typeparam>
/// <typeparam name="TFactory">The factory type</typeparam>
public abstract class ServiceTypeBase<TService, TFactory> : EnumOptionBase<IServiceType>, IServiceType<TService, TFactory>
    where TService : class, IGenericService
    where TFactory : class, IServiceFactory<TService>
{
    /// <summary>
    /// Gets the unique identifier for this service type.
    /// </summary>
    [ServiceTypeLookup("Id")]
    public override int Id => base.Id;

    /// <summary>
    /// Gets the name of this service type.
    /// </summary>
    [ServiceTypeLookup("Name")]
    public override string Name => base.Name;

    /// <summary>
    /// Gets the category for this service type (e.g., "Database", "Cache", "Messaging").
    /// </summary>
    public virtual string Category { get; }

    /// <summary>
    /// Gets the service type interface or class that implementations must satisfy.
    /// </summary>
    [ServiceTypeLookup("ServiceType")]
    public Type ServiceType => typeof(TService);
    
    /// <summary>
    /// Gets the configuration section name for appsettings.json.
    /// </summary>
    public string SectionName { get; }

    /// <summary>
    /// Gets the display name for this service type.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the description of what this service type provides.
    /// </summary>
    public string Description { get; }

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
    /// Creates a factory instance for this service type.
    /// Returns the factory type that can create instances of the service.
    /// </summary>
    /// <returns>A result containing the factory type or failure information.</returns>
    public Type FactoryType => typeof(TFactory);
    

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeBase{TService, TConfiguration, TFactory}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="sectionName">The configuration section name for appsettings.json.</param>
    /// <param name="displayName">The display name for this service type.</param>
    /// <param name="description">The description of what this service type provides.</param>
    /// <param name="category">The category of the service type (optional, defaults to "Default").</param>
    protected ServiceTypeBase(
        int id,
        string name,
        string sectionName,
        string displayName,
        string description,
        string? category = null)
        : base(id, name)
    {
        SectionName = sectionName;
        DisplayName = displayName;
        Description = description;
        Category = category ?? "Default";
    }
}
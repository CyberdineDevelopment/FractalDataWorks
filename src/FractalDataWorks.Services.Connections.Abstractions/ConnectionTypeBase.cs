using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Results;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Base class for connection service type definitions that inherit from ServiceTypeBase.
/// Provides metadata and factory creation for connection services.
/// </summary>
/// <typeparam name="TService">The connection service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the connection service.</typeparam>
/// <typeparam name="TFactory">The factory type for creating connection service instances.</typeparam>
public abstract class ConnectionTypeBase<TService, TConfiguration, TFactory> : 
    ServiceTypeBase<TService, TFactory, TConfiguration>,
    IConnectionType<TService, TConfiguration, TFactory>,
    IConnectionType
    where TService : class, IGenericConnection
    where TConfiguration : class, IConnectionConfiguration
    where TFactory : class, IConnectionFactory<TService, TConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTypeBase{TService,TConfiguration,TFactory}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this connection service type.</param>
    /// <param name="name">The name of this connection service type.</param>
    /// <param name="sectionName">The configuration section name for appsettings.json.</param>
    /// <param name="displayName">The display name for this service type.</param>
    /// <param name="description">The description of what this service type provides.</param>
    /// <param name="category">The category for this connection type (defaults to "Connection").</param>
    protected ConnectionTypeBase(
        int id,
        string name,
        string sectionName,
        string displayName,
        string description,
        string? category = null)
        : base(id, name, sectionName, displayName, description, category ?? "Connection")
    {
    }


    /// <summary>
    /// Gets the factory type for creating connection service instances.
    /// </summary>
    /// <returns>The factory type.</returns>
    public Type Factory() => GenericResult<Type>.Success(typeof(TFactory));

    // NOTE: Container type and translator support will be added when those abstractions are ready
}


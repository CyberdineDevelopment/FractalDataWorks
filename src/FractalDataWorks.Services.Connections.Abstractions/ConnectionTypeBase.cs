using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    ServiceTypeBase<TService, TConfiguration, TFactory>,
    IConnectionType<TService, TConfiguration, TFactory>,
    IConnectionType
    where TService : class, IFdwConnection
    where TConfiguration : class, IConnectionConfiguration
    where TFactory : class, IConnectionFactory<TService, TConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTypeBase{TService,TConfiguration,TFactory}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this connection service type.</param>
    /// <param name="name">The name of this connection service type.</param>
    /// <param name="category">The category for this connection type (defaults to "Connection").</param>
    protected ConnectionTypeBase(
        int id, 
        string name, 
        string? category = null)
        : base(id, name, category ?? "Connection")
    {
    }

    // TODO: Add container type and translator support when those abstractions are ready
}


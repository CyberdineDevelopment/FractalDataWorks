using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Base collection class for connection service types.
/// Provides a foundation for generating high-performance collections of connection types
/// with FrozenDictionary support and factory methods.
/// </summary>
/// <typeparam name="TBase">The base connection type (e.g., ConnectionTypeBase).</typeparam>
/// <typeparam name="TGeneric">The generic connection type for lookups.</typeparam>
/// <typeparam name="TService">The connection service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for connection services.</typeparam>
/// <typeparam name="TFactory">The factory type for creating connection service instances.</typeparam>
public abstract partial class ConnectionTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory> :
    ServiceTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory>
    where TBase : ConnectionTypeBase<TService, TConfiguration, TFactory>
    where TGeneric : ConnectionTypeBase<TService, TConfiguration, TFactory>
    where TService : class, IFdwConnection
    where TConfiguration : class, IConnectionConfiguration
    where TFactory : class, IConnectionFactory<TService, TConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTypeCollectionBase{TBase,TGeneric,TService,TConfiguration,TFactory}"/> class.
    /// </summary>
    protected ConnectionTypeCollectionBase()
    {
        // The source generator will populate this collection with discovered connection types
    }
}
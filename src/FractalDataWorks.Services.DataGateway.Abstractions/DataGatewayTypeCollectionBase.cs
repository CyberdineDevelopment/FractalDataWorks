using System;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

/// <summary>
/// Base collection class for data provider service types.
/// Provides a foundation for generating high-performance collections of data provider types
/// with FrozenDictionary support and factory methods.
/// </summary>
/// <typeparam name="TBase">The base data provider type (e.g., DataGatewayTypeBase).</typeparam>
/// <typeparam name="TGeneric">The generic data provider type for lookups.</typeparam>
/// <typeparam name="TService">The data provider service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for data provider services.</typeparam>
/// <typeparam name="TFactory">The factory type for creating data provider service instances.</typeparam>
public abstract partial class DataGatewayTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory> :
    ServiceTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory>
    where TBase : DataGatewayTypeBase<TService, TConfiguration, TFactory>
    where TGeneric : DataGatewayTypeBase<TService, TConfiguration, TFactory>
    where TService : class, IDataService, IFdwService
    where TConfiguration : class, IDataGatewaysConfiguration, IFdwConfiguration
    where TFactory : class, IServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGatewayTypeCollectionBase{TBase,TGeneric,TService,TConfiguration,TFactory}"/> class.
    /// </summary>
    protected DataGatewayTypeCollectionBase()
    {
        // The source generator will populate this collection with discovered data provider types
    }
}
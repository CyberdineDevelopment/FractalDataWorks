using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Results;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.Services;

namespace FractalDataWorks.DataStores.Abstractions;

/// <summary>
/// Base class for data store type definitions that inherit from ServiceTypeBase.
/// Provides metadata and factory creation for data store services.
/// </summary>
/// <typeparam name="TService">The data store service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the data store service.</typeparam>
/// <typeparam name="TFactory">The factory type for creating data store service instances.</typeparam>
public abstract class DataStoreTypeBase<TService, TConfiguration, TFactory> :
    ServiceTypeBase<TService, TConfiguration, TFactory>,
    IDataStoreType
    where TService : class, IDataStore
    where TConfiguration : class
    where TFactory : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataStoreTypeBase{TService,TConfiguration,TFactory}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this data store service type.</param>
    /// <param name="name">The name of this data store service type.</param>
    /// <param name="sectionName">The configuration section name for appsettings.json.</param>
    /// <param name="displayName">The display name for this service type.</param>
    /// <param name="description">The description of what this service type provides.</param>
    /// <param name="category">The category for this data store type (defaults to "Data Store").</param>
    protected DataStoreTypeBase(
        int id,
        string name,
        string sectionName,
        string displayName,
        string description,
        string? category = null)
        : base(id, name, sectionName, displayName, description, category ?? "Data Store")
    {
    }

    /// <summary>
    /// Gets the factory type for creating data store service instances.
    /// </summary>
    /// <returns>The factory type.</returns>
    public IFdwResult<Type> Factory() => FdwResult<Type>.Success(typeof(TFactory));
}
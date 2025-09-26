using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.ServiceTypes;

/// <summary>
/// Base class for ServiceType collections with high-performance lookups and discovery capabilities.
/// The ServiceTypeCollectionGenerator will generate concrete implementations for classes that inherit from this base.
/// </summary>
/// <typeparam name="TBase">The base ServiceType class (e.g., ConnectionTypeBase)</typeparam>
/// <typeparam name="TGeneric">The generic form of the ServiceType for type-safe operations</typeparam>
/// <typeparam name="TService">The service interface type</typeparam>
/// <typeparam name="TConfiguration">The configuration type</typeparam>
/// <typeparam name="TFactory">The factory type</typeparam>
public abstract class ServiceTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory>
    where TBase : class, IServiceType<TService, TConfiguration, TFactory>
    where TGeneric : IServiceType<TService, TConfiguration, TFactory>
    where TService : class,IFdwService
    where TConfiguration : class, IFdwConfiguration
    where TFactory : class , IServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Gets all service types in this collection.
    /// This property will be populated by the source generator.
    /// </summary>
    public virtual IReadOnlyList<TBase> All => new List<TBase>();

    /// <summary>
    /// Gets an empty/default service type instance for fallback scenarios.
    /// This property will be populated by the source generator.
    /// </summary>
    public virtual TBase Empty => throw new NotSupportedException("Empty property must be implemented by source generator");

    /// <summary>
    /// Gets a service type by its unique identifier.
    /// This method will be implemented by the source generator with O(1) lookup performance.
    /// </summary>
    /// <param name="id">The unique identifier to search for.</param>
    /// <returns>The service type with the specified ID, or Empty if not found.</returns>
    public virtual TBase GetById(int id) => Empty;

    /// <summary>
    /// Gets a service type by its name.
    /// This method will be implemented by the source generator with O(1) lookup performance.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>The service type with the specified name, or Empty if not found.</returns>
    public virtual TBase GetByName(string name) => Empty;

    /// <summary>
    /// Gets all service types that match the specified service type.
    /// This method will be implemented by the source generator.
    /// </summary>
    /// <param name="serviceType">The service type to filter by.</param>
    /// <returns>Collection of service types that match the service type.</returns>
    public virtual IEnumerable<TBase> GetByServiceType(Type serviceType) => Enumerable.Empty<TBase>();

    /// <summary>
    /// Gets all service types that match the specified configuration type.
    /// This method will be implemented by the source generator.
    /// </summary>
    /// <param name="configurationType">The configuration type to filter by.</param>
    /// <returns>Collection of service types that match the configuration type.</returns>
    public virtual IEnumerable<TBase> GetByConfigurationType(Type configurationType) => Enumerable.Empty<TBase>();

    /// <summary>
    /// Gets all service types that match the specified section name.
    /// This method will be implemented by the source generator.
    /// </summary>
    /// <param name="sectionName">The section name to filter by.</param>
    /// <returns>Collection of service types that match the section name.</returns>
    public virtual IEnumerable<TBase> GetBySectionName(string sectionName) => Enumerable.Empty<TBase>();
}
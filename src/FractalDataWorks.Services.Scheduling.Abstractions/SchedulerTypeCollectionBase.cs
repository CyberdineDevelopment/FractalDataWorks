using System;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Base collection class for scheduler service types.
/// Provides a foundation for generating high-performance collections of scheduler types
/// with FrozenDictionary support and factory methods.
/// </summary>
/// <typeparam name="TBase">The base scheduler type (e.g., SchedulerTypeBase).</typeparam>
/// <typeparam name="TGeneric">The generic scheduler type for lookups.</typeparam>
/// <typeparam name="TService">The scheduler service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for scheduler services.</typeparam>
/// <typeparam name="TFactory">The factory type for creating scheduler service instances.</typeparam>
public abstract partial class SchedulerTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory> :
    ServiceTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory>
    where TBase : SchedulerTypeBase<TService, TConfiguration, TFactory>
    where TGeneric : SchedulerTypeBase<TService, TConfiguration, TFactory>
    where TService : class, IFdwSchedulingService
    where TConfiguration : class, ISchedulingConfiguration
    where TFactory : class, IServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerTypeCollectionBase{TBase,TGeneric,TService,TConfiguration,TFactory}"/> class.
    /// </summary>
    protected SchedulerTypeCollectionBase()
    {
        // The source generator will populate this collection with discovered scheduler types
    }
}
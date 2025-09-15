using System;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Transformations.Abstractions;

/// <summary>
/// Base collection class for transformation service types.
/// Provides a foundation for generating high-performance collections of transformation types
/// with FrozenDictionary support and factory methods.
/// </summary>
/// <typeparam name="TBase">The base transformation type (e.g., TransformationTypeBase).</typeparam>
/// <typeparam name="TGeneric">The generic transformation type for lookups.</typeparam>
/// <typeparam name="TService">The transformation service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for transformation services.</typeparam>
/// <typeparam name="TFactory">The factory type for creating transformation service instances.</typeparam>
public abstract partial class TransformationTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory> :
    ServiceTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory>
    where TBase : TransformationTypeBase<TService, TConfiguration, TFactory>
    where TGeneric : TransformationTypeBase<TService, TConfiguration, TFactory>
    where TService : class, ITransformationProvider
    where TConfiguration : class, ITransformationsConfiguration
    where TFactory : class, IServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransformationTypeCollectionBase{TBase,TGeneric,TService,TConfiguration,TFactory}"/> class.
    /// </summary>
    protected TransformationTypeCollectionBase()
    {
        // The source generator will populate this collection with discovered transformation types
    }
}
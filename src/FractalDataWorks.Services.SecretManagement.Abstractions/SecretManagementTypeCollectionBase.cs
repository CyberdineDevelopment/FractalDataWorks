using System;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Base collection class for secret management service types.
/// Provides a foundation for generating high-performance collections of secret management types
/// with FrozenDictionary support and factory methods.
/// </summary>
/// <typeparam name="TBase">The base secret management type (e.g., SecretManagementTypeBase).</typeparam>
/// <typeparam name="TGeneric">The generic secret management type for lookups.</typeparam>
/// <typeparam name="TService">The secret management service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for secret management services.</typeparam>
/// <typeparam name="TFactory">The factory type for creating secret management service instances.</typeparam>
public abstract partial class SecretManagementTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory> :
    ServiceTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory>
    where TBase : SecretManagementTypeBase<TService, TConfiguration, TFactory>
    where TGeneric : SecretManagementTypeBase<TService, TConfiguration, TFactory>
    where TService : class, ISecretService
    where TConfiguration : class, ISecretManagementConfiguration
    where TFactory : class, ISecretServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretManagementTypeCollectionBase{TBase,TGeneric,TService,TConfiguration,TFactory}"/> class.
    /// </summary>
    protected SecretManagementTypeCollectionBase()
    {
        // The source generator will populate this collection with discovered secret management types
    }
}
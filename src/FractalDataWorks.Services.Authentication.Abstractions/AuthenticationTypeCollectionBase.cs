using System;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.Services.Authentication.Abstractions.Security;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Base collection class for authentication service types.
/// Provides a foundation for generating high-performance collections of authentication types
/// with FrozenDictionary support and factory methods.
/// </summary>
/// <typeparam name="TBase">The base authentication type (e.g., AuthenticationTypeBase).</typeparam>
/// <typeparam name="TGeneric">The generic authentication type for lookups.</typeparam>
/// <typeparam name="TService">The authentication service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for authentication services.</typeparam>
/// <typeparam name="TFactory">The factory type for creating authentication service instances.</typeparam>
public abstract partial class AuthenticationTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory> :
    ServiceTypeCollectionBase<TBase, TGeneric, TService, TConfiguration, TFactory>
    where TBase : AuthenticationTypeBase<TService, TConfiguration, TFactory>
    where TGeneric : AuthenticationTypeBase<TService, TConfiguration, TFactory>
    where TService : class, IAuthenticationService
    where TConfiguration : class, IAuthenticationConfiguration
    where TFactory : class, IAuthenticationServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationTypeCollectionBase{TBase,TGeneric,TService,TConfiguration,TFactory}"/> class.
    /// </summary>
    protected AuthenticationTypeCollectionBase()
    {
        // The source generator will populate this collection with discovered authentication types
    }
}
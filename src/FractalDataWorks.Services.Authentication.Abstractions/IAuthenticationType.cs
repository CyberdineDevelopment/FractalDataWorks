using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Interface for authentication service types.
/// Defines the contract for authentication service type implementations that integrate
/// with the service framework's dependency injection and configuration systems.
/// </summary>
/// <typeparam name="TService">The authentication service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the authentication service.</typeparam>
/// <typeparam name="TFactory">The factory type for creating authentication service instances.</typeparam>
public interface IAuthenticationType<TService, TConfiguration, TFactory> : IServiceType<TService, TConfiguration, TFactory>
    where TService : class, IAuthenticationService
    where TConfiguration : class, IAuthenticationConfiguration
    where TFactory : class, IAuthenticationServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Gets the authentication provider name (e.g., "Microsoft Entra ID", "Auth0").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the authentication method used by this provider.
    /// </summary>
    AuthenticationMethod Method { get; }

    /// <summary>
    /// Gets whether this authentication type supports multi-tenant scenarios.
    /// </summary>
    bool SupportsMultiTenant { get; }

    /// <summary>
    /// Gets whether this authentication type supports token caching.
    /// </summary>
    bool SupportsTokenCaching { get; }

    /// <summary>
    /// Gets whether this authentication type supports token refresh.
    /// </summary>
    bool SupportsTokenRefresh { get; }

    /// <summary>
    /// Gets the priority of this authentication type (higher values = higher priority).
    /// </summary>
    int Priority { get; }
}

/// <summary>
/// Non-generic interface for authentication service types.
/// Provides a common base for all authentication types regardless of generic parameters.
/// </summary>
public interface IAuthenticationType : IServiceType
{
    /// <summary>
    /// Gets the authentication provider name (e.g., "Microsoft Entra ID", "Auth0").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the authentication method used by this provider.
    /// </summary>
    AuthenticationMethod Method { get; }

    /// <summary>
    /// Gets whether this authentication type supports multi-tenant scenarios.
    /// </summary>
    bool SupportsMultiTenant { get; }

    /// <summary>
    /// Gets whether this authentication type supports token caching.
    /// </summary>
    bool SupportsTokenCaching { get; }

    /// <summary>
    /// Gets whether this authentication type supports token refresh.
    /// </summary>
    bool SupportsTokenRefresh { get; }

    /// <summary>
    /// Gets the priority of this authentication type (higher values = higher priority).
    /// </summary>
    int Priority { get; }
}
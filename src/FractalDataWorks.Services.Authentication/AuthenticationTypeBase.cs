using System;
using System.Collections.Generic;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.Services.Authentication.Abstractions.Security;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Base class for authentication service type definitions.
/// Provides authentication-specific metadata and capabilities.
/// </summary>
/// <typeparam name="TService">The authentication service type.</typeparam>
/// <typeparam name="TConfiguration">The authentication configuration type.</typeparam>
/// <typeparam name="TFactory">The factory type for creating authentication service instances.</typeparam>
/// <remarks>
/// Authentication types should inherit from this class and provide metadata only - 
/// no instantiation logic should be included (that belongs in factories).
/// The ServiceTypeCollectionGenerator will discover all types inheriting from this base.
/// </remarks>
public abstract class AuthenticationTypeBase<TService, TConfiguration, TFactory> : 
    ServiceTypeBase<TService, TConfiguration, TFactory>,
    IAuthenticationType<TService, TConfiguration, TFactory>,
    IAuthenticationType
    where TService : class, IAuthenticationService
    where TConfiguration : class, IAuthenticationConfiguration
    where TFactory : class, IAuthenticationServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Gets the authentication provider name.
    /// </summary>
    /// <remarks>
    /// The provider name identifies the underlying authentication system
    /// (e.g., "Microsoft Entra", "Auth0", "Okta", "Local").
    /// </remarks>
    public string ProviderName { get; }

    /// <summary>
    /// Gets the authentication method used by this provider.
    /// </summary>
    /// <remarks>
    /// Defines the primary authentication mechanism such as OAuth2, SAML, JWT, etc.
    /// This helps consumers understand the authentication flow they can expect.
    /// </remarks>
    public AuthenticationMethod Method { get; }

    /// <summary>
    /// Gets the supported authentication protocols.
    /// </summary>
    /// <remarks>
    /// Lists the specific protocols this authentication type supports
    /// (e.g., "OAuth 2.0", "OpenID Connect", "SAML 2.0").
    /// </remarks>
    public IReadOnlyList<string> SupportedProtocols { get; }

    /// <summary>
    /// Gets the supported authentication flows.
    /// </summary>
    /// <remarks>
    /// Defines which OAuth2/OpenID Connect flows are supported
    /// (e.g., "authorization_code", "client_credentials", "implicit").
    /// </remarks>
    public IReadOnlyList<string> SupportedFlows { get; }

    /// <summary>
    /// Gets the supported token types.
    /// </summary>
    /// <remarks>
    /// Specifies which token formats this provider can handle
    /// (e.g., "JWT", "Reference Token", "SAML Assertion").
    /// </remarks>
    public IReadOnlyList<string> SupportedTokenTypes { get; }

    /// <summary>
    /// Gets whether this provider supports multi-tenant scenarios.
    /// </summary>
    /// <remarks>
    /// Multi-tenant support allows a single authentication configuration
    /// to handle users from multiple organizations or directories.
    /// </remarks>
    public bool SupportsMultiTenant { get; }

    /// <summary>
    /// Gets whether this provider supports token caching.
    /// </summary>
    /// <remarks>
    /// Token caching improves performance by storing valid tokens
    /// and reusing them until expiration, reducing authentication calls.
    /// </remarks>
    public bool SupportsTokenCaching { get; }

    /// <summary>
    /// Gets whether this provider supports token refresh.
    /// </summary>
    /// <remarks>
    /// Token refresh allows obtaining new access tokens using refresh tokens
    /// without requiring user re-authentication.
    /// </remarks>
    public bool SupportsTokenRefresh { get; }

    /// <summary>
    /// Gets the priority for provider selection when multiple providers are available.
    /// </summary>
    /// <remarks>
    /// Higher values indicate higher priority. When multiple authentication
    /// providers are configured, the system can use this to determine
    /// the preferred provider for automatic selection.
    /// </remarks>
    public virtual int Priority => 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationTypeBase{TService, TConfiguration, TFactory}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the authentication type.</param>
    /// <param name="name">The name of the authentication type.</param>
    /// <param name="providerName">The authentication provider name.</param>
    /// <param name="method">The authentication method used by this provider.</param>
    /// <param name="supportedProtocols">The supported authentication protocols.</param>
    /// <param name="supportedFlows">The supported authentication flows.</param>
    /// <param name="supportedTokenTypes">The supported token types.</param>
    /// <param name="supportsMultiTenant">Whether this provider supports multi-tenant scenarios.</param>
    /// <param name="supportsTokenCaching">Whether this provider supports token caching.</param>
    /// <param name="supportsTokenRefresh">Whether this provider supports token refresh.</param>
    /// <param name="category">The category for this authentication type (defaults to "Authentication").</param>
    protected AuthenticationTypeBase(
        int id,
        string name,
        string providerName,
        AuthenticationMethod method,
        IReadOnlyList<string> supportedProtocols,
        IReadOnlyList<string> supportedFlows,
        IReadOnlyList<string> supportedTokenTypes,
        bool supportsMultiTenant,
        bool supportsTokenCaching,
        bool supportsTokenRefresh,
        string? category = null)
        : base(id, name, category ?? "Authentication")
    {
        ProviderName = providerName ?? throw new ArgumentNullException(nameof(providerName));
        Method = method ?? throw new ArgumentNullException(nameof(method));
        SupportedProtocols = supportedProtocols ?? throw new ArgumentNullException(nameof(supportedProtocols));
        SupportedFlows = supportedFlows ?? throw new ArgumentNullException(nameof(supportedFlows));
        SupportedTokenTypes = supportedTokenTypes ?? throw new ArgumentNullException(nameof(supportedTokenTypes));
        SupportsMultiTenant = supportsMultiTenant;
        SupportsTokenCaching = supportsTokenCaching;
        SupportsTokenRefresh = supportsTokenRefresh;
    }
}
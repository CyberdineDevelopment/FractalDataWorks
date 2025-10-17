using System;
using FractalDataWorks.Services.Authentication.Abstractions;

namespace FractalDataWorks.Services.Authentication.Auth0.Configuration;

/// <summary>
/// Configuration for Auth0 authentication.
/// </summary>
public sealed class Auth0Configuration : IAuthenticationConfiguration
{
    /// <inheritdoc/>
    public int Id { get; set; }

    /// <inheritdoc/>
    public string Name { get; set; } = "Auth0";

    /// <inheritdoc/>
    public string SectionName { get; set; } = "Services:Authentication:Auth0";

    /// <inheritdoc/>
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc/>
    public string AuthenticationType { get; set; } = "Auth0";

    /// <inheritdoc/>
    public string ClientId { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string Authority { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string RedirectUri { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string[] Scopes { get; set; } = [];

    /// <inheritdoc/>
    public bool EnableTokenCaching { get; set; } = true;

    /// <inheritdoc/>
    public int TokenCacheLifetimeMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the Auth0 domain.
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Auth0 client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Auth0 audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Auth0 API identifier.
    /// </summary>
    public string ApiIdentifier { get; set; } = string.Empty;
}

using System;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;

namespace FractalDataWorks.Services.Authentication.Auth0;

/// <summary>
/// Represents an Auth0 authentication type.
/// </summary>
public sealed class Auth0AuthenticationType : AuthenticationType, IEnumOption<Auth0AuthenticationType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Auth0AuthenticationType"/> class.
    /// </summary>
    public Auth0AuthenticationType()
        : base(
            id: 1,
            name: "Auth0",
            providerName: "Auth0",
            method: new OAuth2AuthenticationMethod())
    {
    }

    /// <summary>
    /// Gets the Auth0 domain.
    /// </summary>
    public string Domain { get; init; } = string.Empty;

    /// <summary>
    /// Gets the Auth0 client ID.
    /// </summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the Auth0 audience.
    /// </summary>
    public string Audience { get; init; } = string.Empty;
}

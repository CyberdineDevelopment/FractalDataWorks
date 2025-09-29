using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// OAuth 2.0 authentication method.
/// ExtendedEnum that wraps Microsoft's OAuth2 authentication with framework behaviors.
/// </summary>
[TypeOption(typeof(AuthenticationMethods), "OAuth2")]
public sealed class OAuth2AuthenticationMethod : AuthenticationMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OAuth2AuthenticationMethod"/> class.
    /// </summary>
    public OAuth2AuthenticationMethod() : base(
        id: 1,
        name: "OAuth2",
        requiresUserInteraction: true,
        supportsTokenRefresh: true,
        supportsMultiTenant: true,
        authenticationScheme: "Bearer",
        priority: 90)
    {
    }
}
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// OAuth2 authentication method implementation.
/// </summary>
[EnumOption(typeof(AuthenticationMethod), "OAuth2")]
public sealed class OAuth2AuthenticationMethod : AuthenticationMethodBase
{
    /// <summary>
    /// Initializes a new instance of the OAuth2 authentication method.
    /// </summary>
    public OAuth2AuthenticationMethod() : base(
        id: 1,
        name: "OAuth2",
        requiresUserInteraction: true,
        supportsTokenRefresh: true,
        supportsMultiTenant: true,
        authenticationScheme: "Bearer",
        priority: 100)
    {
    }
}
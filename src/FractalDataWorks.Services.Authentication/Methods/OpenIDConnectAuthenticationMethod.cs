using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// OpenID Connect authentication method.
/// </summary>
[TypeOption(typeof(AuthenticationMethods), "OpenIDConnect")]
public sealed class OpenIDConnectAuthenticationMethod : AuthenticationMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenIDConnectAuthenticationMethod"/> class.
    /// </summary>
    public OpenIDConnectAuthenticationMethod() : base(
        id: 7,
        name: "OpenIDConnect",
        requiresUserInteraction: true,
        supportsTokenRefresh: true,
        supportsMultiTenant: true,
        authenticationScheme: "Bearer",
        priority: 90)
    {
    }
}

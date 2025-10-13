using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// Bearer token authentication method.
/// </summary>
[TypeOption(typeof(AuthenticationMethods), "BearerToken")]
public sealed class BearerTokenAuthenticationMethod : AuthenticationMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BearerTokenAuthenticationMethod"/> class.
    /// </summary>
    public BearerTokenAuthenticationMethod() : base(
        id: 3,
        name: "BearerToken",
        requiresUserInteraction: false,
        supportsTokenRefresh: true,
        supportsMultiTenant: true,
        authenticationScheme: "Bearer",
        priority: 80)
    {
    }
}

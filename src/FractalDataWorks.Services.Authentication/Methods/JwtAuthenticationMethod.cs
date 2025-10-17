using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// JWT (JSON Web Token) authentication method.
/// </summary>
[TypeOption(typeof(AuthenticationMethods), "JWT")]
public sealed class JwtAuthenticationMethod : AuthenticationMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JwtAuthenticationMethod"/> class.
    /// </summary>
    public JwtAuthenticationMethod() : base(
        id: 2,
        name: "JWT",
        requiresUserInteraction: false,
        supportsTokenRefresh: false,
        supportsMultiTenant: true,
        authenticationScheme: "Bearer",
        priority: 85)
    {
    }
}

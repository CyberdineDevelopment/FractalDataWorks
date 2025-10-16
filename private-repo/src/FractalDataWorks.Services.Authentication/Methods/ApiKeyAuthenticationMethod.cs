using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// API Key authentication method.
/// </summary>
[TypeOption(typeof(AuthenticationMethods), "ApiKey")]
public sealed class ApiKeyAuthenticationMethod : AuthenticationMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyAuthenticationMethod"/> class.
    /// </summary>
    public ApiKeyAuthenticationMethod() : base(
        id: 4,
        name: "ApiKey",
        requiresUserInteraction: false,
        supportsTokenRefresh: false,
        supportsMultiTenant: false,
        authenticationScheme: "ApiKey",
        priority: 70)
    {
    }
}

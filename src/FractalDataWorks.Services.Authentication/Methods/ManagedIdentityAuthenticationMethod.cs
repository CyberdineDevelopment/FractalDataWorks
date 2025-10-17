using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// Managed Identity authentication method for Azure resources.
/// </summary>
[TypeOption(typeof(AuthenticationMethods), "ManagedIdentity")]
public sealed class ManagedIdentityAuthenticationMethod : AuthenticationMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedIdentityAuthenticationMethod"/> class.
    /// </summary>
    public ManagedIdentityAuthenticationMethod() : base(
        id: 6,
        name: "ManagedIdentity",
        requiresUserInteraction: false,
        supportsTokenRefresh: true,
        supportsMultiTenant: true,
        authenticationScheme: "Bearer",
        priority: 100)
    {
    }
}

using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// Certificate-based authentication method.
/// </summary>
[TypeOption(typeof(AuthenticationMethods), "Certificate")]
public sealed class CertificateAuthenticationMethod : AuthenticationMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CertificateAuthenticationMethod"/> class.
    /// </summary>
    public CertificateAuthenticationMethod() : base(
        id: 5,
        name: "Certificate",
        requiresUserInteraction: false,
        supportsTokenRefresh: false,
        supportsMultiTenant: true,
        authenticationScheme: "Certificate",
        priority: 95)
    {
    }
}

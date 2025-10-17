using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// OpenID Connect authentication protocol.
/// </summary>
[TypeOption(typeof(AuthenticationProtocols), "OpenIDConnect")]
public sealed class OpenIDConnectProtocol : AuthenticationProtocolBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenIDConnectProtocol"/> class.
    /// </summary>
    public OpenIDConnectProtocol() : base(
        id: 2,
        name: "OpenIDConnect",
        version: "1.0",
        requiresSecureTransport: true,
        supportsTokens: true)
    {
    }
}

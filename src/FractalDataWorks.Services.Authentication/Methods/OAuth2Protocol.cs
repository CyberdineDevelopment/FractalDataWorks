using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// OAuth 2.0 authentication protocol.
/// </summary>
[TypeOption(typeof(AuthenticationProtocols), "OAuth2")]
public sealed class OAuth2Protocol : AuthenticationProtocolBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OAuth2Protocol"/> class.
    /// </summary>
    public OAuth2Protocol() : base(
        id: 1,
        name: "OAuth2",
        version: "2.0",
        requiresSecureTransport: true,
        supportsTokens: true)
    {
    }
}

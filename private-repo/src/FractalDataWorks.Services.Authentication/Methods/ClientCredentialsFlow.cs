using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// OAuth 2.0 Client Credentials flow.
/// </summary>
[TypeOption(typeof(AuthenticationFlows), "ClientCredentials")]
public sealed class ClientCredentialsFlow : AuthenticationFlowBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientCredentialsFlow"/> class.
    /// </summary>
    public ClientCredentialsFlow() : base(
        id: 2,
        name: "ClientCredentials",
        requiresUserInteraction: false,
        supportsRefreshTokens: false,
        isServerToServer: true)
    {
    }
}

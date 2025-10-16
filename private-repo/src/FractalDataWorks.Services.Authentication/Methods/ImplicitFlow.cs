using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// OAuth 2.0 Implicit flow (deprecated but still supported).
/// </summary>
[TypeOption(typeof(AuthenticationFlows), "Implicit")]
public sealed class ImplicitFlow : AuthenticationFlowBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImplicitFlow"/> class.
    /// </summary>
    public ImplicitFlow() : base(
        id: 5,
        name: "Implicit",
        requiresUserInteraction: true,
        supportsRefreshTokens: false,
        isServerToServer: false)
    {
    }
}

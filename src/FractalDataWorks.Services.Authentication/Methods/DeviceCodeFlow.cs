using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// OAuth 2.0 Device Code flow.
/// </summary>
[TypeOption(typeof(AuthenticationFlows), "DeviceCode")]
public sealed class DeviceCodeFlow : AuthenticationFlowBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceCodeFlow"/> class.
    /// </summary>
    public DeviceCodeFlow() : base(
        id: 4,
        name: "DeviceCode",
        requiresUserInteraction: true,
        supportsRefreshTokens: true,
        isServerToServer: false)
    {
    }
}

using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Security;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// Bearer token type for HTTP Authorization header.
/// </summary>
[TypeOption(typeof(TokenTypes), "BearerToken")]
public sealed class BearerTokenType : TokenTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BearerTokenType"/> class.
    /// </summary>
    public BearerTokenType() : base(
        id: 4,
        name: "BearerToken",
        format: "JWT",
        canBeRefreshed: false,
        containsUserIdentity: true,
        typicalLifetimeSeconds: 3600) // 1 hour
    {
    }
}

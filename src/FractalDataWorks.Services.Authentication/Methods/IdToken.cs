using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Security;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// OpenID Connect ID Token type.
/// </summary>
[TypeOption(typeof(TokenTypes), "IdToken")]
public sealed class IdToken : TokenTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdToken"/> class.
    /// </summary>
    public IdToken() : base(
        id: 2,
        name: "IdToken",
        format: "JWT",
        canBeRefreshed: false,
        containsUserIdentity: true,
        typicalLifetimeSeconds: 3600) // 1 hour
    {
    }
}

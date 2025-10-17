using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Security;

/// <summary>
/// OAuth 2.0 authentication.
/// </summary>
[TypeOption(typeof(SecurityMethods), "OAuth2")]
public sealed class OAuth2 : SecurityMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OAuth2"/> class.
    /// </summary>
    public OAuth2() : base(4, "OAuth2", requiresAuthentication: true, authenticationScheme: "Bearer", supportsTokenRefresh: true) { }
}

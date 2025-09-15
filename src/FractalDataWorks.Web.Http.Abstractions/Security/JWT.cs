using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Security;

/// <summary>
/// JSON Web Token authentication.
/// </summary>
[EnumOption("JWT")]
public sealed class JWT : SecurityMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JWT"/> class.
    /// </summary>
    public JWT() : base(2, "JWT", requiresAuthentication: true, authenticationScheme: "Bearer", supportsTokenRefresh: true) { }
}

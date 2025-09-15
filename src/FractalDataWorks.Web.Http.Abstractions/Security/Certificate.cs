using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Security;

/// <summary>
/// Client certificate authentication.
/// </summary>
[EnumOption("Certificate")]
public sealed class Certificate : SecurityMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Certificate"/> class.
    /// </summary>
    public Certificate() : base(5, "Certificate", requiresAuthentication: true, authenticationScheme: "Certificate", supportsTokenRefresh: false) { }
}

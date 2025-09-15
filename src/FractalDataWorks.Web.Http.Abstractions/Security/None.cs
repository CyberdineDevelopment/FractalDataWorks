using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Security;

/// <summary>
/// No authentication required.
/// </summary>
[EnumOption("None")]
public sealed class None : SecurityMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    public None() : base(1, "None", requiresAuthentication: false, authenticationScheme: null, supportsTokenRefresh: false) { }
}

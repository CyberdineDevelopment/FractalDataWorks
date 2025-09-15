using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Security;

/// <summary>
/// API key-based authentication.
/// </summary>
[EnumOption("ApiKey")]
public sealed class ApiKey : SecurityMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKey"/> class.
    /// </summary>
    public ApiKey() : base(3, "ApiKey", requiresAuthentication: true, authenticationScheme: "ApiKey", supportsTokenRefresh: false) { }
}

using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// Form-based authentication method.
/// ExtendedEnum that wraps Microsoft's form-based authentication with framework behaviors.
/// </summary>
[TypeOption(typeof(AuthenticationMethods), "FormBased")]
public sealed class FormBasedAuthenticationMethod : AuthenticationMethodBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormBasedAuthenticationMethod"/> class.
    /// </summary>
    public FormBasedAuthenticationMethod() : base(
        id: 2,
        name: "FormBased",
        requiresUserInteraction: true,
        supportsTokenRefresh: false,
        supportsMultiTenant: false,
        authenticationScheme: "Cookies",
        priority: 30)
    {
    }
}
using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Web.Http.Abstractions.Security;

/// <summary>
/// Interface for security method enhanced enums.
/// Provides abstraction for security authentication methods.
/// </summary>
public interface ISecurityMethod : IEnumOption
{
    /// <summary>
    /// Gets a value indicating whether this security method requires authentication.
    /// </summary>
    bool RequiresAuthentication { get; }
    
    /// <summary>
    /// Gets the authentication scheme name used by this security method.
    /// </summary>
    string? AuthenticationScheme { get; }
    
    /// <summary>
    /// Gets a value indicating whether this security method supports token refresh.
    /// </summary>
    bool SupportsTokenRefresh { get; }
}

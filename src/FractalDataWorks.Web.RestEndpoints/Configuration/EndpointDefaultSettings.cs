using FractalDataWorks.Web.Http.Abstractions.Policies;
using FractalDataWorks.Web.Http.Abstractions.Security;

namespace FractalDataWorks.Web.RestEndpoints.Configuration;

/// <summary>
/// Default settings for a specific endpoint type.
/// Following Enhanced Enum pattern: store names, retrieve objects via ByName().
/// </summary>
public record EndpointDefaultSettings(
    string SecurityMethodName,
    string RateLimitPolicyName,
    int TimeoutMs,
    long MaxBodySize,
    bool RequireAuthentication,
    string[] AllowedRoles)
{
    /// <summary>
    /// Gets the security method Enhanced Enum by name.
    /// </summary>
    public ISecurityMethod SecurityMethod => SecurityMethods.ByName(SecurityMethodName);
    
    /// <summary>
    /// Gets the rate limit policy Enhanced Enum by name.
    /// </summary>
    public IRateLimitPolicy RateLimitPolicy => RateLimitPolicies.ByName(RateLimitPolicyName);
};
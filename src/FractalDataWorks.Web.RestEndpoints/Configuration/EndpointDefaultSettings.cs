using FractalDataWorks.Web.Http.Abstractions.Policies;
using FractalDataWorks.Web.Http.Abstractions.Security;

namespace FractalDataWorks.Web.RestEndpoints.Configuration;

/// <summary>
/// Default settings for a specific endpoint type.
/// Following TypeCollection pattern: store names, retrieve objects via GetByName(string).
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
    /// Gets the security method by name.
    /// TODO: Re-enable once TypeCollection source generator creates GetByName method
    /// </summary>
    public ISecurityMethod? SecurityMethod => null; // SecurityMethods.GetByName(SecurityMethodName);

    /// <summary>
    /// Gets the rate limit policy by name.
    /// TODO: Re-enable once TypeCollection source generator creates GetByName method
    /// </summary>
    public IRateLimitPolicy? RateLimitPolicy => null; // RateLimitPolicies.GetByName(RateLimitPolicyName);
};
using FractalDataWorks.Web.Http.Abstractions.Policies;
using FractalDataWorks.Web.Http.Abstractions.Security;

namespace FractalDataWorks.Web.RestEndpoints.Configuration;

/// <summary>
/// Default settings for a specific endpoint type.
/// Following TypeCollection pattern: store names, retrieve objects via Name(string).
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
    /// </summary>
    // TODO: Enable once SecurityMethods source generator creates Name(string) method
    public ISecurityMethod? SecurityMethod => null; // SecurityMethods.Name(SecurityMethodName);

    /// <summary>
    /// Gets the rate limit policy by name.
    /// </summary>
    // TODO: Enable once RateLimitPolicies source generator creates Name(string) method
    public IRateLimitPolicy? RateLimitPolicy => null; // RateLimitPolicies.Name(RateLimitPolicyName);
};
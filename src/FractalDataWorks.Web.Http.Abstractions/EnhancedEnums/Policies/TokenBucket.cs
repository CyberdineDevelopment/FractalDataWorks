using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Policies;

/// <summary>
/// Token bucket algorithm rate limiting.
/// </summary>
[TypeOption(typeof(RateLimitPolicies), "TokenBucket")]
public sealed class TokenBucket : RateLimitPolicyBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenBucket"/> class.
    /// </summary>
    public TokenBucket() : base(
        id: 4,
        name: "TokenBucket",
        maxRequests: 50,
        windowSizeInSeconds: 10,
        policyType: "TokenBucket",
        isEnabled: true,
        defaultRequestLimit: 50,
        defaultTimeWindowSeconds: 10,
        supportsBurstCapacity: true)
    {
    }
}

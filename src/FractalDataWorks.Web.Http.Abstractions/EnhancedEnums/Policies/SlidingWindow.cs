using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Policies;

/// <summary>
/// Sliding time window rate limiting.
/// </summary>
[TypeOption(typeof(RateLimitPolicies), "SlidingWindow")]
public sealed class SlidingWindow : RateLimitPolicyBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SlidingWindow"/> class.
    /// </summary>
    public SlidingWindow() : base(
        id: 3,
        name: "SlidingWindow",
        maxRequests: 150,
        windowSizeInSeconds: 60,
        policyType: "SlidingWindow",
        isEnabled: true,
        defaultRequestLimit: 150,
        defaultTimeWindowSeconds: 60,
        supportsBurstCapacity: true)
    {
    }
}

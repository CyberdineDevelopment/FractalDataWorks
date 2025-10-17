using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Policies;

/// <summary>
/// Fixed time window rate limiting.
/// </summary>
[TypeOption(typeof(RateLimitPolicies), "FixedWindow")]
public sealed class FixedWindow : RateLimitPolicyBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FixedWindow"/> class.
    /// </summary>
    public FixedWindow() : base(
        id: 2,
        name: "FixedWindow",
        maxRequests: 100,
        windowSizeInSeconds: 60,
        policyType: "FixedWindow",
        isEnabled: true,
        defaultRequestLimit: 100,
        defaultTimeWindowSeconds: 60,
        supportsBurstCapacity: false)
    {
    }
}

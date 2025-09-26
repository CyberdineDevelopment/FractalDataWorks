using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Policies;

/// <summary>
/// Concurrent request limiting.
/// </summary>
[TypeOption(typeof(RateLimitPolicies), "Concurrency")]
public sealed class Concurrency : RateLimitPolicyBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Concurrency"/> class.
    /// </summary>
    public Concurrency() : base(
        id: 5,
        name: "Concurrency",
        maxRequests: 10,
        windowSizeInSeconds: 1,
        policyType: "Concurrency",
        isEnabled: true,
        defaultRequestLimit: 10,
        defaultTimeWindowSeconds: null, // Concurrency doesn't use time windows
        supportsBurstCapacity: false)
    {
    }
}

using FractalDataWorks.Collections.Attributes;
using System;
namespace FractalDataWorks.Web.Http.Abstractions.Policies;

/// <summary>
/// No rate limiting applied.
/// </summary>
[TypeOption(typeof(RateLimitPolicies), "None")]
public sealed class None : RateLimitPolicyBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    public None() : base(
        id: 1,
        name: "None",
        maxRequests: int.MaxValue,
        windowSizeInSeconds: 0,
        policyType: "None",
        isEnabled: false,
        defaultRequestLimit: null,
        defaultTimeWindowSeconds: null,
        supportsBurstCapacity: false)
    {
    }
}

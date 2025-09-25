using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Policies;

/// <summary>
/// No rate limiting applied.
/// </summary>
[TypeOption(typeof(RateLimitPolicies), "None")]
public sealed class None : RateLimitPolicyBase
{
    /// <summary>
    /// Gets the maximum number of requests allowed within the time window.
    /// </summary>
    public override int MaxRequests => int.MaxValue;

    /// <summary>
    /// Gets the time window duration in seconds for rate limiting.
    /// </summary>
    public override int WindowSizeInSeconds => 0;

    /// <summary>
    /// Gets the policy type identifier for the rate limiting algorithm.
    /// </summary>
    public override string PolicyType => "None";

    /// <summary>
    /// Gets a value indicating whether rate limiting is enabled for this policy.
    /// </summary>
    public override bool IsEnabled => false;

    /// <summary>
    /// Gets the default request limit per time window.
    /// Returns null if not applicable to this policy type.
    /// </summary>
    public override int? DefaultRequestLimit => null;

    /// <summary>
    /// Gets the default time window in seconds.
    /// Returns null if not applicable to this policy type.
    /// </summary>
    public override int? DefaultTimeWindowSeconds => null;

    /// <summary>
    /// Gets a value indicating whether this policy supports burst capacity.
    /// </summary>
    public override bool SupportsBurstCapacity => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    public None() : base(1, "None") { }
}

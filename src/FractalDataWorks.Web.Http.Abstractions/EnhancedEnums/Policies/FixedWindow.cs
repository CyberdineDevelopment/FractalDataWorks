using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Policies;

/// <summary>
/// Fixed time window rate limiting.
/// </summary>
[TypeOption(typeof(RateLimitPolicies), "FixedWindow")]
public sealed class FixedWindow : RateLimitPolicyBase
{
    /// <summary>
    /// Gets the maximum number of requests allowed within the time window.
    /// </summary>
    public override int MaxRequests => 100;

    /// <summary>
    /// Gets the time window duration in seconds for rate limiting.
    /// </summary>
    public override int WindowSizeInSeconds => 60;

    /// <summary>
    /// Gets the policy type identifier for the rate limiting algorithm.
    /// </summary>
    public override string PolicyType => "FixedWindow";

    /// <summary>
    /// Gets a value indicating whether rate limiting is enabled for this policy.
    /// </summary>
    public override bool IsEnabled => true;

    /// <summary>
    /// Gets the default request limit per time window.
    /// Returns null if not applicable to this policy type.
    /// </summary>
    public override int? DefaultRequestLimit => 100;

    /// <summary>
    /// Gets the default time window in seconds.
    /// Returns null if not applicable to this policy type.
    /// </summary>
    public override int? DefaultTimeWindowSeconds => 60;

    /// <summary>
    /// Gets a value indicating whether this policy supports burst capacity.
    /// </summary>
    public override bool SupportsBurstCapacity => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedWindow"/> class.
    /// </summary>
    public FixedWindow() : base(2, "FixedWindow") { }
}

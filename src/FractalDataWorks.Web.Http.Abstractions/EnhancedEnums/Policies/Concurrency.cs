using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Policies;

/// <summary>
/// Concurrent request limiting.
/// </summary>
[EnumOption("Concurrency")]
public sealed class Concurrency : RateLimitPolicyBase
{
    /// <summary>
    /// Gets the maximum number of requests allowed within the time window.
    /// </summary>
    public override int MaxRequests => 10;

    /// <summary>
    /// Gets the time window duration in seconds for rate limiting.
    /// </summary>
    public override int WindowSizeInSeconds => 1;

    /// <summary>
    /// Gets the policy type identifier for the rate limiting algorithm.
    /// </summary>
    public override string PolicyType => "Concurrency";

    /// <summary>
    /// Gets a value indicating whether rate limiting is enabled for this policy.
    /// </summary>
    public override bool IsEnabled => true;

    /// <summary>
    /// Gets the default request limit per time window.
    /// Returns null if not applicable to this policy type.
    /// </summary>
    public override int? DefaultRequestLimit => 10;

    /// <summary>
    /// Gets the default time window in seconds.
    /// Returns null if not applicable to this policy type.
    /// </summary>
    public override int? DefaultTimeWindowSeconds => null; // Concurrency doesn't use time windows

    /// <summary>
    /// Gets a value indicating whether this policy supports burst capacity.
    /// </summary>
    public override bool SupportsBurstCapacity => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Concurrency"/> class.
    /// </summary>
    public Concurrency() : base(5, "Concurrency") { }
}

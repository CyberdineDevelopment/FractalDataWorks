using FractalDataWorks.Collections;

namespace FractalDataWorks.Web.Http.Abstractions.Policies;

/// <summary>
/// Enhanced enum defining rate limiting policies for endpoint protection.
/// Provides various strategies for controlling request frequency and preventing abuse.
/// </summary>
public abstract class RateLimitPolicyBase : IRateLimitPolicy
{
    /// <summary>
    /// Gets the maximum number of requests allowed within the time window.
    /// </summary>
    public abstract int MaxRequests { get; }

    /// <summary>
    /// Gets the time window duration in seconds for rate limiting.
    /// </summary>
    public abstract int WindowSizeInSeconds { get; }

    /// <summary>
    /// Gets the policy type identifier for the rate limiting algorithm.
    /// </summary>
    public abstract string PolicyType { get; }

    /// <summary>
    /// Gets a value indicating whether rate limiting is enabled for this policy.
    /// </summary>
    public abstract bool IsEnabled { get; }

    /// <summary>
    /// Gets the default request limit per time window.
    /// Returns null if not applicable to this policy type.
    /// </summary>
    public abstract int? DefaultRequestLimit { get; }

    /// <summary>
    /// Gets the default time window in seconds.
    /// Returns null if not applicable to this policy type.
    /// </summary>
    public abstract int? DefaultTimeWindowSeconds { get; }

    /// <summary>
    /// Gets a value indicating whether this policy supports burst capacity.
    /// </summary>
    public abstract bool SupportsBurstCapacity { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitPolicyBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this rate limit policy.</param>
    /// <param name="name">The name of the rate limit policy.</param>
    protected RateLimitPolicyBase(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <inheritdoc/>
    public int Id { get; }

    /// <inheritdoc/>
    public string Name { get; }
}

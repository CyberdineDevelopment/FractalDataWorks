using System;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Configuration;

/// <summary>
/// Configuration settings for service health checks.
/// </summary>
public sealed class HealthCheckSettings
{
    /// <summary>
    /// Gets or sets the interval between health checks.
    /// Defaults to 30 seconds.
    /// </summary>
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the timeout for individual health checks.
    /// Defaults to 10 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets the number of consecutive failures before marking as unhealthy.
    /// Defaults to 3.
    /// </summary>
    public int FailureThreshold { get; set; } = 3;

    /// <summary>
    /// Gets or sets the number of consecutive successes required to recover from unhealthy state.
    /// Defaults to 2.
    /// </summary>
    public int SuccessThreshold { get; set; } = 2;

    /// <summary>
    /// Gets or sets a value indicating whether health checks are enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the initial delay before starting health checks.
    /// Defaults to 5 seconds.
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(5);
}
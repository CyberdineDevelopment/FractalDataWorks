using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Configuration interface for scheduling services.
/// </summary>
public interface ISchedulingConfiguration : IGenericConfiguration
{
    /// <summary>
    /// Gets the maximum number of concurrent jobs that can be executed.
    /// </summary>
    int MaxConcurrency { get; }

    /// <summary>
    /// Gets the default timeout for job execution in seconds.
    /// </summary>
    int DefaultTimeoutSeconds { get; }

    /// <summary>
    /// Gets a value indicating whether job execution history should be persisted.
    /// </summary>
    bool PersistJobHistory { get; }

    /// <summary>
    /// Gets the connection string for persistent storage if job persistence is enabled.
    /// </summary>
    string? PersistenceConnectionString { get; }

    /// <summary>
    /// Gets a value indicating whether to enable clustering support.
    /// </summary>
    bool EnableClustering { get; }

    /// <summary>
    /// Gets the cluster instance identifier for this scheduler instance.
    /// </summary>
    string? ClusterInstanceId { get; }

    /// <summary>
    /// Gets the interval in seconds for checking for misfired triggers.
    /// </summary>
    int MisfireThresholdSeconds { get; }

    /// <summary>
    /// Gets a value indicating whether to enable detailed logging.
    /// </summary>
    bool EnableDetailedLogging { get; }
}
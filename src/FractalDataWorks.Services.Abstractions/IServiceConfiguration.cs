using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Services.Abstractions;

/// <summary>
/// Base interface for service configurations.
/// </summary>
public interface IServiceConfiguration : IFractalConfiguration
{
    /// <summary>
    /// Gets the service type this configuration is for.
    /// </summary>
    string ServiceType { get; }

    /// <summary>
    /// Gets the retry policy configuration.
    /// </summary>
    IRetryPolicyConfiguration? RetryPolicy { get; }

    /// <summary>
    /// Gets the timeout in milliseconds.
    /// </summary>
    int TimeoutMs { get; }
}

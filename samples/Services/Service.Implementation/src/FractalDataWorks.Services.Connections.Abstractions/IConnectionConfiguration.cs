namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Base interface for connection configuration.
/// All connection configurations must implement this interface.
/// </summary>
public interface IConnectionConfiguration
{
    /// <summary>
    /// Gets or sets the connection type name (e.g., "MsSql").
    /// Used by the ConnectionProvider to determine which factory to use.
    /// </summary>
    string ConnectionTypeName { get; set; }

    /// <summary>
    /// Gets or sets the connection string or endpoint.
    /// </summary>
    string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// </summary>
    int CommandTimeout { get; set; }

    /// <summary>
    /// Gets or sets the maximum retry count for transient failures.
    /// </summary>
    int MaxRetryCount { get; set; }
}
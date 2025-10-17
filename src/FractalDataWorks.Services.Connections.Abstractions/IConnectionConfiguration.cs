using FractalDataWorks.Configuration;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Configuration interface for connection services.
/// All connection configurations must specify which connection type they are for
/// and their desired service lifetime for dependency injection registration.
/// Validation is provided by the source generator extension methods.
/// </summary>
public interface IConnectionConfiguration : IGenericConfiguration
{
    /// <summary>
    /// Gets the connection type name this configuration is for.
    /// This property is used by the ConnectionProvider to determine which factory to use.
    /// </summary>
    string ConnectionType { get; }

    /// <summary>
    /// Gets the service lifetime for this connection instance.
    /// Determines how the connection service is registered in the DI container.
    /// </summary>
    /// <remarks>
    /// Common patterns:
    /// - Scoped: For connections that should be shared within a request/scope (recommended default)
    /// - Singleton: For expensive connections that are safe for concurrent use (connection pools)
    /// - Transient: For lightweight connections without shared state (rare)
    /// </remarks>
    IServiceLifetime Lifetime { get; }
}

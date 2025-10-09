using System.Threading.Tasks;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Interface for connection factories in the FractalDataWorks framework.
/// Provides factory methods for creating connections.
/// </summary>
/// <remarks>
/// Connection factories abstract the creation of connections, enabling
/// the framework to create connections without knowing the specific implementation details.
/// </remarks>
public interface IConnectionFactory : IServiceFactory
{
    /// <summary>
    /// Creates a connection using the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration object for the connection.</param>
    /// <returns>
    /// A task representing the asynchronous connection creation operation.
    /// The result contains the created connection if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="configuration"/> is not of the expected type.
    /// </exception>
    /// <remarks>
    /// This method creates and initializes a new connection instance. The connection
    /// is not automatically opened - callers must call OpenAsync() separately.
    /// </remarks>
    Task<IGenericResult<IGenericConnection>> CreateConnectionAsync(IGenericConfiguration configuration);

    /// <summary>
    /// Creates a connection using the provided configuration with connection type specification.
    /// </summary>
    /// <param name="configuration">The configuration object for the connection.</param>
    /// <param name="connectionType">The specific type of connection to create.</param>
    /// <returns>
    /// A task representing the asynchronous connection creation operation.
    /// The result contains the created connection if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configuration"/> or <paramref name="connectionType"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="configuration"/> is not of the expected type or
    /// <paramref name="connectionType"/> is not supported.
    /// </exception>
    /// <remarks>
    /// This overload allows specifying the exact type of connection to create when
    /// the factory supports multiple connection types.
    /// </remarks>
    Task<IGenericResult<IGenericConnection>> CreateConnectionAsync(IGenericConfiguration configuration, string connectionType);
}

/// <summary>
/// Generic interface for connection factories with typed configuration.
/// Extends the base factory interface with type-safe configuration handling.
/// </summary>
/// <typeparam name="TConnection">The type of connection this factory creates.</typeparam>
/// <typeparam name="TConfiguration">The type of configuration this factory requires.</typeparam>
/// <remarks>
/// Use this interface for factories that work with specific configuration and connection types.
/// It provides type safety and eliminates the need for runtime type checking and casting.
/// </remarks>
public interface IConnectionFactory<TConnection, in TConfiguration> : IConnectionFactory, IServiceFactory<TConnection, TConfiguration>
    where TConfiguration : IConnectionConfiguration
    where TConnection : IGenericConnection
{
    /// <summary>
    /// Creates a typed connection using the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration object for the connection.</param>
    /// <returns>
    /// A task representing the asynchronous connection creation operation.
    /// The result contains the created connection if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <remarks>
    /// This method provides type-safe connection creation without the need for casting.
    /// The connection is not automatically opened - callers must call OpenAsync() separately.
    /// </remarks>
    Task<IGenericResult<TConnection>> CreateConnectionAsync(TConfiguration configuration);

    /// <summary>
    /// Creates a typed connection using the provided configuration with connection type specification.
    /// </summary>
    /// <param name="configuration">The configuration object for the connection.</param>
    /// <param name="connectionType">The specific type of connection to create.</param>
    /// <returns>
    /// A task representing the asynchronous connection creation operation.
    /// The result contains the created connection if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configuration"/> or <paramref name="connectionType"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="connectionType"/> is not supported.
    /// </exception>
    /// <remarks>
    /// This overload allows specifying the exact type of connection to create when
    /// the factory supports multiple connection types, with full type safety.
    /// </remarks>
    Task<IGenericResult<TConnection>> CreateConnectionAsync(TConfiguration configuration, string connectionType);
}

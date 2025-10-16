using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Defines the contract for connection providers in the FractalDataWorks framework.
/// The provider uses ConnectionTypes to lookup configuration types and factories.
/// Follows Railway-Oriented Programming - all operations return Results.
/// </summary>
public interface IDefaultConnectionProvider
{
    /// <summary>
    /// Gets a connection using the provided configuration.
    /// The configuration's ConnectionType property determines which factory to use.
    /// </summary>
    /// <param name="configuration">The configuration containing the connection type and settings.</param>
    /// <returns>A result containing the connection instance or failure information.</returns>
    Task<IGenericResult<IGenericConnection>> GetConnection(IConnectionConfiguration configuration);
    
    /// <summary>
    /// Gets a connection by configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration to load.</param>
    /// <returns>A result containing the connection instance or failure information.</returns>
    Task<IGenericResult<IGenericConnection>> GetConnection(int configurationId);
    
    /// <summary>
    /// Gets a connection by configuration name from appsettings.
    /// </summary>
    /// <param name="configurationName">The name of the configuration section.</param>
    /// <returns>A result containing the connection instance or failure information.</returns>
    Task<IGenericResult<IGenericConnection>> GetConnection(string configurationName);

    /// <summary>
    /// Gets a connection using the provided configuration and attempts to cast it to the specified type.
    /// The configuration's ConnectionType property determines which factory to use.
    /// </summary>
    /// <typeparam name="T">The specific connection interface type to cast to (e.g., IDataConnection).</typeparam>
    /// <param name="configuration">The configuration containing the connection type and settings.</param>
    /// <returns>A result containing the typed connection instance or failure information if not found or cast fails.</returns>
    Task<IGenericResult<T>> GetConnection<T>(IConnectionConfiguration configuration) where T : IGenericConnection;

    /// <summary>
    /// Gets a connection by configuration ID and attempts to cast it to the specified type.
    /// </summary>
    /// <typeparam name="T">The specific connection interface type to cast to (e.g., IDataConnection).</typeparam>
    /// <param name="configurationId">The ID of the configuration to load.</param>
    /// <returns>A result containing the typed connection instance or failure information if not found or cast fails.</returns>
    Task<IGenericResult<T>> GetConnection<T>(int configurationId) where T : IGenericConnection;

    /// <summary>
    /// Gets a connection by configuration name from appsettings and attempts to cast it to the specified type.
    /// </summary>
    /// <typeparam name="T">The specific connection interface type to cast to (e.g., IDataConnection).</typeparam>
    /// <param name="configurationName">The name of the configuration section.</param>
    /// <returns>A result containing the typed connection instance or failure information if not found or cast fails.</returns>
    Task<IGenericResult<T>> GetConnection<T>(string configurationName) where T : IGenericConnection;
}
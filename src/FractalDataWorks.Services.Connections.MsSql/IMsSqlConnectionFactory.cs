using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.MsSql;

/// <summary>
/// Factory interface for creating Microsoft SQL Server connection instances.
/// Provides SQL Server-specific connection creation and management.
/// </summary>
public interface IMsSqlConnectionFactory : IConnectionFactory<IGenericConnection, MsSqlConfiguration>
{
    /// <summary>
    /// Creates a new SQL Server connection with the specified configuration.
    /// </summary>
    /// <param name="configuration">The SQL Server connection configuration.</param>
    /// <returns>A result containing the created connection or failure information.</returns>
    Task<IGenericResult<IGenericConnection>> CreateAsync(MsSqlConfiguration configuration);

    /// <summary>
    /// Validates a SQL Server connection configuration.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>A result indicating whether the configuration is valid.</returns>
    IGenericResult ValidateConfiguration(MsSqlConfiguration configuration);

    /// <summary>
    /// Tests connectivity to a SQL Server using the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration to test.</param>
    /// <returns>A result indicating whether the connection test succeeded.</returns>
    Task<IGenericResult> TestConnectionAsync(MsSqlConfiguration configuration);
}
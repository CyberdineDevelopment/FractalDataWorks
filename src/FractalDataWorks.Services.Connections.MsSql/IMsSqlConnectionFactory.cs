using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.MsSql;

/// <summary>
/// Factory interface for creating Microsoft SQL Server connection instances.
/// </summary>
public interface IMsSqlConnectionFactory : IConnectionFactory<IGenericConnection, MsSqlConfiguration>
{
    /// <summary>
    /// Creates a new SQL Server connection with the specified configuration.
    /// </summary>
    /// <param name="configuration">The SQL Server connection configuration.</param>
    /// <returns>A result containing the created connection or failure information.</returns>
    Task<IGenericResult<IGenericConnection>> CreateAsync(MsSqlConfiguration configuration);
}
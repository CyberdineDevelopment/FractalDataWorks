using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Interface for providers that can retrieve data connections by name.
/// Used by data gateway services to route data commands to the appropriate connection.
/// </summary>
/// <remarks>
/// This interface is implemented by connection providers that manage IDataConnection instances.
/// The DataGateway service depends on this interface to resolve connections by name and
/// execute data commands (containing LINQ expressions, SQL queries, etc.) against them.
/// </remarks>
public interface IDataConnectionProvider
{
    /// <summary>
    /// Retrieves a data connection by name.
    /// </summary>
    /// <param name="connectionName">The name of the connection to retrieve.</param>
    /// <returns>A result containing the data connection if found, or a failure result if not found.</returns>
    Task<IGenericResult<IDataConnection>> GetConnection(string connectionName);
}

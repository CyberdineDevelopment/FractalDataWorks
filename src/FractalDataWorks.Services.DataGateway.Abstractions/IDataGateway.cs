using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Data.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

/// <summary>
/// Service that routes data commands to the appropriate connection.
/// The DataGateway selects the correct connection based on the command's ConnectionName
/// and delegates execution to that connection.
/// </summary>
public interface IDataGateway
{
    /// <summary>
    /// Executes a data command by routing it to the appropriate connection.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="command">The data command containing ConnectionName.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    Task<IGenericResult<T>> Execute<T>(IDataCommand command, CancellationToken cancellationToken = default);
}

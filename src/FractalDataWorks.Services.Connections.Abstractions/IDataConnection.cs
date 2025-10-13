using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Data.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Interface for connections capable of executing data commands.
/// Extends IGenericConnection with data-specific command execution methods that accept IDataCommand.
/// </summary>
/// <remarks>
/// This interface provides method overloads for IDataCommand execution alongside the base
/// IGenericConnection Execute(IGenericCommand) methods. Implementations can handle both
/// generic commands and specialized data commands (containing LINQ expressions, queries, etc.).
/// </remarks>
public interface IDataConnection : IGenericConnection
{
    /// <summary>
    /// Executes a data command and returns a typed result.
    /// </summary>
    /// <typeparam name="T">The type of data expected in the result.</typeparam>
    /// <param name="command">The data command to execute.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the typed execution outcome.</returns>
    Task<IGenericResult<T>> Execute<T>(IDataCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Executes a data command and returns a non-typed result.
    /// </summary>
    /// <param name="command">The data command to execute.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the execution outcome.</returns>
    Task<IGenericResult> Execute(IDataCommand command, CancellationToken cancellationToken);
}

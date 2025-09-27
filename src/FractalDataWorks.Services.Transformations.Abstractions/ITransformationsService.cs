using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Transformations.Abstractions;

/// <summary>
/// Interface for transformation service implementations.
/// Provides data transformation, mapping, and processing capabilities.
/// </summary>
public interface ITransformationsService : IFdwService
{
    /// <summary>
    /// Executes a transformation request using the specified transformation context.
    /// </summary>
    /// <typeparam name="TOutput">The type of the output data.</typeparam>
    /// <param name="request">The transformation request configuration.</param>
    /// <param name="context">The transformation execution context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous transformation operation.</returns>
    Task<IFdwResult<TOutput>> Transform<TOutput>(
        ITransformationRequest request,
        ITransformationContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metrics about transformation performance and execution statistics.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous metrics retrieval operation.</returns>
    Task<IFdwResult<ITransformationMetrics>> GetTransformationMetrics(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a transformation command and returns a typed result.
    /// Integrates with the command pattern for unified service execution.
    /// </summary>
    /// <typeparam name="TResult">The type of result expected from the command execution.</typeparam>
    /// <param name="command">The transformation command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the command execution to complete.</param>
    /// <returns>A task that represents the asynchronous command execution operation.</returns>
    Task<IFdwResult<TResult>> Execute<TResult>(ITransformationsCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a transformation command without returning a specific result type.
    /// Integrates with the command pattern for unified service execution.
    /// </summary>
    /// <param name="command">The transformation command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the command execution to complete.</param>
    /// <returns>A task that represents the asynchronous command execution operation.</returns>
    Task<IFdwResult> Execute(ITransformationsCommand command, CancellationToken cancellationToken = default);
}
namespace FractalDataWorks.Commands.Abstractions.Commands;

/// <summary>
/// Defines bulk operation types.
/// </summary>
public interface IBulkOperation : ICommandCategory
{
    /// <summary>
    /// Gets whether this operation supports parallel execution.
    /// </summary>
    bool SupportsParallel { get; }

    /// <summary>
    /// Gets the optimal batch size for this operation.
    /// </summary>
    int OptimalBatchSize { get; }
}

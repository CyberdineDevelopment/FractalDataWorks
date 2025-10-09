using System.Collections.Generic;

namespace FractalDataWorks.Commands.Abstractions.Commands;

/// <summary>
/// Interface for bulk operation commands.
/// </summary>
/// <remarks>
/// Bulk commands operate on multiple records efficiently,
/// using techniques like batch processing and bulk loading.
/// </remarks>
public interface IBulkCommand : IDataCommand
{
    /// <summary>
    /// Gets the bulk operation type.
    /// </summary>
    /// <value>The type of bulk operation to perform.</value>
    IBulkOperation Operation { get; }

    /// <summary>
    /// Gets the collection of items for bulk processing.
    /// </summary>
    /// <value>The items to process in bulk.</value>
    IEnumerable<object> Items { get; }

    /// <summary>
    /// Gets the batch size for processing.
    /// </summary>
    /// <value>Number of items to process in each batch.</value>
    int BatchSize { get; }

    /// <summary>
    /// Gets whether to continue on errors.
    /// </summary>
    /// <value>True to continue processing after encountering errors.</value>
    bool ContinueOnError { get; }

    /// <summary>
    /// Gets whether to validate data before processing.
    /// </summary>
    /// <value>True to perform validation on all items before execution.</value>
    bool PreValidate { get; }

    /// <summary>
    /// Gets the parallel processing options.
    /// </summary>
    /// <value>Configuration for parallel bulk processing.</value>
    IParallelOptions? ParallelOptions { get; }

    /// <summary>
    /// Gets the progress reporting callback.
    /// </summary>
    /// <value>Callback for reporting bulk operation progress.</value>
    IProgressCallback? ProgressCallback { get; }
}

/// <summary>
/// Defines bulk operation types.
/// </summary>
public interface IBulkOperation : ICommandCategory
{
    /// <summary>
    /// Gets whether this operation supports streaming.
    /// </summary>
    bool SupportsStreaming { get; }

    /// <summary>
    /// Gets whether this operation supports parallel execution.
    /// </summary>
    bool SupportsParallel { get; }

    /// <summary>
    /// Gets the optimal batch size for this operation.
    /// </summary>
    int OptimalBatchSize { get; }
}

/// <summary>
/// Configuration for parallel bulk processing.
/// </summary>
public interface IParallelOptions
{
    /// <summary>
    /// Gets the maximum degree of parallelism.
    /// </summary>
    int MaxDegreeOfParallelism { get; }

    /// <summary>
    /// Gets whether to preserve item order.
    /// </summary>
    bool PreserveOrder { get; }
}

/// <summary>
/// Callback for reporting bulk operation progress.
/// </summary>
public interface IProgressCallback
{
    /// <summary>
    /// Reports progress of bulk operation.
    /// </summary>
    /// <param name="processedCount">Number of items processed.</param>
    /// <param name="totalCount">Total number of items.</param>
    /// <param name="currentBatch">Current batch number.</param>
    void ReportProgress(int processedCount, int totalCount, int currentBatch);
}
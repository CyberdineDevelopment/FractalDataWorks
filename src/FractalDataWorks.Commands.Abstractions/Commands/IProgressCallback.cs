namespace FractalDataWorks.Commands.Abstractions.Commands;

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

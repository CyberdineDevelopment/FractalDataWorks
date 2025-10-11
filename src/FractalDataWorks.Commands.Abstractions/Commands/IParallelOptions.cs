namespace FractalDataWorks.Commands.Abstractions.Commands;

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

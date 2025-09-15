namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Basic result interface for sample.
/// NOTE: In production, use IFdwResult from FractalDataWorks.Results
/// </summary>
public interface IFdwResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    string Error { get; }
}

/// <summary>
/// Basic result interface with value for sample.
/// NOTE: In production, use IFdwResult&lt;T&gt; from FractalDataWorks.Results
/// </summary>
public interface IFdwResult<T> : IFdwResult
{
    /// <summary>
    /// Gets the value if the operation was successful.
    /// </summary>
    T Value { get; }
}
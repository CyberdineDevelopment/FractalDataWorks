namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public interface IFdwResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    bool IsFailure { get; }

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    string Error { get; }
}

/// <summary>
/// Represents the result of an operation with a return value.
/// </summary>
public interface IFdwResult<T> : IFdwResult
{
    /// <summary>
    /// Gets the value if the operation was successful.
    /// </summary>
    T Value { get; }
}
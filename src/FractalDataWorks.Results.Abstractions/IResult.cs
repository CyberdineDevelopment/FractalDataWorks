namespace FractalDataWorks.Results;

/// <summary>
/// Represents the result of an operation that may succeed or fail.
/// </summary>
public interface IResult
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
    string? Error { get; }

    /// <summary>
    /// Gets the error code if the operation failed.
    /// </summary>
    string? ErrorCode { get; }

    /// <summary>
    /// Gets additional message about the operation result.
    /// </summary>
    string? Message { get; }
}

/// <summary>
/// Represents the result of an operation that may succeed with a value or fail.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public interface IResult<T> : IResult
{
    /// <summary>
    /// Gets the value if the operation was successful.
    /// </summary>
    T? Value { get; }
}
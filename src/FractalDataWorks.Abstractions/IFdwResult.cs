using FractalDataWorks.Messages;

namespace FractalDataWorks.Results;

/// <summary>
/// Represents a result that can be either success or failure.
/// </summary>
public interface IFdwResult
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    bool IsFailure { get; }

    /// <summary>
    /// Gets a value indicating whether this represents an empty result
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Gets a value indicating whether this result represents an error.
    /// </summary>
    bool Error { get; }

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    string? Message { get; }
}

/// <summary>
/// Represents a result that can be either success or failure with a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public interface IFdwResult<out T> : IFdwResult
{
    /// <summary>
    /// Gets the result value if successful.
    /// </summary>
    T? Value { get; }
}

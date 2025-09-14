namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Implementation of IFdwResult.
/// </summary>
public sealed class FdwResult : IFdwResult
{
    private FdwResult(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <inheritdoc />
    public bool IsSuccess { get; }
    /// <inheritdoc />
    public bool IsFailure => !IsSuccess;
    /// <inheritdoc />
    public string Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static IFdwResult Success() => new FdwResult(true, string.Empty);

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult Failure(string error) => new FdwResult(false, error);

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>A successful result with the value.</returns>
    public static IFdwResult<T> Success<T>(T value) => new FdwResult<T>(true, value, string.Empty);

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult<T> Failure<T>(string error) => new FdwResult<T>(false, default!, error);
}

/// <summary>
/// Implementation of IFdwResult&lt;T&gt;.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public sealed class FdwResult<T> : IFdwResult<T>
{
    internal FdwResult(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <inheritdoc />
    public bool IsSuccess { get; }
    /// <inheritdoc />
    public bool IsFailure => !IsSuccess;
    /// <inheritdoc />
    public string Error { get; }
    /// <inheritdoc />
    public T Value { get; }
}
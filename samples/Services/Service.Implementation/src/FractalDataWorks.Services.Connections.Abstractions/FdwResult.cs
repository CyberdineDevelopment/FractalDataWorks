namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Basic implementation of IFdwResult for sample.
/// NOTE: In production, use FdwResult from FractalDataWorks.Results
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
    public string Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static IFdwResult Success() => new FdwResult(true, string.Empty);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static IFdwResult Failure(string error) => new FdwResult(false, error);

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static IFdwResult<T> Success<T>(T value) => new FdwResult<T>(true, value, string.Empty);

    /// <summary>
    /// Creates a failed result with a value.
    /// </summary>
    public static IFdwResult<T> Failure<T>(string error) => new FdwResult<T>(false, default!, error);
}

/// <summary>
/// Basic implementation of IFdwResult&lt;T&gt; for sample.
/// </summary>
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
    public string Error { get; }

    /// <inheritdoc />
    public T Value { get; }
}
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

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    public static IFdwResult Success() => new FdwResult(true, string.Empty);
    public static IFdwResult Failure(string error) => new FdwResult(false, error);

    public static IFdwResult<T> Success<T>(T value) => new FdwResult<T>(true, value, string.Empty);
    public static IFdwResult<T> Failure<T>(string error) => new FdwResult<T>(false, default!, error);
}

/// <summary>
/// Implementation of IFdwResult<T>.
/// </summary>
public sealed class FdwResult<T> : IFdwResult<T>
{
    internal FdwResult(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public T Value { get; }
}
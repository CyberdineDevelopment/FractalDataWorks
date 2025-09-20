using System;

namespace FractalDataWorks.Results;

/// <summary>
/// Represents the result of an operation that may succeed or fail.
/// </summary>
public class Result : IResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class for a successful result.
    /// </summary>
    protected Result()
    {
        IsSuccess = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class for a failed result.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="errorCode">The optional error code.</param>
    protected Result(string error, string? errorCode = null)
    {
        IsSuccess = false;
        Error = error ?? throw new ArgumentNullException(nameof(error));
        ErrorCode = errorCode;
    }

    /// <inheritdoc />
    public bool IsSuccess { get; }

    /// <inheritdoc />
    public bool IsFailure => !IsSuccess;

    /// <inheritdoc />
    public string? Error { get; }

    /// <inheritdoc />
    public string? ErrorCode { get; }

    /// <inheritdoc />
    public string? Message { get; protected set; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static Result Success() => new Result();

    /// <summary>
    /// Creates a successful result with a message.
    /// </summary>
    /// <param name="message">The success message.</param>
    /// <returns>A successful result with a message.</returns>
    public static Result Success(string message) => new Result { Message = message };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result.</returns>
    public static Result Failure(string error) => new Result(error);

    /// <summary>
    /// Creates a failed result with an error code.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <returns>A failed result with an error code.</returns>
    public static Result Failure(string error, string errorCode) => new Result(error, errorCode);
}

/// <summary>
/// Represents the result of an operation that may succeed with a value or fail.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class Result<T> : Result, IResult<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class for a successful result.
    /// </summary>
    /// <param name="value">The value.</param>
    protected Result(T value) : base()
    {
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class for a failed result.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="errorCode">The optional error code.</param>
    protected Result(string error, string? errorCode = null) : base(error, errorCode)
    {
        Value = default;
    }

    /// <inheritdoc />
    public T? Value { get; }

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A successful result with a value.</returns>
    public static Result<T> Success(T value) => new Result<T>(value);

    /// <summary>
    /// Creates a successful result with a value and message.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="message">The success message.</param>
    /// <returns>A successful result with a value and message.</returns>
    public static Result<T> Success(T value, string message) => new Result<T>(value) { Message = message };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result.</returns>
    public new static Result<T> Failure(string error) => new Result<T>(error);

    /// <summary>
    /// Creates a failed result with an error code.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <returns>A failed result with an error code.</returns>
    public new static Result<T> Failure(string error, string errorCode) => new Result<T>(error, errorCode);
}
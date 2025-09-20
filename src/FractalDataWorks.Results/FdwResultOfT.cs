using System;
using System.Collections.Generic;
using FractalDataWorks.Messages;

namespace FractalDataWorks.Results;

/// <summary>
/// Basic implementation of IFdwResult with a value.
/// </summary>
/// <typeparam name="TResult">The type of the value.</typeparam>
public class FdwResult<TResult> : FdwResult, IFdwResult<TResult>
{
    private readonly TResult _value;
    private readonly bool _hasValue;

    private FdwResult(bool isSuccess, TResult value, string? message = null) : base(isSuccess, message)
    {
        _value = value;
        _hasValue = isSuccess;
    }

    private FdwResult(bool isSuccess, TResult value, IFdwMessage? message) : base(isSuccess, message)
    {
        _value = value;
        _hasValue = isSuccess;
    }

    private FdwResult(bool isSuccess, TResult value, IEnumerable<IFdwMessage>? messages) : base(isSuccess, messages)
    {
        _value = value;
        _hasValue = isSuccess;
    }

    /// <summary>
    /// Provides collection of messages associated with the result
    /// </summary>
    public new IReadOnlyList<IFdwMessage> Messages => base.Messages;

    /// <summary>
    /// Returns a value indicating whether it is empty
    /// </summary>
    public override bool IsEmpty => !_hasValue;

    /// <inheritdoc/>
    public TResult Value
    {
        get
        {
            if (!_hasValue)
                throw new InvalidOperationException("Cannot access value of a failed result.");
            return _value;
        }
    }

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult<TResult> Success(TResult value) => new FdwResult<TResult>(true, value);

    /// <summary>
    /// Creates a successful result with a value and message.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="message">The success message.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult<TResult> Success(TResult value, string message) => new FdwResult<TResult>(true, value, message);

    /// <summary>
    /// Creates a successful result with a value and IFractalMessage.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="message">The success message object.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult<TResult> Success(TResult value, IFdwMessage message) => new FdwResult<TResult>(true, value, message);

    /// <summary>
    /// Creates a successful result with a value and any object that implements IFractalMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that implements IFractalMessage.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="message">The success message object.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult<TResult> Success<TMessage>(TResult value, TMessage message) where TMessage : IFdwMessage => new FdwResult<TResult>(true, value, message);

    /// <summary>
    /// Creates a successful result with a value and multiple IFractalMessages.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="messages">The success message objects.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult<TResult> Success(TResult value, IEnumerable<IFdwMessage> messages) => new FdwResult<TResult>(true, value, messages);

    /// <summary>
    /// Creates a successful result with a value and multiple IFractalMessages.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="messages">The success message objects.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult<TResult> Success(TResult value, params IFdwMessage[] messages) => new FdwResult<TResult>(true, value, messages);

    /// <summary>
    /// Creates a failed result with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <typeparam name="T">The type of the result</typeparam>
    /// <returns>A failed result.</returns>
    public static IFdwResult<T> Failure<T>(string message) => new FdwResult<T>(false, default!, message);

    /// <summary>
    /// Creates a failed result with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <returns>A failed result.</returns>
    public new static IFdwResult<TResult> Failure(string message) => new FdwResult<TResult>(false, default!, message);

    /// <summary>
    /// Creates a failed result with an IFractalMessage.
    /// </summary>
    /// <param name="message">The failure message object.</param>
    /// <returns>A failed result.</returns>
    public new static IFdwResult<TResult> Failure(IFdwMessage message) => new FdwResult<TResult>(false, default!, message);

    /// <summary>
    /// Creates a failed result with any object that implements IFractalMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that implements IFractalMessage.</typeparam>
    /// <param name="message">The failure message object.</param>
    /// <returns>A failed result.</returns>
    public new static IFdwResult<TResult> Failure<TMessage>(TMessage message) where TMessage : IFdwMessage => new FdwResult<TResult>(false, default!, message);

    /// <summary>
    /// Creates a failed result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The failure message objects.</param>
    /// <returns>A failed result.</returns>
    public new static IFdwResult<TResult> Failure(IEnumerable<IFdwMessage> messages) => new FdwResult<TResult>(false, default!, messages);

    /// <summary>
    /// Creates a failed result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The failure message objects.</param>
    /// <returns>A failed result.</returns>
    public new static IFdwResult<TResult> Failure(params IFdwMessage[] messages) => new FdwResult<TResult>(false, default!, messages);


    /// <inheritdoc/>
    public IFdwResult<TNew> Map<TNew>(Func<TResult, TNew> mapper)
    {
        if (mapper == null)
            throw new ArgumentNullException(nameof(mapper));

        return IsSuccess
            ? FdwResult<TNew>.Success(mapper(Value))
            : FdwResult<TNew>.Failure(Message ?? "Operation failed");
    }

    /// <inheritdoc/>
    public T Match<T>(Func<TResult, T> success, Func<string, T> failure)
    {
        if (success == null)
            throw new ArgumentNullException(nameof(success));
        if (failure == null)
            throw new ArgumentNullException(nameof(failure));

        return IsSuccess ? success(Value) : failure(Message ?? string.Empty);
    }
}
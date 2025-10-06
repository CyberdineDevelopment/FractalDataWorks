using System;
using System.Collections.Generic;
using FractalDataWorks.Messages;

namespace FractalDataWorks.Results;

/// <summary>
/// Basic implementation of IGenericResult with a value.
/// </summary>
/// <typeparam name="TResult">The type of the value.</typeparam>
public class GenericResult<TResult> : GenericResult, IGenericResult<TResult>
{
    private readonly TResult _value;
    private readonly bool _hasValue;

    private GenericResult(bool isSuccess, TResult value, string? message = null) : base(isSuccess, message)
    {
        _value = value;
        _hasValue = isSuccess;
    }

    private GenericResult(bool isSuccess, TResult value, IGenericMessage? message) : base(isSuccess, message)
    {
        _value = value;
        _hasValue = isSuccess;
    }

    private GenericResult(bool isSuccess, TResult value, IEnumerable<IGenericMessage>? messages) : base(isSuccess, messages)
    {
        _value = value;
        _hasValue = isSuccess;
    }

    /// <summary>
    /// Provides collection of messages associated with the result
    /// </summary>
    public new IReadOnlyList<IGenericMessage> Messages => base.Messages;

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
    public static IGenericResult<TResult> Success(TResult value) => new GenericResult<TResult>(true, value);

    /// <summary>
    /// Creates a successful result with a value and message.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="message">The success message.</param>
    /// <returns>A successful result.</returns>
    public static IGenericResult<TResult> Success(TResult value, string message) => new GenericResult<TResult>(true, value, message);

    /// <summary>
    /// Creates a successful result with a value and IRecMessage.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="message">The success message object.</param>
    /// <returns>A successful result.</returns>
    public static IGenericResult<TResult> Success(TResult value, IGenericMessage message) => new GenericResult<TResult>(true, value, message);

    /// <summary>
    /// Creates a successful result with a value and any object that implements IRecMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that implements IRecMessage.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="message">The success message object.</param>
    /// <returns>A successful result.</returns>
    public static IGenericResult<TResult> Success<TMessage>(TResult value, TMessage message) where TMessage : IGenericMessage => new GenericResult<TResult>(true, value, message);

    /// <summary>
    /// Creates a successful result with a value and multiple IRecMessages.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="messages">The success message objects.</param>
    /// <returns>A successful result.</returns>
    public static IGenericResult<TResult> Success(TResult value, IEnumerable<IGenericMessage> messages) => new GenericResult<TResult>(true, value, messages);

    /// <summary>
    /// Creates a successful result with a value and multiple IRecMessages.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="messages">The success message objects.</param>
    /// <returns>A successful result.</returns>
    public static IGenericResult<TResult> Success(TResult value, params IGenericMessage[] messages) => new GenericResult<TResult>(true, value, messages);

    /// <summary>
    /// Creates a failed result with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <typeparam name="T">The type of the result</typeparam>
    /// <returns>A failed result.</returns>
    public static IGenericResult<T> Failure<T>(string message) => new GenericResult<T>(false, default!, message);

    /// <summary>
    /// Creates a failed result with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <returns>A failed result.</returns>
    public new static IGenericResult<TResult> Failure(string message) => new GenericResult<TResult>(false, default!, message);

    /// <summary>
    /// Creates a failed result with an IRecMessage.
    /// </summary>
    /// <param name="message">The failure message object.</param>
    /// <returns>A failed result.</returns>
    public new static IGenericResult<TResult> Failure(IGenericMessage message) => new GenericResult<TResult>(false, default!, message);

    /// <summary>
    /// Creates a failed result with any object that implements IRecMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that implements IRecMessage.</typeparam>
    /// <param name="message">The failure message object.</param>
    /// <returns>A failed result.</returns>
    public new static IGenericResult<TResult> Failure<TMessage>(TMessage message) where TMessage : IGenericMessage => new GenericResult<TResult>(false, default!, message);

    /// <summary>
    /// Creates a failed result with multiple IRecMessages.
    /// </summary>
    /// <param name="messages">The failure message objects.</param>
    /// <returns>A failed result.</returns>
    public new static IGenericResult<TResult> Failure(IEnumerable<IGenericMessage> messages) => new GenericResult<TResult>(false, default!, messages);

    /// <summary>
    /// Creates a failed result with multiple IRecMessages.
    /// </summary>
    /// <param name="messages">The failure message objects.</param>
    /// <returns>A failed result.</returns>
    public new static IGenericResult<TResult> Failure(params IGenericMessage[] messages) => new GenericResult<TResult>(false, default!, messages);


    /// <inheritdoc/>
    public IGenericResult<TNew> Map<TNew>(Func<TResult, TNew> mapper)
    {
        if (mapper == null)
            throw new ArgumentNullException(nameof(mapper));

        return IsSuccess
            ? GenericResult<TNew>.Success(mapper(Value))
            : GenericResult<TNew>.Failure(Message ?? "Operation failed");
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
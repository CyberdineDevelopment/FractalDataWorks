using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FractalDataWorks.Messages;

namespace FractalDataWorks.Results;

/// <summary>
/// Basic implementation of IGenericResult.
/// </summary>
public class GenericResult : IGenericResult
{
    private readonly List<IGenericMessage> _messages = [];

    /// <summary>
    /// Constructor for GenericResult
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="message"></param>
    protected GenericResult(bool isSuccess, string? message = null)
    {
        IsSuccess = isSuccess;
        if (!string.IsNullOrEmpty(message))
        {
            _messages.Add(new RecMessage(message!));
        }
    }

    /// <summary>
    /// Constructor for GenericResult with IRecMessage
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="message"></param>
    protected GenericResult(bool isSuccess, IGenericMessage? message)
    {
        IsSuccess = isSuccess;
        if (message != null)
        {
            _messages.Add(message);
        }
    }

    /// <summary>
    /// Constructor for GenericResult with multiple messages
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="messages"></param>
    protected GenericResult(bool isSuccess, IEnumerable<IGenericMessage>? messages)
    {
        IsSuccess = isSuccess;
        if (messages != null)
        {
            _messages.AddRange(messages);
        }
    }

    /// <inheritdoc/>
    public virtual bool IsSuccess { get; }

    /// <inheritdoc/>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Returns a value indicating whether there is an error
    /// </summary>
    public bool Error => !IsSuccess;

    /// <summary>
    /// Returns a value indicating whether there is a message;
    /// </summary>
    public virtual bool IsEmpty => _messages.Count == 0;

    /// <summary>
    /// Provides collection of messages associated with the result
    /// </summary>
    public IReadOnlyList<IGenericMessage> Messages => _messages.AsReadOnly();

    /// <summary>
    /// Gets the most recent message (LIFO - Last In, First Out)
    /// </summary>
    public string? CurrentMessage => _messages.LastOrDefault()?.Message;

    /// <summary>
    /// Adds a message to this result's message collection.
    /// </summary>
    /// <param name="message">The message to add.</param>
    /// <ExcludedFromCoverage>Protected extension point for derived classes - not currently used</ExcludedFromCoverage>
    [ExcludeFromCodeCoverage]
    protected void AddMessage(IGenericMessage message)
    {
        _messages.Add(message);
    }

    /// <summary>
    /// Adds multiple messages to this result's message collection.
    /// </summary>
    /// <param name="messages">The messages to add.</param>
    /// <ExcludedFromCoverage>Protected extension point for derived classes - not currently used</ExcludedFromCoverage>
    [ExcludeFromCodeCoverage]
    protected void AddMessages(IEnumerable<IGenericMessage> messages)
    {
        _messages.AddRange(messages);
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static IGenericResult Success() => new GenericResult(true);

    /// <summary>
    /// Creates a successful result with a message.
    /// </summary>
    /// <param name="message">The success message.</param>
    /// <returns>A successful result.</returns>
    public static IGenericResult Success(string message) => new GenericResult(true, message);

    /// <summary>
    /// Creates a successful result with an IRecMessage.
    /// </summary>
    /// <param name="message">The success message object.</param>
    /// <returns>A successful result.</returns>
    public static IGenericResult Success(IGenericMessage message) => new GenericResult(true, message);

    /// <summary>
    /// Creates a successful result with any object that implements IRecMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that implements IRecMessage.</typeparam>
    /// <param name="message">The success message object.</param>
    /// <returns>A successful result.</returns>
    public static IGenericResult Success<TMessage>(TMessage message) where TMessage : IGenericMessage => new GenericResult(true, message);

    /// <summary>
    /// Creates a successful result with multiple IRecMessages.
    /// </summary>
    /// <param name="messages">The success message objects.</param>
    /// <returns>A successful result.</returns>
    public static IGenericResult Success(IEnumerable<IGenericMessage> messages) => new GenericResult(true, messages);

    /// <summary>
    /// Creates a successful result with multiple IRecMessages.
    /// </summary>
    /// <param name="messages">The success message objects.</param>
    /// <returns>A successful result.</returns>
    public static IGenericResult Success(params IGenericMessage[] messages) => new GenericResult(true, messages);

    /// <summary>
    /// Creates a failed result with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <returns>A failed result.</returns>
    public static IGenericResult Failure(string message) => new GenericResult(false, message);

    /// <summary>
    /// Creates a failed result with an IRecMessage.
    /// </summary>
    /// <param name="message">The failure message object.</param>
    /// <returns>A failed result.</returns>
    public static IGenericResult Failure(IGenericMessage message) => new GenericResult(false, message);

    /// <summary>
    /// Creates a failed result with any object that implements IRecMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that implements IRecMessage.</typeparam>
    /// <param name="message">The failure message object.</param>
    /// <returns>A failed result.</returns>
    public static IGenericResult Failure<TMessage>(TMessage message) where TMessage : IGenericMessage => new GenericResult(false, message);

    /// <summary>
    /// Creates a failed result with multiple IRecMessages.
    /// </summary>
    /// <param name="messages">The failure message objects.</param>
    /// <returns>A failed result.</returns>
    public static IGenericResult Failure(IEnumerable<IGenericMessage> messages) => new GenericResult(false, messages);

    /// <summary>
    /// Creates a failed result with multiple IRecMessages.
    /// </summary>
    /// <param name="messages">The failure message objects.</param>
    /// <returns>A failed result.</returns>
    public static IGenericResult Failure(params IGenericMessage[] messages) => new GenericResult(false, messages);

}

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
            : GenericResult<TNew>.Failure(CurrentMessage ?? "Operation failed");
    }

    /// <inheritdoc/>
    public T Match<T>(Func<TResult, T> success, Func<string, T> failure)
    {
        if (success == null)
            throw new ArgumentNullException(nameof(success));
        if (failure == null)
            throw new ArgumentNullException(nameof(failure));

        return IsSuccess ? success(Value) : failure(CurrentMessage ?? string.Empty);
    }
}
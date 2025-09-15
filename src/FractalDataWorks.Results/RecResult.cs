using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Messages;

namespace FractalDataWorks.Results;

/// <summary>
/// Basic implementation of IFdwResult.
/// </summary>
public class FdwResult : IFdwResult
{
    private readonly List<IFractalMessage> _messages = [];

    /// <summary>
    /// Constructor for FdwResult
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="message"></param>
    protected FdwResult(bool isSuccess, string? message = null)
    {
        IsSuccess = isSuccess;
        if (!string.IsNullOrEmpty(message))
        {
            _messages.Add(new FractalMessage(message!));
        }
    }

    /// <summary>
    /// Constructor for FdwResult with IFractalMessage
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="message"></param>
    protected FdwResult(bool isSuccess, IFractalMessage? message)
    {
        IsSuccess = isSuccess;
        if (message != null)
        {
            _messages.Add(message);
        }
    }

    /// <summary>
    /// Constructor for FdwResult with multiple messages
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="messages"></param>
    protected FdwResult(bool isSuccess, IEnumerable<IFractalMessage>? messages)
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
    public IReadOnlyList<IFractalMessage> Messages => _messages.AsReadOnly();

    /// <summary>
    /// Gets the first message or an empty string
    /// </summary>
    public string Message => _messages.FirstOrDefault()?.Message ?? string.Empty;

    /// <summary>
    /// Adds a message to this result's message collection.
    /// </summary>
    /// <param name="message">The message to add.</param>
    protected void AddMessage(IFractalMessage message)
    {
        _messages.Add(message);
    }

    /// <summary>
    /// Adds multiple messages to this result's message collection.
    /// </summary>
    /// <param name="messages">The messages to add.</param>
    protected void AddMessages(IEnumerable<IFractalMessage> messages)
    {
        _messages.AddRange(messages);
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static IFdwResult Success() => new FdwResult(true);

    /// <summary>
    /// Creates a successful result with a message.
    /// </summary>
    /// <param name="message">The success message.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult Success(string message) => new FdwResult(true, message);

    /// <summary>
    /// Creates a successful result with an IFractalMessage.
    /// </summary>
    /// <param name="message">The success message object.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult Success(IFractalMessage message) => new FdwResult(true, message);

    /// <summary>
    /// Creates a successful result with any object that implements IFractalMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that implements IFractalMessage.</typeparam>
    /// <param name="message">The success message object.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult Success<TMessage>(TMessage message) where TMessage : IFractalMessage => new FdwResult(true, message);

    /// <summary>
    /// Creates a successful result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The success message objects.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult Success(IEnumerable<IFractalMessage> messages) => new FdwResult(true, messages);

    /// <summary>
    /// Creates a successful result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The success message objects.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult Success(params IFractalMessage[] messages) => new FdwResult(true, messages);

    /// <summary>
    /// Creates a failed result with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult Failure(string message) => new FdwResult(false, message);

    /// <summary>
    /// Creates a failed result with an IFractalMessage.
    /// </summary>
    /// <param name="message">The failure message object.</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult Failure(IFractalMessage message) => new FdwResult(false, message);

    /// <summary>
    /// Creates a failed result with any object that implements IFractalMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that implements IFractalMessage.</typeparam>
    /// <param name="message">The failure message object.</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult Failure<TMessage>(TMessage message) where TMessage : IFractalMessage => new FdwResult(false, message);

    /// <summary>
    /// Creates a failed result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The failure message objects.</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult Failure(IEnumerable<IFractalMessage> messages) => new FdwResult(false, messages);

    /// <summary>
    /// Creates a failed result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The failure message objects.</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult Failure(params IFractalMessage[] messages) => new FdwResult(false, messages);

}

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

    private FdwResult(bool isSuccess, TResult value, IFractalMessage? message) : base(isSuccess, message)
    {
        _value = value;
        _hasValue = isSuccess;
    }

    private FdwResult(bool isSuccess, TResult value, IEnumerable<IFractalMessage>? messages) : base(isSuccess, messages)
    {
        _value = value;
        _hasValue = isSuccess;
    }

    /// <summary>
    /// Provides collection of messages associated with the result
    /// </summary>
    public new IReadOnlyList<IFractalMessage> Messages => base.Messages;

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
    public static IFdwResult<TResult> Success(TResult value, IFractalMessage message) => new FdwResult<TResult>(true, value, message);

    /// <summary>
    /// Creates a successful result with a value and any object that implements IFractalMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that implements IFractalMessage.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="message">The success message object.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult<TResult> Success<TMessage>(TResult value, TMessage message) where TMessage : IFractalMessage => new FdwResult<TResult>(true, value, message);

    /// <summary>
    /// Creates a successful result with a value and multiple IFractalMessages.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="messages">The success message objects.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult<TResult> Success(TResult value, IEnumerable<IFractalMessage> messages) => new FdwResult<TResult>(true, value, messages);

    /// <summary>
    /// Creates a successful result with a value and multiple IFractalMessages.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="messages">The success message objects.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult<TResult> Success(TResult value, params IFractalMessage[] messages) => new FdwResult<TResult>(true, value, messages);

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
    public new static IFdwResult<TResult> Failure(IFractalMessage message) => new FdwResult<TResult>(false, default!, message);

    /// <summary>
    /// Creates a failed result with any object that implements IFractalMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that implements IFractalMessage.</typeparam>
    /// <param name="message">The failure message object.</param>
    /// <returns>A failed result.</returns>
    public new static IFdwResult<TResult> Failure<TMessage>(TMessage message) where TMessage : IFractalMessage => new FdwResult<TResult>(false, default!, message);

    /// <summary>
    /// Creates a failed result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The failure message objects.</param>
    /// <returns>A failed result.</returns>
    public new static IFdwResult<TResult> Failure(IEnumerable<IFractalMessage> messages) => new FdwResult<TResult>(false, default!, messages);

    /// <summary>
    /// Creates a failed result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The failure message objects.</param>
    /// <returns>A failed result.</returns>
    public new static IFdwResult<TResult> Failure(params IFractalMessage[] messages) => new FdwResult<TResult>(false, default!, messages);


    /// <inheritdoc/>
    public IGenericResult<TNew> Map<TNew>(Func<TResult, TNew> mapper)
    {
        if (mapper == null)
            throw new ArgumentNullException(nameof(mapper));

        return IsSuccess
            ? (IGenericResult<TNew>)FdwResult<TNew>.Success(mapper(Value))
            : FdwResult<TNew>.Failure(Message);
    }

    /// <inheritdoc/>
    public T Match<T>(Func<TResult, T> success, Func<string, T> failure)
    {
        if (success == null)
            throw new ArgumentNullException(nameof(success));
        if (failure == null)
            throw new ArgumentNullException(nameof(failure));

        return IsSuccess ? success(Value) : failure(Message);
    }
}

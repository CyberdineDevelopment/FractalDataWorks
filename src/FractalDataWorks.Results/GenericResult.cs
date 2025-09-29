using System;
using System.Collections.Generic;
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
            _messages.Add(new FractalMessage(message!));
        }
    }

    /// <summary>
    /// Constructor for GenericResult with IFractalMessage
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
    /// Gets the first message or an empty string
    /// </summary>
    public string? Message => _messages.FirstOrDefault()?.Message;

    /// <summary>
    /// Adds a message to this result's message collection.
    /// </summary>
    /// <param name="message">The message to add.</param>
    protected void AddMessage(IGenericMessage message)
    {
        _messages.Add(message);
    }

    /// <summary>
    /// Adds multiple messages to this result's message collection.
    /// </summary>
    /// <param name="messages">The messages to add.</param>
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
    /// Creates a successful result with an IFractalMessage.
    /// </summary>
    /// <param name="message">The success message object.</param>
    /// <returns>A successful result.</returns>
    public static IGenericResult Success(IGenericMessage message) => new GenericResult(true, message);

    /// <summary>
    /// Creates a successful result with any object that implements IFractalMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that implements IFractalMessage.</typeparam>
    /// <param name="message">The success message object.</param>
    /// <returns>A successful result.</returns>
    public static IGenericResult Success<TMessage>(TMessage message) where TMessage : IGenericMessage => new GenericResult(true, message);

    /// <summary>
    /// Creates a successful result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The success message objects.</param>
    /// <returns>A successful result.</returns>
    public static IGenericResult Success(IEnumerable<IGenericMessage> messages) => new GenericResult(true, messages);

    /// <summary>
    /// Creates a successful result with multiple IFractalMessages.
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
    /// Creates a failed result with an IFractalMessage.
    /// </summary>
    /// <param name="message">The failure message object.</param>
    /// <returns>A failed result.</returns>
    public static IGenericResult Failure(IGenericMessage message) => new GenericResult(false, message);

    /// <summary>
    /// Creates a failed result with any object that implements IFractalMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that implements IFractalMessage.</typeparam>
    /// <param name="message">The failure message object.</param>
    /// <returns>A failed result.</returns>
    public static IGenericResult Failure<TMessage>(TMessage message) where TMessage : IGenericMessage => new GenericResult(false, message);

    /// <summary>
    /// Creates a failed result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The failure message objects.</param>
    /// <returns>A failed result.</returns>
    public static IGenericResult Failure(IEnumerable<IGenericMessage> messages) => new GenericResult(false, messages);

    /// <summary>
    /// Creates a failed result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The failure message objects.</param>
    /// <returns>A failed result.</returns>
    public static IGenericResult Failure(params IGenericMessage[] messages) => new GenericResult(false, messages);

}
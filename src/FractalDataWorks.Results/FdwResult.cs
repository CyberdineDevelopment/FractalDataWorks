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
    private readonly List<IFdwMessage> _messages = [];

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
    protected FdwResult(bool isSuccess, IFdwMessage? message)
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
    protected FdwResult(bool isSuccess, IEnumerable<IFdwMessage>? messages)
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
    public IReadOnlyList<IFdwMessage> Messages => _messages.AsReadOnly();

    /// <summary>
    /// Gets the first message or an empty string
    /// </summary>
    public string? Message => _messages.FirstOrDefault()?.Message;

    /// <summary>
    /// Adds a message to this result's message collection.
    /// </summary>
    /// <param name="message">The message to add.</param>
    protected void AddMessage(IFdwMessage message)
    {
        _messages.Add(message);
    }

    /// <summary>
    /// Adds multiple messages to this result's message collection.
    /// </summary>
    /// <param name="messages">The messages to add.</param>
    protected void AddMessages(IEnumerable<IFdwMessage> messages)
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
    public static IFdwResult Success(IFdwMessage message) => new FdwResult(true, message);

    /// <summary>
    /// Creates a successful result with any object that implements IFractalMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that implements IFractalMessage.</typeparam>
    /// <param name="message">The success message object.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult Success<TMessage>(TMessage message) where TMessage : IFdwMessage => new FdwResult(true, message);

    /// <summary>
    /// Creates a successful result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The success message objects.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult Success(IEnumerable<IFdwMessage> messages) => new FdwResult(true, messages);

    /// <summary>
    /// Creates a successful result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The success message objects.</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult Success(params IFdwMessage[] messages) => new FdwResult(true, messages);

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
    public static IFdwResult Failure(IFdwMessage message) => new FdwResult(false, message);

    /// <summary>
    /// Creates a failed result with any object that implements IFractalMessage.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that implements IFractalMessage.</typeparam>
    /// <param name="message">The failure message object.</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult Failure<TMessage>(TMessage message) where TMessage : IFdwMessage => new FdwResult(false, message);

    /// <summary>
    /// Creates a failed result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The failure message objects.</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult Failure(IEnumerable<IFdwMessage> messages) => new FdwResult(false, messages);

    /// <summary>
    /// Creates a failed result with multiple IFractalMessages.
    /// </summary>
    /// <param name="messages">The failure message objects.</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult Failure(params IFdwMessage[] messages) => new FdwResult(false, messages);

}
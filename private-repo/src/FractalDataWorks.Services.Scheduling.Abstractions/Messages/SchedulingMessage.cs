using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

/// <summary>
/// Base class for all scheduling-related messages.
/// </summary>
[MessageCollection("SchedulingMessages")]
public abstract class SchedulingMessage : MessageTemplate<MessageSeverity>, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulingMessage"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this message type.</param>
    /// <param name="name">The name of this message type.</param>
    /// <param name="severity">The severity level of the message.</param>
    /// <param name="message">The human-readable message text.</param>
    /// <param name="code">The message code or identifier (optional).</param>
    protected SchedulingMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, "Scheduling", message, code, null, null) { }
}
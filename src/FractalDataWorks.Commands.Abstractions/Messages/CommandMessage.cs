using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Commands.Abstractions.Messages;

/// <summary>
/// Base class for all command-related messages.
/// </summary>
/// <remarks>
/// Command messages provide structured information about command operations,
/// including errors, warnings, and informational messages.
/// </remarks>
[MessageCollection("CommandMessages")]
public abstract class CommandMessage : MessageTemplate<MessageSeverity>, IGenericMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandMessage"/> class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="name">The message name.</param>
    /// <param name="severity">The message severity.</param>
    /// <param name="message">The message text.</param>
    /// <param name="code">The optional error code.</param>
    protected CommandMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, "Command", message, code, null, null)
    {
    }
}
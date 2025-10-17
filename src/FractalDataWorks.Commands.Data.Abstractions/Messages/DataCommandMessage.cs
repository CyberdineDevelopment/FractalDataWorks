using FractalDataWorks.Messages;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Base class for data command messages.
/// </summary>
public abstract class DataCommandMessage : MessageTemplate<MessageSeverity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandMessage"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for this message.</param>
    /// <param name="name">Name of the message.</param>
    /// <param name="severity">Severity level.</param>
    /// <param name="message">Message text (can include format placeholders).</param>
    /// <param name="code">Unique code for programmatic handling.</param>
    protected DataCommandMessage(
        int id,
        string name,
        MessageSeverity severity,
        string message,
        string? code = null)
        : base(id, name, severity, message, code, "DataCommands", null, null)
    {
    }
}

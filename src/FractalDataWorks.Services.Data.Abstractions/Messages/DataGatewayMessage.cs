using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Services.Data.Abstractions.Messages;

/// <summary>
/// Base class for DataGateway-related messages.
/// </summary>
public abstract class DataGatewayMessage : MessageTemplate<MessageSeverity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGatewayMessage"/> class.
    /// </summary>
    /// <param name="id">The unique message identifier.</param>
    /// <param name="name">The message name.</param>
    /// <param name="severity">The message severity level.</param>
    /// <param name="message">The message template text.</param>
    /// <param name="code">Optional message code for categorization.</param>
    protected DataGatewayMessage(
        int id,
        string name,
        MessageSeverity severity,
        string message,
        string? code = null)
        : base(id, name, severity, "DataGateway", message, code, null, null)
    {
    }
}

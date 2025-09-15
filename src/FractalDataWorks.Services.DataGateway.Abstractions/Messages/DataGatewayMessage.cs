using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Messages;

/// <summary>
/// Base class for all data provider-related messages.
/// </summary>
[MessageCollection("DataGatewayMessages")]
public abstract class DataGatewayMessage : MessageTemplate<MessageSeverity>, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGatewayMessage"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this message type.</param>
    /// <param name="name">The name of this message type.</param>
    /// <param name="severity">The severity level of the message.</param>
    /// <param name="message">The human-readable message text.</param>
    /// <param name="code">The message code or identifier (optional).</param>
    protected DataGatewayMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, "DataGateway", message, code, null, null) { }
}
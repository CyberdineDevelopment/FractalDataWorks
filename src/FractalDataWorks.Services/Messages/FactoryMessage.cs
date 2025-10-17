using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Base class for all factory-related messages in the Services component.
/// </summary>
[MessageCollection("FactoryMessages")]
public abstract class FactoryMessage : MessageTemplate<MessageSeverity>, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FactoryMessage"/> class.
    /// </summary>
    /// <param name="id">The message code.</param>
    /// <param name="name">The message name.</param>
    /// <param name="severity">The message severity.</param>
    /// <param name="message">The message text.</param>
    /// <param name="code">The optional error code.</param>
    protected FactoryMessage(int id, string name, MessageSeverity severity, 
                                string message, string? code = null)
        : base(id, name, severity, "Services", message, code, null, null) { }
}
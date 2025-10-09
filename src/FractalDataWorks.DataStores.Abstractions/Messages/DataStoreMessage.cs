using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.DataStores.Abstractions.Messages;

/// <summary>
/// Base class for all data store-related messages.
/// </summary>
[MessageCollection("DataStoreMessages")]
public abstract class DataStoreMessage : MessageTemplate<MessageSeverity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataStoreMessage"/> class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="name">The message name.</param>
    /// <param name="severity">The message severity level.</param>
    /// <param name="message">The message text.</param>
    /// <param name="code">The optional message code.</param>
    protected DataStoreMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, "DataStore", message, code, null, null) { }
}

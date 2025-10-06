using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.DataStores.Abstractions.Messages;

[MessageCollection("DataStoreMessages")]
public abstract class DataStoreMessage : MessageTemplate<MessageSeverity>
{
    protected DataStoreMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, "DataStore", message, code, null, null) { }
}

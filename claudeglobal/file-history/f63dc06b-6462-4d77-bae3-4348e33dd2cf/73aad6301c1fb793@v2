using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.DataStores.Abstractions.Messages;

[Message("ConnectionTypeNullOrEmpty")]
public sealed class ConnectionTypeNullOrEmptyMessage : DataStoreMessage
{
    public ConnectionTypeNullOrEmptyMessage()
        : base(1001, "ConnectionTypeNullOrEmpty", MessageSeverity.Error,
               "Connection type cannot be null or empty", "DS_CONN_TYPE_NULL") { }
}

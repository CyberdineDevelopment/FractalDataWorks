using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Data.DataStores.Abstractions.Messages;

/// <summary>
/// Error message indicating that a connection type is null or empty.
/// </summary>
[Message("ConnectionTypeNullOrEmpty")]
public sealed class ConnectionTypeNullOrEmptyMessage : DataStoreMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTypeNullOrEmptyMessage"/> class.
    /// </summary>
    public ConnectionTypeNullOrEmptyMessage()
        : base(1001, "ConnectionTypeNullOrEmpty", MessageSeverity.Error,
               "Connection type cannot be null or empty", "DS_CONN_TYPE_NULL") { }
}

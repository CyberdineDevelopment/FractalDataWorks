using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Data.DataStores.Abstractions.Messages;

/// <summary>
/// Message indicating that a connection test failed.
/// </summary>
[Message("ConnectionTestFailed")]
public sealed class ConnectionTestFailedMessage : DataStoreMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTestFailedMessage"/> class.
    /// </summary>
    public ConnectionTestFailedMessage()
        : base(
            id: 1003,
            name: "ConnectionTestFailed",
            severity: MessageSeverity.Error,
            message: "Connection test failed: {0}",
            code: "DS_CONN_TEST_FAILED")
    {
    }
}

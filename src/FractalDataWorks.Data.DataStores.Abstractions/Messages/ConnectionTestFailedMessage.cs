using FractalDataWorks.Messages;

namespace FractalDataWorks.Data.DataStores.Abstractions.Messages;

/// <summary>
/// Message indicating that a connection test failed.
/// </summary>
public sealed class ConnectionTestFailedMessage : DataStoreMessage
{
    /// <summary>
    /// Gets the singleton instance of this message.
    /// </summary>
    public static ConnectionTestFailedMessage Instance { get; } = new();

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

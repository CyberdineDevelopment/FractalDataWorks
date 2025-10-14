using FractalDataWorks.Messages;

namespace FractalDataWorks.Services.Data.Abstractions.Messages;

/// <summary>
/// Message indicating that a requested connection was not found.
/// </summary>
public sealed class ConnectionNotFoundMessage : DataGatewayMessage
{
    /// <summary>
    /// Gets the singleton instance of this message.
    /// </summary>
    public static ConnectionNotFoundMessage Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionNotFoundMessage"/> class.
    /// </summary>
    public ConnectionNotFoundMessage()
        : base(
            id: 1001,
            name: "ConnectionNotFound",
            severity: MessageSeverity.Error,
            message: "Connection '{0}' not found",
            code: "DG_CONN_NOT_FOUND")
    {
    }
}

using FractalDataWorks.Messages;

namespace FractalDataWorks.Data.DataStores.Abstractions.Messages;

/// <summary>
/// Message indicating that path discovery failed.
/// </summary>
public sealed class PathDiscoveryFailedMessage : DataStoreMessage
{
    /// <summary>
    /// Gets the singleton instance of this message.
    /// </summary>
    public static PathDiscoveryFailedMessage Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PathDiscoveryFailedMessage"/> class.
    /// </summary>
    public PathDiscoveryFailedMessage()
        : base(
            id: 1004,
            name: "PathDiscoveryFailed",
            severity: MessageSeverity.Error,
            message: "Path discovery failed: {0}",
            code: "DS_PATH_DISC_FAILED")
    {
    }
}

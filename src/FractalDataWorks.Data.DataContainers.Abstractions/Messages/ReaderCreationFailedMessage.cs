using FractalDataWorks.Messages;

namespace FractalDataWorks.Data.DataContainers.Abstractions.Messages;

/// <summary>
/// Message indicating that reader creation failed for a container.
/// </summary>
public sealed class ReaderCreationFailedMessage : ContainerMessage
{
    /// <summary>
    /// Gets the singleton instance of this message.
    /// </summary>
    public static ReaderCreationFailedMessage Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReaderCreationFailedMessage"/> class.
    /// </summary>
    public ReaderCreationFailedMessage()
        : base(
            id: 2004,
            name: "ReaderCreationFailed",
            severity: MessageSeverity.Error,
            message: "Failed to create reader for container '{0}': {1}",
            code: "CONT_READER_FAILED")
    {
    }
}

using FractalDataWorks.Messages;

namespace FractalDataWorks.Data.DataStores.Abstractions.Messages;

/// <summary>
/// Message indicating that configuration update failed.
/// </summary>
public sealed class ConfigurationUpdateFailedMessage : DataStoreMessage
{
    /// <summary>
    /// Gets the singleton instance of this message.
    /// </summary>
    public static ConfigurationUpdateFailedMessage Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationUpdateFailedMessage"/> class.
    /// </summary>
    public ConfigurationUpdateFailedMessage()
        : base(
            id: 1006,
            name: "ConfigurationUpdateFailed",
            severity: MessageSeverity.Error,
            message: "Configuration update failed: {0}",
            code: "DS_CONFIG_UPDATE_FAILED")
    {
    }
}

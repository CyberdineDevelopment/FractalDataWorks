using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Data.DataStores.Abstractions.Messages;

/// <summary>
/// Message indicating that configuration update failed.
/// </summary>
[Message("ConfigurationUpdateFailed")]
public sealed class ConfigurationUpdateFailedMessage : DataStoreMessage
{
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

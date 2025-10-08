using FractalDataWorks.Messages;

namespace FractalDataWorks.Services.DomainName.Abstractions.Messages;

/// <summary>
/// Message indicating that the configuration is null.
/// </summary>
public sealed class ConfigurationNullMessage : DomainNameMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationNullMessage"/> class.
    /// </summary>
    public ConfigurationNullMessage(string details = "")
        : base(
            id: 1001,
            name: "ConfigurationNull",
            severity: MessageSeverity.Error,
            message: "Configuration cannot be null",
            code: "DOMAIN_CONFIG_NULL",
            source: "DomainNameService",
            details: details)
    {
    }
}

using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Authentication.Abstractions.Messages;

/// <summary>
/// Message indicating that configuration was null.
/// </summary>
[Message("ConfigurationNull")]
public sealed class ConfigurationNullMessage : AuthenticationMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationNullMessage"/> class.
    /// </summary>
    public ConfigurationNullMessage()
        : base(1001, "ConfigurationNull", MessageSeverity.Error,
               "Configuration cannot be null", "AUTH_CONFIG_NULL") { }
}

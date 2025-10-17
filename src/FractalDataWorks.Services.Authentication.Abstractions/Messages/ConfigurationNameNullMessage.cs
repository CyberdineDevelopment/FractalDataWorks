using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Authentication.Abstractions.Messages;

/// <summary>
/// CurrentMessage indicating that configuration name was null or empty.
/// </summary>
[Message("ConfigurationNameNull")]
public sealed class ConfigurationNameNullMessage : AuthenticationMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationNameNullMessage"/> class.
    /// </summary>
    public ConfigurationNameNullMessage()
        : base(1004, "ConfigurationNameNull", MessageSeverity.Error,
               "Configuration name cannot be null or empty", "AUTH_CONFIG_NAME_NULL") { }
}

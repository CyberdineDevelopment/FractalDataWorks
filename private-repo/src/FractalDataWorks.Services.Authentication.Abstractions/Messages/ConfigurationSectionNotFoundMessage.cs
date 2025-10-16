using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Authentication.Abstractions.Messages;

/// <summary>
/// CurrentMessage indicating that the configuration section was not found.
/// </summary>
[Message("ConfigurationSectionNotFound")]
public sealed class ConfigurationSectionNotFoundMessage : AuthenticationMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationSectionNotFoundMessage"/> class.
    /// </summary>
    /// <param name="configurationName">The configuration section name that was not found.</param>
    public ConfigurationSectionNotFoundMessage(string configurationName)
        : base(1005, "ConfigurationSectionNotFound", MessageSeverity.Error,
               $"Configuration section not found: Authentication:{configurationName}", "AUTH_CONFIG_NOT_FOUND") { }
}

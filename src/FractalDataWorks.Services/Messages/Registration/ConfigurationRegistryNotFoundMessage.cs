using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// CurrentMessage indicating that a configuration registry was not found in the DI container.
/// </summary>
[Message("ConfigurationRegistryNotFound")]
public sealed class ConfigurationRegistryNotFoundMessage : RegistrationMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationRegistryNotFoundMessage"/> class.
    /// </summary>
    public ConfigurationRegistryNotFoundMessage() 
        : base(2202, "ConfigurationRegistryNotFound", MessageSeverity.Error, 
               "Configuration registry not found in DI container", "REG003") { }

    /// <summary>
    /// Initializes a new instance with the configuration type.
    /// </summary>
    /// <param name="configurationType">The type of configuration registry not found.</param>
    public ConfigurationRegistryNotFoundMessage(string configurationType) 
        : base(2202, "ConfigurationRegistryNotFound", MessageSeverity.Error, 
               $"Configuration registry for type '{configurationType}' not found in DI container", "REG003") { }
}
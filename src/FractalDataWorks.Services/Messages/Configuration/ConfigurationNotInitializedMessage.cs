using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Message indicating that the configuration registry has not been initialized.
/// </summary>
[Message("ConfigurationNotInitialized")]
public sealed class ConfigurationNotInitializedMessage : ServiceMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationNotInitializedMessage"/> class.
    /// </summary>
    public ConfigurationNotInitializedMessage() 
        : base(1006, "ConfigurationNotInitialized", MessageSeverity.Error, 
               "Configuration registry not initialized", "CONFIG_NOT_INITIALIZED") { }

    /// <summary>
    /// Initializes a new instance with a custom message.
    /// </summary>
    /// <param name="customMessage">Custom message describing the configuration issue.</param>
    public ConfigurationNotInitializedMessage(string customMessage) 
        : base(1006, "ConfigurationNotInitialized", MessageSeverity.Error, 
               customMessage, "CONFIG_NOT_INITIALIZED") { }

}

using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Services.SecretManagers.AzureKeyVault;

/// <summary>
/// Message indicating that a command was null.
/// </summary>
[Message(CollectionName = "AzureKeyVaultMessages", Name = "CommandNull")]
public sealed class CommandNullMessage : AzureKeyVaultMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandNullMessage"/> class.
    /// </summary>
    public CommandNullMessage()
        : base(MessageSeverity.Error, "Command cannot be null.", "AKV_COMMAND_NULL")
    {
    }
}

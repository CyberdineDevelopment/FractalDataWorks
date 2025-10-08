using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Services.SecretManagers.AzureKeyVault;

/// <summary>
/// Message indicating that a command type is not supported.
/// </summary>
[Message(CollectionName = "AzureKeyVaultMessages", Name = "UnsupportedCommandType")]
public sealed class UnsupportedCommandTypeMessage : AzureKeyVaultMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedCommandTypeMessage"/> class.
    /// </summary>
    public UnsupportedCommandTypeMessage(string commandType)
        : base(MessageSeverity.Error, $"Unsupported command type: {commandType}", "AKV_UNSUPPORTED_COMMAND")
    {
        Data["CommandType"] = commandType;
    }
}

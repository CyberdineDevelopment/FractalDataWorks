using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Services.SecretManagers.AzureKeyVault;

/// <summary>
/// Base class for Azure Key Vault service messages.
/// </summary>
[MessageCollection("AzureKeyVaultMessages")]
public abstract class AzureKeyVaultMessageBase : MessageTemplate<MessageSeverity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultMessageBase"/> class.
    /// </summary>
    protected AzureKeyVaultMessageBase(MessageSeverity severity, string message, string code)
        : base(severity, message, code, nameof(AzureKeyVaultService))
    {
    }
}

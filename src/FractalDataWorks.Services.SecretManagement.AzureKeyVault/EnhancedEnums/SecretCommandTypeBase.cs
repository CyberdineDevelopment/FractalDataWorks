using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Results;
using FractalDataWorks.Services.SecretManagement.Abstractions;
using FractalDataWorks.Services.SecretManagement;
using FractalDataWorks.Services.SecretManagement.Commands;

namespace FractalDataWorks.Services.SecretManagement.AzureKeyVault.EnhancedEnums;

/// <summary>
/// Base class for all Azure Key Vault secret command types.
/// Encapsulates command execution logic and validation in a type-safe, extensible pattern.
/// </summary>
public abstract class SecretCommandTypeBase : EnumOptionBase<SecretCommandTypeBase>, ISecretCommandType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretCommandTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this command type.</param>
    /// <param name="name">The name of this command type.</param>
    /// <param name="isSecretModifying">Whether this command modifies secrets.</param>
    protected SecretCommandTypeBase(int id, string name, bool isSecretModifying = false) : base(id, name) 
    { 
        IsSecretModifying = isSecretModifying;
    }
    
    /// <summary>
    /// Gets a value indicating whether this command modifies secrets.
    /// Following Enhanced Enum pattern: embed behavior in the enum options.
    /// </summary>
    public bool IsSecretModifying { get; }

    /// <summary>
    /// Executes this command type against the Azure Key Vault service.
    /// </summary>
    /// <param name="service">The Azure Key Vault service instance.</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    public abstract Task<IFdwResult> Execute(
        AzureKeyVaultService service, 
        ISecretCommand command, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Validates that the command has required parameters for this operation.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns><c>true</c> if the command is valid; otherwise, <c>false</c>.</returns>
    public abstract bool Validate(ISecretCommand command);

    /// <summary>
    /// Gets the command type that this secret command type handles.
    /// Used for type-based lookups in the collection.
    /// </summary>
    public abstract Type CommandType { get; }
}
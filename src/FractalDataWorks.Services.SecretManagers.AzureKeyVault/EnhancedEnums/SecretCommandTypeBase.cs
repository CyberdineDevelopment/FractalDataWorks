using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Results;
using FractalDataWorks.Services.SecretManagers.Abstractions;
using FractalDataWorks.Services.SecretManager;
using FractalDataWorks.Services.SecretManagers.Commands;
using ISecretManagerCommand = FractalDataWorks.Services.SecretManagers.Commands.ISecretManagerCommand;

namespace FractalDataWorks.Services.SecretManagers.AzureKeyVault.EnhancedEnums;

/// <summary>
/// Base class for all Azure Key Vault secret managementCommand types.
/// Encapsulates managementCommand execution logic and validation in a type-safe, extensible pattern.
/// </summary>
public abstract class SecretCommandTypeBase : EnumOptionBase<SecretCommandTypeBase>, ISecretCommandType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretCommandTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this managementCommand type.</param>
    /// <param name="name">The name of this managementCommand type.</param>
    /// <param name="isSecretModifying">Whether this managementCommand modifies secrets.</param>
    protected SecretCommandTypeBase(int id, string name, bool isSecretModifying = false) : base(id, name) 
    { 
        IsSecretModifying = isSecretModifying;
    }
    
    /// <summary>
    /// Gets a value indicating whether this managementCommand modifies secrets.
    /// Following Enhanced Enum pattern: embed behavior in the enum options.
    /// </summary>
    public bool IsSecretModifying { get; }

    /// <summary>
    /// Executes this managementCommand type against the Azure Key Vault service.
    /// </summary>
    /// <param name="service">The Azure Key Vault service instance.</param>
    /// <param name="managementCommandmanagementCommand to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    public abstract Task<IFdwResult> Execute(
        AzureKeyVaultService service, 
        ISecretManagerCommand managementCommand, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Validates that the managementCommand has required parameters for this operation.
    /// </summary>
    /// <param name="managementCommandmanagementCommand to validate.</param>
    /// <returns><c>true</c> if the managementCommand is valid; otherwise, <c>false</c>.</returns>
    public abstract bool Validate(ISecretManagerCommand managementCommand);

    /// <summary>
    /// Gets the managementCommand type that this secret managementCommand type handles.
    /// Used for type-based lookups in the collection.
    /// </summary>
    public abstract Type CommandType { get; }
}
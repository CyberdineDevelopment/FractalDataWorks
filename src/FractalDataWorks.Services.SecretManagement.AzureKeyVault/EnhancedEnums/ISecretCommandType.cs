using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.SecretManagement.Abstractions;
using FractalDataWorks.Services.SecretManagement;

namespace FractalDataWorks.Services.SecretManagement.AzureKeyVault.EnhancedEnums;

/// <summary>
/// Interface for Azure Key Vault secret command types.
/// Defines the contract for command execution, validation, and type lookup.
/// </summary>
public interface ISecretCommandType
{
    /// <summary>
    /// Gets the unique identifier for this command type.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Gets the name of this command type.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the command type that this secret command type handles.
    /// Used by EnumLookup to generate ByType method for type-based lookups.
    /// </summary>
    Type CommandType { get; }
    
    /// <summary>
    /// Gets a value indicating whether this command modifies secrets.
    /// Following Enhanced Enum pattern: embed behavior in the enum options.
    /// </summary>
    bool IsSecretModifying { get; }

    /// <summary>
    /// Executes this command type against the Azure Key Vault service.
    /// </summary>
    /// <param name="service">The Azure Key Vault service instance.</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    Task<IFdwResult> Execute(AzureKeyVaultService service, ISecretCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Validates that the command has required parameters for this operation.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns><c>true</c> if the command is valid; otherwise, <c>false</c>.</returns>
    bool Validate(ISecretCommand command);
}
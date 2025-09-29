using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.SecretManagers.Abstractions;
using FractalDataWorks.Services.SecretManager;

namespace FractalDataWorks.Services.SecretManagers.AzureKeyVault.EnhancedEnums;

/// <summary>
/// Interface for Azure Key Vault secret managementCommand types.
/// Defines the contract for managementCommand execution, validation, and type lookup.
/// </summary>
public interface ISecretCommandType
{
    /// <summary>
    /// Gets the unique identifier for this managementCommand type.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Gets the name of this managementCommand type.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the managementCommand type that this secret managementCommand type handles.
    /// Used by EnumLookup to generate ByType method for type-based lookups.
    /// </summary>
    Type CommandType { get; }
    
    /// <summary>
    /// Gets a value indicating whether this managementCommand modifies secrets.
    /// Following Enhanced Enum pattern: embed behavior in the enum options.
    /// </summary>
    bool IsSecretModifying { get; }

    /// <summary>
    /// Executes this managementCommand type against the Azure Key Vault service.
    /// </summary>
    /// <param name="service">The Azure Key Vault service instance.</param>
    /// <param name="command">The managementCommand to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    Task<IGenericResult> Execute(AzureKeyVaultService service, ISecretCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Validates that the managementCommand has required parameters for this operation.
    /// </summary>
    /// <param name="command">The managementCommand to validate.</param>
    /// <returns><c>true</c> if the managementCommand is valid; otherwise, <c>false</c>.</returns>
    bool Validate(ISecretCommand command);
}
using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Results;
using FractalDataWorks.Services.SecretManagers.Abstractions;
using FractalDataWorks.Services.SecretManager;
using FractalDataWorks.Services.SecretManagers.Commands;
using ISecretManagerCommand = FractalDataWorks.Services.SecretManagers.Commands.ISecretManagerCommand;

namespace FractalDataWorks.Services.SecretManagers.AzureKeyVault.EnhancedEnums;

/// <summary>
/// List secrets managementCommand type for retrieving all secret names from Azure Key Vault.
/// </summary>
[TypeOption(typeof(SecretCommandTypes), "List")]
public class ListSecrets : SecretCommandTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListSecrets"/> managementCommand type.
    /// </summary>
    public ListSecrets() : base(4, nameof(ListSecrets)) { }

    /// <summary>
    /// Executes the list secrets operation.
    /// </summary>
    public override async Task<IGenericResult> Execute(
        AzureKeyVaultService service, 
        ISecretManagerCommand managementCommand, 
        CancellationToken cancellationToken)
    {
        return await service.Execute(managementCommand, cancellationToken);
    }

    /// <summary>
    /// Validates the managementCommand - no specific parameters required for listing.
    /// </summary>
    public override bool Validate(ISecretManagerCommand managementCommand)
    {
        return true; // No specific validation required for list operation
    }

    /// <summary>
    /// Gets the managementCommand type that this secret managementCommand type handles.
    /// </summary>
    public override Type CommandType => typeof(ListSecretsManagementCommand);
}
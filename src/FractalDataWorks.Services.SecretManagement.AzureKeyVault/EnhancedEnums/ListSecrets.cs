using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Results;
using FractalDataWorks.Services.SecretManagement.Abstractions;
using FractalDataWorks.Services.SecretManagement;
using FractalDataWorks.Services.SecretManagement.Commands;
using FractalDataWorks.Services.SecretManagement.Abstractions;
using FractalDataWorks.Services.SecretManagement;

namespace FractalDataWorks.Services.SecretManagement.AzureKeyVault.EnhancedEnums;

/// <summary>
/// List secrets command type for retrieving all secret names from Azure Key Vault.
/// </summary>
[EnumOption]
public class ListSecrets : SecretCommandTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListSecrets"/> command type.
    /// </summary>
    public ListSecrets() : base(4, nameof(ListSecrets)) { }

    /// <summary>
    /// Executes the list secrets operation.
    /// </summary>
    public override async Task<IFdwResult> Execute(
        AzureKeyVaultService service, 
        ISecretCommand command, 
        CancellationToken cancellationToken)
    {
        return await service.Execute(command, cancellationToken);
    }

    /// <summary>
    /// Validates the command - no specific parameters required for listing.
    /// </summary>
    public override bool Validate(ISecretCommand command)
    {
        return true; // No specific validation required for list operation
    }

    /// <summary>
    /// Gets the command type that this secret command type handles.
    /// </summary>
    public override Type CommandType => typeof(ListSecretsCommand);
}
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
/// Delete secret managementCommand type for removing secrets from Azure Key Vault.
/// </summary>
[TypeOption(typeof(SecretCommandTypes), "Delete")]
public class DeleteSecret : SecretCommandTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteSecret"/> managementCommand type.
    /// </summary>
    public DeleteSecret() : base(3, nameof(DeleteSecret), isSecretModifying: true) { }

    /// <summary>
    /// Executes the delete secret operation.
    /// </summary>
    public override async Task<IGenericResult> Execute(
        AzureKeyVaultService service, 
        ISecretManagerCommand managementCommand, 
        CancellationToken cancellationToken)
    {
        return await service.Execute(managementCommand, cancellationToken);
    }

    /// <summary>
    /// Validates that the managementCommand has a SecretKey parameter.
    /// </summary>
    public override bool Validate(ISecretManagerCommand managementCommand)
    {
        return !string.IsNullOrWhiteSpace(managementCommand.SecretKey);
    }

    /// <summary>
    /// Gets the managementCommand type that this secret managementCommand type handles.
    /// </summary>
    public override Type CommandType => typeof(DeleteSecretManagerCommand);
}
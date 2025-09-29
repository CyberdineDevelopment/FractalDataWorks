using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Results;
using FractalDataWorks.Services.SecretManagers.Abstractions;
using FractalDataWorks.Services.SecretManager;
using FractalDataWorks.Services.SecretManagers.Commands;
using FractalDataWorks.Services.SecretManagers.Abstractions;
using FractalDataWorks.Services.SecretManager;
using ISecretManagerCommand = FractalDataWorks.Services.SecretManagers.Commands.ISecretManagerCommand;

namespace FractalDataWorks.Services.SecretManagers.AzureKeyVault.EnhancedEnums;

/// <summary>
/// Set secret managementCommand type for storing secret values in Azure Key Vault.
/// </summary>
[TypeOption(typeof(SecretCommandTypes), "Set")]
public class SetSecret : SecretCommandTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetSecret"/> managementCommand type.
    /// </summary>
    public SetSecret() : base(2, nameof(SetSecret), isSecretModifying: true) { }

    /// <summary>
    /// Executes the set secret operation.
    /// </summary>
    public override async Task<IGenericResult> Execute(
        AzureKeyVaultService service, 
        ISecretManagerCommand managementCommand, 
        CancellationToken cancellationToken)
    {
        return await service.Execute(managementCommand, cancellationToken);
    }

    /// <summary>
    /// Validates that the managementCommand has SecretKey and SecretValue parameters.
    /// </summary>
    public override bool Validate(ISecretManagerCommand managementCommand)
    {
        return !string.IsNullOrWhiteSpace(managementCommand.SecretKey) &&
               managementCommand.Parameters.TryGetValue("SecretValue", out var secretValue) &&
               !string.IsNullOrWhiteSpace(secretValue?.ToString());
    }

    /// <summary>
    /// Gets the managementCommand type that this secret managementCommand type handles.
    /// </summary>
    public override Type CommandType => typeof(SetSecretManagerCommand);
}
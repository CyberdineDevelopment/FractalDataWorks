using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Results;
using FractalDataWorks.Services.SecretManagement.Abstractions;
using FractalDataWorks.Services.SecretManagement;
using FractalDataWorks.Services.SecretManagement.Commands;
using FractalDataWorks.Services.SecretManagement.Abstractions;
using FractalDataWorks.Services.SecretManagement;

namespace FractalDataWorks.Services.SecretManagement.AzureKeyVault.EnhancedEnums;

/// <summary>
/// Set secret command type for storing secret values in Azure Key Vault.
/// </summary>
[TypeOption(typeof(SecretCommandTypes), "Set")]
public class SetSecret : SecretCommandTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetSecret"/> command type.
    /// </summary>
    public SetSecret() : base(2, nameof(SetSecret), isSecretModifying: true) { }

    /// <summary>
    /// Executes the set secret operation.
    /// </summary>
    public override async Task<IFdwResult> Execute(
        AzureKeyVaultService service, 
        ISecretCommand command, 
        CancellationToken cancellationToken)
    {
        return await service.Execute(command, cancellationToken);
    }

    /// <summary>
    /// Validates that the command has SecretKey and SecretValue parameters.
    /// </summary>
    public override bool Validate(ISecretCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.SecretKey) &&
               command.Parameters.TryGetValue("SecretValue", out var secretValue) &&
               !string.IsNullOrWhiteSpace(secretValue?.ToString());
    }

    /// <summary>
    /// Gets the command type that this secret command type handles.
    /// </summary>
    public override Type CommandType => typeof(SetSecretCommand);
}
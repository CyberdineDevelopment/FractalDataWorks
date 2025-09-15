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
/// Get secret command type for retrieving secret values from Azure Key Vault.
/// </summary>
[EnumOption]
public class GetSecret : SecretCommandTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSecret"/> command type.
    /// </summary>
    public GetSecret() : base(1, nameof(GetSecret)) { }

    /// <summary>
    /// Executes the get secret operation.
    /// </summary>
    public override async Task<IFdwResult> Execute(
        AzureKeyVaultService service, 
        ISecretCommand command, 
        CancellationToken cancellationToken)
    {
        return await service.Execute(command, cancellationToken);
    }

    /// <summary>
    /// Validates that the command has a SecretKey parameter.
    /// </summary>
    public override bool Validate(ISecretCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.SecretKey);
    }

    /// <summary>
    /// Gets the command type that this secret command type handles.
    /// </summary>
    public override Type CommandType => typeof(GetSecretCommand);
}
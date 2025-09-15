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
/// Get secret versions command type for retrieving all versions of a specific secret.
/// </summary>
[EnumOption]
public class GetSecretVersions : SecretCommandTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSecretVersions"/> command type.
    /// </summary>
    public GetSecretVersions() : base(5, nameof(GetSecretVersions)) { }

    /// <summary>
    /// Executes the get secret versions operation.
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
    public override Type CommandType => typeof(GetSecretVersionsCommand);
}
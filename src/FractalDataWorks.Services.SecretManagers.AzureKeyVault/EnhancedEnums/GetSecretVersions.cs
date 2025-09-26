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
/// Get secret versions managementCommand type for retrieving all versions of a specific secret.
/// </summary>
[TypeOption(typeof(SecretCommandTypes), "Versions")]
public class GetSecretVersions : SecretCommandTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSecretVersions"/> managementCommand type.
    /// </summary>
    public GetSecretVersions() : base(5, nameof(GetSecretVersions)) { }

    /// <summary>
    /// Executes the get secret versions operation.
    /// </summary>
    public override async Task<IFdwResult> Execute(
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
    public override Type CommandType => typeof(GetSecretManagerVersionsCommand);
}
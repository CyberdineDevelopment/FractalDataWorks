using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions.Commands;
using FractalDataWorks.Services.SecretManagers.Abstractions;

namespace FractalDataWorks.Services.SecretManager;

/// <summary>
/// Base class for secret management services.
/// </summary>
/// <typeparam name="TSecretCommand">The secret managementCommand type.</typeparam>
/// <typeparam name="TSecretManagerConfiguration">The secret management configuration type.</typeparam>
/// <typeparam name="TSecretManagerService">The concrete secret management service type for logging category.</typeparam>
public abstract class SecretManagerServiceBase<TSecretCommand, TSecretManagerConfiguration, TSecretManagerService>
    : ServiceBase<TSecretCommand, TSecretManagerConfiguration, TSecretManagerService>, ISecretManager
    where TSecretCommand : IGenericCommand, ISecretManagerCommand
    where TSecretManagerConfiguration : ISecretManagerConfiguration
    where TSecretManagerService : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretManagerServiceBase{TSecretCommand, TSecretManagerConfiguration, TSecretManagerService}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for the concrete service type.</param>
    /// <param name="configuration">The secret management configuration.</param>
    protected SecretManagerServiceBase(ILogger<TSecretManagerService> logger, TSecretManagerConfiguration configuration)
        : base(logger, configuration)
    {
    }

    /// <inheritdoc/>
    public abstract Task<IGenericResult<object?>> Execute(ISecretManagerCommand managementCommand, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<IGenericResult<TResult>> Execute<TResult>(ISecretManagerCommand<TResult> managementCommand, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<IGenericResult> ExecuteBatch(IReadOnlyList<ISecretManagerCommand> commands, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract IGenericResult ValidateCommand(ISecretManagerCommand managementCommand);
}
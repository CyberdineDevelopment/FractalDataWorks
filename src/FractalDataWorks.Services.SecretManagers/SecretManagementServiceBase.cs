using Microsoft.Extensions.Logging;
using FractalDataWorks.Services;
using FractalDataWorks.Services.SecretManagers.Abstractions;
using FractalDataWorks.Services.SecretManagers.Commands;
using ISecretManagerCommand = FractalDataWorks.Services.SecretManagers.Commands;

namespace FractalDataWorks.Services.SecretManager;

/// <summary>
/// Base class for secret management services.
/// </summary>
/// <typeparam name="TSecretCommand">The secret managementCommand type.</typeparam>
/// <typeparam name="TSecretManagerConfiguration">The secret management configuration type.</typeparam>
/// <typeparam name="TSecretManagerService">The concrete secret management service type for logging category.</typeparam>
public abstract class SecretManagerServiceBase<TSecretCommand, TSecretManagerConfiguration, TSecretManagerService> 
    : ServiceBase<TSecretCommand, TSecretManagerConfiguration, TSecretManagerService>, ISecretManagerService<TSecretCommand>
    where TSecretCommand : Abstractions.ISecretManagerCommand
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
}
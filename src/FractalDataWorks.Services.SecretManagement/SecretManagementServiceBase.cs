using Microsoft.Extensions.Logging;
using FractalDataWorks.Services;
using FractalDataWorks.Services.SecretManagement.Abstractions;
using FractalDataWorks.Services.SecretManagement.Commands;

namespace FractalDataWorks.Services.SecretManagement;

/// <summary>
/// Base class for secret management services.
/// </summary>
/// <typeparam name="TSecretCommand">The secret command type.</typeparam>
/// <typeparam name="TSecretManagementConfiguration">The secret management configuration type.</typeparam>
/// <typeparam name="TSecretManagementService">The concrete secret management service type for logging category.</typeparam>
public abstract class SecretManagementServiceBase<TSecretCommand, TSecretManagementConfiguration, TSecretManagementService> 
    : ServiceBase<TSecretCommand, TSecretManagementConfiguration, TSecretManagementService>, ISecretService<TSecretCommand>
    where TSecretCommand : ISecretCommand
    where TSecretManagementConfiguration : ISecretManagementConfiguration
    where TSecretManagementService : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretManagementServiceBase{TSecretCommand, TSecretManagementConfiguration, TSecretManagementService}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for the concrete service type.</param>
    /// <param name="configuration">The secret management configuration.</param>
    protected SecretManagementServiceBase(ILogger<TSecretManagementService> logger, TSecretManagementConfiguration configuration) 
        : base(logger, configuration) 
    { 
    }
}
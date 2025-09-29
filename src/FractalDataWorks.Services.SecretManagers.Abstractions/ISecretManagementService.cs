using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.SecretManagers.Abstractions;

/// <summary>
/// Non-generic marker interface for secret management services.
/// </summary>
public interface ISecretManagerService : IGenericService
{
    /// <summary>
    /// Executes a secret manager command and returns a typed result.
    /// Integrates with the command pattern for unified service execution.
    /// </summary>
    /// <typeparam name="TResult">The type of result expected from the command execution.</typeparam>
    /// <param name="command">The secret manager command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the command execution to complete.</param>
    /// <returns>A task that represents the asynchronous command execution operation.</returns>
    Task<IGenericResult<TResult>> Execute<TResult>(ISecretManagerCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a secret manager command without returning a specific result type.
    /// Integrates with the command pattern for unified service execution.
    /// </summary>
    /// <param name="command">The secret manager command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the command execution to complete.</param>
    /// <returns>A task that represents the asynchronous command execution operation.</returns>
    Task<IGenericResult> Execute(ISecretManagerCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for secret service implementations that handle specific secret storage backends.
/// Defines the contract for services like AWS Secrets Manager, Azure Key Vault, HashiCorp Vault, etc.
/// </summary>
/// <typeparam name="TSecretCommand">The secret managementCommand type.</typeparam>
/// <remarks>
/// Secret services are responsible for implementing the actual communication with
/// secret storage systems. They handle service-specific authentication, API calls,
/// error handling, and result formatting.
/// </remarks>
public interface ISecretManagerService<TSecretCommand> : ISecretManagerService, IGenericService<TSecretCommand>
    where TSecretCommand : ISecretManagerCommand
{
}
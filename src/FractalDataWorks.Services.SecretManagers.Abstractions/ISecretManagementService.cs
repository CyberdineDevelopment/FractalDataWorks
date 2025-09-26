using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.SecretManagers.Commands;

namespace FractalDataWorks.Services.SecretManagers.Abstractions;

/// <summary>
/// Non-generic marker interface for secret management services.
/// </summary>
public interface ISecretManagerService : IFdwService
{
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
public interface ISecretManagerService<TSecretCommand> : ISecretManagerService, IFdwService<TSecretCommand>
    where TSecretCommand : Commands.ISecretManagerCommand
{
}
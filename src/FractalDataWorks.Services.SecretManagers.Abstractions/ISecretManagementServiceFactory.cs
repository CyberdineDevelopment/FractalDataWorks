using FractalDataWorks.Abstractions;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.SecretManagers.Abstractions;

/// <summary>
/// Non-generic marker interface for secret management service factories.
/// </summary>
public interface ISecretManagerServiceFactory : IServiceFactory
{
}

/// <summary>
/// Interface for secret management service factories that create specific secret management service implementations.
/// </summary>
/// <typeparam name="TSecretService">The secret management service type to create.</typeparam>
public interface ISecretManagerServiceFactory<TSecretService> : ISecretManagerServiceFactory, IServiceFactory<TSecretService>
    where TSecretService : ISecretManager
{
}

/// <summary>
/// Interface for secret management service factories that create secret management services with configuration.
/// </summary>
/// <typeparam name="TSecretService">The secret management service type to create.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the secret management service.</typeparam>
public interface ISecretManagerServiceFactory<TSecretService, TConfiguration> : ISecretManagerServiceFactory<TSecretService>, IServiceFactory<TSecretService, TConfiguration>
    where TSecretService : ISecretManager
    where TConfiguration : ISecretManagerConfiguration
{
}
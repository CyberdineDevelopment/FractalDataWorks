using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Non-generic marker interface for secret service factories.
/// </summary>
public interface ISecretServiceFactory : IServiceFactory
{
}

/// <summary>
/// Interface for secret service factories that create specific secret service implementations.
/// </summary>
/// <typeparam name="TSecretService">The secret service type to create.</typeparam>
public interface ISecretServiceFactory<TSecretService> : ISecretServiceFactory, IServiceFactory<TSecretService>
    where TSecretService : class, ISecretService
{
}

/// <summary>
/// Interface for secret service factories that create secret services with configuration.
/// </summary>
/// <typeparam name="TSecretService">The secret service type to create.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the secret service.</typeparam>
public interface ISecretServiceFactory<TSecretService, TConfiguration> : ISecretServiceFactory<TSecretService>, IServiceFactory<TSecretService, TConfiguration>
    where TSecretService : class, ISecretService
    where TConfiguration : class, ISecretManagementConfiguration
{
}
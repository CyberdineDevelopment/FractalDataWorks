using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.SecretManagers.Abstractions;

/// <summary>
/// Interface for secret management service types.
/// Defines the contract for secret management service type implementations that integrate
/// with the service framework's dependency injection and configuration systems.
/// </summary>
/// <typeparam name="TService">The secret management service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the secret management service.</typeparam>
/// <typeparam name="TFactory">The factory type for creating secret management service instances.</typeparam>
public interface ISecretManagerType<TService, TFactory, TConfiguration> : IServiceType<TService, TFactory, TConfiguration>
    where TService : ISecretManager
    where TConfiguration : ISecretManagerConfiguration
    where TFactory : ISecretManagerServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Gets the secret store types supported by this provider.
    /// </summary>
    string[] SupportedSecretStores { get; }

    /// <summary>
    /// Gets the secret types supported by this provider.
    /// </summary>
    IReadOnlyList<string> SupportedSecretTypes { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports secret rotation.
    /// </summary>
    bool SupportsRotation { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports secret versioning.
    /// </summary>
    bool SupportsVersioning { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports bulk operations.
    /// </summary>
    bool SupportsBulkOperations { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports encryption at rest.
    /// </summary>
    bool SupportsEncryptionAtRest { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports access auditing.
    /// </summary>
    bool SupportsAuditing { get; }

    /// <summary>
    /// Gets the maximum secret size supported by this provider (in bytes).
    /// </summary>
    int MaxSecretSize { get; }

    /// <summary>
    /// Gets the cloud provider or platform for this secret management service.
    /// </summary>
    string CloudProvider { get; }
}

/// <summary>
/// Non-generic interface for secret management service types.
/// Provides a common base for all secret management types regardless of generic parameters.
/// </summary>
public interface ISecretManagerType : IServiceType<ISecretManager, ISecretManagerServiceFactory<ISecretManager, ISecretManagerConfiguration>, ISecretManagerConfiguration>
{
    /// <summary>
    /// Gets the secret store types supported by this provider.
    /// </summary>
    string[] SupportedSecretStores { get; }

    /// <summary>
    /// Gets the secret types supported by this provider.
    /// </summary>
    IReadOnlyList<string> SupportedSecretTypes { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports secret rotation.
    /// </summary>
    bool SupportsRotation { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports secret versioning.
    /// </summary>
    bool SupportsVersioning { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports bulk operations.
    /// </summary>
    bool SupportsBulkOperations { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports encryption at rest.
    /// </summary>
    bool SupportsEncryptionAtRest { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports access auditing.
    /// </summary>
    bool SupportsAuditing { get; }

    /// <summary>
    /// Gets the maximum secret size supported by this provider (in bytes).
    /// </summary>
    int MaxSecretSize { get; }

    /// <summary>
    /// Gets the cloud provider or platform for this secret management service.
    /// </summary>
    string CloudProvider { get; }

}
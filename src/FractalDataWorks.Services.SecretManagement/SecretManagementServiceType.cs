using System;
using System.Collections.Generic;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.SecretManagement.Abstractions;

namespace FractalDataWorks.Services.SecretManagement;

/// <summary>
/// Base class for secret management service type definitions.
/// Provides Enhanced Enum functionality and service registration for secret management services.
/// </summary>
/// <typeparam name="TSelf">The concrete secret management service type.</typeparam>
/// <typeparam name="TService">The service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type.</typeparam>
/// <typeparam name="TFactory">The factory type.</typeparam>
public abstract class SecretManagementServiceType<TSelf, TService, TConfiguration, TFactory> :
    ServiceType<TSelf, TService, TConfiguration, TFactory>
    where TSelf : SecretManagementServiceType<TSelf, TService, TConfiguration, TFactory>, IEnumOption<TSelf>
    where TService : class, IFdwService
    where TConfiguration : class, IFdwConfiguration
    where TFactory : class, IServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretManagementServiceType{TSelf, TService, TConfiguration, TFactory}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="description">The description of this service type.</param>
    protected SecretManagementServiceType(int id, string name, string description )
        : base(id, name, description,"SecretManagers")
    {
    }

    /// <summary>
    /// Gets the list of secret store types supported by this service.
    /// </summary>
    /// <value>An array of supported secret store type names.</value>
    public abstract string[] SupportedSecretStores { get; }

    /// <summary>
    /// Gets the name of the underlying provider used by this service.
    /// </summary>
    /// <value>The provider name.</value>
    public abstract string ProviderName { get; }

    /// <summary>
    /// Gets the list of supported authentication methods for this service.
    /// </summary>
    /// <value>A read-only list of authentication method names.</value>
    public abstract IReadOnlyList<string> SupportedAuthenticationMethods { get; }

    /// <summary>
    /// Gets the list of operations supported by this secret management service.
    /// </summary>
    /// <value>A read-only list of operation names.</value>
    public abstract IReadOnlyList<string> SupportedOperations { get; }

    /// <summary>
    /// Gets the priority of this service type for selection when multiple types are available.
    /// </summary>
    /// <value>The priority value. Higher values indicate higher priority.</value>
    public abstract int Priority { get; }

    /// <summary>
    /// Gets a value indicating whether this service supports encryption at rest.
    /// </summary>
    /// <value><c>true</c> if encryption at rest is supported; otherwise, <c>false</c>.</value>
    public abstract bool SupportsEncryptionAtRest { get; }

    /// <summary>
    /// Gets a value indicating whether this service supports audit logging.
    /// </summary>
    /// <value><c>true</c> if audit logging is supported; otherwise, <c>false</c>.</value>
    public abstract bool SupportsAuditLogging { get; }
}
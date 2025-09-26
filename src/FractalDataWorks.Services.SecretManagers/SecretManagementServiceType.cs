using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.SecretManagers.Abstractions;

namespace FractalDataWorks.Services.SecretManager;

/// <summary>
/// Base class for secret management service type definitions.
/// Provides Enhanced Enum functionality and service registration for secret management services.
/// </summary>
/// <typeparam name="TSelf">The concrete secret management service type.</typeparam>
/// <typeparam name="TService">The service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type.</typeparam>
/// <typeparam name="TFactory">The factory type.</typeparam>
public abstract class SecretManagerServiceType<TSelf, TService, TConfiguration, TFactory> :
    ServiceTypeBase<TService, TConfiguration, TFactory>
    where TSelf : SecretManagerServiceType<TSelf, TService, TConfiguration, TFactory>, IEnumOption<TSelf>
    where TService : class, IFdwService
    where TConfiguration : class, IFdwConfiguration
    where TFactory : class, IServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretManagerServiceType{TSelf, TService, TConfiguration, TFactory}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="sectionName">The configuration section name for appsettings.json.</param>
    /// <param name="displayName">The display name for this service type.</param>
    /// <param name="description">The description of this service type.</param>
    protected SecretManagerServiceType(
        int id,
        string name,
        string sectionName,
        string displayName,
        string description)
        : base(id, name, sectionName, displayName, description, "SecretManagers")
    {
    }

    /// <summary>
    /// Registers the services required by this service type with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    public abstract override void Register(IServiceCollection services);

    /// <summary>
    /// Configures the service type using the provided configuration.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public abstract override void Configure(IConfiguration configuration);

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
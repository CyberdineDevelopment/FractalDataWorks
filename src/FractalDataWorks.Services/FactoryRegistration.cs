using FractalDataWorks.Services.Abstractions;
using System;
using System.Collections.Generic;
using FractalDataWorks.Abstractions;

namespace FractalDataWorks.Services;

/// <summary>
/// Represents a factory registration with its associated service lifetimeBase.
/// Used by ServiceFactoryProvider to manage factory instances and their DI registration preferences.
/// </summary>
/// <remarks>
/// This class encapsulates both the factory instance and its desired service lifetimeBase,
/// enabling the DI container to register services with the appropriate scope
/// (Transient, Scoped, or Singleton) based on configuration.
/// </remarks>
public sealed class FactoryRegistration
{
    /// <summary>
    /// Gets or sets the service factory instance.
    /// </summary>
    /// <value>The factory responsible for creating service instances.</value>
    public IServiceFactory Factory { get; set; } = null!;

    /// <summary>
    /// Gets or sets the service lifetimeBase for DI container registration.
    /// </summary>
    /// <value>The lifetimeBase scope that determines when instances are created and disposed.</value>
    /// <remarks>
    /// This lifetimeBase is used when registering the service with the DI container:
    /// - Transient: New instance every time (most expensive, most isolated)
    /// - Scoped: One instance per scope/request (balanced approach)
    /// - Singleton: Single instance for application lifetimeBase (most efficient, shared state)
    /// </remarks>
    public IServiceLifetime Lifetime { get; set; } = ServiceLifetimes.Scoped;

    /// <summary>
    /// Gets or sets the service type name for identification.
    /// </summary>
    /// <value>The unique name identifying the service type (e.g., "MsSql", "Rest", "AzureEntra").</value>
    /// <remarks>
    /// This name is used as the key for factory lookup and corresponds to the
    /// service type names used in configurations and service discovery.
    /// </remarks>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the priority for this factory registration.
    /// </summary>
    /// <value>The priority value used when multiple factories could handle the same service type.</value>
    /// <remarks>
    /// Higher values indicate higher priority. When multiple factories are available
    /// for a service type, the system can use this to determine the preferred factory.
    /// </remarks>
    public int Priority { get; set; } = 50;

    /// <summary>
    /// Gets or sets additional metadata for this factory registration.
    /// </summary>
    /// <value>A dictionary containing custom metadata about this factory.</value>
    /// <remarks>
    /// Can be used to store additional information about the factory's capabilities,
    /// configuration requirements, or other metadata needed by the application.
    /// </remarks>
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>(StringComparer.Ordinal);
}
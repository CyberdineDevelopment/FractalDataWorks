using Microsoft.Extensions.DependencyInjection;
using System;
namespace FractalDataWorks.Services;

/// <summary>
/// Options for service registration.
/// </summary>
public class ServiceRegistrationOptions
{
    /// <summary>
    /// Gets or sets the service lifetimeBase.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

    /// <summary>
    /// Gets or sets whether to register as the primary implementation.
    /// </summary>
    public bool RegisterAsPrimary { get; set; } = true;

    /// <summary>
    /// Gets or sets the configuration section name.
    /// </summary>
    public string ConfigurationSection { get; set; } = string.Empty;
}
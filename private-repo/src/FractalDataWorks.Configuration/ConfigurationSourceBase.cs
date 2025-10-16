using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;using System.Threading.Tasks;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Base implementation of a configuration source.
/// </summary>
/// <ExcludeFromTest>Abstract base class for configuration sources with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage]
public abstract class ConfigurationSourceBase : IGenericConfigurationSource
{

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationSourceBase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="name">The name of this configuration source.</param>
    protected ConfigurationSourceBase(ILogger logger, string name)
    {
        _logger = logger;
        Name = name;
    }

    /// <summary>
    /// Gets the name of this configuration source.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this source supports write operations.
    /// </summary>
    public abstract bool IsWritable { get; }

    /// <summary>
    /// Gets a value indicating whether this source supports automatic reload.
    /// </summary>
    public abstract bool SupportsReload { get; }

    /// <summary>
    /// Occurs when the configuration source changes.
    /// </summary>
    public event EventHandler<ConfigurationSourceChangedEventArgs>? Changed;

    /// <summary>
    /// Loads configurations from this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to load.</typeparam>
    /// <returns>A task containing the loaded configurations.</returns>
    public abstract Task<IGenericResult<IEnumerable<TConfiguration>>> Load<TConfiguration>()
        where TConfiguration : IGenericConfiguration;

    /// <summary>
    /// Saves a configuration to this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to save.</typeparam>
    /// <param name="configuration">The configuration to save.</param>
    /// <returns>A task containing the save operation result.</returns>
    /// <remarks>
    /// Implementations should check IsWritable and return failure if the source is read-only.
    /// </remarks>
    public abstract Task<IGenericResult<TConfiguration>> Save<TConfiguration>(TConfiguration configuration)
        where TConfiguration : IGenericConfiguration;

    /// <summary>
    /// Deletes a configuration from this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to delete.</typeparam>
    /// <param name="id">The ID of the configuration to delete.</param>
    /// <returns>A task containing the delete operation result.</returns>
    /// <remarks>
    /// Implementations should check IsWritable and return failure if the source is read-only.
    /// </remarks>
    public abstract Task<IGenericResult<NonResult>> Delete<TConfiguration>(int id)
        where TConfiguration : IGenericConfiguration;

    /// <summary>
    /// Raises the Changed event.
    /// </summary>
    /// <param name="changeType">The type of change that occurred.</param>
    /// <param name="configurationType">The type of configuration that changed.</param>
    /// <param name="configurationId">The ID of the configuration that changed.</param>
    protected void OnChanged(
        ConfigurationChangeTypeBase changeType,
        Type configurationType,
        int? configurationId = null)
    {
        var args = new ConfigurationSourceChangedEventArgs(changeType, configurationType, configurationId);
        Changed?.Invoke(this, args);

        ConfigurationSourceBaseLog.ConfigurationChanged(_logger, Name, changeType, configurationType.Name);
    }
}

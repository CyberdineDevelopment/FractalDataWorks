using FractalDataWorks.Results;
using System;
using System.Collections.Generic;
using System.Threading;using System.Threading.Tasks;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Defines the contract for configuration sources in the Rec framework.
/// </summary>
public interface IGenericConfigurationSource
{
    /// <summary>
    /// Gets the name of this configuration source.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this source supports write operations.
    /// </summary>
    bool IsWritable { get; }

    /// <summary>
    /// Gets a value indicating whether this source supports automatic reload.
    /// </summary>
    bool SupportsReload { get; }

    /// <summary>
    /// Loads configurations from this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to load.</typeparam>
    /// <returns>A task containing the loaded configurations.</returns>
    Task<IGenericResult<IEnumerable<TConfiguration>>> Load<TConfiguration>()
        where TConfiguration : IGenericConfiguration;

    /// <summary>
    /// Saves a configuration to this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to save.</typeparam>
    /// <param name="configuration">The configuration to save.</param>
    /// <returns>A task containing the save operation result.</returns>
    Task<IGenericResult<TConfiguration>> Save<TConfiguration>(TConfiguration configuration)
        where TConfiguration : IGenericConfiguration;

    /// <summary>
    /// Deletes a configuration from this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to delete.</typeparam>
    /// <param name="id">The ID of the configuration to delete.</param>
    /// <returns>A task containing the delete operation result.</returns>
    Task<IGenericResult<NonResult>> Delete<TConfiguration>(int id)
        where TConfiguration : IGenericConfiguration;

    /// <summary>
    /// Occurs when the configuration source changes.
    /// </summary>
    event EventHandler<ConfigurationSourceChangedEventArgs>? Changed;
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Data.DataStores.Abstractions;

namespace FractalDataWorks.Data.DataContainers.Abstractions;

/// <summary>
/// Represents configuration settings specific to a container format.
/// </summary>
/// <remarks>
/// IContainerConfiguration provides format-specific settings that control
/// how data is read from or written to the container. Each container type
/// will have its own configuration implementation.
/// </remarks>
public interface IContainerConfiguration
{
    /// <summary>
    /// Gets the container type this configuration applies to.
    /// </summary>
    /// <value>The container type identifier.</value>
    string ContainerType { get; }

    /// <summary>
    /// Gets configuration settings as key-value pairs.
    /// </summary>
    /// <value>
    /// Configuration settings specific to the container format.
    /// For example, CSV might have "Delimiter", "HasHeaders", "Encoding".
    /// </value>
    IReadOnlyDictionary<string, object> Settings { get; }

    /// <summary>
    /// Validates that the configuration is valid for the container type.
    /// </summary>
    /// <returns>A result indicating whether the configuration is valid.</returns>
    IGenericResult Validate();

    /// <summary>
    /// Gets a configuration value as a specific type.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The default value if the key is not found.</param>
    /// <returns>The configuration value cast to the specified type.</returns>
    T GetValue<T>(string key, T defaultValue = default!);
}
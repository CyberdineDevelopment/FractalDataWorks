using System.Collections.Generic;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Registry for managing configuration objects.
/// </summary>
public interface IConfigurationRegistry
{
    /// <summary>
    /// Registers a configuration object.
    /// </summary>
    /// <typeparam name="T">The type of configuration.</typeparam>
    /// <param name="configuration">The configuration object to register.</param>
    void Register<T>(T configuration) where T : IFractalConfiguration;

    /// <summary>
    /// Gets a configuration object by type.
    /// </summary>
    /// <typeparam name="T">The type of configuration.</typeparam>
    /// <returns>The configuration object if found; otherwise, null.</returns>
    T? Get<T>() where T : IFractalConfiguration;

    /// <summary>
    /// Checks if a configuration type is registered.
    /// </summary>
    /// <typeparam name="T">The type of configuration.</typeparam>
    /// <returns>True if the configuration type is registered; otherwise, false.</returns>
    bool IsRegistered<T>() where T : IFractalConfiguration;
}

/// <summary>
/// Generic registry for managing configuration objects of a specific type.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration managed by this registry.</typeparam>
public interface IConfigurationRegistry<TConfiguration> where TConfiguration : IFractalConfiguration
{
    /// <summary>
    /// Gets a configuration by ID.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <returns>The configuration if found; otherwise, null.</returns>
    TConfiguration? Get(int id);
    
    /// <summary>
    /// Gets a configuration by name.
    /// </summary>
    /// <param name="name">The configuration name.</param>
    /// <returns>The configuration if found; otherwise, null.</returns>
    TConfiguration? GetByName(string name);

    /// <summary>
    /// Gets all configurations.
    /// </summary>
    /// <returns>All available configurations.</returns>
    IEnumerable<TConfiguration> GetAll();

    /// <summary>
    /// Tries to get a configuration by ID.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <param name="configuration">The configuration if found; otherwise, null.</param>
    /// <returns>True if the configuration was found; otherwise, false.</returns>
    bool TryGet(int id, out TConfiguration? configuration);
    
    /// <summary>
    /// Checks if a configuration exists by ID.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <returns>True if the configuration exists; otherwise, false.</returns>
    bool Contains(int id);
    
    /// <summary>
    /// Checks if a configuration exists by name.
    /// </summary>
    /// <param name="name">The configuration name.</param>
    /// <returns>True if the configuration exists; otherwise, false.</returns>
    bool ContainsByName(string name);
}

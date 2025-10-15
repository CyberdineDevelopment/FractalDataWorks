using System;
using System.Collections.Generic;
using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Services;

/// <summary>
/// Implementation of IConfigurationRegistry that wraps a list of configurations.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration managed by this registry.</typeparam>
public sealed class ConfigurationRegistryCore<TConfiguration> : IConfigurationRegistry<TConfiguration>
    where TConfiguration : IGenericConfiguration
{
    private readonly Dictionary<int, TConfiguration> _configurationsById;
    private readonly Dictionary<string, TConfiguration> _configurationsByName;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationRegistryCore{TConfiguration}"/> class.
    /// </summary>
    /// <param name="configurations">The collection of configurations to manage.</param>
    public ConfigurationRegistryCore(IEnumerable<TConfiguration> configurations)
    {
        _configurationsById = new Dictionary<int, TConfiguration>();
        _configurationsByName = new Dictionary<string, TConfiguration>(StringComparer.OrdinalIgnoreCase);
        
        if (configurations != null)
        {
            foreach (var config in configurations)
            {
                _configurationsById[config.Id] = config;
                if (!string.IsNullOrEmpty(config.Name))
                {
                    _configurationsByName[config.Name] = config;
                }
            }
        }
    }

    /// <summary>
    /// Gets a configuration by ID.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <returns>The configuration if found; otherwise, default.</returns>
    public TConfiguration? Get(int id)
    {
        return _configurationsById.TryGetValue(id, out var config) ? config : default;
    }
    
    /// <summary>
    /// Gets a configuration by name.
    /// </summary>
    /// <param name="name">The configuration name.</param>
    /// <returns>The configuration if found; otherwise, default.</returns>
    public TConfiguration? GetByName(string name)
    {
        return string.IsNullOrEmpty(name) ? default : 
            _configurationsByName.TryGetValue(name, out var config) ? config : default;
    }

    /// <summary>
    /// Gets all configurations.
    /// </summary>
    /// <returns>All available configurations.</returns>
    public IEnumerable<TConfiguration> GetAll()
    {
        return _configurationsById.Values;
    }

    /// <summary>
    /// Tries to get a configuration by ID.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <param name="configuration">The configuration if found; otherwise, null.</param>
    /// <returns>True if the configuration was found; otherwise, false.</returns>
    public bool TryGet(int id, out TConfiguration? configuration)
    {
        return _configurationsById.TryGetValue(id, out configuration);
    }
    
    /// <summary>
    /// Checks if a configuration exists by ID.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <returns>True if the configuration exists; otherwise, false.</returns>
    public bool Contains(int id)
    {
        return _configurationsById.ContainsKey(id);
    }
    
    /// <summary>
    /// Checks if a configuration exists by name.
    /// </summary>
    /// <param name="name">The configuration name.</param>
    /// <returns>True if the configuration exists; otherwise, false.</returns>
    public bool ContainsByName(string name)
    {
        return !string.IsNullOrEmpty(name) && _configurationsByName.ContainsKey(name);
    }
}

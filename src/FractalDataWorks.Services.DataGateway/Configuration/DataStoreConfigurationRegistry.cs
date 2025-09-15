using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Services;
using FractalDataWorks.Services.DataGateway.Abstractions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FractalDataWorks.Services.DataGateway.Configuration;

/// <summary>
/// Thread-safe registry for DataStoreConfiguration instances with hot-reload support.
/// </summary>
/// <remarks>
/// This registry provides centralized access to data store configurations with the following features:
/// - Named store lookup by connection name
/// - Provider-specific configuration retrieval  
/// - Hot-reload support through IOptionsMonitor
/// - Thread-safe access to configurations
/// - Comprehensive error handling and logging
/// - Efficient lookup using StringComparer.Ordinal
/// 
/// The registry automatically updates when configuration changes are detected and
/// maintains internal caches for performance while ensuring thread safety.
/// </remarks>
public sealed class DataStoreConfigurationRegistry : IConfigurationRegistry<DataStoreConfiguration>, IDisposable
{
    private readonly ILogger<DataStoreConfigurationRegistry> _logger;
    private readonly IOptionsMonitor<DataStoreConfiguration[]> _optionsMonitor;
    private readonly ConcurrentDictionary<string, DataStoreConfiguration> _configurationsByName;
    private readonly ConcurrentDictionary<string, List<DataStoreConfiguration>> _configurationsByProvider;
    private readonly System.Threading.Lock _refreshLock = new();
    private readonly IDisposable? _changeTokenRegistration;
    private DataStoreConfiguration[] _allConfigurations = [];
    private DataStoreConfiguration? _defaultConfiguration;
    private bool _disposed;

    // LoggerMessage.Define delegates for high-performance logging
    private static readonly Action<ILogger, int, Exception?> LogRegistryInitialized =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(1, "RegistryInitialized"),
            "DataStoreConfigurationRegistry initialized with {ConfigCount} configurations");

    private static readonly Action<ILogger, string, Exception?> LogConfigurationRetrieved =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(2, "ConfigurationRetrieved"),
            "Retrieved configuration for store {StoreName}");

    private static readonly Action<ILogger, string, Exception?> LogConfigurationNotFound =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(3, "ConfigurationNotFound"),
            "No configuration found for store {StoreName}");

    private static readonly Action<ILogger, int, string, Exception?> LogProviderConfigurationsRetrieved =
        LoggerMessage.Define<int, string>(
            LogLevel.Debug,
            new EventId(4, "ProviderConfigurationsRetrieved"),
            "Retrieved {ConfigCount} configurations for provider {ProviderType}");

    private static readonly Action<ILogger, string, Exception?> LogProviderConfigurationsNotFound =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(5, "ProviderConfigurationsNotFound"),
            "No configurations found for provider {ProviderType}");

    private static readonly Action<ILogger, string, Exception?> LogDefaultConfigurationRetrieved =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(6, "DefaultConfigurationRetrieved"),
            "Retrieved default configuration for store {StoreName}");

    private static readonly Action<ILogger, Exception?> LogNoDefaultConfiguration =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(7, "NoDefaultConfiguration"),
            "No default configuration available");

    private static readonly Action<ILogger, string, bool, Exception?> LogStoreEnabledStatus =
        LoggerMessage.Define<string, bool>(
            LogLevel.Debug,
            new EventId(8, "StoreEnabledStatus"),
            "Store {StoreName} enabled status: {IsEnabled}");

    private static readonly Action<ILogger, Exception?> LogConfigurationChangeDetected =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(9, "ConfigurationChangeDetected"),
            "Configuration change detected, refreshing registry");

    private static readonly Action<ILogger, int, Exception?> LogRegistryRefreshed =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(10, "RegistryRefreshed"),
            "Registry refreshed with {ConfigCount} configurations");

    private static readonly Action<ILogger, Exception> LogConfigurationRefreshFailed =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(11, "ConfigurationRefreshFailed"),
            "Failed to refresh configurations after change detection");

    private static readonly Action<ILogger, int, int, Exception?> LogConfigurationsProcessed =
        LoggerMessage.Define<int, int>(
            LogLevel.Debug,
            new EventId(12, "ConfigurationsProcessed"),
            "Processed {ValidCount} valid configurations out of {TotalCount} total");

    private static readonly Action<ILogger, Exception?> LogNullConfigurationSkipped =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(13, "NullConfigurationSkipped"),
            "Null configuration encountered, skipping");

    private static readonly Action<ILogger, Exception?> LogEmptyStoreNameSkipped =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(14, "EmptyStoreNameSkipped"),
            "Configuration with empty StoreName encountered, skipping");

    private static readonly Action<ILogger, string, Exception?> LogEmptyProviderTypeSkipped =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(15, "EmptyProviderTypeSkipped"),
            "Configuration {StoreName} has empty ProviderType, skipping");

    private static readonly Action<ILogger, string, Exception?> LogDuplicateStoreName =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(16, "DuplicateStoreName"),
            "Duplicate store name {StoreName} detected, using first occurrence");

    private static readonly Action<ILogger, string, string, Exception?> LogConfigurationProcessed =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(17, "ConfigurationProcessed"),
            "Processed configuration for store {StoreName} with provider {ProviderType}");

    private static readonly Action<ILogger, string, Exception?> LogExplicitDefaultConfiguration =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(18, "ExplicitDefaultConfiguration"),
            "Using explicitly marked default configuration: {StoreName}");

    private static readonly Action<ILogger, string, Exception?> LogFirstEnabledAsDefault =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(19, "FirstEnabledAsDefault"),
            "Using first enabled configuration as default: {StoreName}");

    private static readonly Action<ILogger, Exception?> LogRegistryDisposed =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(20, "RegistryDisposed"),
            "DataStoreConfigurationRegistry disposed");

    private static readonly Action<ILogger, Exception> LogDisposalError =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(21, "DisposalError"),
            "Error occurred during registry disposal");

    /// <summary>
    /// Initializes a new instance of the <see cref="DataStoreConfigurationRegistry"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for this registry.</param>
    /// <param name="optionsMonitor">The options monitor for hot-reload support.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or optionsMonitor is null.</exception>
    public DataStoreConfigurationRegistry(
        ILogger<DataStoreConfigurationRegistry> logger,
        IOptionsMonitor<DataStoreConfiguration[]> optionsMonitor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        
        _configurationsByName = new ConcurrentDictionary<string, DataStoreConfiguration>(StringComparer.Ordinal);
        _configurationsByProvider = new ConcurrentDictionary<string, List<DataStoreConfiguration>>(StringComparer.Ordinal);

        // Register for configuration changes
        _changeTokenRegistration = _optionsMonitor.OnChange(OnConfigurationChanged);
        
        // Initialize with current configuration
        RefreshConfigurations();
        
        LogRegistryInitialized(_logger, _allConfigurations.Length, null);
    }

    /// <inheritdoc/>
    public DataStoreConfiguration? Get(int id)
    {
        ThrowIfDisposed();
        
        return _allConfigurations.FirstOrDefault(c => c.Id == id);
    }

    /// <inheritdoc/>
    public IEnumerable<DataStoreConfiguration> GetAll()
    {
        ThrowIfDisposed();
        
        return _allConfigurations.AsEnumerable();
    }

    /// <inheritdoc/>
    public bool TryGet(int id, out DataStoreConfiguration? configuration)
    {
        configuration = Get(id);
        return configuration != null;
    }

    /// <summary>
    /// Checks if a configuration exists by ID.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <returns>True if the configuration exists; otherwise, false.</returns>
    public bool Contains(int id) => _allConfigurations.Any(c => c.Id == id);
    
    /// <summary>
    /// Checks if a configuration exists by name.
    /// </summary>
    /// <param name="name">The configuration name.</param>
    /// <returns>True if the configuration exists; otherwise, false.</returns>
    public bool ContainsByName(string name) => _allConfigurations.Any(c => string.Equals(c.StoreName, name, StringComparison.Ordinal));

    /// <summary>
    /// Gets a configuration by store name.
    /// </summary>
    /// <param name="name">The unique name of the data store.</param>
    /// <returns>The configuration if found; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    public DataStoreConfiguration? GetByName(string name)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Store name cannot be null or empty", nameof(name));
        }

        _configurationsByName.TryGetValue(name, out var configuration);
        
        if (configuration != null)
        {
            LogConfigurationRetrieved(_logger, name, null);
        }
        else
        {
            LogConfigurationNotFound(_logger, name, null);
        }
        
        return configuration;
    }

    /// <summary>
    /// Gets all configurations for a specific provider type.
    /// </summary>
    /// <param name="providerType">The provider type (e.g., "MsSql", "PostgreSql").</param>
    /// <returns>A collection of configurations for the specified provider type.</returns>
    /// <exception cref="ArgumentException">Thrown when providerType is null or empty.</exception>
    public IEnumerable<DataStoreConfiguration> GetByProvider(string providerType)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(providerType))
        {
            throw new ArgumentException("Provider type cannot be null or empty", nameof(providerType));
        }

        if (_configurationsByProvider.TryGetValue(providerType, out var configurations))
        {
            LogProviderConfigurationsRetrieved(_logger, configurations.Count, providerType, null);
            return configurations.AsEnumerable();
        }

        LogProviderConfigurationsNotFound(_logger, providerType, null);
        return [];
    }

    /// <summary>
    /// Gets the default data store configuration.
    /// </summary>
    /// <returns>The default configuration if found; otherwise, null.</returns>
    /// <remarks>
    /// The default configuration is determined by:
    /// 1. A configuration explicitly marked as default
    /// 2. The first enabled configuration if no explicit default exists
    /// 3. null if no configurations are available
    /// </remarks>
    public DataStoreConfiguration? GetDefault()
    {
        ThrowIfDisposed();
        
        if (_defaultConfiguration != null)
        {
            LogDefaultConfigurationRetrieved(_logger, _defaultConfiguration.StoreName, null);
        }
        else
        {
            LogNoDefaultConfiguration(_logger, null);
        }
        
        return _defaultConfiguration;
    }

    /// <summary>
    /// Checks if a data store is enabled by name.
    /// </summary>
    /// <param name="name">The unique name of the data store.</param>
    /// <returns>True if the store exists and is enabled; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    public bool IsEnabled(string name)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Store name cannot be null or empty", nameof(name));
        }

        var configuration = GetByName(name);
        var isEnabled = configuration?.IsEnabled == true;
        
        LogStoreEnabledStatus(_logger, name, isEnabled, null);
        
        return isEnabled;
    }

    /// <summary>
    /// Tries to get a configuration by store name.
    /// </summary>
    /// <param name="name">The unique name of the data store.</param>
    /// <param name="configuration">The configuration if found; otherwise, null.</param>
    /// <returns>True if the configuration was found; otherwise, false.</returns>
    public bool TryGetByName(string name, out DataStoreConfiguration? configuration)
    {
        try
        {
            configuration = GetByName(name);
            return configuration != null;
        }
        catch (ArgumentException)
        {
            configuration = null;
            return false;
        }
    }

    /// <summary>
    /// Gets all enabled configurations.
    /// </summary>
    /// <returns>A collection of enabled configurations.</returns>
    public IEnumerable<DataStoreConfiguration> GetAllEnabled()
    {
        ThrowIfDisposed();
        
        return _allConfigurations.Where(c => c.IsEnabled);
    }

    /// <summary>
    /// Gets the count of registered configurations.
    /// </summary>
    /// <returns>The total number of configurations.</returns>
    public int Count 
    { 
        get 
        { 
            ThrowIfDisposed(); 
            return _allConfigurations.Length; 
        } 
    }

    /// <summary>
    /// Gets the count of enabled configurations.
    /// </summary>
    /// <returns>The number of enabled configurations.</returns>
    public int EnabledCount 
    { 
        get 
        { 
            ThrowIfDisposed(); 
            return _allConfigurations.Count(c => c.IsEnabled); 
        } 
    }

    /// <summary>
    /// Called when configuration changes are detected.
    /// </summary>
    /// <param name="configurations">The updated configurations array.</param>
    private void OnConfigurationChanged(DataStoreConfiguration[] configurations)
    {
        if (_disposed)
        {
            return;
        }

        lock (_refreshLock)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                LogConfigurationChangeDetected(_logger, null);
                RefreshConfigurations();
                LogRegistryRefreshed(_logger, _allConfigurations.Length, null);
            }
            catch (Exception ex)
            {
                LogConfigurationRefreshFailed(_logger, ex);
            }
        }
    }

    /// <summary>
    /// Refreshes the internal configuration caches.
    /// </summary>
    private void RefreshConfigurations()
    {
        var configurations = _optionsMonitor.CurrentValue ?? [];
        var validConfigurations = new List<DataStoreConfiguration>();

        // Clear existing caches
        _configurationsByName.Clear();
        _configurationsByProvider.Clear();

        // Process each configuration
        foreach (var config in configurations)
        {
            if (ValidateConfiguration(config))
            {
                validConfigurations.Add(config);
                ProcessConfiguration(config);
            }
        }

        // Update arrays and default
        _allConfigurations = validConfigurations.ToArray();
        _defaultConfiguration = DetermineDefaultConfiguration(validConfigurations);

        LogConfigurationsProcessed(_logger, validConfigurations.Count, configurations.Length, null);
    }

    /// <summary>
    /// Validates a configuration before adding it to the registry.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>True if the configuration is valid; otherwise, false.</returns>
    private bool ValidateConfiguration(DataStoreConfiguration configuration)
    {
        if (configuration == null)
        {
            LogNullConfigurationSkipped(_logger, null);
            return false;
        }

        if (string.IsNullOrWhiteSpace(configuration.StoreName))
        {
            LogEmptyStoreNameSkipped(_logger, null);
            return false;
        }

        if (string.IsNullOrWhiteSpace(configuration.ProviderType))
        {
            LogEmptyProviderTypeSkipped(_logger, configuration.StoreName, null);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Processes a valid configuration and adds it to the appropriate caches.
    /// </summary>
    /// <param name="configuration">The configuration to process.</param>
    private void ProcessConfiguration(DataStoreConfiguration configuration)
    {
        // Add to name-based lookup
        if (!_configurationsByName.TryAdd(configuration.StoreName, configuration))
        {
            LogDuplicateStoreName(_logger, configuration.StoreName, null);
        }

        // Add to provider-based lookup
        _configurationsByProvider.AddOrUpdate(
            configuration.ProviderType,
            [configuration],
            (key, existing) =>
            {
                existing.Add(configuration);
                return existing;
            });

        LogConfigurationProcessed(_logger, configuration.StoreName, configuration.ProviderType, null);
    }

    /// <summary>
    /// Determines the default configuration from available configurations.
    /// </summary>
    /// <param name="configurations">The available configurations.</param>
    /// <returns>The default configuration or null if none available.</returns>
    private DataStoreConfiguration? DetermineDefaultConfiguration(List<DataStoreConfiguration> configurations)
    {
        if (configurations.Count == 0)
        {
            return null;
        }

        // Look for explicitly marked default (if such a property exists in extended properties)
        var explicitDefault = configurations.FirstOrDefault(c => 
            c.TryGetExtendedProperty<bool>("IsDefault", out var isDefault) && isDefault);

        if (explicitDefault != null)
        {
            LogExplicitDefaultConfiguration(_logger, explicitDefault.StoreName, null);
            return explicitDefault;
        }

        // Fall back to first enabled configuration
        var firstEnabled = configurations.FirstOrDefault(c => c.IsEnabled);
        if (firstEnabled != null)
        {
            LogFirstEnabledAsDefault(_logger, firstEnabled.StoreName, null);
        }

        return firstEnabled;
    }

    /// <summary>
    /// Throws ObjectDisposedException if this instance has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_refreshLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            
            try
            {
                _changeTokenRegistration?.Dispose();
                _configurationsByName.Clear();
                _configurationsByProvider.Clear();
                LogRegistryDisposed(_logger, null);
            }
            catch (Exception ex)
            {
                LogDisposalError(_logger, ex);
            }
        }
    }
}

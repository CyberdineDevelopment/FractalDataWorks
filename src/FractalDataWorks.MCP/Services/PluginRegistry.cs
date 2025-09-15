using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FractalDataWorks.MCP.Abstractions;
using FractalDataWorks.MCP.EnhancedEnums;
using FractalDataWorks.Results;
using ModelContextProtocol.Abstractions;

namespace FractalDataWorks.MCP.Services;

/// <summary>
/// Registry for managing MCP tool plugins with auto-discovery.
/// </summary>
public sealed class PluginRegistry : IPluginRegistry
{
    private readonly ConcurrentDictionary<string, IToolPlugin> _plugins = new(StringComparer.Ordinal);
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PluginRegistry> _logger;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _isInitialized;

    public PluginRegistry(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<PluginRegistry> logger)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IReadOnlyList<IToolPlugin> GetAllPlugins()
    {
        return _plugins.Values
            .Where(p => p.IsEnabled)
            .OrderByDescending(p => p.Priority)
            .ThenBy(p => p.Name, StringComparer.Ordinal)
            .ToList();
    }

    public IReadOnlyList<IToolPlugin> GetPluginsByCategory(ToolCategoryBase category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        return _plugins.Values
            .Where(p => p.IsEnabled && p.Category?.Id == category.Id)
            .OrderByDescending(p => p.Priority)
            .ThenBy(p => p.Name, StringComparer.Ordinal)
            .ToList();
    }

    public IToolPlugin? GetPluginById(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return null;

        _plugins.TryGetValue(pluginId, out var plugin);
        return plugin?.IsEnabled == true ? plugin : null;
    }

    public async Task<IFdwResult> RegisterPluginAsync(IToolPlugin plugin, CancellationToken cancellationToken = default)
    {
        if (plugin == null)
            return FdwResult.Failure("Plugin cannot be null");

        try
        {
            if (_plugins.TryAdd(plugin.Id, plugin))
            {
                _logger.LogInformation("Registered plugin: {PluginId} ({PluginName})", plugin.Id, plugin.Name);

                // Initialize the plugin if registry is already initialized
                if (_isInitialized)
                {
                    var configSection = _configuration.GetSection($"Plugins:{plugin.Id}");
                    var config = CreatePluginConfiguration(plugin, configSection);

                    var initResult = await plugin.InitializeAsync(config, cancellationToken);
                    if (!initResult.IsSuccess)
                    {
                        _plugins.TryRemove(plugin.Id, out _);
                        return FdwResult.Failure($"Failed to initialize plugin {plugin.Id}: {initResult.Message}");
                    }
                }

                return FdwResult.Success();
            }

            return FdwResult.Failure($"Plugin with ID {plugin.Id} is already registered");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering plugin {PluginId}", plugin.Id);
            return FdwResult.Failure($"Error registering plugin: {ex.Message}");
        }
    }

    public async Task<IFdwResult> UnregisterPluginAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return FdwResult.Failure("Plugin ID cannot be null or empty");

        try
        {
            if (_plugins.TryRemove(pluginId, out var plugin))
            {
                await plugin.ShutdownAsync(cancellationToken);
                _logger.LogInformation("Unregistered plugin: {PluginId}", pluginId);
                return FdwResult.Success();
            }

            return FdwResult.Failure($"Plugin with ID {pluginId} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering plugin {PluginId}", pluginId);
            return FdwResult.Failure($"Error unregistering plugin: {ex.Message}");
        }
    }

    public async Task<IFdwResult<int>> DiscoverAndLoadPluginsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting plugin discovery...");

            var pluginTypes = DiscoverPluginTypes();
            var loadedCount = 0;

            foreach (var pluginType in pluginTypes)
            {
                try
                {
                    var plugin = CreatePluginInstance(pluginType);
                    if (plugin != null)
                    {
                        var result = await RegisterPluginAsync(plugin, cancellationToken);
                        if (result.IsSuccess)
                        {
                            loadedCount++;
                            _logger.LogInformation("Loaded plugin: {PluginType}", pluginType.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading plugin type {PluginType}", pluginType.Name);
                }
            }

            _logger.LogInformation("Plugin discovery complete. Loaded {Count} plugins", loadedCount);
            return FdwResult<int>.Success(loadedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during plugin discovery");
            return FdwResult<int>.Failure($"Plugin discovery failed: {ex.Message}");
        }
    }

    public IReadOnlyList<IMcpTool> GetAllTools()
    {
        var tools = new List<IMcpTool>();

        foreach (var plugin in GetAllPlugins())
        {
            try
            {
                var pluginTools = plugin.GetTools();
                if (pluginTools != null)
                {
                    tools.AddRange(pluginTools.Where(t => t != null && t.IsEnabled));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tools from plugin {PluginId}", plugin.Id);
            }
        }

        return tools
            .OrderBy(t => t.Category?.DisplayPriority ?? 999)
            .ThenByDescending(t => t.Priority)
            .ThenBy(t => t.Name, StringComparer.Ordinal)
            .ToList();
    }

    public async Task<IFdwResult> InitializeAllPluginsAsync(CancellationToken cancellationToken = default)
    {
        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_isInitialized)
                return FdwResult.Success();

            _logger.LogInformation("Initializing all plugins...");

            var failures = new List<string>();

            foreach (var plugin in _plugins.Values)
            {
                try
                {
                    var configSection = _configuration.GetSection($"Plugins:{plugin.Id}");
                    var config = CreatePluginConfiguration(plugin, configSection);

                    var result = await plugin.InitializeAsync(config, cancellationToken);
                    if (!result.IsSuccess)
                    {
                        failures.Add($"{plugin.Id}: {result.Message}");
                        _logger.LogWarning("Failed to initialize plugin {PluginId}: {Message}",
                            plugin.Id, result.Message);
                    }
                    else
                    {
                        _logger.LogInformation("Initialized plugin {PluginId}", plugin.Id);
                    }
                }
                catch (Exception ex)
                {
                    failures.Add($"{plugin.Id}: {ex.Message}");
                    _logger.LogError(ex, "Error initializing plugin {PluginId}", plugin.Id);
                }
            }

            _isInitialized = true;

            if (failures.Count > 0)
            {
                return FdwResult.Failure($"Some plugins failed to initialize: {string.Join("; ", failures)}");
            }

            _logger.LogInformation("All plugins initialized successfully");
            return FdwResult.Success();
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task<IFdwResult> ShutdownAllPluginsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Shutting down all plugins...");

        var tasks = _plugins.Values.Select(async plugin =>
        {
            try
            {
                await plugin.ShutdownAsync(cancellationToken);
                _logger.LogInformation("Shut down plugin {PluginId}", plugin.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shutting down plugin {PluginId}", plugin.Id);
            }
        });

        await Task.WhenAll(tasks);

        _plugins.Clear();
        _isInitialized = false;

        _logger.LogInformation("All plugins shut down");
        return FdwResult.Success();
    }

    private List<Type> DiscoverPluginTypes()
    {
        var pluginTypes = new List<Type>();

        // Discover plugins in current assembly
        var currentAssembly = Assembly.GetExecutingAssembly();
        pluginTypes.AddRange(GetPluginTypesFromAssembly(currentAssembly));

        // Discover plugins in entry assembly
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null && entryAssembly != currentAssembly)
        {
            pluginTypes.AddRange(GetPluginTypesFromAssembly(entryAssembly));
        }

        // Could also load from external assemblies here if needed

        return pluginTypes;
    }

    private List<Type> GetPluginTypesFromAssembly(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes()
                .Where(t => !t.IsAbstract &&
                           !t.IsInterface &&
                           typeof(IToolPlugin).IsAssignableFrom(t) &&
                           t.GetConstructor(Type.EmptyTypes) != null)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering plugin types in assembly {Assembly}", assembly.FullName);
            return new List<Type>();
        }
    }

    private IToolPlugin? CreatePluginInstance(Type pluginType)
    {
        try
        {
            // Try to get from DI container first
            var plugin = _serviceProvider.GetService(pluginType) as IToolPlugin;

            // Fall back to parameterless constructor
            plugin ??= Activator.CreateInstance(pluginType) as IToolPlugin;

            return plugin;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating plugin instance of type {PluginType}", pluginType.Name);
            return null;
        }
    }

    private IToolPluginConfiguration CreatePluginConfiguration(IToolPlugin plugin, IConfigurationSection section)
    {
        // Create a basic configuration - could be enhanced to create specific config types
        return new BasicPluginConfiguration
        {
            Enabled = section.GetValue("Enabled", true),
            Priority = section.GetValue("Priority", 100),
            OperationTimeout = TimeSpan.FromSeconds(section.GetValue("TimeoutSeconds", 30)),
            EnableDetailedLogging = section.GetValue("EnableDetailedLogging", false)
        };
    }

    private class BasicPluginConfiguration : IToolPluginConfiguration
    {
        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 100;
        public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public bool EnableDetailedLogging { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.MCP.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.MCP.Services;

/// <summary>
/// Registry for managing MCP tool plugins.
/// </summary>
public interface IPluginRegistry
{
    /// <summary>
    /// Gets all registered plugins.
    /// </summary>
    IReadOnlyList<IToolPlugin> GetAllPlugins();

    /// <summary>
    /// Gets plugins by category.
    /// </summary>
    IReadOnlyList<IToolPlugin> GetPluginsByCategory(ToolCategoryBase category);

    /// <summary>
    /// Gets a plugin by its ID.
    /// </summary>
    IToolPlugin? GetPluginById(string pluginId);

    /// <summary>
    /// Registers a plugin.
    /// </summary>
    Task<IFdwResult> RegisterPluginAsync(IToolPlugin plugin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters a plugin.
    /// </summary>
    Task<IFdwResult> UnregisterPluginAsync(string pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discovers and loads all plugins.
    /// </summary>
    Task<IFdwResult<int>> DiscoverAndLoadPluginsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all MCP tools from all plugins.
    /// </summary>
    IReadOnlyList<IMcpTool> GetAllTools();

    /// <summary>
    /// Initializes all plugins with their configurations.
    /// </summary>
    Task<IFdwResult> InitializeAllPluginsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Shuts down all plugins gracefully.
    /// </summary>
    Task<IFdwResult> ShutdownAllPluginsAsync(CancellationToken cancellationToken = default);
}
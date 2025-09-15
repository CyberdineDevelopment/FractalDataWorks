using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Abstractions;
using FractalDataWorks.MCP.EnhancedEnums;
using FractalDataWorks.Results;
using ModelContextProtocol.Abstractions;

namespace FractalDataWorks.MCP.Abstractions;

/// <summary>
/// Base interface for all MCP tool plugins.
/// </summary>
public interface IToolPlugin : IFdwService
{
    /// <summary>
    /// Gets the unique identifier for this plugin.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the display name of this plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of this plugin's functionality.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the category of tools this plugin provides.
    /// </summary>
    ToolCategoryBase Category { get; }

    /// <summary>
    /// Gets the priority for this plugin (higher values load first).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Gets whether this plugin is currently enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets the collection of MCP tools provided by this plugin.
    /// </summary>
    IReadOnlyCollection<IMcpTool> GetTools();

    /// <summary>
    /// Initializes the plugin with its configuration.
    /// </summary>
    Task<IFdwResult> InitializeAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the plugin configuration.
    /// </summary>
    Task<IFdwResult> ValidateConfigurationAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the health status of this plugin.
    /// </summary>
    Task<IFdwResult<PluginHealth>> GetHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Shuts down the plugin gracefully.
    /// </summary>
    Task<IFdwResult> ShutdownAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for typed tool plugins with specific configuration.
/// </summary>
public interface IToolPlugin<TConfiguration> : IToolPlugin
    where TConfiguration : class, IToolPluginConfiguration
{
    /// <summary>
    /// Gets the current configuration for this plugin.
    /// </summary>
    TConfiguration Configuration { get; }

    /// <summary>
    /// Initializes the plugin with typed configuration.
    /// </summary>
    Task<IFdwResult> InitializeAsync(TConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the typed configuration.
    /// </summary>
    Task<IFdwResult> ValidateConfigurationAsync(TConfiguration configuration, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base interface for tool plugin configuration.
/// </summary>
public interface IToolPluginConfiguration : IFractalConfiguration
{
    /// <summary>
    /// Gets or sets whether this plugin is enabled.
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the priority for this plugin.
    /// </summary>
    int Priority { get; set; }

    /// <summary>
    /// Gets or sets the timeout for plugin operations.
    /// </summary>
    TimeSpan OperationTimeout { get; set; }

    /// <summary>
    /// Gets or sets whether to enable detailed logging.
    /// </summary>
    bool EnableDetailedLogging { get; set; }
}

/// <summary>
/// Represents the health status of a plugin.
/// </summary>
public sealed class PluginHealth
{
    /// <summary>
    /// Gets the health status.
    /// </summary>
    public HealthStatus Status { get; init; }

    /// <summary>
    /// Gets the health message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last check timestamp.
    /// </summary>
    public DateTimeOffset LastChecked { get; init; }

    /// <summary>
    /// Gets additional health details.
    /// </summary>
    public IReadOnlyDictionary<string, object> Details { get; init; } = new Dictionary<string, object>(StringComparer.Ordinal);
}

/// <summary>
/// Health status enumeration.
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// Plugin is healthy and operational.
    /// </summary>
    Healthy,

    /// <summary>
    /// Plugin is degraded but operational.
    /// </summary>
    Degraded,

    /// <summary>
    /// Plugin is unhealthy and not operational.
    /// </summary>
    Unhealthy,

    /// <summary>
    /// Plugin health is unknown.
    /// </summary>
    Unknown
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FractalDataWorks.MCP.Abstractions;
using FractalDataWorks.MCP.EnhancedEnums;
using FractalDataWorks.MCP.Services;
using FractalDataWorks.MCP.Tools;
using FractalDataWorks.Results;
using ModelContextProtocol.Abstractions;

namespace FractalDataWorks.MCP.Plugins;

/// <summary>
/// Plugin providing session management tools for compilation workspaces.
/// </summary>
public sealed class SessionManagementPlugin : IToolPlugin<SessionManagementConfiguration>
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly ILogger<SessionManagementPlugin> _logger;
    private readonly List<IMcpTool> _tools;
    private SessionManagementConfiguration? _configuration;
    private bool _isInitialized;

    public SessionManagementPlugin(
        WorkspaceSessionManager sessionManager,
        SessionTools sessionTools,
        SessionLifecycleTools lifecycleTools,
        ILogger<SessionManagementPlugin> logger)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Id = "SessionManagement";
        Name = "Session Management Plugin";
        Description = "Provides tools for managing Roslyn compilation sessions and workspace state";
        Category = SessionManagement.Instance;
        Priority = 1000; // High priority - foundational plugin

        // Wrap existing tools as MCP tools
        _tools = new List<IMcpTool>();

        if (sessionTools != null)
        {
            _tools.AddRange(WrapSessionTools(sessionTools));
        }

        if (lifecycleTools != null)
        {
            _tools.AddRange(WrapLifecycleTools(lifecycleTools));
        }
    }

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public ToolCategoryBase Category { get; }
    public int Priority { get; }
    public bool IsEnabled => _configuration?.Enabled ?? true;
    public SessionManagementConfiguration Configuration => _configuration ?? new SessionManagementConfiguration();

    public IReadOnlyCollection<IMcpTool> GetTools()
    {
        return _tools.Where(t => t.IsEnabled).ToList();
    }

    public async Task<IFdwResult> InitializeAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is SessionManagementConfiguration typedConfig)
        {
            return await InitializeAsync(typedConfig, cancellationToken);
        }

        // Convert basic configuration to typed
        var sessionConfig = new SessionManagementConfiguration
        {
            Enabled = configuration.Enabled,
            Priority = configuration.Priority,
            OperationTimeout = configuration.OperationTimeout,
            EnableDetailedLogging = configuration.EnableDetailedLogging
        };

        return await InitializeAsync(sessionConfig, cancellationToken);
    }

    public async Task<IFdwResult> InitializeAsync(SessionManagementConfiguration configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Validate configuration
            var validationResult = await ValidateConfigurationAsync(configuration, cancellationToken);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            // Initialize session manager with configuration
            if (_sessionManager is IConfigurable<SessionManagementConfiguration> configurable)
            {
                await configurable.ConfigureAsync(configuration, cancellationToken);
            }

            _isInitialized = true;
            _logger.LogInformation("Session Management plugin initialized successfully");

            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Session Management plugin");
            return FdwResult.Failure($"Initialization failed: {ex.Message}");
        }
    }

    public Task<IFdwResult> ValidateConfigurationAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is SessionManagementConfiguration typedConfig)
        {
            return ValidateConfigurationAsync(typedConfig, cancellationToken);
        }

        return Task.FromResult(FdwResult.Success());
    }

    public Task<IFdwResult> ValidateConfigurationAsync(SessionManagementConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            return Task.FromResult(FdwResult.Failure("Configuration cannot be null"));
        }

        if (configuration.MaxConcurrentSessions <= 0)
        {
            return Task.FromResult(FdwResult.Failure("MaxConcurrentSessions must be greater than 0"));
        }

        if (configuration.DefaultSessionTimeoutMinutes <= 0)
        {
            return Task.FromResult(FdwResult.Failure("DefaultSessionTimeoutMinutes must be greater than 0"));
        }

        return Task.FromResult(FdwResult.Success());
    }

    public async Task<IFdwResult<PluginHealth>> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var activeSessions = _sessionManager.GetActiveSessions().Count;
            var maxSessions = Configuration.MaxConcurrentSessions;

            var status = activeSessions switch
            {
                0 => HealthStatus.Healthy,
                _ when activeSessions < maxSessions * 0.8 => HealthStatus.Healthy,
                _ when activeSessions < maxSessions => HealthStatus.Degraded,
                _ => HealthStatus.Unhealthy
            };

            var health = new PluginHealth
            {
                Status = status,
                Message = $"Active sessions: {activeSessions}/{maxSessions}",
                LastChecked = DateTimeOffset.UtcNow,
                Details = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["ActiveSessions"] = activeSessions,
                    ["MaxSessions"] = maxSessions,
                    ["IsInitialized"] = _isInitialized
                }
            };

            return await Task.FromResult(FdwResult<PluginHealth>.Success(health));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking plugin health");

            var errorHealth = new PluginHealth
            {
                Status = HealthStatus.Unknown,
                Message = $"Health check failed: {ex.Message}",
                LastChecked = DateTimeOffset.UtcNow
            };

            return FdwResult<PluginHealth>.Success(errorHealth);
        }
    }

    public async Task<IFdwResult> ShutdownAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Shutting down Session Management plugin");

            // Close all active sessions
            var activeSessions = _sessionManager.GetActiveSessions();
            foreach (var session in activeSessions)
            {
                try
                {
                    await _sessionManager.EndSessionAsync(session.Id, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error closing session {SessionId} during shutdown", session.Id);
                }
            }

            _isInitialized = false;
            _logger.LogInformation("Session Management plugin shut down successfully");

            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during plugin shutdown");
            return FdwResult.Failure($"Shutdown failed: {ex.Message}");
        }
    }

    private List<IMcpTool> WrapSessionTools(SessionTools tools)
    {
        var wrappedTools = new List<IMcpTool>();

        // Get all public methods from SessionTools that are marked as Tool
        var toolMethods = tools.GetType().GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(ToolAttribute), false).Any());

        foreach (var method in toolMethods)
        {
            var toolAttr = method.GetCustomAttribute<ToolAttribute>();
            if (toolAttr != null)
            {
                wrappedTools.Add(new McpToolWrapper(
                    name: toolAttr.Name ?? method.Name,
                    description: toolAttr.Description ?? "Session management tool",
                    plugin: this,
                    category: Category,
                    executeFunc: async (args, ct) =>
                    {
                        try
                        {
                            var result = method.Invoke(tools, new[] { args, ct });
                            if (result is Task task)
                            {
                                await task;
                                return FdwResult<object>.Success(task.GetType().GetProperty("Result")?.GetValue(task));
                            }
                            return FdwResult<object>.Success(result);
                        }
                        catch (Exception ex)
                        {
                            return FdwResult<object>.Failure(ex.Message);
                        }
                    }));
            }
        }

        return wrappedTools;
    }

    private List<IMcpTool> WrapLifecycleTools(SessionLifecycleTools tools)
    {
        var wrappedTools = new List<IMcpTool>();

        // Get all public methods from SessionLifecycleTools that are marked as Tool
        var toolMethods = tools.GetType().GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(ToolAttribute), false).Any());

        foreach (var method in toolMethods)
        {
            var toolAttr = method.GetCustomAttribute<ToolAttribute>();
            if (toolAttr != null)
            {
                wrappedTools.Add(new McpToolWrapper(
                    name: toolAttr.Name ?? method.Name,
                    description: toolAttr.Description ?? "Session lifecycle tool",
                    plugin: this,
                    category: Category,
                    executeFunc: async (args, ct) =>
                    {
                        try
                        {
                            var result = method.Invoke(tools, new[] { args, ct });
                            if (result is Task task)
                            {
                                await task;
                                return FdwResult<object>.Success(task.GetType().GetProperty("Result")?.GetValue(task));
                            }
                            return FdwResult<object>.Success(result);
                        }
                        catch (Exception ex)
                        {
                            return FdwResult<object>.Failure(ex.Message);
                        }
                    }));
            }
        }

        return wrappedTools;
    }
}

/// <summary>
/// Configuration for the Session Management plugin.
/// </summary>
public class SessionManagementConfiguration : IToolPluginConfiguration
{
    public bool Enabled { get; set; } = true;
    public int Priority { get; set; } = 1000;
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public bool EnableDetailedLogging { get; set; }
    public int MaxConcurrentSessions { get; set; } = 10;
    public int DefaultSessionTimeoutMinutes { get; set; } = 30;
    public bool EnableAutomaticCleanup { get; set; } = true;
}

/// <summary>
/// Interface for configurable services.
/// </summary>
public interface IConfigurable<TConfiguration>
{
    Task ConfigureAsync(TConfiguration configuration, CancellationToken cancellationToken = default);
}
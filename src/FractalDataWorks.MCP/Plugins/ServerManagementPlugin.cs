using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FractalDataWorks.MCP.Abstractions;
using FractalDataWorks.MCP.EnhancedEnums;
using RoslynMcpServer.Services;
using RoslynMcpServer.Tools;
using FractalDataWorks.Results;
using ModelContextProtocol.Abstractions;
using ModelContextProtocol.Server;

namespace FractalDataWorks.MCP.Plugins;

/// <summary>
/// Plugin providing server management and lifecycle tools.
/// </summary>
public sealed class ServerManagementPlugin : IToolPlugin<ServerManagementConfiguration>
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ServerInfoTool _serverInfoTool;
    private readonly ServerShutdownTool _serverShutdownTool;
    private readonly ServerRestartTool _serverRestartTool;
    private readonly ILogger<ServerManagementPlugin> _logger;
    private readonly List<IMcpTool> _tools;
    private ServerManagementConfiguration? _configuration;
    private bool _isInitialized;

    public ServerManagementPlugin(
        WorkspaceSessionManager sessionManager,
        IHostApplicationLifetime applicationLifetime,
        ServerInfoTool serverInfoTool,
        ServerShutdownTool serverShutdownTool,
        ServerRestartTool serverRestartTool,
        ILogger<ServerManagementPlugin> logger)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        _serverInfoTool = serverInfoTool ?? throw new ArgumentNullException(nameof(serverInfoTool));
        _serverShutdownTool = serverShutdownTool ?? throw new ArgumentNullException(nameof(serverShutdownTool));
        _serverRestartTool = serverRestartTool ?? throw new ArgumentNullException(nameof(serverRestartTool));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Id = "ServerManagement";
        Name = "Server Management Plugin";
        Description = "Provides tools for managing the MCP server lifecycle";
        Category = ServerManagement.Instance;
        Priority = 100; // Lower priority - typically used for administrative tasks

        // Wrap existing server management tools as MCP tools
        _tools = WrapServerManagementTools();
    }

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public ToolCategoryBase Category { get; }
    public int Priority { get; }
    public bool IsEnabled => _configuration?.Enabled ?? true;
    public ServerManagementConfiguration Configuration => _configuration ?? new ServerManagementConfiguration();

    public IReadOnlyCollection<IMcpTool> GetTools()
    {
        return _tools.Where(t => t.IsEnabled).ToList();
    }

    public async Task<IFdwResult> InitializeAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is ServerManagementConfiguration typedConfig)
        {
            return await InitializeAsync(typedConfig, cancellationToken);
        }

        // Convert basic configuration to typed
        var managementConfig = new ServerManagementConfiguration
        {
            Enabled = configuration.Enabled,
            Priority = configuration.Priority,
            OperationTimeout = configuration.OperationTimeout,
            EnableDetailedLogging = configuration.EnableDetailedLogging
        };

        return await InitializeAsync(managementConfig, cancellationToken);
    }

    public async Task<IFdwResult> InitializeAsync(ServerManagementConfiguration configuration, CancellationToken cancellationToken = default)
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

            _isInitialized = true;
            _logger.LogInformation("Server Management plugin initialized successfully");

            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Server Management plugin");
            return FdwResult.Failure($"Initialization failed: {ex.Message}");
        }
    }

    public Task<IFdwResult> ValidateConfigurationAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is ServerManagementConfiguration typedConfig)
        {
            return ValidateConfigurationAsync(typedConfig, cancellationToken);
        }

        return Task.FromResult(FdwResult.Success());
    }

    public Task<IFdwResult> ValidateConfigurationAsync(ServerManagementConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            return Task.FromResult(FdwResult.Failure("Configuration cannot be null"));
        }

        if (configuration.ShutdownGracePeriod <= TimeSpan.Zero)
        {
            return Task.FromResult(FdwResult.Failure("ShutdownGracePeriod must be greater than zero"));
        }

        return Task.FromResult(FdwResult.Success());
    }

    public async Task<IFdwResult<PluginHealth>> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var activeSessions = _sessionManager.GetActiveSessions().Count;
            var isShuttingDown = _applicationLifetime.ApplicationStopping.IsCancellationRequested;

            var status = _isInitialized && !isShuttingDown ? HealthStatus.Healthy :
                        isShuttingDown ? HealthStatus.Degraded : HealthStatus.Unknown;

            var health = new PluginHealth
            {
                Status = status,
                Message = $"Server management ready. Active sessions: {activeSessions}. Shutting down: {isShuttingDown}",
                LastChecked = DateTimeOffset.UtcNow,
                Details = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["ActiveSessions"] = activeSessions,
                    ["IsInitialized"] = _isInitialized,
                    ["IsShuttingDown"] = isShuttingDown,
                    ["ProcessId"] = Environment.ProcessId
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
            _logger.LogInformation("Shutting down Server Management plugin");

            // This plugin manages shutdown, so it should be one of the last to shut down
            // Ensure graceful shutdown procedures are in place
            _isInitialized = false;
            _logger.LogInformation("Server Management plugin shut down successfully");

            return await Task.FromResult(FdwResult.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during plugin shutdown");
            return FdwResult.Failure($"Shutdown failed: {ex.Message}");
        }
    }

    private List<IMcpTool> WrapServerManagementTools()
    {
        var wrappedTools = new List<IMcpTool>();

        // Wrap ServerInfoTool
        var infoToolMethods = _serverInfoTool.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any());

        foreach (var method in infoToolMethods)
        {
            var toolAttr = method.GetCustomAttribute<McpServerToolAttribute>();
            if (toolAttr != null)
            {
                wrappedTools.Add(CreateServerManagementWrapper(method, toolAttr, _serverInfoTool, "server info"));
            }
        }

        // Wrap ServerShutdownTool
        var shutdownToolMethods = _serverShutdownTool.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any());

        foreach (var method in shutdownToolMethods)
        {
            var toolAttr = method.GetCustomAttribute<McpServerToolAttribute>();
            if (toolAttr != null)
            {
                wrappedTools.Add(CreateServerManagementWrapper(method, toolAttr, _serverShutdownTool, "server shutdown"));
            }
        }

        // Wrap ServerRestartTool
        var restartToolMethods = _serverRestartTool.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any());

        foreach (var method in restartToolMethods)
        {
            var toolAttr = method.GetCustomAttribute<McpServerToolAttribute>();
            if (toolAttr != null)
            {
                wrappedTools.Add(CreateServerManagementWrapper(method, toolAttr, _serverRestartTool, "server restart"));
            }
        }

        return wrappedTools;
    }

    private McpToolWrapper CreateServerManagementWrapper(MethodInfo method, McpServerToolAttribute toolAttr, object toolInstance, string toolType)
    {
        return new McpToolWrapper(
            name: toolAttr.Name ?? method.Name,
            description: toolAttr.Description ?? $"Server management {toolType} tool",
            plugin: this,
            category: Category,
            executeFunc: async (args, ct) =>
            {
                try
                {
                    // Apply timeout from configuration
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    timeoutCts.CancelAfter(Configuration.OperationTimeout);

                    var parameters = ConvertArgsToParameters(args, method);
                    var result = method.Invoke(toolInstance, parameters);

                    if (result is Task task)
                    {
                        await task.ConfigureAwait(false);
                        var taskResult = task.GetType().GetProperty("Result")?.GetValue(task);
                        return FdwResult<object>.Success(taskResult);
                    }

                    return FdwResult<object>.Success(result);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    return FdwResult<object>.Failure("Operation was cancelled");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing server management tool {ToolName}", toolAttr.Name);
                    return FdwResult<object>.Failure($"Tool execution failed: {ex.Message}");
                }
            });
    }

    private static object?[] ConvertArgsToParameters(object? args, MethodInfo method)
    {
        var parameters = method.GetParameters();
        var values = new object?[parameters.Length];

        if (args is IDictionary<string, object> argDict)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                if (argDict.TryGetValue(param.Name ?? string.Empty, out var value))
                {
                    values[i] = Convert.ChangeType(value, param.ParameterType);
                }
                else if (param.HasDefaultValue)
                {
                    values[i] = param.DefaultValue;
                }
            }
        }

        return values;
    }
}

/// <summary>
/// Configuration for the Server Management plugin.
/// </summary>
public class ServerManagementConfiguration : IToolPluginConfiguration
{
    public bool Enabled { get; set; } = true;
    public int Priority { get; set; } = 100;
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(1);
    public bool EnableDetailedLogging { get; set; }
    public TimeSpan ShutdownGracePeriod { get; set; } = TimeSpan.FromSeconds(5);
    public bool EnableStatePreservation { get; set; } = true;
    public bool EnableRestartDetection { get; set; } = true;
}
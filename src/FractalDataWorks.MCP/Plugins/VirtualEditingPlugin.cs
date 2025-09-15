using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
/// Plugin providing virtual editing tools for preview and rollback capabilities.
/// </summary>
public sealed class VirtualEditingPlugin : IToolPlugin<VirtualEditingConfiguration>
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly VirtualEditTools _virtualEditTools;
    private readonly VirtualEditService _editService;
    private readonly ILogger<VirtualEditingPlugin> _logger;
    private readonly List<IMcpTool> _tools;
    private VirtualEditingConfiguration? _configuration;
    private bool _isInitialized;

    public VirtualEditingPlugin(
        WorkspaceSessionManager sessionManager,
        VirtualEditTools virtualEditTools,
        VirtualEditService editService,
        ILogger<VirtualEditingPlugin> logger)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _virtualEditTools = virtualEditTools ?? throw new ArgumentNullException(nameof(virtualEditTools));
        _editService = editService ?? throw new ArgumentNullException(nameof(editService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Id = "VirtualEditing";
        Name = "Virtual Editing Plugin";
        Description = "Provides tools for preview editing with rollback capabilities";
        Category = VirtualEditing.Instance;
        Priority = 700; // High priority for core editing functionality

        // Wrap existing virtual edit tools as MCP tools
        _tools = WrapVirtualEditTools();
    }

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public ToolCategoryBase Category { get; }
    public int Priority { get; }
    public bool IsEnabled => _configuration?.Enabled ?? true;
    public VirtualEditingConfiguration Configuration => _configuration ?? new VirtualEditingConfiguration();

    public IReadOnlyCollection<IMcpTool> GetTools()
    {
        return _tools.Where(t => t.IsEnabled).ToList();
    }

    public async Task<IFdwResult> InitializeAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is VirtualEditingConfiguration typedConfig)
        {
            return await InitializeAsync(typedConfig, cancellationToken);
        }

        // Convert basic configuration to typed
        var editingConfig = new VirtualEditingConfiguration
        {
            Enabled = configuration.Enabled,
            Priority = configuration.Priority,
            OperationTimeout = configuration.OperationTimeout,
            EnableDetailedLogging = configuration.EnableDetailedLogging
        };

        return await InitializeAsync(editingConfig, cancellationToken);
    }

    public async Task<IFdwResult> InitializeAsync(VirtualEditingConfiguration configuration, CancellationToken cancellationToken = default)
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
            _logger.LogInformation("Virtual Editing plugin initialized successfully");

            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Virtual Editing plugin");
            return FdwResult.Failure($"Initialization failed: {ex.Message}");
        }
    }

    public Task<IFdwResult> ValidateConfigurationAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is VirtualEditingConfiguration typedConfig)
        {
            return ValidateConfigurationAsync(typedConfig, cancellationToken);
        }

        return Task.FromResult(FdwResult.Success());
    }

    public Task<IFdwResult> ValidateConfigurationAsync(VirtualEditingConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            return Task.FromResult(FdwResult.Failure("Configuration cannot be null"));
        }

        if (configuration.MaxVirtualEditsPerSession <= 0)
        {
            return Task.FromResult(FdwResult.Failure("MaxVirtualEditsPerSession must be greater than 0"));
        }

        if (configuration.VirtualEditRetentionHours <= 0)
        {
            return Task.FromResult(FdwResult.Failure("VirtualEditRetentionHours must be greater than 0"));
        }

        return Task.FromResult(FdwResult.Success());
    }

    public async Task<IFdwResult<PluginHealth>> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var activeSessions = _sessionManager.GetActiveSessions().Count;
            var pendingEdits = await GetPendingEditsCountAsync();

            var status = _isInitialized && activeSessions >= 0 ? HealthStatus.Healthy : HealthStatus.Degraded;

            if (pendingEdits > Configuration.MaxVirtualEditsPerSession * activeSessions * 0.8)
            {
                status = HealthStatus.Degraded;
            }

            var health = new PluginHealth
            {
                Status = status,
                Message = $"Virtual editing ready. Pending edits: {pendingEdits}",
                LastChecked = DateTimeOffset.UtcNow,
                Details = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["ActiveSessions"] = activeSessions,
                    ["PendingEdits"] = pendingEdits,
                    ["IsInitialized"] = _isInitialized,
                    ["AutoCommitEnabled"] = Configuration.EnableAutoCommit
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
            _logger.LogInformation("Shutting down Virtual Editing plugin");

            // Optionally commit or rollback pending edits based on configuration
            if (Configuration.CommitOnShutdown)
            {
                await CommitAllPendingEditsAsync(cancellationToken);
            }
            else if (Configuration.RollbackOnShutdown)
            {
                await RollbackAllPendingEditsAsync(cancellationToken);
            }

            _isInitialized = false;
            _logger.LogInformation("Virtual Editing plugin shut down successfully");

            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during plugin shutdown");
            return FdwResult.Failure($"Shutdown failed: {ex.Message}");
        }
    }

    private List<IMcpTool> WrapVirtualEditTools()
    {
        var wrappedTools = new List<IMcpTool>();

        // Get all public methods from VirtualEditTools that are marked with ToolAttribute
        var toolMethods = _virtualEditTools.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any());

        foreach (var method in toolMethods)
        {
            var toolAttr = method.GetCustomAttribute<McpServerToolAttribute>();
            if (toolAttr != null)
            {
                wrappedTools.Add(new McpToolWrapper(
                    name: toolAttr.Name ?? method.Name,
                    description: toolAttr.Description ?? "Virtual editing tool",
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
                            var result = method.Invoke(_virtualEditTools, parameters);

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
                            _logger.LogError(ex, "Error executing virtual edit tool {ToolName}", toolAttr.Name);
                            return FdwResult<object>.Failure($"Tool execution failed: {ex.Message}");
                        }
                    }));
            }
        }

        return wrappedTools;
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

    private async Task<int> GetPendingEditsCountAsync()
    {
        try
        {
            // This would need to be implemented based on the VirtualEditService API
            // For now, return 0 as a placeholder
            return await Task.FromResult(0);
        }
        catch
        {
            return 0;
        }
    }

    private async Task CommitAllPendingEditsAsync(CancellationToken cancellationToken)
    {
        var activeSessions = _sessionManager.GetActiveSessions();
        foreach (var session in activeSessions)
        {
            try
            {
                // This would commit pending edits for the session
                // Implementation depends on VirtualEditService API
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to commit pending edits for session {SessionId}", session.Id);
            }
        }
    }

    private async Task RollbackAllPendingEditsAsync(CancellationToken cancellationToken)
    {
        var activeSessions = _sessionManager.GetActiveSessions();
        foreach (var session in activeSessions)
        {
            try
            {
                // This would rollback pending edits for the session
                // Implementation depends on VirtualEditService API
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to rollback pending edits for session {SessionId}", session.Id);
            }
        }
    }
}

/// <summary>
/// Configuration for the Virtual Editing plugin.
/// </summary>
public class VirtualEditingConfiguration : IToolPluginConfiguration
{
    public bool Enabled { get; set; } = true;
    public int Priority { get; set; } = 700;
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public bool EnableDetailedLogging { get; set; }
    public int MaxVirtualEditsPerSession { get; set; } = 100;
    public int VirtualEditRetentionHours { get; set; } = 24;
    public bool EnableAutoCommit { get; set; } = false;
    public bool CommitOnShutdown { get; set; } = false;
    public bool RollbackOnShutdown { get; set; } = true;
}
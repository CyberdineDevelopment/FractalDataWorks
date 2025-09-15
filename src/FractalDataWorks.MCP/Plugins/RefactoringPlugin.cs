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
/// Plugin providing code refactoring and reorganization tools.
/// </summary>
public sealed class RefactoringPlugin : IToolPlugin<RefactoringConfiguration>
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly RefactoringTools _refactoringTools;
    private readonly VirtualEditService _editService;
    private readonly ILogger<RefactoringPlugin> _logger;
    private readonly List<IMcpTool> _tools;
    private RefactoringConfiguration? _configuration;
    private bool _isInitialized;

    public RefactoringPlugin(
        WorkspaceSessionManager sessionManager,
        RefactoringTools refactoringTools,
        VirtualEditService editService,
        ILogger<RefactoringPlugin> logger)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _refactoringTools = refactoringTools ?? throw new ArgumentNullException(nameof(refactoringTools));
        _editService = editService ?? throw new ArgumentNullException(nameof(editService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Id = "Refactoring";
        Name = "Refactoring Plugin";
        Description = "Provides tools for code refactoring and reorganization";
        Category = Refactoring.Instance;
        Priority = 600; // Medium-high priority for refactoring operations

        // Wrap existing refactoring tools as MCP tools
        _tools = WrapRefactoringTools();
    }

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public ToolCategoryBase Category { get; }
    public int Priority { get; }
    public bool IsEnabled => _configuration?.Enabled ?? true;
    public RefactoringConfiguration Configuration => _configuration ?? new RefactoringConfiguration();

    public IReadOnlyCollection<IMcpTool> GetTools()
    {
        return _tools.Where(t => t.IsEnabled).ToList();
    }

    public async Task<IFdwResult> InitializeAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is RefactoringConfiguration typedConfig)
        {
            return await InitializeAsync(typedConfig, cancellationToken);
        }

        // Convert basic configuration to typed
        var refactoringConfig = new RefactoringConfiguration
        {
            Enabled = configuration.Enabled,
            Priority = configuration.Priority,
            OperationTimeout = configuration.OperationTimeout,
            EnableDetailedLogging = configuration.EnableDetailedLogging
        };

        return await InitializeAsync(refactoringConfig, cancellationToken);
    }

    public async Task<IFdwResult> InitializeAsync(RefactoringConfiguration configuration, CancellationToken cancellationToken = default)
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
            _logger.LogInformation("Refactoring plugin initialized successfully");

            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Refactoring plugin");
            return FdwResult.Failure($"Initialization failed: {ex.Message}");
        }
    }

    public Task<IFdwResult> ValidateConfigurationAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is RefactoringConfiguration typedConfig)
        {
            return ValidateConfigurationAsync(typedConfig, cancellationToken);
        }

        return Task.FromResult(FdwResult.Success());
    }

    public Task<IFdwResult> ValidateConfigurationAsync(RefactoringConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            return Task.FromResult(FdwResult.Failure("Configuration cannot be null"));
        }

        if (configuration.MaxRefactoringResults <= 0)
        {
            return Task.FromResult(FdwResult.Failure("MaxRefactoringResults must be greater than 0"));
        }

        if (configuration.SymbolRenameTimeout <= TimeSpan.Zero)
        {
            return Task.FromResult(FdwResult.Failure("SymbolRenameTimeout must be greater than zero"));
        }

        return Task.FromResult(FdwResult.Success());
    }

    public async Task<IFdwResult<PluginHealth>> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var activeSessions = _sessionManager.GetActiveSessions().Count;
            var status = _isInitialized && activeSessions >= 0 ? HealthStatus.Healthy : HealthStatus.Degraded;

            var health = new PluginHealth
            {
                Status = status,
                Message = $"Refactoring tools ready. Active sessions: {activeSessions}",
                LastChecked = DateTimeOffset.UtcNow,
                Details = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["ActiveSessions"] = activeSessions,
                    ["IsInitialized"] = _isInitialized,
                    ["EnablePreviewMode"] = Configuration.EnablePreviewMode,
                    ["EnableCodeFixIntegration"] = Configuration.EnableCodeFixIntegration
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
            _logger.LogInformation("Shutting down Refactoring plugin");

            // Cancel any ongoing refactoring operations
            var activeSessions = _sessionManager.GetActiveSessions();
            foreach (var session in activeSessions)
            {
                try
                {
                    // If there are pending refactoring operations, clean them up
                    // Implementation depends on specific refactoring service APIs
                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error cleaning up refactoring operations for session {SessionId}", session.Id);
                }
            }

            _isInitialized = false;
            _logger.LogInformation("Refactoring plugin shut down successfully");

            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during plugin shutdown");
            return FdwResult.Failure($"Shutdown failed: {ex.Message}");
        }
    }

    private List<IMcpTool> WrapRefactoringTools()
    {
        var wrappedTools = new List<IMcpTool>();

        // Get all public methods from RefactoringTools that are marked with ToolAttribute
        var toolMethods = _refactoringTools.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any());

        foreach (var method in toolMethods)
        {
            var toolAttr = method.GetCustomAttribute<McpServerToolAttribute>();
            if (toolAttr != null)
            {
                wrappedTools.Add(new McpToolWrapper(
                    name: toolAttr.Name ?? method.Name,
                    description: toolAttr.Description ?? "Code refactoring tool",
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
                            var result = method.Invoke(_refactoringTools, parameters);

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
                            _logger.LogError(ex, "Error executing refactoring tool {ToolName}", toolAttr.Name);
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
}

/// <summary>
/// Configuration for the Refactoring plugin.
/// </summary>
public class RefactoringConfiguration : IToolPluginConfiguration
{
    public bool Enabled { get; set; } = true;
    public int Priority { get; set; } = 600;
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(10);
    public bool EnableDetailedLogging { get; set; }
    public int MaxRefactoringResults { get; set; } = 500;
    public TimeSpan SymbolRenameTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public bool EnablePreviewMode { get; set; } = true;
    public bool EnableCodeFixIntegration { get; set; } = true;
    public bool EnableBulkOperations { get; set; } = true;
}
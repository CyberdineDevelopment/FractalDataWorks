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
/// Plugin providing project dependency analysis and management tools.
/// </summary>
public sealed class ProjectDependenciesPlugin : IToolPlugin<ProjectDependenciesConfiguration>
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly ProjectDependencyTools _dependencyTools;
    private readonly ProjectDependencyService _dependencyService;
    private readonly ILogger<ProjectDependenciesPlugin> _logger;
    private readonly List<IMcpTool> _tools;
    private ProjectDependenciesConfiguration? _configuration;
    private bool _isInitialized;

    public ProjectDependenciesPlugin(
        WorkspaceSessionManager sessionManager,
        ProjectDependencyTools dependencyTools,
        ProjectDependencyService dependencyService,
        ILogger<ProjectDependenciesPlugin> logger)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _dependencyTools = dependencyTools ?? throw new ArgumentNullException(nameof(dependencyTools));
        _dependencyService = dependencyService ?? throw new ArgumentNullException(nameof(dependencyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Id = "ProjectDependencies";
        Name = "Project Dependencies Plugin";
        Description = "Provides tools for analyzing project references and dependencies";
        Category = ProjectDependencies.Instance;
        Priority = 400; // Medium priority for dependency analysis

        // Wrap existing project dependency tools as MCP tools
        _tools = WrapProjectDependencyTools();
    }

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public ToolCategoryBase Category { get; }
    public int Priority { get; }
    public bool IsEnabled => _configuration?.Enabled ?? true;
    public ProjectDependenciesConfiguration Configuration => _configuration ?? new ProjectDependenciesConfiguration();

    public IReadOnlyCollection<IMcpTool> GetTools()
    {
        return _tools.Where(t => t.IsEnabled).ToList();
    }

    public async Task<IFdwResult> InitializeAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is ProjectDependenciesConfiguration typedConfig)
        {
            return await InitializeAsync(typedConfig, cancellationToken);
        }

        // Convert basic configuration to typed
        var dependenciesConfig = new ProjectDependenciesConfiguration
        {
            Enabled = configuration.Enabled,
            Priority = configuration.Priority,
            OperationTimeout = configuration.OperationTimeout,
            EnableDetailedLogging = configuration.EnableDetailedLogging
        };

        return await InitializeAsync(dependenciesConfig, cancellationToken);
    }

    public async Task<IFdwResult> InitializeAsync(ProjectDependenciesConfiguration configuration, CancellationToken cancellationToken = default)
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
            _logger.LogInformation("Project Dependencies plugin initialized successfully");

            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Project Dependencies plugin");
            return FdwResult.Failure($"Initialization failed: {ex.Message}");
        }
    }

    public Task<IFdwResult> ValidateConfigurationAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is ProjectDependenciesConfiguration typedConfig)
        {
            return ValidateConfigurationAsync(typedConfig, cancellationToken);
        }

        return Task.FromResult(FdwResult.Success());
    }

    public Task<IFdwResult> ValidateConfigurationAsync(ProjectDependenciesConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            return Task.FromResult(FdwResult.Failure("Configuration cannot be null"));
        }

        if (configuration.MaxDependencyDepth <= 0)
        {
            return Task.FromResult(FdwResult.Failure("MaxDependencyDepth must be greater than 0"));
        }

        if (configuration.DependencyAnalysisTimeout <= TimeSpan.Zero)
        {
            return Task.FromResult(FdwResult.Failure("DependencyAnalysisTimeout must be greater than zero"));
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
                Message = $"Project dependency analysis ready. Active sessions: {activeSessions}",
                LastChecked = DateTimeOffset.UtcNow,
                Details = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["ActiveSessions"] = activeSessions,
                    ["IsInitialized"] = _isInitialized,
                    ["EnableCaching"] = Configuration.EnableDependencyCaching,
                    ["MaxDepth"] = Configuration.MaxDependencyDepth
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
            _logger.LogInformation("Shutting down Project Dependencies plugin");

            // Clear any cached dependency graphs
            var activeSessions = _sessionManager.GetActiveSessions();
            foreach (var session in activeSessions)
            {
                try
                {
                    // Clear cached dependency data for the session
                    // Implementation depends on ProjectDependencyService API
                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error clearing dependency cache for session {SessionId}", session.Id);
                }
            }

            _isInitialized = false;
            _logger.LogInformation("Project Dependencies plugin shut down successfully");

            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during plugin shutdown");
            return FdwResult.Failure($"Shutdown failed: {ex.Message}");
        }
    }

    private List<IMcpTool> WrapProjectDependencyTools()
    {
        var wrappedTools = new List<IMcpTool>();

        // Get all public methods from ProjectDependencyTools that are marked with ToolAttribute
        var toolMethods = _dependencyTools.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any());

        foreach (var method in toolMethods)
        {
            var toolAttr = method.GetCustomAttribute<McpServerToolAttribute>();
            if (toolAttr != null)
            {
                wrappedTools.Add(new McpToolWrapper(
                    name: toolAttr.Name ?? method.Name,
                    description: toolAttr.Description ?? "Project dependency analysis tool",
                    plugin: this,
                    category: Category,
                    executeFunc: async (args, ct) =>
                    {
                        try
                        {
                            // Apply timeout from configuration
                            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                            timeoutCts.CancelAfter(Configuration.DependencyAnalysisTimeout);

                            var parameters = ConvertArgsToParameters(args, method);
                            var result = method.Invoke(_dependencyTools, parameters);

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
                            _logger.LogError(ex, "Error executing project dependency tool {ToolName}", toolAttr.Name);
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
/// Configuration for the Project Dependencies plugin.
/// </summary>
public class ProjectDependenciesConfiguration : IToolPluginConfiguration
{
    public bool Enabled { get; set; } = true;
    public int Priority { get; set; } = 400;
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(3);
    public bool EnableDetailedLogging { get; set; }
    public int MaxDependencyDepth { get; set; } = 10;
    public TimeSpan DependencyAnalysisTimeout { get; set; } = TimeSpan.FromMinutes(2);
    public bool EnableDependencyCaching { get; set; } = true;
    public bool EnableImpactAnalysis { get; set; } = true;
    public bool EnableCompilationOrder { get; set; } = true;
}
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
/// Plugin providing type discovery, resolution, and analysis tools.
/// </summary>
public sealed class TypeAnalysisPlugin : IToolPlugin<TypeAnalysisConfiguration>
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly TypeAnalysisTools _typeAnalysisTools;
    private readonly TypeResolutionTools _typeResolutionTools;
    private readonly ILogger<TypeAnalysisPlugin> _logger;
    private readonly List<IMcpTool> _tools;
    private TypeAnalysisConfiguration? _configuration;
    private bool _isInitialized;

    public TypeAnalysisPlugin(
        WorkspaceSessionManager sessionManager,
        TypeAnalysisTools typeAnalysisTools,
        TypeResolutionTools typeResolutionTools,
        ILogger<TypeAnalysisPlugin> logger)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _typeAnalysisTools = typeAnalysisTools ?? throw new ArgumentNullException(nameof(typeAnalysisTools));
        _typeResolutionTools = typeResolutionTools ?? throw new ArgumentNullException(nameof(typeResolutionTools));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Id = "TypeAnalysis";
        Name = "Type Analysis Plugin";
        Description = "Provides tools for type discovery, resolution, and analysis";
        Category = TypeAnalysis.Instance;
        Priority = 500; // Medium priority for analysis tools

        // Wrap existing type analysis and resolution tools as MCP tools
        _tools = WrapTypeAnalysisTools();
    }

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public ToolCategoryBase Category { get; }
    public int Priority { get; }
    public bool IsEnabled => _configuration?.Enabled ?? true;
    public TypeAnalysisConfiguration Configuration => _configuration ?? new TypeAnalysisConfiguration();

    public IReadOnlyCollection<IMcpTool> GetTools()
    {
        return _tools.Where(t => t.IsEnabled).ToList();
    }

    public async Task<IFdwResult> InitializeAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is TypeAnalysisConfiguration typedConfig)
        {
            return await InitializeAsync(typedConfig, cancellationToken);
        }

        // Convert basic configuration to typed
        var analysisConfig = new TypeAnalysisConfiguration
        {
            Enabled = configuration.Enabled,
            Priority = configuration.Priority,
            OperationTimeout = configuration.OperationTimeout,
            EnableDetailedLogging = configuration.EnableDetailedLogging
        };

        return await InitializeAsync(analysisConfig, cancellationToken);
    }

    public async Task<IFdwResult> InitializeAsync(TypeAnalysisConfiguration configuration, CancellationToken cancellationToken = default)
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
            _logger.LogInformation("Type Analysis plugin initialized successfully");

            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Type Analysis plugin");
            return FdwResult.Failure($"Initialization failed: {ex.Message}");
        }
    }

    public Task<IFdwResult> ValidateConfigurationAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is TypeAnalysisConfiguration typedConfig)
        {
            return ValidateConfigurationAsync(typedConfig, cancellationToken);
        }

        return Task.FromResult(FdwResult.Success());
    }

    public Task<IFdwResult> ValidateConfigurationAsync(TypeAnalysisConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            return Task.FromResult(FdwResult.Failure("Configuration cannot be null"));
        }

        if (configuration.MaxTypeSearchResults <= 0)
        {
            return Task.FromResult(FdwResult.Failure("MaxTypeSearchResults must be greater than 0"));
        }

        if (configuration.TypeAnalysisTimeout <= TimeSpan.Zero)
        {
            return Task.FromResult(FdwResult.Failure("TypeAnalysisTimeout must be greater than zero"));
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
                Message = $"Type analysis ready. Active sessions: {activeSessions}",
                LastChecked = DateTimeOffset.UtcNow,
                Details = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["ActiveSessions"] = activeSessions,
                    ["IsInitialized"] = _isInitialized,
                    ["EnableTypeDiscovery"] = Configuration.EnableTypeDiscovery,
                    ["EnableMissingTypeAnalysis"] = Configuration.EnableMissingTypeAnalysis
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
            _logger.LogInformation("Shutting down Type Analysis plugin");

            // Clear any cached type analysis data
            // Implementation depends on specific caching mechanisms
            await Task.CompletedTask;

            _isInitialized = false;
            _logger.LogInformation("Type Analysis plugin shut down successfully");

            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during plugin shutdown");
            return FdwResult.Failure($"Shutdown failed: {ex.Message}");
        }
    }

    private List<IMcpTool> WrapTypeAnalysisTools()
    {
        var wrappedTools = new List<IMcpTool>();

        // Wrap TypeAnalysisTools methods
        var analysisToolMethods = _typeAnalysisTools.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any());

        foreach (var method in analysisToolMethods)
        {
            var toolAttr = method.GetCustomAttribute<McpServerToolAttribute>();
            if (toolAttr != null)
            {
                wrappedTools.Add(new McpToolWrapper(
                    name: toolAttr.Name ?? method.Name,
                    description: toolAttr.Description ?? "Type analysis tool",
                    plugin: this,
                    category: Category,
                    executeFunc: async (args, ct) =>
                    {
                        try
                        {
                            // Apply timeout from configuration
                            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                            timeoutCts.CancelAfter(Configuration.TypeAnalysisTimeout);

                            var parameters = ConvertArgsToParameters(args, method);
                            var result = method.Invoke(_typeAnalysisTools, parameters);

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
                            _logger.LogError(ex, "Error executing type analysis tool {ToolName}", toolAttr.Name);
                            return FdwResult<object>.Failure($"Tool execution failed: {ex.Message}");
                        }
                    }));
            }
        }

        // Wrap TypeResolutionTools methods
        var resolutionToolMethods = _typeResolutionTools.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any());

        foreach (var method in resolutionToolMethods)
        {
            var toolAttr = method.GetCustomAttribute<McpServerToolAttribute>();
            if (toolAttr != null)
            {
                wrappedTools.Add(new McpToolWrapper(
                    name: toolAttr.Name ?? method.Name,
                    description: toolAttr.Description ?? "Type resolution tool",
                    plugin: this,
                    category: Category,
                    executeFunc: async (args, ct) =>
                    {
                        try
                        {
                            // Apply timeout from configuration
                            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                            timeoutCts.CancelAfter(Configuration.TypeAnalysisTimeout);

                            var parameters = ConvertArgsToParameters(args, method);
                            var result = method.Invoke(_typeResolutionTools, parameters);

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
                            _logger.LogError(ex, "Error executing type resolution tool {ToolName}", toolAttr.Name);
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
/// Configuration for the Type Analysis plugin.
/// </summary>
public class TypeAnalysisConfiguration : IToolPluginConfiguration
{
    public bool Enabled { get; set; } = true;
    public int Priority { get; set; } = 500;
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(3);
    public bool EnableDetailedLogging { get; set; }
    public int MaxTypeSearchResults { get; set; } = 1000;
    public TimeSpan TypeAnalysisTimeout { get; set; } = TimeSpan.FromMinutes(2);
    public bool EnableTypeDiscovery { get; set; } = true;
    public bool EnableMissingTypeAnalysis { get; set; } = true;
    public bool EnableTypeStatistics { get; set; } = true;
}
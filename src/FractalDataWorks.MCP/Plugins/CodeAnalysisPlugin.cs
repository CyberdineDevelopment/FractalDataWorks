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
/// Plugin providing code analysis and diagnostic tools.
/// </summary>
public sealed class CodeAnalysisPlugin : IToolPlugin<CodeAnalysisConfiguration>
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly DiagnosticTools _diagnosticTools;
    private readonly ILogger<CodeAnalysisPlugin> _logger;
    private readonly List<IMcpTool> _tools;
    private CodeAnalysisConfiguration? _configuration;
    private bool _isInitialized;

    public CodeAnalysisPlugin(
        WorkspaceSessionManager sessionManager,
        DiagnosticTools diagnosticTools,
        ILogger<CodeAnalysisPlugin> logger)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _diagnosticTools = diagnosticTools ?? throw new ArgumentNullException(nameof(diagnosticTools));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Id = "CodeAnalysis";
        Name = "Code Analysis Plugin";
        Description = "Provides tools for analyzing code quality, diagnostics, and compiler errors";
        Category = CodeAnalysis.Instance;
        Priority = 800; // High priority for core analysis

        // Wrap existing diagnostic tools as MCP tools
        _tools = WrapDiagnosticTools();
    }

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public ToolCategoryBase Category { get; }
    public int Priority { get; }
    public bool IsEnabled => _configuration?.Enabled ?? true;
    public CodeAnalysisConfiguration Configuration => _configuration ?? new CodeAnalysisConfiguration();

    public IReadOnlyCollection<IMcpTool> GetTools()
    {
        return _tools.Where(t => t.IsEnabled).ToList();
    }

    public async Task<IFdwResult> InitializeAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is CodeAnalysisConfiguration typedConfig)
        {
            return await InitializeAsync(typedConfig, cancellationToken);
        }

        // Convert basic configuration to typed
        var analysisConfig = new CodeAnalysisConfiguration
        {
            Enabled = configuration.Enabled,
            Priority = configuration.Priority,
            OperationTimeout = configuration.OperationTimeout,
            EnableDetailedLogging = configuration.EnableDetailedLogging
        };

        return await InitializeAsync(analysisConfig, cancellationToken);
    }

    public async Task<IFdwResult> InitializeAsync(CodeAnalysisConfiguration configuration, CancellationToken cancellationToken = default)
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
            _logger.LogInformation("Code Analysis plugin initialized successfully");

            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Code Analysis plugin");
            return FdwResult.Failure($"Initialization failed: {ex.Message}");
        }
    }

    public Task<IFdwResult> ValidateConfigurationAsync(IToolPluginConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is CodeAnalysisConfiguration typedConfig)
        {
            return ValidateConfigurationAsync(typedConfig, cancellationToken);
        }

        return Task.FromResult(FdwResult.Success());
    }

    public Task<IFdwResult> ValidateConfigurationAsync(CodeAnalysisConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            return Task.FromResult(FdwResult.Failure("Configuration cannot be null"));
        }

        if (configuration.MaxDiagnosticsResults <= 0)
        {
            return Task.FromResult(FdwResult.Failure("MaxDiagnosticsResults must be greater than 0"));
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
                Message = $"Code analysis ready. Active sessions: {activeSessions}",
                LastChecked = DateTimeOffset.UtcNow,
                Details = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["ActiveSessions"] = activeSessions,
                    ["IsInitialized"] = _isInitialized,
                    ["IncludeAnalyzerDiagnostics"] = Configuration.IncludeAnalyzerDiagnostics
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
            _logger.LogInformation("Shutting down Code Analysis plugin");

            _isInitialized = false;
            _logger.LogInformation("Code Analysis plugin shut down successfully");

            return await Task.FromResult(FdwResult.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during plugin shutdown");
            return FdwResult.Failure($"Shutdown failed: {ex.Message}");
        }
    }

    private List<IMcpTool> WrapDiagnosticTools()
    {
        var wrappedTools = new List<IMcpTool>();

        // Get all public methods from DiagnosticTools that are marked with ToolAttribute
        var toolMethods = _diagnosticTools.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any());

        foreach (var method in toolMethods)
        {
            var toolAttr = method.GetCustomAttribute<McpServerToolAttribute>();
            if (toolAttr != null)
            {
                wrappedTools.Add(new McpToolWrapper(
                    name: toolAttr.Name ?? method.Name,
                    description: toolAttr.Description ?? "Code analysis diagnostic tool",
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
                            var result = method.Invoke(_diagnosticTools, parameters);

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
                            _logger.LogError(ex, "Error executing diagnostic tool {ToolName}", toolAttr.Name);
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
/// Configuration for the Code Analysis plugin.
/// </summary>
public class CodeAnalysisConfiguration : IToolPluginConfiguration
{
    public bool Enabled { get; set; } = true;
    public int Priority { get; set; } = 800;
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(2);
    public bool EnableDetailedLogging { get; set; }
    public bool IncludeAnalyzerDiagnostics { get; set; } = true;
    public int MaxDiagnosticsResults { get; set; } = 1000;
    public bool EnableDiagnosticCaching { get; set; } = true;
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FractalDataWorks.MCP.Abstractions;
using FractalDataWorks.MCP.Plugins;
using RoslynMcpServer.Services;
using RoslynMcpServer.Tools;
using RoslynMcpServer.Tools;

namespace FractalDataWorks.MCP.Extensions;

/// <summary>
/// Extension methods for registering MCP services and plugins.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all MCP services, tools, and plugins.
    /// </summary>
    public static IServiceCollection AddFractalMcpServices(this IServiceCollection services)
    {
        // Register core services
        services.TryAddSingleton<IPluginRegistry, PluginRegistry>();
        services.TryAddSingleton<WorkspaceSessionManager>();
        services.TryAddSingleton<VirtualEditService>();
        services.TryAddSingleton<ProjectDependencyService>();
        services.TryAddSingleton<CompilationCacheService>();
        services.TryAddSingleton<AnalyzerService>();
        services.TryAddSingleton<FileSystemWatcherService>();

        // Register existing tool classes (these will be wrapped by plugins)
        services.TryAddSingleton<SessionTools>();
        services.TryAddSingleton<SessionLifecycleTools>();
        services.TryAddSingleton<DiagnosticTools>();
        services.TryAddSingleton<VirtualEditTools>();
        services.TryAddSingleton<RefactoringTools>();
        services.TryAddSingleton<TypeAnalysisTools>();
        services.TryAddSingleton<TypeResolutionTools>();
        services.TryAddSingleton<ProjectDependencyTools>();

        // Register management tools
        services.TryAddSingleton<ServerInfoTool>();
        services.TryAddSingleton<ServerShutdownTool>();
        services.TryAddSingleton<ServerRestartTool>();
        services.TryAddSingleton<ErrorReportingTool>();

        // Register plugins with their dependencies
        services.TryAddSingleton<SessionManagementPlugin>();
        services.TryAddSingleton<CodeAnalysisPlugin>();
        services.TryAddSingleton<VirtualEditingPlugin>();
        services.TryAddSingleton<RefactoringPlugin>();
        services.TryAddSingleton<TypeAnalysisPlugin>();
        services.TryAddSingleton<ProjectDependenciesPlugin>();
        services.TryAddSingleton<ServerManagementPlugin>();

        return services;
    }

    /// <summary>
    /// Registers and initializes all MCP plugins.
    /// </summary>
    public static IServiceCollection AddFractalMcpPlugins(this IServiceCollection services)
    {
        // Add core services first
        services.AddFractalMcpServices();

        // The PluginRegistry will auto-discover and register plugins via reflection
        // No explicit registration needed here since plugins implement IToolPlugin
        // and the registry will find them automatically

        return services;
    }

    /// <summary>
    /// Registers MCP plugins with custom configurations.
    /// </summary>
    public static IServiceCollection AddFractalMcpPlugins(
        this IServiceCollection services,
        Action<McpPluginOptions>? configureOptions = null)
    {
        services.AddFractalMcpPlugins();

        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        return services;
    }
}

/// <summary>
/// Configuration options for MCP plugins.
/// </summary>
public class McpPluginOptions
{
    /// <summary>
    /// Gets or sets whether to enable auto-discovery of plugins.
    /// </summary>
    public bool EnableAutoDiscovery { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to initialize plugins during startup.
    /// </summary>
    public bool InitializePluginsOnStartup { get; set; } = true;

    /// <summary>
    /// Gets or sets the plugin configuration overrides.
    /// </summary>
    public Dictionary<string, IToolPluginConfiguration> PluginConfigurations { get; set; } =
        new(StringComparer.Ordinal);
}
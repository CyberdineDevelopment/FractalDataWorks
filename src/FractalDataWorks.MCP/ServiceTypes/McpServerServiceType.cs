using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.MCP.Abstractions;
using FractalDataWorks.MCP.Services;
using FractalDataWorks.MCP.Configuration;

namespace FractalDataWorks.MCP.ServiceTypes;

/// <summary>
/// Service type definition for the MCP server service.
/// Provides Model Context Protocol server capabilities with plugin-based tool discovery.
/// </summary>
public sealed class McpServerServiceType : ServiceTypeBase<IMcpServerService, McpServerConfiguration, IMcpServerServiceFactory>
{
    /// <summary>
    /// Gets the singleton instance of the MCP server service type.
    /// </summary>
    public static McpServerServiceType Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="McpServerServiceType"/> class.
    /// </summary>
    private McpServerServiceType() : base(1001, "McpServer", "MCP Services")
    {
    }

    /// <inheritdoc/>
    public override string SectionName => "RoslynMcpServer";

    /// <inheritdoc/>
    public override string DisplayName => "MCP Server";

    /// <inheritdoc/>
    public override string Description => "Model Context Protocol server for Roslyn-based code analysis and refactoring tools";

    /// <inheritdoc/>
    public override Type FactoryType => typeof(IMcpServerServiceFactory);

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        // Register MCP server factory and services
        services.AddSingleton<IMcpServerServiceFactory, McpServerServiceFactory>();
        services.AddSingleton<IMcpServerService, McpServerService>();

        // Register plugin system
        services.AddSingleton<IPluginRegistry, PluginRegistry>();
        services.AddSingleton<IPluginLoader, PluginLoader>();
        services.AddSingleton<IPluginHealthMonitor, PluginHealthMonitor>();

        // Register core services (workspace, compilation, etc.)
        services.AddSingleton<WorkspaceSessionManager>();
        services.AddSingleton<CompilationCacheService>();
        services.AddSingleton<VirtualEditService>();
        services.AddSingleton<FileSystemWatcherService>();
        services.AddSingleton<AnalyzerService>();
        services.AddSingleton<ProjectDependencyService>();

        // Plugin discovery will automatically register tool plugins
        // via TypeCollection attributes
    }

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        var config = configuration.GetSection(SectionName).Get<McpServerConfiguration>();
        if (config == null)
        {
            throw new InvalidOperationException($"Configuration section '{SectionName}' not found");
        }

        // Validate configuration
        var validator = new McpServerConfigurationValidator();
        var validationResult = validator.Validate(config);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors);
            throw new InvalidOperationException($"Invalid MCP server configuration: {errors}");
        }
    }
}
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.MCP.Connections;
using FractalDataWorks.MCP.Configuration;

namespace FractalDataWorks.MCP.ConnectionTypes;

/// <summary>
/// Connection type for Roslyn workspace connections.
/// Provides access to MSBuild/Roslyn compilation workspaces for code analysis.
/// </summary>
public sealed class RoslynWorkspaceConnectionType : ConnectionTypeBase<IRoslynWorkspaceConnection, RoslynWorkspaceConfiguration, IRoslynWorkspaceConnectionFactory>
{
    /// <summary>
    /// Gets the singleton instance of the Roslyn workspace connection type.
    /// </summary>
    public static RoslynWorkspaceConnectionType Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RoslynWorkspaceConnectionType"/> class.
    /// </summary>
    private RoslynWorkspaceConnectionType() : base(2001, "RoslynWorkspace", "Development Tools")
    {
    }

    /// <inheritdoc/>
    public override Type FactoryType => typeof(IRoslynWorkspaceConnectionFactory);

    /// <inheritdoc/>
    public override string DisplayName => "Roslyn Workspace";

    /// <inheritdoc/>
    public override string Description => "Connection to Roslyn compilation workspaces for code analysis and refactoring";

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        // Register Roslyn workspace specific services
        services.AddScoped<IRoslynWorkspaceConnectionFactory, RoslynWorkspaceConnectionFactory>();
        services.AddScoped<RoslynWorkspaceService>();
        services.AddScoped<WorkspaceCommandTranslator>();
        services.AddScoped<CompilationProvider>();
        services.AddScoped<DocumentAnalyzer>();
        services.AddScoped<SymbolResolver>();

        // Register workspace session management
        services.AddSingleton<WorkspaceSessionPool>();
        services.AddScoped<WorkspaceHealthChecker>();
    }

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        var config = configuration.GetSection("RoslynWorkspace").Get<RoslynWorkspaceConfiguration>();
        if (config != null)
        {
            // Validate workspace configuration
            if (config.MaxConcurrentSessions <= 0)
            {
                throw new InvalidOperationException("MaxConcurrentSessions must be greater than 0");
            }

            if (config.SessionTimeoutMinutes <= 0)
            {
                throw new InvalidOperationException("SessionTimeoutMinutes must be greater than 0");
            }
        }
    }
}
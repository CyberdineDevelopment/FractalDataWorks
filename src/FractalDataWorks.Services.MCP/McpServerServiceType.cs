using System;
using FractalDataWorks.ServiceTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.MCP;

/// <summary>
/// Service type configuration for MCP Server that provides code analysis and refactoring capabilities.
/// </summary>
public class McpServerServiceType : ServiceTypeBase<McpOrchestrationService, object, object>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="McpServerServiceType"/> class.
    /// </summary>
    public McpServerServiceType() : base(
        id: 1001,
        name: "McpServer",
        sectionName: "MCP:Server",
        displayName: "MCP Server",
        description: "MCP Server Service for code analysis and refactoring",
        category: "Development Tools")
    {
    }

    /// <summary>
    /// Registers the MCP orchestration service with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    public override void Register(IServiceCollection services)
    {
        services.AddSingleton<McpOrchestrationService>();
    }

    /// <summary>
    /// Configures the MCP server service with the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    public override void Configure(IConfiguration configuration)
    {
        // Configuration will be added as needed
    }
}
using System;
using FractalDataWorks.ServiceTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.MCP;

public class McpServerServiceType : ServiceTypeBase<McpOrchestrationService, object, object>
{
    public McpServerServiceType() : base(
        id: 1001,
        name: "McpServer",
        sectionName: "MCP:Server",
        displayName: "MCP Server",
        description: "MCP Server Service for code analysis and refactoring",
        category: "Development Tools")
    {
    }

    public override void Register(IServiceCollection services)
    {
        services.AddSingleton<McpOrchestrationService>();
    }

    public override void Configure(IConfiguration configuration)
    {
        // Configuration will be added as needed
    }
}
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using FractalDataWorks.McpTools.Abstractions;
using FractalDataWorks.McpTools.Abstractions.Logging;

namespace FractalDataWorks.McpTools.ProjectDependencies;

/// <summary>
/// Service providing project dependency analysis tools for MCP.
/// </summary>
public class ProjectDependenciesToolService
{
    private readonly ILogger<ProjectDependenciesToolService> _logger;
    private readonly List<IMcpTool> _tools;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectDependenciesToolService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ProjectDependenciesToolService(ILogger<ProjectDependenciesToolService> logger)
    {
        _logger = logger;
        ServiceName = "Project Dependencies Tools";
        Category = "ProjectDependencies";

        // Initialize tools
        _tools = new List<IMcpTool>();
        RegisterTools();
    }

    /// <inheritdoc />
    public string ServiceName { get; }

    /// <inheritdoc />
    public string Category { get; }

    /// <inheritdoc />
    public IEnumerable<IMcpTool> GetTools() => _tools;

    private void RegisterTools()
    {
        // Tools will be registered here as we migrate them
        McpToolServiceLog.ToolsRegistered(_logger, _tools.Count, "project dependency");
    }
}
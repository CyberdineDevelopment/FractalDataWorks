using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using FractalDataWorks.McpTools.Abstractions;
using FractalDataWorks.McpTools.Abstractions.Logging;

namespace FractalDataWorks.McpTools.ServerManagement;

/// <summary>
/// Service providing server management tools for MCP.
/// </summary>
public class ServerManagementToolService
{
    private readonly ILogger<ServerManagementToolService> _logger;
    private readonly List<IMcpTool> _tools;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerManagementToolService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ServerManagementToolService(ILogger<ServerManagementToolService> logger)
    {
        _logger = logger;
        ServiceName = "Server Management Tools";
        Category = "ServerManagement";

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
        McpToolServiceLog.ToolsRegistered(_logger, _tools.Count, "server management");
    }
}
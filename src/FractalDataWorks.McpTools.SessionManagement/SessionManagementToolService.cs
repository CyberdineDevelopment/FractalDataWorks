using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using FractalDataWorks.McpTools.Abstractions;

namespace FractalDataWorks.McpTools.SessionManagement;

/// <summary>
/// Service providing session management tools for MCP.
/// </summary>
public class SessionManagementToolService
{
    private readonly ILogger<SessionManagementToolService> _logger;
    private readonly List<IMcpTool> _tools;

    public SessionManagementToolService(ILogger<SessionManagementToolService> logger)
    {
        _logger = logger;
        ServiceName = "Session Management Tools";
        Category = "SessionManagement";

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
        _logger.LogInformation("Registered {Count} session management tools", _tools.Count);
    }
}
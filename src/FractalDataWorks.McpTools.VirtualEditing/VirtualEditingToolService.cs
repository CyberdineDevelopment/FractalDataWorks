using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using FractalDataWorks.McpTools.Abstractions;

namespace FractalDataWorks.McpTools.VirtualEditing;

/// <summary>
/// Service providing virtual editing tools for MCP.
/// </summary>
public class VirtualEditingToolService
{
    private readonly ILogger<VirtualEditingToolService> _logger;
    private readonly List<IMcpTool> _tools;

    public VirtualEditingToolService(ILogger<VirtualEditingToolService> logger)
    {
        _logger = logger;
        ServiceName = "Virtual Editing Tools";
        Category = "VirtualEditing";

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
        _logger.LogInformation("Registered {Count} virtual editing tools", _tools.Count);
    }
}
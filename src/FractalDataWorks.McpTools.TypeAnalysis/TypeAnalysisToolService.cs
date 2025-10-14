using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using FractalDataWorks.McpTools.Abstractions;
using FractalDataWorks.McpTools.Abstractions.Logging;

namespace FractalDataWorks.McpTools.TypeAnalysis;

/// <summary>
/// Service providing type analysis tools for MCP.
/// </summary>
public class TypeAnalysisToolService
{
    private readonly ILogger<TypeAnalysisToolService> _logger;
    private readonly List<IMcpTool> _tools;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeAnalysisToolService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public TypeAnalysisToolService(ILogger<TypeAnalysisToolService> logger)
    {
        _logger = logger;
        ServiceName = "Type Analysis Tools";
        Category = "TypeAnalysis";

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
        McpToolServiceLog.ToolsRegistered(_logger, _tools.Count, "type analysis");
    }
}
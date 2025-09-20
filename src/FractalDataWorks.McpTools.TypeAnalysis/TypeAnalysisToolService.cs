using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using FractalDataWorks.McpTools.Abstractions;

namespace FractalDataWorks.McpTools.TypeAnalysis;

/// <summary>
/// Service providing type analysis tools for MCP.
/// </summary>
public class TypeAnalysisToolService
{
    private readonly ILogger<TypeAnalysisToolService> _logger;
    private readonly List<IMcpTool> _tools;

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
        _logger.LogInformation("Registered {Count} type analysis tools", _tools.Count);
    }
}
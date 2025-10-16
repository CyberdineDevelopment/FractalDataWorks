using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using FractalDataWorks.McpTools.Abstractions;
using FractalDataWorks.McpTools.Abstractions.Logging;

namespace FractalDataWorks.McpTools.CodeAnalysis;

/// <summary>
/// Service providing code analysis and diagnostic tools for MCP.
/// </summary>
public class CodeAnalysisToolService
{
    private readonly ILogger<CodeAnalysisToolService> _logger;
    private readonly List<IMcpTool> _tools;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeAnalysisToolService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public CodeAnalysisToolService(ILogger<CodeAnalysisToolService> logger)
    {
        _logger = logger;
        ServiceName = "Code Analysis Tools";
        Category = "CodeAnalysis";

        // Initialize tools
        _tools = [];
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
        McpToolServiceLog.ToolsRegistered(_logger, _tools.Count, "code analysis");
    }
}
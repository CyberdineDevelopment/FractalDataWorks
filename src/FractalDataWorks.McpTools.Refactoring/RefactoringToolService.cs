using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using FractalDataWorks.McpTools.Abstractions;

namespace FractalDataWorks.McpTools.Refactoring;

/// <summary>
/// Service providing refactoring tools for MCP.
/// </summary>
public class RefactoringToolService
{
    private readonly ILogger<RefactoringToolService> _logger;
    private readonly List<IMcpTool> _tools;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefactoringToolService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public RefactoringToolService(ILogger<RefactoringToolService> logger)
    {
        _logger = logger;
        ServiceName = "Refactoring Tools";
        Category = "Refactoring";

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
        _logger.LogInformation("Registered {Count} refactoring tools", _tools.Count);
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.McpTools.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services.MCP;

/// <summary>
/// Interface for the MCP orchestration service that manages and coordinates tool services.
/// </summary>
public interface IMcpOrchestrationService
{
    /// <summary>
    /// Gets all available MCP tools.
    /// </summary>
    IEnumerable<IMcpTool> GetAllTools();

    /// <summary>
    /// Gets tools by category.
    /// </summary>
    IEnumerable<IMcpTool> GetToolsByCategory(string category);

    /// <summary>
    /// Gets a tool by name.
    /// </summary>
    IMcpTool? GetToolByName(string name);

    /// <summary>
    /// Executes a tool by name with the provided arguments.
    /// </summary>
    Task<IGenericResult<object>> ExecuteToolAsync(string toolName, object? arguments, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available categories.
    /// </summary>
    IEnumerable<string> GetCategories();
}
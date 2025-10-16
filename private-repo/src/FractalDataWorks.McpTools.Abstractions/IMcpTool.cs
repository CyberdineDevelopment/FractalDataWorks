using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.McpTools.Abstractions;

/// <summary>
/// Base interface for MCP tools.
/// </summary>
public interface IMcpTool
{
    /// <summary>
    /// Gets the tool name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the tool description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the tool category.
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Gets whether this tool is currently enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets the tool priority within its category.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Executes the tool with Railway-oriented result handling.
    /// </summary>
    Task<IGenericResult<object>> ExecuteAsync(object? arguments, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the tool arguments before execution.
    /// </summary>
    Task<IGenericResult> ValidateArgumentsAsync(object? arguments, CancellationToken cancellationToken = default);
}
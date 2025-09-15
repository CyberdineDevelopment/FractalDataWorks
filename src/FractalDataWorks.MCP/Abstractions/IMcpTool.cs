using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using ModelContextProtocol.Abstractions;

namespace FractalDataWorks.MCP.Abstractions;

/// <summary>
/// Wrapper interface for MCP tools that integrates with the plugin system.
/// </summary>
public interface IMcpTool : ITool
{
    /// <summary>
    /// Gets the plugin that provides this tool.
    /// </summary>
    IToolPlugin OwningPlugin { get; }

    /// <summary>
    /// Gets the tool category.
    /// </summary>
    ToolCategoryBase Category { get; }

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
    Task<IFdwResult<object>> ExecuteAsync(object? arguments, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the tool arguments before execution.
    /// </summary>
    Task<IFdwResult> ValidateArgumentsAsync(object? arguments, CancellationToken cancellationToken = default);
}
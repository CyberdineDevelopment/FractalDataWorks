using FractalDataWorks.Mcp.Abstractions;

namespace FractalDataWorks.McpTools.ProjectDependencies;

/// <summary>
/// Represents the Project Dependencies tool type in the MCP tools collection.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ProjectDependenciesToolType"/> class.
/// </remarks>
public sealed class ProjectDependenciesToolType() : McpToolType(6,
    "ProjectDependencies",
    "Project Dependencies Tool",
    "Analyzes project dependencies and impact of changes across solutions");
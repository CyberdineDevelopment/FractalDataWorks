using FractalDataWorks.Mcp.Abstractions;

namespace FractalDataWorks.McpTools.ProjectDependencies;

/// <summary>
/// Represents the Project Dependencies tool type in the MCP tools collection.
/// </summary>
public sealed class ProjectDependenciesToolType : McpToolType
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="ProjectDependenciesToolType"/>.
    /// </summary>
    public static ProjectDependenciesToolType Instance { get; } = new();

    private ProjectDependenciesToolType()
        : base(6,
               "ProjectDependencies",
               "Project Dependencies Tool",
               "Analyzes project dependencies and impact of changes across solutions")
    {
    }
}
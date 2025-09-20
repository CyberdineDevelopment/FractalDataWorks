using FractalDataWorks.MCP.Abstractions;

namespace FractalDataWorks.McpTools.ProjectDependencies;

/// <summary>
/// Represents the Project Dependencies tool type in the MCP tools collection.
/// </summary>
public sealed class ProjectDependenciesToolType : McpToolType
{
    public static ProjectDependenciesToolType Instance { get; } = new();

    private ProjectDependenciesToolType()
        : base(6,
               "ProjectDependencies",
               "Project Dependencies Tool",
               "Analyzes project dependencies and impact of changes across solutions")
    {
    }
}
using FractalDataWorks.MCP.Abstractions;

namespace FractalDataWorks.McpTools.TypeAnalysis;

/// <summary>
/// Represents the Type Analysis tool type in the MCP tools collection.
/// </summary>
public sealed class TypeAnalysisToolType : McpToolType
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="TypeAnalysisToolType"/>.
    /// </summary>
    public static TypeAnalysisToolType Instance { get; } = new();

    private TypeAnalysisToolType()
        : base(5,
               "TypeAnalysis",
               "Type Analysis Tool",
               "Analyzes types, inheritance, and dependencies in .NET solutions")
    {
    }
}
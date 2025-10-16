using FractalDataWorks.Mcp.Abstractions;

namespace FractalDataWorks.McpTools.TypeAnalysis;

/// <summary>
/// Represents the Type Analysis tool type in the MCP tools collection.
/// </summary>
public sealed class TypeAnalysisToolType : McpToolType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeAnalysisToolType"/> class.
    /// </summary>
    public TypeAnalysisToolType()
        : base(5,
               "TypeAnalysis",
               "Type Analysis Tool",
               "Analyzes types, inheritance, and dependencies in .NET solutions")
    {
    }
}
using FractalDataWorks.Mcp.Abstractions;

namespace FractalDataWorks.McpTools.CodeAnalysis;

/// <summary>
/// Represents the Code Analysis tool type in the MCP tools collection.
/// </summary>
public sealed class CodeAnalysisToolType : McpToolType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CodeAnalysisToolType"/> class.
    /// </summary>
    public CodeAnalysisToolType()
        : base(1,
               "CodeAnalysis",
               "Code Analysis Tool",
               "Provides code analysis capabilities for .NET solutions using Roslyn")
    {
    }
}
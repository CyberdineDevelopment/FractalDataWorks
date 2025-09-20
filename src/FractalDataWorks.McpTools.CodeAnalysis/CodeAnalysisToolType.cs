using FractalDataWorks.MCP.Abstractions;

namespace FractalDataWorks.McpTools.CodeAnalysis;

/// <summary>
/// Represents the Code Analysis tool type in the MCP tools collection.
/// </summary>
public sealed class CodeAnalysisToolType : McpToolType
{
    public static CodeAnalysisToolType Instance { get; } = new();

    private CodeAnalysisToolType()
        : base(1,
               "CodeAnalysis",
               "Code Analysis Tool",
               "Provides code analysis capabilities for .NET solutions using Roslyn")
    {
    }
}
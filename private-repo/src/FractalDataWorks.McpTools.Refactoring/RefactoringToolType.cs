using FractalDataWorks.Mcp.Abstractions;

namespace FractalDataWorks.McpTools.Refactoring;

/// <summary>
/// Represents the Refactoring tool type in the MCP tools collection.
/// </summary>
public sealed class RefactoringToolType : McpToolType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RefactoringToolType"/> class.
    /// </summary>
    public RefactoringToolType()
        : base(3,
               "Refactoring",
               "Refactoring Tool",
               "Provides automated refactoring capabilities using Roslyn")
    {
    }
}
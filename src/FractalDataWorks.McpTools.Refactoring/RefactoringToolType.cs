using FractalDataWorks.MCP.Abstractions;

namespace FractalDataWorks.McpTools.Refactoring;

/// <summary>
/// Represents the Refactoring tool type in the MCP tools collection.
/// </summary>
public sealed class RefactoringToolType : McpToolType
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="RefactoringToolType"/>.
    /// </summary>
    public static RefactoringToolType Instance { get; } = new();

    private RefactoringToolType()
        : base(3,
               "Refactoring",
               "Refactoring Tool",
               "Provides automated refactoring capabilities using Roslyn")
    {
    }
}
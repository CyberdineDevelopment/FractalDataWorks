using FractalDataWorks.Mcp.Abstractions;

namespace FractalDataWorks.McpTools.VirtualEditing;

/// <summary>
/// Represents the Virtual Editing tool type in the MCP tools collection.
/// </summary>
public sealed class VirtualEditingToolType : McpToolType
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="VirtualEditingToolType"/>.
    /// </summary>
    public static VirtualEditingToolType Instance { get; } = new();

    private VirtualEditingToolType()
        : base(2,
               "VirtualEditing",
               "Virtual Editing Tool",
               "Provides virtual file editing capabilities for testing changes before committing")
    {
    }
}
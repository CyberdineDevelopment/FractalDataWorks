using FractalDataWorks.MCP.Abstractions;

namespace FractalDataWorks.McpTools.VirtualEditing;

/// <summary>
/// Represents the Virtual Editing tool type in the MCP tools collection.
/// </summary>
public sealed class VirtualEditingToolType : McpToolType
{
    public static VirtualEditingToolType Instance { get; } = new();

    private VirtualEditingToolType()
        : base(2,
               "VirtualEditing",
               "Virtual Editing Tool",
               "Provides virtual file editing capabilities for testing changes before committing")
    {
    }
}
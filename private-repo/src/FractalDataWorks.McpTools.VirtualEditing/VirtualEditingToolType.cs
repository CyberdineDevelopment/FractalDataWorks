using FractalDataWorks.Mcp.Abstractions;

namespace FractalDataWorks.McpTools.VirtualEditing;

/// <summary>
/// Represents the Virtual Editing tool type in the MCP tools collection.
/// </summary>
public sealed class VirtualEditingToolType : McpToolType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualEditingToolType"/> class.
    /// </summary>
    public VirtualEditingToolType()
        : base(2,
               "VirtualEditing",
               "Virtual Editing Tool",
               "Provides virtual file editing capabilities for testing changes before committing")
    {
    }
}
using FractalDataWorks.Collections;

namespace FractalDataWorks.MCP.Abstractions;

/// <summary>
/// Base class for MCP tool types.
/// </summary>
public abstract class McpToolType : TypeOptionBase<McpToolType>
{
    /// <summary>
    /// Gets the description of this tool type.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the display name for this tool type.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpToolType"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this tool type.</param>
    /// <param name="name">The name of this tool type.</param>
    /// <param name="displayName">The display name for this tool type.</param>
    /// <param name="description">The description of this tool type.</param>
    /// <param name="category">The category (default is "MCP Tools").</param>
    protected McpToolType(int id, string name, string displayName, string description, string? category = "MCP Tools")
        : base(id, name, category)
    {
        DisplayName = displayName;
        Description = description;
    }
}
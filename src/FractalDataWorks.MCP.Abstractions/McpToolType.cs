using FractalDataWorks.Collections;

namespace FractalDataWorks.MCP.Abstractions;

/// <summary>
/// Abstract base class for MCP tool type definitions.
/// Provides the foundation for MCP tool implementations in the framework.
/// </summary>
public abstract class McpToolType : TypeOptionBase<McpToolType>, ITypeOption<McpToolType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="McpToolType"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this tool type.</param>
    /// <param name="name">The name of the tool type.</param>
    /// <param name="description">The description of what this tool does.</param>
    /// <param name="category">The category this tool belongs to.</param>
    protected McpToolType(int id, string name, string description, string category)
        : base(id, name, $"MCP:Tools:{name}", $"{name} MCP Tool", description, category)
    {
        Description = description;
    }

    /// <summary>
    /// Gets the description of what this tool does.
    /// </summary>
    public new string Description { get; }
}
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.MCP.Abstractions;

/// <summary>
/// Collection of all MCP tool types available in the framework.
/// This collection is source-generated and automatically discovers all types inheriting from McpToolType.
/// </summary>
[TypeCollection(CollectionName = "McpToolTypes")]
public abstract partial class McpToolTypesBase : TypeCollectionBase<McpToolType>
{
    // The source generator will populate this with all discovered MCP tool types
}
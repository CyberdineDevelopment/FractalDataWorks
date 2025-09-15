using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.MCP.EnhancedEnums;

/// <summary>
/// Base class for tool category definitions.
/// </summary>
public abstract class ToolCategoryBase : EnhancedEnumBase<ToolCategoryBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ToolCategoryBase"/> class.
    /// </summary>
    protected ToolCategoryBase(int id, string name, string description) : base(id, name)
    {
        Description = description;
    }

    /// <summary>
    /// Gets the category description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the display priority for UI ordering.
    /// </summary>
    public virtual int DisplayPriority => Id;
}

/// <summary>
/// Tool category for session management operations.
/// </summary>
[EnumOption("SessionManagement")]
public sealed class SessionManagement : ToolCategoryBase
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static SessionManagement Instance { get; } = new();

    private SessionManagement() : base(1, "SessionManagement", "Tools for managing compilation sessions and workspace state")
    {
    }
}

/// <summary>
/// Tool category for code analysis and diagnostics.
/// </summary>
[EnumOption("CodeAnalysis")]
public sealed class CodeAnalysis : ToolCategoryBase
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static CodeAnalysis Instance { get; } = new();

    private CodeAnalysis() : base(2, "CodeAnalysis", "Tools for analyzing code quality, diagnostics, and compiler errors")
    {
    }
}

/// <summary>
/// Tool category for virtual editing operations.
/// </summary>
[EnumOption("VirtualEditing")]
public sealed class VirtualEditing : ToolCategoryBase
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static VirtualEditing Instance { get; } = new();

    private VirtualEditing() : base(3, "VirtualEditing", "Tools for preview editing with rollback capabilities")
    {
    }
}

/// <summary>
/// Tool category for refactoring operations.
/// </summary>
[EnumOption("Refactoring")]
public sealed class Refactoring : ToolCategoryBase
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static Refactoring Instance { get; } = new();

    private Refactoring() : base(4, "Refactoring", "Tools for code refactoring and reorganization")
    {
    }
}

/// <summary>
/// Tool category for type analysis and resolution.
/// </summary>
[EnumOption("TypeAnalysis")]
public sealed class TypeAnalysis : ToolCategoryBase
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static TypeAnalysis Instance { get; } = new();

    private TypeAnalysis() : base(5, "TypeAnalysis", "Tools for type discovery, resolution, and analysis")
    {
    }
}

/// <summary>
/// Tool category for project dependency analysis.
/// </summary>
[EnumOption("ProjectDependencies")]
public sealed class ProjectDependencies : ToolCategoryBase
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static ProjectDependencies Instance { get; } = new();

    private ProjectDependencies() : base(6, "ProjectDependencies", "Tools for analyzing project references and dependencies")
    {
    }
}

/// <summary>
/// Tool category for server management operations.
/// </summary>
[EnumOption("ServerManagement")]
public sealed class ServerManagement : ToolCategoryBase
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static ServerManagement Instance { get; } = new();

    private ServerManagement() : base(7, "ServerManagement", "Tools for managing the MCP server lifecycle")
    {
    }

    public override int DisplayPriority => 100; // Show last in UI
}

/// <summary>
/// Source-generated collection of tool categories.
/// </summary>
[TypeCollection("ToolCategoryBase", "ToolCategories")]
public static partial class ToolCategories
{
    // Generated methods:
    // - All: IReadOnlyList<ToolCategoryBase>
    // - ById(int id): ToolCategoryBase?
    // - ByName(string name): ToolCategoryBase?
    // - TryGetById(int id, out ToolCategoryBase category): bool
    // - TryGetByName(string name, out ToolCategoryBase category): bool
}
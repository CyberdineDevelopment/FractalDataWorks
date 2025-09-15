namespace TemplateNamespace;

/// <summary>
/// Interface for EnhancedEnumName Enhanced Enum.
/// Defines the contract for all EnhancedEnumName implementations.
/// </summary>
public interface IEnhancedEnumName
{
    /// <summary>
    /// The unique identifier for this EnhancedEnumName option.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// The display name of this EnhancedEnumName option.
    /// </summary>
    string Name { get; }

#if (IncludeDescription)
    /// <summary>
    /// A detailed description of this EnhancedEnumName option.
    /// </summary>
    string Description { get; }
#endif

#if (IncludeSort)
    /// <summary>
    /// The sort order for displaying EnhancedEnumName options.
    /// </summary>
    int SortOrder { get; }
#endif

    /// <summary>
    /// Indicates whether this EnhancedEnumName option is currently active/enabled.
    /// </summary>
    bool IsActive { get; }

    // TODO: Add additional properties specific to your EnhancedEnumName
    // For example:
    // string Color { get; }
    // string IconClass { get; }
    // bool RequiresPermission { get; }
    // string[] AllowedRoles { get; }
}
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Collection of command categories.
/// </summary>
/// <remarks>
/// This collection is populated by the source generator with all types
/// that inherit from CommandCategoryBase and implement ICommandCategory.
/// </remarks>
[EnhancedEnumCollection(typeof(CommandCategoryBase), typeof(ICommandCategory), typeof(CommandCategories))]
public partial class CommandCategories
{
    // Source generator will add:
    // - public static IReadOnlyList<ICommandCategory> All { get; }
    // - public static ICommandCategory GetById(int id)
    // - public static ICommandCategory GetByName(string name)
    // - Individual static properties for each category
}
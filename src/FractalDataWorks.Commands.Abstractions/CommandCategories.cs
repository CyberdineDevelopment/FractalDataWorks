using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Collection of command categories.
/// </summary>
/// <remarks>
/// This collection is populated by the source generator with all types
/// that inherit from CommandCategoryBase and implement IGenericCommandCategory.
/// </remarks>
[TypeCollection(typeof(CommandCategoryBase), typeof(IGenericCommandCategory), typeof(CommandCategories))]
public abstract partial class CommandCategories : TypeCollectionBase<CommandCategoryBase, IGenericCommandCategory>
{
}
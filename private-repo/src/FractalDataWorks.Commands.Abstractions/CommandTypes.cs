using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Collection of command types.
/// </summary>
/// <remarks>
/// This collection is populated by the source generator with all types
/// that inherit from CommandTypeBase and implement IGenericCommandType.
/// Provides high-performance lookups for command routing and discovery.
/// </remarks>
[TypeCollection(typeof(CommandTypeBase), typeof(IGenericCommandType), typeof(CommandTypes))]
public abstract partial class CommandTypes : TypeCollectionBase<CommandTypeBase, IGenericCommandType>
{
}
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;

namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Collection of command types.
/// </summary>
/// <remarks>
/// This collection is populated by the source generator with all types
/// that inherit from CommandTypeBase and implement ICommandType.
/// Provides high-performance lookups for command routing and discovery.
/// </remarks>
[ServiceTypeCollection(typeof(CommandTypeBase), typeof(ICommandType), typeof(CommandTypes))]
public static partial class CommandTypes
{
    // Source generator will add:
    // - public static IReadOnlyList<ICommandType> All { get; }
    // - public static ICommandType GetById(int id)
    // - public static ICommandType GetByName(string name)
    // - Individual static properties for each command type
}
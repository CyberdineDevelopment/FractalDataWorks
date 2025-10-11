using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Commands.Abstractions.Commands;

/// <summary>
/// Collection of conflict strategy types.
/// </summary>
[TypeCollection(typeof(ConflictStrategyBase), typeof(IConflictStrategy), typeof(ConflictStrategies))]
public abstract partial class ConflictStrategies : TypeCollectionBase<ConflictStrategyBase, IConflictStrategy>
{
}

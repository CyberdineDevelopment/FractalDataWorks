using FractalDataWorks.Collections;

namespace FractalDataWorks.Commands.Abstractions.Commands;

/// <summary>
/// Defines strategies for handling conflicts during mutations.
/// </summary>
public interface IConflictStrategy : ITypeOption<ConflictStrategyBase>
{
    /// <summary>
    /// Gets the conflict resolution behavior.
    /// </summary>
    ConflictResolution Resolution { get; }

    /// <summary>
    /// Gets whether to log conflicts.
    /// </summary>
    bool LogConflicts { get; }
}
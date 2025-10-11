using FractalDataWorks.Collections;

namespace FractalDataWorks.Commands.Abstractions.Commands;

/// <summary>
/// Base class for conflict strategy definitions.
/// </summary>
public abstract class ConflictStrategyBase : TypeOptionBase<ConflictStrategyBase>, IConflictStrategy
{
    /// <inheritdoc/>
    public ConflictResolution Resolution { get; }

    /// <inheritdoc/>
    public bool LogConflicts { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictStrategyBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this conflict strategy.</param>
    /// <param name="name">The name of this conflict strategy.</param>
    /// <param name="resolution">The conflict resolution behavior.</param>
    /// <param name="logConflicts">Whether to log conflicts.</param>
    protected ConflictStrategyBase(
        int id,
        string name,
        ConflictResolution resolution,
        bool logConflicts = true)
        : base(id, name)
    {
        Resolution = resolution;
        LogConflicts = logConflicts;
    }
}

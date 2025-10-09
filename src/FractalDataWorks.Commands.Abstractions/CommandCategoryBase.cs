using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Base class for command categories.
/// </summary>
/// <remarks>
/// Inherit from this class to define specific command categories with
/// their execution characteristics and requirements.
/// </remarks>
public abstract class CommandCategoryBase : EnhancedEnumBase<CommandCategoryBase, ICommandCategory>, ICommandCategory
{
    /// <inheritdoc/>
    public abstract bool RequiresTransaction { get; }

    /// <inheritdoc/>
    public abstract bool SupportsStreaming { get; }

    /// <inheritdoc/>
    public abstract bool IsCacheable { get; }

    /// <inheritdoc/>
    public abstract bool IsMutation { get; }

    /// <inheritdoc/>
    public virtual int ExecutionPriority => 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandCategoryBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this category.</param>
    /// <param name="name">The name of this category.</param>
    protected CommandCategoryBase(int id, string name) : base(id, name)
    {
    }
}
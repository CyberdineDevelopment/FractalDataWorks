using System.Collections.Generic;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Base class for command type definitions.
/// </summary>
/// <remarks>
/// Command types provide metadata about command implementations and their capabilities.
/// Inherit from this class to define specific command types that will be discovered
/// by the TypeCollection source generator.
/// </remarks>
public abstract class CommandTypeBase : TypeOptionBase<CommandTypeBase, ICommandType>, ICommandType
{
    /// <inheritdoc/>
    public abstract ICommandCategory Category { get; }

    /// <inheritdoc/>
    public abstract IReadOnlyCollection<ITranslatorType> SupportedTranslators { get; }

    /// <inheritdoc/>
    public virtual bool SupportsBatching => false;

    /// <inheritdoc/>
    public virtual bool SupportsPipelining => false;

    /// <inheritdoc/>
    public virtual int MaxBatchSize => 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this command type.</param>
    /// <param name="name">The name of this command type.</param>
    /// <param name="description">The description of this command type.</param>
    protected CommandTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }
}
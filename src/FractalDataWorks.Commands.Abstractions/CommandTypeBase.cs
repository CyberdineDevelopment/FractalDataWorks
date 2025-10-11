using System.Collections.Generic;
using FractalDataWorks.Collections;

namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Base class for command type definitions.
/// </summary>
/// <remarks>
/// Command types provide metadata about command implementations and their capabilities.
/// Inherit from this class to define specific command types that will be discovered
/// by the TypeCollection source generator.
/// </remarks>
public abstract class CommandTypeBase : TypeOptionBase<CommandTypeBase>, ICommandType
{
    /// <inheritdoc/>
    public ICommandCategory CommandCategory { get; }

    /// <inheritdoc/>
    public IReadOnlyCollection<ITranslatorType> SupportedTranslators { get; }

    /// <inheritdoc/>
    public bool SupportsBatching { get; }

    /// <inheritdoc/>
    public bool SupportsPipelining { get; }

    /// <inheritdoc/>
    public int MaxBatchSize { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this command type.</param>
    /// <param name="name">The name of this command type.</param>
    /// <param name="description">The description of this command type.</param>
    /// <param name="category">The command category.</param>
    /// <param name="supportedTranslators">The supported translator types.</param>
    /// <param name="supportsBatching">Whether batching is supported (default false).</param>
    /// <param name="supportsPipelining">Whether pipelining is supported (default false).</param>
    /// <param name="maxBatchSize">Maximum batch size (default 1).</param>
    protected CommandTypeBase(
        int id,
        string name,
        string description,
        ICommandCategory category,
        IReadOnlyCollection<ITranslatorType> supportedTranslators,
        bool supportsBatching = false,
        bool supportsPipelining = false,
        int maxBatchSize = 1)
        : base(id, name, description)
    {
        CommandCategory = category;
        SupportedTranslators = supportedTranslators;
        SupportsBatching = supportsBatching;
        SupportsPipelining = supportsPipelining;
        MaxBatchSize = maxBatchSize;
    }
}
using System;
using FractalDataWorks.Results;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Base class for translator type definitions.
/// </summary>
/// <remarks>
/// Translator types provide metadata and factory methods for command translators.
/// Inherit from this class to define specific translator types that will be discovered
/// by the TypeCollection source generator.
/// </remarks>
public abstract class TranslatorTypeBase : TypeOptionBase<TranslatorTypeBase, ITranslatorType>, ITranslatorType
{
    /// <inheritdoc/>
    public abstract IDataFormat SourceFormat { get; }

    /// <inheritdoc/>
    public abstract IDataFormat TargetFormat { get; }

    /// <inheritdoc/>
    public abstract TranslationCapabilities Capabilities { get; }

    /// <inheritdoc/>
    public virtual int Priority => 50;

    /// <inheritdoc/>
    public abstract IGenericResult<ICommandTranslator> CreateTranslator(IServiceProvider services);

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatorTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this translator type.</param>
    /// <param name="name">The name of this translator type.</param>
    /// <param name="description">The description of this translator type.</param>
    protected TranslatorTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }
}
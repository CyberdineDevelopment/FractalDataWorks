using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Base class for translator implementations.
/// </summary>
public abstract class TranslatorBase : ITranslator, ITypeOption<TranslatorBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatorBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this translator type.</param>
    /// <param name="name">The name of this translator.</param>
    protected TranslatorBase(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Gets the unique identifier for this translator type.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this translator.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the category for this translator type (default: "Translator").
    /// </summary>
    public virtual string Category => "Translator";

    /// <summary>
    /// Gets the domain name this translator targets.
    /// </summary>
    public abstract string DomainName { get; }

    /// <summary>
    /// Determines whether this translator supports the given container schema.
    /// </summary>
    public abstract bool SupportsSchema(IContainerSchema schema);

    /// <summary>
    /// Translates a data request into a domain-specific query string.
    /// </summary>
    public abstract System.Threading.Tasks.Task<FractalDataWorks.Results.IGenericResult<string>> Translate(
        IContainer container,
        IDataRequest request,
        System.Threading.CancellationToken cancellationToken = default);
}

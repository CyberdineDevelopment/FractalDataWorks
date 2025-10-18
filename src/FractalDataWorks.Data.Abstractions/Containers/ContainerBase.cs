using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Base class for container implementations.
/// </summary>
public abstract class ContainerBase : IContainer, ITypeOption<ContainerBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this container type.</param>
    /// <param name="name">The name of this container.</param>
    protected ContainerBase(int id, string name)
    {
        Id = id;
        Name = name;
        TranslatorTypeNames = System.Array.Empty<string>();
        FormatTypeNames = System.Array.Empty<string>();
        Translators = System.Array.Empty<ITranslator>();
        Formats = System.Array.Empty<IFormat>();
    }

    /// <summary>
    /// Gets the unique identifier for this container type.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this container.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the category for this container type (default: "Container").
    /// </summary>
    public virtual string Category => "Container";

    /// <summary>
    /// Gets the path to this container.
    /// </summary>
    public abstract IPath Path { get; }

    /// <summary>
    /// Gets the schema definition.
    /// </summary>
    public abstract IContainerSchema Schema { get; }

    /// <summary>
    /// Gets the translator type names.
    /// </summary>
    public string[] TranslatorTypeNames { get; protected set; }

    /// <summary>
    /// Gets the format type names.
    /// </summary>
    public string[] FormatTypeNames { get; protected set; }

    /// <summary>
    /// Gets or sets the resolved translators.
    /// </summary>
    public ITranslator[] Translators { get; set; }

    /// <summary>
    /// Gets or sets the resolved formats.
    /// </summary>
    public IFormat[] Formats { get; set; }
}

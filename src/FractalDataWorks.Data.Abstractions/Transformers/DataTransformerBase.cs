using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Base class for data transformer implementations (non-generic marker base).
/// </summary>
public abstract class DataTransformerBase : IDataTransformer, ITypeOption<DataTransformerBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataTransformerBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this transformer type.</param>
    /// <param name="name">The name of this transformer.</param>
    protected DataTransformerBase(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Gets the unique identifier for this transformer type.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this transformer.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the category for this transformer type (default: "Transformer").
    /// </summary>
    public virtual string Category => "Transformer";

    /// <summary>
    /// Gets the transformer name.
    /// </summary>
    public abstract string TransformerName { get; }
}

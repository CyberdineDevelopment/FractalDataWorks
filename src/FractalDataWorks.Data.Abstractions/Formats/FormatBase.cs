using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Base class for format implementations.
/// </summary>
public abstract class FormatBase : IFormat, ITypeOption<FormatBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormatBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this format type.</param>
    /// <param name="name">The name of this format.</param>
    protected FormatBase(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Gets the unique identifier for this format type.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this format.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the category for this format type (default: "Format").
    /// </summary>
    public virtual string Category => "Format";

    /// <summary>
    /// Gets the format name.
    /// </summary>
    public abstract string FormatName { get; }

    /// <summary>
    /// Gets the MIME type for this format.
    /// </summary>
    public abstract string MimeType { get; }

    /// <summary>
    /// Gets whether this format is binary.
    /// </summary>
    public abstract bool IsBinary { get; }

    /// <summary>
    /// Gets whether this format supports streaming.
    /// </summary>
    public abstract bool SupportsStreaming { get; }

    /// <summary>
    /// Serializes data to a stream.
    /// </summary>
    public abstract Task Serialize(
        Stream output,
        object data,
        IContainerSchema schema,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserializes data from a stream.
    /// </summary>
    public abstract Task<T?> Deserialize<T>(
        Stream input,
        IContainerSchema schema,
        CancellationToken cancellationToken = default);
}

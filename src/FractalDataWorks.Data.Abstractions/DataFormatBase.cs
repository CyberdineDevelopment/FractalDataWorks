using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Base class for data format definitions.
/// </summary>
/// <remarks>
/// Data formats represent different ways data can be structured and transmitted.
/// Inherit from this class to define specific formats like SQL, JSON, XML, CSV, etc.
/// </remarks>
public abstract class DataFormatBase : EnhancedEnumBase<DataFormatBase, IDataFormat>, IDataFormat
{
    /// <inheritdoc/>
    public abstract bool SupportsSchemaDiscovery { get; }

    /// <inheritdoc/>
    public abstract bool SupportsStreaming { get; }

    /// <inheritdoc/>
    public abstract string MimeType { get; }

    /// <inheritdoc/>
    public abstract string[] FileExtensions { get; }

    /// <inheritdoc/>
    public abstract bool IsBinary { get; }

    /// <inheritdoc/>
    public abstract bool SupportsCompression { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataFormatBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this format.</param>
    /// <param name="name">The name of this format.</param>
    protected DataFormatBase(int id, string name) : base(id, name)
    {
    }
}
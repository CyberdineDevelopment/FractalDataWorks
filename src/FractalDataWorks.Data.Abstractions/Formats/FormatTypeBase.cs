using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Base class for format type definitions.
/// Provides metadata about data serialization formats.
/// </summary>
public abstract class FormatTypeBase : TypeOptionBase<FormatTypeBase>, IFormatType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormatTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this format type.</param>
    /// <param name="name">The name of this format type.</param>
    /// <param name="displayName">The display name for this format type.</param>
    /// <param name="description">The description of this format type.</param>
    /// <param name="mimeType">The MIME type for this format.</param>
    /// <param name="isBinary">Whether this format is binary.</param>
    /// <param name="supportsStreaming">Whether this format supports streaming.</param>
    /// <param name="category">The category for this format type (defaults to "Format").</param>
    protected FormatTypeBase(
        int id,
        string name,
        string displayName,
        string description,
        string mimeType,
        bool isBinary,
        bool supportsStreaming,
        string? category = null)
        : base(id, name, $"Formats:{name}", displayName, description, category ?? "Format")
    {
        MimeType = mimeType;
        IsBinary = isBinary;
        SupportsStreaming = supportsStreaming;
    }

    /// <inheritdoc/>
    public string MimeType { get; }

    /// <inheritdoc/>
    public bool IsBinary { get; }

    /// <inheritdoc/>
    public bool SupportsStreaming { get; }
}

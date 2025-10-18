using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Represents a format type definition - metadata about data serialization formats.
/// </summary>
/// <remarks>
/// Format types describe how data is serialized/deserialized (Tabular, Json, Csv, Xml, Parquet, etc.).
/// </remarks>
public interface IFormatType : ITypeOption
{
    /// <summary>
    /// Gets the configuration key for this format type value.
    /// </summary>
    string ConfigurationKey { get; }

    /// <summary>
    /// Gets the display name for this format type value.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the description of this format type value.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the MIME type for this format.
    /// </summary>
    string MimeType { get; }

    /// <summary>
    /// Gets whether this format is binary.
    /// </summary>
    bool IsBinary { get; }

    /// <summary>
    /// Gets whether this format supports streaming.
    /// </summary>
    bool SupportsStreaming { get; }
}

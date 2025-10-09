using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Interface for data format definitions.
/// </summary>
/// <remarks>
/// Data formats represent different ways data can be structured and transmitted,
/// such as SQL, JSON, XML, CSV, etc. Formats are implemented as EnhancedEnums
/// for extensibility and type-safe comparisons.
/// </remarks>
public interface IDataFormat : IEnhancedEnum<IDataFormat>
{
    /// <summary>
    /// Gets whether this format supports schema discovery.
    /// </summary>
    /// <value>True if the format allows automatic schema detection.</value>
    bool SupportsSchemaDiscovery { get; }

    /// <summary>
    /// Gets whether this format supports streaming.
    /// </summary>
    /// <value>True if data can be streamed rather than loaded entirely.</value>
    bool SupportsStreaming { get; }

    /// <summary>
    /// Gets the MIME type for this format.
    /// </summary>
    /// <value>The standard MIME type identifier.</value>
    string MimeType { get; }

    /// <summary>
    /// Gets the file extensions associated with this format.
    /// </summary>
    /// <value>Common file extensions for this format (e.g., ".json", ".xml").</value>
    string[] FileExtensions { get; }

    /// <summary>
    /// Gets whether this format is binary.
    /// </summary>
    /// <value>True if the format uses binary encoding.</value>
    bool IsBinary { get; }

    /// <summary>
    /// Gets whether this format supports compression.
    /// </summary>
    /// <value>True if the format can be compressed.</value>
    bool SupportsCompression { get; }
}
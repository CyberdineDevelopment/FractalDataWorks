using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Data.Abstractions.Formats;

/// <summary>
/// Represents JSON as a data format.
/// </summary>
/// <remarks>
/// This format is used for JavaScript Object Notation data,
/// commonly used in REST APIs and document stores.
/// </remarks>
public sealed class JsonFormat : DataFormatBase, IEnumOption<JsonFormat>
{
    /// <summary>
    /// Gets the singleton instance of the JSON format.
    /// </summary>
    public static JsonFormat Instance { get; } = new();

    /// <inheritdoc/>
    public override bool SupportsSchemaDiscovery => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;

    /// <inheritdoc/>
    public override string MimeType => "application/json";

    /// <inheritdoc/>
    public override string[] FileExtensions => new[] { ".json", ".jsonl" };

    /// <inheritdoc/>
    public override bool IsBinary => false;

    /// <inheritdoc/>
    public override bool SupportsCompression => true;

    private JsonFormat() : base(3, "JSON")
    {
    }
}
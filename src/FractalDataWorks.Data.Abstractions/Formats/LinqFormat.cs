using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Data.Abstractions.Formats;

/// <summary>
/// Represents LINQ expressions as a data format.
/// </summary>
/// <remarks>
/// This format is used as the source format for translating LINQ queries
/// into other formats like SQL, HTTP queries, etc.
/// </remarks>
public sealed class LinqFormat : DataFormatBase, IEnumOption<LinqFormat>
{
    /// <summary>
    /// Gets the singleton instance of the LINQ format.
    /// </summary>
    public static LinqFormat Instance { get; } = new();

    /// <inheritdoc/>
    public override bool SupportsSchemaDiscovery => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;

    /// <inheritdoc/>
    public override string MimeType => "application/x-linq-expression";

    /// <inheritdoc/>
    public override string[] FileExtensions => new[] { ".linq" };

    /// <inheritdoc/>
    public override bool IsBinary => false;

    /// <inheritdoc/>
    public override bool SupportsCompression => false;

    private LinqFormat() : base(1, "LINQ")
    {
    }
}
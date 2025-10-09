using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Data.Abstractions.Formats;

/// <summary>
/// Represents SQL as a data format.
/// </summary>
/// <remarks>
/// This format is used for SQL queries and commands that interact
/// with relational databases.
/// </remarks>
public sealed class SqlFormat : DataFormatBase, IEnumOption<SqlFormat>
{
    /// <summary>
    /// Gets the singleton instance of the SQL format.
    /// </summary>
    public static SqlFormat Instance { get; } = new();

    /// <inheritdoc/>
    public override bool SupportsSchemaDiscovery => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;

    /// <inheritdoc/>
    public override string MimeType => "application/sql";

    /// <inheritdoc/>
    public override string[] FileExtensions => new[] { ".sql" };

    /// <inheritdoc/>
    public override bool IsBinary => false;

    /// <inheritdoc/>
    public override bool SupportsCompression => true;

    private SqlFormat() : base(2, "SQL")
    {
    }
}
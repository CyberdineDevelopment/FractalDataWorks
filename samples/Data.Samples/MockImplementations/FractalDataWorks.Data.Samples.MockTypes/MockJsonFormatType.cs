using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MockTypes;

/// <summary>
/// Demonstrates the FormatType TypeCollection pattern for JSON serialization.
/// </summary>
/// <remarks>
/// <para><strong>What This Demonstrates:</strong></para>
/// <list type="bullet">
///   <item><description>
///     <strong>Format Metadata Properties:</strong> Formats have rich metadata (MIME type, binary flag,
///     streaming support) that guide serialization/deserialization without runtime type checks.
///   </description></item>
///   <item><description>
///     <strong>Serialization Abstraction:</strong> Formats describe how data is represented on the wire
///     (JSON, XML, CSV, Parquet, Protocol Buffers) independent of the data source.
///   </description></item>
///   <item><description>
///     <strong>Streaming Capability:</strong> The SupportsStreaming flag enables performance optimization
///     for large datasets (stream JSON arrays vs. loading entire document into memory).
///   </description></item>
///   <item><description>
///     <strong>Content Negotiation:</strong> MIME type enables HTTP content negotiation - clients can
///     request specific formats via Accept headers.
///   </description></item>
/// </list>
/// <para><strong>In Production Use:</strong></para>
/// <para>
/// Configuration would reference this as:
/// <code>
/// "DataExport": {
///   "FormatType": "Json",  // Resolved to this type at runtime
///   "Indent": true,
///   "CamelCase": true
/// }
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(FormatTypes), "Json")]
public sealed class MockJsonFormatType : FormatTypeBase
{
    public MockJsonFormatType()
        : base(
            id: 1,
            name: "Json",
            displayName: "JSON Format",
            description: "JavaScript Object Notation serialization format",
            mimeType: "application/json",
            isBinary: false,
            supportsStreaming: true)
    {
    }
}

using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MsSql;

/// <summary>
/// Sample JSON format type demonstrating the TypeCollection pattern.
/// </summary>
/// <remarks>
/// In production, this would include actual JSON serialization/deserialization logic
/// with support for streaming, schema validation, and custom converters.
/// </remarks>
[TypeOption(typeof(FormatTypes), "Json")]
public sealed class JsonFormatType : FormatTypeBase
{
    public JsonFormatType()
        : base(
            id: 1,
            name: "Json",
            displayName: "JSON Format",
            description: "JavaScript Object Notation serialization format with streaming support",
            mimeType: "application/json",
            isBinary: false,
            supportsStreaming: true,
            category: "Text")
    {
    }
}

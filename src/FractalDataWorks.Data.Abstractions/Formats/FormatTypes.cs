using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// TypeCollection for all format type implementations.
/// Formats handle serialization/deserialization of data (Tabular, Json, Csv, Xml, Parquet, etc.).
/// </summary>
[TypeCollection(typeof(FormatTypeBase), typeof(IFormatType), typeof(FormatTypes), RestrictToCurrentCompilation = false)]
public sealed partial class FormatTypes : TypeCollectionBase<FormatTypeBase, IFormatType>
{
    // TypeCollectionGenerator will generate all members
}

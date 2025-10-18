using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// TypeCollection for all data type converter type implementations.
/// Converters handle type mapping between data stores and CLR types (SQL → CLR, JSON → CLR, etc.).
/// Uses CrossAssemblyDiscovery to find converters in domain-specific packages.
/// </summary>
[TypeCollection(typeof(DataTypeConverterBase), typeof(IDataTypeConverter), typeof(DataTypeConverterTypes), RestrictToCurrentCompilation = false)]
public sealed partial class DataTypeConverterTypes : TypeCollectionBase<DataTypeConverterBase, IDataTypeConverter>
{
    // TypeCollectionGenerator will generate all members
    // CrossAssemblyDiscovery enabled to find converters in:
    // - FractalDataWorks.Data.Sql (SqlInt32Converter, SqlStringConverter, etc.)
    // - FractalDataWorks.Data.Rest (JsonInt32Converter, JsonStringConverter, etc.)
    // - Any other domain-specific packages
}

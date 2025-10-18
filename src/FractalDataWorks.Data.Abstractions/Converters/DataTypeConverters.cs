using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// TypeCollection for all data type converter implementations.
/// Contains actual converter instances with Convert/ConvertBack methods.
/// </summary>
/// <remarks>
/// This collection provides access to concrete converter implementations that can
/// perform bidirectional type conversion between data stores and CLR types.
/// Uses cross-assembly discovery to find converters in domain-specific packages.
/// </remarks>
[TypeCollection(typeof(DataTypeConverterBase), typeof(IDataTypeConverter), typeof(DataTypeConverters), RestrictToCurrentCompilation = false)]
public sealed partial class DataTypeConverters : TypeCollectionBase<DataTypeConverterBase, IDataTypeConverter>
{
    // TypeCollectionGenerator will generate all members
    // CrossAssemblyDiscovery enabled to find converters in:
    // - FractalDataWorks.Data.Sql (SqlInt32Converter, SqlStringConverter, etc.)
    // - FractalDataWorks.Data.Rest (JsonInt32Converter, JsonStringConverter, etc.)
    // - Any other domain-specific packages
}

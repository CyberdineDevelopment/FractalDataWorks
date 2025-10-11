using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Collection of data formats.
/// </summary>
/// <remarks>
/// This collection is populated by the source generator with all types
/// that inherit from DataFormatBase and implement IDataFormat.
/// Provides standard data formats used across the framework.
/// </remarks>
[TypeCollection(typeof(DataFormatBase), typeof(IDataFormat), typeof(DataFormats))]
public abstract partial class DataFormats : TypeCollectionBase<DataFormatBase, IDataFormat>
{
}
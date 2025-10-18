using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// TypeCollection for all data transformer type implementations.
/// Transformers apply ETL-style transformations to data (aggregation, filtering, calculations, etc.).
/// </summary>
[TypeCollection(typeof(DataTransformerTypeBase), typeof(IDataTransformerType), typeof(DataTransformerTypes))]
public sealed partial class DataTransformerTypes : TypeCollectionBase<DataTransformerTypeBase, IDataTransformerType>
{
    // TypeCollectionGenerator will generate all members
}

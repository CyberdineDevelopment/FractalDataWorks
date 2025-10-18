using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using System;using System.Diagnostics.CodeAnalysis;

namespace FractalDataWorks.Data.Transformers.Abstractions;

/// <summary>
/// TypeCollection for all data transformers.
/// Source generator discovers all types marked with [TypeOption(typeof(DataTransformers), ...)]
/// and creates static properties for each transformer.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// var transformer = DataTransformers.GetByName("PayPalToTransaction");
/// var allTransformers = DataTransformers.All();
/// </code>
/// Generated code tested through source generator tests.
/// </remarks>
[ExcludeFromCodeCoverage]
[TypeCollection(typeof(IDataTransformer), typeof(IDataTransformer), typeof(DataTransformers))]
public abstract partial class DataTransformers : TypeCollectionBase<IDataTransformer, IDataTransformer>
{
    // Source generator creates:
    // - Static properties for each [TypeOption] transformer
    // - All() method returning FrozenSet<IDataTransformer>
    // - GetByName(string name) with O(1) lookup
    // - GetById(int id) with O(1) lookup
}

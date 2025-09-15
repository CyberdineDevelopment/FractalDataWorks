using FractalDataWorks.Collections;

namespace FractalDataWorks.DataSets.Abstractions;

/// <summary>
/// Base class for dataset collections following the FractalDataWorks.Collections pattern.
/// The TypeCollectionGenerator will populate all methods and properties in the generated partial class.
/// This provides type-safe access to all dataset definitions with lookup capabilities by Id, Name, and Category.
/// </summary>
/// <typeparam name="TDataSet">The dataset type that derives from DataSetBase.</typeparam>
public abstract class DataSetCollectionBase<TDataSet> : TypeCollectionBase<TDataSet>
    where TDataSet : class, IDataSet
{
    // TypeCollectionGenerator will generate all functionality in the partial class including:
    // - Static instances of all TDataSet implementations
    // - Lookup methods: GetById, GetByName, GetByCategory
    // - All() method returning all dataset instances
    // - Count property
    // - IEnumerable implementation for iteration
}
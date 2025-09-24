namespace FractalDataWorks.DataSets.Abstractions;

/// <summary>
/// Base class for dataset collections following the FractalDataWorks.Collections pattern.
/// Use [TypeCollection] attribute on concrete implementations to generate high-performance collections.
/// </summary>
/// <typeparam name="TDataSet">The dataset type that derives from DataSetBase.</typeparam>
public abstract class DataSetCollectionBase<TDataSet>
    where TDataSet : class, IDataSet
{
    // Use [TypeCollection("YourDataSetType", "YourCollectionName")] attribute on concrete classes
    // TypeCollectionGenerator will generate all functionality including:
    // - Static instances of all TDataSet implementations
    // - Lookup methods: GetById, GetByName, GetByCategory
    // - All() method returning all dataset instances
    // - Count property
    // - IEnumerable implementation for iteration
}
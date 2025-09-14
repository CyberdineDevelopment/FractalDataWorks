using FractalDataWorks.Collections;

namespace DataStore.Abstractions;

/// <summary>
/// Base class for DataStore type implementations using Type Collections
/// </summary>
public abstract class DataStoreTypeBase : TypeOptionBase<DataStoreTypeBase>
{
    /// <summary>
    /// Initializes a new instance of the DataStoreTypeBase class.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The name of the data store type.</param>
    /// <param name="category">The category (Database, File, Web, etc.).</param>
    protected DataStoreTypeBase(int id, string name, string? category = null) 
        : base(id, name, category)
    {
    }
}
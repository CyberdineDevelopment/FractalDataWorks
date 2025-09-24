using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace DataStore.Abstractions;

/// <summary>
/// Base class for DataStore type implementations using Type Collections with dynamic TypeLookup methods
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

    /// <summary>
    /// Gets the unique identifier for this data store type.
    /// Generates Id(int id) lookup method.
    /// </summary>
    [TypeLookup]
    public override int Id { get; }

    /// <summary>
    /// Gets the name of this data store type.
    /// Generates Name(string name) lookup method.
    /// </summary>
    [TypeLookup]
    public override string Name { get; }

    /// <summary>
    /// Gets the category of this data store type (Database, File, Web, etc.).
    /// Generates Category(string category) lookup method.
    /// </summary>
    [TypeLookup]
    public override string? Category { get; }

    /// <summary>
    /// Gets the connection string template for this data store type.
    /// </summary>
    public abstract string ConnectionStringTemplate { get; }

    /// <summary>
    /// Gets whether this data store type supports transactions.
    /// </summary>
    public abstract bool SupportsTransactions { get; }

    /// <summary>
    /// Gets the maximum connection pool size for this data store type.
    /// </summary>
    public virtual int MaxConnectionPoolSize => 100;
}
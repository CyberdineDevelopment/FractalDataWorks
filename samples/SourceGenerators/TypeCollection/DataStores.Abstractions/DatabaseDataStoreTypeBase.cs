using FractalDataWorks.Collections.Attributes;

namespace DataStore.Abstractions;

/// <summary>
/// Abstract base class for database data store types.
/// Demonstrates abstract type inclusion in collections.
/// </summary>
[TypeOption(typeof(DataStoreTypes), "DatabaseBase")]
public abstract class DatabaseDataStoreTypeBase : DataStoreTypeBase
{
    protected DatabaseDataStoreTypeBase(int id, string name)
        : base(id, name, "Database")
    {
    }

    public override bool SupportsTransactions => true;

    /// <summary>
    /// Gets the default port for this database type.
    /// </summary>
    public abstract int DefaultPort { get; }

    /// <summary>
    /// Gets the SQL dialect used by this database.
    /// </summary>
    public abstract string SqlDialect { get; }
}
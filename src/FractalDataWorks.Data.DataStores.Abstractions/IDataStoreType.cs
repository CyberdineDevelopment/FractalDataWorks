using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.DataStores.Abstractions;

/// <summary>
/// Represents a data store type definition - metadata about WHERE data is physically stored.
/// </summary>
/// <remarks>
/// Data store types describe storage backend characteristics (SQL Server, PostgreSQL, S3, FileSystem, etc.)
/// This is NOT a service interface - it's metadata for type discovery via TypeCollection.
/// </remarks>
public interface IDataStoreType : ITypeOption
{
    // DisplayName, Description, and Category are inherited from ITypeOption

    /// <summary>
    /// Gets whether this store supports read operations.
    /// </summary>
    bool SupportsRead { get; }

    /// <summary>
    /// Gets whether this store supports write operations.
    /// </summary>
    bool SupportsWrite { get; }

    /// <summary>
    /// Gets whether this store supports transactions.
    /// </summary>
    bool SupportsTransactions { get; }
}
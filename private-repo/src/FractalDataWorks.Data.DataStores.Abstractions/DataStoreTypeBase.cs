using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.DataStores.Abstractions;

/// <summary>
/// Base class for data store type definitions.
/// Provides metadata about data storage locations and capabilities.
/// </summary>
/// <remarks>
/// DataStoreType is NOT a service - it's metadata describing WHERE data is physically stored.
/// Use TypeCollection pattern for discovery and lookup.
/// </remarks>
public abstract class DataStoreTypeBase : TypeOptionBase<DataStoreTypeBase>, IDataStoreType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataStoreTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this data store type.</param>
    /// <param name="name">The name of this data store type.</param>
    /// <param name="displayName">The display name for this data store type.</param>
    /// <param name="description">The description of this data store type.</param>
    /// <param name="supportsRead">Whether this store supports read operations.</param>
    /// <param name="supportsWrite">Whether this store supports write operations.</param>
    /// <param name="supportsTransactions">Whether this store supports transactions.</param>
    /// <param name="category">The category for this data store type (defaults to "Data Store").</param>
    protected DataStoreTypeBase(
        int id,
        string name,
        string displayName,
        string description,
        bool supportsRead,
        bool supportsWrite,
        bool supportsTransactions,
        string? category = null)
        : base(id, name, $"DataStores:{name}", displayName, description, category ?? "Data Store")
    {
        SupportsRead = supportsRead;
        SupportsWrite = supportsWrite;
        SupportsTransactions = supportsTransactions;
    }

    // DisplayName, Description, and Category are inherited from TypeOptionBase

    /// <inheritdoc/>
    public bool SupportsRead { get; }

    /// <inheritdoc/>
    public bool SupportsWrite { get; }

    /// <inheritdoc/>
    public bool SupportsTransactions { get; }
}
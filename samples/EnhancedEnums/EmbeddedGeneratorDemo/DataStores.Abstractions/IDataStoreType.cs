namespace DataStore.Abstractions;

/// <summary>
/// Represents a data store type with connection and capability information
/// This demonstrates the pattern for Enhanced Enum abstractions with embedded source generators
/// </summary>
public interface IDataStoreType
{
    /// <summary>
    /// Gets the unique identifier for this data store type
    /// </summary>
    int Id { get; }
    
    /// <summary>
    /// Gets the name of this data store type
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the category of the data store (Database, File, Web, etc.)
    /// </summary>
    string Category { get; }
    
    /// <summary>
    /// Gets the connection string template for this data store type
    /// </summary>
    string ConnectionStringTemplate { get; }
    
    /// <summary>
    /// Gets the default port for this data store type (if applicable)
    /// </summary>
    int? DefaultPort { get; }
    
    /// <summary>
    /// Gets a value indicating whether this data store supports transactions
    /// </summary>
    bool SupportsTransactions { get; }
}
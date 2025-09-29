using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataStores.Abstractions;

/// <summary>
/// Represents a location where data is stored and can be accessed.
/// DataStores abstract the physical location (URL, file path, connection string) 
/// from the logical data access patterns.
/// </summary>
/// <remarks>
/// IDataStore is a core abstraction that represents WHERE data lives, separate from
/// HOW it's accessed (connections) or WHAT format it's in (containers). Examples:
/// - SQL Server database (connection string + database name)
/// - REST API endpoint (base URL + authentication)
/// - File system location (directory path + access permissions)
/// - Cloud storage bucket (bucket name + credentials)
/// </remarks>
public interface IDataStore
{
    /// <summary>
    /// Gets the unique identifier for this data store.
    /// </summary>
    /// <value>A unique identifier used for registration and lookup.</value>
    string Id { get; }

    /// <summary>
    /// Gets the display name for this data store.
    /// </summary>
    /// <value>A human-readable name for UI and logging purposes.</value>
    string Name { get; }

    /// <summary>
    /// Gets the type of data store (e.g., "SqlServer", "RestApi", "FileSystem").
    /// </summary>
    /// <value>The store type identifier for connection selection.</value>
    string StoreType { get; }

    /// <summary>
    /// Gets the base location string for this data store.
    /// </summary>
    /// <value>
    /// The location information in a format appropriate for the store type:
    /// - SQL: Connection string
    /// - HTTP: Base URL  
    /// - File: Root directory path
    /// - Cloud: Bucket/container name
    /// </value>
    string Location { get; }

    /// <summary>
    /// Gets metadata about this data store.
    /// </summary>
    /// <value>Additional properties and configuration information.</value>
    IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets the data paths available in this store.
    /// </summary>
    /// <value>
    /// The paths that can be navigated within this store to locate specific data.
    /// </value>
    IEnumerable<IDataPath> AvailablePaths { get; }

    /// <summary>
    /// Tests connectivity to this data store.
    /// </summary>
    /// <returns>A result indicating whether the store is accessible.</returns>
    /// <remarks>
    /// This method performs a basic connectivity test without accessing specific data.
    /// It validates that the store location is reachable and credentials are valid.
    /// </remarks>
    Task<IGenericResult> TestConnectionAsync();

    /// <summary>
    /// Discovers available data paths within this store.
    /// </summary>
    /// <returns>A result containing the discovered paths, or failure information.</returns>
    /// <remarks>
    /// This method performs discovery to find what data paths are available.
    /// For example, it might list tables in a database, endpoints in an API,
    /// or directories in a file system.
    /// </remarks>
    Task<IGenericResult<IEnumerable<IDataPath>>> DiscoverPathsAsync();

    /// <summary>
    /// Gets a specific data path by name.
    /// </summary>
    /// <param name="pathName">The name of the path to retrieve.</param>
    /// <returns>The data path if found, or null if not found.</returns>
    IDataPath? GetPath(string pathName);

    /// <summary>
    /// Validates that this store can be used with a specific connection type.
    /// </summary>
    /// <param name="connectionType">The connection type to validate against.</param>
    /// <returns>A result indicating whether the combination is valid.</returns>
    /// <remarks>
    /// This method checks compatibility between the store type and connection type.
    /// For example, an SQL store should be compatible with SQL connections but
    /// not with HTTP connections.
    /// </remarks>
    IGenericResult ValidateConnectionCompatibility(string connectionType);
}

/// <summary>
/// Generic interface for strongly-typed data stores with specific configuration.
/// </summary>
/// <typeparam name="TConfiguration">The configuration type for this store.</typeparam>
/// <remarks>
/// This generic version provides compile-time type safety for stores that have
/// specific configuration requirements, while maintaining compatibility with
/// the base IDataStore interface for runtime scenarios.
/// </remarks>
public interface IDataStore<TConfiguration> : IDataStore
    where TConfiguration : class
{
    /// <summary>
    /// Gets the strongly-typed configuration for this data store.
    /// </summary>
    /// <value>The configuration instance containing store-specific settings.</value>
    TConfiguration Configuration { get; }

    /// <summary>
    /// Updates the configuration for this data store.
    /// </summary>
    /// <param name="configuration">The new configuration to apply.</param>
    /// <returns>A result indicating whether the update was successful.</returns>
    /// <remarks>
    /// This method allows runtime reconfiguration of the data store.
    /// The store should validate the new configuration and may need to
    /// re-establish connections or clear caches after the update.
    /// </remarks>
    Task<IGenericResult> UpdateConfigurationAsync(TConfiguration configuration);
}
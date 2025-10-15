using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.Data.DataStores.Abstractions.Messages;
using FractalDataWorks.Results;

namespace FractalDataWorks.Data.DataStores.Abstractions;

/// <summary>
/// Abstract base class providing common functionality for data store implementations.
/// Handles standard operations and provides extension points for store-specific logic.
/// </summary>
/// <typeparam name="TConfiguration">The configuration type for this store.</typeparam>
/// <remarks>
/// DataStoreBase reduces boilerplate code in concrete store implementations by
/// providing common functionality like path management, validation, and metadata handling.
/// Derived classes focus on the store-specific connection and discovery logic.
/// </remarks>
public abstract class DataStoreBase<TConfiguration> : IDataStore<TConfiguration>
    where TConfiguration : class
{
    private readonly Dictionary<string, IDataPath> _paths;
    private readonly Dictionary<string, object> _metadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataStoreBase{TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this store.</param>
    /// <param name="name">The display name for this store.</param>
    /// <param name="storeType">The store type identifier.</param>
    /// <param name="location">The base location string.</param>
    /// <param name="configuration">The store configuration.</param>
    /// <param name="metadata">Optional metadata for this store.</param>
    protected DataStoreBase(
        string id,
        string name,
        string storeType,
        string location,
        TConfiguration configuration,
        IDictionary<string, object>? metadata = null)
    {
        Id = id;
        Name = name;
        StoreType = storeType;
        Location = location;
        Configuration = configuration;

        _paths = new Dictionary<string, IDataPath>(StringComparer.Ordinal);
        _metadata = new Dictionary<string, object>(StringComparer.Ordinal);

        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                _metadata[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <inheritdoc/>
    public string Id { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string StoreType { get; }

    /// <inheritdoc/>
    public string Location { get; }

    /// <inheritdoc/>
    public TConfiguration Configuration { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;

    /// <inheritdoc/>
    public IEnumerable<IDataPath> AvailablePaths => _paths.Values;

    /// <inheritdoc/>
    public virtual async Task<IGenericResult> TestConnection()
    {
        try
        {
            return await PerformConnectionTest().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return GenericResult.Failure(string.Format(CultureInfo.InvariantCulture, DataStoreMessages.ConnectionTestFailed().Message, ex.Message));
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IGenericResult<IEnumerable<IDataPath>>> DiscoverPaths()
    {
        try
        {
            var discoveredPaths = await DiscoverStorePaths().ConfigureAwait(false);

            if (discoveredPaths.IsSuccess)
            {
                // Update the internal paths collection
                _paths.Clear();
                foreach (var path in discoveredPaths.Value!)
                {
                    _paths[path.Name] = path;
                }
            }

            return discoveredPaths;
        }
        catch (Exception ex)
        {
            return GenericResult<IEnumerable<IDataPath>>.Failure(string.Format(CultureInfo.InvariantCulture, DataStoreMessages.PathDiscoveryFailed().Message, ex.Message));
        }
    }

    /// <inheritdoc/>
    public virtual IDataPath? GetPath(string pathName)
    {
        if (string.IsNullOrWhiteSpace(pathName))
            return null;

        _paths.TryGetValue(pathName, out var path);
        return path;
    }

    /// <inheritdoc/>
    public virtual IGenericResult ValidateConnectionCompatibility(string connectionType)
    {
        if (string.IsNullOrWhiteSpace(connectionType))
            return GenericResult.Failure(DataStoreMessages.ConnectionTypeNullOrEmpty().Message);

        var compatibleTypes = GetCompatibleConnectionTypes();

        if (compatibleTypes.Contains(connectionType, StringComparer.OrdinalIgnoreCase))
            return GenericResult.Success();

        return GenericResult.Failure(
            string.Format(CultureInfo.InvariantCulture, DataStoreMessages.StoreTypeIncompatible().Message, StoreType, connectionType, string.Join(", ", compatibleTypes)));
    }

    /// <inheritdoc/>
    public virtual async Task<IGenericResult> UpdateConfiguration(TConfiguration configuration)
    {
        if (configuration == null)
            return GenericResult.Failure(DataStoreMessages.DataStoreConfigurationNull());

        try
        {
            var validationResult = await ValidateConfiguration(configuration).ConfigureAwait(false);
            if (!validationResult.IsSuccess)
                return validationResult;

            var updateResult = await ApplyConfiguration(configuration).ConfigureAwait(false);
            if (updateResult.IsSuccess)
            {
                Configuration = configuration;
            }

            return updateResult;
        }
        catch (Exception ex)
        {
            return GenericResult.Failure(string.Format(CultureInfo.InvariantCulture, DataStoreMessages.ConfigurationUpdateFailed().Message, ex.Message));
        }
    }

    /// <summary>
    /// Performs the connection test logic specific to the store type.
    /// </summary>
    /// <returns>A result indicating whether the connection test succeeded.</returns>
    /// <remarks>
    /// This method must be implemented by derived classes to perform store-specific
    /// connectivity testing. It should validate that the store location is reachable
    /// and any required credentials are valid.
    /// </remarks>
    protected abstract Task<IGenericResult> PerformConnectionTest();

    /// <summary>
    /// Discovers the available data paths within the store.
    /// </summary>
    /// <returns>A result containing the discovered paths.</returns>
    /// <remarks>
    /// This method must be implemented by derived classes to discover available
    /// data paths within the store. For example, listing tables in a database,
    /// endpoints in an API, or files in a directory.
    /// </remarks>
    protected abstract Task<IGenericResult<IEnumerable<IDataPath>>> DiscoverStorePaths();

    /// <summary>
    /// Gets the connection types that are compatible with this store type.
    /// </summary>
    /// <returns>A collection of compatible connection type identifiers.</returns>
    /// <remarks>
    /// This method must be implemented by derived classes to specify which
    /// connection types can be used with this store type. For example, an SQL
    /// store should return SQL-related connection types.
    /// </remarks>
    protected abstract IEnumerable<string> GetCompatibleConnectionTypes();

    /// <summary>
    /// Validates a new configuration before it's applied.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>A result indicating whether the configuration is valid.</returns>
    /// <remarks>
    /// The default implementation returns success. Override this method to add
    /// store-specific configuration validation.
    /// </remarks>
    protected virtual Task<IGenericResult> ValidateConfiguration(TConfiguration configuration)
    {
        return Task.FromResult(GenericResult.Success());
    }

    /// <summary>
    /// Applies store-specific configuration update logic.
    /// </summary>
    /// <param name="configuration">The new configuration to apply.</param>
    /// <returns>A result indicating whether the update succeeded.</returns>
    /// <remarks>
    /// The default implementation returns success. Override this method to add
    /// store-specific update logic such as re-establishing connections or
    /// clearing caches.
    /// </remarks>
    protected virtual Task<IGenericResult> ApplyConfiguration(TConfiguration configuration)
    {
        return Task.FromResult(GenericResult.Success());
    }

    /// <summary>
    /// Adds a data path to this store's available paths.
    /// </summary>
    /// <param name="path">The path to add.</param>
    /// <remarks>
    /// This method is provided for derived classes to programmatically add
    /// paths during initialization or discovery operations.
    /// </remarks>
    protected void AddPath(IDataPath path)
    {
        if (path != null)
        {
            _paths[path.Name] = path;
        }
    }

    /// <summary>
    /// Adds metadata to this store.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <remarks>
    /// This method is provided for derived classes to add metadata
    /// during initialization or runtime operations.
    /// </remarks>
    protected void AddMetadata(string key, object value)
    {
        if (!string.IsNullOrWhiteSpace(key) && value != null)
        {
            _metadata[key] = value;
        }
    }

    /// <summary>
    /// Returns a string representation of this data store for debugging purposes.
    /// </summary>
    /// <returns>A string describing this data store.</returns>
    public override string ToString()
    {
        return $"DataStore[{StoreType}]: {Name} ({Location})";
    }
}
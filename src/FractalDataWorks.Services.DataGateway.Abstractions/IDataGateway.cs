using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.DataSets.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

/// <summary>
/// Provides unified data access across different connection types and data sources.
/// Acts as the orchestration layer between DataSets, ConnectionProvider, and query execution.
/// </summary>
/// <remarks>
/// The DataGateway is the main entry point for executing data queries. It handles:
/// - Query routing to appropriate connection types
/// - Connection selection and management
/// - Query translation coordination
/// - Result aggregation and mapping
/// - Cross-source operations (future: joins, unions)
/// </remarks>
public interface IDataGateway
{
    /// <summary>
    /// Executes a data query using a named connection configuration.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="query">The data query to execute.</param>
    /// <param name="connectionName">The name of the connection configuration to use.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the query execution result.</returns>
    /// <remarks>
    /// The connection name should correspond to a configured connection in the application's
    /// configuration system. The DataGateway will resolve the connection configuration
    /// and select the appropriate connection type for execution.
    /// </remarks>
    Task<IFdwResult<T>> Execute<T>(IDataQuery query, string connectionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a data query using a specific connection configuration.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="query">The data query to execute.</param>
    /// <param name="connectionConfig">The connection configuration to use.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the query execution result.</returns>
    /// <remarks>
    /// This overload allows direct specification of connection configuration,
    /// bypassing the configuration resolution process. Useful for dynamic
    /// connection scenarios or testing.
    /// </remarks>
    Task<IFdwResult<T>> Execute<T>(IDataQuery query, IConnectionConfiguration connectionConfig, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a data query using a configuration ID for lookup.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="query">The data query to execute.</param>
    /// <param name="configurationId">The ID of the configuration to look up.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the query execution result.</returns>
    /// <remarks>
    /// The configuration ID will be used to look up the connection configuration
    /// from the registered configuration sources (database, JSON files, etc.).
    /// </remarks>
    Task<IFdwResult<T>> Execute<T>(IDataQuery query, int configurationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available connection types that can execute queries against a specific dataset.
    /// </summary>
    /// <param name="dataSetName">The name of the dataset to query.</param>
    /// <returns>A task containing the available connection types.</returns>
    /// <remarks>
    /// This method helps applications determine which connection types are available
    /// for a given dataset. Useful for dynamic connection selection or user interface
    /// scenarios where users can choose their preferred data source.
    /// </remarks>
    Task<IFdwResult<string[]>> GetAvailableConnections(string dataSetName);

    /// <summary>
    /// Tests connectivity for a specific connection configuration.
    /// </summary>
    /// <param name="connectionConfig">The connection configuration to test.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the connectivity test result.</returns>
    /// <remarks>
    /// This method verifies that the specified connection configuration can successfully
    /// establish a connection to its target data source. Used for health checks,
    /// configuration validation, and diagnostic purposes.
    /// </remarks>
    Task<IFdwResult> TestConnection(IConnectionConfiguration connectionConfig, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata about available datasets from a specific connection.
    /// </summary>
    /// <param name="connectionConfig">The connection configuration to query.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing dataset metadata.</returns>
    /// <remarks>
    /// This method discovers what datasets are available through a specific connection.
    /// For example, it might return table names from a SQL database, endpoint lists
    /// from a REST API, or file listings from a file system connection.
    /// </remarks>
    Task<IFdwResult<DataSetMetadata[]>> DiscoverDataSets(IConnectionConfiguration connectionConfig, CancellationToken cancellationToken = default);
}

/// <summary>
/// Metadata information about a discovered dataset.
/// </summary>
/// <remarks>
/// Contains information discovered from a data source about available datasets,
/// including schema information, estimated row counts, and access patterns.
/// </remarks>
public sealed record DataSetMetadata
{
    /// <summary>
    /// Gets the name of the dataset.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the description of the dataset, if available.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the estimated number of records in the dataset.
    /// </summary>
    public long? EstimatedRowCount { get; init; }

    /// <summary>
    /// Gets the schema information for the dataset.
    /// </summary>
    public DataFieldMetadata[] Fields { get; init; } = Array.Empty<DataFieldMetadata>();

    /// <summary>
    /// Gets the last modified timestamp, if available.
    /// </summary>
    public DateTimeOffset? LastModified { get; init; }

    /// <summary>
    /// Gets additional properties specific to the data source.
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new(StringComparer.Ordinal);
}

/// <summary>
/// Metadata information about a field in a dataset.
/// </summary>
/// <remarks>
/// Describes a single field/column in a dataset, including its type,
/// nullability, and other characteristics discovered from the data source.
/// </remarks>
public sealed record DataFieldMetadata
{
    /// <summary>
    /// Gets the name of the field.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the data type of the field.
    /// </summary>
    public required Type DataType { get; init; }

    /// <summary>
    /// Gets a value indicating whether the field can contain null values.
    /// </summary>
    public bool IsNullable { get; init; }

    /// <summary>
    /// Gets a value indicating whether this field is part of the primary key.
    /// </summary>
    public bool IsKey { get; init; }

    /// <summary>
    /// Gets the maximum length for string fields, if applicable.
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Gets additional field-specific properties.
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new(StringComparer.Ordinal);
}
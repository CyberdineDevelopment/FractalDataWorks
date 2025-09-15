using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.DataStores.Abstractions;

namespace FractalDataWorks.DataContainers.Abstractions;

/// <summary>
/// Represents a data container that defines the physical format and structure of data.
/// DataContainers abstract the format (CSV, JSON, SQL table) from the logical data access.
/// </summary>
/// <remarks>
/// IDataContainer represents WHAT format data is stored in, separate from WHERE it lives
/// (data store) or HOW it's accessed (connections). Examples:
/// - CSV file with specific column structure
/// - JSON document with defined schema
/// - SQL table with columns and types
/// - Parquet file with column definitions
/// - XML document with element structure
/// </remarks>
public interface IDataContainer
{
    /// <summary>
    /// Gets the unique identifier for this data container.
    /// </summary>
    /// <value>A unique identifier for this container instance.</value>
    string Id { get; }

    /// <summary>
    /// Gets the display name for this data container.
    /// </summary>
    /// <value>A human-readable name for UI and logging purposes.</value>
    string Name { get; }

    /// <summary>
    /// Gets the container type (e.g., "CsvFile", "JsonDocument", "SqlTable").
    /// </summary>
    /// <value>The container type identifier for format handling.</value>
    string ContainerType { get; }

    /// <summary>
    /// Gets the schema definition for this container.
    /// </summary>
    /// <value>
    /// The schema that describes the structure, fields, and types within this container.
    /// </value>
    IDataSchema Schema { get; }

    /// <summary>
    /// Gets metadata about this data container.
    /// </summary>
    /// <value>Additional properties and configuration information.</value>
    IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets the data location where this container can be found.
    /// </summary>
    /// <value>
    /// The complete location specification including store, path, and parameters.
    /// </value>
    DataLocation Location { get; }

    /// <summary>
    /// Gets format-specific configuration for this container.
    /// </summary>
    /// <value>
    /// Configuration settings specific to the container format, such as
    /// CSV delimiters, JSON parsing options, or SQL connection settings.
    /// </value>
    IContainerConfiguration Configuration { get; }

    /// <summary>
    /// Validates that this container can be read from the specified location.
    /// </summary>
    /// <param name="location">The location to validate access for.</param>
    /// <returns>A result indicating whether read access is possible.</returns>
    /// <remarks>
    /// This method performs validation without actually reading data.
    /// It checks for accessibility, permissions, schema compatibility, etc.
    /// </remarks>
    Task<IFdwResult> ValidateReadAccessAsync(DataLocation location);

    /// <summary>
    /// Validates that this container can be written to the specified location.
    /// </summary>
    /// <param name="location">The location to validate write access for.</param>
    /// <returns>A result indicating whether write access is possible.</returns>
    /// <remarks>
    /// This method performs validation without actually writing data.
    /// It checks for write permissions, schema compatibility, format support, etc.
    /// </remarks>
    Task<IFdwResult> ValidateWriteAccessAsync(DataLocation location);

    /// <summary>
    /// Gets estimated metrics for reading from this container.
    /// </summary>
    /// <param name="location">The location to estimate metrics for.</param>
    /// <returns>Estimated read metrics, or failure if unavailable.</returns>
    /// <remarks>
    /// This method provides estimates for query planning and optimization.
    /// Metrics might include record count, data size, read time, etc.
    /// </remarks>
    Task<IFdwResult<ContainerMetrics>> GetReadMetricsAsync(DataLocation location);

    /// <summary>
    /// Creates a reader instance for accessing data from this container.
    /// </summary>
    /// <param name="location">The location to read from.</param>
    /// <returns>A reader instance configured for this container format.</returns>
    /// <remarks>
    /// The reader will be configured with the container's schema and format settings.
    /// Callers are responsible for disposing the reader when finished.
    /// </remarks>
    Task<IFdwResult<IDataReader>> CreateReaderAsync(DataLocation location);

    /// <summary>
    /// Creates a writer instance for writing data to this container.
    /// </summary>
    /// <param name="location">The location to write to.</param>
    /// <param name="writeMode">The write mode (overwrite, append, etc.).</param>
    /// <returns>A writer instance configured for this container format.</returns>
    /// <remarks>
    /// The writer will be configured with the container's schema and format settings.
    /// Callers are responsible for disposing the writer when finished.
    /// </remarks>
    Task<IFdwResult<IDataWriter>> CreateWriterAsync(DataLocation location, ContainerWriteMode writeMode = ContainerWriteMode.Overwrite);

    /// <summary>
    /// Discovers the actual schema of data at the specified location.
    /// </summary>
    /// <param name="location">The location to discover schema for.</param>
    /// <param name="sampleSize">The number of records to sample for schema inference.</param>
    /// <returns>The discovered schema, or failure if schema cannot be determined.</returns>
    /// <remarks>
    /// This method analyzes actual data to determine schema, which may differ
    /// from the container's declared schema. Useful for schema validation and
    /// dynamic schema scenarios.
    /// </remarks>
    Task<IFdwResult<IDataSchema>> DiscoverSchemaAsync(DataLocation location, int sampleSize = 1000);
}

/// <summary>
/// Represents configuration settings specific to a container format.
/// </summary>
/// <remarks>
/// IContainerConfiguration provides format-specific settings that control
/// how data is read from or written to the container. Each container type
/// will have its own configuration implementation.
/// </remarks>
public interface IContainerConfiguration
{
    /// <summary>
    /// Gets the container type this configuration applies to.
    /// </summary>
    /// <value>The container type identifier.</value>
    string ContainerType { get; }

    /// <summary>
    /// Gets configuration settings as key-value pairs.
    /// </summary>
    /// <value>
    /// Configuration settings specific to the container format.
    /// For example, CSV might have "Delimiter", "HasHeaders", "Encoding".
    /// </value>
    IReadOnlyDictionary<string, object> Settings { get; }

    /// <summary>
    /// Validates that the configuration is valid for the container type.
    /// </summary>
    /// <returns>A result indicating whether the configuration is valid.</returns>
    IFdwResult Validate();

    /// <summary>
    /// Gets a configuration value as a specific type.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The default value if the key is not found.</param>
    /// <returns>The configuration value cast to the specified type.</returns>
    T GetValue<T>(string key, T defaultValue = default!);
}

/// <summary>
/// Represents metrics about a data container's characteristics.
/// </summary>
/// <remarks>
/// ContainerMetrics provides information for query optimization, resource
/// planning, and performance tuning. Different container types may provide
/// different levels of metric detail.
/// </remarks>
public sealed class ContainerMetrics
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerMetrics"/> class.
    /// </summary>
    /// <param name="estimatedRecordCount">The estimated number of records.</param>
    /// <param name="estimatedDataSize">The estimated data size in bytes.</param>
    /// <param name="lastModified">When the data was last modified.</param>
    /// <param name="additionalMetrics">Additional container-specific metrics.</param>
    public ContainerMetrics(
        long estimatedRecordCount,
        long estimatedDataSize,
        DateTimeOffset? lastModified = null,
        IDictionary<string, object>? additionalMetrics = null)
    {
        EstimatedRecordCount = estimatedRecordCount;
        EstimatedDataSize = estimatedDataSize;
        LastModified = lastModified;
        AdditionalMetrics = additionalMetrics != null
            ? new Dictionary<string, object>(additionalMetrics, StringComparer.Ordinal)
            : new Dictionary<string, object>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets the estimated number of records in the container.
    /// </summary>
    /// <value>The estimated record count, or -1 if unknown.</value>
    public long EstimatedRecordCount { get; }

    /// <summary>
    /// Gets the estimated data size in bytes.
    /// </summary>
    /// <value>The estimated size, or -1 if unknown.</value>
    public long EstimatedDataSize { get; }

    /// <summary>
    /// Gets when the data was last modified.
    /// </summary>
    /// <value>The last modified timestamp, or null if unknown.</value>
    public DateTimeOffset? LastModified { get; }

    /// <summary>
    /// Gets additional container-specific metrics.
    /// </summary>
    /// <value>
    /// Additional metrics that may be specific to the container type,
    /// such as compression ratio, index information, or format-specific data.
    /// </value>
    public IReadOnlyDictionary<string, object> AdditionalMetrics { get; }
}

/// <summary>
/// Specifies how data should be written to a container.
/// </summary>
public enum ContainerWriteMode
{
    /// <summary>
    /// Overwrite any existing data completely.
    /// </summary>
    Overwrite,

    /// <summary>
    /// Append new data to existing data.
    /// </summary>
    Append,

    /// <summary>
    /// Create new container, fail if it already exists.
    /// </summary>
    CreateNew,

    /// <summary>
    /// Update existing records based on key fields.
    /// </summary>
    Upsert
}
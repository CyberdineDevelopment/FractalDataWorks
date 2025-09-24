using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.DataSets.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

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
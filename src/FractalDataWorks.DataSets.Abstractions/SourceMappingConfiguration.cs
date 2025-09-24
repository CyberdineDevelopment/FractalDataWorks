using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Configuration;
using FluentValidation;

namespace FractalDataWorks.DataSets.Abstractions;

/// <summary>
/// Configuration for source mappings that define how to access data from different connection types.
/// </summary>
public sealed class SourceMappingConfiguration
{
    /// <summary>
    /// Gets or sets the connection type name (e.g., "SQL", "HTTP", "File").
    /// </summary>
    /// <value>The type identifier for the connection that can provide this data.</value>
    public string ConnectionType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the priority of this source when multiple sources are available.
    /// </summary>
    /// <value>Lower values indicate higher priority (1 = highest priority).</value>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Gets or sets the estimated cost/performance rating for this source.
    /// </summary>
    /// <value>Estimated cost where lower values indicate better performance/lower cost.</value>
    public int EstimatedCost { get; set; } = 50;

    /// <summary>
    /// Gets or sets SQL-specific mapping configuration.
    /// </summary>
    /// <value>Configuration for SQL table/view access.</value>
    public SqlMappingConfiguration? Sql { get; set; }

    /// <summary>
    /// Gets or sets HTTP-specific mapping configuration.
    /// </summary>
    /// <value>Configuration for HTTP API access.</value>
    public HttpMappingConfiguration? Http { get; set; }

    /// <summary>
    /// Gets or sets file-specific mapping configuration.
    /// </summary>
    /// <value>Configuration for file-based data access.</value>
    public FileMappingConfiguration? File { get; set; }
}
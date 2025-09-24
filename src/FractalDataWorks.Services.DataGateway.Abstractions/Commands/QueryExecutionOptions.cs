using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentValidation.Results;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.DataSets.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Commands;

/// <summary>
/// Execution options that control how a data query is executed.
/// </summary>
/// <remarks>
/// These options provide fine-grained control over query execution behavior,
/// allowing applications to optimize performance, reliability, and resource usage
/// based on their specific requirements.
/// </remarks>
public sealed record QueryExecutionOptions
{
    /// <summary>
    /// Gets the default execution options.
    /// </summary>
    public static readonly QueryExecutionOptions Default = new();

    /// <summary>
    /// Gets or sets the timeout for query execution.
    /// </summary>
    /// <remarks>
    /// If not specified, the connection's default timeout will be used.
    /// </remarks>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the query results should be cached.
    /// </summary>
    /// <remarks>
    /// When enabled, query results may be cached based on the query signature
    /// and data freshness requirements. Default is false.
    /// </remarks>
    public bool EnableCaching { get; init; } = false;

    /// <summary>
    /// Gets or sets the cache duration for query results.
    /// </summary>
    /// <remarks>
    /// Only relevant when EnableCaching is true. If not specified,
    /// a default cache duration will be used based on the data source characteristics.
    /// </remarks>
    public TimeSpan? CacheDuration { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of rows to return.
    /// </summary>
    /// <remarks>
    /// This provides a safety limit to prevent accidentally returning huge result sets.
    /// If not specified, no artificial limit is applied beyond what's in the query expression.
    /// </remarks>
    public int? MaxRows { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to track query execution metrics.
    /// </summary>
    /// <remarks>
    /// When enabled, detailed execution metrics will be collected for monitoring
    /// and performance analysis. Default is true.
    /// </remarks>
    public bool TrackMetrics { get; init; } = true;
}
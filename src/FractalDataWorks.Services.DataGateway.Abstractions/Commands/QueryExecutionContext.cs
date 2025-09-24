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
/// Execution context information for a data query.
/// </summary>
/// <remarks>
/// Context provides additional metadata about the execution environment
/// that may be needed for logging, auditing, security, or connection-specific behavior.
/// </remarks>
public sealed record QueryExecutionContext
{
    /// <summary>
    /// Gets an empty execution context.
    /// </summary>
    public static readonly QueryExecutionContext Empty = new();

    /// <summary>
    /// Gets or sets the correlation ID for this query execution.
    /// </summary>
    /// <remarks>
    /// Used for tracking queries across distributed systems and logging correlation.
    /// </remarks>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets or sets the user identity associated with this query.
    /// </summary>
    /// <remarks>
    /// May be used for authorization, auditing, or connection-specific user context.
    /// </remarks>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets or sets the tenant ID for multi-tenant scenarios.
    /// </summary>
    /// <remarks>
    /// Used in multi-tenant applications to ensure data isolation and proper routing.
    /// </remarks>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets or sets additional custom properties for this execution context.
    /// </summary>
    /// <remarks>
    /// Allows applications to pass custom metadata that may be needed by
    /// specific connection types or query translators.
    /// </remarks>
    public Dictionary<string, object> Properties { get; init; } = new(StringComparer.Ordinal);
}
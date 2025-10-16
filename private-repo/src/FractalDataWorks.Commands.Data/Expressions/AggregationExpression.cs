using System.Collections.Generic;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Implementation of IAggregationExpression for GROUP BY clause representation.
/// </summary>
/// <remarks>
/// Full aggregation support will be implemented in future phases.
/// This provides basic structure for group by and aggregation functions.
/// </remarks>
public sealed class AggregationExpression : IAggregationExpression
{
    /// <summary>
    /// Gets or sets the fields to group by.
    /// </summary>
    public required IReadOnlyList<string> GroupByFields { get; init; }

    /// <summary>
    /// Gets or sets the aggregation functions.
    /// Dictionary maps result field name to aggregation specification.
    /// </summary>
    public required IReadOnlyDictionary<string, string> Aggregations { get; init; }
}

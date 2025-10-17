using System.Collections.Generic;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Implementation of IFilterExpression for WHERE clause representation.
/// </summary>
public sealed class FilterExpression : IFilterExpression
{
    /// <summary>
    /// Gets or sets the filter conditions.
    /// </summary>
    public required IReadOnlyList<FilterCondition> Conditions { get; init; }

    /// <summary>
    /// Gets or sets the logical operator for combining conditions.
    /// Defaults to AND if null.
    /// </summary>
    public LogicalOperator? LogicalOperator { get; init; }
}

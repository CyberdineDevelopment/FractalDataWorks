using System.Collections.Generic;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Interface for filter expressions (WHERE clause representation).
/// </summary>
/// <remarks>
/// Represents the universal WHERE clause that works across all data sources.
/// Translators convert this to SQL WHERE, OData $filter, file filtering, etc.
/// </remarks>
public interface IFilterExpression
{
    /// <summary>
    /// Gets the filter conditions.
    /// </summary>
    /// <value>A collection of filter conditions to apply.</value>
    IReadOnlyList<FilterCondition> Conditions { get; }

    /// <summary>
    /// Gets the logical operator for combining multiple conditions (AND / OR).
    /// </summary>
    /// <value>The logical operator. Defaults to AND if null.</value>
    LogicalOperator? LogicalOperator { get; }
}

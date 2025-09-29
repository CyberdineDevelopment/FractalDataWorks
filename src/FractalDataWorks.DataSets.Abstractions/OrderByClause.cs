using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FractalDataWorks.DataSets.Abstractions.Operators;

namespace FractalDataWorks.DataSets.Abstractions;

/// <summary>
/// Represents an ORDER BY clause extracted from a query.
/// </summary>
public sealed class OrderByClause
{
    /// <summary>
    /// Gets the field name to order by.
    /// </summary>
    public string FieldName { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the sort direction.
    /// </summary>
    public required SortDirectionBase Direction { get; init; }
    
    /// <summary>
    /// Gets the original expression for this ordering.
    /// </summary>
    public Expression OriginalExpression { get; init; } = Expression.Empty();
}
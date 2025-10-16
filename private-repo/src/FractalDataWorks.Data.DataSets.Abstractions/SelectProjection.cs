using FractalDataWorks.Data.DataSets.Abstractions.Operators;
using System;
using System.Collections.Generic;
using System.Linq;using System.Linq.Expressions;

namespace FractalDataWorks.Data.DataSets.Abstractions;

/// <summary>
/// Represents a SELECT projection extracted from a query.
/// </summary>
public sealed class SelectProjection
{
    /// <summary>
    /// Gets the fields being selected.
    /// </summary>
    public IReadOnlyList<ProjectedField> Fields { get; init; } = [];
    
    /// <summary>
    /// Gets the original selector expression.
    /// </summary>
    public Expression OriginalExpression { get; init; } = Expression.Empty();
    
    /// <summary>
    /// Gets the result type of the projection.
    /// </summary>
    public Type ResultType { get; init; } = typeof(object);
}
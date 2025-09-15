using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FractalDataWorks.DataSets.Abstractions.Operators;

namespace FractalDataWorks.DataSets.Abstractions;

/// <summary>
/// Represents a structured query expression that can be analyzed and translated
/// to different backend query languages (SQL, REST, file operations, etc.).
/// </summary>
public interface IQueryExpression
{
    /// <summary>
    /// Gets the original LINQ expression tree.
    /// Contains the raw expression as captured from LINQ methods.
    /// </summary>
    Expression OriginalExpression { get; }
    
    /// <summary>
    /// Gets the dataset being queried.
    /// </summary>
    IDataSet DataSet { get; }
    
    /// <summary>
    /// Gets the Where clauses extracted from the expression.
    /// Each condition can be analyzed separately for optimization.
    /// </summary>
    IReadOnlyList<WhereClause> WhereConditions { get; }
    
    /// <summary>
    /// Gets the Select projection if present.
    /// Null if no Select clause (returns full records).
    /// </summary>
    SelectProjection? SelectProjection { get; }
    
    /// <summary>
    /// Gets the OrderBy clauses if present.
    /// </summary>
    IReadOnlyList<OrderByClause> OrderByConditions { get; }
    
    /// <summary>
    /// Gets the Skip count if present (for pagination).
    /// </summary>
    int? Skip { get; }
    
    /// <summary>
    /// Gets the Take count if present (for pagination/limiting).
    /// </summary>
    int? Take { get; }
    
    /// <summary>
    /// Gets the result type after all projections.
    /// May be anonymous type for Select projections.
    /// </summary>
    Type ResultType { get; }
}

/// <summary>
/// Represents a single WHERE condition extracted from a query.
/// </summary>
public sealed class WhereClause
{
    /// <summary>
    /// Gets the field name being filtered.
    /// </summary>
    public string FieldName { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the comparison operator (Equals, NotEquals, GreaterThan, etc.).
    /// </summary>
    public ComparisonOperatorBase Operator { get; init; }
    
    /// <summary>
    /// Gets the value to compare against.
    /// </summary>
    public object? Value { get; init; }
    
    /// <summary>
    /// Gets the logical operator combining this condition with others (And, Or).
    /// </summary>
    public LogicalOperatorBase LogicalOperator { get; init; }
    
    /// <summary>
    /// Gets the original expression for this condition.
    /// Used when translation requires the full expression context.
    /// </summary>
    public Expression OriginalExpression { get; init; } = Expression.Empty();
}

/// <summary>
/// Represents a SELECT projection extracted from a query.
/// </summary>
public sealed class SelectProjection
{
    /// <summary>
    /// Gets the fields being selected.
    /// </summary>
    public IReadOnlyList<ProjectedField> Fields { get; init; } = Array.Empty<ProjectedField>();
    
    /// <summary>
    /// Gets the original selector expression.
    /// </summary>
    public Expression OriginalExpression { get; init; } = Expression.Empty();
    
    /// <summary>
    /// Gets the result type of the projection.
    /// </summary>
    public Type ResultType { get; init; } = typeof(object);
}

/// <summary>
/// Represents a field in a SELECT projection.
/// </summary>
public sealed class ProjectedField
{
    /// <summary>
    /// Gets the source field name from the dataset.
    /// </summary>
    public string SourceField { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the alias for this field in the result.
    /// </summary>
    public string Alias { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the field type.
    /// </summary>
    public Type FieldType { get; init; } = typeof(object);
}

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
    public SortDirectionBase Direction { get; init; }
    
    /// <summary>
    /// Gets the original expression for this ordering.
    /// </summary>
    public Expression OriginalExpression { get; init; } = Expression.Empty();
}

// Enhanced Enums will be defined in separate files for source generation
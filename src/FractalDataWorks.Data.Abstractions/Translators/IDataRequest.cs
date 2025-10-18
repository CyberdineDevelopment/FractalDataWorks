using System.Collections.Generic;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Represents a data request with filters, projections, sorting, and aggregations.
/// Universal request model that translators convert to domain-specific queries.
/// </summary>
public interface IDataRequest
{
    /// <summary>
    /// Fields to select (projection). Null means select all.
    /// </summary>
    IReadOnlyList<string>? SelectFields { get; }

    /// <summary>
    /// Filter expressions.
    /// </summary>
    IReadOnlyList<IFilterExpression>? Filters { get; }

    /// <summary>
    /// Sort specifications.
    /// </summary>
    IReadOnlyList<ISortSpecification>? Sorting { get; }

    /// <summary>
    /// Aggregation specifications.
    /// </summary>
    IReadOnlyList<IAggregationSpecification>? Aggregations { get; }

    /// <summary>
    /// Maximum number of results to return (LIMIT/TOP).
    /// </summary>
    int? Top { get; }

    /// <summary>
    /// Number of results to skip (OFFSET).
    /// </summary>
    int? Skip { get; }
}

/// <summary>
/// Represents a filter expression.
/// </summary>
public interface IFilterExpression
{
    /// <summary>
    /// Field name to filter on.
    /// </summary>
    string FieldName { get; }

    /// <summary>
    /// Filter operator (Equals, NotEquals, GreaterThan, LessThan, Contains, etc.).
    /// </summary>
    string Operator { get; }

    /// <summary>
    /// Filter value.
    /// </summary>
    object? Value { get; }
}

/// <summary>
/// Represents a sort specification.
/// </summary>
public interface ISortSpecification
{
    /// <summary>
    /// Field name to sort by.
    /// </summary>
    string FieldName { get; }

    /// <summary>
    /// Sort direction (Ascending or Descending).
    /// </summary>
    SortDirection Direction { get; }
}

/// <summary>
/// Sort direction.
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// Ascending order.
    /// </summary>
    Ascending,

    /// <summary>
    /// Descending order.
    /// </summary>
    Descending
}

/// <summary>
/// Represents an aggregation specification.
/// </summary>
public interface IAggregationSpecification
{
    /// <summary>
    /// Aggregation function (Sum, Avg, Count, Min, Max).
    /// </summary>
    string Function { get; }

    /// <summary>
    /// Field name to aggregate. Null for COUNT(*).
    /// </summary>
    string? FieldName { get; }

    /// <summary>
    /// Alias for the aggregated result.
    /// </summary>
    string? Alias { get; }
}

using System.Collections.Generic;

namespace FractalDataWorks.Commands.Abstractions.Commands;

/// <summary>
/// Interface for query commands that retrieve data.
/// </summary>
/// <remarks>
/// Query commands represent read operations like SELECT in SQL or GET in REST.
/// They do not modify data and can be cached based on their properties.
/// </remarks>
public interface IQueryCommand : IDataCommand
{
    /// <summary>
    /// Gets the fields to select in the query.
    /// </summary>
    /// <value>List of field names to include in the result.</value>
    IReadOnlyCollection<string>? SelectFields { get; }

    /// <summary>
    /// Gets the filter criteria for the query.
    /// </summary>
    /// <value>Filter expression or criteria object.</value>
    object? FilterCriteria { get; }

    /// <summary>
    /// Gets the ordering specifications.
    /// </summary>
    /// <value>Collection of ordering rules.</value>
    IReadOnlyCollection<IOrderSpecification>? OrderBy { get; }

    /// <summary>
    /// Gets the number of records to skip.
    /// </summary>
    /// <value>Number of records to skip for pagination.</value>
    int? Skip { get; }

    /// <summary>
    /// Gets the maximum number of records to return.
    /// </summary>
    /// <value>Maximum result set size.</value>
    int? Take { get; }

    /// <summary>
    /// Gets whether to include related data.
    /// </summary>
    /// <value>List of related entities to include.</value>
    IReadOnlyCollection<string>? IncludeRelations { get; }

    /// <summary>
    /// Gets whether this query result can be cached.
    /// </summary>
    /// <value>True if query results can be cached.</value>
    bool IsCacheable { get; }

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    /// <value>How long to cache results, if cacheable.</value>
    int? CacheDurationSeconds { get; }
}

/// <summary>
/// Specifies ordering for query results.
/// </summary>
public interface IOrderSpecification
{
    /// <summary>
    /// Gets the field name to order by.
    /// </summary>
    string FieldName { get; }

    /// <summary>
    /// Gets whether to order in descending order.
    /// </summary>
    bool IsDescending { get; }
}
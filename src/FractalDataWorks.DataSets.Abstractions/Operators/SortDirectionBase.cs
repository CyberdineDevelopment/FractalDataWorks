using FractalDataWorks.Collections;

namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Interface for sort directions.
/// </summary>
public interface ISortDirection : ITypeOption<SortDirectionBase>
{
    string Description { get; }
    string SqlKeyword { get; }
    bool IsAscending { get; }
}

/// <summary>
/// Sort directions for ORDER BY clauses in dataset queries.
/// Provides extensible sort directions that can be translated to different backend implementations.
/// </summary>
public abstract class SortDirectionBase : TypeOptionBase<SortDirectionBase>, ISortDirection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SortDirectionBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this sort direction.</param>
    /// <param name="name">The name of this sort direction.</param>
    /// <param name="description">The description of this sort direction.</param>
    /// <param name="isAscending">Whether this direction sorts in ascending order.</param>
    /// <param name="category">The optional category for this sort direction.</param>
    protected SortDirectionBase(int id, string name, string description, bool isAscending, string? category = null)
        : base(id, name, category ?? "Sort")
    {
        Description = description;
        IsAscending = isAscending;
    }

    /// <summary>
    /// Gets the description of this sort direction.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the SQL keyword representation of this direction.
    /// Used when translating queries to SQL databases.
    /// </summary>
    public abstract string SqlKeyword { get; }

    /// <summary>
    /// Gets a value indicating whether this direction sorts in ascending order.
    /// </summary>
    public bool IsAscending { get; }
}
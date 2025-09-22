using FractalDataWorks.Collections;

namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Comparison operators for WHERE conditions in dataset queries.
/// Provides extensible operators that can be translated to different backend implementations.
/// </summary>
public abstract class ComparisonOperatorBase : TypeOptionBase<ComparisonOperatorBase>, IComparisonOperator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ComparisonOperatorBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this comparison operator.</param>
    /// <param name="name">The name of this comparison operator.</param>
    /// <param name="description">The description of this comparison operator.</param>
    /// <param name="category">The optional category for this comparison operator.</param>
    protected ComparisonOperatorBase(int id, string name, string description, string? category = null)
        : base(id, name, category ?? "Comparison")
    {
        Description = description;
    }

    /// <summary>
    /// Gets the description of this comparison operator.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the SQL representation of this operator.
    /// Used when translating queries to SQL databases.
    /// </summary>
    public abstract string SqlOperator { get; }

    /// <summary>
    /// Gets a value indicating whether this operator requires a single value.
    /// False for operators like IN, NOT IN that accept multiple values.
    /// </summary>
    public abstract bool IsSingleValue { get; }
}
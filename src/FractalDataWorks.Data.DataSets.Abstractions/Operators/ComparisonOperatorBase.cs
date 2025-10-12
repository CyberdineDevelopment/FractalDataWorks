using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

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
    /// <param name="sqlOperator">The SQL representation of this operator.</param>
    /// <param name="isSingleValue">Indicates whether this operator requires a single value.</param>
    /// <param name="category">The optional category for this comparison operator.</param>
    protected ComparisonOperatorBase(
        int id,
        string name,
        string description,
        string sqlOperator,
        bool isSingleValue,
        string? category = null)
        : base(id, name, $"DataSets:Operators:Comparison:{name}", $"{name} Comparison Operator", description, category ?? "Comparison")
    {
        Description = description;
        SqlOperator = sqlOperator;
        IsSingleValue = isSingleValue;
    }

    /// <summary>
    /// Gets the description of this comparison operator.
    /// </summary>
    public new string Description { get; }

    /// <summary>
    /// Gets the SQL representation of this operator.
    /// Used when translating queries to SQL databases.
    /// </summary>
    public string SqlOperator { get; }

    /// <summary>
    /// Gets a value indicating whether this operator requires a single value.
    /// False for operators like IN, NOT IN that accept multiple values.
    /// </summary>
    public bool IsSingleValue { get; }
}
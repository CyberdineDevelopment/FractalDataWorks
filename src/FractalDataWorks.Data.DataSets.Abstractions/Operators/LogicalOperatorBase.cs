using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Logical operators for combining WHERE conditions in dataset queries.
/// Provides extensible operators that can be translated to different backend implementations.
/// </summary>
public abstract class LogicalOperatorBase : TypeOptionBase<LogicalOperatorBase>, ILogicalOperator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogicalOperatorBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this logical operator.</param>
    /// <param name="name">The name of this logical operator.</param>
    /// <param name="description">The description of this logical operator.</param>
    /// <param name="precedence">The precedence level for order of operations.</param>
    /// <param name="sqlOperator">The SQL representation of this operator.</param>
    /// <param name="category">The optional category for this logical operator.</param>
    protected LogicalOperatorBase(
        int id,
        string name,
        string description,
        int precedence,
        string sqlOperator,
        string? category = null)
        : base(id, name, $"DataSets:Operators:Logical:{name}", $"{name} Logical Operator", description, category ?? "Logical")
    {
        Description = description;
        Precedence = precedence;
        SqlOperator = sqlOperator;
    }

    /// <summary>
    /// Gets the description of this logical operator.
    /// </summary>
    public new string Description { get; }

    /// <summary>
    /// Gets the SQL representation of this operator.
    /// Used when translating queries to SQL databases.
    /// </summary>
    public string SqlOperator { get; }

    /// <summary>
    /// Gets the precedence level for this operator.
    /// Higher values have higher precedence (are evaluated first).
    /// </summary>
    public int Precedence { get; }
}
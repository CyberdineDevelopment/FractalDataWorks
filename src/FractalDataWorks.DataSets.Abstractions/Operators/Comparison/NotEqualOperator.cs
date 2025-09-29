namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Inequality comparison operator (!=, &lt;&gt;).
/// </summary>
public sealed class NotEqualOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotEqualOperator"/> class.
    /// </summary>
    public NotEqualOperator() : base(
        id: 2,
        name: "NotEqual",
        description: "Not equal to operator",
        sqlOperator: "!=",
        isSingleValue: true)
    {
    }
}
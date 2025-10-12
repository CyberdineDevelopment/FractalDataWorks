namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Equality comparison operator (equals).
/// </summary>
public sealed class EqualOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EqualOperator"/> class.
    /// </summary>
    public EqualOperator() : base(
        id: 1,
        name: "Equal",
        description: "Equal to operator",
        sqlOperator: "=",
        isSingleValue: true)
    {
    }
}
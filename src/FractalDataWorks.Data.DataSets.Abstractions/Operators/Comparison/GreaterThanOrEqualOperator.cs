namespace FractalDataWorks.Data.DataSets.Abstractions.Operators;

/// <summary>
/// Greater than or equal comparison operator (>=).
/// </summary>
public sealed class GreaterThanOrEqualOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GreaterThanOrEqualOperator"/> class.
    /// </summary>
    public GreaterThanOrEqualOperator() : base(
        id: 4,
        name: "GreaterThanOrEqual",
        description: "Greater than or equal to operator",
        sqlOperator: ">=",
        isSingleValue: true)
    {
    }
}
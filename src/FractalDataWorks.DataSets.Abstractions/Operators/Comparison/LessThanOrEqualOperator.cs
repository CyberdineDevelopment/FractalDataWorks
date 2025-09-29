namespace FractalDataWorks.DataSets.Abstractions.Operators;

/// <summary>
/// Less than or equal comparison operator (&lt;=).
/// </summary>
public sealed class LessThanOrEqualOperator : ComparisonOperatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LessThanOrEqualOperator"/> class.
    /// </summary>
    public LessThanOrEqualOperator() : base(
        id: 6,
        name: "LessThanOrEqual",
        description: "Less than or equal to operator",
        sqlOperator: "<=",
        isSingleValue: true)
    {
    }
}